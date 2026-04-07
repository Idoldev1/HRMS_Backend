using HRMS.API.Models;
using HRMS.API.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace HRMS.API.Services
{
    // ── DTO returned by tax-preview and bulk-generate ──────────────────────────
    public record TaxBracket(string Label, decimal Rate, decimal IncomeInBracket, decimal Tax);

    public record TaxBreakdown(
        decimal AnnualGross,
        decimal AnnualTax,
        decimal EffectiveRate,
        IReadOnlyList<TaxBracket> Brackets,
        decimal MonthlyTax);

    public record PayrollSummary(
        int TotalRecords,
        int PendingCount,
        int ProcessedCount,
        int PaidCount,
        decimal TotalGrossPayroll,
        decimal TotalNetPayroll,
        decimal TotalTaxCollected,
        decimal AverageNetSalary);

    // ── Interface ──────────────────────────────────────────────────────────────
    public interface IPayrollService
    {
        Task<IEnumerable<Payroll>> GetPayrollsAsync(int? employeeId, string? status);
        Task<Payroll>              GeneratePayrollAsync(Payroll payroll);
        Task                       UpdatePayrollAsync(int id, Payroll payroll);
        Task<Payroll>              ProcessPayrollAsync(int id, int processedByEmployeeId);
        Task<Payroll>              MarkAsPaidAsync(int id, DateTime paymentDate, string paymentMethod);
        Task<IEnumerable<Payroll>> BulkGenerateAsync(IEnumerable<string> employeeIds, DateTime start, DateTime end);
        Task<PayrollSummary>       GetSummaryAsync(int? month, int? year);
        TaxBreakdown               CalculateTax(decimal annualGrossSalary);
    }

    // ── Implementation ─────────────────────────────────────────────────────────
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository   _payrollRepository;
        private readonly IEmployeeRepository  _employeeRepository;
        private readonly IPayslipService      _payslipService;
        private readonly ILogger<PayrollService> _logger;

        // US Federal 2024 Marginal Tax Brackets (single filer)
        private static readonly (decimal UpperBound, decimal Rate)[] TaxBrackets =
        [
            (11_600m,  0.10m),
            (47_150m,  0.12m),
            (100_525m, 0.22m),
            (191_950m, 0.24m),
            (243_725m, 0.32m),
            (609_350m, 0.35m),
            (decimal.MaxValue, 0.37m)
        ];

        public PayrollService(
            IPayrollRepository payrollRepository,
            IEmployeeRepository employeeRepository,
            IPayslipService payslipService,
            ILogger<PayrollService> logger)
        {
            _payrollRepository   = payrollRepository;
            _employeeRepository  = employeeRepository;
            _payslipService      = payslipService;
            _logger              = logger;
        }

        // ── Read ───────────────────────────────────────────────────────────────
        public async Task<IEnumerable<Payroll>> GetPayrollsAsync(int? employeeId, string? status)
        {
            _logger.LogInformation("Retrieving payrolls. EmployeeId={Emp}, Status={Status}", employeeId, status);
            return await _payrollRepository.GetPayrollsWithIncludesAsync(employeeId, status);
        }

        // ── Create ─────────────────────────────────────────────────────────────
        public async Task<Payroll> GeneratePayrollAsync(Payroll payroll)
        {
            _logger.LogInformation("Generating payroll for employee {Id}.", payroll.EmployeeId);

            CalculateTotals(payroll);
            var employee = await _employeeRepository.GetByIdWithIncludesAsync(payroll.EmployeeId);
            var empId = employee?.EmployeeId ?? payroll.EmployeeId.ToString();
            payroll.PayslipNumber = _payslipService.GeneratePayslipNumber(0, empId); // temp; updated after insert
            payroll.CreatedAt = DateTime.Now;
            payroll.UpdatedAt = DateTime.Now;

            var created = await _payrollRepository.AddAsync(payroll);

            // Assign permanent payslip number using the real Id
            created.PayslipNumber = _payslipService.GeneratePayslipNumber(created.Id, empId);
            await _payrollRepository.UpdateAsync(created);

            _logger.LogInformation("Payroll {Id} generated, payslip# {Num}.", created.Id, created.PayslipNumber);
            return created;
        }

        // ── Update ─────────────────────────────────────────────────────────────
        public async Task UpdatePayrollAsync(int id, Payroll payroll)
        {
            _logger.LogInformation("Updating payroll {Id}.", id);
            if (id != payroll.Id)
                throw new ArgumentException("Payroll id mismatch.");

            var existing = await _payrollRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Payroll {id} not found.");

            CalculateTotals(payroll);
            payroll.UpdatedAt = DateTime.Now;
            await _payrollRepository.UpdateAsync(payroll);
        }

        // ── Process (Pending → Processed) ──────────────────────────────────────
        public async Task<Payroll> ProcessPayrollAsync(int id, int processedByEmployeeId)
        {
            _logger.LogInformation("Processing payroll {Id}.", id);
            var payroll = await GetOrThrow(id);

            if (payroll.Status != "Pending")
                throw new InvalidOperationException($"Only Pending payrolls can be processed (current: {payroll.Status}).");

            payroll.Status      = "Processed";
            payroll.ProcessedAt = DateTime.Now;
            payroll.UpdatedAt   = DateTime.Now;
            await _payrollRepository.UpdateAsync(payroll);

            return payroll;
        }

        // ── Mark Paid (Processed → Paid) ───────────────────────────────────────
        public async Task<Payroll> MarkAsPaidAsync(int id, DateTime paymentDate, string paymentMethod)
        {
            _logger.LogInformation("Marking payroll {Id} as paid.", id);
            var payroll = await GetOrThrow(id);

            if (payroll.Status != "Processed")
                throw new InvalidOperationException($"Only Processed payrolls can be marked Paid (current: {payroll.Status}).");

            payroll.Status        = "Paid";
            payroll.PaymentDate   = paymentDate;
            payroll.PaymentMethod = paymentMethod;
            payroll.UpdatedAt     = DateTime.Now;
            await _payrollRepository.UpdateAsync(payroll);

            return payroll;
        }

        // ── Bulk Generate ──────────────────────────────────────────────────────
        public async Task<IEnumerable<Payroll>> BulkGenerateAsync(
            IEnumerable<string> employeeIds, DateTime start, DateTime end)
        {
            _logger.LogInformation("Bulk generating payroll for period {Start}–{End}.", start, end);
            var results = new List<Payroll>();

            foreach (var businessId in employeeIds)
            {
                var employee = await _employeeRepository.GetByEmployeeIdAsync(businessId);
                if (employee == null)
                {
                    _logger.LogWarning("Bulk generate: employee '{Id}' not found, skipping.", businessId);
                    continue;
                }

                var p = new Payroll
                {
                    EmployeeId         = employee.Id,
                    BaseSalary         = employee.Salary,
                    PayPeriodStartDate = start,
                    PayPeriodEndDate   = end
                };
                var created = await GeneratePayrollAsync(p);
                results.Add(created);
            }
            return results;
        }

        // ── Summary ────────────────────────────────────────────────────────────
        public async Task<PayrollSummary> GetSummaryAsync(int? month, int? year)
        {
            var all = await _payrollRepository.GetPayrollsWithIncludesAsync(null, null);

            if (month.HasValue && year.HasValue)
                all = all.Where(p =>
                    p.PayPeriodStartDate.Month == month.Value &&
                    p.PayPeriodStartDate.Year  == year.Value);

            var list = all.ToList();
            return new PayrollSummary(
                TotalRecords:      list.Count,
                PendingCount:      list.Count(p => p.Status == "Pending"),
                ProcessedCount:    list.Count(p => p.Status == "Processed"),
                PaidCount:         list.Count(p => p.Status == "Paid"),
                TotalGrossPayroll: list.Sum(p => p.TotalEarnings),
                TotalNetPayroll:   list.Sum(p => p.NetSalary),
                TotalTaxCollected: list.Sum(p => p.TaxDeduction),
                AverageNetSalary:  list.Count > 0 ? list.Average(p => p.NetSalary) : 0);
        }

        // ── Tax Calculator (US progressive brackets) ───────────────────────────
        public TaxBreakdown CalculateTax(decimal annualGrossSalary)
        {
            var brackets = new List<TaxBracket>();
            decimal totalTax   = 0;
            decimal remaining  = annualGrossSalary;
            decimal lowerBound = 0;

            foreach (var (upper, rate) in TaxBrackets)
            {
                if (remaining <= 0) break;

                var bracketWidth  = upper == decimal.MaxValue ? remaining : upper - lowerBound;
                var incomeInBand  = Math.Min(remaining, bracketWidth);
                var taxInBand     = Math.Round(incomeInBand * rate, 2);

                var pct = (int)(rate * 100);
                brackets.Add(new TaxBracket($"{pct}% bracket", rate, incomeInBand, taxInBand));

                totalTax  += taxInBand;
                remaining -= incomeInBand;
                lowerBound = upper == decimal.MaxValue ? upper : upper;

                if (upper == decimal.MaxValue) break;
            }

            var effectiveRate = annualGrossSalary > 0
                ? Math.Round(totalTax / annualGrossSalary, 4)
                : 0;

            return new TaxBreakdown(
                AnnualGross:   annualGrossSalary,
                AnnualTax:     totalTax,
                EffectiveRate: effectiveRate,
                Brackets:      brackets,
                MonthlyTax:    Math.Round(totalTax / 12, 2));
        }

        // ── Private helpers ────────────────────────────────────────────────────
        private static void CalculateTotals(Payroll payroll)
        {
            var totalAllowances = payroll.HousingAllowance + payroll.TransportAllowance +
                                  payroll.MedicalAllowance + payroll.OtherAllowances;
            var totalDeductions = payroll.TaxDeduction + payroll.InsuranceDeduction +
                                  payroll.LoanDeduction + payroll.OtherDeductions;

            payroll.OvertimeAmount  = payroll.OvertimeHours * payroll.OvertimeRate;
            payroll.TotalEarnings   = payroll.BaseSalary + totalAllowances + payroll.OvertimeAmount + payroll.Bonuses;
            payroll.TotalDeductions = totalDeductions;
            payroll.NetSalary       = payroll.TotalEarnings - payroll.TotalDeductions;
        }

        private async Task<Payroll> GetOrThrow(int id)
        {
            return await _payrollRepository.GetByIdWithIncludesAsync(id)
                ?? throw new KeyNotFoundException($"Payroll {id} not found.");
        }
    }
}


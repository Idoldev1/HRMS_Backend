using HRMS.API.Repositories;
using System.Text.Json;

namespace HRMS.API.Services
{
    public interface IPayslipService
    {
        Task<byte[]> GeneratePayslipAsync(int payrollId);
        Task<string> ExportToAccountingSystemAsync(int payrollId, string format = "CSV");
        string GeneratePayslipNumber(int payrollId, string employeeId);
    }

    public class PayslipService : IPayslipService
    {
        private readonly IPayrollRepository _payrollRepository;

        public PayslipService(IPayrollRepository payrollRepository)
        {
            _payrollRepository = payrollRepository;
        }

        public async Task<byte[]> GeneratePayslipAsync(int payrollId)
        {
            var payroll = await _payrollRepository.GetPayrollsWithIncludesAsync(null, null);
            var p = payroll.FirstOrDefault(x => x.Id == payrollId)
                ?? throw new KeyNotFoundException($"Payroll {payrollId} not found.");

            var html = BuildPayslipHtml(p);
            return System.Text.Encoding.UTF8.GetBytes(html);
        }

        public async Task<string> ExportToAccountingSystemAsync(int payrollId, string format = "CSV")
        {
            var payrolls = await _payrollRepository.GetPayrollsWithIncludesAsync(null, null);
            var p = payrolls.FirstOrDefault(x => x.Id == payrollId)
                ?? throw new KeyNotFoundException($"Payroll {payrollId} not found.");

            return format.ToUpper() == "JSON"
                ? BuildJsonExport(p)
                : BuildCsvExport(p);
        }

        public string GeneratePayslipNumber(int payrollId, string employeeId)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd");
            return $"PSL-{employeeId}-{timestamp}-{payrollId:D6}";
        }

        // ──────────────────────────────── private helpers ────────────────────

        private static string Fmt(decimal value) =>
            value.ToString("N2");

        private static string BuildPayslipHtml(Models.Payroll p)
        {
            var employee = p.Employee;
            var empName = employee != null ? $"{employee.FirstName} {employee.LastName}" : $"Employee #{p.EmployeeId}";
            var empId   = employee?.EmployeeId ?? p.EmployeeId.ToString();
            var dept    = employee?.Department?.Name ?? "—";
            var pos     = employee?.Position ?? "—";

            var period  = $"{p.PayPeriodStartDate:MMM dd, yyyy} – {p.PayPeriodEndDate:MMM dd, yyyy}";
            var pslNum  = p.PayslipNumber ?? $"PSL-{p.PayPeriodStartDate:yyyyMM}-{p.Id:D6}";
            var payDate = p.PaymentDate.HasValue ? p.PaymentDate.Value.ToString("MMM dd, yyyy") : "Pending";

            var totalAllowances = p.HousingAllowance + p.TransportAllowance + p.MedicalAllowance + p.OtherAllowances;

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8"">
<title>Payslip – {pslNum}</title>
<style>
  *{{box-sizing:border-box;margin:0;padding:0}}
  body{{font-family:'Segoe UI',Arial,sans-serif;background:#f4f6fb;color:#1a1a2e;font-size:13px}}
  .page{{max-width:760px;margin:24px auto;background:#fff;border-radius:8px;box-shadow:0 2px 12px rgba(0,0,0,.12);overflow:hidden}}
  .banner{{background:linear-gradient(135deg,#1976d2 0%,#0d47a1 100%);color:#fff;padding:24px 32px}}
  .banner h1{{font-size:26px;letter-spacing:1px;font-weight:700}}
  .banner .sub{{opacity:.85;margin-top:4px;font-size:12px}}
  .body{{padding:24px 32px}}
  .meta-grid{{display:grid;grid-template-columns:1fr 1fr;gap:12px;margin-bottom:20px}}
  .meta-box{{background:#f8fafc;border-left:3px solid #1976d2;border-radius:4px;padding:10px 14px}}
  .meta-box label{{font-size:10px;text-transform:uppercase;color:#666;letter-spacing:.5px}}
  .meta-box p{{font-weight:600;margin-top:2px;color:#1a1a2e}}
  h3{{font-size:12px;text-transform:uppercase;letter-spacing:.8px;color:#1976d2;margin-bottom:8px;padding-bottom:4px;border-bottom:1px solid #e3eaf4}}
  table{{width:100%;border-collapse:collapse;margin-bottom:20px}}
  td{{padding:7px 10px;border-bottom:1px solid #f0f4f9}}
  td:last-child{{text-align:right;font-family:monospace;font-size:13px}}
  .subtotal td{{background:#f0f7ff;font-weight:600}}
  .deduction td:last-child{{color:#c62828}}
  .net-row td{{background:#1976d2;color:#fff;font-size:15px;font-weight:700;border-radius:0 0 4px 4px}}
  .net-row td:first-child{{font-size:13px;letter-spacing:.5px}}
  .footer{{background:#f8fafc;padding:14px 32px;font-size:11px;color:#888;border-top:1px solid #e3eaf4;display:flex;justify-content:space-between}}
  @media print{{body{{background:#fff}}.page{{box-shadow:none}}}}
</style>
</head>
<body>
<div class=""page"">
  <div class=""banner"">
    <h1>PAYSLIP</h1>
    <div class=""sub"">Reference: {pslNum} &nbsp;|&nbsp; Pay Period: {period}</div>
  </div>
  <div class=""body"">
    <div class=""meta-grid"">
      <div class=""meta-box""><label>Employee Name</label><p>{empName}</p></div>
      <div class=""meta-box""><label>Employee ID</label><p>{empId}</p></div>
      <div class=""meta-box""><label>Department</label><p>{dept}</p></div>
      <div class=""meta-box""><label>Position</label><p>{pos}</p></div>
      <div class=""meta-box""><label>Payment Method</label><p>{p.PaymentMethod}</p></div>
      <div class=""meta-box""><label>Payment Date</label><p>{payDate}</p></div>
    </div>

    <h3>Earnings</h3>
    <table>
      <tr><td>Base Salary</td><td>₦{Fmt(p.BaseSalary)}</td></tr>
      <tr><td>Housing Allowance</td><td>₦{Fmt(p.HousingAllowance)}</td></tr>
      <tr><td>Transport Allowance</td><td>₦{Fmt(p.TransportAllowance)}</td></tr>
      <tr><td>Medical Allowance</td><td>₦{Fmt(p.MedicalAllowance)}</td></tr>
      {(p.OtherAllowances > 0 ? $"<tr><td>Other Allowances</td><td>₦{Fmt(p.OtherAllowances)}</td></tr>" : "")}
      {(p.OvertimeAmount > 0 ? $"<tr><td>Overtime ({p.OvertimeHours} hrs @ ₦{Fmt(p.OvertimeRate)}/hr)</td><td>₦{Fmt(p.OvertimeAmount)}</td></tr>" : "")}
      {(p.Bonuses > 0 ? $"<tr><td>Bonus</td><td>₦{Fmt(p.Bonuses)}</td></tr>" : "")}
      <tr class=""subtotal""><td>Total Earnings</td><td>₦{Fmt(p.TotalEarnings)}</td></tr>
    </table>

    <h3>Deductions</h3>
    <table>
      <tr class=""deduction""><td>Income Tax</td><td>–₦{Fmt(p.TaxDeduction)}</td></tr>
      <tr class=""deduction""><td>Health Insurance</td><td>–₦{Fmt(p.InsuranceDeduction)}</td></tr>
      {(p.LoanDeduction > 0 ? $"<tr class=\"deduction\"><td>Loan Repayment</td><td>–₦{Fmt(p.LoanDeduction)}</td></tr>" : "")}
      {(p.OtherDeductions > 0 ? $"<tr class=\"deduction\"><td>Other Deductions</td><td>–₦{Fmt(p.OtherDeductions)}</td></tr>" : "")}
      <tr class=""subtotal""><td>Total Deductions</td><td>–₦{Fmt(p.TotalDeductions)}</td></tr>
    </table>

    <table>
      <tr class=""net-row""><td>NET SALARY</td><td>₦{Fmt(p.NetSalary)}</td></tr>
    </table>

    {(string.IsNullOrWhiteSpace(p.Notes) ? "" : $"<p style='font-size:11px;color:#555;margin-bottom:16px'>Notes: {p.Notes}</p>")}
  </div>
  <div class=""footer"">
    <span>Generated: {DateTime.Now:MMM dd, yyyy HH:mm}</span>
    <span>This is a computer-generated document. No signature required.</span>
  </div>
</div>
</body></html>";
        }

        private static string BuildCsvExport(Models.Payroll p)
        {
            var empName = p.Employee != null ? $"{p.Employee.FirstName} {p.Employee.LastName}" : $"Employee#{p.EmployeeId}";
            var empId   = p.Employee?.EmployeeId ?? p.EmployeeId.ToString();
            var dept    = p.Employee?.Department?.Name ?? "";
            var period  = $"{p.PayPeriodStartDate:yyyy-MM-dd}_to_{p.PayPeriodEndDate:yyyy-MM-dd}";

            var header = "PayslipNumber,EmployeeId,EmployeeName,Department,PayPeriod,BaseSalary," +
                         "HousingAllowance,TransportAllowance,MedicalAllowance,OtherAllowances," +
                         "OvertimeHours,OvertimeAmount,Bonus,TotalEarnings," +
                         "TaxDeduction,InsuranceDeduction,LoanDeduction,OtherDeductions,TotalDeductions," +
                         "NetSalary,Status,PaymentMethod,PaymentDate,GeneratedAt";

            var pslNum = p.PayslipNumber ?? $"PSL-{p.PayPeriodStartDate:yyyyMM}-{p.Id:D6}";
            var payDate = p.PaymentDate.HasValue ? p.PaymentDate.Value.ToString("yyyy-MM-dd") : "";

            var row = $"{pslNum},{empId},{Quote(empName)},{Quote(dept)},{period}," +
                      $"{p.BaseSalary},{p.HousingAllowance},{p.TransportAllowance},{p.MedicalAllowance},{p.OtherAllowances}," +
                      $"{p.OvertimeHours},{p.OvertimeAmount},{p.Bonuses},{p.TotalEarnings}," +
                      $"{p.TaxDeduction},{p.InsuranceDeduction},{p.LoanDeduction},{p.OtherDeductions},{p.TotalDeductions}," +
                      $"{p.NetSalary},{p.Status},{p.PaymentMethod},{payDate},{DateTime.Now:yyyy-MM-dd HH:mm}";

            return $"{header}\n{row}";
        }

        private static string BuildJsonExport(Models.Payroll p)
        {
            var empName = p.Employee != null ? $"{p.Employee.FirstName} {p.Employee.LastName}" : $"Employee#{p.EmployeeId}";
            var empId   = p.Employee?.EmployeeId ?? p.EmployeeId.ToString();
            var dept    = p.Employee?.Department?.Name ?? "";
            var pslNum  = p.PayslipNumber ?? $"PSL-{p.PayPeriodStartDate:yyyyMM}-{p.Id:D6}";

            var obj = new
            {
                payslipNumber   = pslNum,
                exportDate      = DateTime.Now.ToString("yyyy-MM-dd"),
                payPeriod       = new { from = p.PayPeriodStartDate.ToString("yyyy-MM-dd"), to = p.PayPeriodEndDate.ToString("yyyy-MM-dd") },
                employee        = new { id = empId, name = empName, department = dept },
                earnings = new {
                    baseSalary         = p.BaseSalary,
                    housingAllowance   = p.HousingAllowance,
                    transportAllowance = p.TransportAllowance,
                    medicalAllowance   = p.MedicalAllowance,
                    otherAllowances    = p.OtherAllowances,
                    overtimeHours      = p.OvertimeHours,
                    overtimeRate       = p.OvertimeRate,
                    overtimeAmount     = p.OvertimeAmount,
                    bonuses            = p.Bonuses,
                    totalEarnings      = p.TotalEarnings
                },
                deductions = new {
                    taxDeduction       = p.TaxDeduction,
                    insuranceDeduction = p.InsuranceDeduction,
                    loanDeduction      = p.LoanDeduction,
                    otherDeductions    = p.OtherDeductions,
                    totalDeductions    = p.TotalDeductions
                },
                netSalary     = p.NetSalary,
                status        = p.Status,
                paymentMethod = p.PaymentMethod,
                paymentDate   = p.PaymentDate?.ToString("yyyy-MM-dd")
            };

            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }

        private static string Quote(string s) =>
            s.Contains(',') || s.Contains('"') ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
    }
}

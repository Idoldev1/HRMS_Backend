namespace HRMS.API.DTOs
{
    public class PayrollDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime PayPeriodStartDate { get; set; }
        public DateTime PayPeriodEndDate { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal InsuranceDeduction { get; set; }
        public decimal LoanDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal OvertimeRate { get; set; }
        public decimal OvertimeAmount { get; set; }
        public decimal Bonuses { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public EmployeeSummaryDto? Employee { get; set; }
    }

    public class PayrollSummaryDto
    {
        public int TotalRecords { get; set; }
        public int PendingCount { get; set; }
        public int ProcessedCount { get; set; }
        public int PaidCount { get; set; }
        public decimal TotalGrossPayroll { get; set; }
        public decimal TotalNetPayroll { get; set; }
        public decimal TotalTaxCollected { get; set; }
        public decimal AverageNetSalary { get; set; }
    }

    public class TaxBracketDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal IncomeInBracket { get; set; }
        public decimal Tax { get; set; }
    }

    public class TaxBreakdownDto
    {
        public decimal AnnualGross { get; set; }
        public decimal AnnualTax { get; set; }
        public decimal EffectiveRate { get; set; }
        public IReadOnlyList<TaxBracketDto> Brackets { get; set; } = Array.Empty<TaxBracketDto>();
        public decimal MonthlyTax { get; set; }
    }

    public class PayslipHtmlDto
    {
        public string Content { get; set; } = string.Empty;
    }

    public class PayrollExportDto
    {
        public string Data { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
    }
}

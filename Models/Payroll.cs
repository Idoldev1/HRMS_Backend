using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Models
{
    public class Payroll
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PayPeriodStartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PayPeriodEndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HousingAllowance { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TransportAllowance { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MedicalAllowance { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherAllowances { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxDeduction { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal InsuranceDeduction { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal LoanDeduction { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherDeductions { get; set; } = 0;

        public decimal OvertimeHours { get; set; } = 0;
        public decimal OvertimeRate { get; set; } = 0;
        public decimal OvertimeAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Bonuses { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEarnings { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        [StringLength(20)]
        public string PaymentMethod { get; set; } = "Bank Transfer";

        public string? Notes { get; set; }

        [StringLength(30)]
        public string? PayslipNumber { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

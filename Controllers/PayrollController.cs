using HRMS.API.DTOs;
using HRMS.API.Models;
using HRMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;
        private readonly IPayslipService _payslipService;

        public PayrollController(IPayrollService payrollService, IPayslipService payslipService)
        {
            _payrollService = payrollService;
            _payslipService = payslipService;
        }

        // ── Query ─────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,HR,Finance")]
        public async Task<ActionResult<IEnumerable<PayrollDto>>> GetPayrolls(
            [FromQuery] int? employeeId,
            [FromQuery] string? status)
        {
            var payrolls = await _payrollService.GetPayrollsAsync(employeeId, status);
            return Ok(payrolls.Select(payroll => payroll.ToDto()));
        }

        /// <summary>Allows any authenticated employee to view their own payroll records.</summary>
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<PayrollDto>>> GetMyPayrolls([FromQuery] string? status)
        {
            var empIdClaim = User.FindFirstValue("EmployeeId");
            if (!int.TryParse(empIdClaim, out var empId))
                return Unauthorized("Employee profile not found.".ToMessageDto());

            var payrolls = await _payrollService.GetPayrollsAsync(empId, status);
            return Ok(payrolls.Select(payroll => payroll.ToDto()));
        }

        /// <summary>Aggregate summary stats for dashboard cards.</summary>
        [HttpGet("summary")]
        [Authorize(Roles = "Admin,HR,Finance")]
        public async Task<ActionResult<PayrollSummaryDto>> GetSummary([FromQuery] int? month, [FromQuery] int? year)
        {
            var summary = await _payrollService.GetSummaryAsync(month, year);
            return Ok(summary.ToDto());
        }

        /// <summary>Returns progressive tax breakdown for a given annual salary – no auth required for preview.</summary>
        [HttpGet("tax-preview")]
        public ActionResult<TaxBreakdownDto> GetTaxPreview([FromQuery] decimal annualSalary)
        {
            if (annualSalary < 0)
                return BadRequest("annualSalary must be ≥ 0".ToMessageDto());

            var breakdown = _payrollService.CalculateTax(annualSalary);
            return Ok(breakdown.ToDto());
        }

        // ── Create ────────────────────────────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = "Admin,HR,Finance")]
        public async Task<ActionResult<PayrollDto>> GeneratePayroll(Payroll payroll)
        {
            var created = await _payrollService.GeneratePayrollAsync(payroll);
            return CreatedAtAction(nameof(GetPayrolls), new { id = created.Id }, created.ToDto());
        }

        [HttpPost("bulk-generate")]
        [Authorize(Roles = "Admin,HR,Finance")]
        public async Task<ActionResult<IEnumerable<PayrollDto>>> BulkGenerate([FromBody] BulkGenerateRequest request)
        {
            var results = await _payrollService.BulkGenerateAsync(
                request.EmployeeIds, request.PeriodStart, request.PeriodEnd);

            return Ok(results.Select(payroll => payroll.ToDto()));
        }

        // ── Update ────────────────────────────────────────────────────────────

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HR,Finance")]
        public async Task<IActionResult> UpdatePayroll(int id, Payroll payroll)
        {
            await _payrollService.UpdatePayrollAsync(id, payroll);
            return NoContent();
        }

        // ── Workflow transitions ───────────────────────────────────────────────

        [HttpPost("{id}/process")]
        [Authorize(Roles = "Admin,HR,Finance")]
        public async Task<ActionResult<PayrollDto>> ProcessPayroll(int id)
        {
            var empIdClaim = User.FindFirstValue("EmployeeId");
            int.TryParse(empIdClaim, out var processedBy);

            var payroll = await _payrollService.ProcessPayrollAsync(id, processedBy);
            return Ok(payroll.ToDto());
        }

        [HttpPost("{id}/mark-paid")]
        [Authorize(Roles = "Admin,HR,Finance")]
        public async Task<ActionResult<PayrollDto>> MarkAsPaid(int id, [FromBody] MarkPaidRequest request)
        {
            var payroll = await _payrollService.MarkAsPaidAsync(
                id,
                request.PaymentDate ?? DateTime.Today,
                request.PaymentMethod ?? "Bank Transfer");

            return Ok(payroll.ToDto());
        }

        // ── Payslip ───────────────────────────────────────────────────────────

        [HttpGet("{id}/payslip/html")]
        public async Task<ActionResult<PayslipHtmlDto>> GetPayslipHtml(int id)
        {
            var bytes = await _payslipService.GeneratePayslipAsync(id);
            return Ok(new PayslipHtmlDto { Content = System.Text.Encoding.UTF8.GetString(bytes) });
        }

        [HttpGet("{id}/payslip")]
        public async Task<IActionResult> GetPayslip(int id)
        {
            var bytes = await _payslipService.GeneratePayslipAsync(id);
            return File(bytes, "text/html", $"payslip-{id}.html");
        }

        // ── Accounting export ─────────────────────────────────────────────────

        [HttpPost("{id}/export-accounting")]
        [Authorize(Roles = "Admin,HR,Finance")]
        public async Task<ActionResult<PayrollExportDto>> ExportForAccounting(int id, [FromQuery] string format = "CSV")
        {
            var data = await _payslipService.ExportToAccountingSystemAsync(id, format);
            var ext  = format.ToUpper() == "JSON" ? "json" : "csv";
            return Ok(new PayrollExportDto { Data = data, Filename = $"payroll-export-{id}.{ext}" });
        }
    }

    // ── Request DTOs ─────────────────────────────────────────────────────────

    public record BulkGenerateRequest(
        IEnumerable<string> EmployeeIds,
        DateTime PeriodStart,
        DateTime PeriodEnd);

    public record MarkPaidRequest(
        DateTime? PaymentDate,
        string? PaymentMethod);
}

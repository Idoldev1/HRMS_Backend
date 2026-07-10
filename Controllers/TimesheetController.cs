using HRMS.API.Constants;
using HRMS.API.Contracts.Timesheet;
using HRMS.API.DTOs;
using HRMS.API.Repositories;
using HRMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TimesheetController : ControllerBase
    {
        private readonly ITimesheetService _timesheetService;
        private readonly IEmployeeRepository _employeeRepository;

        public TimesheetController(ITimesheetService timesheetService, IEmployeeRepository employeeRepository)
        {
            _timesheetService = timesheetService;
            _employeeRepository = employeeRepository;
        }

        /// <summary>
        /// Generate a monthly timesheet from attendance records.
        /// Employees generate their own; HR/Admin can generate for any employee.
        /// </summary>
        [HttpPost("generate")]
        public async Task<ActionResult<TimesheetDto>> GenerateTimesheet([FromBody] GenerateTimesheetRequest request)
        {
            var (employeeId, userId, fullName) = await ResolveClaimsAsync();
            if (employeeId == null) return Forbid();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var isAdminOrHr = role == Roles.Admin || role == Roles.HR;

            var targetEmployeeId = isAdminOrHr && request.EmployeeId.HasValue
                ? request.EmployeeId.Value
                : employeeId.Value;

            if (!isAdminOrHr && request.EmployeeId.HasValue && request.EmployeeId.Value != employeeId.Value)
                return Forbid();

            var (timesheet, error) = await _timesheetService.GenerateTimesheetAsync(
                targetEmployeeId, request.Month, request.Year, userId, fullName);

            if (error != null) return BadRequest(new { message = error });
            return Ok(timesheet!.ToDto());
        }

        /// <summary>
        /// Get timesheets. Employees see only their own; HR/Admin see all.
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<TimesheetSummaryDto>>> GetTimesheets([FromBody] GetTimesheetsRequest request)
        {
            var (employeeId, _, _) = await ResolveClaimsAsync();
            if (employeeId == null) return Forbid();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var isAdminOrHr = role == Roles.Admin || role == Roles.HR || role == Roles.Manager;

            var targetEmployeeId = isAdminOrHr ? request.EmployeeId : employeeId;

            var timesheets = await _timesheetService.GetTimesheetsAsync(
                targetEmployeeId, request.Month, request.Year, request.Status);

            return Ok(timesheets.Select(t => t.ToSummaryDto()));
        }

        /// <summary>
        /// Get a single timesheet with full details including entries and audit log.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TimesheetDto>> GetTimesheet(int id)
        {
            var (employeeId, _, _) = await ResolveClaimsAsync();
            if (employeeId == null) return Forbid();

            var timesheet = await _timesheetService.GetTimesheetByIdAsync(id);
            if (timesheet == null) return NotFound();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var isAdminOrHr = role == Roles.Admin || role == Roles.HR || role == Roles.Manager;

            if (!isAdminOrHr && timesheet.EmployeeId != employeeId.Value)
                return Forbid();

            return Ok(timesheet.ToDto());
        }

        /// <summary>
        /// Update work descriptions and notes on timesheet entries (Draft or Rejected timesheets only).
        /// </summary>
        [HttpPut("{id:int}/entries")]
        public async Task<ActionResult<TimesheetDto>> UpdateEntries(int id, [FromBody] UpdateTimesheetEntriesRequest request)
        {
            var (employeeId, _, _) = await ResolveClaimsAsync();
            if (employeeId == null) return Forbid();

            var timesheet = await _timesheetService.UpdateEntriesAsync(id, employeeId.Value, request.Entries);
            return Ok(timesheet.ToDto());
        }

        /// <summary>
        /// Submit a timesheet for HR approval.
        /// </summary>
        [HttpPost("{id:int}/submit")]
        public async Task<ActionResult<TimesheetDto>> SubmitTimesheet(int id)
        {
            var (employeeId, userId, fullName) = await ResolveClaimsAsync();
            if (employeeId == null) return Forbid();

            var timesheet = await _timesheetService.SubmitTimesheetAsync(id, employeeId.Value, userId, fullName);
            return Ok(timesheet.ToDto());
        }

        /// <summary>
        /// Get all submitted timesheets pending approval. HR/Admin/Manager only.
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR},{Roles.Manager}")]
        public async Task<ActionResult<IEnumerable<TimesheetSummaryDto>>> GetPendingApprovals()
        {
            var timesheets = await _timesheetService.GetPendingApprovalsAsync();
            return Ok(timesheets.Select(t => t.ToSummaryDto()));
        }

        /// <summary>
        /// Approve a submitted timesheet. HR/Admin/Manager only.
        /// </summary>
        [HttpPost("{id:int}/approve")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR},{Roles.Manager}")]
        public async Task<ActionResult<TimesheetDto>> ApproveTimesheet(int id, [FromBody] ApproveTimesheetRequest request)
        {
            var (employeeId, userId, fullName) = await ResolveClaimsAsync();
            if (employeeId == null) return Forbid();

            var timesheet = await _timesheetService.ApproveTimesheetAsync(
                id, employeeId.Value, request.Comment ?? string.Empty, userId, fullName);

            return Ok(timesheet.ToDto());
        }

        /// <summary>
        /// Reject a submitted timesheet with a mandatory reason. HR/Admin/Manager only.
        /// </summary>
        [HttpPost("{id:int}/reject")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR},{Roles.Manager}")]
        public async Task<ActionResult<TimesheetDto>> RejectTimesheet(int id, [FromBody] RejectTimesheetRequest request)
        {
            var (employeeId, userId, fullName) = await ResolveClaimsAsync();
            if (employeeId == null) return Forbid();

            var timesheet = await _timesheetService.RejectTimesheetAsync(
                id, employeeId.Value, request.Reason, userId, fullName);

            return Ok(timesheet.ToDto());
        }

        /// <summary>
        /// Export timesheet as an HTML document (printable / PDF-ready).
        /// </summary>
        [HttpGet("{id:int}/export")]
        public async Task<IActionResult> ExportTimesheet(int id)
        {
            var (employeeId, _, _) = await ResolveClaimsAsync();
            if (employeeId == null) return Forbid();

            var timesheet = await _timesheetService.GetTimesheetByIdAsync(id);
            if (timesheet == null) return NotFound();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var isAdminOrHr = role == Roles.Admin || role == Roles.HR || role == Roles.Manager;

            if (!isAdminOrHr && timesheet.EmployeeId != employeeId.Value)
                return Forbid();

            var bytes = await _timesheetService.ExportTimesheetAsync(id);
            var period = new DateTime(timesheet.Year, timesheet.Month, 1).ToString("yyyy-MM");
            var fileName = $"Timesheet_{period}_EMP{timesheet.EmployeeId}.html";
            return File(bytes, "text/html", fileName);
        }

        /// <summary>
        /// Lock an approved timesheet after payroll processing. HR/Admin/Finance only.
        /// </summary>
        [HttpPost("{id:int}/lock")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR},{Roles.Finance}")]
        public async Task<ActionResult<TimesheetDto>> LockTimesheet(int id)
        {
            var (_, userId, fullName) = await ResolveClaimsAsync();
            var timesheet = await _timesheetService.LockTimesheetAsync(id, userId, fullName);
            return Ok(timesheet.ToDto());
        }

        // ──────────────────────────── helpers ────────────────────────────

        private async Task<(int? EmployeeId, string UserId, string FullName)> ResolveClaimsAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var firstName = User.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
            var lastName = User.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;
            var fullName = $"{firstName} {lastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = User.FindFirstValue(ClaimTypes.Name) ?? userId;

            var employeeStringId = User.FindFirstValue("EmployeeId");
            if (string.IsNullOrEmpty(employeeStringId))
                return (null, userId, fullName);

            var employee = await _employeeRepository.GetByEmployeeIdAsync(employeeStringId);
            return (employee?.Id, userId, fullName);
        }
    }
}

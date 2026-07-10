using HRMS.API.Constants;
using HRMS.API.Contracts.Leave;
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
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;
        private readonly IEmployeeRepository _employeeRepository;

        public LeaveController(ILeaveService leaveService, IEmployeeRepository employeeRepository)
        {
            _leaveService = leaveService;
            _employeeRepository = employeeRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveDto>>> GetLeaves(
            [FromQuery] string? employeeId,
            [FromQuery] string? status)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var isAdminOrHr = role == Roles.Admin || role == Roles.HR || role == Roles.Manager;

            int? resolvedEmployeeId = null;

            if (isAdminOrHr)
            {
                if (!string.IsNullOrEmpty(employeeId))
                {
                    var emp = await _employeeRepository.GetByEmployeeIdAsync(employeeId);
                    resolvedEmployeeId = emp?.Id;
                }
            }
            else
            {
                var claimEmployeeId = User.FindFirstValue("EmployeeId");
                if (!string.IsNullOrEmpty(claimEmployeeId))
                {
                    var emp = await _employeeRepository.GetByEmployeeIdAsync(claimEmployeeId);
                    resolvedEmployeeId = emp?.Id;
                }
            }

            var leaves = await _leaveService.GetLeavesAsync(resolvedEmployeeId, status);
            return Ok(leaves);
        }

        [HttpPost]
        public async Task<ActionResult<LeaveDto>> CreateLeave([FromBody] CreateLeaveRequest request)
        {
            var claimEmployeeId = User.FindFirstValue("EmployeeId");
            if (string.IsNullOrEmpty(claimEmployeeId))
                return Forbid();

            var employee = await _employeeRepository.GetByEmployeeIdAsync(claimEmployeeId);
            if (employee is null)
                return Forbid();

            request.EmployeeId = employee.Id;

            var result = await _leaveService.CreateLeaveAsync(request);
            if (!result.Success)
                return BadRequest((result.ErrorMessage ?? "Unable to create leave request.").ToMessageDto());

            return CreatedAtAction(nameof(GetLeaves), new { id = result.Leave!.Id }, result.Leave);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> ApproveLeave(int id, [FromBody] ApproveLeaveRequest request)
        {
            await _leaveService.ApproveLeaveAsync(id, request.ApprovedById);
            return NoContent();
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> RejectLeave(int id, [FromBody] RejectLeaveRequest request)
        {
            await _leaveService.RejectLeaveAsync(id, request.Reason);
            return NoContent();
        }
    }
}

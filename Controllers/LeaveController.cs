using HRMS.API.Contracts.Leave;
using HRMS.API.DTOs;
using HRMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveDto>>> GetLeaves(
            [FromQuery] int? employeeId,
            [FromQuery] string? status)
        {
            var leaves = await _leaveService.GetLeavesAsync(employeeId, status);
            return Ok(leaves);
        }

        [HttpPost]
        public async Task<ActionResult<LeaveDto>> CreateLeave([FromBody] CreateLeaveRequest request)
        {
            var created = await _leaveService.CreateLeaveAsync(request);
            return CreatedAtAction(nameof(GetLeaves), new { id = created.Id }, created);
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

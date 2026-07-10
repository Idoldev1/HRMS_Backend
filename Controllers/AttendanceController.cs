using HRMS.API.Common;
using HRMS.API.Constants;
using HRMS.API.Contracts.Attendance;
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
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendances([FromBody] GetAttendancesRequest request)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var isAdminOrHr = role == Roles.Admin || role == Roles.HR;

            string? employeeId = request.EmployeeId;
            if (!isAdminOrHr)
            {
                employeeId = User.FindFirstValue("EmployeeId");
                if (string.IsNullOrEmpty(employeeId))
                    return Forbid();
            }

            var result = await _attendanceService.GetAttendancesAsync(employeeId, request.StartDate, request.EndDate);
            return result.IsSuccess
                ? Ok(result.Value!.Select(a => a.ToDto()))
                : ToActionResult(result);
        }

        [HttpPost("check-in")]
        public async Task<ActionResult<AttendanceDto>> CheckIn([FromBody] MarkAttendanceRequest request)
        {
            var employeeId = User.FindFirstValue("EmployeeId");
            if (string.IsNullOrEmpty(employeeId))
                return Forbid();

            var result = await _attendanceService.CreateAttendanceAsync(employeeId, request);
            return result.IsSuccess
                ? Ok(result.Value!.ToDto())
                : ToActionResult(result);
        }

        [HttpPut("{id:int}/check-out")]
        public async Task<IActionResult> CheckOut(int id, [FromBody] CheckOutRequest request)
        {
            var attendance = new Attendance
            {
                Id = id,
                CheckOut = DateTime.UtcNow,
                BreakDuration = request.BreakDuration,
                Notes = request.Notes,
            };

            var result = await _attendanceService.UpdateAttendanceAsync(id, attendance);
            return result.IsSuccess ? NoContent() : ToActionResult(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<IActionResult> UpdateAttendance(int id, Attendance attendance)
        {
            var result = await _attendanceService.UpdateAttendanceAsync(id, attendance);
            return result.IsSuccess ? NoContent() : ToActionResult(result);
        }

        private ActionResult ToActionResult<T>(Result<T> result) => result.ErrorKind switch
        {
            ResultErrorKind.NotFound => NotFound(new MessageDto(result.Error!)),
            ResultErrorKind.Conflict => Conflict(new MessageDto(result.Error!)),
            _ => BadRequest(new MessageDto(result.Error!)),
        };

        private ActionResult ToActionResult(Result result) => result.ErrorKind switch
        {
            ResultErrorKind.NotFound => NotFound(new MessageDto(result.Error!)),
            ResultErrorKind.Conflict => Conflict(new MessageDto(result.Error!)),
            _ => BadRequest(new MessageDto(result.Error!)),
        };
    }
}

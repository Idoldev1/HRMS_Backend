using HRMS.API.Contracts.Attendance;
using HRMS.API.DTOs;
using HRMS.API.Models;
using HRMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var attendances = await _attendanceService.GetAttendancesAsync(
                request.EmployeeId,
                request.StartDate,
                request.EndDate);
            return Ok(attendances.Select(attendance => attendance.ToDto()));
        }

        [HttpPost]
        public async Task<ActionResult<AttendanceDto>> MarkAttendance(Attendance attendance)
        {
            var created = await _attendanceService.CreateAttendanceAsync(attendance);
            return Ok(created.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, Attendance attendance)
        {
            await _attendanceService.UpdateAttendanceAsync(id, attendance);
            return NoContent();
        }
    }
}

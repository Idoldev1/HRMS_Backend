using HRMS.API.Common;
using HRMS.API.Constants;
using HRMS.API.Contracts.EmployeeDevice;
using HRMS.API.DTOs;
using HRMS.API.Models;
using HRMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/employee-devices")]
    [Authorize]
    public class EmployeeDeviceController : ControllerBase
    {
        private readonly IEmployeeDeviceService _deviceService;

        public EmployeeDeviceController(IEmployeeDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpGet]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<ActionResult<IEnumerable<EmployeeDeviceDto>>> GetAll()
        {
            var result = await _deviceService.GetDevicesAsync(null);
            return result.IsSuccess
                ? Ok(result.Value!.Select(d => d.ToDto()))
                : ToActionResult(result);
        }

        [HttpGet("my-devices")]
        public async Task<ActionResult<IEnumerable<EmployeeDeviceDto>>> GetMyDevices()
        {
            var employeeId = User.FindFirstValue("EmployeeId");
            if (string.IsNullOrEmpty(employeeId))
                return Forbid();

            var result = await _deviceService.GetDevicesAsync(employeeId);
            return result.IsSuccess
                ? Ok(result.Value!.Select(d => d.ToDto()))
                : ToActionResult(result);
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<ActionResult<IEnumerable<EmployeeDeviceDto>>> GetByEmployee(string employeeId)
        {
            var result = await _deviceService.GetDevicesAsync(employeeId);
            return result.IsSuccess
                ? Ok(result.Value!.Select(d => d.ToDto()))
                : ToActionResult(result);
        }

        [HttpGet("{id:int}")]
        //[Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<ActionResult<EmployeeDeviceDto>> GetById(int id)
        {
            var result = await _deviceService.GetDeviceAsync(id);
            return result.IsSuccess
                ? Ok(result.Value!.ToDto())
                : ToActionResult(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<EmployeeDeviceDto>> RegisterMyDevice([FromBody] RegisterDeviceRequest request)
        {
            var employeeId = User.FindFirstValue("EmployeeId");
            if (string.IsNullOrEmpty(employeeId))
                return Forbid();

            var device = new EmployeeDevice
            {
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName,
                DeviceType = request.DeviceType,
            };

            var result = await _deviceService.RegisterDeviceAsync(employeeId, device);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value.ToDto())
                : ToActionResult(result);
        }

        [HttpPost("employee/{employeeId}/register")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<ActionResult<EmployeeDeviceDto>> RegisterForEmployee(
            string employeeId, [FromBody] RegisterDeviceRequest request)
        {
            var device = new EmployeeDevice
            {
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName,
                DeviceType = request.DeviceType,
            };

            var result = await _deviceService.RegisterDeviceAsync(employeeId, device);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value.ToDto())
                : ToActionResult(result);
        }

        [HttpPatch("{id:int}/toggle")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _deviceService.ToggleDeviceStatusAsync(id);
            return result.IsSuccess ? NoContent() : ToActionResult(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _deviceService.DeleteDeviceAsync(id);
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

using HRMS.API.Common;
using HRMS.API.Constants;
using HRMS.API.Contracts.WorkLocation;
using HRMS.API.DTOs;
using HRMS.API.Models;
using HRMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/work-locations")]
    [Authorize]
    public class WorkLocationController : ControllerBase
    {
        private readonly IWorkLocationService _workLocationService;

        public WorkLocationController(IWorkLocationService workLocationService)
        {
            _workLocationService = workLocationService;
        }

        [HttpGet]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<ActionResult<IEnumerable<WorkLocationDto>>> GetAll()
        {
            var result = await _workLocationService.GetAllLocationsAsync();
            return result.IsSuccess
                ? Ok(result.Value!.Select(l => l.ToDto()))
                : ToActionResult(result);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<WorkLocationDto>>> GetActive()
        {
            var result = await _workLocationService.GetActiveLocationsAsync();
            return result.IsSuccess
                ? Ok(result.Value!.Select(l => l.ToDto()))
                : ToActionResult(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<ActionResult<WorkLocationDto>> GetById(int id)
        {
            var result = await _workLocationService.GetLocationAsync(id);
            return result.IsSuccess
                ? Ok(result.Value!.ToDto())
                : ToActionResult(result);
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<ActionResult<WorkLocationDto>> Create([FromBody] CreateWorkLocationRequest request)
        {
            var location = new WorkLocation
            {
                Name = request.Name,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AllowedRadiusMeters = request.AllowedRadiusMeters,
                IsActive = true,
            };

            var result = await _workLocationService.CreateLocationAsync(location);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value.ToDto())
                : ToActionResult(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkLocationRequest request)
        {
            var location = new WorkLocation
            {
                Name = request.Name,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AllowedRadiusMeters = request.AllowedRadiusMeters,
                IsActive = request.IsActive,
            };

            var result = await _workLocationService.UpdateLocationAsync(id, location);
            return result.IsSuccess ? NoContent() : ToActionResult(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _workLocationService.DeleteLocationAsync(id);
            return result.IsSuccess ? NoContent() : ToActionResult(result);
        }

        [HttpGet("employees/{employeeId}")]
        public async Task<ActionResult<IEnumerable<WorkLocationDto>>> GetEmployeeLocations(string employeeId)
        {
            var result = await _workLocationService.GetEmployeeLocationsAsync(employeeId);
            return result.IsSuccess
                ? Ok(result.Value!.Select(l => l.ToDto()))
                : ToActionResult(result);
        }

        [HttpGet("employees/{employeeId}/assignments")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<ActionResult<IEnumerable<EmployeeWorkLocationDto>>> GetEmployeeAssignments(string employeeId)
        {
            var result = await _workLocationService.GetEmployeeAssignmentsAsync(employeeId);
            return result.IsSuccess
                ? Ok(result.Value!.Select(a => a.ToDto()))
                : ToActionResult(result);
        }

        [HttpPost("employees/{employeeId}/assign")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<IActionResult> AssignToEmployee(string employeeId, [FromBody] AssignWorkLocationRequest request)
        {
            var result = await _workLocationService.AssignLocationToEmployeeAsync(employeeId, request.WorkLocationId);
            return result.IsSuccess ? NoContent() : ToActionResult(result);
        }

        [HttpDelete("employees/{employeeId}/assign/{workLocationId:int}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.HR}")]
        public async Task<IActionResult> RemoveFromEmployee(string employeeId, int workLocationId)
        {
            var result = await _workLocationService.RemoveLocationFromEmployeeAsync(employeeId, workLocationId);
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

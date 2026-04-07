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
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(
            [FromQuery] string? department,
            [FromQuery] string? status,
            [FromQuery] string? search)
        {
            var employees = await _employeeService.GetEmployeesAsync(department, status, search);
            return Ok(employees.Select(employee => employee.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
                return NotFound();

            return Ok(employee.ToDto());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee(Employee employee)
        {
            var created = await _employeeService.CreateEmployeeAsync(employee);
            return CreatedAtAction(nameof(GetEmployee), new { id = created.Id }, created.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> UpdateEmployee(int id, Employee employee)
        {
            await _employeeService.UpdateEmployeeAsync(id, employee);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            await _employeeService.SoftDeleteEmployeeAsync(id);
            return NoContent();
        }
    }
}

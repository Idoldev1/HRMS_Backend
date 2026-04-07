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
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            var departments = await _departmentService.GetDepartmentsAsync();
            return Ok(departments.Select(department => department.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
                return NotFound();

            return Ok(department.ToDto());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment(Department department)
        {
            var created = await _departmentService.CreateDepartmentAsync(department);
            return CreatedAtAction(nameof(GetDepartment), new { id = created.Id }, created.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> UpdateDepartment(int id, Department department)
        {
            await _departmentService.UpdateDepartmentAsync(id, department);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            await _departmentService.SoftDeleteDepartmentAsync(id);
            return NoContent();
        }
    }
}

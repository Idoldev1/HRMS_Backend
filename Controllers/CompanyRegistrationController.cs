using HRMS.API.Contracts.CompanyRegistration;
using HRMS.API.DTOs;
using HRMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyRegistrationController : ControllerBase
    {
        private readonly ICompanyRegistrationService _service;
        private readonly ILogger<CompanyRegistrationController> _logger;

        public CompanyRegistrationController(
            ICompanyRegistrationService service,
            ILogger<CompanyRegistrationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Register a new company along with the admin user account, employee profile, and personal information.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<CompanyRegistrationResponseDto>> Register([FromBody] CompanyRegistrationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.RegisterCompanyAsync(request);
            if (!result.Success)
            {
                return BadRequest((result.ErrorMessage ?? "Registration failed.").ToMessageDto());
            }

            return Ok(result.Response);
        }

        /// <summary>
        /// Upload a document for the registered employee.
        /// Call this after a successful company registration using the returned employee ID.
        /// </summary>
        [Authorize]
        [HttpPost("{employeeId:int}/documents")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult<OnboardingDocumentDto>> UploadDocument(
            int employeeId,
            [FromForm] string documentType,
            IFormFile file)
        {
            try
            {
                var doc = await _service.UploadDocumentAsync(employeeId, documentType, file);
                return Ok(doc);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message.ToMessageDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for employee {EmployeeId}.", employeeId);
                return StatusCode(500, "An error occurred.".ToMessageDto());
            }
        }
    }
}

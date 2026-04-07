using HRMS.API.Contracts.Onboarding;
using HRMS.API.DTOs;
using HRMS.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    /// <summary>
    /// Token-based onboarding endpoints – no JWT authentication required.
    /// Access is controlled by the unique, time-limited onboarding token.
    /// </summary>
    [ApiController]
    [Route("api/onboarding")]
    public class OnboardingController : ControllerBase
    {
        private readonly IOnboardingService _onboardingService;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(IOnboardingService onboardingService, ILogger<OnboardingController> logger)
        {
            _onboardingService = onboardingService;
            _logger = logger;
        }

        [HttpGet("{token}")]
        public async Task<ActionResult<OnboardingProfileDto>> GetProfile(string token)
        {
            try
            {
                var profile = await _onboardingService.GetProfileAsync(token);
                if (profile == null)
                    return NotFound("Invalid or expired onboarding link.".ToMessageDto());

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving onboarding profile for token {Token}.", token);
                return StatusCode(500, "An error occurred.".ToMessageDto());
            }
        }

        [HttpPut("{token}")]
        public async Task<ActionResult<OnboardingProfileDto>> UpdateProfile(string token, [FromBody] UpdateOnboardingRequest request)
        {
            try
            {
                var updated = await _onboardingService.UpdateProfileAsync(token, request);
                return Ok(updated);
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound("Invalid or expired onboarding link.".ToMessageDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating onboarding profile for token {Token}.", token);
                return StatusCode(500, "An error occurred.".ToMessageDto());
            }
        }

        [HttpPost("{token}/complete")]
        public async Task<ActionResult<MessageDto>> CompleteOnboarding(string token)
        {
            try
            {
                await _onboardingService.CompleteAsync(token);
                return Ok("Onboarding complete. Your account is now active.".ToMessageDto());
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound("Invalid or expired onboarding link.".ToMessageDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing onboarding for token {Token}.", token);
                return StatusCode(500, "An error occurred.".ToMessageDto());
            }
        }

        [HttpPost("{token}/documents")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult<OnboardingDocumentDto>> UploadDocument(string token, [FromForm] string documentType, IFormFile file)
        {
            try
            {
                var doc = await _onboardingService.UploadDocumentAsync(token, documentType, file);
                return Ok(doc);
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound("Invalid or expired onboarding link.".ToMessageDto());
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message.ToMessageDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for token {Token}.", token);
                return StatusCode(500, "An error occurred.".ToMessageDto());
            }
        }

        [HttpGet("{token}/documents")]
        public async Task<ActionResult<IEnumerable<OnboardingDocumentDto>>> GetDocuments(string token)
        {
            try
            {
                var docs = await _onboardingService.GetDocumentsAsync(token);
                return Ok(docs);
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound("Invalid or expired onboarding link.".ToMessageDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents for token {Token}.", token);
                return StatusCode(500, "An error occurred.".ToMessageDto());
            }
        }

        [HttpDelete("{token}/documents/{docId:int}")]
        public async Task<IActionResult> DeleteDocument(string token, int docId)
        {
            try
            {
                await _onboardingService.DeleteDocumentAsync(token, docId);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound("Invalid or expired onboarding link.".ToMessageDto());
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Document not found.".ToMessageDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocId} for token {Token}.", docId, token);
                return StatusCode(500, "An error occurred.".ToMessageDto());
            }
        }

        [HttpPost("{token}/user-profile")]
        public async Task<ActionResult<MessageDto>> CreateUserProfile(string token, [FromBody] CreateOnboardingUserProfileRequest request)
        {
            try
            {
                await _onboardingService.CreateUserProfileAsync(token, request);
                return Ok("User profile created and onboarding completed successfully.".ToMessageDto());
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound("Invalid or expired onboarding link.".ToMessageDto());
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message.ToMessageDto());
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message.ToMessageDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating user profile for token {token}.");
                return StatusCode(500, "An error occurred.".ToMessageDto());
            }
        }
    }
}


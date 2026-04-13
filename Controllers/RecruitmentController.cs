using HRMS.API.Contracts.Recruitment;
using HRMS.API.DTOs;
using HRMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecruitmentController : ControllerBase
    {
        private readonly IRecruitmentService _recruitmentService;

        public RecruitmentController(IRecruitmentService recruitmentService)
        {
            _recruitmentService = recruitmentService;
        }

        // ── HR Endpoints (Authenticated) ──────────────────────────────

        [HttpGet("jobs")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<IEnumerable<JobPostingDto>>> GetAllJobs()
        {
            var jobs = await _recruitmentService.GetAllJobPostingsAsync();
            return Ok(jobs);
        }

        [HttpGet("jobs/{id}")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<JobPostingDetailDto>> GetJobById(int id)
        {
            var job = await _recruitmentService.GetJobPostingByIdAsync(id);
            if (job == null) return NotFound();
            return Ok(job);
        }

        [HttpPost("jobs")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<JobPostingDto>> CreateJob([FromBody] CreateJobPostingRequest request)
        {
            // Extract employee id from claims if available
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            int? postedById = int.TryParse(employeeIdClaim, out var eid) ? eid : null;

            var created = await _recruitmentService.CreateJobPostingAsync(request, postedById);
            return CreatedAtAction(nameof(GetJobById), new { id = created.Id }, created);
        }

        [HttpPut("jobs/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<ActionResult<JobPostingDto>> UpdateJob(int id, [FromBody] UpdateJobPostingRequest request)
        {
            var updated = await _recruitmentService.UpdateJobPostingAsync(id, request);
            return Ok(updated);
        }

        [HttpPatch("jobs/{id}/status")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> UpdateJobStatus(int id, [FromBody] UpdateJobStatusRequest request)
        {
            await _recruitmentService.UpdateJobStatusAsync(id, request.Status);
            return NoContent();
        }

        [HttpDelete("jobs/{id}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            await _recruitmentService.DeleteJobPostingAsync(id);
            return NoContent();
        }

        [HttpGet("jobs/{jobId}/applications")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<IEnumerable<JobApplicationDto>>> GetApplications(int jobId)
        {
            var apps = await _recruitmentService.GetApplicationsByJobIdAsync(jobId);
            return Ok(apps);
        }

        [HttpPatch("applications/{id}/status")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateApplicationStatusRequest request)
        {
            await _recruitmentService.UpdateApplicationStatusAsync(id, request.Status, request.Notes);
            return NoContent();
        }

        // ── Public Endpoints (No Auth) ────────────────────────────────

        [HttpGet("public/jobs")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<JobPostingDto>>> GetOpenJobs()
        {
            var jobs = await _recruitmentService.GetOpenJobPostingsAsync();
            return Ok(jobs);
        }

        [HttpGet("public/jobs/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<JobPostingDto>> GetPublicJobById(int id)
        {
            var job = await _recruitmentService.GetPublicJobPostingByIdAsync(id);
            if (job == null) return NotFound();
            return Ok(job);
        }

        [HttpPost("public/jobs/{jobId}/apply")]
        [AllowAnonymous]
        [RequestSizeLimit(15 * 1024 * 1024)]
        public async Task<ActionResult<JobApplicationDto>> Apply(
            int jobId,
            [FromForm] string candidateName,
            [FromForm] string candidateEmail,
            [FromForm] string? candidatePhone,
            [FromForm] string? coverLetter,
            IFormFile? cv)
        {
            var app = await _recruitmentService.ApplyForJobAsync(
                jobId, candidateName, candidateEmail, candidatePhone, coverLetter, cv);
            return Created(string.Empty, app);
        }
    }
}

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
    public class PerformanceController : ControllerBase
    {
        private readonly IPerformanceService _performanceService;

        public PerformanceController(IPerformanceService performanceService)
        {
            _performanceService = performanceService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PerformanceReviewDto>>> GetPerformanceReviews(
            [FromQuery] int? employeeId)
        {
            var reviews = await _performanceService.GetReviewsAsync(employeeId);
            return Ok(reviews.Select(review => review.ToDto()));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<PerformanceReviewDto>> CreatePerformanceReview(PerformanceReview review)
        {
            var created = await _performanceService.CreateReviewAsync(review);
            return CreatedAtAction(nameof(GetPerformanceReviews), new { id = created.Id }, created.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> UpdatePerformanceReview(int id, PerformanceReview review)
        {
            await _performanceService.UpdateReviewAsync(id, review);
            return NoContent();
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<PerformanceReviewDto>> ApprovePerformanceReview(int id, [FromBody] ApprovePerformanceRequest request)
        {
            var review = await _performanceService.ApproveReviewAsync(id, request.Comment);
            return Ok(review.ToDto());
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<ActionResult<PerformanceReviewDto>> RejectPerformanceReview(int id, [FromBody] RejectPerformanceRequest request)
        {
            var review = await _performanceService.RejectReviewAsync(id, request.Reason);
            return Ok(review.ToDto());
        }
    }

    public record ApprovePerformanceRequest(string? Comment);
    public record RejectPerformanceRequest(string Reason);
}

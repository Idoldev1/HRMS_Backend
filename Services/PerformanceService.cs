using HRMS.API.Models;
using HRMS.API.Repositories;

namespace HRMS.API.Services
{
    public interface IPerformanceService
    {
        Task<IEnumerable<PerformanceReview>> GetReviewsAsync(int? employeeId);
        Task<PerformanceReview> CreateReviewAsync(PerformanceReview review);
        Task UpdateReviewAsync(int id, PerformanceReview review);
        Task<PerformanceReview> ApproveReviewAsync(int id, string? comment);
        Task<PerformanceReview> RejectReviewAsync(int id, string reason);
    }

    public class PerformanceService : IPerformanceService
    {
        private readonly IPerformanceRepository _performanceRepository;
        private readonly ILogger<PerformanceService> _logger;

        public PerformanceService(IPerformanceRepository performanceRepository, ILogger<PerformanceService> logger)
        {
            _performanceRepository = performanceRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<PerformanceReview>> GetReviewsAsync(int? employeeId)
        {
            _logger.LogInformation("Retrieving performance reviews. EmployeeId: {EmployeeId}", employeeId);

            var reviews = await _performanceRepository.GetReviewsWithIncludesAsync(employeeId);

            _logger.LogInformation("Retrieved {Count} performance reviews.", reviews.Count());
            return reviews;
        }

        public async Task<PerformanceReview> CreateReviewAsync(PerformanceReview review)
        {
            _logger.LogInformation("Creating performance review for employee {EmployeeId}.", review.EmployeeId);

            review.CreatedAt = DateTime.Now;
            review.UpdatedAt = DateTime.Now;

            var createdReview = await _performanceRepository.AddAsync(review);

            _logger.LogInformation("Performance review created with id {ReviewId}.", createdReview.Id);
            return createdReview;
        }

        public async Task UpdateReviewAsync(int id, PerformanceReview review)
        {
            _logger.LogInformation("Updating performance review with id {ReviewId}.", id);

            if (id != review.Id)
            {
                _logger.LogInformation("Review id mismatch. Route id {RouteId}, body id {BodyId}.", id, review.Id);
                throw new ArgumentException("Performance review id mismatch.");
            }

            var existingReview = await _performanceRepository.GetByIdAsync(id);
            if (existingReview == null)
            {
                _logger.LogInformation("Performance review with id {ReviewId} not found.", id);
                throw new KeyNotFoundException($"Performance review with id {id} not found.");
            }

            review.UpdatedAt = DateTime.Now;
            await _performanceRepository.UpdateAsync(review);

            _logger.LogInformation("Performance review with id {ReviewId} updated successfully.", id);
        }

        public async Task<PerformanceReview> ApproveReviewAsync(int id, string? comment)
        {
            _logger.LogInformation("Approving performance review {ReviewId}.", id);

            var review = await _performanceRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Performance review with id {id} not found.");

            if (review.Status != "Submitted")
                throw new InvalidOperationException($"Only submitted reviews can be approved. Current status: '{review.Status}'.");

            review.Status = "Approved";
            review.ApprovalComment = comment;
            review.UpdatedAt = DateTime.Now;
            await _performanceRepository.UpdateAsync(review);

            _logger.LogInformation("Performance review {ReviewId} approved.", id);
            return review;
        }

        public async Task<PerformanceReview> RejectReviewAsync(int id, string reason)
        {
            _logger.LogInformation("Rejecting performance review {ReviewId}.", id);

            var review = await _performanceRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Performance review with id {id} not found.");

            if (review.Status != "Submitted")
                throw new InvalidOperationException($"Only submitted reviews can be rejected. Current status: '{review.Status}'.");

            review.Status = "Rejected";
            review.ApprovalComment = reason;
            review.UpdatedAt = DateTime.Now;
            await _performanceRepository.UpdateAsync(review);

            _logger.LogInformation("Performance review {ReviewId} rejected.", id);
            return review;
        }
    }
}


using HRMS.API.Contracts.Recruitment;
using HRMS.API.DTOs;
using HRMS.API.Models;
using HRMS.API.Repositories;
using HRMS.API.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Services
{
    public interface IRecruitmentService
    {
        // Job Postings - HR
        Task<IEnumerable<JobPostingDto>> GetAllJobPostingsAsync();
        Task<JobPostingDetailDto?> GetJobPostingByIdAsync(int id);
        Task<JobPostingDto> CreateJobPostingAsync(CreateJobPostingRequest request, int? postedById);
        Task<JobPostingDto> UpdateJobPostingAsync(int id, UpdateJobPostingRequest request);
        Task UpdateJobStatusAsync(int id, string status);
        Task DeleteJobPostingAsync(int id);

        // Job Postings - Public
        Task<IEnumerable<JobPostingDto>> GetOpenJobPostingsAsync();
        Task<JobPostingDto?> GetPublicJobPostingByIdAsync(string jobPostingId);

        // Applications
        Task<IEnumerable<JobApplicationDto>> GetApplicationsByJobIdAsync(int jobPostingId);
        Task<(bool Success, bool NotFound, JobApplicationDto? Application, string? ErrorMessage)> ApplyForJobAsync(string jobPostingId, ApplyForJobRequest request);
        Task UpdateApplicationStatusAsync(int applicationId, string status, string? notes);
    }

    public class RecruitmentService : IRecruitmentService
    {
        private readonly IJobPostingRepository _jobPostingRepository;
        private readonly IJobApplicationRepository _jobApplicationRepository;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<RecruitmentService> _logger;
        private readonly long _maxFileSizeBytes;
        private readonly HashSet<string> _allowedContentTypes;

        public RecruitmentService(
            IJobPostingRepository jobPostingRepository,
            IJobApplicationRepository jobApplicationRepository,
            IWebHostEnvironment env,
            IConfiguration configuration,
            ILogger<RecruitmentService> logger)
        {
            _jobPostingRepository = jobPostingRepository;
            _jobApplicationRepository = jobApplicationRepository;
            _env = env;
            _logger = logger;
            _maxFileSizeBytes = configuration.GetValue<long>("UploadSettings:MaxFileSizeBytes", 10 * 1024 * 1024);
            var types = configuration["UploadSettings:AllowedContentTypes"]
                        ?? "application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            _allowedContentTypes = new HashSet<string>(types.Split(','), StringComparer.OrdinalIgnoreCase);
        }

        // ── Job Postings - HR ──────────────────────────────────────────

        public async Task<IEnumerable<JobPostingDto>> GetAllJobPostingsAsync()
        {
            _logger.LogInformation("Retrieving all job postings.");
            var jobs = await _jobPostingRepository.GetAllWithApplicationCountAsync();
            return jobs.Select(j => j.ToDto());
        }

        public async Task<JobPostingDetailDto?> GetJobPostingByIdAsync(int id)
        {
            var job = await _jobPostingRepository.GetByIdWithApplicationsAsync(id);
            return job?.ToDetailDto();
        }

        public async Task<JobPostingDto> CreateJobPostingAsync(CreateJobPostingRequest request, int? postedById)
        {
            _logger.LogInformation("Creating job posting: {Title}", request.Title);
            var job = new JobPosting
            {
                JobPostingId = Guid.NewGuid().ToString(),
                Title = request.Title,
                Description = request.Description,
                Location = request.Location,
                EmploymentType = request.EmploymentType,
                WorkMode = request.WorkMode,
                SalaryMin = request.SalaryMin,
                SalaryMax = request.SalaryMax,
                Department = request.Department,
                Requirements = request.Requirements,
                Responsibilities = request.Responsibilities,
                Status = request.Status,
                ClosingDate = request.ClosingDate,
                PostedById = postedById,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            var created = await _jobPostingRepository.AddAsync(job);
            _logger.LogInformation("Job posting created with id {JobId}.", created.Id);
            return created.ToDto();
        }

        public async Task<JobPostingDto> UpdateJobPostingAsync(int id, UpdateJobPostingRequest request)
        {
            var job = await _jobPostingRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Job posting with id {id} not found.");

            job.Title = request.Title;
            job.Description = request.Description;
            job.Location = request.Location;
            job.EmploymentType = request.EmploymentType;
            job.WorkMode = request.WorkMode;
            job.SalaryMin = request.SalaryMin;
            job.SalaryMax = request.SalaryMax;
            job.Department = request.Department;
            job.Requirements = request.Requirements;
            job.Responsibilities = request.Responsibilities;
            job.Status = request.Status;
            job.ClosingDate = request.ClosingDate;
            job.UpdatedAt = DateTime.Now;

            await _jobPostingRepository.UpdateAsync(job);
            return job.ToDto();
        }

        public async Task UpdateJobStatusAsync(int id, string status)
        {
            var job = await _jobPostingRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Job posting with id {id} not found.");

            job.Status = status;
            job.UpdatedAt = DateTime.Now;
            await _jobPostingRepository.UpdateAsync(job);
        }

        public async Task DeleteJobPostingAsync(int id)
        {
            var job = await _jobPostingRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Job posting with id {id} not found.");

            await _jobPostingRepository.DeleteAsync(job);
            _logger.LogInformation("Job posting {JobId} deleted.", id);
        }

        // ── Job Postings - Public ──────────────────────────────────────

        public async Task<IEnumerable<JobPostingDto>> GetOpenJobPostingsAsync()
        {
            var jobs = await _jobPostingRepository.GetByStatusAsync("Open");
            return jobs.Select(j => j.ToDto());
        }

        public async Task<JobPostingDto?> GetPublicJobPostingByIdAsync(string jobPostingId)
        {
            var job = await _jobPostingRepository.GetByPublicIdWithApplicationsAsync(jobPostingId);
            if (job == null || job.Status != "Open") return null;
            return job.ToDto();
        }

        // ── Applications ──────────────────────────────────────────────

        public async Task<IEnumerable<JobApplicationDto>> GetApplicationsByJobIdAsync(int jobPostingId)
        {
            var apps = await _jobApplicationRepository.GetByJobPostingIdAsync(jobPostingId);
            return apps.Select(a => a.ToDto());
        }

        public async Task<(bool Success, bool NotFound, JobApplicationDto? Application, string? ErrorMessage)> ApplyForJobAsync(
            string jobPostingId, ApplyForJobRequest request)
        {
            var sanitizedName = RecruitmentInputSanitizer.NormalizeName(request.CandidateName);
            var normalizedEmail = RecruitmentInputSanitizer.NormalizeEmail(request.CandidateEmail);
            var sanitizedEmail = RecruitmentInputSanitizer.NormalizeEmailForStorage(request.CandidateEmail);

            var job = await _jobPostingRepository.GetByPublicIdAsync(jobPostingId)
                ;
            if (job == null)
                return (false, true, null, $"Job posting not found.");

            if (job.Status != "Open")
                return (false, false, null, "This job posting is not accepting applications.");

            var existing = await _jobApplicationRepository.GetByJobAndEmailAsync(job.Id, normalizedEmail);
            if (existing != null)
                return (false, false, null, "You have already applied for this position.");

            string? cvPath = null;
            var cv = request.Cv;
            if (cv != null && cv.Length > 0)
            {
                if (cv.Length > _maxFileSizeBytes)
                    return (false, false, null, $"File size exceeds the maximum allowed ({_maxFileSizeBytes / (1024 * 1024)}MB).");

                if (!_allowedContentTypes.Contains(cv.ContentType))
                    return (false, false, null, "File type not allowed. Please upload a PDF or Word document.");

                var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads", "cv", job.JobPostingId);
                Directory.CreateDirectory(uploadsRoot);

                var safeFileName = $"{Guid.NewGuid()}{Path.GetExtension(cv.FileName)}";
                var filePath = Path.Combine(uploadsRoot, safeFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await cv.CopyToAsync(stream);

                cvPath = $"uploads/cv/{job.JobPostingId}/{safeFileName}";
            }

            var application = new JobApplication
            {
                JobPostingId = job.Id,
                CandidateName = sanitizedName,
                CandidateEmail = sanitizedEmail,
                NormalizedCandidateEmail = normalizedEmail,
                CandidatePhone = request.CandidatePhone,
                CoverLetter = request.CoverLetter,
                CvFilePath = cvPath,
                Status = "Applied",
                AppliedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            try
            {
                var created = await _jobApplicationRepository.AddAsync(application);
                _logger.LogInformation("Application {AppId} created for job {JobId} by {Email}.", created.Id, jobPostingId, sanitizedEmail);
                return (true, false, created.ToDto(), null);
            }
            catch (DbUpdateException)
            {
                return (false, false, null, "You have already applied for this position.");
            }
        }

        public async Task UpdateApplicationStatusAsync(int applicationId, string status, string? notes)
        {
            var app = await _jobApplicationRepository.GetByIdAsync(applicationId)
                ?? throw new KeyNotFoundException($"Application with id {applicationId} not found.");

            app.Status = status;
            app.Notes = notes ?? app.Notes;
            app.UpdatedAt = DateTime.Now;
            await _jobApplicationRepository.UpdateAsync(app);
            _logger.LogInformation("Application {AppId} status updated to {Status}.", applicationId, status);
        }
    }
}

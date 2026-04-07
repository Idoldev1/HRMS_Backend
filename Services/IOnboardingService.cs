using HRMS.API.Contracts.Onboarding;
using HRMS.API.DTOs;

namespace HRMS.API.Services
{
    public interface IOnboardingService
    {
        /// <summary>
        /// Get the onboarding profile by token.
        /// </summary>
        Task<OnboardingProfileDto?> GetProfileAsync(string token);

        /// <summary>
        /// Update employee's personal details during onboarding.
        /// </summary>
        Task<OnboardingProfileDto> UpdateProfileAsync(string token, UpdateOnboardingRequest request);

        /// <summary>
        /// Complete the onboarding process and activate the employee account.
        /// </summary>
        Task CompleteAsync(string token);

        /// <summary>
        /// Upload a document during onboarding.
        /// </summary>
        Task<OnboardingDocumentDto> UploadDocumentAsync(
            string token,
            string documentType,
            IFormFile file);

        /// <summary>
        /// Get all documents uploaded by the employee during onboarding.
        /// </summary>
        Task<IEnumerable<OnboardingDocumentDto>> GetDocumentsAsync(string token);

        /// <summary>
        /// Delete a document uploaded during onboarding.
        /// </summary>
        Task DeleteDocumentAsync(string token, int docId);

        /// <summary>
        /// Create login user profile for the pending employee and complete onboarding.
        /// </summary>
        Task CreateUserProfileAsync(string token, CreateOnboardingUserProfileRequest request);
    }
}

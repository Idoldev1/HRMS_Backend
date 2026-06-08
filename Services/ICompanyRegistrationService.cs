using HRMS.API.Contracts.CompanyRegistration;
using HRMS.API.DTOs;
using HRMS.API.Models;
using Microsoft.AspNetCore.Http;

namespace HRMS.API.Services
{
    public interface ICompanyRegistrationService
    {
        Task<(bool Success, string Token, CompanyRegistrationResponseDto? Response, string? ErrorMessage)>
            RegisterCompanyAsync(CompanyRegistrationRequest request);

        Task<OnboardingDocumentDto> UploadDocumentAsync(int employeeId, string documentType, IFormFile file);
    }
}

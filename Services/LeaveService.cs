using HRMS.API.Contracts.Leave;
using HRMS.API.DTOs;
using HRMS.API.Models;
using HRMS.API.Repositories;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Services
{
    public interface ILeaveService
    {
        Task<IEnumerable<LeaveDto>> GetLeavesAsync(int? employeeId, string? status);
        Task<(bool Success, LeaveDto? Leave, string? ErrorMessage)> CreateLeaveAsync(CreateLeaveRequest request);
        Task ApproveLeaveAsync(int id, int approvedById);
        Task RejectLeaveAsync(int id, string reason);
    }

    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<LeaveService> _logger;

        public LeaveService(ILeaveRepository leaveRepository, IEmployeeRepository employeeRepository, ILogger<LeaveService> logger)
        {
            _leaveRepository = leaveRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<LeaveDto>> GetLeavesAsync(int? employeeId, string? status)
        {
            _logger.LogInformation("Retrieving leave requests. EmployeeId: {EmployeeId}, Status: {Status}", employeeId, status);

            var leaves = await _leaveRepository.GetLeavesWithIncludesAsync(employeeId, status);

            _logger.LogInformation("Retrieved {Count} leave records.", leaves.Count());
            return leaves.Select(l => l.ToDto());
        }

        public async Task<(bool Success, LeaveDto? Leave, string? ErrorMessage)> CreateLeaveAsync(CreateLeaveRequest request)
        {
            _logger.LogInformation("Creating leave request for employee {EmployeeId}.", request.EmployeeId);

            if (await _leaveRepository.HasActiveLeaveAsync(request.EmployeeId, request.StartDate, request.EndDate))
            {
                return (false, null, "You currently have an active leave and cannot apply for another leave.");
            }

            var leave = new Leave
            {
                EmployeeId = request.EmployeeId,
                LeaveType = request.LeaveType,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalDays = request.TotalDays,
                Reason = request.Reason,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            leave.TotalDays = (leave.EndDate - leave.StartDate).Days + 1;

            var created = await _leaveRepository.AddAsync(leave);

            _logger.LogInformation("Leave request created with id {LeaveId}.", created.Id);
            return (true, created.ToDto(), null);
        }

        public async Task ApproveLeaveAsync(int id, int approvedById)
        {
            _logger.LogInformation($"Approving leave request with id {id} by approver {approvedById}.");

            var leave = await _leaveRepository.GetByIdAsync(id);
            if (leave == null)
            {
                _logger.LogInformation($"Cannot approve leave. Leave with id {id} not found.");
                throw new KeyNotFoundException($"Leave with id {id} not found.");
            }

            var approver = await _employeeRepository.GetByIdAsync(approvedById);
            if (approver == null)
            {
                _logger.LogInformation($"Cannot approve leave. Approver with id {approvedById} not found.");
                throw new KeyNotFoundException($"Approver with id {approvedById} not found.");
            }

            leave.Status = "Approved";
            leave.ApprovedById = approvedById;
            leave.ApprovedAt = DateTime.Now;
            leave.UpdatedAt = DateTime.Now;

            await _leaveRepository.UpdateAsync(leave);

            _logger.LogInformation($"Leave request with id {id} approved successfully.");
        }

        public async Task RejectLeaveAsync(int id, string reason)
        {
            _logger.LogInformation($"Rejecting leave request with id {id}.");

            var leave = await _leaveRepository.GetByIdAsync(id);
            if (leave == null)
            {
                _logger.LogInformation($"Cannot reject leave. Leave with id {id} not found.");
                throw new KeyNotFoundException($"Leave with id {id} not found.");
            }

            leave.Status = "Rejected";
            leave.RejectionReason = reason;
            leave.UpdatedAt = DateTime.Now;

            await _leaveRepository.UpdateAsync(leave);

            _logger.LogInformation($"Leave request with id {id} rejected successfully.");
        }
    }
}


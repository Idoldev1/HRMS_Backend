using HRMS.API.Models;
using HRMS.API.Services;

namespace HRMS.API.DTOs
{
    public static class DtoMappings
    {
        public static MessageDto ToMessageDto(this string message) => new(message);

        public static DepartmentSummaryDto? ToDepartmentSummaryDto(this Department? department)
        {
            if (department == null)
            {
                return null;
            }

            return new DepartmentSummaryDto
            {
                Id = department.Id,
                Name = department.Name,
            };
        }

        public static DepartmentDto ToDto(this Department department) => new()
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            Budget = department.Budget,
            Location = department.Location,
        };

        public static EmployeeDocumentDto ToDto(this EmployeeDocument document) => new()
        {
            Id = document.Id,
            DocumentType = document.DocumentType,
            FileName = document.FileName,
            FileSizeBytes = document.FileSizeBytes,
            UploadedAt = document.UploadedAt,
        };

        public static EmployeeSummaryDto? ToEmployeeSummaryDto(this Employee? employee)
        {
            if (employee == null)
            {
                return null;
            }

            return new EmployeeSummaryDto
            {
                Id = employee.Id,
                EmployeeId = employee.EmployeeId,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Position = employee.Position,
                JobTitle = employee.Position,
                Department = employee.Department.ToDepartmentSummaryDto(),
            };
        }

        public static EmployeeDto ToDto(this Employee employee) => new()
        {
            Id = employee.Id,
            EmployeeId = employee.EmployeeId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            DateOfBirth = employee.DateOfBirth,
            DepartmentId = employee.DepartmentId,
            Department = employee.Department.ToDepartmentSummaryDto(),
            Position = employee.Position,
            Salary = employee.Salary,
            HireDate = employee.HireDate,
            EmploymentType = employee.EmploymentType,
            Status = employee.Status,
            ManagerId = employee.ManagerId,
            City = employee.City,
            Country = employee.Country,
            Documents = (employee.Documents ?? []).Select(ToDto).ToList(),
        };

        public static AttendanceDto ToDto(this Attendance attendance) => new()
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            Date = attendance.Date,
            CheckIn = attendance.CheckIn,
            CheckOut = attendance.CheckOut,
            BreakDuration = attendance.BreakDuration,
            TotalHours = attendance.TotalHours,
            Status = attendance.Status,
            Notes = attendance.Notes,
            Location = attendance.Location,
            DeviceId = attendance.DeviceId,
            CheckInLatitude = attendance.CheckInLatitude,
            CheckInLongitude = attendance.CheckInLongitude,
            WorkLocationId = attendance.WorkLocationId,
            Employee = attendance.Employee.ToEmployeeSummaryDto(),
        };

        public static WorkLocationDto ToDto(this WorkLocation location) => new()
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            AllowedRadiusMeters = location.AllowedRadiusMeters,
            IsActive = location.IsActive,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt,
        };

        public static EmployeeWorkLocationDto ToDto(this EmployeeWorkLocation ewl) => new()
        {
            Id = ewl.Id,
            EmployeeId = ewl.EmployeeId,
            WorkLocationId = ewl.WorkLocationId,
            IsActive = ewl.IsActive,
            AssignedAt = ewl.AssignedAt,
            WorkLocation = ewl.WorkLocation?.ToDto(),
            Employee = ewl.Employee.ToEmployeeSummaryDto(),
        };

        public static EmployeeDeviceDto ToDto(this EmployeeDevice device) => new()
        {
            Id = device.Id,
            EmployeeId = device.EmployeeId,
            DeviceId = device.DeviceId,
            DeviceName = device.DeviceName,
            DeviceType = device.DeviceType,
            IsActive = device.IsActive,
            RegisteredAt = device.RegisteredAt,
            LastUsedAt = device.LastUsedAt,
            Employee = device.Employee.ToEmployeeSummaryDto(),
        };

        public static LeaveDto ToDto(this Leave leave) => new()
        {
            Id = leave.Id,
            EmployeeId = leave.EmployeeId,
            LeaveType = leave.LeaveType,
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            TotalDays = leave.TotalDays,
            Reason = leave.Reason,
            Status = leave.Status,
            ApprovedById = leave.ApprovedById,
            ApprovedAt = leave.ApprovedAt,
            RejectionReason = leave.RejectionReason,
            CreatedAt = leave.CreatedAt,
            UpdatedAt = leave.UpdatedAt,
            Employee = leave.Employee.ToEmployeeSummaryDto(),
            ApprovedBy = leave.ApprovedBy.ToEmployeeSummaryDto(),
        };

        public static PayrollDto ToDto(this Payroll payroll) => new()
        {
            Id = payroll.Id,
            EmployeeId = payroll.EmployeeId,
            PayPeriodStartDate = payroll.PayPeriodStartDate,
            PayPeriodEndDate = payroll.PayPeriodEndDate,
            BaseSalary = payroll.BaseSalary,
            HousingAllowance = payroll.HousingAllowance,
            TransportAllowance = payroll.TransportAllowance,
            MedicalAllowance = payroll.MedicalAllowance,
            OtherAllowances = payroll.OtherAllowances,
            TaxDeduction = payroll.TaxDeduction,
            InsuranceDeduction = payroll.InsuranceDeduction,
            LoanDeduction = payroll.LoanDeduction,
            OtherDeductions = payroll.OtherDeductions,
            OvertimeHours = payroll.OvertimeHours,
            OvertimeRate = payroll.OvertimeRate,
            OvertimeAmount = payroll.OvertimeAmount,
            Bonuses = payroll.Bonuses,
            TotalEarnings = payroll.TotalEarnings,
            TotalDeductions = payroll.TotalDeductions,
            NetSalary = payroll.NetSalary,
            Status = payroll.Status,
            PaymentDate = payroll.PaymentDate,
            PaymentMethod = payroll.PaymentMethod,
            ProcessedAt = payroll.ProcessedAt,
            CreatedAt = payroll.CreatedAt,
            Employee = payroll.Employee.ToEmployeeSummaryDto(),
        };

        public static PayrollSummaryDto ToDto(this PayrollSummary summary) => new()
        {
            TotalRecords = summary.TotalRecords,
            PendingCount = summary.PendingCount,
            ProcessedCount = summary.ProcessedCount,
            PaidCount = summary.PaidCount,
            TotalGrossPayroll = summary.TotalGrossPayroll,
            TotalNetPayroll = summary.TotalNetPayroll,
            TotalTaxCollected = summary.TotalTaxCollected,
            AverageNetSalary = summary.AverageNetSalary,
        };

        public static TaxBreakdownDto ToDto(this TaxBreakdown breakdown) => new()
        {
            AnnualGross = breakdown.AnnualGross,
            AnnualTax = breakdown.AnnualTax,
            EffectiveRate = breakdown.EffectiveRate,
            Brackets = breakdown.Brackets
                .Select(bracket => new TaxBracketDto
                {
                    Label = bracket.Label,
                    Rate = bracket.Rate,
                    IncomeInBracket = bracket.IncomeInBracket,
                    Tax = bracket.Tax,
                })
                .ToArray(),
            MonthlyTax = breakdown.MonthlyTax,
        };

        public static PerformanceReviewDto ToDto(this PerformanceReview review) => new()
        {
            Id = review.Id,
            EmployeeId = review.EmployeeId,
            ReviewedById = review.ReviewedById,
            ReviewPeriodStartDate = review.ReviewPeriodStartDate,
            ReviewPeriodEndDate = review.ReviewPeriodEndDate,
            OverallRating = review.OverallRating,
            QualityRating = review.QualityRating,
            ProductivityRating = review.ProductivityRating,
            CommunicationRating = review.CommunicationRating,
            TeamworkRating = review.TeamworkRating,
            LeadershipRating = review.LeadershipRating,
            Strengths = review.Strengths,
            AreasForImprovement = review.AreasForImprovement,
            ReviewerComments = review.ReviewerComments,
            EmployeeComments = review.EmployeeComments,
            Status = review.Status,
            ApprovalComment = review.ApprovalComment,
            NextReviewDate = review.NextReviewDate,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            Employee = review.Employee.ToEmployeeSummaryDto(),
        };

        public static AuthUserDto ToDto(this ApplicationUser user) => new()
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Role = user.Role ?? string.Empty,
            EmployeeId = user.EmployeeId ?? string.Empty,
        };

        public static AuthEmployeeDto? ToAuthEmployeeDto(this Employee? employee)
        {
            if (employee == null)
            {
                return null;
            }

            return new AuthEmployeeDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Position = employee.Position,
            };
        }

        public static CompanyDto ToDto(this Company company) => new()
        {
            Id = company.Id,
            Name = company.Name,
            RegistrationNumber = company.RegistrationNumber,
            Industry = company.Industry,
            Email = company.Email,
            Phone = company.Phone,
            Website = company.Website,
            Street = company.Street,
            City = company.City,
            State = company.State,
            ZipCode = company.ZipCode,
            Country = company.Country,
        };

        public static JobPostingDto ToDto(this JobPosting job) => new()
        {
            Id = job.Id,
            JobPostingId = job.JobPostingId,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            EmploymentType = job.EmploymentType,
            WorkMode = job.WorkMode,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            Department = job.Department,
            Requirements = job.Requirements,
            Responsibilities = job.Responsibilities,
            Status = job.Status,
            ClosingDate = job.ClosingDate,
            PostedById = job.PostedById,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            ApplicationCount = job.Applications?.Count ?? 0,
        };

        public static JobPostingDetailDto ToDetailDto(this JobPosting job) => new()
        {
            Id = job.Id,
            JobPostingId = job.JobPostingId,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            EmploymentType = job.EmploymentType,
            WorkMode = job.WorkMode,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            Department = job.Department,
            Requirements = job.Requirements,
            Responsibilities = job.Responsibilities,
            Status = job.Status,
            ClosingDate = job.ClosingDate,
            PostedById = job.PostedById,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            ApplicationCount = job.Applications?.Count ?? 0,
            Applications = job.Applications?.Select(a => a.ToDto()).ToList() ?? new(),
        };

        public static JobApplicationDto ToDto(this JobApplication app) => new()
        {
            Id = app.Id,
            JobPostingId = app.JobPostingId,
            JobPostingPublicId = app.JobPosting?.JobPostingId ?? string.Empty,
            CandidateName = app.CandidateName,
            CandidateEmail = app.CandidateEmail,
            CandidatePhone = app.CandidatePhone,
            CvFilePath = app.CvFilePath,
            CoverLetter = app.CoverLetter,
            Status = app.Status,
            Notes = app.Notes,
            AppliedAt = app.AppliedAt,
            UpdatedAt = app.UpdatedAt,
            JobTitle = app.JobPosting?.Title,
        };

        public static TimesheetEntryDto ToDto(this TimesheetEntry entry) => new()
        {
            Id = entry.Id,
            TimesheetId = entry.TimesheetId,
            Date = entry.Date,
            AttendanceStatus = entry.AttendanceStatus,
            HoursWorked = entry.HoursWorked,
            TasksCompleted = entry.TasksCompleted,
            Notes = entry.Notes,
        };

        public static TimesheetAuditLogDto ToDto(this TimesheetAuditLog log) => new()
        {
            Id = log.Id,
            Action = log.Action,
            PerformedById = log.PerformedById,
            PerformedByName = log.PerformedByName,
            Comment = log.Comment,
            PreviousStatus = log.PreviousStatus,
            NewStatus = log.NewStatus,
            PerformedAt = log.PerformedAt,
        };

        public static TimesheetSummaryDto ToSummaryDto(this Timesheet t) => new()
        {
            Id = t.Id,
            EmployeeId = t.EmployeeId,
            Month = t.Month,
            Year = t.Year,
            MonthName = new DateTime(t.Year, t.Month, 1).ToString("MMMM yyyy"),
            TotalWorkingDays = t.TotalWorkingDays,
            PresentDays = t.PresentDays,
            LateDays = t.LateDays,
            OnLeaveDays = t.OnLeaveDays,
            AbsentDays = t.AbsentDays,
            TotalHours = t.TotalHours,
            Status = t.Status,
            IsLocked = t.IsLocked,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            Employee = t.Employee.ToEmployeeSummaryDto(),
        };

        public static TimesheetDto ToDto(this Timesheet t) => new()
        {
            Id = t.Id,
            EmployeeId = t.EmployeeId,
            Month = t.Month,
            Year = t.Year,
            MonthName = new DateTime(t.Year, t.Month, 1).ToString("MMMM yyyy"),
            TotalWorkingDays = t.TotalWorkingDays,
            PresentDays = t.PresentDays,
            LateDays = t.LateDays,
            OnLeaveDays = t.OnLeaveDays,
            AbsentDays = t.AbsentDays,
            TotalHours = t.TotalHours,
            Status = t.Status,
            IsLocked = t.IsLocked,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            ApprovedById = t.ApprovedById,
            ApprovalComment = t.ApprovalComment,
            RejectionReason = t.RejectionReason,
            ApprovedAt = t.ApprovedAt,
            LockedAt = t.LockedAt,
            LockedBy = t.LockedBy,
            Employee = t.Employee.ToEmployeeSummaryDto(),
            ApprovedBy = t.ApprovedBy.ToEmployeeSummaryDto(),
            Entries = (t.Entries ?? []).Select(e => e.ToDto()).ToList(),
            AuditLogs = (t.AuditLogs ?? []).Select(a => a.ToDto()).ToList(),
        };
    }
}

namespace HRMS.API.Contracts.Timesheet
{
    public class GenerateTimesheetRequest
    {
        public int? EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class UpdateTimesheetEntryRequest
    {
        public int EntryId { get; set; }
        public string? TasksCompleted { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateTimesheetEntriesRequest
    {
        public List<UpdateTimesheetEntryRequest> Entries { get; set; } = new();
    }

    public class ApproveTimesheetRequest
    {
        public string? Comment { get; set; }
    }

    public class RejectTimesheetRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class GetTimesheetsRequest
    {
        public int? EmployeeId { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public string? Status { get; set; }
    }
}

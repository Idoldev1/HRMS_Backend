namespace HRMS.API.Contracts.Attendance
{
    public class GetAttendancesRequest
    {
        public string? EmployeeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}


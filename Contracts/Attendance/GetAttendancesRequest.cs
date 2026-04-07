namespace HRMS.API.Contracts.Attendance
{
    public class GetAttendancesRequest
    {
        public int? EmployeeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}


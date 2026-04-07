namespace HRMS.API.Contracts.Leave
{
    public class CreateLeaveRequest
    {
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}

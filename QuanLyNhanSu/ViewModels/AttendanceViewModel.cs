namespace QuanLyNhanSu.ViewModels
{
    public class AttendanceViewModel
    {
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public bool IsPresent { get; set; }
        public bool IsLate { get; set; }
        public bool IsAbsent { get; set; }
        public bool HasAttendanceData { get; set; }
    }

}

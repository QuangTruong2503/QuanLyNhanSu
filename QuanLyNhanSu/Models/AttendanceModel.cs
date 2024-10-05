using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class AttendanceModel
    {
        [Key]
        [Display(Name = "Mã chấm công")]
        public int Attendance_Id { get; set; }

        [Display(Name = "Mã nhân viên")]
        public string Employee_Id { get; set; }

        [Display(Name = "Ngày chấm công ")]
        public DateTime Attendance_Date { get; set; }


        [Display(Name = "Trạng thái")]
        public AttendanceStatus Status { get; set; }

        // Navigation Property
        public EmployeesModel Employee { get; set; }
    }
    public enum AttendanceStatus
    {
        Có,
        Vắng,
        Trễ,
        Rời
    }
}

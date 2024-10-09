using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.Models
{
    public class AttendanceModel
    {
        [Key]
        [Display(Name = "Mã chấm công")]
        public int Attendance_Id { get; set; }

        [Display(Name = "Mã nhân viên")]
        public string? Employee_Id { get; set; }

        [Display(Name = "Ngày chấm công ")]
        public DateTime Attendance_Date { get; set; }


        [Display(Name = "Trạng thái")]
        public int status_id { get; set; }

        [NotMapped]
        [ValidateNever]
        // Navigation Property
        public EmployeesModel Employee { get; set; }

        [NotMapped]
        [ValidateNever]
        public AttendenceStatusModel AttendenceStatus { get; set; }
    }
}

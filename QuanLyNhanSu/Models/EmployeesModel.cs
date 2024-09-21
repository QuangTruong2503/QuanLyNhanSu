using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.Models
{
    public class EmployeesModel
    {
        [Key]
        [Display(Name = "Mã nhân viên")]
        public int employee_id { get; set; }
        [Display(Name = "Họ và đệm")]
        public required string first_name { get; set; }
        [Display(Name = "Tên")]
        public required string last_name { get; set; }
        [Display(Name = "Email")]
        public required string email { get; set; }
        [Display(Name = "SĐT")]
        public required string phone { get; set; }
        [Display(Name = "Ngày sinh")]
        public DateOnly? date_of_birth { get; set; }
        public enum Gender
        {
            Nam,
            Nữ,
            Khác
        }
        [Display(Name = "Giới tính")]
        [Column(TypeName = "varchar(100)")] // MySQL sẽ lưu enum dưới dạng chuỗi
        public Gender gender { get; set; }
        [Display(Name = "Ngày bắt đầu")]
        public DateOnly hire_date { get; set; }
        [Display(Name = "Vị trí")]
        public required string position { get; set; }
        [Display(Name = "Mã phòng ban")]
        public int department_id { get; set; } 
    }
}

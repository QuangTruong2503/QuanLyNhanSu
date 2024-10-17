using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.Models
{
	public class EmployeesModel
	{
        [Key]
		[Display(Name = "Mã nhân viên")]
		public string? employee_id { get; set; }

		[Required(ErrorMessage = "Họ và đệm là bắt buộc.")]
		[Display(Name = "Họ và đệm")]
		public string first_name { get; set; }

		[Required(ErrorMessage = "Tên là bắt buộc.")]
		[Display(Name = "Tên")]
		public string last_name { get; set; }

		[Required(ErrorMessage = "Email là bắt buộc.")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ.")]
		[Display(Name = "Email")]
		public string email { get; set; }

		[Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
		[Display(Name = "SĐT")]
		public string phone { get; set; }

		[Display(Name = "Mật khẩu")]
		public string? hashed_password { get; set; }

		[Display(Name = "Ngày sinh")]
		public DateOnly? date_of_birth { get; set; }

		public enum Gender
		{
			Nam,
			Nữ,
			Khác
		}

		[Display(Name = "Giới tính")]
		[Column(TypeName = "varchar(100)")] // Lưu enum dưới dạng chuỗi
		public Gender? gender { get; set; }

		[Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
		[Display(Name = "Ngày bắt đầu")]
		public DateTime hire_date { get; set; }

		[Display(Name = "Ngày kết thúc")]
		[NotMapped]
		public DateTime? expired_date { get; set; }

		[Display(Name = "Mã vị trí")]
		public int? position_id { get; set; }

        [Display(Name = "Mã phòng ban")]
		public int? department_id { get; set; }

		[Display(Name = "Mã Vãi trò")]
		public int role_id { get; set; }

		[NotMapped]
		[ValidateNever]
        public RoleModel Role { get; set; }
		[NotMapped]
		[ValidateNever]
		public departmentsModel departments { get; set; }

		[NotMapped]
		[ValidateNever]
		public PositionModel Positions { get; set; }
	}

}

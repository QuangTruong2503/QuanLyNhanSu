using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Display(Name = "Mã nhân sự")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        [Display(Name = "Mật khẩu mới")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc.")]
        [Display(Name = "Xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}

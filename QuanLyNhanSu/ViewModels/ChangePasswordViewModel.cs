namespace QuanLyNhanSu.ViewModels
{
    public class ChangePasswordViewModel
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}

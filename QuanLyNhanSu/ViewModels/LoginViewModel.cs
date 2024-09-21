namespace QuanLyNhanSu.ViewModels
{
    public class LoginViewModel
    {
        public required string UsernameOrEmployeeId { get; set; }  // Cho phép nhập employee_id hoặc username
        public required string Password { get; set; }
    }
}

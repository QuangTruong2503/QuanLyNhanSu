using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class Login
    {
        [Key]
        public int login_id { get; set; }
        public int employee_id { get; set; }
        public required string username { get; set; }
        public required string hashed_password { get; set; }
    }
}

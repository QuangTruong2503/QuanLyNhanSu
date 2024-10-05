using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class RoleModel
    {
        [Key]
        [Display(Name = "Mã vai trò")]
        public int Role_Id { get; set; }

        [Display(Name = "Tên vai trò")]

        public required string Role_Name { get; set; }
    }
}

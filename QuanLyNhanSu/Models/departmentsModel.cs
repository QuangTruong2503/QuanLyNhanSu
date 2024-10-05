using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class departmentsModel
    {
        [Key]
        [Display(Name = "Mã phòng ban")]
        public int department_id {get; set;}

        [Display(Name = "Tên phòng ban")]
        public required string department_name {get; set;}
    }
}

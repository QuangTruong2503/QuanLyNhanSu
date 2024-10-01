using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class PositionModel
    {
        [Key]
        [Display(Name = "Mã chức vụ")]
        public int position_id { get; set; }
        [Display(Name = "Tên chức vụ")]
        public string position_name { get; set; }
        [Display(Name = "Lương cơ bản")]

        public decimal base_salary { get; set; }
        [Display(Name = "Mã phòng ban")]

        public int department_id { get; set; }
        public departmentsModel departments { get; set; }
    }
}

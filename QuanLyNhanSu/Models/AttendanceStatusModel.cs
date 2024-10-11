using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class AttendanceStatusModel
    {
        [Key]
        public int status_id { get; set; }

        [Display(Name ="Trạng thái")]
        public string? status_name { get; set; }
    }
}

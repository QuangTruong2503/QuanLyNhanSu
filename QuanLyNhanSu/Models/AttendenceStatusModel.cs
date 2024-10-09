using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class AttendenceStatusModel
    {
        [Key]
        public int status_id { get; set; }

        [Display(Name ="Tên chấm công")]
        public string? status_name { get; set; }
    }
}

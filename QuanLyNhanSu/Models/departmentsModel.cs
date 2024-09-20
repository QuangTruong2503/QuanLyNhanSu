using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class departmentsModel
    {
        [Key]
        public int department_id {get; set;}
        public required string department_name {get; set;}
    }
}

using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class BonusModel
    {

        [Key]
        [Display(Name = "Mã thưởng")]
        public int Bonus_Id { get; set; }

        [Display(Name = "Mã nhân viên")]
        public string Employee_Id { get; set; }

        [Display(Name = "Tiền thưởng")]
        public decimal Bonus_Amount { get; set; }

        [Display(Name = "Ngày thưởng")]
        public DateTime Bonus_Date { get; set; }

        [Display(Name = "Lý do thưởng")]
        public string Reason { get; set; }

        // Navigation Property
        public EmployeesModel Employee { get; set; }
    }
}

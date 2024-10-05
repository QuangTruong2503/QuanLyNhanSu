using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class SalaryModel
    {
        [Key]
        [Display(Name = "Mã lương")]
        public int Salary_Id { get; set; }

        [Display(Name = "Mã nhân viên")]
        public string Employee_Id { get; set; }

        [Display(Name = "Tiền lương cơ bản")]
        public decimal? Base_Salary { get; set; }

        [Display(Name = "Tiền thưởng")]
        public decimal? Bonus { get; set; }

        [Display(Name = "Tổng tiền lương")]
        public decimal? Total_Salary { get; set; }

        [Display(Name = "Ngày tính lương")]
        public DateTime? Salary_Date { get; set; }

        // Navigation Property
        public EmployeesModel Employee { get; set; }
    }
}

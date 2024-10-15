using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class SalaryModel
    {
        [Key]
        [Display(Name = "Mã lương")]
        public int Salary_Id { get; set; }

        [Display(Name = "Mã nhân viên")]
        public required string Employee_Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        [Display(Name = "Tiền lương cơ bản")]
        public decimal? Base_Salary { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        [Display(Name = "Tiền thưởng")]
        public decimal? Bonus { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        [Display(Name = "Tiền khấu trừ")]
        public decimal? Deduction { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        [Display(Name = "Tổng tiền lương")]
        public decimal? Total_Salary { get; set; }

        [Display(Name = "Ngày tính lương")]
        public DateTime? Salary_Date { get; set; }

        [ValidateNever]
        // Navigation Property
        public EmployeesModel? Employee { get; set; }
    }
}

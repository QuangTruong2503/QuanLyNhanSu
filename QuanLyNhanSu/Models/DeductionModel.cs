using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class DeductionModel
    {
        [Key]

        [Display(Name = "Mã khấu trừ")]
        public int Deduction_Id { get; set; }

        [Display(Name = "Mã nhân viên")]
        public string Employee_Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        [Display(Name = "Tiền khấu trừ")]
        public decimal Deduction_Amount { get; set; }

        [Display(Name = "Ngày khấu trừ")]
        public DateTime Deduction_Date { get; set; }

        [Display(Name = "Lý do khấu trừ")]
        public string Reason { get; set; }

        // Navigation Property
        [ValidateNever]
        public EmployeesModel? Employee { get; set; }
    }
}

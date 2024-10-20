﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace QuanLyNhanSu.Models
{
    public class BonusModel
    {

        [Key]
        [Display(Name = "Mã thưởng")]
        public int Bonus_Id { get; set; }

        [Display(Name = "Mã nhân viên")]
        public required string Employee_Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        [Display(Name = "Tiền thưởng")]
        public decimal Bonus_Amount { get; set; }

        [Display(Name = "Ngày thưởng")] 
        public DateTime Bonus_Date { get; set; }

        [Display(Name = "Lý do thưởng")]
        public string? Reason { get; set; }

        [ValidateNever]
        // Navigation Property
        public EmployeesModel Employee { get; set; }
    }
}

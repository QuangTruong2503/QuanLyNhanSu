using System.Globalization;

namespace QuanLyNhanSu.Helpers
{
    public class FormatHelpers
    {
        public static string FormatCurrencyVND(decimal currency)
        {
            string formattedSalary = string.Format(new CultureInfo("vi-VN"), "{0:C0}", currency);
            return formattedSalary;
        }
    }
}

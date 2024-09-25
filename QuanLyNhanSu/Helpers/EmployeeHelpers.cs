namespace QuanLyNhanSu.Helpers
{
    public class EmployeeHelpers
    {
        public static string EmployeeCodeRandom() {
            var year = DateTime.Now.Year;
            var twoOfYear = year.ToString().Substring(2, 2);
            var month = DateTime.Now.Month.ToString("D2"); // Đảm bảo tháng luôn có 2 chữ số
            var day = DateTime.Now.Day.ToString("D2");     // Đảm bảo ngày luôn có 2 chữ số
            var random = new Random(Guid.NewGuid().GetHashCode()).Next(1, 100); // Tạo số ngẫu nhiên khác nhau
            var employeeCode = $"NV{twoOfYear}{month}{day}{random:D2}"; // Đảm bảo số ngẫu nhiên cũng có 2 chữ số
            return employeeCode;
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Helpers;
using QuanLyNhanSu.Models;
using QuanLyNhanSu.ViewModels;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace QuanLyNhanSu.Controllers
{
    public class DetailsController : Controller
    {
        private readonly QuanLyNhanSuDbContext _context;
        public DetailsController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }


        //Xem dữ liệu tiền thưởng theo ID Nhân sự đăng nhập
        public async Task<IActionResult> TienThuong()
        {
            var employee = KiemTraDangNhap();
            if (employee == null)
            {
                return RedirectToAction("Login", "Login");
            }

            //Lấy dữ liệu tiền thưởng theo EmployeeID
            var tienThuong = await _context.bonuses.Where(x => x.Employee_Id == employee.employee_id).ToListAsync();
            return View(tienThuong);
        }

        //Xem chi tiết tiền khấu trừ theo ID Nhân sự đăng nhập
        public async Task<IActionResult> KhauTru()
        {
            var employee = KiemTraDangNhap();
            if (employee == null)
            {
                return RedirectToAction("Login", "Login");
            }
            // Lấy dữ liệu khấu trừ theo EmployeeID
            var khauTru = await _context.deductions
                                       .Where(x => x.Employee_Id == employee.employee_id)
                                       .ToListAsync();

            return View(khauTru);
        }

        //Xem chi tiết tiền lương theo ID Nhân sự đăng nhập
        public async Task<IActionResult> TienLuong()
        {
            var employee = KiemTraDangNhap();
            if (employee == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // Lấy dữ liệu lương theo EmployeeID
            var luong = await _context.salaries
                                      .Where(x => x.Employee_Id == employee.employee_id)
                                      .ToListAsync();

            return View(luong);
        }
        //Xem chi tiết tiền lương theo mã lương
        public async Task<IActionResult> ChiTietLuong(int id)
        {
            var salaryByID = await _context.salaries.FirstOrDefaultAsync(s => s.Salary_Id == id);
            if (salaryByID == null)
            {
                ModelState.AddModelError("", "Không tìm thấy lương của nhân viên này");
                return View();
            }
            //Lấy dữ liệu lương cơ bản của nhân viên
            var baseSalaryQuery = from salary in _context.salaries
                                  join emp in _context.employees on salary.Employee_Id equals emp.employee_id
                                  join pos in _context.positions on emp.position_id equals pos.position_id
                                  where emp.employee_id == salaryByID.Employee_Id
                                  select pos.base_salary;
            var baseSalary = await baseSalaryQuery.FirstOrDefaultAsync();
            //Lấy dữ liệu thưởng của nhân viên dựa theo ngày bắt đầu tính lương và kết thúc tính lương
            var bonus = await _context.bonuses
                .Where(b => b.Employee_Id == salaryByID.Employee_Id && b.Bonus_Date >= salaryByID.Begin_Date
                && b.Bonus_Date <= salaryByID.End_Date)
                .ToListAsync();

            //Lấy dữ liệu phạt của nhân viên dựa theo ngày bắt đầu tính lương và kết thúc tính lương
            var deduction = await _context.deductions
                .Where(d => d.Employee_Id == salaryByID.Employee_Id && d.Deduction_Date >= salaryByID.Begin_Date
                && d.Deduction_Date <= salaryByID.End_Date)
                .ToListAsync();
            //Tính tổng số ngày đi làm theo nhân viên từ ngày bắt đầu tính lương đến ngày kết thúc tính lương
            var workingDays = await _context.attendances.Where(a => a.Employee_Id == salaryByID.Employee_Id
                && a.Attendance_Date >= salaryByID.Begin_Date && a.Attendance_Date <= salaryByID.End_Date
                && (a.status_id == 1 || a.status_id == 2))
                .CountAsync();

            // Tổng số ngày cần tính lương(số ngày phải đi làm trong tháng trừ chủ nhật)
            int totalDays = CalculateWorkingDays(salaryByID.Begin_Date, salaryByID.End_Date);

            //Số tiền nhận được dựa trên ngày đi làm
            var amountByWorkingDays = baseSalary / totalDays * workingDays;
            //Truyền dữ liệu vào ViewData
            ViewData["Data"] = salaryByID;
            ViewData["Bonuses"] = bonus;
            ViewData["Deductions"] = deduction;
            ViewData["WorkingDays"] = workingDays;
            ViewData["AmountWorkingDays"] = FormatHelpers.FormatCurrencyVND(amountByWorkingDays);
            ViewData["TotalWorkingDays"] = totalDays;
            return View(salaryByID);
        }
        //Xem chi tiết thông tin nhân sự theo ID Nhân sự đăng nhập
        public async Task<IActionResult> ThongTinTaiKhoan()
        {
            var employee = KiemTraDangNhap();
            if (employee == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // Lấy dữ liệu nhân sự theo EmployeeID
            var thongTin = await _context.employees
                                        .Where(x => x.employee_id == employee.employee_id)
                                        .Include(e => e.departments)
                                        .Include(e => e.Positions)
                                        .Include(e => e.Role)
                                        .FirstOrDefaultAsync();

            return View(thongTin);
        }

        //Đổi mật khẩu
        public IActionResult DoiMatKhau()
        {
            var employee = KiemTraDangNhap();
            if (employee == null)
            {
                return RedirectToAction("Login", "Login");
            }   
            return View();
        }

        //Đổi mật khẩu
        [HttpPost]
        public async Task<IActionResult> DoiMatKhau(ChangePasswordViewModel model)
        {
            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra mật khẩu mới và xác nhận mật khẩu có khớp không
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View(model);
            }

            // Kiểm tra đăng nhập
            var employee = KiemTraDangNhap();
            if (employee == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var employeeID = employee.employee_id;

            // Lấy thông tin nhân viên từ database
            var login = await _context.employees.FirstOrDefaultAsync(l => l.employee_id == employeeID);
            if (login == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin nhân viên.");
                return View(model);
            }

            // Kiểm tra mật khẩu hiện tại có đúng không
            if (!PasswordHasher.VerifyPassword(model.CurrentPassword, login.hashed_password))
            {
                ModelState.AddModelError("", "Mật khẩu cũ không chính xác.");
                return View(model);
            }

            // Cập nhật mật khẩu mới
            login.hashed_password = PasswordHasher.HashPassword(model.NewPassword);
            _context.employees.Update(login);
            await _context.SaveChangesAsync();

            // Thông báo đổi mật khẩu thành công
            TempData["ThongBao"] = "Đổi mật khẩu thành công";

            // Chuyển hướng về trang chủ hoặc trang xác nhận đổi mật khẩu thành công
            return RedirectToAction(nameof(DoiMatKhau));
        }

        [HttpGet]
        public async Task<IActionResult> ExportBySalaryID(int? id = null)
        {
            if (!id.HasValue)
            {
                ModelState.AddModelError("", "Vui lòng chọn ngày để xuất file lương");
                return View();
            }


            // Lấy dữ liệu bảng lương theo startDate và endDate
            var salary = await _context.salaries
                .Where(s => s.Salary_Id == id)
                .Include(s => s.Employee)
                .FirstOrDefaultAsync();

            if (salary == null)
            {
                ModelState.AddModelError("", "Không có dữ liệu lương.");
                return View();
            }
            DateTime startDate = salary.Begin_Date;
            DateTime endDate = salary.End_Date;


            var bonuses = await _context.bonuses
                .Where(b => b.Employee_Id == salary.Employee_Id && b.Bonus_Date >= startDate && b.Bonus_Date <= endDate)
                .ToListAsync();

            var deductions = await _context.deductions
                .Where(d => d.Employee_Id == salary.Employee_Id && d.Deduction_Date >= startDate && d.Deduction_Date <= endDate)
                .ToListAsync();

            //Tính số ngày nhân viên có đi làm trong tháng
            var workedDays = await _context.attendances.Where(a => a.Employee_Id == salary.Employee_Id
                && a.Attendance_Date >= salary.Begin_Date && a.Attendance_Date <= salary.End_Date
                && (a.status_id == 1 || a.status_id == 2))
                .CountAsync();

            // Tổng số ngày cần tính lương(số ngày phải đi làm trong tháng trừ chủ nhật)
            int totalDays = CalculateWorkingDays(salary.Begin_Date, salary.End_Date);

            // Tạo nội dung file cho mỗi nhân viên
            StringBuilder content = new StringBuilder();
            content.AppendLine("Thông tin lương");
            content.AppendLine("----------------------");
            content.AppendLine($"Mã lương: {salary.Salary_Id}");
            content.AppendLine($"Mã nhân viên: {salary.Employee_Id}");
            content.AppendLine($"Tên nhân viên: {salary.Employee.first_name} {salary.Employee.last_name}");
            content.AppendLine($"Từ ngày: {salary.Begin_Date:dd/MM/yyyy}");
            content.AppendLine($"Đến ngày: {salary.End_Date:dd/MM/yyyy}");
            content.AppendLine($"Lương cơ bản: {salary.Base_Salary:C0}");
            content.AppendLine();

            content.AppendLine("Tiền Thưởng");
            content.AppendLine("----------------------");
            if (bonuses.Any())
            {
                for (int i = 0; i < bonuses.Count; i++)
                {
                    content.AppendLine($"Thưởng {i + 1}:");
                    content.AppendLine($"- Tiền: {bonuses[i].Bonus_Amount:C0}");
                    content.AppendLine($"- Ngày: {bonuses[i].Bonus_Date:dd/MM/yyyy}");
                    content.AppendLine($"- Lý do: {bonuses[i].Reason}");
                    content.AppendLine();
                }
            }
            else
            {
                content.AppendLine("Không có thưởng.");
                content.AppendLine();
            }

            content.AppendLine("Khấu trừ");
            content.AppendLine("----------------------");
            if (deductions.Any())
            {
                for (int i = 0; i < deductions.Count; i++)
                {
                    content.AppendLine($"Khấu trừ {i + 1}:");
                    content.AppendLine($"- Tiền: {deductions[i].Deduction_Amount:C0}");
                    content.AppendLine($"- Ngày: {deductions[i].Deduction_Date:dd/MM/yyyy}");
                    content.AppendLine($"- Lý do: {deductions[i].Reason}");
                    content.AppendLine();
                }
            }
            else
            {
                content.AppendLine("Không có khấu trừ.");
                content.AppendLine();
            }

            content.AppendLine("Tổng kết");
            content.AppendLine("----------------------");
            content.AppendLine($"Số ngày làm: {workedDays}/{totalDays}");
            content.AppendLine($"Thưởng: {salary.Bonus:C0}");
            content.AppendLine($"Khấu trừ: {salary.Deduction:C0}");
            content.AppendLine($"Tổng lương: {salary.Total_Salary:C0}");
            content.AppendLine();
            content.AppendLine("==================================================");
            content.AppendLine();

            // Trả về file txt
            byte[] fileBytes = Encoding.UTF8.GetBytes(content.ToString());
            string fileName = $"BaoCaoLuong_{salary.Employee_Id}_{salary.Begin_Date:yyyyMMdd}_{salary.End_Date:yyyyMMdd}.txt";

            return File(fileBytes, "text/plain", fileName);
        }

        private EmployeesModel KiemTraDangNhap()
        {
            // Lấy cookie EmployeeData nếu tồn tại
            if (!Request.Cookies.TryGetValue("EmployeeData", out var employeeDataJson) || string.IsNullOrEmpty(employeeDataJson))
            {
                return null; // Cookie không tồn tại hoặc rỗng, trả về null
            }

            EmployeesModel employee;
            try
            {
                // Chuyển đổi chuỗi JSON thành đối tượng Employee
                employee = JsonSerializer.Deserialize<EmployeesModel>(employeeDataJson);
            }
            catch (JsonException)
            {
                // Nếu lỗi khi deserialize, trả về null
                return null;
            }

            // Nếu employee hoặc employee_id không hợp lệ, trả về null
            if (employee == null)
            {
                return null;
            }

            return employee; // Trả về employee nếu thành công
        }
        //Tính toán số ngày cần tính lương ngoại trừ Chủ Nhật
        static int CalculateWorkingDays(DateTime start, DateTime end)
        {
            int totalDays = 0;

            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                // Kiểm tra nếu ngày hiện tại không phải là Chủ Nhật
                if (date.DayOfWeek != DayOfWeek.Sunday)
                {
                    totalDays++;
                }
            }

            return totalDays;
        }

    }
}

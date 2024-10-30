using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Helpers;
using QuanLyNhanSu.Models;
using QuanLyNhanSu.ViewModels;
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
        // GET: Details
        public ActionResult Index()
        {
            return View();
        }

        // GET: Details/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Details/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        //Xem chi tiết tiền thưởng theo ID Nhân sự đăng nhập
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

    }
}

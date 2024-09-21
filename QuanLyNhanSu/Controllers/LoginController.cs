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
    public class LoginController : Controller
    {
        private QuanLyNhanSuDbContext _context;
        public LoginController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        // GET: LoginController
        public ActionResult Index()
        {
            if (Request.Cookies["EmployeeData"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // GET: LoginController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LoginController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LoginController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            // Kiểm tra xem đầu vào là employee_id hay username
            var isNumeric = int.TryParse(model.UsernameOrEmployeeId, out int employeeId);
            // Truy vấn người dùng từ bảng login
            Login login;
            if (isNumeric)
            {
                login = await _context.login.FirstOrDefaultAsync(l => l.employee_id == employeeId);
            }
            else
            {
                login = await _context.login.FirstOrDefaultAsync(l => l.username == model.UsernameOrEmployeeId);
            }
            if (login != null && PasswordHasher.VerifyPassword(model.Password, login.hashed_password))
            {
                EmployeesModel employees = await _context.employees.FirstOrDefaultAsync(e => e.employee_id == login.employee_id);
                // So sánh hashed password
                if(employees != null)
                {
                    // Chuyển đối tượng Employee thành chuỗi JSON
                    var employeeDataJson = JsonSerializer.Serialize(employees);
                    // Lưu chuỗi JSON vào cookies
                    CookieOptions cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTimeOffset.UtcNow.AddHours(3) // Thời gian hết hạn cookie (1 giờ)
                    };
                    Response.Cookies.Append("EmployeeData", employeeDataJson, cookieOptions);
                    // Redirect sau khi đăng nhập thành công
                    return RedirectToAction("Index", "Home");
                }
                
            }
            // Nếu không thành công, trả về view với thông báo lỗi
            ModelState.AddModelError("", "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.");
            return View(model);
        }
        // Action để đăng xuất
        [HttpPost]
        public IActionResult Logout()
        {
            // Xóa cookie EmployeeData
            Response.Cookies.Delete("EmployeeData");

            // Chuyển hướng về trang đăng nhập hoặc trang chủ
            return RedirectToAction("Index", "Home");
        }
        // GET: LoginController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LoginController/Edit/5
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

        // GET: LoginController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LoginController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
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
    }
}

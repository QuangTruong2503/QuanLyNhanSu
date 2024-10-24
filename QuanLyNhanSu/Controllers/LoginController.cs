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
        // GET: LoginController
        public IActionResult Login()
        {
            if (Request.Cookies["EmployeeData"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        // POST: LoginController/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            // Truy vấn người dùng từ bảng login (giả sử đã tạo chỉ mục)
            var login = await _context.employees
                .FirstOrDefaultAsync(l =>
                    (l.employee_id == model.EmployeeIDorPhone) ||
                    (l.phone == model.EmployeeIDorPhone));
            if (login != null && PasswordHasher.VerifyPassword(model.Password, login.hashed_password))
            {

                // Chuyển đối tượng Employee thành chuỗi JSON
                var employeeDataJson = JsonSerializer.Serialize(login);
                // Lưu chuỗi JSON vào cookies
                CookieOptions cookieOptions = new()
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddHours(3) // Thời gian hết hạn cookie (3 giờ)
                };
                Response.Cookies.Append("EmployeeData", employeeDataJson, cookieOptions);
                // Redirect sau khi đăng nhập thành công
                return RedirectToAction("Index", "Home");
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

        public ActionResult ChangePassword()
        {
            return View();
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

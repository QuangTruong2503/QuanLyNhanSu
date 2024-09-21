using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Helpers;
using QuanLyNhanSu.Models;
using QuanLyNhanSu.ViewModels;

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
        public async Task<IActionResult> Index(LoginViewModel model)
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
            if (login != null)
            {
                // So sánh hashed password
                if(PasswordHasher.VerifyPassword(model.Password, login.hashed_password))
                {
                    // Đăng nhập thành công
                    return RedirectToAction("Index", "Home");
                }
                
            }
            // Nếu không thành công, trả về view với thông báo lỗi
            ModelState.AddModelError("", "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.");
            return View(model);
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

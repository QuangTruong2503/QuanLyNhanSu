using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;
using System.Text.Json;

namespace QuanLyNhanSu.Controllers
{
    public class AdminController : Controller
    {
        private readonly QuanLyNhanSuDbContext _context;
        public AdminController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        // GET: AdminController
        public async Task<IActionResult> Index()
        {
            if (!Request.Cookies.TryGetValue("EmployeeData", out var employeeDataJson) || string.IsNullOrEmpty(employeeDataJson))
            {
                return RedirectToAction("Index", "Home");
            }

            // Deserialize chuỗi JSON thành đối tượng Employee
            var employee = JsonSerializer.Deserialize<EmployeesModel>(employeeDataJson);

            // Nếu không có employee, chuyển hướng về trang Home
            if (employee == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin employee từ cơ sở dữ liệu
            var employeeData = await _context.employees.FirstOrDefaultAsync(m => m.employee_id == employee.employee_id);

            // Kiểm tra role_id và chuyển hướng phù hợp
            if (employeeData != null && employee.role_id == employeeData.role_id && employee.role_id == 2)
            {
                return View();
            }

            return RedirectToAction("Index", "Home");
        }


        // GET: AdminController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AdminController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AdminController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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

        // GET: AdminController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AdminController/Edit/5
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

        // GET: AdminController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AdminController/Delete/5
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

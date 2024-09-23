using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Controllers
{
    public class EmployeesController : Controller
    {

        private QuanLyNhanSuDbContext _context;
        public EmployeesController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        // GET: EmployeesController
        public async Task<IActionResult> Index()
        {
            var employees = await _context.employees.ToListAsync();
            return View(employees);
        }

        // GET: EmployeesController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EmployeesController/Create
        public async Task<IActionResult> DangKyNhanVien()
        {
            var departments = await _context.departments.ToListAsync();
			// Tạo danh sách SelectListItem để hiển thị trong dropdown
			ViewBag.Departments = new SelectList(departments, "department_id", "department_name");
            return View();
        }

        // POST: EmployeesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKyNhanVien(EmployeesModel employees)
        {
            try
            {
                if (employees == null)
                {
                    return BadRequest("Employee model is null");
                }

                if (ModelState.IsValid)
                {
                    employees.hire_date = DateOnly.MinValue;
                    _context.Add(employees);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: EmployeesController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: EmployeesController/Edit/5
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

        // GET: EmployeesController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EmployeesController/Delete/5
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

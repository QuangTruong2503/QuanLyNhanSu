using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Helpers;
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
            var employees = await _context.employees
                                  .Include(e => e.Role)       // Include Role
                                  .Include(e => e.departments) // Include Department
                                  .ToListAsync();
            return View(employees);
        }

        // GET: EmployeesController/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var details = await _context.employees
                .Include(e => e.Role)
                .Include(e => e.departments)
                .FirstOrDefaultAsync(m => m.employee_id == id);
            if (details != null)
            {
                return View(details);
            }
            return View();
        }
        // Action để lấy danh sách vị trí (position) theo department_id
        [HttpGet]
        public JsonResult GetPositionsByDepartmentId(int departmentId)
        {
            // Giả sử bạn có một bảng Positions trong database, liên kết với Department
            var positions = _context.positions
                                    .Where(p => p.department_id == departmentId)
                                    .Select(p => new SelectListItem
                                    {
                                        Value = p.position_id.ToString(),
                                        Text = p.position_name,
                                    })
                                    .ToList();

            // Trả về danh sách position dưới dạng JSON
            return Json(positions);
        }
        // GET: EmployeesController/Create
        [HttpGet]
        public async Task<IActionResult> DangKyNhanVien()
        {
            var departments = await _context.departments.ToListAsync();
			// Tạo danh sách SelectListItem để hiển thị trong dropdown
			ViewBag.Departments = new SelectList(departments, "department_id", "department_name", "");
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
                    //Tạo mã nhân viên ngẫu nhiên
                    var randomCode = EmployeeHelpers.EmployeeCodeRandom();
                    employees.employee_id = randomCode;
                    //Thiết lập ngày bắt đầu làm việc
                    employees.hire_date = DateTime.Now;
                    //Thiết lập mật khẩu mặc định
                    var defaultPassword = "123456";
                    employees.hashed_password = PasswordHasher.HashPassword(defaultPassword);
                    _context.Add(employees);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(employees);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi lưu vào cơ sở dữ liệu
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi lưu dữ liệu: " + ex.Message);
            }
            // Nếu có lỗi, quay lại view với thông tin lỗi trong ModelState
            var departments = await _context.departments.ToListAsync();
            // Tạo danh sách SelectListItem để hiển thị trong dropdown
            ViewBag.Departments = new SelectList(departments, "department_id", "department_name");
            return View(employees);
        }

        // GET: EmployeesController/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var detail = await _context.employees.FindAsync(id);
            
            if (detail == null)
            {
                return NotFound();
            }
            var departments = await _context.departments.ToListAsync();
            // Tạo danh sách SelectListItem để hiển thị trong dropdown
            ViewBag.Departments = new SelectList(departments, "department_id", "department_name", detail.department_id);
            return View(detail);
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
        public async Task<IActionResult> Delete(string id)
        {
            var employee = await _context.employees.FindAsync(id);
            if (employee != null)
            {
                _context.employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
            
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

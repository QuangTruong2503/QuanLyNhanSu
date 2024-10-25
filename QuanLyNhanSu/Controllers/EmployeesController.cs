using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Helpers;
using QuanLyNhanSu.Models;
using QuanLyNhanSu.ViewModels;

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
                .Include(e => e.Positions)
                .FirstOrDefaultAsync(m => m.employee_id == id);
            if (details != null)
            {
                return View(details);
            }
            return View();
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
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra: " + ex.Message);
            }
            // Nếu có lỗi, quay lại view với thông tin lỗi trong ModelState
            return View(employees);
        }

        // GET: EmployeesController/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var detail = await _context.employees.FindAsync(id);
            ViewData["DepartmentID"] = detail.department_id;
            ViewData["PositionID"] = detail.position_id;
            ViewData["RoleID"] = detail.role_id;
            if (detail == null)
            {
                return NotFound();
            }
            return View(detail);
        }

        // POST: EmployeesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeesModel model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu dữ liệu không hợp lệ, trả về view kèm theo model để người dùng có thể thấy lỗi
                return View(model);
            }

            try
            {
                // Cập nhật thông tin nhân viên
                _context.employees.Update(model);
                await _context.SaveChangesAsync();

                // Chuyển hướng về trang Index sau khi cập nhật thành công
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Log lỗi để theo dõi (có thể dùng log framework như Serilog, NLog, hoặc log mặc định)
                // _logger.LogError(ex, "Lỗi khi cập nhật thông tin nhân viên");

                // Thêm thông tin lỗi vào ModelState để hiển thị cho người dùng
                ModelState.AddModelError("", $"{ex.Message}");

                // Trả về lại view kèm theo dữ liệu gốc để người dùng không mất thông tin
                return View(model);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung (ví dụ: log lỗi, gửi email cảnh báo...)
                // _logger.LogError(ex, "Lỗi không xác định.");

                ModelState.AddModelError("", $"{ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var employee = await _context.employees.FirstOrDefaultAsync(e => e.employee_id == id);
                if (employee != null)
                {
                    _context.employees.Remove(employee);
                    await _context.SaveChangesAsync();

                    // Sau khi xóa thành công, chuyển hướng về trang Index
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Index));
            }
            catch { return RedirectToAction(nameof(Index)); }
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
        //Action lấy dữ liệu department
        [HttpGet]
        public JsonResult GetDepartments()
        {
            var departments = _context.departments
                                    .Select(d => new SelectListItem
                                    {
                                        Value = d.department_id.ToString(),
                                        Text = d.department_name,
                                    })
                                    .ToList();

            return Json(departments);
        }
        //Action lấy dữ liệu role
        [HttpGet]
        public JsonResult GetRoles()
        {
            var roles = _context.role
                                    .Select(r => new SelectListItem
                                    {
                                        Value = r.Role_Id.ToString(),
                                        Text = r.Role_Name,
                                    })
                                    .ToList();

            return Json(roles);
        }
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var employee = await _context.employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            var model = new ResetPasswordViewModel
            {
                EmployeeId = employee.employee_id
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var employee = await _context.employees.FindAsync(model.EmployeeId);
                    if (employee == null)
                    {
                        return NotFound();
                    }
                    if (model.NewPassword != model.ConfirmPassword)
                    {
                        ModelState.AddModelError("", "Mật khẩu không trùng khớp.");
                        return View(model);
                    }
                    // Băm mật khẩu mới
                    employee.hashed_password = PasswordHasher.HashPassword(model.NewPassword);
                    // Cập nhật mật khẩu
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                    ViewData["SuccessMessage"] = $"Đổi mật khẩu tài khoản: {employee.employee_id} thành công.";
                    return View(); // Điều hướng sau khi thành công
                }
                return View(model);
            }
            catch (Exception ex) 
            {
                ModelState.AddModelError("", $"{ex.Message}");
                return View(model);
            }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;
using QuanLyNhanSu.ViewModels;
using System.Text.Json;

namespace QuanLyNhanSu.Controllers
{
    public class AttendencesController : Controller
    {
        private readonly QuanLyNhanSuDbContext _context;
        public AttendencesController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        // GET: AttendencesController
        public ActionResult Index()
        {
            return View();
        }

        // GET: AttendencesController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AttendencesController/Create
        public ActionResult ChamCong()
        {
            return View();
        }

        // POST: AttendencesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamCong(AttendanceModel model)
        {
            try
            {
                // Kiểm tra xem cookie EmployeeData có tồn tại không
                if (Request.Cookies.ContainsKey("EmployeeData"))
                {
                    var employeeDataJson = Request.Cookies["EmployeeData"];
                    var employee = new EmployeesModel();
                    if (employeeDataJson != null)
                    {
                        // Chuyển đổi chuỗi JSON thành đối tượng Employee
                        employee = JsonSerializer.Deserialize<EmployeesModel>(employeeDataJson);
                        var employeeID = employee.employee_id;

                        //Kiểm tra nhân sự đã chấm công hôm nay hay chưa
                        var existsEmployee = await _context.attendences.FirstOrDefaultAsync(m => m.Employee_Id == employeeID && m.Attendance_Date.Date == DateTime.Now.Date);
                        if(existsEmployee != null)
                        {
                            ModelState.AddModelError("", "Bạn đã chấm công hôm nay rồi!");
                            return View(model);
                        }

                        //Tiến hành chấm công
                        DateTime currentDateTime = DateTime.Now;
                        TimeSpan timeCheck = new TimeSpan(7, 0, 0); // 7:00 AM
                        // Kiểm tra Status dựa trên giờ hiện tại
                        int status;
                        if (currentDateTime.TimeOfDay > timeCheck.Add(TimeSpan.FromHours(1)))
                        {
                            status = 2; // Trễ hơn 1 giờ sau 7h sáng
                        }
                        else
                        {
                            status = 1; // Đúng giờ
                        }
                        // Tạo mới Attendance
                        AttendanceModel attendance = new AttendanceModel
                        {
                            Employee_Id = employeeID,
                            Attendance_Date = currentDateTime,
                            status_id = status
                        };
                        _context.attendences.Add(attendance);
                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Chấm công thành công!";
                        return RedirectToAction("ChamCong");
                    }

                }
                return RedirectToAction("Login", "Login");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                return View(model);
            }
        }

        // GET: AttendencesController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AttendencesController/Edit/5
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

        // GET: AttendencesController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AttendencesController/Delete/5
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

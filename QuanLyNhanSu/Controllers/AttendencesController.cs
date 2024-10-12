using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;
using QuanLyNhanSu.ViewModels;
using System.Text.Json;

namespace QuanLyNhanSu.Controllers
{
    public class AttendancesController : Controller
    {
        private readonly QuanLyNhanSuDbContext _context;
        public AttendancesController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        // GET: AttendancesController
        public async Task<IActionResult> DanhSach(string? selectedDate = null)
        {
            DateTime selectDateTime;
            if (string.IsNullOrEmpty(selectedDate))
            {
                // Nếu không có ngày nào được chọn, mặc định là ngày hôm nay
                selectDateTime = DateTime.Today;
            }
            else
            {
                // Nếu có ngày được chọn, chuyển đổi sang định dạng DateTime
                selectDateTime = DateTime.Parse(selectedDate);
            }
            var attendancesList = await _context.attendances.Where(m => m.Attendance_Date.Date == selectDateTime.Date)
               .Include(a => a.Employee)
               .Include(a => a.AttendanceStatus).ToListAsync();
            // Truyền danh sách và ngày đã chọn vào View
            ViewBag.SelectedDate = selectDateTime.ToString("dd/MM/yyyy");
            return View(attendancesList);
        }

        // GET: AttendancesController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AttendancesController/Create
        public ActionResult ChamCong()
        {
            TempData["DateNow"] = DateTime.Now.ToString("dd-MM-yyyy");
            return View();
        }

        // POST: AttendancesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamCongVao(AttendanceModel model)
        {
            try
            {
                var employeeID = model.Employee_Id;
                //Kiểm tra tính hợp lệ của employeeID
                var employeeData = await _context.employees.FirstOrDefaultAsync(m => m.employee_id == employeeID);
                if (employeeData == null)
                {
                    ModelState.AddModelError("", "Mã nhân viên không hợp lệ!");
                    return View("ChamCong", model);
                }
                //Kiểm tra nhân sự đã chấm công hôm nay hay chưa
                var existsEmployee = await _context.attendances.FirstOrDefaultAsync(m => m.Employee_Id == employeeID && m.Attendance_Date.Date == DateTime.Now.Date);
                if (existsEmployee != null)
                {
                    ModelState.AddModelError("", $"Nhân viên {employeeID} đã chấm công hôm nay!");
                    return View("ChamCong", model);
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
                _context.attendances.Add(attendance);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Chấm công thành công!";
                return View("ChamCong", model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                return View("ChamCong", model);
            }
        }

        //Chấm công ra
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamCongRa(AttendanceModel model)
        {
            try
            {
                var employeeID = model.Employee_Id;
                //Giá trị trạng thái chấm công "Rời"(có thể thay đổi theo database)
                var checkOutStatusID = 3;
                //Kiểm tra tính hợp lệ của employeeID
                var employeeData = await _context.employees.FirstOrDefaultAsync(m => m.employee_id == employeeID);
                if (employeeData == null)
                {
                    ModelState.AddModelError("", "Mã nhân viên không hợp lệ!");
                    return View("ChamCong", model);
                }
                //Kiểm tra nhân sự đã chấm công hôm nay hay chưa
                var existsEmployee = await _context.attendances.FirstOrDefaultAsync(m => m.Employee_Id == employeeID && m.Attendance_Date.Date == DateTime.Now.Date);
                if (existsEmployee == null)
                {
                    ModelState.AddModelError("", $"Nhân viên {employeeID} chưa chấm công vào!");
                    return View("ChamCong", model);
                }

                //Kiểm tra nhân sự đã chấm công ra hay chưa
                var checkedOutEmployee = await _context.attendances.FirstOrDefaultAsync(m => m.Employee_Id == employeeID && m.Attendance_Date.Date == DateTime.Now.Date && m.status_id == checkOutStatusID);
                if (checkedOutEmployee != null)
                {
                    ModelState.AddModelError("", $"Nhân viên {employeeID} đã chấm công ra!");
                    return View("ChamCong", model);
                }
                //Tiến hành chấm công
                DateTime currentDateTime = DateTime.Now;
                
                // Tạo mới Attendance
                AttendanceModel attendance = new AttendanceModel
                {
                    Employee_Id = employeeID,
                    Attendance_Date = currentDateTime,
                    status_id = checkOutStatusID
                };
                _context.attendances.Add(attendance);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Chấm công ra thành công!";
                return RedirectToAction("ChamCong");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                return RedirectToAction("ChamCong");
            }
        }

        //Kết thúc chấm công
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KetThucChamCong()
        {
            // Lấy ngày hiện tại
            DateTime today = DateTime.Today;
            // Lấy tất cả employee_id từ bảng employees
            var allEmployees = await _context.employees.Where(m => m.employee_id != "admin").Select(m => m.employee_id).ToListAsync();

            // Lấy tất cả employee_id đã chấm công trong ngày hôm nay
            var employeeCheckedIn = await _context.attendances
                .Where(m => m.Attendance_Date.Date == today.Date)
                .Select(m => m.Employee_Id)
                .ToListAsync();

            // So sánh để tìm những employee_id chưa chấm công
            var employeeNotCheckedIn = allEmployees.Except(employeeCheckedIn).ToList();

            // Đánh dấu là vắng (status = 4) cho những nhân viên chưa chấm công
            foreach(var employeeId in employeeNotCheckedIn)
            {
                AttendanceModel attendance = new AttendanceModel
                {
                    Employee_Id = employeeId,
                    Attendance_Date = today,
                    status_id = 4
                };
                _context.attendances.Add(attendance);
            }
            await _context.SaveChangesAsync();
            TempData["Message"] = "Kết thúc chấm công, nhân viên chưa chấm công đã được ghi vắng!";
            return RedirectToAction("ChamCong");
        }
        // GET: AttendancesController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AttendancesController/Edit/5
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

        //Lấy dữ liệu Attendance_status
        [HttpGet]
        public JsonResult GetAttendanceStatus()
        {
            var status = _context.attendance_status.Select(a => new SelectListItem
            {
                Value = a.status_id.ToString(),
                Text = a.status_name
            }).ToList();
            return Json(status);
        }
    }
}

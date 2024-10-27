using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Helpers;
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

        public ActionResult DanhSach()
        {
            return View(new List<AttendanceViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> DanhSach(string? attendanceDate = null)
        {
            DateTime selectedDateTime;
            if (string.IsNullOrEmpty(attendanceDate))
            {
                selectedDateTime = DateTime.Today;
               ViewData["AttendanceDate"] = DateTime.Today.ToString("dd-MM-yyyy");
            }
            else
            {
                selectedDateTime = DateTime.Parse(attendanceDate);
                ViewData["AttendanceDate"] = selectedDateTime.ToString("dd-MM-yyyy");
            }
            var employees = await _context.employees.Where(e => e.employee_id != "admin" && e.expired_date == null ).ToListAsync();
            var attendances = await _context.attendances.Where(a => a.Attendance_Date.Date == selectedDateTime.Date).ToListAsync();

            var viewModel = employees.Select(e => {
                var attendance = attendances.FirstOrDefault(a => a.Employee_Id == e.employee_id);
                return new AttendanceViewModel
                {
                    EmployeeId = e.employee_id,
                    FullName = e.first_name + " " + e.last_name,
                    IsPresent = attendance != null && attendance.status_id == 1,
                    IsLate = attendance != null && attendance.status_id == 2,
                    IsAbsent = attendance != null && attendance.status_id == 4,
                    HasAttendanceData = attendance != null
                };
            }).ToList();

            return View(viewModel);
        }

        //Lưu thay đổi chấm công trong danh sách
        [HttpPost]
        public async Task<IActionResult> SaveAttendance(List<AttendanceViewModel> model, DateTime attendanceDate)
        {
            try
            {
                foreach (var item in model)
                {
                    // Xác định trạng thái chấm công của nhân sự
                    var status_id = item.IsPresent ? 1 : item.IsLate ? 2 : item.IsAbsent ? 4 : (int?) null;

                    // Kiểm tra xem bản ghi đã tồn tại hay chưa
                    var existingRecord = await _context.attendances
                        .FirstOrDefaultAsync(a => a.Employee_Id == item.EmployeeId && a.Attendance_Date.Date == attendanceDate.Date);

                    if (existingRecord != null && status_id == null)
                    {
                        // Nếu có bản ghi, và trạng thái là null thì xóa bản ghi
                        _context.attendances .Remove(existingRecord);
                    }
                    else if (existingRecord != null && status_id != null)
                    {
                        // Nếu có bản ghi và có trạng thái chấm công thì cập nhật
                        existingRecord.status_id = (int)status_id;
                    }
                    else if(existingRecord == null && status_id != null)
                    {
                        // Nếu không có và có trạng thái chấm công thì thêm mới
                        _context.attendances.Add(new AttendanceModel
                        {
                            Employee_Id = item.EmployeeId,
                            Attendance_Date = attendanceDate,
                            status_id = (int)status_id
                        });
                    }
                    else
                    {
                        continue;
                    }
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                // Lấy lại danh sách nhân viên và trạng thái chấm công để hiển thị trong view
                var employees = await _context.employees.Where(e => e.employee_id != "admin" && e.expired_date == null).ToListAsync();
                var attendances = await _context.attendances.Where(a => a.Attendance_Date.Date == attendanceDate.Date).ToListAsync();

                var viewModel = employees.Select(e => {
                    var attendance = attendances.FirstOrDefault(a => a.Employee_Id == e.employee_id);
                    return new AttendanceViewModel
                    {
                        EmployeeId = e.employee_id,
                        FullName = e.first_name + " " + e.last_name,
                        IsPresent = attendance != null && attendance.status_id == 1,
                        IsLate = attendance != null && attendance.status_id == 2,
                        IsAbsent = attendance != null && attendance.status_id == 4,
                        HasAttendanceData = attendance != null
                    };
                }).ToList();

                // Đặt giá trị cho ViewData["AttendanceDate"] để truyền ngày về view
                ViewData["AttendanceDate"] = attendanceDate.ToString("dd-MM-yyyy");

                // Trả về view "DanhSach" cùng với model và ngày đã chọn
                return View("DanhSach", viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Nếu có lỗi, trả về view "DanhSach" cùng với ngày đã chọn để người dùng thử lại
                ViewData["AttendanceDate"] = attendanceDate.ToString("dd-MM-yyyy");
                return View("DanhSach", model);
            }
        }

        public ActionResult ChamCong()
        {
            return View();
        }

        //Bắt đầu chấm công
        [HttpPost]
        public ActionResult BatDauChamCong()
        {
            DateTime today = DateTime.Today;
            var token = GenerateAttendanceToken();
            ViewBag.ToDay = today.ToString("dd-MM-yyyy");
            ViewBag.VerifyCode = token.Result.Verify_Code;
            ViewBag.Expiration = token.Result.Expiration.ToString("yyyy-MM-dd HH:mm:ss");
            return View("ChamCong");
        }
        private async Task<TokenAttendanceModel> GenerateAttendanceToken()
        {
            var codeOTP = GetVerifyRandom.GenerateOTP();
            var token = new TokenAttendanceModel
            {
                Verify_Code = codeOTP,
                Expiration = DateTime.Now.AddMinutes(2) //Hạn mã OTP là 2phut
            };
            //Làm sạch token cũ
            _context.token_attendance.RemoveRange(_context.token_attendance);
            // Lưu token vào database
            _context.token_attendance.Add(token);
            await _context.SaveChangesAsync();
            return token;

        }
        //Làm mới token sau khi hết hạn
        [HttpPost]
        public ActionResult RefreshAttendanceToken()
        {
            var token = GenerateAttendanceToken();
            return Json(new
            {
                Code = token.Result.Verify_Code,
                Expiration = token.Result.Expiration.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        //Xác nhận chấm công của nhân viên
        [HttpPost]
        public async Task<IActionResult> XacNhanChamCong(string verifyCode)
        {
            var employee = KiemTraDangNhap();
            if (employee == null)
            {
                return RedirectToAction("Login", "Login");
            }
            
            try
            {
                var token = await _context.token_attendance.FirstOrDefaultAsync();
                if(token != null && token.Verify_Code == verifyCode && token.Expiration > DateTime.UtcNow)
                {
                    var today = DateTime.Today;
                    var existsAttendance = await _context.attendances.FirstOrDefaultAsync(a => a.Employee_Id == employee.employee_id && a.Attendance_Date.Date == today.Date);
                    //Kiểm tra nhân viên đã chấm công hay chưa
                    if(existsAttendance != null)
                    {
                        TempData["Message"] = "Bạn đã chấm công hôm nay rồi!";
                        return View("ChamCong");
                    }
                    var attendance = new AttendanceModel
                    {
                        Employee_Id = employee.employee_id,
                        Attendance_Date = DateTime.Now,
                        //Nếu giờ hiện tại lớn hơn 8AM thì status_id = 2 (Đi trễ)
                        status_id = DateTime.Now.TimeOfDay >= new TimeSpan(8, 0, 0) ? 2 : 1
                    };
                    _context.attendances.Add(attendance);
                    await _context.SaveChangesAsync();
                    return View("ChamCong");
                }
                return View("ChamCong");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                return View("ChamCong");
            }
        }

        //Kết thúc chấm công
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KetThucChamCong()
        {
            return RedirectToAction("ChamCong");
        }

        //Kiểm tra đăng nhập trong Cookies
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

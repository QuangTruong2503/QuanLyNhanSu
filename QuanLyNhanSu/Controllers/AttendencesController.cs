using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Helpers;
using QuanLyNhanSu.Models;
using QuanLyNhanSu.ViewModels;
using System.Drawing.Imaging;
using System.Text.Json;
using System.IO;
using System.Drawing;

namespace QuanLyNhanSu.Controllers
{
    public class AttendancesController : Controller
    {
        private readonly QuanLyNhanSuDbContext _context;
        public AttendancesController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        public async Task<IActionResult> DanhSach()
        {
            DateTime selectedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            ViewData["AttendanceDate"] = selectedDateTime.ToString("dd-MM-yyyy");
            var employees = await _context.employees.Where(e => e.employee_id != "admin" && e.expired_date == null).ToListAsync();
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
        //Cập nhật dữ liệu chấm công của nhân viên
        [HttpPost]
        public async Task<IActionResult> DanhSach(string? attendanceDate = null)
        {
            DateTime selectedDateTime;
            if (string.IsNullOrEmpty(attendanceDate))
            {
                selectedDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
               ViewData["AttendanceDate"] = selectedDateTime.ToString("dd-MM-yyyy");
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

        //Hiển thị chức năng chấm công với từng nhân viên
        public ActionResult NhanVienChamCong()
        {
            return View();
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
            DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            ViewBag.ToDay = today.ToString("dd-MM-yyyy");
            return View();
        }

        //Bắt đầu chấm công, hiển thị mã để nhân viên nhập
        [HttpPost]
        public ActionResult BatDauChamCong()
        {
            DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var token = GenerateAttendanceToken();
            ViewBag.ToDay = today.ToString("dd-MM-yyyy");
            ViewBag.VerifyCode = token.Result.Verify_Code;
            ViewBag.Expiration = token.Result.Expiration.ToString("yyyy-MM-dd HH:mm:ss");
            return View("ChamCong");
        }
        //Tạo mã OTP với thời hạn để chấm công
        private async Task<TokenAttendanceModel> GenerateAttendanceToken()
        {
            var codeOTP = GetVerifyRandom.GenerateOTP();
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var token = new TokenAttendanceModel
            {
                Verify_Code = codeOTP,
                Expiration = dateTime.AddMinutes(2) //Hạn mã OTP là 2phut
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


        [HttpGet]
        public ActionResult ChamCongQRCode(string verifyCode)
        {
            ViewBag.VerifyCode = verifyCode;
            return View();
        }

        //Xác nhận chấm công của nhân viên khi nhập mã OTP
        [HttpPost, HttpGet]
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
                //Nếu mã OTP hợp lệ và còn thời hạn
                if(token != null && token.Verify_Code == verifyCode && token.Expiration > DateTime.Now)
                {
                    var today = DateTime.Today;
                    var existsAttendance = await _context.attendances.FirstOrDefaultAsync(a => a.Employee_Id == employee.employee_id && a.Attendance_Date.Date == today.Date);
                    //Kiểm tra nhân viên đã chấm công hay chưa
                    if(existsAttendance != null)
                    {
                        TempData["MessageError"] = "Bạn đã chấm công hôm nay rồi!";
                        return RedirectToAction(nameof(NhanVienChamCong));
                    }
                    var attendance = new AttendanceModel
                    {
                        Employee_Id = employee.employee_id,
                        Attendance_Date = DateTime.Now,
                        //Nếu giờ hiện tại lớn hơn 8AM thì status_id = 2 (Đi trễ)
                        status_id = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).TimeOfDay >= new TimeSpan(8, 0, 0) ? 2 : 1
                    };
                    _context.attendances.Add(attendance);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Chấm công thành công!";
                    return RedirectToAction(nameof(NhanVienChamCong));
                }
                TempData["MessageError"] = "Mã OTP không hợp lệ !";
                return RedirectToAction(nameof(NhanVienChamCong));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                return RedirectToAction(nameof(NhanVienChamCong));
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
            foreach (var employeeId in employeeNotCheckedIn)
            {
                AttendanceModel attendance = new AttendanceModel
                {
                    Employee_Id = employeeId,
                    Attendance_Date = today,
                    status_id = 4
                };
                //Thêm vào bảng Attendance
                _context.attendances.Add(attendance);
                await _context.SaveChangesAsync();
            }
            //Lấy dữ liệu chấm công ngày hôm nay
            var attendancesList = await _context.attendances.Where(a => a.Attendance_Date == today).ToListAsync();

            foreach (var attendance in attendancesList)
            {
                //Nếu nhân sự đi trễ
                if (attendance.status_id == 2)
                {
                    DeductionModel deduction = new DeductionModel()
                    {
                        Employee_Id = attendance.Employee_Id,
                        Deduction_Amount = 50000,
                        Deduction_Date = attendance.Attendance_Date,
                        Reason = "Đi trễ"
                    };
                    _context.deductions.Add(deduction);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("DanhSach");
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

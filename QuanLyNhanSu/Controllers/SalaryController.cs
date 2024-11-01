using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Helpers;
using QuanLyNhanSu.Models;
using System.IO.Compression;
using System.Text;

namespace QuanLyNhanSu.Controllers
{
    public class SalaryController : Controller
    {
        private readonly QuanLyNhanSuDbContext _context;
        public SalaryController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        //Tạo time zone của Viet Nam
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        // GET: SalaryContronller
        public async Task<IActionResult> Index(string? employeeID = null, DateTime? monthYear = null, int page = 1, int pageSize = 8)
        {
            var salaryListQuery = _context.salaries.AsQueryable();

            // Lọc theo employeeID nếu có
            if (!string.IsNullOrEmpty(employeeID))
            {
                salaryListQuery = salaryListQuery.Where(s => s.Employee_Id == employeeID);
                ViewBag.EmployeeID = employeeID;
            }

            // Lọc theo tháng và năm nếu có
            if (monthYear.HasValue)
            {
                var month = monthYear.Value.Month;
                var year = monthYear.Value.Year;
                DateTime startDate;
                DateTime endDate;

                if (month != 1)
                {
                    startDate = new DateTime(year, month - 1, 11);
                    endDate = new DateTime(year, month, 10);
                }
                else
                {
                    startDate = new DateTime(year - 1, 12, 11);
                    endDate = new DateTime(year, month, 10);
                }
                salaryListQuery = salaryListQuery.Where(s => s.Begin_Date.Date >= startDate.Date && s.End_Date.Date <= endDate.Date);
                ViewBag.MonthYear = monthYear;
            }

            // Tổng số bản ghi sau khi lọc
            var totalRecords = await salaryListQuery.CountAsync();

            // Phân trang
            var salaryList = await salaryListQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tính tổng số trang dựa trên số bản ghi đã lọc
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(salaryList);
        }


        // GET: SalaryContronller/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: SalaryContronller/Create
        public ActionResult TinhLuong()
        {
            return View();
        }

        // POST: SalaryContronller/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TinhLuong(DateTime? monthYear = null)
        {
            if (!monthYear.HasValue)
            {
                ModelState.AddModelError("", "Không có dữ liệu tháng");
                return View();
            }
            var year = monthYear.Value.Year;
            var month = monthYear.Value.Month;
            DateTime startDate;
            DateTime endDate;
            if (month != 1)
            {
                //Ngày bắt đầu lấy từ ngày 11 tháng trước
                startDate = new DateTime(year, month - 1, 11);
                endDate = new DateTime(year, month, 10);
            }
            else
            {
                //Ngày bắt đầu lấy từ ngày 11 tháng trước
                startDate = new DateTime(year - 1, 12, 11);
                endDate = new DateTime(year, month, 10);
            }
            try
            {
                // Tổng số ngày cần tính lương(số ngày phải đi làm trong tháng trừ chủ nhật)
                int totalDays = CalculateWorkingDays(startDate, endDate);
                // Lấy danh sách nhân viên
                var employees = await _context.employees.Where(e => e.employee_id != "admin").ToListAsync();

                foreach (var employee in employees)
                {
                    //Lấy lương cơ bản dựa trên mỗi nhân viên
                    var baseSalary = await _context.positions
                        .Where(p => p.position_id == employee.position_id)
                        .Select(p => p.base_salary)
                        .FirstOrDefaultAsync();
                    //Nếu không có dữ liệu lương cơ bản bỏ qua nhân viên đó
                    if(baseSalary == null)
                    {
                        continue;
                    }

                    // Lấy tổng tiền thưởng của nhân viên từ bảng Bonus trong khoảng startDate đến endDate
                    var totalBonus = await _context.bonuses
                        .Where(e => e.Employee_Id == employee.employee_id && e.Bonus_Date >= startDate && e.Bonus_Date <= endDate)
                        .SumAsync(e => e.Bonus_Amount);

                    // Lấy tổng tiền khấu trừ của nhân viên từ bảng Bonus trong khoảng startDate đến endDate
                    var totalDeduction = await _context.deductions
                        .Where(e => e.Employee_Id == employee.employee_id && e.Deduction_Date >= startDate && e.Deduction_Date <= endDate)
                        .SumAsync(e => e.Deduction_Amount);

                    //Tổng số ngày đi làm từ startDate đến endDate
                    var workingDays = await _context.attendances
                        .Where(a => a.Employee_Id == employee.employee_id && a.Attendance_Date >= startDate 
                                && a.Attendance_Date <= endDate && (a.status_id == 1 || a.status_id == 2))
                        .CountAsync();

                    //Số tiền nhận được dựa trên ngày đi làm
                    var amountByWorkingDays = baseSalary / totalDays * workingDays;
                    //Tính tổng tiền thực nhận
                    var totalSalary = amountByWorkingDays + totalBonus - totalDeduction;

                    //Tạo bản ghi Salary mới
                    SalaryModel salary = new SalaryModel
                    {
                        Employee_Id = employee.employee_id,
                        Base_Salary = baseSalary,
                        Bonus = totalBonus,
                        Deduction = totalDeduction,
                        Total_Salary = totalSalary,
                        Begin_Date = startDate,
                        End_Date = endDate
                    };
                    //Thêm vào bảng Salary
                    _context.salaries.Add(salary);
                }
                //Lưu thay đổi vào database
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tính lương thành công";
                return RedirectToAction(nameof(TinhLuong));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("","Có lỗi xảy ra: " + ex.Message);
                return View();
            }
        }

        // GET: SalaryContronller/Edit/5
        public async Task<ActionResult> CapNhatLuongNhanSu(int id)
        {
            var salaryByID = await _context.salaries.FirstOrDefaultAsync(s => s.Salary_Id == id);
            if (salaryByID == null)
            {
                ModelState.AddModelError("", "Không tìm thấy lương của nhân viên này");
                return View();
            }
            //Lấy dữ liệu lương cơ bản của nhân viên
            var baseSalaryQuery = from salary in _context.salaries
                                  join emp in _context.employees on salary.Employee_Id equals emp.employee_id
                                  join pos in _context.positions on emp.position_id equals pos.position_id
                                  where emp.employee_id == salaryByID.Employee_Id
                                  select pos.base_salary;
            var baseSalary = await baseSalaryQuery.FirstOrDefaultAsync();
            //Lấy dữ liệu thưởng của nhân viên dựa theo ngày bắt đầu tính lương và kết thúc tính lương
            var bonus = await _context.bonuses
                .Where(b => b.Employee_Id == salaryByID.Employee_Id && b.Bonus_Date >= salaryByID.Begin_Date 
                && b.Bonus_Date <= salaryByID.End_Date)
                .ToListAsync();

            //Lấy dữ liệu phạt của nhân viên dựa theo ngày bắt đầu tính lương và kết thúc tính lương
            var deduction = await _context.deductions
                .Where(d => d.Employee_Id == salaryByID.Employee_Id && d.Deduction_Date >= salaryByID.Begin_Date
                && d.Deduction_Date <= salaryByID.End_Date)
                .ToListAsync();
            //Tính tổng số ngày đi làm theo nhân viên từ ngày bắt đầu tính lương đến ngày kết thúc tính lương
            var workingDays = await _context.attendances.Where(a => a.Employee_Id == salaryByID.Employee_Id
                && a.Attendance_Date >= salaryByID.Begin_Date && a.Attendance_Date <= salaryByID.End_Date
                && (a.status_id == 1 || a.status_id == 2))
                .CountAsync();

            // Tổng số ngày cần tính lương(số ngày phải đi làm trong tháng trừ chủ nhật)
            int totalDays = CalculateWorkingDays(salaryByID.Begin_Date, salaryByID.End_Date);

            //Số tiền nhận được dựa trên ngày đi làm
            var amountByWorkingDays = baseSalary / totalDays * workingDays;
            //Truyền dữ liệu vào ViewData
            ViewData["Data"] = salaryByID;
            ViewData["Bonuses"] = bonus;
            ViewData["Deductions"] = deduction;
            ViewData["WorkingDays"] = workingDays;
            ViewData["AmountWorkingDays"] = FormatHelpers.FormatCurrencyVND(amountByWorkingDays);
            ViewData["TotalWorkingDays"] = totalDays;
            return View(salaryByID);
        }

        // POST: SalaryContronller/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatLuongNhanSu(int id, IFormCollection collection)
        {
            var salaryByID = await _context.salaries.FirstOrDefaultAsync(s => s.Salary_Id == id);
            if (salaryByID == null)
            {
                ModelState.AddModelError("", "Không tìm thấy lương của nhân viên này");
                return View();
            }
            try
            {
                //Lấy tổng tiền thưởng theo nhân viên từ ngày bắt đầu tính lương đến ngày kết thúc tính lương
                var totalBonuses = await _context.bonuses
                    .Where(b => b.Employee_Id == salaryByID.Employee_Id && b.Bonus_Date >= salaryByID.Begin_Date 
                    && b.Bonus_Date <= salaryByID.End_Date)
                    .SumAsync(b => b.Bonus_Amount);
                //Lấy tổng tiền phạt theo nhân viên từ ngày bắt đầu tính lương đến ngày kết thúc tính lương
                var totalDeductions = await _context.deductions
                    .Where(d => d.Employee_Id == salaryByID.Employee_Id 
                    && d.Deduction_Date >= salaryByID.Begin_Date && d.Deduction_Date <= salaryByID.End_Date)
                    .SumAsync(d => d.Deduction_Amount);
                //Tính tổng số ngày đi làm theo nhân viên từ ngày bắt đầu tính lương đến ngày kết thúc tính lương
                var workingDays = await _context.attendances.Where(a => a.Employee_Id == salaryByID.Employee_Id
                    && a.Attendance_Date >= salaryByID.Begin_Date && a.Attendance_Date <= salaryByID.End_Date
                    && (a.status_id == 1 || a.status_id == 2))
                    .CountAsync();
                //Lấy dữ liệu lương cơ bản của nhân viên
                var baseSalaryQuery = from salary in _context.salaries
                                      join emp in _context.employees on salary.Employee_Id equals emp.employee_id
                                      join pos in _context.positions on emp.position_id equals pos.position_id
                                      where emp.employee_id == salaryByID.Employee_Id
                                      select pos.base_salary;
                var baseSalary = await baseSalaryQuery.FirstOrDefaultAsync();

                // Tổng số ngày cần tính lương(số ngày phải đi làm trong tháng trừ chủ nhật)
                int totalDays = CalculateWorkingDays(salaryByID.Begin_Date, salaryByID.End_Date);
                //Số tiền nhận được dựa trên ngày đi làm
                var amountByWorkingDays = baseSalary / totalDays * workingDays;
                //Tính tổng tiền thực nhận
                var totalSalary = amountByWorkingDays + totalBonuses - totalDeductions;

                //Gán dữ liệu để cập nhật
                salaryByID.Base_Salary = baseSalary;
                salaryByID.Bonus = totalBonuses;
                salaryByID.Deduction = totalDeductions;
                salaryByID.Total_Salary = totalSalary;

                _context.salaries.Update(salaryByID);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cập nhật dữ liệu lương với Mã số lương: {salaryByID.Salary_Id} thành công!";
                return RedirectToAction(nameof(CapNhatLuongNhanSu));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View();
            }
        }

        //Xuất file lương
        [HttpGet]
        public ActionResult ExportSalaryFile()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportSalaryFile(DateTime? date = null)
        {
            if (!date.HasValue)
            {
                ModelState.AddModelError("", "Vui lòng chọn ngày để xuất file lương");
                return View();
            }

            var year = date.Value.Year;
            var month = date.Value.Month;
            DateTime startDate;
            DateTime endDate;
            if (month != 1)
            {
                // Ngày bắt đầu lấy từ ngày 11 tháng trước
                startDate = new DateTime(year, month - 1, 11);
                endDate = new DateTime(year, month, 10);
            }
            else
            {
                // Ngày bắt đầu lấy từ ngày 11 tháng 12 năm trước
                startDate = new DateTime(year - 1, 12, 11);
                endDate = new DateTime(year, month, 10);
            }

            // Lấy dữ liệu bảng lương theo startDate và endDate
            var salaries = await _context.salaries
                .Where(s => s.Begin_Date.Date == startDate.Date && s.End_Date.Date == endDate.Date)
                .Include(s => s.Employee)
                .ToListAsync();

            if (!salaries.Any())
            {
                ModelState.AddModelError("", "Không có dữ liệu lương.");
                return View();
            }

            string tempDirectory = Path.Combine(Path.GetTempPath(), "SalaryFiles");
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            foreach (var salary in salaries)
            {
                var bonuses = await _context.bonuses
                    .Where(b => b.Employee_Id == salary.Employee_Id && b.Bonus_Date >= startDate && b.Bonus_Date <= endDate)
                    .ToListAsync();

                var deductions = await _context.deductions
                    .Where(d => d.Employee_Id == salary.Employee_Id && d.Deduction_Date >= startDate && d.Deduction_Date <= endDate)
                    .ToListAsync();

                //Tính số ngày nhân viên có đi làm trong tháng
                var workedDays = await _context.attendances.Where(a => a.Employee_Id == salary.Employee_Id
                    && a.Attendance_Date >= salary.Begin_Date && a.Attendance_Date <= salary.End_Date
                    && (a.status_id == 1 || a.status_id == 2))
                    .CountAsync();

                // Tổng số ngày cần tính lương(số ngày phải đi làm trong tháng trừ chủ nhật)
                int totalDays = CalculateWorkingDays(salary.Begin_Date, salary.End_Date);

                // Tạo nội dung file cho mỗi nhân viên
                StringBuilder content = new StringBuilder();
                content.AppendLine("Thông tin lương");
                content.AppendLine("----------------------");
                content.AppendLine($"Mã lương: {salary.Salary_Id}");
                content.AppendLine($"Mã nhân viên: {salary.Employee_Id}");
                content.AppendLine($"Tên nhân viên: {salary.Employee.first_name} {salary.Employee.last_name}");
                content.AppendLine($"Từ ngày: {salary.Begin_Date:dd/MM/yyyy}");
                content.AppendLine($"Đến ngày: {salary.End_Date:dd/MM/yyyy}");
                content.AppendLine($"Lương cơ bản: {salary.Base_Salary:C0}");
                content.AppendLine();

                content.AppendLine("Tiền Thưởng");
                content.AppendLine("----------------------");
                if (bonuses.Any())
                {
                    for (int i = 0; i < bonuses.Count; i++)
                    {
                        content.AppendLine($"Thưởng {i + 1}:");
                        content.AppendLine($"- Tiền: {bonuses[i].Bonus_Amount:C0}");
                        content.AppendLine($"- Ngày: {bonuses[i].Bonus_Date:dd/MM/yyyy}");
                        content.AppendLine($"- Lý do: {bonuses[i].Reason}");
                        content.AppendLine();
                    }
                }
                else
                {
                    content.AppendLine("Không có thưởng.");
                    content.AppendLine();
                }

                content.AppendLine("Khấu trừ");
                content.AppendLine("----------------------");
                if (deductions.Any())
                {
                    for (int i = 0; i < deductions.Count; i++)
                    {
                        content.AppendLine($"Khấu trừ {i + 1}:");
                        content.AppendLine($"- Tiền: {deductions[i].Deduction_Amount:C0}");
                        content.AppendLine($"- Ngày: {deductions[i].Deduction_Date:dd/MM/yyyy}");
                        content.AppendLine($"- Lý do: {deductions[i].Reason}");
                        content.AppendLine();
                    }
                }
                else
                {
                    content.AppendLine("Không có khấu trừ.");
                    content.AppendLine();
                }

                content.AppendLine("Tổng kết");
                content.AppendLine("----------------------");
                content.AppendLine($"Số ngày làm: {workedDays}/{totalDays}");
                content.AppendLine($"Thưởng: {salary.Bonus:C0}");
                content.AppendLine($"Khấu trừ: {salary.Deduction:C0}");
                content.AppendLine($"Tổng lương: {salary.Total_Salary:C0}");
                content.AppendLine();
                content.AppendLine("==================================================");
                content.AppendLine();

                // Tạo tên file và lưu vào thư mục
                string fileName = $"Thong_tin_luong_{salary.Employee_Id}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.txt";
                string filePath = Path.Combine(tempDirectory, fileName);
                await System.IO.File.WriteAllTextAsync(filePath, content.ToString(), Encoding.UTF8);
            }

            string zipFilePath = Path.Combine(Path.GetTempPath(), $"Thong_tin_luong_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.zip");
            if (System.IO.File.Exists(zipFilePath))
            {
                System.IO.File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(tempDirectory, zipFilePath);

            Directory.Delete(tempDirectory, true);

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(zipFilePath);
            return File(fileBytes, "application/zip", Path.GetFileName(zipFilePath));
        }


        // GET: SalaryContronller/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: SalaryContronller/Delete/5
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
        //Tính toán số ngày cần tính lương ngoại trừ Chủ Nhật
        static int CalculateWorkingDays(DateTime start, DateTime end)
        {
            int totalDays = 0;

            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                // Kiểm tra nếu ngày hiện tại không phải là Chủ Nhật
                if (date.DayOfWeek != DayOfWeek.Sunday)
                {
                    totalDays++;
                }
            }

            return totalDays;
        }
        
    }
}

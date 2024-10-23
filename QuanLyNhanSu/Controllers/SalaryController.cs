using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Helpers;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Controllers
{
    public class SalaryController : Controller
    {
        private readonly QuanLyNhanSuDbContext _context;
        public SalaryController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        // GET: SalaryContronller
        public async Task<IActionResult> Index()
        {
            var salaryList = await _context.salaries.ToListAsync();
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
        public async Task<ActionResult> TinhLuong(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Tổng số ngày cần tính lương
                int totalDays = (toDate-fromDate).Days + 1;
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

                    // Lấy tổng tiền thưởng của nhân viên từ bảng Bonus trong khoảng fromDate đến toDate
                    var totalBonus = await _context.bonuses
                        .Where(e => e.Employee_Id == employee.employee_id && e.Bonus_Date >= fromDate && e.Bonus_Date <= toDate)
                        .SumAsync(e => e.Bonus_Amount);

                    // Lấy tổng tiền khấu trừ của nhân viên từ bảng Bonus trong khoảng fromDate đến toDate
                    var totalDeduction = await _context.deductions
                        .Where(e => e.Employee_Id == employee.employee_id && e.Deduction_Date >= fromDate && e.Deduction_Date <= toDate)
                        .SumAsync(e => e.Deduction_Amount);

                    //Tổng số ngày đi làm từ fromDate đến toDate
                    var workingDays = await _context.attendances
                        .Where(a => a.Employee_Id == employee.employee_id && a.Attendance_Date >= fromDate 
                                && a.Attendance_Date <= toDate && (a.status_id == 1 || a.status_id == 2))
                        .CountAsync();
                    //Số tiền nhận được dựa trên ngày đi làm
                    var amountByWorkingDays = baseSalary / 26 * workingDays;
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
                        Begin_Date = fromDate,
                        End_Date = toDate
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
            //Số tiền nhận được dựa trên ngày đi làm
            var amountByWorkingDays = baseSalary / 26 * workingDays;
            //Truyền dữ liệu vào ViewData
            ViewData["Bonuses"] = bonus;
            ViewData["Deductions"] = deduction;
            ViewData["WorkingDays"] = workingDays;
            ViewData["AmountWorkingDays"] = FormatHelpers.FormatCurrencyVND(amountByWorkingDays);
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
                //Số tiền nhận được dựa trên ngày đi làm
                var amountByWorkingDays = baseSalary / 26 * workingDays;
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
    }
}

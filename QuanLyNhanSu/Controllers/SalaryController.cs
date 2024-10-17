using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
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

                    //Lấy danh sách chấm công từ fromDate đến toDate
                    var attendances = await _context.attendances
                        .Where(a => a.Employee_Id == employee.employee_id && a.Attendance_Date >= fromDate && a.Attendance_Date <= toDate)
                        .ToListAsync();

                    //Khấu trừ lương do đi trễ
                    decimal deductionByLate = 50000;
                    foreach (var attendance in attendances)
                    {
                        if (attendance.status_id == 2)
                        {
                            DeductionModel deduction = new DeductionModel
                            {
                                Employee_Id = employee.employee_id,
                                Deduction_Amount = deductionByLate,
                                Deduction_Date = attendance.Attendance_Date,
                                Reason = "Đi trễ"
                            };
                            //thêm dữ liệu vào bảng deductions
                            _context.deductions.Add(deduction);
                        }
                        else if (attendance.status_id == 4)
                        {
                            DeductionModel deduction = new DeductionModel
                            {
                                Employee_Id = employee.employee_id,
                                Deduction_Amount = baseSalary/totalDays,
                                Deduction_Date = attendance.Attendance_Date,
                                Reason = "Vắng mặt"
                            };
                            //thêm dữ liệu vào bảng deductions
                            _context.deductions.Add(deduction);
                        }
                    }
                    await _context.SaveChangesAsync();

                    //Tính tổng tiền đi trễ
                    var totalDeduction = await _context.deductions
                        .Where(e => e.Employee_Id == employee.employee_id && e.Deduction_Date >= fromDate && e.Deduction_Date <= toDate)
                        .SumAsync(e => e.Deduction_Amount);

                    //Tính tổng tiền thực nhận
                    var totalSalary = baseSalary + totalBonus - totalDeduction;

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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: SalaryContronller/Edit/5
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

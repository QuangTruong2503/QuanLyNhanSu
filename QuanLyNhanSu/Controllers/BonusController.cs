using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Helpers;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Controllers
{
    public class BonusController : Controller
    {
        private readonly QuanLyNhanSuDbContext _context;
        public BonusController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        // GET: BonusController
        public async Task<ActionResult> Index(string? searchID = null, DateTime? date = null, int page = 1, int pageSize = 8)
        {
            var bonuses = await _context.bonuses.ToListAsync();
            if (bonuses == null)
            {
                return View();
            }
            if (!string.IsNullOrEmpty(searchID))
            {
                ViewBag.SearchID = searchID;
                bonuses = bonuses.Where(d => d.Employee_Id.Contains(searchID)).ToList();
            }
            if (date.HasValue)
            {
                var month = date.Value.Month;
                var year = date.Value.Year;
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
                ViewBag.Date = date.Value.ToString("yyyy-MM");
                bonuses = bonuses.Where(b => b.Bonus_Date.Date >= startDate.Date && b.Bonus_Date.Date <= endDate.Date).ToList();
            }
            //Đếm dữ liệu hiện có để phân trang
            var counts = bonuses.Count;
            bonuses = bonuses
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            ViewBag.TotalPages = (int)Math.Ceiling(counts / (double)pageSize);
            ViewBag.CurrentPage = page;
            return View(bonuses);
        }

        // GET: BonusController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BonusController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BonusController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(BonusModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra sự tồn tại của nhân viên với truy vấn tối ưu
            var existsEmployee = await _context.employees.AnyAsync(x => x.employee_id == model.Employee_Id);
            if (!existsEmployee)
            {
                ModelState.AddModelError("", "Không tồn tại nhân viên này");
                return View(model);
            }
            try
            {
                _context.bonuses.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Thêm dữ liệu thưởng thành công, Mã số NV: {model.Employee_Id}. Lý do: {model.Reason}";
                return RedirectToAction("Create");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Gặp lỗi: " + ex.Message);
                return View();
            }
        }

        // GET: BonusController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var bonusDetail = await _context.bonuses.FirstOrDefaultAsync(b => b.Bonus_Id == id);
            if (bonusDetail == null)
            {
                return NotFound();
            }
            return View(bonusDetail);
        }

        // POST: BonusController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BonusModel model)
        {
            if (!ModelState.IsValid) 
            {
                return View(model);
            }
            try
            {
                _context.bonuses.Update(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Cập nhật dữ liệu thành công";
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Gặp lỗi: " + ex.Message);
                return View();
            }
        }

        //Delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var bonusID = await _context.bonuses.FirstOrDefaultAsync(x => x.Bonus_Id == id);
                if (bonusID != null) 
                {
                    _context.bonuses.Remove(bonusID);
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
    }
}

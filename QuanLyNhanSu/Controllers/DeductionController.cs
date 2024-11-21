using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;
using System.Drawing.Printing;

namespace QuanLyNhanSu.Controllers
{
    public class DeductionController : Controller
    {
        private readonly QuanLyNhanSuDbContext _context;
        public DeductionController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        // GET: DeductionController
        public async Task<IActionResult> Index(string? searchID = null, DateTime? date = null, int page = 1, int pageSize = 10)
        {
            var deductions = await _context.deductions.ToListAsync();
            if (!string.IsNullOrEmpty(searchID))
            {
                ViewBag.SearchID = searchID;
                deductions = deductions.Where(d => d.Employee_Id.Contains(searchID)).ToList();
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
                deductions = deductions.Where(b => b.Deduction_Date.Date >= startDate.Date && b.Deduction_Date.Date <= endDate.Date).ToList();
            }
            var counts = deductions.Count;
            deductions = deductions
                .Skip((page - 1) * pageSize)
            .Take(pageSize)
                .ToList();
            ViewBag.TotalPages = (int)Math.Ceiling(counts / (double)pageSize);
            ViewBag.CurrentPage = page;
            return View(deductions);
        }

        // GET: DeductionController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DeductionController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DeductionController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeductionModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _context.deductions.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Thêm dữ liệu khấu trừ thành công! NV: {model.Employee_Id}, Lý do: {model.Reason}";
                return RedirectToAction(nameof(Create));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Gặp lỗi: " + ex.Message);
                return View();
            }
        }

        // GET: DeductionController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var deduction = await _context.deductions.FindAsync(id);
            if (deduction == null) {
                return NotFound();
            }
            return View(deduction);
        }

        // POST: DeductionController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, DeductionModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _context.deductions.Update(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Cập nhật dữ liệu thành công! NV: {model.Employee_Id}, Lý do: {model.Reason}";
                return RedirectToAction(nameof(Edit));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Gặp lỗi: " + ex.Message);
                return View();
            }
        }

        // GET: DeductionController/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var deduction = await _context.deductions.FindAsync(id);
            if (deduction != null) { 
                _context.deductions.Remove(deduction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

    }
}

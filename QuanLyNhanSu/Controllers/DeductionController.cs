using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;

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
        public async Task<IActionResult> Index(string? searchID = null)
        {
            var deductions = new List<DeductionModel>();
            if (searchID != null)
            {
                deductions = await _context.deductions.Where(d => d.Employee_Id.Contains(searchID)).ToListAsync();
                return View(deductions);
            }
            deductions = await _context.deductions.ToListAsync();
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DeductionController/Edit/5
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

        // GET: DeductionController/Delete/5
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

        // POST: DeductionController/Delete/5
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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Controllers
{
    public class departmentsController : Controller
    {
        private QuanLyNhanSuDbContext _context;
        public departmentsController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        // GET: departmentsController
        public async Task<IActionResult> Index()
        {
            var list = await _context.departments.ToListAsync();
            return View(list);
        }
            
        // GET: departmentsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: departmentsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: departmentsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(departmentsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _context.departments.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(model);
            }
        }

        // GET: departmentsController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var details = await _context.departments.FindAsync(id);
            return View(details);
        }

        // POST: departmentsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, departmentsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công";
                return RedirectToAction(nameof(Edit));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View();
            }
        }

        // GET: departmentsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: departmentsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                var model = await _context.departments.FindAsync(id);
                if (model == null)
                {
                    TempData["Error"] = $"Không tìm thấy phòng ban với ID = {id}";
                    return RedirectToAction(nameof(Index));
                }
                _context.departments.Remove(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

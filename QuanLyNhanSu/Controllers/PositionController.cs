using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Helpers;
using QuanLyNhanSu.Models;
using QuanLyNhanSu.ViewModels;

namespace QuanLyNhanSu.Controllers
{
    public class PositionController : Controller
    {
        private readonly QuanLyNhanSuDbContext _context;
        public PositionController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        // GET: PositionController
        public async Task<IActionResult> ChucVu()
        {
            var listPositionModelView = from p in _context.positions
                               join d in _context.departments
                               on p.department_id equals d.department_id
                               select new PositionViewModel
                               {
                                   position_id = p.position_id,
                                   position_name = p.position_name,
                                   base_salary = FormatHelpers.FormatCurrencyVND(p.base_salary),
                                   department_id = d.department_id,
                                   department_name = d.department_name
                               };
            var list = await listPositionModelView.ToListAsync();
            return View(list);
        }

        // GET: PositionController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PositionController/Create
        public async Task<IActionResult> ThemMoiChucVu()
        {
            var departments = await _context.departments.ToListAsync();
            ViewBag.departments = new SelectList(departments, "department_id", "department_name");
            return View();
        }

        // POST: PositionController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemMoiChucVu(PositionModel model)
        {
            try
            {
                var departments = await _context.departments.ToListAsync();
                ViewBag.departments = new SelectList(departments, "department_id", "department_name");
                var list = await _context.positions.ToListAsync();
                if (ModelState.IsValid)
                {
                    var nameExists = list.Where(x => x.position_name == model.position_name && x.department_id == model.department_id).FirstOrDefault();
                    if (nameExists != null) { 
                        ModelState.AddModelError("position_name", "Tên chức vụ đã tồn tại");
                        return View(model);
                    }
                    _context.positions.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ChucVu));
                }
                return RedirectToAction(nameof(ChucVu));
            }
            catch
            {
                var departments = await _context.departments.ToListAsync();
                ViewBag.departments = new SelectList(departments, "department_id", "department_name");
                return View(model);
            }
        }

        // GET: PositionController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PositionController/Edit/5
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

        // GET: PositionController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PositionController/Delete/5
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

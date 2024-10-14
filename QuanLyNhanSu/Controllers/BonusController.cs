using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
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
        public async Task<ActionResult> Index()
        {
            var listBonus = await _context.bonuses.ToListAsync();
            return View(listBonus);
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
                model.Bonus_Date = DateTime.Now;
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BonusController/Edit/5
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

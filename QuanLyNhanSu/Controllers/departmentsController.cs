using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;

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
        public ActionResult Create(IFormCollection collection)
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

        // GET: departmentsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: departmentsController/Edit/5
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

        // GET: departmentsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: departmentsController/Delete/5
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

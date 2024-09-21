using Microsoft.AspNetCore.Mvc;
using QuanLyNhanSu.Models;
using System.Diagnostics;
using System.Text.Json;

namespace QuanLyNhanSu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Kiểm tra xem cookie EmployeeData có tồn tại không
            if (Request.Cookies.ContainsKey("EmployeeData"))
            {
                var employeeDataJson = Request.Cookies["EmployeeData"];

                // Chuyển đổi chuỗi JSON thành đối tượng Employee
                var employee = JsonSerializer.Deserialize<EmployeesModel>(employeeDataJson);
                
                // Sử dụng dữ liệu employee trong View
                ViewData["EmployeeName"] = employee.first_name;
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

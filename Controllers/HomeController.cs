using Microsoft.AspNetCore.Mvc;
using QuanLyKyTucXa_UNETIxx_xxx.Models;
using System.Diagnostics;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
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

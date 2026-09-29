using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa.Data;

namespace QuanLyKyTucXa.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TongSV = await _context.SinhViens.CountAsync();
            ViewBag.TongPhong = await _context.PhongKTXs.CountAsync();
            ViewBag.DangO = await _context.PhanPhongs.CountAsync(pp => pp.TrangThai == "DangO");
            ViewBag.PhongTrong = ViewBag.TongPhong - await _context.PhongKTXs.CountAsync(p => p.PhanPhongs.Any(pp => pp.TrangThai == "DangO"));
            ViewBag.DoanhThu = "125.000.000đ";

            return View();
        }
    }
}

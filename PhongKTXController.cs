using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa.Data;

namespace QuanLyKyTucXa.Controllers
{
    public class PhongKTXController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhongKTXController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.PhongKTXs
                .Include(p => p.LoaiPhong)
                .Include(p => p.PhanPhongs)
                .ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            var phong = await _context.PhongKTXs
                .Include(p => p.LoaiPhong)
                .Include(p => p.PhanPhongs)
                .ThenInclude(pp => pp.DangKyKTX)
                .ThenInclude(dk => dk!.SinhVien)
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phong == null) return NotFound();
            return View(phong);
        }
    }
}

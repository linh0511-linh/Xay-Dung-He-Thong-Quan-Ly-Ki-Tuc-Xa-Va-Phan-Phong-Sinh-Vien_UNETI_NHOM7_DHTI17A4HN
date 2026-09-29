using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa.Data;
using QuanLyKyTucXa.Models;

namespace QuanLyKyTucXa.Controllers
{
    public class SinhVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SinhVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách sinh viên
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.SinhViens.Include(s => s.TaiKhoan).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.HoTen.Contains(searchString) || s.Lop.Contains(searchString));
            }

            return View(await query.ToListAsync());
        }

        // 2. Thêm mới Sinh viên
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SinhVien sv, string tenDangNhap, string matKhau)
        {
            if (ModelState.IsValid)
            {
                // Tạo tài khoản trước
                var tk = new TaiKhoan
                {
                    TenDangNhap = tenDangNhap,
                    MatKhau = matKhau,
                    HoTen = sv.HoTen,
                    Email = sv.Email,
                    VaiTro = "SinhVien",
                    TrangThai = true
                };
                _context.TaiKhoans.Add(tk);
                await _context.SaveChangesAsync();

                sv.MaTaiKhoan = tk.MaTaiKhoan;
                sv.TrangThai = true;
                _context.SinhViens.Add(sv);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(sv);
        }

        // 3. Khóa / Mở khóa tài khoản Sinh viên
        public async Task<IActionResult> ToggleLock(int id)
        {
            var sv = await _context.SinhViens.Include(s => s.TaiKhoan).FirstOrDefaultAsync(s => s.MaSinhVien == id);
            if (sv != null && sv.TaiKhoan != null)
            {
                sv.TaiKhoan.TrangThai = !sv.TaiKhoan.TrangThai;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
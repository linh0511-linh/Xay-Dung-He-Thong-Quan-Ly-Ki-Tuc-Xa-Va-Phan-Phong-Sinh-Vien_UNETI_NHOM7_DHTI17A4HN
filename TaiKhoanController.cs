using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa.Data;
using QuanLyKyTucXa.Models;

namespace QuanLyKyTucXa.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return View();
            }

            var user = await _context.TaiKhoans
                .FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap && u.MatKhau == matKhau);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                return View();
            }

            if (!user.TrangThai)
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khóa.";
                return View();
            }

            HttpContext.Session.SetInt32("MaTaiKhoan", user.MaTaiKhoan);
            HttpContext.Session.SetString("HoTen", user.HoTen);
            HttpContext.Session.SetString("VaiTro", user.VaiTro);

            if (user.VaiTro == "Admin")
                return RedirectToAction("Dashboard", "ThongKe");
            
            return RedirectToAction("Profile", "SinhVien");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string tenDangNhap, string matKhau, string xacNhanMatKhau, string hoTen, string email, string gioiTinh, string lop, string khoa, string soDienThoai)
        {
            if (matKhau != xacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            var checkUser = await _context.TaiKhoans.AnyAsync(u => u.TenDangNhap == tenDangNhap);
            if (checkUser)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại trong hệ thống.";
                return View();
            }

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhau = matKhau,
                HoTen = hoTen,
                Email = email,
                VaiTro = "SinhVien",
                TrangThai = true
            };
            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            var sinhVien = new SinhVien
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                HoTen = hoTen,
                NgaySinh = DateTime.Now.AddYears(-18),
                GioiTinh = gioiTinh ?? "Nam",
                Lop = lop ?? "Chưa xếp lớp",
                Khoa = khoa ?? "Công nghệ thông tin",
                SoDienThoai = soDienThoai ?? "",
                Email = email,
                DiaChi = "",
                TrangThai = true
            };
            _context.SinhViens.Add(sinhVien);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

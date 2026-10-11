using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Models;
using QuanLyKyTucXa_UNETIxx_xxx.ViewModels;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

public class AccountController : Controller
{
    private readonly KtxDbContext _db;
    private readonly PasswordHasher<TaiKhoan> _hasher = new();

    public AccountController(KtxDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == vm.TenDangNhap);
        if (user == null || !user.DangHoatDong || _hasher.VerifyHashedPassword(user, user.MatKhauHash, vm.MatKhau) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, user != null && !user.DangHoatDong ? "Tài khoản đang bị khóa hoặc ngừng hoạt động." : "Sai tên đăng nhập hoặc mật khẩu");
            return View(vm);
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("Role", user.VaiTro);
        HttpContext.Session.SetString("UserName", user.TenDangNhap);

        if (user.VaiTro == "Admin") return RedirectToAction("Index", "ThongKe");
        return RedirectToAction("Index", "DangKyKTX");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var tenDangNhap = vm.TenDangNhap.Trim();
        var maSV = vm.MaSV.Trim();

        if (await _db.TaiKhoans.AnyAsync(x => x.TenDangNhap == tenDangNhap))
        {
            ModelState.AddModelError(nameof(vm.TenDangNhap), "Tên đăng nhập đã tồn tại.");
            return View(vm);
        }

        if (await _db.SinhViens.AnyAsync(x => x.MaSV == maSV))
        {
            ModelState.AddModelError(nameof(vm.MaSV), "Mã sinh viên đã được đăng ký.");
            return View(vm);
        }

        if (vm.NgaySinh > DateTime.Today)
        {
            ModelState.AddModelError(nameof(vm.NgaySinh), "Ngày sinh không hợp lệ.");
            return View(vm);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                VaiTro = "SinhVien"
            };
            taiKhoan.MatKhauHash = _hasher.HashPassword(taiKhoan, vm.MatKhau);
            _db.TaiKhoans.Add(taiKhoan);
            await _db.SaveChangesAsync();

            var sinhVien = new SinhVien
            {
                MaSV = maSV,
                HoTen = vm.HoTen.Trim(),
                NgaySinh = vm.NgaySinh,
                GioiTinh = string.IsNullOrWhiteSpace(vm.GioiTinh) ? "Nam" : vm.GioiTinh,
                Lop = string.IsNullOrWhiteSpace(vm.Lop) ? null : vm.Lop.Trim(),
                Khoa = string.IsNullOrWhiteSpace(vm.Khoa) ? null : vm.Khoa.Trim(),
                SoDienThoai = string.IsNullOrWhiteSpace(vm.SoDienThoai) ? null : vm.SoDienThoai.Trim(),
                Email = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim(),
                DiaChi = string.IsNullOrWhiteSpace(vm.DiaChi) ? null : vm.DiaChi.Trim(),
                TaiKhoanId = taiKhoan.Id
            };
            _db.SinhViens.Add(sinhVien);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Msg"] = "Đăng ký tài khoản thành công. Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }
        catch
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Đăng ký không thành công. Vui lòng thử lại.");
            return View(vm);
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}

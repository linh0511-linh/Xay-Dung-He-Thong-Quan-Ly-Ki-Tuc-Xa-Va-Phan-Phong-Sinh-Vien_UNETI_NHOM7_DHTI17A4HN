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
        if (user == null || _hasher.VerifyHashedPassword(user, user.MatKhauHash, vm.MatKhau) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Sai tên đăng nhập hoặc mật khẩu");
            return View(vm);
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("Role", user.VaiTro);
        HttpContext.Session.SetString("UserName", user.TenDangNhap);

        if (user.VaiTro == "Admin") return RedirectToAction("Index", "ThongKe");
        return RedirectToAction("Index", "DangKyKTX");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}

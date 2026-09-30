using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("Admin")]
public class TaiKhoanController : Controller
{
    private readonly KtxDbContext _db;
    public TaiKhoanController(KtxDbContext db) => _db = db;
    public async Task<IActionResult> Index() => View(await _db.TaiKhoans.Include(x=>x.SinhVien).OrderBy(x=>x.VaiTro).ThenBy(x=>x.TenDangNhap).ToListAsync());
}

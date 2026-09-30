using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("Admin")]
public class DonDangKyController : Controller
{
    private readonly KtxDbContext _db;
    public DonDangKyController(KtxDbContext db) => _db = db;
    public async Task<IActionResult> Index(string? status)
    {
        var q = _db.DangKyKTXs.Include(x => x.SinhVien).Include(x => x.LoaiPhong).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.TrangThai == status);
        ViewBag.Status = status;
        return View(await q.OrderByDescending(x => x.NgayDangKy).ToListAsync());
    }
}

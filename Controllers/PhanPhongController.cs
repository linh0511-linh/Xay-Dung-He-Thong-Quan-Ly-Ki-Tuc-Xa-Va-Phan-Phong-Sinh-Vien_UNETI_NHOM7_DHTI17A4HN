using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.Models;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("Admin")]
public class PhanPhongController : Controller
{
    private readonly KtxDbContext _db;
    public PhanPhongController(KtxDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var ds = await _db.DangKyKTXs
            .Include(x => x.SinhVien)
            .Include(x => x.LoaiPhong)
            .Where(x => x.TrangThai == "ChoDuyet")
            .OrderBy(x => x.NgayDangKy)
            .ToListAsync();
        ViewBag.Phongs = await _db.PhongKTXs.Include(x => x.LoaiPhong).Where(x => x.DangHoatDong).OrderBy(x => x.ToaNha).ThenBy(x => x.SoPhong).ToListAsync();
        return View(ds);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int dangKyId, int phongId)
    {
        var dk = await _db.DangKyKTXs.FindAsync(dangKyId);
        var phong = await _db.PhongKTXs.Include(p => p.LoaiPhong).FirstOrDefaultAsync(p => p.Id == phongId);
        if (dk == null || phong == null) return NotFound();

        var dangO = await _db.PhanPhongs.CountAsync(x => x.PhongKTXId == phongId && x.NgayRa == null);
        if (!phong.DangHoatDong || phong.LoaiPhong == null || dangO >= phong.LoaiPhong.SucChua)
        {
            TempData["Err"] = "Phòng đã đầy hoặc ngừng hoạt động.";
            return RedirectToAction(nameof(Index));
        }

        var daCoPhong = await _db.PhanPhongs.AnyAsync(x => x.SinhVienId == dk.SinhVienId && x.NgayRa == null);
        if (daCoPhong)
        {
            TempData["Err"] = "Sinh viên đã được phân phòng.";
            return RedirectToAction(nameof(Index));
        }

        _db.PhanPhongs.Add(new PhanPhong { SinhVienId = dk.SinhVienId, PhongKTXId = phongId });
        dk.TrangThai = "DaDuyet";
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Phân phòng thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var dk = await _db.DangKyKTXs.FindAsync(id);
        if (dk == null) return NotFound();
        dk.TrangThai = "TuChoi";
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã từ chối đơn đăng ký.";
        return RedirectToAction(nameof(Index));
    }
}

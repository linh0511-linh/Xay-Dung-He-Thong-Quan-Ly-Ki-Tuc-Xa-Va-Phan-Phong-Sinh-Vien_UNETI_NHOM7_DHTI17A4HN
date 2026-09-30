using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.Models;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("SinhVien")]
public class DangKyKTXController : Controller
{
    private readonly KtxDbContext _db;
    public DangKyKTXController(KtxDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var sv = await _db.SinhViens.FirstOrDefaultAsync(x => x.TaiKhoanId == userId);
        if (sv == null) return View("MissingStudent");

        var dangKy = await _db.DangKyKTXs
            .Include(x => x.LoaiPhong)
            .Where(x => x.SinhVienId == sv.Id)
            .OrderByDescending(x => x.NgayDangKy)
            .ToListAsync();

        var phong = await _db.PhanPhongs
            .Include(x => x.PhongKTX)!.ThenInclude(x => x!.LoaiPhong)
            .FirstOrDefaultAsync(x => x.SinhVienId == sv.Id && x.NgayRa == null);

        ViewBag.SinhVien = sv;
        ViewBag.PhongDangO = phong;
        ViewBag.LoaiPhongs = await _db.LoaiPhongs.OrderBy(x => x.TenLoai).ToListAsync();
        return View(dangKy);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int loaiPhongId, string? ghiChu)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var sv = await _db.SinhViens.FirstOrDefaultAsync(x => x.TaiKhoanId == userId);
        if (sv == null) return NotFound("Không tìm thấy sinh viên.");

        var loai = await _db.LoaiPhongs.FindAsync(loaiPhongId);
        if (loai == null) return NotFound("Loại phòng không tồn tại.");

        var dangO = await _db.PhanPhongs.AnyAsync(x => x.SinhVienId == sv.Id && x.NgayRa == null);
        if (dangO)
        {
            TempData["Err"] = "Bạn đang ở một phòng, không thể đăng ký thêm.";
            return RedirectToAction(nameof(Index));
        }

        var dangCho = await _db.DangKyKTXs.AnyAsync(x => x.SinhVienId == sv.Id && x.TrangThai == "ChoDuyet");
        if (dangCho)
        {
            TempData["Err"] = "Bạn đã có đơn đang chờ duyệt.";
            return RedirectToAction(nameof(Index));
        }

        _db.DangKyKTXs.Add(new DangKyKTX
        {
            SinhVienId = sv.Id,
            LoaiPhongId = loaiPhongId,
            GhiChu = ghiChu,
            TrangThai = "ChoDuyet"
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đăng ký KTX thành công.";
        return RedirectToAction(nameof(Index));
    }
}

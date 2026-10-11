using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.Models;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("SinhVien")]
public class StudentChuyenPhongController : Controller
{
    private readonly KtxDbContext _db;
    public StudentChuyenPhongController(KtxDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.TaiKhoanId == userId);
        if (student == null) return NotFound("Không tìm thấy hồ sơ sinh viên.");

        var current = await _db.PhanPhongs
            .Include(p => p.PhongKTX)!.ThenInclude(p => p!.LoaiPhong)
            .FirstOrDefaultAsync(p => p.SinhVienId == student.Id && p.NgayRa == null);
        ViewBag.SinhVien = student;
        ViewBag.PhongHienTai = current;
        ViewBag.YeuCauGanNhat = await _db.YeuCauChuyenPhongs
            .Include(x => x.PhongHienTai).Include(x => x.PhongMuonDen)
            .Where(x => x.SinhVienId == student.Id)
            .OrderByDescending(x => x.NgayYeuCau)
            .FirstOrDefaultAsync();

        var rooms = await _db.PhongKTXs
            .Include(p => p.LoaiPhong)
            .Where(p => p.DangHoatDong && (current == null || p.Id != current.PhongKTXId))
            .OrderBy(p => p.ToaNha).ThenBy(p => p.SoPhong)
            .ToListAsync();

        var occupancy = await _db.PhanPhongs.Where(p => p.NgayRa == null)
            .GroupBy(p => p.PhongKTXId)
            .Select(g => new { RoomId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoomId, x => x.Count);
        ViewBag.Occupancy = occupancy;
        return View(rooms);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Request(int phongMuonDenId, string? lyDo)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.TaiKhoanId == userId);
        if (student == null) return NotFound("Không tìm thấy hồ sơ sinh viên.");

        var current = await _db.PhanPhongs.FirstOrDefaultAsync(p => p.SinhVienId == student.Id && p.NgayRa == null);
        if (current == null)
        {
            TempData["Err"] = "Bạn chưa được phân phòng nên chưa thể đăng ký chuyển phòng.";
            return RedirectToAction(nameof(Index));
        }

        var room = await _db.PhongKTXs.Include(p => p.LoaiPhong).FirstOrDefaultAsync(p => p.Id == phongMuonDenId && p.DangHoatDong);
        if (room == null || room.Id == current.PhongKTXId || room.LoaiPhong == null)
        {
            TempData["Err"] = "Phòng bạn chọn không hợp lệ hoặc đang ngừng hoạt động.";
            return RedirectToAction(nameof(Index));
        }

        var occupied = await _db.PhanPhongs.CountAsync(p => p.PhongKTXId == room.Id && p.NgayRa == null);
        if (occupied >= room.LoaiPhong.SucChua)
        {
            TempData["Err"] = "Phòng này đã đủ chỗ. Vui lòng chọn phòng khác còn chỗ.";
            return RedirectToAction(nameof(Index));
        }

        var pending = await _db.YeuCauChuyenPhongs.AnyAsync(x => x.SinhVienId == student.Id && x.TrangThai == "ChoDuyet");
        if (pending)
        {
            TempData["Err"] = "Bạn đã có yêu cầu chuyển phòng đang chờ Admin duyệt.";
            return RedirectToAction(nameof(Index));
        }

        _db.YeuCauChuyenPhongs.Add(new YeuCauChuyenPhong
        {
            SinhVienId = student.Id,
            PhongHienTaiId = current.PhongKTXId,
            PhongMuonDenId = room.Id,
            LyDo = string.IsNullOrWhiteSpace(lyDo) ? null : lyDo.Trim(),
            TrangThai = "ChoDuyet",
            NgayYeuCau = DateTime.Now
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã gửi yêu cầu chuyển phòng trực tiếp trên cổng sinh viên. Vui lòng chờ Admin duyệt.";
        return RedirectToAction(nameof(Index));
    }
}

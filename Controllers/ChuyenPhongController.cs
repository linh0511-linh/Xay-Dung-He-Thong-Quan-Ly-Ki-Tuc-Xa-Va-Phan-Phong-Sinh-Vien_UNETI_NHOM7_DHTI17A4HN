using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.Models;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("Admin")]
public class ChuyenPhongController : Controller
{
    private readonly KtxDbContext _db;
    public ChuyenPhongController(KtxDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.Phongs = await _db.PhongKTXs
            .Include(x => x.LoaiPhong)
            .Where(x => x.DangHoatDong)
            .OrderBy(x => x.ToaNha).ThenBy(x => x.SoPhong)
            .ToListAsync();

        ViewBag.YeuCau = await _db.YeuCauChuyenPhongs
            .Include(x => x.SinhVien)
            .Include(x => x.PhongHienTai)
            .Include(x => x.PhongMuonDen)
            .Where(x => x.TrangThai == "ChoDuyet")
            .OrderByDescending(x => x.NgayYeuCau)
            .ToListAsync();

        return View(await _db.PhanPhongs
            .Include(x => x.SinhVien)
            .Include(x => x.PhongKTX)
            .Where(x => x.NgayRa == null)
            .OrderBy(x => x.PhongKTX!.ToaNha)
            .ThenBy(x => x.PhongKTX!.SoPhong)
            .ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(int id, int phongId)
    {
        var pp = await _db.PhanPhongs.Include(x => x.PhongKTX)
            .FirstOrDefaultAsync(x => x.Id == id && x.NgayRa == null);
        var room = await _db.PhongKTXs.Include(x => x.LoaiPhong).FirstOrDefaultAsync(x => x.Id == phongId);
        if (pp == null || room == null) return NotFound();

        var count = await _db.PhanPhongs.CountAsync(x => x.PhongKTXId == phongId && x.NgayRa == null);
        if (!room.DangHoatDong || room.LoaiPhong == null || count >= room.LoaiPhong.SucChua)
        {
            TempData["Err"] = "Phòng mới đã đầy hoặc không hoạt động.";
            return RedirectToAction(nameof(Index));
        }

        pp.NgayRa = DateTime.Today;
        _db.PhanPhongs.Add(new PhanPhong { SinhVienId = pp.SinhVienId, PhongKTXId = phongId, NgayVao = DateTime.Today });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã chuyển phòng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveRequest(int id)
    {
        var request = await _db.YeuCauChuyenPhongs.FirstOrDefaultAsync(x => x.Id == id && x.TrangThai == "ChoDuyet");
        if (request == null) return NotFound();

        var current = await _db.PhanPhongs.FirstOrDefaultAsync(x => x.SinhVienId == request.SinhVienId && x.NgayRa == null);
        var room = await _db.PhongKTXs.Include(x => x.LoaiPhong).FirstOrDefaultAsync(x => x.Id == request.PhongMuonDenId);
        if (current == null || room == null || !room.DangHoatDong || room.LoaiPhong == null)
        {
            request.TrangThai = "TuChoi";
            await _db.SaveChangesAsync();
            TempData["Err"] = "Không thể duyệt yêu cầu vì phòng hoặc thông tin phân phòng không còn hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var count = await _db.PhanPhongs.CountAsync(x => x.PhongKTXId == room.Id && x.NgayRa == null);
        if (count >= room.LoaiPhong.SucChua)
        {
            TempData["Err"] = "Phòng được yêu cầu đã đầy, yêu cầu chưa được duyệt.";
            return RedirectToAction(nameof(Index));
        }

        current.NgayRa = DateTime.Today;
        _db.PhanPhongs.Add(new PhanPhong { SinhVienId = request.SinhVienId, PhongKTXId = room.Id, NgayVao = DateTime.Today });
        request.TrangThai = "DaDuyet";
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã duyệt yêu cầu chuyển phòng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRequest(int id)
    {
        var request = await _db.YeuCauChuyenPhongs.FirstOrDefaultAsync(x => x.Id == id && x.TrangThai == "ChoDuyet");
        if (request == null) return NotFound();
        request.TrangThai = "TuChoi";
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã từ chối yêu cầu chuyển phòng.";
        return RedirectToAction(nameof(Index));
    }
}

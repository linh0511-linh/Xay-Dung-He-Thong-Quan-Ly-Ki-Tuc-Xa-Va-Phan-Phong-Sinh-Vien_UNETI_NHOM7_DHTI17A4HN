using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.Models;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("Admin")]
public class LoaiPhongController : Controller
{
    private readonly KtxDbContext _db;
    public LoaiPhongController(KtxDbContext db) => _db = db;

    public async Task<IActionResult> Index() => View(await _db.LoaiPhongs.Include(x => x.Phongs).OrderBy(x => x.TenLoai).ToListAsync());

    [HttpGet]
    public IActionResult Create() => View(new LoaiPhong { SoLuongPhongTao = 1 });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LoaiPhong model)
    {
        if (!ModelState.IsValid) return View(model);
        model.Phongs = new List<PhongKTX>();
        _db.LoaiPhongs.Add(model);
        await _db.SaveChangesAsync();
        await TaoPhongMoi(model.Id, model.SoLuongPhongTao, model.ToaNhaTao);
        TempData["Msg"] = $"Đã thêm loại phòng và tạo {model.SoLuongPhongTao} phòng tại tòa {model.ToaNhaTao.ToUpperInvariant()}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var x = await _db.LoaiPhongs.Include(x => x.Phongs).FirstOrDefaultAsync(x => x.Id == id);
        if (x == null) return NotFound();
        x.SoLuongPhongTao = 0;
        x.ToaNhaTao = x.Phongs.FirstOrDefault()?.ToaNha ?? "A";
        return View(x);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LoaiPhong model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            model.Phongs = await _db.PhongKTXs.Where(p => p.LoaiPhongId == id).ToListAsync();
            return View(model);
        }

        var entity = await _db.LoaiPhongs.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        entity.TenLoai = model.TenLoai;
        entity.SucChua = model.SucChua;
        entity.GiaThang = model.GiaThang;
        await _db.SaveChangesAsync();

        if (model.SoLuongPhongTao > 0)
            await TaoPhongMoi(id, model.SoLuongPhongTao, model.ToaNhaTao);

        TempData["Msg"] = model.SoLuongPhongTao > 0
            ? $"Đã cập nhật loại phòng và tạo thêm {model.SoLuongPhongTao} phòng tại tòa {model.ToaNhaTao.ToUpperInvariant()}."
            : "Đã cập nhật loại phòng.";
        return RedirectToAction(nameof(Index));
    }

    // Tạo phòng trong tòa do Admin chọn, tự đánh số từ số nhỏ nhất còn chưa dùng (1, 2, 3...).
    private async Task TaoPhongMoi(int loaiPhongId, int soLuong, string toaNha)
    {
        if (soLuong <= 0) return;
        toaNha = string.IsNullOrWhiteSpace(toaNha) ? "A" : toaNha.Trim().ToUpperInvariant();

        var existing = await _db.PhongKTXs
            .Where(p => p.ToaNha.ToUpper() == toaNha)
            .Select(p => p.SoPhong)
            .ToListAsync();
        var used = existing
            .Select(s => int.TryParse(s, out var n) ? n : -1)
            .Where(n => n > 0)
            .ToHashSet();

        var rooms = new List<PhongKTX>();
        var candidate = 1;
        while (rooms.Count < soLuong)
        {
            while (used.Contains(candidate)) candidate++;
            rooms.Add(new PhongKTX
            {
                SoPhong = candidate.ToString(),
                ToaNha = toaNha,
                Tang = candidate >= 100 ? candidate / 100 : 0,
                LoaiPhongId = loaiPhongId,
                DangHoatDong = true
            });
            used.Add(candidate);
            candidate++;
        }

        _db.PhongKTXs.AddRange(rooms);
        await _db.SaveChangesAsync();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var x = await _db.LoaiPhongs.Include(x => x.Phongs).FirstOrDefaultAsync(x => x.Id == id);
        if (x == null) return NotFound();
        if (x.Phongs.Any())
        {
            TempData["Err"] = "Không thể xóa loại phòng đang có phòng sử dụng.";
            return RedirectToAction(nameof(Index));
        }
        _db.Remove(x);
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã xóa loại phòng.";
        return RedirectToAction(nameof(Index));
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.Models;
using QuanLyKyTucXa_UNETIxx_xxx.ViewModels;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("Admin")]
public class PhongKTXController : Controller
{
    private readonly KtxDbContext _db;
    private const int PageSize = 10;
    public PhongKTXController(KtxDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search, int? loaiPhongId, string? sort, int page = 1)
    {
        page = Math.Max(1, page);
        var q = _db.PhongKTXs.Include(p => p.LoaiPhong).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(p => p.SoPhong.Contains(search) || p.ToaNha.Contains(search));
        if (loaiPhongId.HasValue) q = q.Where(p => p.LoaiPhongId == loaiPhongId.Value);
        q = sort switch
        {
            "phong_desc" => q.OrderByDescending(p => p.SoPhong),
            "toa" => q.OrderBy(p => p.ToaNha).ThenBy(p => p.SoPhong),
            _ => q.OrderBy(p => p.SoPhong)
        };
        var total = await q.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Min(page, totalPages);
        var vm = new PhongListViewModel
        {
            Search = search, LoaiPhongId = loaiPhongId, Sort = sort, Page = page,
            TotalPages = totalPages,
            Phongs = await q.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
            LoaiPhongs = await _db.LoaiPhongs.OrderBy(x => x.TenLoai).ToListAsync()
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.LoaiPhongs = await _db.LoaiPhongs.OrderBy(x => x.TenLoai).ToListAsync();
        return View(new PhongKTX());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PhongKTX model)
    {
        if (await _db.PhongKTXs.AnyAsync(p => p.SoPhong == model.SoPhong && p.ToaNha == model.ToaNha))
            ModelState.AddModelError(nameof(model.SoPhong), "Phòng này đã tồn tại trong tòa nhà.");
        if (!ModelState.IsValid)
        {
            ViewBag.LoaiPhongs = await _db.LoaiPhongs.OrderBy(x => x.TenLoai).ToListAsync();
            return View(model);
        }
        _db.PhongKTXs.Add(model);
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Thêm phòng thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _db.PhongKTXs.FindAsync(id);
        if (model == null) return NotFound();
        ViewBag.LoaiPhongs = await _db.LoaiPhongs.OrderBy(x => x.TenLoai).ToListAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PhongKTX model)
    {
        if (id != model.Id) return BadRequest();
        if (await _db.PhongKTXs.AnyAsync(p => p.Id != id && p.SoPhong == model.SoPhong && p.ToaNha == model.ToaNha))
            ModelState.AddModelError(nameof(model.SoPhong), "Phòng này đã tồn tại trong tòa nhà.");
        if (!ModelState.IsValid)
        {
            ViewBag.LoaiPhongs = await _db.LoaiPhongs.OrderBy(x => x.TenLoai).ToListAsync();
            return View(model);
        }
        _db.Update(model);
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Cập nhật phòng thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _db.PhongKTXs.FindAsync(id);
        if (model == null) return NotFound();
        var dangO = await _db.PhanPhongs.AnyAsync(x => x.PhongKTXId == id && x.NgayRa == null);
        if (dangO)
        {
            TempData["Err"] = "Không thể xóa phòng đang có sinh viên ở.";
            return RedirectToAction(nameof(Index));
        }
        _db.PhongKTXs.Remove(model);
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã xóa phòng.";
        return RedirectToAction(nameof(Index));
    }
}

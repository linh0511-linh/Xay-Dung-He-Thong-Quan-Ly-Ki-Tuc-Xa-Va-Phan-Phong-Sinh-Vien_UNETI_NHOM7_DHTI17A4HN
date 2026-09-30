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
    [HttpGet] public IActionResult Create() => View(new LoaiPhong());
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Create(LoaiPhong model)
    { if (!ModelState.IsValid) return View(model); _db.LoaiPhongs.Add(model); await _db.SaveChangesAsync(); TempData["Msg"]="Đã thêm loại phòng."; return RedirectToAction(nameof(Index)); }
    [HttpGet] public async Task<IActionResult> Edit(int id) { var x=await _db.LoaiPhongs.FindAsync(id); return x==null?NotFound():View(x); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Edit(int id, LoaiPhong model)
    { if(id!=model.Id)return BadRequest(); if(!ModelState.IsValid)return View(model); _db.Update(model); await _db.SaveChangesAsync(); TempData["Msg"]="Đã cập nhật loại phòng."; return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Delete(int id)
    { var x=await _db.LoaiPhongs.Include(x=>x.Phongs).FirstOrDefaultAsync(x=>x.Id==id); if(x==null)return NotFound(); if(x.Phongs.Any()){TempData["Err"]="Không thể xóa loại phòng đang có phòng sử dụng."; return RedirectToAction(nameof(Index));} _db.Remove(x); await _db.SaveChangesAsync(); TempData["Msg"]="Đã xóa loại phòng."; return RedirectToAction(nameof(Index)); }
}

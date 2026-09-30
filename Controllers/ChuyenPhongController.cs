using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.Models;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;
[SessionAuthorize("Admin")]
public class ChuyenPhongController : Controller
{
    private readonly KtxDbContext _db; public ChuyenPhongController(KtxDbContext db)=>_db=db;
    public async Task<IActionResult> Index(){ViewBag.Phongs=await _db.PhongKTXs.Include(x=>x.LoaiPhong).Where(x=>x.DangHoatDong).OrderBy(x=>x.ToaNha).ThenBy(x=>x.SoPhong).ToListAsync(); return View(await _db.PhanPhongs.Include(x=>x.SinhVien).Include(x=>x.PhongKTX).Where(x=>x.NgayRa==null).OrderBy(x=>x.PhongKTX!.ToaNha).ThenBy(x=>x.PhongKTX!.SoPhong).ToListAsync());}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Move(int id,int phongId){var pp=await _db.PhanPhongs.Include(x=>x.PhongKTX).FirstOrDefaultAsync(x=>x.Id==id&&x.NgayRa==null);var room=await _db.PhongKTXs.Include(x=>x.LoaiPhong).FirstOrDefaultAsync(x=>x.Id==phongId);if(pp==null||room==null)return NotFound();var count=await _db.PhanPhongs.CountAsync(x=>x.PhongKTXId==phongId&&x.NgayRa==null);if(!room.DangHoatDong||room.LoaiPhong==null||count>=room.LoaiPhong.SucChua){TempData["Err"]="Phòng mới đã đầy hoặc không hoạt động.";return RedirectToAction(nameof(Index));}pp.NgayRa=DateTime.Today;_db.PhanPhongs.Add(new PhanPhong{SinhVienId=pp.SinhVienId,PhongKTXId=phongId,NgayVao=DateTime.Today});await _db.SaveChangesAsync();TempData["Msg"]="Đã chuyển phòng.";return RedirectToAction(nameof(Index));}
}

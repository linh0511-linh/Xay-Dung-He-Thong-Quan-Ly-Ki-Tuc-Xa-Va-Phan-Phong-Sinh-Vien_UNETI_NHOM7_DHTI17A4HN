using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;
[SessionAuthorize("Admin")]
public class TraPhongController : Controller
{
    private readonly KtxDbContext _db; public TraPhongController(KtxDbContext db)=>_db=db;
    public async Task<IActionResult> Index()
    {
        var ds = await _db.PhanPhongs
            .Include(x => x.SinhVien)
            .Include(x => x.PhongKTX)
            .Where(x => x.NgayRa == null)
            .OrderBy(x => x.PhongKTX!.ToaNha)
            .ThenBy(x => x.PhongKTX!.SoPhong)
            .ToListAsync();
        ViewBag.YeuCauTra = await _db.DangKyKTXs
            .Where(x => x.TrangThai == "YeuCauTra")
            .Select(x => x.SinhVienId)
            .ToListAsync();
        return View(ds);
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Return(int id){var x=await _db.PhanPhongs.FirstOrDefaultAsync(x=>x.Id==id&&x.NgayRa==null);if(x==null)return NotFound();x.NgayRa=DateTime.Today;
        var yeuCau = await _db.DangKyKTXs
            .Where(d => d.SinhVienId == x.SinhVienId && d.TrangThai == "YeuCauTra")
            .OrderByDescending(d => d.NgayDangKy)
            .FirstOrDefaultAsync();
        if (yeuCau != null) yeuCau.TrangThai = "DaTra";
        await _db.SaveChangesAsync();
        TempData["Msg"]="Đã ghi nhận trả phòng.";return RedirectToAction(nameof(Index));}
}

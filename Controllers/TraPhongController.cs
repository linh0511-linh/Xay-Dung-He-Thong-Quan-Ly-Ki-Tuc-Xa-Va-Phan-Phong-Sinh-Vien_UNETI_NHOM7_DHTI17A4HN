using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;
[SessionAuthorize("Admin")]
public class TraPhongController : Controller
{
    private readonly KtxDbContext _db; public TraPhongController(KtxDbContext db)=>_db=db;
    public async Task<IActionResult> Index()=>View(await _db.PhanPhongs.Include(x=>x.SinhVien).Include(x=>x.PhongKTX).Where(x=>x.NgayRa==null).OrderBy(x=>x.PhongKTX!.ToaNha).ThenBy(x=>x.PhongKTX!.SoPhong).ToListAsync());
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Return(int id){var x=await _db.PhanPhongs.FirstOrDefaultAsync(x=>x.Id==id&&x.NgayRa==null);if(x==null)return NotFound();x.NgayRa=DateTime.Today;await _db.SaveChangesAsync();TempData["Msg"]="Đã ghi nhận trả phòng.";return RedirectToAction(nameof(Index));}
}

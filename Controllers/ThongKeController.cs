using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.ViewModels;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("Admin")]
public class ThongKeController : Controller
{
    private readonly KtxDbContext _db;
    public ThongKeController(KtxDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var tongSucChua = await _db.PhongKTXs
            .Where(p => p.DangHoatDong && p.LoaiPhong != null)
            .SumAsync(p => p.LoaiPhong!.SucChua);
        var dangO = await _db.PhanPhongs.CountAsync(p => p.NgayRa == null);

        var vm = new DashboardViewModel
        {
            TongPhong = await _db.PhongKTXs.CountAsync(),
            TongSinhVien = await _db.SinhViens.CountAsync(),
            DangChoDuyet = await _db.DangKyKTXs.CountAsync(d => d.TrangThai == "ChoDuyet"),
            DangO = dangO,
            TongSucChua = tongSucChua,
            ConTrong = Math.Max(0, tongSucChua - dangO),
            TongTaiKhoan = await _db.TaiKhoans.CountAsync(),
            TongLoaiPhong = await _db.LoaiPhongs.CountAsync()
        };

        var now = DateTime.Today;
        for (var i = 5; i >= 0; i--)
        {
            var first = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var next = first.AddMonths(1);
            var count = await _db.DangKyKTXs.CountAsync(x => x.NgayDangKy >= first && x.NgayDangKy < next);
            vm.DangKy6Thang.Add(new ChartItem { Label = $"T{i + 1}", Value = count });
        }

        vm.TheoToa = await _db.PhanPhongs
            .Where(p => p.NgayRa == null && p.PhongKTX != null)
            .GroupBy(p => p.PhongKTX!.ToaNha)
            .Select(g => new BuildingStat { Toa = g.Key, SoNguoi = g.Count() })
            .OrderBy(x => x.Toa)
            .ToListAsync();

        return View(vm);
    }
}

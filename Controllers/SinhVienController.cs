using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.Models;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("Admin")]
public class SinhVienController : Controller
{
    private readonly KtxDbContext _db; public SinhVienController(KtxDbContext db)=>_db=db;
    public async Task<IActionResult> Index(string? search){var q=_db.SinhViens.Include(x=>x.TaiKhoan).Include(x=>x.PhanPhongs).ThenInclude(x=>x.PhongKTX).AsQueryable(); if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.MaSV.Contains(search)||x.HoTen.Contains(search)); ViewBag.Search=search; return View(await q.OrderBy(x=>x.MaSV).ToListAsync());}
}

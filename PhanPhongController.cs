using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa.Data;
using QuanLyKyTucXa.Models;

namespace QuanLyKyTucXa.Controllers
{
    public class PhanPhongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhanPhongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách Đăng ký chờ duyệt
        public async Task<IActionResult> DanhSachDangKy()
        {
            var list = await _context.DangKyKTXs
                .Include(d => d.SinhVien)
                .Where(d => d.TrangThai == "ChoDuyet")
                .ToListAsync();
            return View(list);
        }

        // 2. Thực hiện Phân phòng
        [HttpGet]
        public async Task<IActionResult> Create(int? maDangKy)
        {
            ViewBag.SinhViens = await _context.SinhViens.ToListAsync();
            ViewBag.Phongs = await _context.PhongKTXs.Include(p => p.LoaiPhong).Where(p => p.TrangThai).ToListAsync();
            ViewBag.MaDangKy = maDangKy;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int maDangKy, int maPhong, int soGiuong, DateTime ngayBatDau, DateTime ngayKetThuc)
        {
            var dangKy = await _context.DangKyKTXs.Include(d => d.SinhVien).FirstOrDefaultAsync(d => d.MaDangKy == maDangKy);
            var phong = await _context.PhongKTXs.Include(p => p.LoaiPhong).Include(p => p.PhanPhongs).FirstOrDefaultAsync(p => p.MaPhong == maPhong);

            if (dangKy == null || phong == null) return NotFound();

            // Kiểm tra sức chứa
            int soDangO = phong.PhanPhongs.Count(pp => pp.TrangThai == "DangO");
            if (soDangO >= phong.LoaiPhong!.SoNguoiToiDa)
            {
                ModelState.AddModelError("", "Phòng đã đầy, không thể phân thêm!");
                return View();
            }

            // Tạo bản ghi Phân phòng
            var phanPhong = new PhanPhong
            {
                MaDangKy = maDangKy,
                MaPhong = maPhong,
                SoGiuong = soGiuong,
                NgayVao = ngayBatDau,
                TrangThai = "DangO"
            };
            _context.PhanPhongs.Add(phanPhong);

            // Tạo Hợp đồng tự động
            var hopDong = new HopDong
            {
                MaHopDong = "HD" + DateTime.Now.Ticks.ToString().Substring(12),
                MaSinhVien = dangKy.MaSinhVien,
                MaPhong = maPhong,
                SoGiuong = soGiuong,
                NgayBatDau = ngayBatDau,
                NgayKetThuc = ngayKetThuc,
                TienPhong = phong.LoaiPhong.DonGiaThang,
                TrangThai = "Đang hiệu lực"
            };
            _context.HopDongs.Add(hopDong);

            dangKy.TrangThai = "DaPhanPhong";
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PhongKTX");
        }

        // 3. Trả phòng
        [HttpPost]
        public async Task<IActionResult> TraPhong(int maPhanPhong)
        {
            var pp = await _context.PhanPhongs.Include(p => p.DangKyKTX).FirstOrDefaultAsync(p => p.MaPhanPhong == maPhanPhong);
            if (pp != null)
            {
                pp.TrangThai = "DaTraPhong";
                pp.NgayRa = DateTime.Now;
                if (pp.DangKyKTX != null) pp.DangKyKTX.TrangThai = "DaTraPhong";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "PhongKTX");
        }
    }
}
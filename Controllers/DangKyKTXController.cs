using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.Models;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("SinhVien")]
public class DangKyKTXController : Controller
{
    private readonly KtxDbContext _db;
    public DangKyKTXController(KtxDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var sv = await _db.SinhViens
            .Include(x => x.TaiKhoan)
            .FirstOrDefaultAsync(x => x.TaiKhoanId == userId);
        if (sv == null) return View("MissingStudent");

        var dangKy = await _db.DangKyKTXs
            .Include(x => x.LoaiPhong)
            .Where(x => x.SinhVienId == sv.Id)
            .OrderByDescending(x => x.NgayDangKy)
            .ToListAsync();

        var phong = await _db.PhanPhongs
            .Include(x => x.PhongKTX)!.ThenInclude(x => x!.LoaiPhong)
            .Where(x => x.SinhVienId == sv.Id && x.NgayRa == null)
            .OrderByDescending(x => x.NgayVao)
            .FirstOrDefaultAsync();

        // Đơn đã được phân phòng gần nhất, dùng để hiển thị ngày kết thúc dự kiến.
        var dangKyHienTai = phong == null ? null : await _db.DangKyKTXs
            .Include(x => x.LoaiPhong)
            .Where(x => x.SinhVienId == sv.Id && x.TrangThai == "DaDuyet")
            .OrderByDescending(x => x.NgayDangKy)
            .FirstOrDefaultAsync();

        var lichSuO = await _db.PhanPhongs
            .Include(x => x.PhongKTX)!.ThenInclude(x => x!.LoaiPhong)
            .Where(x => x.SinhVienId == sv.Id)
            .OrderByDescending(x => x.NgayVao)
            .ToListAsync();

        ViewBag.SinhVien = sv;
        ViewBag.PhongDangO = phong;
        ViewBag.DangKyHienTai = dangKyHienTai;
        ViewBag.LichSuO = lichSuO;
        ViewBag.LoaiPhongs = await _db.LoaiPhongs.OrderBy(x => x.TenLoai).ToListAsync();
        ViewBag.PhongChuyenDen = await _db.PhongKTXs
            .Include(x => x.LoaiPhong)
            .Where(x => x.DangHoatDong && (phong == null || x.Id != phong.PhongKTXId))
            .OrderBy(x => x.ToaNha).ThenBy(x => x.SoPhong)
            .ToListAsync();
        ViewBag.YeuCauChuyenPhong = phong == null ? null : await _db.YeuCauChuyenPhongs
            .Include(x => x.PhongHienTai).Include(x => x.PhongMuonDen)
            .Where(x => x.SinhVienId == sv.Id)
            .OrderByDescending(x => x.NgayYeuCau)
            .FirstOrDefaultAsync();
        return View(dangKy);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmAutoRoom(int dangKyId, bool accept)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var sv = await _db.SinhViens.FirstOrDefaultAsync(x => x.TaiKhoanId == userId);
        if (sv == null) return NotFound("Không tìm thấy sinh viên.");

        var dk = await _db.DangKyKTXs.FirstOrDefaultAsync(x => x.Id == dangKyId && x.SinhVienId == sv.Id);
        if (dk == null || dk.TrangThai != "ChoXacNhanPhanPhong" || string.IsNullOrWhiteSpace(dk.GhiChu))
        {
            TempData["Err"] = "Không tìm thấy đề xuất phân phòng đang chờ xác nhận.";
            return RedirectToAction(nameof(Index));
        }

        if (!accept)
        {
            dk.TrangThai = "TuChoi";
            dk.GhiChu = "Bạn không đồng ý phòng trường đề xuất. Vui lòng đăng ký phòng khác.";
            await _db.SaveChangesAsync();
            TempData["Msg"] = "Đã từ chối phòng được đề xuất. Bạn có thể đăng ký loại phòng khác.";
            return RedirectToAction(nameof(Index));
        }

        var marker = dk.GhiChu.Split(';', 2)[0];
        if (!marker.StartsWith("AUTO_ROOM_ID=") || !int.TryParse(marker.Substring("AUTO_ROOM_ID=".Length), out var roomId))
        {
            TempData["Err"] = "Thông tin phòng đề xuất không hợp lệ. Vui lòng liên hệ Admin.";
            return RedirectToAction(nameof(Index));
        }
        var room = await _db.PhongKTXs.Include(x => x.LoaiPhong)
            .FirstOrDefaultAsync(x => x.Id == roomId && x.DangHoatDong);
        if (room?.LoaiPhong == null)
        {
            TempData["Err"] = "Phòng đề xuất không còn hoạt động. Vui lòng liên hệ Admin.";
            return RedirectToAction(nameof(Index));
        }
        if (await _db.PhanPhongs.AnyAsync(x => x.SinhVienId == sv.Id && x.NgayRa == null))
        {
            TempData["Err"] = "Bạn đã có phòng đang ở nên không thể xác nhận thêm.";
            return RedirectToAction(nameof(Index));
        }
        var occupied = await _db.PhanPhongs.CountAsync(x => x.PhongKTXId == room.Id && x.NgayRa == null);
        if (occupied >= room.LoaiPhong.SucChua)
        {
            TempData["Err"] = "Phòng đề xuất vừa đủ chỗ. Vui lòng liên hệ Admin để được phân phòng khác.";
            return RedirectToAction(nameof(Index));
        }

        _db.PhanPhongs.Add(new PhanPhong { SinhVienId = sv.Id, PhongKTXId = room.Id,
            NgayVao = dk.NgayBatDau.Date < DateTime.Today ? DateTime.Today : dk.NgayBatDau.Date });
        dk.TrangThai = "DaDuyet";
        dk.GhiChu = $"Sinh viên đã xác nhận phòng trường đề xuất: Tòa {room.ToaNha}, phòng {room.SoPhong}.";
        await _db.SaveChangesAsync();
        TempData["Msg"] = $"Bạn đã xác nhận thành công phòng {room.SoPhong}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string hoTen, DateTime ngaySinh, string gioiTinh, string? lop, string? khoa, string? soDienThoai, string? email, string? diaChi)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var sv = await _db.SinhViens.FirstOrDefaultAsync(x => x.TaiKhoanId == userId);
        if (sv == null) return NotFound("Không tìm thấy sinh viên.");

        if (string.IsNullOrWhiteSpace(hoTen))
        {
            TempData["Err"] = "Họ tên không được để trống.";
            return RedirectToAction(nameof(Index), new { edit = true });
        }

        if (ngaySinh == default)
        {
            TempData["Err"] = "Ngày sinh không hợp lệ.";
            return RedirectToAction(nameof(Index), new { edit = true });
        }

        if (!string.IsNullOrWhiteSpace(email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            TempData["Err"] = "Email không đúng định dạng.";
            return RedirectToAction(nameof(Index), new { edit = true });
        }

        sv.HoTen = hoTen.Trim();
        sv.NgaySinh = ngaySinh;
        sv.GioiTinh = string.IsNullOrWhiteSpace(gioiTinh) ? "Nam" : gioiTinh;
        sv.Lop = string.IsNullOrWhiteSpace(lop) ? null : lop.Trim();
        sv.Khoa = string.IsNullOrWhiteSpace(khoa) ? null : khoa.Trim();
        sv.SoDienThoai = string.IsNullOrWhiteSpace(soDienThoai) ? null : soDienThoai.Trim();
        sv.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        sv.DiaChi = string.IsNullOrWhiteSpace(diaChi) ? null : diaChi.Trim();

        await _db.SaveChangesAsync();
        TempData["Msg"] = "Cập nhật thông tin sinh viên thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestMove(int phongMuonDenId, string? lyDo)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var sv = await _db.SinhViens.FirstOrDefaultAsync(x => x.TaiKhoanId == userId);
        if (sv == null) return NotFound("Không tìm thấy sinh viên.");

        var current = await _db.PhanPhongs.FirstOrDefaultAsync(x => x.SinhVienId == sv.Id && x.NgayRa == null);
        if (current == null)
        {
            TempData["Err"] = "Bạn chưa được phân phòng nên không thể xin chuyển phòng.";
            return RedirectToAction(nameof(Index));
        }

        var room = await _db.PhongKTXs.Include(x => x.LoaiPhong).FirstOrDefaultAsync(x => x.Id == phongMuonDenId);
        if (room == null || !room.DangHoatDong)
        {
            TempData["Err"] = "Phòng muốn chuyển đến không tồn tại hoặc đang ngừng hoạt động.";
            return RedirectToAction(nameof(Index));
        }
        if (room.Id == current.PhongKTXId)
        {
            TempData["Err"] = "Phòng mới phải khác phòng hiện tại.";
            return RedirectToAction(nameof(Index));
        }

        var soNguoi = await _db.PhanPhongs.CountAsync(x => x.PhongKTXId == room.Id && x.NgayRa == null);
        if (room.LoaiPhong == null || soNguoi >= room.LoaiPhong.SucChua)
        {
            TempData["Err"] = "Phòng bạn chọn đã đủ chỗ.";
            return RedirectToAction(nameof(Index));
        }

        var pending = await _db.YeuCauChuyenPhongs.AnyAsync(x => x.SinhVienId == sv.Id && x.TrangThai == "ChoDuyet");
        if (pending)
        {
            TempData["Err"] = "Bạn đã có một yêu cầu chuyển phòng đang chờ duyệt.";
            return RedirectToAction(nameof(Index));
        }

        _db.YeuCauChuyenPhongs.Add(new YeuCauChuyenPhong
        {
            SinhVienId = sv.Id,
            PhongHienTaiId = current.PhongKTXId,
            PhongMuonDenId = phongMuonDenId,
            LyDo = string.IsNullOrWhiteSpace(lyDo) ? null : lyDo.Trim(),
            TrangThai = "ChoDuyet"
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã gửi yêu cầu chuyển phòng. Vui lòng chờ Admin duyệt.";
        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestReturn()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var sv = await _db.SinhViens.FirstOrDefaultAsync(x => x.TaiKhoanId == userId);
        if (sv == null) return NotFound("Không tìm thấy sinh viên.");

        var current = await _db.PhanPhongs
            .Include(x => x.PhongKTX)
            .FirstOrDefaultAsync(x => x.SinhVienId == sv.Id && x.NgayRa == null);
        if (current == null)
        {
            TempData["Err"] = "Bạn hiện không có phòng để gửi yêu cầu trả phòng.";
            return RedirectToAction(nameof(Index));
        }

        var pending = await _db.DangKyKTXs.AnyAsync(x => x.SinhVienId == sv.Id && x.TrangThai == "YeuCauTra");
        if (pending)
        {
            TempData["Err"] = "Bạn đã gửi yêu cầu trả phòng và đang chờ Admin xử lý.";
            return RedirectToAction(nameof(Index));
        }

        var application = await _db.DangKyKTXs
            .Where(x => x.SinhVienId == sv.Id && x.TrangThai == "DaDuyet")
            .OrderByDescending(x => x.NgayDangKy)
            .FirstOrDefaultAsync();

        if (application == null)
        {
            if (current.PhongKTX == null)
            {
                TempData["Err"] = "Không xác định được loại phòng hiện tại.";
                return RedirectToAction(nameof(Index));
            }

            application = new DangKyKTX
            {
                SinhVienId = sv.Id,
                LoaiPhongId = current.PhongKTX.LoaiPhongId,
                NgayDangKy = DateTime.Now,
                TrangThai = "YeuCauTra",
                GhiChu = "Yêu cầu trả phòng"
            };
            _db.DangKyKTXs.Add(application);
        }
        else
        {
            application.TrangThai = "YeuCauTra";
            application.GhiChu = "Yêu cầu trả phòng";
        }

        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã gửi yêu cầu trả phòng. Vui lòng chờ Admin xác nhận.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int loaiPhongId, DateTime ngayBatDau, DateTime ngayKetThucDuKien, string? ghiChu)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var sv = await _db.SinhViens.FirstOrDefaultAsync(x => x.TaiKhoanId == userId);
        if (sv == null) return NotFound("Không tìm thấy sinh viên.");

        var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(x => x.Id == sv.TaiKhoanId);
        if (taiKhoan == null || !taiKhoan.DangHoatDong)
        {
            TempData["Err"] = "Tài khoản sinh viên đang ngừng hoạt động, không thể đăng ký KTX.";
            return RedirectToAction(nameof(Index));
        }

        var loai = await _db.LoaiPhongs.FindAsync(loaiPhongId);
        if (loai == null) return NotFound("Loại phòng không tồn tại.");

        ngayBatDau = ngayBatDau.Date;
        ngayKetThucDuKien = ngayKetThucDuKien.Date;
        if (ngayBatDau < DateTime.Today)
        {
            TempData["Err"] = "Ngày bắt đầu phải từ hôm nay trở đi.";
            return RedirectToAction(nameof(Index));
        }
        if (ngayKetThucDuKien <= ngayBatDau)
        {
            TempData["Err"] = "Ngày kết thúc dự kiến phải sau ngày bắt đầu.";
            return RedirectToAction(nameof(Index));
        }

        var dangO = await _db.PhanPhongs.AnyAsync(x => x.SinhVienId == sv.Id && x.NgayRa == null);
        if (dangO)
        {
            TempData["Err"] = "Bạn đang ở một phòng (đã được phân phòng), không thể đăng ký thêm.";
            return RedirectToAction(nameof(Index));
        }

        // Chỉ cho phép một đơn đang chờ xử lý tại một thời điểm.
        var dangCho = await _db.DangKyKTXs.AnyAsync(x => x.SinhVienId == sv.Id && (x.TrangThai == "ChoDuyet" || x.TrangThai == "ChoXacNhanPhanPhong"));
        if (dangCho)
        {
            TempData["Err"] = "Bạn đã có đơn đang chờ duyệt, không thể tạo thêm đăng ký cùng lúc.";
            return RedirectToAction(nameof(Index));
        }

        _db.DangKyKTXs.Add(new DangKyKTX
        {
            SinhVienId = sv.Id,
            LoaiPhongId = loaiPhongId,
            NgayBatDau = ngayBatDau,
            NgayKetThucDuKien = ngayKetThucDuKien,
            // Không nhận NgayDangKy từ form: hệ thống tự xác định thời điểm gửi.
            NgayDangKy = DateTime.Now,
            GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim(),
            TrangThai = "ChoDuyet"
        });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đăng ký KTX thành công.";
        return RedirectToAction(nameof(Index));
    }
}

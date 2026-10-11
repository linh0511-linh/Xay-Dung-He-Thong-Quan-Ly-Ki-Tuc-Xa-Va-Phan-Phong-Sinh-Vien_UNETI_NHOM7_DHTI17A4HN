using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.Models;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("Admin")]
public class PhanPhongController : Controller
{
    private readonly KtxDbContext _db;

    public PhanPhongController(KtxDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var ds = await _db.DangKyKTXs
            .AsNoTracking()
            .Include(x => x.SinhVien)
            .Include(x => x.LoaiPhong)
            .Where(x => x.TrangThai == "ChoDuyet")
            .OrderBy(x => x.NgayDangKy)
            .ToListAsync();

        // Chỉ lấy phòng đang hoạt động. Số chỗ còn lại sẽ được tính ở View.
        var phongs = await _db.PhongKTXs
            .AsNoTracking()
            .Include(x => x.LoaiPhong)
            .Where(x => x.DangHoatDong && x.LoaiPhong != null)
            .OrderBy(x => x.ToaNha)
            .ThenBy(x => x.SoPhong)
            .ToListAsync();

        // Đếm số sinh viên đang ở trong từng phòng.
        var soNguoiTheoPhong = await _db.PhanPhongs
            .AsNoTracking()
            .Where(x => x.NgayRa == null)
            .GroupBy(x => x.PhongKTXId)
            .Select(g => new
            {
                PhongId = g.Key,
                SoNguoi = g.Count()
            })
            .ToDictionaryAsync(x => x.PhongId, x => x.SoNguoi);

        ViewBag.Phongs = phongs;
        ViewBag.SoNguoiTheoPhong = soNguoiTheoPhong;

        return View(ds);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int dangKyId, int phongId, bool choPhepVuotSucChua = false)
    {
        // Kiểm tra dữ liệu gửi lên.
        if (dangKyId <= 0 || phongId <= 0)
        {
            TempData["Err"] = "Dữ liệu phân phòng không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        // Include đầy đủ dữ liệu cần dùng.
        var dk = await _db.DangKyKTXs
            .Include(x => x.SinhVien)
            .Include(x => x.LoaiPhong)
            .FirstOrDefaultAsync(x => x.Id == dangKyId);

        var phong = await _db.PhongKTXs
            .Include(x => x.LoaiPhong)
            .FirstOrDefaultAsync(x => x.Id == phongId);

        if (dk == null)
        {
            TempData["Err"] = "Không tìm thấy đơn đăng ký KTX.";
            return RedirectToAction(nameof(Index));
        }

        if (dk.SinhVien == null)
        {
            TempData["Err"] = "Đơn đăng ký chưa có sinh viên hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        if (phong == null)
        {
            TempData["Err"] = "Không tìm thấy phòng được chọn.";
            return RedirectToAction(nameof(Index));
        }

        // Không cho phân lại đơn đã được xử lý.
        if (dk.TrangThai != "ChoDuyet")
        {
            TempData["Err"] = "Đơn này đã được xử lý trước đó.";
            return RedirectToAction(nameof(Index));
        }

        if (!phong.DangHoatDong)
        {
            TempData["Err"] = "Phòng đang ngừng hoạt động.";
            return RedirectToAction(nameof(Index));
        }

        if (phong.LoaiPhong == null)
        {
            TempData["Err"] = "Phòng chưa được thiết lập loại phòng.";
            return RedirectToAction(nameof(Index));
        }

        // Phòng phải đúng loại mà sinh viên đã đăng ký.
        if (phong.LoaiPhongId != dk.LoaiPhongId)
        {
            TempData["Err"] = "Phòng được chọn không đúng loại phòng sinh viên đăng ký.";
            return RedirectToAction(nameof(Index));
        }

        // Kiểm tra sinh viên đã có phòng đang ở hay chưa.
        var daCoPhong = await _db.PhanPhongs
            .AnyAsync(x => x.SinhVienId == dk.SinhVienId && x.NgayRa == null);

        if (daCoPhong)
        {
            TempData["Err"] = "Sinh viên đã được phân một phòng đang ở.";
            return RedirectToAction(nameof(Index));
        }

        // Đếm lại ngay tại thời điểm bấm Phân để tránh phòng bị đầy
        // do một admin khác vừa phân cho sinh viên khác.
        var dangO = await _db.PhanPhongs
            .CountAsync(x => x.PhongKTXId == phongId && x.NgayRa == null);

        if (dangO >= phong.LoaiPhong.SucChua && !choPhepVuotSucChua)
        {
            TempData["Err"] = $"Phòng {phong.SoPhong} đã đầy ({dangO}/{phong.LoaiPhong.SucChua} người). Nếu admin muốn phân thủ công vào phòng đầy, hãy xác nhận tùy chọn vượt sức chứa.";
            return RedirectToAction(nameof(Index));
        }

        // Ngày vào lấy từ đơn đăng ký; nếu dữ liệu cũ không hợp lệ thì dùng hôm nay.
        var ngayVao = dk.NgayBatDau.Date < DateTime.Today
            ? DateTime.Today
            : dk.NgayBatDau.Date;

        _db.PhanPhongs.Add(new PhanPhong
        {
            SinhVienId = dk.SinhVienId,
            PhongKTXId = phongId,
            NgayVao = ngayVao,
            NgayRa = null
        });

        dk.TrangThai = "DaDuyet";

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Err"] = "Không thể lưu phân phòng. Có thể dữ liệu phòng/sinh viên đã thay đổi, hãy tải lại trang rồi thử lại.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Msg"] = choPhepVuotSucChua
            ? $"Admin đã phân thủ công phòng {phong.SoPhong} cho sinh viên {dk.SinhVien.MaSV}. Lưu ý: phòng đang vượt sức chứa tiêu chuẩn."
            : $"Đã phân phòng {phong.SoPhong} cho sinh viên {dk.SinhVien.MaSV} thành công.";

        return RedirectToAction(nameof(Index));
    }


    // Phân phòng tự động: ưu tiên phòng còn chỗ đúng loại; nếu hết, đề xuất phòng trống khác để sinh viên xác nhận.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AutoAssign(int dangKyId)
    {
        var dk = await _db.DangKyKTXs.Include(x => x.SinhVien).Include(x => x.LoaiPhong)
            .FirstOrDefaultAsync(x => x.Id == dangKyId);
        if (dk == null || dk.TrangThai != "ChoDuyet")
        {
            TempData["Err"] = "Không tìm thấy đơn đang chờ duyệt hoặc đơn đã được xử lý.";
            return RedirectToAction(nameof(Index));
        }
        if (await _db.PhanPhongs.AnyAsync(x => x.SinhVienId == dk.SinhVienId && x.NgayRa == null))
        {
            TempData["Err"] = "Sinh viên đã có phòng đang ở.";
            return RedirectToAction(nameof(Index));
        }

        var rooms = await _db.PhongKTXs.Include(x => x.LoaiPhong)
            .Where(x => x.DangHoatDong && x.LoaiPhong != null).ToListAsync();
        var counts = await _db.PhanPhongs.Where(x => x.NgayRa == null)
            .GroupBy(x => x.PhongKTXId).Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count);
        var available = rooms.Where(r => counts.GetValueOrDefault(r.Id, 0) < (r.LoaiPhong?.SucChua ?? 0))
            .OrderBy(r => r.LoaiPhongId == dk.LoaiPhongId ? 0 : 1)
            .ThenBy(r => r.ToaNha).ThenBy(r => r.SoPhong).ToList();
        var room = available.FirstOrDefault();
        if (room == null)
        {
            TempData["Err"] = "Tất cả phòng đang hoạt động đều đã đầy. Hãy bổ sung chỗ/phòng trước khi phân tự động.";
            return RedirectToAction(nameof(Index));
        }

        var roomCount = counts.GetValueOrDefault(room.Id, 0);
        if (room.LoaiPhongId == dk.LoaiPhongId)
        {
            _db.PhanPhongs.Add(new PhanPhong { SinhVienId = dk.SinhVienId, PhongKTXId = room.Id,
                NgayVao = dk.NgayBatDau.Date < DateTime.Today ? DateTime.Today : dk.NgayBatDau.Date });
            dk.TrangThai = "DaDuyet";
            dk.GhiChu = $"Phân phòng tự động: Tòa {room.ToaNha}, phòng {room.SoPhong}.";
            await _db.SaveChangesAsync();
            TempData["Msg"] = $"Đã tự động phân phòng {room.SoPhong} cho sinh viên {dk.SinhVien?.MaSV}.";
        }
        else
        {
            // Lưu mã phòng đề xuất trong ghi chú để không cần thay đổi schema/database hiện có.
            dk.TrangThai = "ChoXacNhanPhanPhong";
            dk.GhiChu = $"AUTO_ROOM_ID={room.Id}; Đề xuất phòng {room.SoPhong} - Tòa {room.ToaNha} ({room.LoaiPhong?.TenLoai}), còn {room.LoaiPhong!.SucChua - roomCount}/{room.LoaiPhong.SucChua} chỗ. Phòng đăng ký ban đầu đã hết chỗ.";
            await _db.SaveChangesAsync();
            TempData["Msg"] = $"Đã gửi đề xuất phòng {room.SoPhong} cho sinh viên xác nhận.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        if (id <= 0)
        {
            TempData["Err"] = "Mã đơn không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var dk = await _db.DangKyKTXs.FindAsync(id);

        if (dk == null)
        {
            TempData["Err"] = "Không tìm thấy đơn đăng ký.";
            return RedirectToAction(nameof(Index));
        }

        if (dk.TrangThai != "ChoDuyet")
        {
            TempData["Err"] = "Đơn này đã được xử lý trước đó.";
            return RedirectToAction(nameof(Index));
        }

        dk.TrangThai = "TuChoi";
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Đã từ chối đơn đăng ký.";
        return RedirectToAction(nameof(Index));
    }
}

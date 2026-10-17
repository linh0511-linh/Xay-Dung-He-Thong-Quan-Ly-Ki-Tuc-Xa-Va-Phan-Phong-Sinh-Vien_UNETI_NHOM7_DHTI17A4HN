
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Filters;
using QuanLyKyTucXa_UNETIxx_xxx.Models;
using QuanLyKyTucXa_UNETIxx_xxx.ViewModels;

namespace QuanLyKyTucXa_UNETIxx_xxx.Controllers;

[SessionAuthorize("Admin")]
public class SinhVienController : Controller
{
    private readonly KtxDbContext _db;
    private readonly PasswordHasher<TaiKhoan> _hasher = new();

    public SinhVienController(KtxDbContext db)
    {
        _db = db;
    }

    // Danh sách và tìm kiếm sinh viên
    public async Task<IActionResult> Index(string? search)
    {
        var q = _db.SinhViens
            .Include(x => x.TaiKhoan)
            .Include(x => x.PhanPhongs)
            .ThenInclude(x => x.PhongKTX)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            q = q.Where(x =>
                x.MaSV.Contains(search) ||
                x.HoTen.Contains(search));
        }

        ViewBag.Search = search;

        return View(await q.OrderBy(x => x.MaSV).ToListAsync());
    }

    // Xem chi tiết sinh viên
    public async Task<IActionResult> Details(int id)
    {
        var sinhVien = await _db.SinhViens
            .Include(x => x.TaiKhoan)
            .Include(x => x.PhanPhongs)
            .ThenInclude(x => x.PhongKTX)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (sinhVien == null)
            return NotFound();

        return View(sinhVien);
    }

    // Hiển thị biểu mẫu thêm sinh viên
    [HttpGet]
    public IActionResult Create()
    {
        return View(new TaoSinhVienViewModel());
    }

    // Xử lý thêm sinh viên
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaoSinhVienViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var tenDangNhap = vm.TenDangNhap.Trim();
        var maSV = vm.MaSV.Trim();

        if (await _db.TaiKhoans.AnyAsync(
                x => x.TenDangNhap == tenDangNhap))
        {
            ModelState.AddModelError(
                nameof(vm.TenDangNhap),
                "Tên đăng nhập đã tồn tại.");
        }

        if (await _db.SinhViens.AnyAsync(
                x => x.MaSV == maSV))
        {
            ModelState.AddModelError(
                nameof(vm.MaSV),
                "Mã sinh viên đã tồn tại.");
        }

        if (vm.NgaySinh.Date > DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(vm.NgaySinh),
                "Ngày sinh không được ở tương lai.");
        }

        if (!ModelState.IsValid)
            return View(vm);

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                VaiTro = "SinhVien",
                DangHoatDong = true
            };

            taiKhoan.MatKhauHash =
                _hasher.HashPassword(taiKhoan, vm.MatKhau);

            _db.TaiKhoans.Add(taiKhoan);
            await _db.SaveChangesAsync();

            var sinhVien = new SinhVien
            {
                MaSV = maSV,
                HoTen = vm.HoTen.Trim(),
                NgaySinh = vm.NgaySinh,
                GioiTinh = vm.GioiTinh,
                Lop = string.IsNullOrWhiteSpace(vm.Lop)
                    ? null : vm.Lop.Trim(),
                Khoa = string.IsNullOrWhiteSpace(vm.Khoa)
                    ? null : vm.Khoa.Trim(),
                DiaChi = string.IsNullOrWhiteSpace(vm.DiaChi)
                    ? null : vm.DiaChi.Trim(),
                Email = string.IsNullOrWhiteSpace(vm.Email)
                    ? null : vm.Email.Trim(),
                SoDienThoai = string.IsNullOrWhiteSpace(vm.SoDienThoai)
                    ? null : vm.SoDienThoai.Trim(),
                TaiKhoanId = taiKhoan.Id
            };

            _db.SinhViens.Add(sinhVien);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["Msg"] = "Thêm sinh viên thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "Không thể lưu dữ liệu. Hãy kiểm tra mã sinh viên, " +
                "tên đăng nhập và cấu trúc cơ sở dữ liệu.");

            return View(vm);
        }
        catch
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "Đã xảy ra lỗi khi thêm sinh viên. Vui lòng thử lại.");

            return View(vm);
        }
    }

    // Hiển thị biểu mẫu sửa sinh viên
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var sinhVien = await _db.SinhViens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (sinhVien == null)
            return NotFound();

        var vm = new SuaSinhVienViewModel
        {
            Id = sinhVien.Id,
            MaSV = sinhVien.MaSV,
            HoTen = sinhVien.HoTen,
            NgaySinh = sinhVien.NgaySinh,
            GioiTinh = sinhVien.GioiTinh,
            Lop = sinhVien.Lop,
            Khoa = sinhVien.Khoa,
            Email = sinhVien.Email,
            SoDienThoai = sinhVien.SoDienThoai,
            DiaChi = sinhVien.DiaChi
        };

        return View(vm);
    }

    // Xử lý cập nhật thông tin sinh viên
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, SuaSinhVienViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        if (vm.NgaySinh.Date > DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(vm.NgaySinh),
                "Ngày sinh không được ở tương lai.");
        }

        if (vm.GioiTinh != "Nam" &&
            vm.GioiTinh != "Nữ" &&
            vm.GioiTinh != "Khác")
        {
            ModelState.AddModelError(
                nameof(vm.GioiTinh),
                "Giới tính không hợp lệ.");
        }

        var maSV = vm.MaSV.Trim();

        if (await _db.SinhViens.AnyAsync(
                x => x.MaSV == maSV && x.Id != id))
        {
            ModelState.AddModelError(
                nameof(vm.MaSV),
                "Mã sinh viên đã được sử dụng.");
        }

        if (!ModelState.IsValid)
            return View(vm);

        var sinhVien = await _db.SinhViens
            .FirstOrDefaultAsync(x => x.Id == id);

        if (sinhVien == null)
            return NotFound();

        sinhVien.MaSV = maSV;
        sinhVien.HoTen = vm.HoTen.Trim();
        sinhVien.NgaySinh = vm.NgaySinh;
        sinhVien.GioiTinh = vm.GioiTinh;
        sinhVien.Lop = string.IsNullOrWhiteSpace(vm.Lop)
            ? null : vm.Lop.Trim();
        sinhVien.Khoa = string.IsNullOrWhiteSpace(vm.Khoa)
            ? null : vm.Khoa.Trim();
        sinhVien.Email = string.IsNullOrWhiteSpace(vm.Email)
            ? null : vm.Email.Trim();
        sinhVien.SoDienThoai =
            string.IsNullOrWhiteSpace(vm.SoDienThoai)
                ? null : vm.SoDienThoai.Trim();
        sinhVien.DiaChi = string.IsNullOrWhiteSpace(vm.DiaChi)
            ? null : vm.DiaChi.Trim();

        try
        {
            await _db.SaveChangesAsync();

            TempData["Msg"] = "Cập nhật sinh viên thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Không thể cập nhật dữ liệu. Vui lòng kiểm tra lại.");

            return View(vm);
        }
    }
}

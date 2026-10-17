
namespace QuanLyKyTucXa_UNETIxx_xxx.Models;

public class DangKyKTX
{
    public int Id { get; set; }

    public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }

    public int LoaiPhongId { get; set; }
    public LoaiPhong? LoaiPhong { get; set; }

    public DateTime NgayDangKy { get; set; } = DateTime.Now;

    // Thời gian ở dự kiến của lần đăng ký.
    // Ngày đăng ký vẫn do hệ thống tự ghi.
    public DateTime NgayBatDau { get; set; } = DateTime.Today;

    public DateTime NgayKetThucDuKien { get; set; } = DateTime.Today.AddMonths(4);

    public string TrangThai { get; set; } = "ChoDuyet";

    public string? GhiChu { get; set; }
}

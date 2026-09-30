namespace QuanLyKyTucXa_UNETIxx_xxx.Models;

public class DangKyKTX
{
    public int Id { get; set; }
    public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }

    public int LoaiPhongId { get; set; }          // loại phòng SV mong muốn
    public LoaiPhong? LoaiPhong { get; set; }

    public DateTime NgayDangKy { get; set; } = DateTime.Now;
    public string TrangThai { get; set; } = "ChoDuyet";   // ChoDuyet / DaDuyet / TuChoi
    public string? GhiChu { get; set; }
}
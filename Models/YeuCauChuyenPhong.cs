namespace QuanLyKyTucXa_UNETIxx_xxx.Models;

public class YeuCauChuyenPhong
{
    public int Id { get; set; }

    public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }

    public int PhongHienTaiId { get; set; }
    public PhongKTX? PhongHienTai { get; set; }

    public int PhongMuonDenId { get; set; }
    public PhongKTX? PhongMuonDen { get; set; }

    public string? LyDo { get; set; }
    public string TrangThai { get; set; } = "ChoDuyet";
    public DateTime NgayYeuCau { get; set; } = DateTime.Now;
}

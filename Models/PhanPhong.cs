namespace QuanLyKyTucXa_UNETIxx_xxx.Models;

public class PhanPhong
{
    public int Id { get; set; }
    public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }

    public int PhongKTXId { get; set; }
    public PhongKTX? PhongKTX { get; set; }

    public DateTime NgayVao { get; set; } = DateTime.Today;
    public DateTime? NgayRa { get; set; }          // null = đang ở
}
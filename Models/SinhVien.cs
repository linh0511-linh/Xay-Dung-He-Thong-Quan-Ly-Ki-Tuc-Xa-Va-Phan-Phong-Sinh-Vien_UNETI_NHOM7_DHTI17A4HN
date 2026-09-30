using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETIxx_xxx.Models;

public class SinhVien
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Mã sinh viên")]
    public string MaSV { get; set; } = "";

    [Required(ErrorMessage = "Nhập họ tên"), StringLength(100)]
    public string HoTen { get; set; } = "";

    [Display(Name = "Giới tính")]
    public string GioiTinh { get; set; } = "Nam";

    [DataType(DataType.Date)]
    public DateTime NgaySinh { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "SĐT không hợp lệ")]
    public string? SoDienThoai { get; set; }

    public int TaiKhoanId { get; set; }
    public TaiKhoan? TaiKhoan { get; set; }

    public List<DangKyKTX> DangKys { get; set; } = new();
    public List<PhanPhong> PhanPhongs { get; set; } = new();
}
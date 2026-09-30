using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETIxx_xxx.Models;

public class TaiKhoan
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [StringLength(50)]
    public string TenDangNhap { get; set; } = "";

    [Required]
    public string MatKhauHash { get; set; } = "";

    [Required]
    public string VaiTro { get; set; } = "SinhVien";   // "Admin" hoặc "SinhVien"

    public SinhVien? SinhVien { get; set; }
}
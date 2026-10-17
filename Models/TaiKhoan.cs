
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
    public string VaiTro { get; set; } = "SinhVien";

    // Tài khoản có thể bị khóa nhưng vẫn giữ lại lịch sử.
    public bool DangHoatDong { get; set; } = true;

    public SinhVien? SinhVien { get; set; }
}

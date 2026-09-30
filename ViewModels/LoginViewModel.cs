using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETIxx_xxx.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Nhập tên đăng nhập")]
    public string TenDangNhap { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string MatKhau { get; set; } = "";
}

using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETIxx_xxx.ViewModels;

public class SuaSinhVienViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã sinh viên")]
    [StringLength(20)]
    [Display(Name = "Mã sinh viên")]
    public string MaSV { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string HoTen { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime NgaySinh { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giới tính")]
    [Display(Name = "Giới tính")]
    public string GioiTinh { get; set; } = "Nam";

    [StringLength(100)]
    [Display(Name = "Lớp")]
    public string? Lop { get; set; }

    [StringLength(100)]
    [Display(Name = "Khoa")]
    public string? Khoa { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(150)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(30)]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [StringLength(250)]
    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; }
}

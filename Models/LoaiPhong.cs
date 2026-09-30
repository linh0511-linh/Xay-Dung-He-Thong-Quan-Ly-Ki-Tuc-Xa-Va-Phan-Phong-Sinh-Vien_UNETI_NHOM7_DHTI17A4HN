using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETIxx_xxx.Models;

public class LoaiPhong
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nhập tên loại phòng")]
    [StringLength(100)]
    [Display(Name = "Tên loại phòng")]
    public string TenLoai { get; set; } = "";

    [Range(1, 20, ErrorMessage = "Sức chứa từ 1 đến 20")]
    [Display(Name = "Sức chứa")]
    public int SucChua { get; set; }

    [Range(0, 100000000)]
    [Display(Name = "Giá / tháng")]
    public decimal GiaThang { get; set; }

    public List<PhongKTX> Phongs { get; set; } = new();
}
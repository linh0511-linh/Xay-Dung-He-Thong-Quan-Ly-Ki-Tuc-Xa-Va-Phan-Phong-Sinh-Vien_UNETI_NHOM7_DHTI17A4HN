
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    [NotMapped]
    [Required(ErrorMessage = "Nhập tên tòa nhà")]
    [StringLength(50)]
    [Display(Name = "Tòa nhà")]
    public string ToaNhaTao { get; set; } = "A";

    [NotMapped]
    [Range(0, 200, ErrorMessage = "Số phòng cần tạo từ 0 đến 200")]
    [Display(Name = "Số lượng phòng cần tạo / thêm")]
    public int SoLuongPhongTao { get; set; } = 1;
}

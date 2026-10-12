using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETIxx_xxx.Models;

public class PhongKTX
{
    public int Id { get; set; }

    [Display(Name = "Tòa nhà")]
    [StringLength(50)]
    public string ToaNha { get; set; } = "";

    [Required, StringLength(20)]
    [Display(Name = "Số phòng")]
    public string SoPhong { get; set; } = "";
    
    [Range(0, 100, ErrorMessage = "Tầng từ 0 đến 100")]
    [Display(Name = "Tầng")]
    public int Tang { get; set; }
    public int LoaiPhongId { get; set; }
    public bool DangHoatDong { get; set; } = true;
    public LoaiPhong? LoaiPhong { get; set; }
    public List<PhanPhong> PhanPhongs { get; set; } = new();
}

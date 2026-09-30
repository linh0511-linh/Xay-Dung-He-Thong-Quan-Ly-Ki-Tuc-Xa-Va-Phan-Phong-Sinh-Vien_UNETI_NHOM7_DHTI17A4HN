using System.ComponentModel.DataAnnotations;

namespace QuanLyKyTucXa_UNETIxx_xxx.Models;

public class PhongKTX
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Số phòng")]
    public string SoPhong { get; set; } = "";

    [Display(Name = "Tòa nhà")]
    [StringLength(50)]
    public string ToaNha { get; set; } = "";

    public int LoaiPhongId { get; set; }
    public LoaiPhong? LoaiPhong { get; set; }

    public bool DangHoatDong { get; set; } = true;

    public List<PhanPhong> PhanPhongs { get; set; } = new();
}
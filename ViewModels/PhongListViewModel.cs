using QuanLyKyTucXa_UNETIxx_xxx.Models;

namespace QuanLyKyTucXa_UNETIxx_xxx.ViewModels;

public class PhongListViewModel
{
    public List<PhongKTX> Phongs { get; set; } = new();
    public string? Search { get; set; }
    public int? LoaiPhongId { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
    public List<LoaiPhong> LoaiPhongs { get; set; } = new();
}
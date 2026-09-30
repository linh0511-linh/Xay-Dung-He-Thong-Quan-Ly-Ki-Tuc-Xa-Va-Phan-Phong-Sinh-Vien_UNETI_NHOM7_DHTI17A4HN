namespace QuanLyKyTucXa_UNETIxx_xxx.ViewModels;

public class DashboardViewModel
{
    public int TongPhong { get; set; }
    public int TongSinhVien { get; set; }
    public int DangChoDuyet { get; set; }
    public int DangO { get; set; }
    public int TongSucChua { get; set; }
    public int ConTrong { get; set; }
    public int TongTaiKhoan { get; set; }
    public int TongLoaiPhong { get; set; }
    public List<ChartItem> DangKy6Thang { get; set; } = new();
    public List<BuildingStat> TheoToa { get; set; } = new();
}

public class ChartItem
{
    public string Label { get; set; } = "";
    public int Value { get; set; }
}

public class BuildingStat
{
    public string Toa { get; set; } = "";
    public int SoNguoi { get; set; }
}

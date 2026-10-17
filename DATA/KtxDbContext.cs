
using Microsoft.EntityFrameworkCore;
using QuanLyKyTucXa_UNETIxx_xxx.Models;

namespace QuanLyKyTucXa_UNETIxx_xxx.Data;

public class KtxDbContext : DbContext
{
    public KtxDbContext(DbContextOptions<KtxDbContext> options) : base(options) { }

    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<LoaiPhong> LoaiPhongs => Set<LoaiPhong>();
    public DbSet<PhongKTX> PhongKTXs => Set<PhongKTX>();
    public DbSet<SinhVien> SinhViens => Set<SinhVien>();
    public DbSet<DangKyKTX> DangKyKTXs => Set<DangKyKTX>();
    public DbSet<PhanPhong> PhanPhongs => Set<PhanPhong>();
    public DbSet<YeuCauChuyenPhong> YeuCauChuyenPhongs => Set<YeuCauChuyenPhong>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<TaiKhoan>().HasIndex(t => t.TenDangNhap).IsUnique();
        mb.Entity<SinhVien>().HasIndex(s => s.MaSV).IsUnique();
        mb.Entity<LoaiPhong>().Property(l => l.GiaThang).HasColumnType("decimal(18,0)");

        // Tránh xóa dây chuyền gây lỗi multiple cascade paths
        mb.Entity<PhanPhong>().HasOne(p => p.SinhVien).WithMany(s => s.PhanPhongs)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<DangKyKTX>().HasOne(d => d.LoaiPhong).WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<YeuCauChuyenPhong>().HasOne(x => x.SinhVien).WithMany()
            .HasForeignKey(x => x.SinhVienId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<YeuCauChuyenPhong>().HasOne(x => x.PhongHienTai).WithMany()
            .HasForeignKey(x => x.PhongHienTaiId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<YeuCauChuyenPhong>().HasOne(x => x.PhongMuonDen).WithMany()
            .HasForeignKey(x => x.PhongMuonDenId).OnDelete(DeleteBehavior.Restrict);
    }
}

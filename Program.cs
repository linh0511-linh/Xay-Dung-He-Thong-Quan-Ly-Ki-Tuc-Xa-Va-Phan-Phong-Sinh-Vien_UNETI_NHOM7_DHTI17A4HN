using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuanLyKyTucXa_UNETIxx_xxx.Data;
using QuanLyKyTucXa_UNETIxx_xxx.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<KtxDbContext>(o => o
    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    // Snapshot migration cũ chưa đồng bộ với một số thuộc tính đã được bổ sung.
    // Các cột này được bổ sung an toàn bằng SQL kiểm tra tồn tại bên dưới.
    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromMinutes(30); o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true; });
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Account}/{action=Login}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KtxDbContext>();
    db.Database.Migrate();

    // Bổ sung các cột mới của nghiệp vụ đăng ký/xem lịch sử cho cả DB mới và DB cũ.
    // Dùng IF COL_LENGTH để chạy an toàn nhiều lần, không làm mất dữ liệu cũ.
    await db.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('dbo.TaiKhoans', 'DangHoatDong') IS NULL
        BEGIN
            ALTER TABLE dbo.TaiKhoans ADD DangHoatDong bit NOT NULL CONSTRAINT DF_TaiKhoans_DangHoatDong DEFAULT(1);
        END

        IF COL_LENGTH('dbo.PhongKTXs', 'Tang') IS NULL
        BEGIN
            ALTER TABLE dbo.PhongKTXs ADD Tang int NOT NULL CONSTRAINT DF_PhongKTXs_Tang DEFAULT(0);
        END

        IF COL_LENGTH('dbo.DangKyKTXs', 'NgayBatDau') IS NULL
        BEGIN
            ALTER TABLE dbo.DangKyKTXs ADD NgayBatDau datetime2 NOT NULL CONSTRAINT DF_DangKyKTXs_NgayBatDau DEFAULT(GETDATE());
        END

        IF COL_LENGTH('dbo.DangKyKTXs', 'NgayKetThucDuKien') IS NULL
        BEGIN
            ALTER TABLE dbo.DangKyKTXs ADD NgayKetThucDuKien datetime2 NOT NULL CONSTRAINT DF_DangKyKTXs_NgayKetThucDuKien DEFAULT(DATEADD(month, 4, GETDATE()));
        END

        -- Với dữ liệu phòng cũ dạng 101/201..., tự điền tầng nếu trước đây chưa có cột.
        UPDATE dbo.PhongKTXs
        SET Tang = TRY_CONVERT(int, LEFT(SoPhong, 1))
        WHERE Tang = 0 AND SoPhong NOT LIKE '%[^0-9]%' AND LEN(SoPhong) >= 3;
    """);

    // Đảm bảo 3 cột thông tin sinh viên tồn tại trong DB cũ.
    // Trường hợp database đã có dữ liệu nhưng chưa chạy migration ThemThongTinSinhVien.
    await db.Database.ExecuteSqlRawAsync("""
        IF COL_LENGTH('dbo.SinhViens', 'Lop') IS NULL
            ALTER TABLE dbo.SinhViens ADD Lop nvarchar(100) NULL;
        IF COL_LENGTH('dbo.SinhViens', 'Khoa') IS NULL
            ALTER TABLE dbo.SinhViens ADD Khoa nvarchar(100) NULL;
        IF COL_LENGTH('dbo.SinhViens', 'DiaChi') IS NULL
            ALTER TABLE dbo.SinhViens ADD DiaChi nvarchar(250) NULL;
    """);

    // Bổ sung bảng yêu cầu chuyển phòng cho database cũ nếu bảng chưa tồn tại.
    await db.Database.ExecuteSqlRawAsync("""
        IF OBJECT_ID('dbo.YeuCauChuyenPhongs', 'U') IS NULL
        BEGIN
            CREATE TABLE dbo.YeuCauChuyenPhongs
            (
                Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_YeuCauChuyenPhongs PRIMARY KEY,
                SinhVienId int NOT NULL,
                PhongHienTaiId int NOT NULL,
                PhongMuonDenId int NOT NULL,
                LyDo nvarchar(max) NULL,
                TrangThai nvarchar(max) NOT NULL,
                NgayYeuCau datetime2 NOT NULL,
                CONSTRAINT FK_YeuCauChuyenPhongs_SinhViens_SinhVienId FOREIGN KEY (SinhVienId) REFERENCES dbo.SinhViens(Id),
                CONSTRAINT FK_YeuCauChuyenPhongs_PhongKTXs_PhongHienTaiId FOREIGN KEY (PhongHienTaiId) REFERENCES dbo.PhongKTXs(Id),
                CONSTRAINT FK_YeuCauChuyenPhongs_PhongKTXs_PhongMuonDenId FOREIGN KEY (PhongMuonDenId) REFERENCES dbo.PhongKTXs(Id)
            );
            CREATE INDEX IX_YeuCauChuyenPhongs_SinhVienId ON dbo.YeuCauChuyenPhongs(SinhVienId);
            CREATE INDEX IX_YeuCauChuyenPhongs_PhongHienTaiId ON dbo.YeuCauChuyenPhongs(PhongHienTaiId);
            CREATE INDEX IX_YeuCauChuyenPhongs_PhongMuonDenId ON dbo.YeuCauChuyenPhongs(PhongMuonDenId);
        END
    """);

    var hasher = new PasswordHasher<TaiKhoan>();

    // Đảm bảo tài khoản Admin mặc định luôn đăng nhập được.
    // Nếu database cũ đã có admin nhưng mật khẩu/hash cũ không còn đúng,
    // hệ thống sẽ cập nhật lại mật khẩu mặc định: admin / Admin@123.
    var admin = await db.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == "admin");
    if (admin == null)
    {
        admin = new TaiKhoan { TenDangNhap = "admin", VaiTro = "Admin" };
        admin.MatKhauHash = hasher.HashPassword(admin, "Admin@123");
        db.TaiKhoans.Add(admin);
    }
    else
    {
        admin.VaiTro = "Admin";
        var resetAdminPassword = false;
        try
        {
            resetAdminPassword = hasher.VerifyHashedPassword(admin, admin.MatKhauHash ?? "", "Admin@123") == PasswordVerificationResult.Failed;
        }
        catch
        {
            resetAdminPassword = true;
        }

        if (resetAdminPassword)
            admin.MatKhauHash = hasher.HashPassword(admin, "Admin@123");
    }

    if (!await db.LoaiPhongs.AnyAsync())
    {
        db.LoaiPhongs.AddRange(
            new LoaiPhong { TenLoai = "Phòng 4 người", SucChua = 4, GiaThang = 500000 },
            new LoaiPhong { TenLoai = "Phòng 6 người", SucChua = 6, GiaThang = 400000 },
            new LoaiPhong { TenLoai = "Phòng 8 người", SucChua = 8, GiaThang = 300000 });
        await db.SaveChangesAsync();
    }

    if (!await db.PhongKTXs.AnyAsync())
    {
        var loai4 = await db.LoaiPhongs.FirstAsync(x => x.SucChua == 4);
        var loai6 = await db.LoaiPhongs.FirstAsync(x => x.SucChua == 6);
        db.PhongKTXs.AddRange(
            new PhongKTX { SoPhong = "101", ToaNha = "A", Tang = 1, LoaiPhongId = loai4.Id },
            new PhongKTX { SoPhong = "102", ToaNha = "A", Tang = 1, LoaiPhongId = loai6.Id },
            new PhongKTX { SoPhong = "201", ToaNha = "B", Tang = 2, LoaiPhongId = loai4.Id });
    }

    if (!await db.TaiKhoans.AnyAsync(x => x.TenDangNhap == "sv001"))
    {
        var svAccount = new TaiKhoan { TenDangNhap = "sv001", VaiTro = "SinhVien" };
        svAccount.MatKhauHash = hasher.HashPassword(svAccount, "123456");
        db.TaiKhoans.Add(svAccount);
        await db.SaveChangesAsync();
        db.SinhViens.Add(new SinhVien { MaSV = "SV001", HoTen = "Nguyễn Văn A", GioiTinh = "Nam", NgaySinh = new DateTime(2005, 1, 1), Lop = "DHTI17A1HN", Khoa = "Công nghệ thông tin", SoDienThoai = "0900000001", Email = "sv001@example.com", DiaChi = "Hà Nội", TaiKhoanId = svAccount.Id });
    }
    await db.SaveChangesAsync();
}
app.Run();

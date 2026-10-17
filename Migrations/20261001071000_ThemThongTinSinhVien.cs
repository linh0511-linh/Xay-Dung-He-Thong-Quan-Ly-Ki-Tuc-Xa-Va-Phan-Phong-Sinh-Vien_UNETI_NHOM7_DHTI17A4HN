using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKyTucXa_UNETIxx_xxx.Migrations
{
    public partial class ThemThongTinSinhVien : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.SinhViens', 'DiaChi') IS NULL
                    ALTER TABLE dbo.SinhViens ADD DiaChi nvarchar(250) NULL;
                IF COL_LENGTH('dbo.SinhViens', 'Khoa') IS NULL
                    ALTER TABLE dbo.SinhViens ADD Khoa nvarchar(100) NULL;
                IF COL_LENGTH('dbo.SinhViens', 'Lop') IS NULL
                    ALTER TABLE dbo.SinhViens ADD Lop nvarchar(100) NULL;
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DiaChi", table: "SinhViens");
            migrationBuilder.DropColumn(name: "Khoa", table: "SinhViens");
            migrationBuilder.DropColumn(name: "Lop", table: "SinhViens");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKyTucXa_UNETIxx_xxx.Migrations
{
    public partial class ThemYeuCauChuyenPhong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YeuCauChuyenPhongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    PhongHienTaiId = table.Column<int>(type: "int", nullable: false),
                    PhongMuonDenId = table.Column<int>(type: "int", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayYeuCau = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauChuyenPhongs", x => x.Id);
                    table.ForeignKey("FK_YeuCauChuyenPhongs_PhongKTXs_PhongHienTaiId", x => x.PhongHienTaiId, "PhongKTXs", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_YeuCauChuyenPhongs_PhongKTXs_PhongMuonDenId", x => x.PhongMuonDenId, "PhongKTXs", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_YeuCauChuyenPhongs_SinhViens_SinhVienId", x => x.SinhVienId, "SinhViens", "Id", onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateIndex("IX_YeuCauChuyenPhongs_PhongHienTaiId", "YeuCauChuyenPhongs", "PhongHienTaiId");
            migrationBuilder.CreateIndex("IX_YeuCauChuyenPhongs_PhongMuonDenId", "YeuCauChuyenPhongs", "PhongMuonDenId");
            migrationBuilder.CreateIndex("IX_YeuCauChuyenPhongs_SinhVienId", "YeuCauChuyenPhongs", "SinhVienId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "YeuCauChuyenPhongs");
        }
    }
}

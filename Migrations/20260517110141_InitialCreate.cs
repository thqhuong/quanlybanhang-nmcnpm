using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quanlybanhang_nmcnpm.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    MaNhaCungCap = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNCC = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.MaNhaCungCap);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    MaKH = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKH = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiaChiKH = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.MaKH);
                });

            migrationBuilder.CreateTable(
                name: "InventoryReceipts",
                columns: table => new
                {
                    MaPhieuNhapKho = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgayNhapKho = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<int>(type: "int", nullable: false),
                    MaNhanVien = table.Column<int>(type: "int", nullable: false),
                    MaNhaCungCap = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceipts", x => x.MaPhieuNhapKho);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    MaVaiTro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenVaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.MaVaiTro);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    MaHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenHang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false),
                    MaNhaCungCap = table.Column<int>(type: "int", nullable: false),
                    MaLoai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.MaHang);
                    table.ForeignKey(
                        name: "FK_Products_Categories_MaNhaCungCap",
                        column: x => x.MaNhaCungCap,
                        principalTable: "Categories",
                        principalColumn: "MaNhaCungCap",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    MaDonHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKH = table.Column<int>(type: "int", nullable: false),
                    MaNhanVien = table.Column<int>(type: "int", nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.MaDonHang);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_MaKH",
                        column: x => x.MaKH,
                        principalTable: "Customers",
                        principalColumn: "MaKH",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    MaNhanVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaKH = table.Column<int>(type: "int", nullable: false),
                    TenKH = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChiKH = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaVaiTro = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.MaNhanVien);
                    table.ForeignKey(
                        name: "FK_Users_Roles_MaVaiTro",
                        column: x => x.MaVaiTro,
                        principalTable: "Roles",
                        principalColumn: "MaVaiTro",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryReceiptDetails",
                columns: table => new
                {
                    MaPhieuNhapKho = table.Column<int>(type: "int", nullable: false),
                    MaHang = table.Column<int>(type: "int", nullable: false),
                    SoLuongNhap = table.Column<int>(type: "int", nullable: false),
                    DonGiaNhap = table.Column<int>(type: "int", nullable: false),
                    ThanhTien = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptDetails", x => new { x.MaPhieuNhapKho, x.MaHang });
                    table.ForeignKey(
                        name: "FK_InventoryReceiptDetails_InventoryReceipts_MaPhieuNhapKho",
                        column: x => x.MaPhieuNhapKho,
                        principalTable: "InventoryReceipts",
                        principalColumn: "MaPhieuNhapKho",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptDetails_Products_MaHang",
                        column: x => x.MaHang,
                        principalTable: "Products",
                        principalColumn: "MaHang",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    MaHang = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGiaBan = table.Column<int>(type: "int", nullable: false),
                    ThanhTien = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => new { x.MaDonHang, x.MaHang });
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "Orders",
                        principalColumn: "MaDonHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Products_MaHang",
                        column: x => x.MaHang,
                        principalTable: "Products",
                        principalColumn: "MaHang",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "MaNhaCungCap", "TenNCC" },
                values: new object[,]
                {
                    { 1, "Nha Cung Cap A" },
                    { 2, "Nha Cung Cap B" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "MaVaiTro", "TenVaiTro" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "NhanVien" }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "MaKH", "DiaChiKH", "Email", "NgayDangKy", "NgaySinh", "SoDienThoai", "TenKH" },
                values: new object[,]
                {
                    { 1, "12 Le Loi, Da Nang", "kh.b@example.com", new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1995, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "0900000002", "Tran Thi B" },
                    { 2, "99 Tran Hung Dao, Ho Chi Minh", "kh.c@example.com", new DateTime(2024, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1988, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "0900000003", "Pham Van C" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "MaHang", "MaLoai", "MaNhaCungCap", "SoLuongTon", "TenHang" },
                values: new object[,]
                {
                    { 1, "PC", 1, 50, "Ban Phim Co" },
                    { 2, "ACC", 2, 40, "Chuot Khong Day" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "MaNhanVien", "DiaChiKH", "Email", "MaKH", "MaVaiTro", "NgayDangKy", "NgaySinh", "SoDienThoai", "TenKH", "TenNV" },
                values: new object[] { 1, "1 Nguyen Trai, Ha Noi", "nv.a@example.com", 1, 1, new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1990, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "0900000001", "Nguyen Van A", "Nguyen Van A" });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "MaDonHang", "MaKH", "MaNhanVien", "NgayLap", "TongTien" },
                values: new object[] { 1, 1, 1, new DateTime(2024, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1500000m });

            migrationBuilder.InsertData(
                table: "InventoryReceipts",
                columns: new[] { "MaPhieuNhapKho", "MaNhaCungCap", "MaNhanVien", "NgayNhapKho", "TongTien" },
                values: new object[] { 1, 1, 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 900000 });

            migrationBuilder.InsertData(
                table: "OrderDetails",
                columns: new[] { "MaDonHang", "MaHang", "DonGiaBan", "SoLuong", "ThanhTien" },
                values: new object[,]
                {
                    { 1, 1, 1000000, 1, 1000000 },
                    { 1, 2, 500000, 1, 500000 }
                });

            migrationBuilder.InsertData(
                table: "InventoryReceiptDetails",
                columns: new[] { "MaPhieuNhapKho", "MaHang", "DonGiaNhap", "SoLuongNhap", "ThanhTien" },
                values: new object[] { 1, 1, 90000, 10, 900000 });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptDetails_MaHang",
                table: "InventoryReceiptDetails",
                column: "MaHang");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_MaHang",
                table: "OrderDetails",
                column: "MaHang");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_MaKH",
                table: "Orders",
                column: "MaKH");

            migrationBuilder.CreateIndex(
                name: "IX_Products_MaNhaCungCap",
                table: "Products",
                column: "MaNhaCungCap");

            migrationBuilder.CreateIndex(
                name: "IX_Users_MaVaiTro",
                table: "Users",
                column: "MaVaiTro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryReceiptDetails");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "InventoryReceipts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}

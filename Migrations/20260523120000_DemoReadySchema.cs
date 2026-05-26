using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using quanlybanhang_nmcnpm.Database;

#nullable disable

namespace quanlybanhang_nmcnpm.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260523120000_DemoReadySchema")]
    public partial class DemoReadySchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaSanPham",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE Products SET MaSanPham = CONCAT('SP', RIGHT('000' + CAST(MaHang AS varchar(10)), 3)) WHERE MaSanPham = ''");

            migrationBuilder.AlterColumn<string>(
                name: "TenHang",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DonViTinh",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "GiaBan",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Products_MaSanPham",
                table: "Products",
                column: "MaSanPham",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenHang",
                table: "Products",
                column: "TenHang",
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "DiemTichLuy",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgaySinh",
                table: "Customers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SoDienThoai",
                table: "Customers",
                column: "SoDienThoai",
                unique: true);

            migrationBuilder.AddColumn<string>(
                name: "TenDangNhap",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE Users SET TenDangNhap = CONCAT('user', MaNhanVien) WHERE TenDangNhap = ''");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgaySinh",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.DropColumn(
                name: "MaKH",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenKH",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DiaChiKH",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenDangNhap",
                table: "Users",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TamTinh",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GiamGia",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "Orders",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TienVat",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "DonGiaBan",
                table: "OrderDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "ThanhTien",
                table: "OrderDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "NguoiGiao",
                table: "InventoryReceipts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "InventoryReceipts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "TongTien",
                table: "InventoryReceipts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipts_MaNhaCungCap",
                table: "InventoryReceipts",
                column: "MaNhaCungCap");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceipts_Categories_MaNhaCungCap",
                table: "InventoryReceipts",
                column: "MaNhaCungCap",
                principalTable: "Categories",
                principalColumn: "MaNhaCungCap",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AlterColumn<decimal>(
                name: "DonGiaNhap",
                table: "InventoryReceiptDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "ThanhTien",
                table: "InventoryReceiptDetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceipts_Categories_MaNhaCungCap",
                table: "InventoryReceipts");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceipts_MaNhaCungCap",
                table: "InventoryReceipts");

            migrationBuilder.DropIndex(
                name: "IX_Products_MaSanPham",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenHang",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Customers_SoDienThoai",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenDangNhap",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MaSanPham",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DonViTinh",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GiaBan",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiemTichLuy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenDangNhap",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TamTinh",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GiamGia",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TienVat",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NguoiGiao",
                table: "InventoryReceipts");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "InventoryReceipts");

            migrationBuilder.AlterColumn<string>(
                name: "TenHang",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgaySinh",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1900, 1, 1),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "MaKH",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TenKH",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DiaChiKH",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgaySinh",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1900, 1, 1),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DonGiaBan",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "ThanhTien",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "TongTien",
                table: "InventoryReceipts",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "DonGiaNhap",
                table: "InventoryReceiptDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "ThanhTien",
                table: "InventoryReceiptDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}

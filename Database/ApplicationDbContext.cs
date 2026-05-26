using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Models;

namespace quanlybanhang_nmcnpm.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<InventoryReceipt> InventoryReceipts { get; set; }
    public DbSet<InventoryReceiptDetail> InventoryReceiptDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User -> Role relationship
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.MaVaiTro)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Product -> Category relationship
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.MaNhaCungCap)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Order -> Customer relationship
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.MaKH)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure OrderDetail -> Order relationship
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.MaDonHang)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure OrderDetail -> Product relationship
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.MaHang)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure InventoryReceiptDetail -> InventoryReceipt relationship
        modelBuilder.Entity<InventoryReceiptDetail>()
            .HasOne(ird => ird.InventoryReceipt)
            .WithMany(ir => ir.InventoryReceiptDetails)
            .HasForeignKey(ird => ird.MaPhieuNhapKho)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure InventoryReceiptDetail -> Product relationship
        modelBuilder.Entity<InventoryReceiptDetail>()
            .HasOne(ird => ird.Product)
            .WithMany(p => p.InventoryReceiptDetails)
            .HasForeignKey(ird => ird.MaHang)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InventoryReceipt>()
            .HasOne(ir => ir.Category)
            .WithMany(c => c.InventoryReceipts)
            .HasForeignKey(ir => ir.MaNhaCungCap)
            .OnDelete(DeleteBehavior.Restrict);

        // Set primary keys
        modelBuilder.Entity<User>().HasKey(u => u.MaNhanVien);
        modelBuilder.Entity<Role>().HasKey(r => r.MaVaiTro);
        modelBuilder.Entity<Product>().HasKey(p => p.MaHang);
        modelBuilder.Entity<Category>().HasKey(c => c.MaNhaCungCap);
        modelBuilder.Entity<Customer>().HasKey(c => c.MaKH);
        modelBuilder.Entity<Order>().HasKey(o => o.MaDonHang);
        modelBuilder.Entity<OrderDetail>().HasKey(od => new { od.MaDonHang, od.MaHang });
        modelBuilder.Entity<InventoryReceipt>().HasKey(ir => ir.MaPhieuNhapKho);
        modelBuilder.Entity<InventoryReceiptDetail>().HasKey(ird => new { ird.MaPhieuNhapKho, ird.MaHang });

        modelBuilder.Entity<Role>().HasData(
            new Role { MaVaiTro = 1, TenVaiTro = "Admin" },
            new Role { MaVaiTro = 2, TenVaiTro = "NhanVien" });

        modelBuilder.Entity<User>().HasData(
            new User
            {
                MaNhanVien = 1,
                TenNV = "Nguyen Van A",
                NgaySinh = new DateTime(1990, 5, 20),
                NgayDangKy = new DateTime(2024, 1, 5),
                MaKH = 1,
                TenKH = "Nguyen Van A",
                DiaChiKH = "1 Nguyen Trai, Ha Noi",
                SoDienThoai = "0900000001",
                Email = "nv.a@example.com",
                MaVaiTro = 1
            });

        modelBuilder.Entity<Category>().HasData(
            new Category { MaNhaCungCap = 1, TenNCC = "Nha Cung Cap A" },
            new Category { MaNhaCungCap = 2, TenNCC = "Nha Cung Cap B" });

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                MaHang = 1,
                TenHang = "Ban Phim Co",
                SoLuongTon = 50,
                MaNhaCungCap = 1,
                MaLoai = "PC"
            },
            new Product
            {
                MaHang = 2,
                TenHang = "Chuot Khong Day",
                SoLuongTon = 40,
                MaNhaCungCap = 2,
                MaLoai = "ACC"
            });

        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                MaKH = 1,
                TenKH = "Tran Thi B",
                NgaySinh = new DateTime(1995, 8, 10),
                NgayDangKy = new DateTime(2024, 1, 10),
                DiaChiKH = "12 Le Loi, Da Nang",
                SoDienThoai = "0900000002",
                Email = "kh.b@example.com"
            },
            new Customer
            {
                MaKH = 2,
                TenKH = "Pham Van C",
                NgaySinh = new DateTime(1988, 3, 15),
                NgayDangKy = new DateTime(2024, 2, 2),
                DiaChiKH = "99 Tran Hung Dao, Ho Chi Minh",
                SoDienThoai = "0900000003",
                Email = "kh.c@example.com"
            });

        modelBuilder.Entity<Order>().HasData(
            new Order
            {
                MaDonHang = 1,
                MaKH = 1,
                MaNhanVien = 1,
                NgayLap = new DateTime(2024, 2, 5),
                TongTien = 1500000m
            });

        modelBuilder.Entity<OrderDetail>().HasData(
            new OrderDetail
            {
                MaDonHang = 1,
                MaHang = 1,
                SoLuong = 1,
                DonGiaBan = 1000000,
                ThanhTien = 1000000
            },
            new OrderDetail
            {
                MaDonHang = 1,
                MaHang = 2,
                SoLuong = 1,
                DonGiaBan = 500000,
                ThanhTien = 500000
            });

        modelBuilder.Entity<InventoryReceipt>().HasData(
            new InventoryReceipt
            {
                MaPhieuNhapKho = 1,
                NgayNhapKho = new DateTime(2024, 1, 15),
                TongTien = 900000,
                MaNhanVien = 1,
                MaNhaCungCap = 1
            });

        modelBuilder.Entity<InventoryReceiptDetail>().HasData(
            new InventoryReceiptDetail
            {
                MaPhieuNhapKho = 1,
                MaHang = 1,
                SoLuongNhap = 10,
                DonGiaNhap = 90000,
                ThanhTien = 900000
            });
    }
}

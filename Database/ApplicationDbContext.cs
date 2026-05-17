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
    }
}

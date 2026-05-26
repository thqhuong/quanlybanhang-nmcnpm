using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Models;

namespace quanlybanhang_nmcnpm.Database;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        if (!await dbContext.Roles.AnyAsync())
        {
            dbContext.Roles.AddRange(
                new Role { TenVaiTro = "Admin" },
                new Role { TenVaiTro = "Cashier" },
                new Role { TenVaiTro = "Storekeeper" });
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.Categories.AnyAsync())
        {
            dbContext.Categories.AddRange(
                new Category { TenNCC = "Bánh kẹo" },
                new Category { TenNCC = "Sữa" },
                new Category { TenNCC = "Nước giải khát" },
                new Category { TenNCC = "Mì ăn liền" });
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.Users.AnyAsync())
        {
            var adminRoleId = await GetRoleIdAsync(dbContext, "Admin");
            var cashierRoleId = await GetRoleIdAsync(dbContext, "Cashier");
            var storekeeperRoleId = await GetRoleIdAsync(dbContext, "Storekeeper");
            var now = DateTime.Today;

            dbContext.Users.AddRange(
                new User
                {
                    TenDangNhap = "admin",
                    TenNV = "Trần Quản Trị",
                    NgayDangKy = now.AddDays(-30),
                    SoDienThoai = "0900000001",
                    Email = "admin@example.local",
                    MaVaiTro = adminRoleId,
                    IsActive = true,
                    LastLoginAt = DateTime.Now.AddHours(-2)
                },
                new User
                {
                    TenDangNhap = "cashier",
                    TenNV = "Nguyễn Thu Ngân",
                    NgayDangKy = now.AddDays(-20),
                    SoDienThoai = "0900000002",
                    Email = "cashier@example.local",
                    MaVaiTro = cashierRoleId,
                    IsActive = true,
                    LastLoginAt = DateTime.Now.AddHours(-4)
                },
                new User
                {
                    TenDangNhap = "storekeeper",
                    TenNV = "Phạm Thủ Kho",
                    NgayDangKy = now.AddDays(-15),
                    SoDienThoai = "0900000003",
                    Email = "storekeeper@example.local",
                    MaVaiTro = storekeeperRoleId,
                    IsActive = true,
                    LastLoginAt = DateTime.Now.AddHours(-1)
                });
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.Products.AnyAsync())
        {
            var banhKeoId = await GetCategoryIdAsync(dbContext, "Bánh kẹo");
            var suaId = await GetCategoryIdAsync(dbContext, "Sữa");
            var nuocId = await GetCategoryIdAsync(dbContext, "Nước giải khát");
            var miId = await GetCategoryIdAsync(dbContext, "Mì ăn liền");

            dbContext.Products.AddRange(
                new Product { MaSanPham = "SP001", TenHang = "Bánh quy bơ Danisa 454g", DonViTinh = "Hộp", GiaBan = 150000m, SoLuongTon = 45, MaNhaCungCap = banhKeoId, MaLoai = "BANHKEO" },
                new Product { MaSanPham = "SP002", TenHang = "Sữa tươi TH True Milk 1L", DonViTinh = "Hộp", GiaBan = 35000m, SoLuongTon = 120, MaNhaCungCap = suaId, MaLoai = "SUA" },
                new Product { MaSanPham = "SP003", TenHang = "Kẹo dẻo Chupa Chups", DonViTinh = "Gói", GiaBan = 25000m, SoLuongTon = 80, MaNhaCungCap = banhKeoId, MaLoai = "BANHKEO" },
                new Product { MaSanPham = "SP004", TenHang = "Nước ngọt Coca Cola 1.5L", DonViTinh = "Chai", GiaBan = 20000m, SoLuongTon = 50, MaNhaCungCap = nuocId, MaLoai = "NUOC" },
                new Product { MaSanPham = "SP005", TenHang = "Mì Hảo Hảo tôm chua cay", DonViTinh = "Thùng", GiaBan = 135000m, SoLuongTon = 15, MaNhaCungCap = miId, MaLoai = "MI" });
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.Customers.AnyAsync())
        {
            var now = DateTime.Today;
            dbContext.Customers.AddRange(
                new Customer { TenKH = "Khách lẻ", NgayDangKy = now, DiaChiKH = "", SoDienThoai = "0000000000", Email = "", DiemTichLuy = 0 },
                new Customer { TenKH = "Nguyễn Văn A", NgayDangKy = now.AddDays(-21), DiaChiKH = "Quận 1, TP.HCM", SoDienThoai = "0901234567", Email = "nguyenvana@example.local", DiemTichLuy = 150 },
                new Customer { TenKH = "Trần Thị B", NgayDangKy = now.AddDays(-18), DiaChiKH = "Quận 3, TP.HCM", SoDienThoai = "0912345678", Email = "tranthib@example.local", DiemTichLuy = 25 },
                new Customer { TenKH = "Lê Văn C", NgayDangKy = now.AddDays(-40), DiaChiKH = "Thủ Đức, TP.HCM", SoDienThoai = "0987654321", Email = "levanc@example.local", DiemTichLuy = 500 });
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.Orders.AnyAsync())
        {
            var customerId = await dbContext.Customers
                .Where(c => c.SoDienThoai == "0901234567")
                .Select(c => c.MaKH)
                .FirstAsync();
            var cashierId = await dbContext.Users
                .Where(u => u.TenDangNhap == "cashier")
                .Select(u => u.MaNhanVien)
                .FirstAsync();
            var product = await dbContext.Products.FirstAsync(p => p.MaSanPham == "SP002");
            const int quantity = 2;
            var subtotal = product.GiaBan * quantity;
            var vat = decimal.Round(subtotal * 0.08m, 2);

            dbContext.Orders.Add(new Order
            {
                MaKH = customerId,
                MaNhanVien = cashierId,
                NgayLap = DateTime.Now.AddDays(-1),
                TamTinh = subtotal,
                GiamGia = 0m,
                VatRate = 8m,
                TienVat = vat,
                TongTien = subtotal + vat,
                OrderDetails =
                {
                    new OrderDetail
                    {
                        MaHang = product.MaHang,
                        SoLuong = quantity,
                        DonGiaBan = product.GiaBan,
                        ThanhTien = subtotal
                    }
                }
            });
            await dbContext.SaveChangesAsync();
        }
    }

    private static Task<int> GetRoleIdAsync(ApplicationDbContext dbContext, string roleName)
    {
        return dbContext.Roles
            .Where(role => role.TenVaiTro == roleName)
            .Select(role => role.MaVaiTro)
            .FirstAsync();
    }

    private static Task<int> GetCategoryIdAsync(ApplicationDbContext dbContext, string categoryName)
    {
        return dbContext.Categories
            .Where(category => category.TenNCC == categoryName)
            .Select(category => category.MaNhaCungCap)
            .FirstAsync();
    }
}

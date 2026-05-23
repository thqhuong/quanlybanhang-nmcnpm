using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Services;
using Xunit;

namespace quanlybanhang_nmcnpm.Tests;

public sealed class OrderAndInventoryServiceTests
{
    [Fact]
    public async Task CreateOrderAsync_ComputesTotalsAndReducesStock()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var service = new OrderService(dbContext);
        var customerId = await dbContext.Customers
            .Where(c => c.SoDienThoai == "0000000000")
            .Select(c => c.MaKH)
            .FirstAsync();
        var employeeId = await dbContext.Users
            .Where(u => u.TenDangNhap == "cashier")
            .Select(u => u.MaNhanVien)
            .FirstAsync();
        var product = await dbContext.Products.FirstAsync(p => p.MaSanPham == "SP001");
        var originalStock = product.SoLuongTon;

        var result = await service.CreateOrderAsync(new CreateOrderInput(
            customerId,
            employeeId,
            10000m,
            8m,
            new[] { new OrderLineInput(product.MaHang, 2) }));

        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal((product.GiaBan * 2) - 10000m + (((product.GiaBan * 2) - 10000m) * 0.08m), result.Value!.Total);

        var updatedProduct = await dbContext.Products.FirstAsync(p => p.MaHang == product.MaHang);
        Assert.Equal(originalStock - 2, updatedProduct.SoLuongTon);
    }

    [Fact]
    public async Task CreateReceiptAsync_IncreasesStock()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var service = new InventoryService(dbContext);
        var supplierId = await dbContext.Categories
            .Select(c => c.MaNhaCungCap)
            .FirstAsync();
        var employeeId = await dbContext.Users
            .Where(u => u.TenDangNhap == "storekeeper")
            .Select(u => u.MaNhanVien)
            .FirstAsync();
        var product = await dbContext.Products.FirstAsync(p => p.MaSanPham == "SP002");
        var originalStock = product.SoLuongTon;

        var result = await service.CreateReceiptAsync(new CreateInventoryReceiptInput(
            supplierId,
            employeeId,
            "Nhà cung cấp demo",
            "Nhập kiểm thử",
            new[] { new InventoryReceiptLineInput(product.MaHang, 5, 20000m) }));

        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal(100000m, result.Value);

        var updatedProduct = await dbContext.Products.FirstAsync(p => p.MaHang == product.MaHang);
        Assert.Equal(originalStock + 5, updatedProduct.SoLuongTon);
    }
}

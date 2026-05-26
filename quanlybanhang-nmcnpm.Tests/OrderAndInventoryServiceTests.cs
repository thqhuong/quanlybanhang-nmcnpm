using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;
using quanlybanhang_nmcnpm.Services;
using Xunit;

namespace quanlybanhang_nmcnpm.Tests;

public sealed class OrderAndInventoryServiceTests
{
    [Fact]
    public async Task CreateOrderAsync_ComputesTotalsReducesStockAndBuildsReceipt()
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
        var expectedTotal = (product.GiaBan * 2) - 10000m + (((product.GiaBan * 2) - 10000m) * 0.08m);

        var result = await service.CreateOrderAsync(new CreateOrderInput(
            customerId,
            employeeId,
            10000m,
            8m,
            expectedTotal + 50000m,
            new[] { new OrderLineInput(product.MaHang, 2) }));

        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal(expectedTotal, result.Value!.Total);
        Assert.Equal(50000m, result.Value.Change);

        var updatedProduct = await dbContext.Products.FirstAsync(p => p.MaHang == product.MaHang);
        Assert.Equal(originalStock - 2, updatedProduct.SoLuongTon);

        var receipt = await service.GetReceiptAsync(result.Value.OrderId, result.Value.PaidAmount);
        Assert.NotNull(receipt);
        Assert.Equal(result.Value.OrderId, receipt!.OrderId);
        Assert.Single(receipt.Lines);
    }

    [Fact]
    public async Task CreateReceiptAsync_SavesReceiptDetailsAndIncreasesStock()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var service = new InventoryService(dbContext);
        var ids = await GetInventoryIdsAsync(dbContext);
        var originalReceiptCount = await dbContext.InventoryReceipts.CountAsync();

        var result = await service.CreateReceiptAsync(new CreateInventoryReceiptInput(
            ids.SupplierId,
            ids.EmployeeId,
            DateTime.Today,
            "Nhà cung cấp demo",
            "Nhập kiểm thử",
            new[] { new InventoryReceiptLineInput(ids.ProductId, 5, 20000m) }));

        Assert.True(result.IsValid, result.ErrorMessage);
        Assert.Equal(100000m, result.Value);

        var updatedProduct = await dbContext.Products.FirstAsync(product => product.MaHang == ids.ProductId);
        Assert.Equal(ids.OriginalStock + 5, updatedProduct.SoLuongTon);
        Assert.Equal(originalReceiptCount + 1, await dbContext.InventoryReceipts.CountAsync());

        var savedReceipt = await dbContext.InventoryReceipts
            .Include(receipt => receipt.InventoryReceiptDetails)
            .SingleAsync(receipt => receipt.GhiChu == "Nhập kiểm thử");
        Assert.Equal(100000m, savedReceipt.TongTien);

        var detail = Assert.Single(savedReceipt.InventoryReceiptDetails);
        Assert.Equal(ids.ProductId, detail.MaHang);
        Assert.Equal(5, detail.SoLuongNhap);
        Assert.Equal(20000m, detail.DonGiaNhap);
        Assert.Equal(100000m, detail.ThanhTien);
    }

    [Fact]
    public async Task CreateReceiptAsync_RejectsInvalidInputsWithoutMutatingStock()
    {
        await AssertInvalidReceiptAsync(ids => new CreateInventoryReceiptInput(
            -1,
            ids.EmployeeId,
            DateTime.Today,
            "Nhà cung cấp demo",
            "",
            new[] { new InventoryReceiptLineInput(ids.ProductId, 1, 1000m) }));

        await AssertInvalidReceiptAsync(ids => new CreateInventoryReceiptInput(
            ids.SupplierId,
            -1,
            DateTime.Today,
            "Nhà cung cấp demo",
            "",
            new[] { new InventoryReceiptLineInput(ids.ProductId, 1, 1000m) }));

        await AssertInvalidReceiptAsync(ids => new CreateInventoryReceiptInput(
            ids.SupplierId,
            ids.EmployeeId,
            DateTime.Today,
            "Nhà cung cấp demo",
            "",
            Array.Empty<InventoryReceiptLineInput>()));

        await AssertInvalidReceiptAsync(ids => new CreateInventoryReceiptInput(
            ids.SupplierId,
            ids.EmployeeId,
            DateTime.Today,
            "Nhà cung cấp demo",
            "",
            new[] { new InventoryReceiptLineInput(-1, 1, 1000m) }));

        await AssertInvalidReceiptAsync(ids => new CreateInventoryReceiptInput(
            ids.SupplierId,
            ids.EmployeeId,
            DateTime.Today,
            "Nhà cung cấp demo",
            "",
            new[] { new InventoryReceiptLineInput(ids.ProductId, 0, 1000m) }));

        await AssertInvalidReceiptAsync(ids => new CreateInventoryReceiptInput(
            ids.SupplierId,
            ids.EmployeeId,
            DateTime.Today,
            "Nhà cung cấp demo",
            "",
            new[] { new InventoryReceiptLineInput(ids.ProductId, 1, 0m) }));

        await AssertInvalidReceiptAsync(ids => new CreateInventoryReceiptInput(
            ids.SupplierId,
            ids.EmployeeId,
            DateTime.Today,
            "Nhà cung cấp demo",
            "",
            new[]
            {
                new InventoryReceiptLineInput(ids.ProductId, 1, 1000m),
                new InventoryReceiptLineInput(ids.ProductId, 2, 1000m)
            }));
    }

    [Fact]
    public async Task GetLowStockAsync_ReturnsLowStockProductsSortedByStock()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var service = new InventoryService(dbContext);

        var items = await service.GetLowStockAsync();

        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.True(item.Stock <= 20));
        Assert.Equal(items.Select(item => item.Stock).OrderBy(stock => stock), items.Select(item => item.Stock));
        Assert.Contains(items, item => item.ProductCode == "SP005");
    }

    private static async Task AssertInvalidReceiptAsync(Func<InventoryIds, CreateInventoryReceiptInput> createInput)
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var service = new InventoryService(dbContext);
        var ids = await GetInventoryIdsAsync(dbContext);
        var originalReceiptCount = await dbContext.InventoryReceipts.CountAsync();

        var result = await service.CreateReceiptAsync(createInput(ids));

        Assert.False(result.IsValid);
        Assert.Equal(originalReceiptCount, await dbContext.InventoryReceipts.CountAsync());

        var product = await dbContext.Products.FirstAsync(product => product.MaHang == ids.ProductId);
        Assert.Equal(ids.OriginalStock, product.SoLuongTon);
    }

    private static async Task<InventoryIds> GetInventoryIdsAsync(ApplicationDbContext dbContext)
    {
        var supplierId = await dbContext.Categories
            .Select(category => category.MaNhaCungCap)
            .FirstAsync();
        var employeeId = await dbContext.Users
            .Where(user => user.TenDangNhap == "storekeeper")
            .Select(user => user.MaNhanVien)
            .FirstAsync();
        var product = await dbContext.Products.FirstAsync(p => p.MaSanPham == "SP002");
        return new InventoryIds(supplierId, employeeId, product.MaHang, product.SoLuongTon);
    }

    private sealed record InventoryIds(int SupplierId, int EmployeeId, int ProductId, int OriginalStock);
}

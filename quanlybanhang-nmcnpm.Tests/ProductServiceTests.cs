using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Services;
using Xunit;

namespace quanlybanhang_nmcnpm.Tests;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsDuplicateCode()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var service = new ProductService(dbContext);
        var category = await dbContext.Categories.FirstAsync();

        var result = await service.CreateAsync(new ProductInput(
            "SP001",
            "Bánh mới",
            "Hộp",
            10000m,
            10,
            category.MaNhaCungCap,
            "TEST"));

        Assert.False(result.IsValid);
        Assert.Contains("Mã sản phẩm", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateUpdateDeleteAsync_PersistsProduct()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var service = new ProductService(dbContext);
        var category = await dbContext.Categories.FirstAsync();

        var create = await service.CreateAsync(new ProductInput(
            "SP999",
            "Sản phẩm kiểm thử",
            "Cái",
            12000m,
            5,
            category.MaNhaCungCap,
            "TEST"));

        Assert.True(create.IsValid, create.ErrorMessage);

        var update = await service.UpdateAsync(create.Value!.Id, new ProductInput(
            "SP999",
            "Sản phẩm kiểm thử cập nhật",
            "Cái",
            15000m,
            7,
            category.MaNhaCungCap,
            "TEST"));

        Assert.True(update.IsValid, update.ErrorMessage);
        Assert.Equal(15000m, update.Value!.Price);
        Assert.Equal(7, update.Value.Stock);

        var delete = await service.DeleteAsync(create.Value.Id);
        Assert.True(delete.IsValid, delete.ErrorMessage);
        Assert.False(await dbContext.Products.AnyAsync(p => p.MaSanPham == "SP999"));
    }
}

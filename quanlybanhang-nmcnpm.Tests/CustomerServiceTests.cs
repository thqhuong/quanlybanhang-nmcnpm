using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Services;
using Xunit;

namespace quanlybanhang_nmcnpm.Tests;

public sealed class CustomerServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsDuplicatePhone()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var service = new CustomerService(dbContext);

        var result = await service.CreateAsync(new CustomerInput(
            "Khách trùng",
            "0901234567",
            "duplicate@example.local",
            "TP.HCM",
            null,
            0));

        Assert.False(result.IsValid);
        Assert.Contains("Số điện thoại", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateUpdateDeleteAsync_PersistsCustomer()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var service = new CustomerService(dbContext);

        var create = await service.CreateAsync(new CustomerInput(
            "Khách kiểm thử",
            "0999999999",
            "test@example.local",
            "Hà Nội",
            null,
            10));

        Assert.True(create.IsValid, create.ErrorMessage);

        var update = await service.UpdateAsync(create.Value!.Id, new CustomerInput(
            "Khách kiểm thử cập nhật",
            "0999999999",
            "updated@example.local",
            "Đà Nẵng",
            null,
            20));

        Assert.True(update.IsValid, update.ErrorMessage);
        Assert.Equal(20, update.Value!.Points);

        var delete = await service.DeleteAsync(create.Value.Id);
        Assert.True(delete.IsValid, delete.ErrorMessage);
        Assert.False(await dbContext.Customers.AnyAsync(c => c.SoDienThoai == "0999999999"));
    }
}

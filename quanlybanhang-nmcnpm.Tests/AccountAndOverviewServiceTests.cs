using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Services;
using Xunit;

namespace quanlybanhang_nmcnpm.Tests;

public sealed class AccountAndOverviewServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidatesAndPersistsAccount()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var service = new AccountService(dbContext);
        var roleId = await dbContext.Roles
            .Where(role => role.TenVaiTro == "Cashier")
            .Select(role => role.MaVaiTro)
            .FirstAsync();

        var invalid = await service.CreateAsync(new AccountInput(
            "x",
            "Tài khoản lỗi",
            "abc",
            "bad-email",
            roleId,
            true,
            "admin"));
        Assert.False(invalid.IsValid);

        var created = await service.CreateAsync(new AccountInput(
            "cashier.demo",
            "Thu ngân demo",
            "0909999999",
            "cashier.demo@example.local",
            roleId,
            true,
            "admin"));

        Assert.True(created.IsValid, created.ErrorMessage);
        Assert.Equal("cashier.demo", created.Value!.Username);

        var locked = await service.SetActiveAsync(created.Value.Id, false);
        Assert.True(locked.IsValid, locked.ErrorMessage);
        Assert.False(await dbContext.Users.Where(user => user.MaNhanVien == created.Value.Id).Select(user => user.IsActive).FirstAsync());
    }

    [Fact]
    public async Task GetMetricsAsync_ReturnsReportData()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var service = new OverviewService(dbContext);

        var metrics = await service.GetMetricsAsync(DateTime.Today.AddDays(-7), DateTime.Today);

        Assert.True(metrics.OrderCount >= 1);
        Assert.True(metrics.Revenue > 0);
        Assert.NotEmpty(metrics.TopProducts);
        Assert.NotEmpty(metrics.LowStockItems);
    }

    [Fact]
    public async Task Cashier_CannotReadAccounts()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var session = new UserSessionService();
        session.Start(new UserSession(2, "cashier", "Cashier", RoleNames.Cashier, 2));
        var service = new AccountService(dbContext, session);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetAllAsync());
    }

    [Fact]
    public async Task Storekeeper_CannotReadSalesOrOverview()
    {
        await using var dbContext = await ServiceTestFixture.CreateSeededDbContextAsync();
        var session = new UserSessionService();
        session.Start(new UserSession(3, "storekeeper", "Storekeeper", RoleNames.Storekeeper, 3));

        var customerService = new CustomerService(dbContext, session);
        var overviewService = new OverviewService(dbContext, session);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => customerService.GetAllAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => overviewService.GetMetricsAsync());
    }
}

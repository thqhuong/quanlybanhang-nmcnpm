using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;
using Xunit;

namespace quanlybanhang_nmcnpm.Tests;

public sealed class DatabaseSeederTests
{
    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        await using var dbContext = ServiceTestFixture.CreateDbContext();

        await DatabaseSeeder.SeedAsync(dbContext);
        await DatabaseSeeder.SeedAsync(dbContext);

        Assert.Equal(3, await dbContext.Roles.CountAsync());
        Assert.Equal(3, await dbContext.Users.CountAsync());
        Assert.True(await dbContext.Products.CountAsync() >= 5);
        Assert.True(await dbContext.Customers.CountAsync() >= 4);
        Assert.Single(await dbContext.Users.Where(u => u.TenDangNhap == "admin").ToListAsync());
    }
}

using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;

namespace quanlybanhang_nmcnpm.Tests;

internal static class ServiceTestFixture
{
    public static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public static async Task<ApplicationDbContext> CreateSeededDbContextAsync()
    {
        var dbContext = CreateDbContext();
        await DatabaseSeeder.SeedAsync(dbContext);
        return dbContext;
    }
}

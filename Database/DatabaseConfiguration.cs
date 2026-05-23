using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace quanlybanhang_nmcnpm.Database;

public static class DatabaseConfiguration
{
    public const string ConnectionStringEnvironmentVariable = "QLBH_CONNECTION_STRING";
    public const string DefaultConnectionString =
        "Server=localhost;Database=QuanLyBanHang;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;";

    public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        var connectionString = GetConnectionString();

        services.AddDbContext<ApplicationDbContext>(
            options => options.UseSqlServer(connectionString),
            ServiceLifetime.Transient);

        return services;
    }

    public static string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)
            ?? DefaultConnectionString;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(dbContext);
    }
}

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
        var environmentConnectionString = Environment.GetEnvironmentVariable("QLBH_CONNECTION_STRING");
        var configConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
        var connectionString = !string.IsNullOrWhiteSpace(environmentConnectionString)
            ? environmentConnectionString
            : !string.IsNullOrWhiteSpace(configConnectionString)
                ? configConnectionString
                : throw new InvalidOperationException(
                    "Connection string not configured. Set QLBH_CONNECTION_STRING or DefaultConnection in App.config.");

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

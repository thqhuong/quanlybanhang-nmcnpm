using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace quanlybanhang_nmcnpm.Database;

public static class DatabaseConfiguration
{
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

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}

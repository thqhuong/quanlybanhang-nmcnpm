using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Configuration;

namespace quanlybanhang_nmcnpm.Database;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable("QLBH_CONNECTION_STRING");
        var configuredConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
        var connectionString = !string.IsNullOrWhiteSpace(environmentConnectionString)
            ? environmentConnectionString
            : !string.IsNullOrWhiteSpace(configuredConnectionString)
                ? configuredConnectionString
                : throw new InvalidOperationException(
                    "Connection string not configured. Set QLBH_CONNECTION_STRING or DefaultConnection in App.config.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(DatabaseConfiguration.GetConnectionString());

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

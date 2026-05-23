using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;

namespace quanlybanhang_nmcnpm.Services;

public sealed class OverviewService : IOverviewService
{
    private readonly ApplicationDbContext _dbContext;

    public OverviewService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OverviewMetrics> GetMetricsAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var todayOrders = _dbContext.Orders
            .Where(o => o.NgayLap >= today && o.NgayLap < tomorrow);

        var revenue = await todayOrders.SumAsync(o => (decimal?)o.TongTien) ?? 0m;
        var orderCount = await todayOrders.CountAsync();
        var lowStock = await _dbContext.Products.CountAsync(p => p.SoLuongTon <= 20);
        var newCustomers = await _dbContext.Customers.CountAsync(c => c.NgayDangKy >= monthStart);

        return new OverviewMetrics(revenue, orderCount, lowStock, newCustomers);
    }
}

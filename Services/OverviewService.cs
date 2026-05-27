using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;

namespace quanlybanhang_nmcnpm.Services;

public sealed class OverviewService : IOverviewService
{
    private readonly int _lowStockThreshold;
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserSessionService? _sessionService;

    public OverviewService(ApplicationDbContext dbContext, IUserSessionService? sessionService = null)
    {
        _dbContext = dbContext;
        _sessionService = sessionService;
        _lowStockThreshold = Get_lowStockThreshold();
    }

    private static int Get_lowStockThreshold()
    {
        var envValue = Environment.GetEnvironmentVariable("QLBH_LOW_STOCK_THRESHOLD");
        return int.TryParse(envValue, out var threshold) && threshold > 0 ? threshold : 20;
    }

    public async Task<OverviewMetrics> GetMetricsAsync(DateTime? from = null, DateTime? to = null)
    {
        if (_sessionService is not null && !_sessionService.IsInRole(RoleNames.Admin, RoleNames.Cashier))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem tổng quan.");
        }

        var start = (from ?? DateTime.Today).Date;
        var end = (to ?? DateTime.Today).Date;
        if (end < start)
        {
            (start, end) = (end, start);
        }

        var exclusiveEnd = end.AddDays(1);
        var orders = _dbContext.Orders
            .Where(order => order.NgayLap >= start && order.NgayLap < exclusiveEnd);

        var revenue = await orders.SumAsync(order => (decimal?)order.TongTien) ?? 0m;
        var orderCount = await orders.CountAsync();
        var averageOrderValue = orderCount == 0 ? 0m : decimal.Round(revenue / orderCount, 2);
        var lowStockCount = await _dbContext.Products.CountAsync(product => product.SoLuongTon <= _lowStockThreshold);
        var newCustomers = await _dbContext.Customers.CountAsync(customer =>
            customer.NgayDangKy >= start && customer.NgayDangKy < exclusiveEnd);

        var salesLines = await _dbContext.OrderDetails
            .AsNoTracking()
            .Include(detail => detail.Order)
            .Include(detail => detail.Product)
            .Where(detail => detail.Order != null &&
                detail.Order.NgayLap >= start &&
                detail.Order.NgayLap < exclusiveEnd)
            .ToListAsync();

        var topProducts = salesLines
            .Where(detail => detail.Product != null)
            .GroupBy(detail => new
            {
                detail.MaHang,
                detail.Product!.MaSanPham,
                detail.Product.TenHang
            })
            .Select(group => new TopProductReportItem(
                group.Key.MaSanPham,
                group.Key.TenHang,
                group.Sum(detail => detail.SoLuong),
                group.Sum(detail => detail.ThanhTien)))
            .OrderByDescending(item => item.QuantitySold)
            .ThenBy(item => item.ProductCode)
            .Take(5)
            .ToList();

        var lowStockItems = await _dbContext.Products
            .Where(product => product.SoLuongTon <= _lowStockThreshold)
            .OrderBy(product => product.SoLuongTon)
            .ThenBy(product => product.MaSanPham)
            .Select(product => new LowStockReportItem(
                product.MaSanPham,
                product.TenHang,
                product.SoLuongTon,
                product.DonViTinh))
            .Take(10)
            .ToListAsync();

        var recentOrders = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Where(o => o.NgayLap >= start && o.NgayLap < exclusiveEnd)
            .OrderByDescending(o => o.NgayLap)
            .ThenByDescending(o => o.MaDonHang)
            .Take(20)
            .Select(o => new RecentOrderItem(
                o.MaDonHang,
                o.Customer != null ? o.Customer.TenKH : "(Khách lẻ)",
                o.NgayLap,
                o.TongTien))
            .ToListAsync();

        var rawOrders = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.NgayLap >= start && o.NgayLap < exclusiveEnd)
            .Select(o => new { o.NgayLap, o.TongTien })
            .ToListAsync();

        var dailyRevenue = rawOrders
            .GroupBy(o => o.NgayLap.Date)
            .Select(g => new DailyRevenueItem(g.Key, g.Sum(o => o.TongTien)))
            .OrderBy(d => d.Date)
            .ToList();

        return new OverviewMetrics(
            start,
            end,
            revenue,
            orderCount,
            averageOrderValue,
            lowStockCount,
            newCustomers,
            topProducts,
            lowStockItems,
            recentOrders,
            dailyRevenue);
    }
}

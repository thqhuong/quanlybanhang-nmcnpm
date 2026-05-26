using quanlybanhang_nmcnpm.Models;

namespace quanlybanhang_nmcnpm.Services;

public sealed record ValidationResult(bool IsValid, string? ErrorMessage = null)
{
    public static ValidationResult Success() => new(true);

    public static ValidationResult Failure(string message) => new(false, message);
}

public sealed record ValidationResult<T>(bool IsValid, T? Value = default, string? ErrorMessage = null)
{
    public static ValidationResult<T> Success(T value) => new(true, value);

    public static ValidationResult<T> Failure(string message) => new(false, default, message);
}

public sealed record CategoryOption(int Id, string Name);

public sealed record ProductInput(
    string Code,
    string Name,
    string Unit,
    decimal Price,
    int Stock,
    int CategoryId,
    string CategoryCode);

public sealed record ProductListItem(
    int Id,
    string Code,
    string Name,
    string Category,
    string Unit,
    decimal Price,
    int Stock);

public sealed record CustomerInput(
    string Name,
    string Phone,
    string Email,
    string Address,
    DateTime? BirthDate,
    int Points);

public sealed record CustomerListItem(
    int Id,
    string Name,
    string Phone,
    string Email,
    string Address,
    int Points,
    DateTime? LastPurchase);

public sealed record OrderLineInput(int ProductId, int Quantity);

public sealed record CreateOrderInput(
    int CustomerId,
    int EmployeeId,
    decimal Discount,
    decimal VatRate,
    decimal PaidAmount,
    IReadOnlyCollection<OrderLineInput> Lines);

public sealed record OrderSummary(
    int OrderId,
    decimal Subtotal,
    decimal Discount,
    decimal Vat,
    decimal Total,
    decimal PaidAmount,
    decimal Change);

public sealed record OrderReceipt(
    int OrderId,
    DateTime CreatedAt,
    string CashierName,
    string CustomerName,
    decimal Subtotal,
    decimal Discount,
    decimal Vat,
    decimal Total,
    decimal PaidAmount,
    decimal Change,
    IReadOnlyList<OrderReceiptLine> Lines);

public sealed record OrderReceiptLine(
    string ProductCode,
    string ProductName,
    string Unit,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record InventoryReceiptLineInput(int ProductId, int Quantity, decimal UnitCost);

public sealed record CreateInventoryReceiptInput(
    int SupplierId,
    int EmployeeId,
    string DeliveredBy,
    string Note,
    IReadOnlyCollection<InventoryReceiptLineInput> Lines);

public sealed record AccountInput(
    string Username,
    string FullName,
    string Phone,
    string Email,
    int RoleId,
    bool IsActive,
    string Password);

public sealed record AccountListItem(
    int Id,
    string Username,
    string FullName,
    string Phone,
    string Email,
    string Role,
    bool IsActive,
    DateTime? LastLoginAt)
{
    public string ActiveStatus => IsActive ? "Hoạt động" : "Đã khóa";
}

public sealed record LoginInput(string Username, string Password);

public sealed record UserSession(
    int Id,
    string Username,
    string FullName,
    string Role,
    int RoleId);

public sealed record OverviewMetrics(
    DateTime From,
    DateTime To,
    decimal Revenue,
    int OrderCount,
    decimal AverageOrderValue,
    int LowStockProducts,
    int NewCustomers,
    IReadOnlyList<TopProductReportItem> TopProducts,
    IReadOnlyList<LowStockReportItem> LowStockItems);

public sealed record TopProductReportItem(
    string ProductCode,
    string ProductName,
    int QuantitySold,
    decimal Revenue);

public sealed record LowStockReportItem(
    string ProductCode,
    string ProductName,
    int Stock,
    string Unit);

public static class ServiceModelMapping
{
    public static ProductListItem ToListItem(this Product product)
    {
        return new ProductListItem(
            product.MaHang,
            product.MaSanPham,
            product.TenHang,
            product.Category?.TenNCC ?? "",
            product.DonViTinh,
            product.GiaBan,
            product.SoLuongTon);
    }

    public static CustomerListItem ToListItem(this Customer customer)
    {
        var lastPurchase = customer.Orders
            .OrderByDescending(o => o.NgayLap)
            .Select(o => (DateTime?)o.NgayLap)
            .FirstOrDefault();

        return new CustomerListItem(
            customer.MaKH,
            customer.TenKH,
            customer.SoDienThoai,
            customer.Email,
            customer.DiaChiKH,
            customer.DiemTichLuy,
            lastPurchase);
    }

    public static AccountListItem ToListItem(this User user)
    {
        return new AccountListItem(
            user.MaNhanVien,
            user.TenDangNhap,
            user.TenNV,
            user.SoDienThoai,
            user.Email,
            user.Role?.TenVaiTro ?? "",
            user.IsActive,
            user.LastLoginAt);
    }
}

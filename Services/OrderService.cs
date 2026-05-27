using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;
using quanlybanhang_nmcnpm.Models;

namespace quanlybanhang_nmcnpm.Services;

public sealed class OrderService : IOrderService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserSessionService? _sessionService;

    public OrderService(ApplicationDbContext dbContext, IUserSessionService? sessionService = null)
    {
        _dbContext = dbContext;
        _sessionService = sessionService;
    }

    public async Task<ValidationResult<OrderSummary>> CreateOrderAsync(CreateOrderInput input)
    {
        if (!HasSalesAccess())
        {
            return ValidationResult<OrderSummary>.Failure("Bạn không có quyền truy cập bán hàng.");
        }

        var validation = await ValidateAsync(input);
        if (!validation.IsValid)
        {
            return ValidationResult<OrderSummary>.Failure(validation.ErrorMessage!);
        }

        return await CreateOrderInternalAsync(input);
    }

    private async Task<ValidationResult<OrderSummary>> CreateOrderInternalAsync(CreateOrderInput input)
    {
        var productIds = input.Lines.Select(line => line.ProductId).Distinct().ToList();
        var products = await _dbContext.Products
            .Where(product => productIds.Contains(product.MaHang))
            .ToDictionaryAsync(product => product.MaHang);

        var subtotal = input.Lines.Sum(line => products[line.ProductId].GiaBan * line.Quantity);
        var discount = Math.Min(input.Discount, subtotal);
        var taxable = subtotal - discount;
        var vat = decimal.Round(taxable * (input.VatRate / 100m), 2);
        var total = taxable + vat;
        if (input.PaidAmount < total)
        {
            return ValidationResult<OrderSummary>.Failure("Số tiền khách thanh toán chưa đủ.");
        }

        var order = new Order
        {
            MaKH = input.CustomerId,
            MaNhanVien = input.EmployeeId,
            NgayLap = DateTime.Now,
            TamTinh = subtotal,
            GiamGia = discount,
            VatRate = input.VatRate,
            TienVat = vat,
            TongTien = total
        };

        foreach (var line in input.Lines)
        {
            var product = products[line.ProductId];
            if (product.SoLuongTon < line.Quantity)
            {
                return ValidationResult<OrderSummary>.Failure($"Sản phẩm {product.MaSanPham} không đủ tồn kho.");
            }
            product.SoLuongTon -= line.Quantity;
            order.OrderDetails.Add(new OrderDetail
            {
                MaHang = product.MaHang,
                SoLuong = line.Quantity,
                DonGiaBan = product.GiaBan,
                ThanhTien = product.GiaBan * line.Quantity
            });
        }

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var customer = await _dbContext.Customers.FindAsync(input.CustomerId);
        if (customer is not null)
        {
            var pointsEarned = (int)(total / 10000);
            customer.DiemTichLuy += pointsEarned;
            await _dbContext.SaveChangesAsync();
        }

        return ValidationResult<OrderSummary>.Success(new OrderSummary(
            order.MaDonHang,
            subtotal,
            discount,
            vat,
            total,
            input.PaidAmount,
            input.PaidAmount - total));
    }

    public async Task<OrderReceipt?> GetReceiptAsync(int orderId, decimal? paidAmount = null)
    {
        EnsureSalesAccess();

        var order = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.MaDonHang == orderId);
        if (order is null)
        {
            return null;
        }

        var cashierName = await _dbContext.Users
            .Where(user => user.MaNhanVien == order.MaNhanVien)
            .Select(user => user.TenNV)
            .FirstOrDefaultAsync() ?? "Thu ngân";

        var paid = paidAmount ?? order.TongTien;
        return new OrderReceipt(
            order.MaDonHang,
            order.NgayLap,
            cashierName,
            order.Customer?.TenKH ?? "Khách lẻ",
            order.TamTinh,
            order.GiamGia,
            order.TienVat,
            order.TongTien,
            paid,
            Math.Max(0m, paid - order.TongTien),
            order.OrderDetails
                .OrderBy(od => od.Product?.MaSanPham)
                .Select(od => new OrderReceiptLine(
                    od.Product?.MaSanPham ?? "",
                    od.Product?.TenHang ?? "",
                    od.Product?.DonViTinh ?? "",
                    od.SoLuong,
                    od.DonGiaBan,
                    od.ThanhTien))
                .ToList());
    }

    private async Task<ValidationResult> ValidateAsync(CreateOrderInput input)
    {
        if (!await _dbContext.Customers.AnyAsync(customer => customer.MaKH == input.CustomerId))
        {
            return ValidationResult.Failure("Khách hàng không hợp lệ.");
        }

        if (!await _dbContext.Users.AnyAsync(user => user.MaNhanVien == input.EmployeeId && user.IsActive))
        {
            return ValidationResult.Failure("Nhân viên không hợp lệ.");
        }

        if (input.Discount < 0)
        {
            return ValidationResult.Failure("Giảm giá không được âm.");
        }

        if (input.VatRate < 0)
        {
            return ValidationResult.Failure("VAT không được âm.");
        }

        if (input.PaidAmount < 0)
        {
            return ValidationResult.Failure("Số tiền khách thanh toán không được âm.");
        }

        if (input.Lines.Count == 0)
        {
            return ValidationResult.Failure("Đơn hàng cần ít nhất một sản phẩm.");
        }

        var duplicateProduct = input.Lines
            .GroupBy(line => line.ProductId)
            .Any(group => group.Count() > 1);
        if (duplicateProduct)
        {
            return ValidationResult.Failure("Mỗi sản phẩm chỉ nên xuất hiện một lần trong đơn hàng.");
        }

        if (input.Lines.Any(line => line.Quantity <= 0))
        {
            return ValidationResult.Failure("Số lượng bán phải lớn hơn 0.");
        }

        var productIds = input.Lines.Select(line => line.ProductId).Distinct().ToList();
        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.MaHang))
            .ToDictionaryAsync(p => p.MaHang);

        foreach (var line in input.Lines)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
            {
                return ValidationResult.Failure("Sản phẩm không hợp lệ.");
            }

            if (product.SoLuongTon < line.Quantity)
            {
                return ValidationResult.Failure($"Sản phẩm {product.MaSanPham} không đủ tồn kho.");
            }
        }

        return ValidationResult.Success();
    }

    private bool HasSalesAccess()
    {
        return _sessionService is null || _sessionService.IsInRole(RoleNames.Admin, RoleNames.Cashier);
    }

    private void EnsureSalesAccess()
    {
        if (!HasSalesAccess())
        {
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập bán hàng.");
        }
    }
}

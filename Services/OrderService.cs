using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;
using quanlybanhang_nmcnpm.Models;

namespace quanlybanhang_nmcnpm.Services;

public sealed class OrderService : IOrderService
{
    private readonly ApplicationDbContext _dbContext;

    public OrderService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ValidationResult<OrderSummary>> CreateOrderAsync(CreateOrderInput input)
    {
        var validation = await ValidateAsync(input);
        if (!validation.IsValid)
        {
            return ValidationResult<OrderSummary>.Failure(validation.ErrorMessage!);
        }

        var productIds = input.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.MaHang))
            .ToDictionaryAsync(p => p.MaHang);

        var subtotal = input.Lines.Sum(line => products[line.ProductId].GiaBan * line.Quantity);
        var discount = Math.Min(input.Discount, subtotal);
        var taxable = subtotal - discount;
        var vat = decimal.Round(taxable * (input.VatRate / 100m), 2);
        var total = taxable + vat;

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

        return ValidationResult<OrderSummary>.Success(new OrderSummary(
            order.MaDonHang,
            subtotal,
            discount,
            vat,
            total));
    }

    private async Task<ValidationResult> ValidateAsync(CreateOrderInput input)
    {
        if (!await _dbContext.Customers.AnyAsync(c => c.MaKH == input.CustomerId))
        {
            return ValidationResult.Failure("Khách hàng không hợp lệ.");
        }

        if (!await _dbContext.Users.AnyAsync(u => u.MaNhanVien == input.EmployeeId && u.IsActive))
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

        if (input.Lines.Count == 0)
        {
            return ValidationResult.Failure("Đơn hàng cần ít nhất một sản phẩm.");
        }

        var duplicateProduct = input.Lines
            .GroupBy(l => l.ProductId)
            .Any(g => g.Count() > 1);
        if (duplicateProduct)
        {
            return ValidationResult.Failure("Mỗi sản phẩm chỉ nên xuất hiện một lần trong đơn hàng.");
        }

        foreach (var line in input.Lines)
        {
            if (line.Quantity <= 0)
            {
                return ValidationResult.Failure("Số lượng bán phải lớn hơn 0.");
            }

            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaHang == line.ProductId);
            if (product is null)
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
}

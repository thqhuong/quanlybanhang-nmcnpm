using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;
using quanlybanhang_nmcnpm.Models;

namespace quanlybanhang_nmcnpm.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _dbContext;

    public InventoryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoryOption>> GetSuppliersAsync()
    {
        return await _dbContext.Categories
            .OrderBy(c => c.TenNCC)
            .Select(c => new CategoryOption(c.MaNhaCungCap, c.TenNCC))
            .ToListAsync();
    }

    public async Task<ValidationResult<decimal>> CreateReceiptAsync(CreateInventoryReceiptInput input)
    {
        var validation = await ValidateAsync(input);
        if (!validation.IsValid)
        {
            return ValidationResult<decimal>.Failure(validation.ErrorMessage!);
        }

        var productIds = input.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.MaHang))
            .ToDictionaryAsync(p => p.MaHang);

        var total = input.Lines.Sum(line => line.Quantity * line.UnitCost);
        var receipt = new InventoryReceipt
        {
            MaNhaCungCap = input.SupplierId,
            MaNhanVien = input.EmployeeId,
            NgayNhapKho = DateTime.Now,
            NguoiGiao = input.DeliveredBy.Trim(),
            GhiChu = input.Note.Trim(),
            TongTien = total
        };

        foreach (var line in input.Lines)
        {
            var product = products[line.ProductId];
            product.SoLuongTon += line.Quantity;
            receipt.InventoryReceiptDetails.Add(new InventoryReceiptDetail
            {
                MaHang = product.MaHang,
                SoLuongNhap = line.Quantity,
                DonGiaNhap = line.UnitCost,
                ThanhTien = line.Quantity * line.UnitCost
            });
        }

        _dbContext.InventoryReceipts.Add(receipt);
        await _dbContext.SaveChangesAsync();

        return ValidationResult<decimal>.Success(total);
    }

    private async Task<ValidationResult> ValidateAsync(CreateInventoryReceiptInput input)
    {
        if (!await _dbContext.Categories.AnyAsync(c => c.MaNhaCungCap == input.SupplierId))
        {
            return ValidationResult.Failure("Nhà cung cấp không hợp lệ.");
        }

        if (!await _dbContext.Users.AnyAsync(u => u.MaNhanVien == input.EmployeeId && u.IsActive))
        {
            return ValidationResult.Failure("Nhân viên không hợp lệ.");
        }

        if (input.Lines.Count == 0)
        {
            return ValidationResult.Failure("Phiếu nhập cần ít nhất một sản phẩm.");
        }

        var duplicateProduct = input.Lines
            .GroupBy(l => l.ProductId)
            .Any(g => g.Count() > 1);
        if (duplicateProduct)
        {
            return ValidationResult.Failure("Mỗi sản phẩm chỉ nên xuất hiện một lần trong phiếu nhập.");
        }

        foreach (var line in input.Lines)
        {
            if (line.Quantity <= 0)
            {
                return ValidationResult.Failure("Số lượng nhập phải lớn hơn 0.");
            }

            if (line.UnitCost < 0)
            {
                return ValidationResult.Failure("Đơn giá nhập không được âm.");
            }

            if (!await _dbContext.Products.AnyAsync(p => p.MaHang == line.ProductId))
            {
                return ValidationResult.Failure("Sản phẩm không hợp lệ.");
            }
        }

        return ValidationResult.Success();
    }
}

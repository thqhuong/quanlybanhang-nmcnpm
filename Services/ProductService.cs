using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;
using quanlybanhang_nmcnpm.Models;

namespace quanlybanhang_nmcnpm.Services;

public sealed class ProductService : IProductService
{
    private readonly ApplicationDbContext _dbContext;

    public ProductService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductListItem>> GetAllAsync()
    {
        var products = await ProductQuery()
            .OrderBy(p => p.MaSanPham)
            .ToListAsync();

        return products.Select(p => p.ToListItem()).ToList();
    }

    public async Task<IReadOnlyList<ProductListItem>> SearchAsync(string? searchText, int? categoryId = null)
    {
        var query = ProductQuery();
        var normalized = Normalize(searchText);

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            query = query.Where(p =>
                p.MaSanPham.ToLower().Contains(normalized) ||
                p.TenHang.ToLower().Contains(normalized));
        }

        if (categoryId.GetValueOrDefault() > 0)
        {
            query = query.Where(p => p.MaNhaCungCap == categoryId);
        }

        var products = await query
            .OrderBy(p => p.MaSanPham)
            .ToListAsync();

        return products.Select(p => p.ToListItem()).ToList();
    }

    public async Task<IReadOnlyList<CategoryOption>> GetCategoriesAsync()
    {
        return await _dbContext.Categories
            .OrderBy(c => c.TenNCC)
            .Select(c => new CategoryOption(c.MaNhaCungCap, c.TenNCC))
            .ToListAsync();
    }

    public async Task<ValidationResult<ProductListItem>> CreateAsync(ProductInput input)
    {
        var validation = await ValidateAsync(input);
        if (!validation.IsValid)
        {
            return ValidationResult<ProductListItem>.Failure(validation.ErrorMessage!);
        }

        var product = new Product();
        ApplyInput(product, input);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        await _dbContext.Entry(product).Reference(p => p.Category).LoadAsync();
        return ValidationResult<ProductListItem>.Success(product.ToListItem());
    }

    public async Task<ValidationResult<ProductListItem>> UpdateAsync(int id, ProductInput input)
    {
        var product = await ProductQuery().FirstOrDefaultAsync(p => p.MaHang == id);
        if (product is null)
        {
            return ValidationResult<ProductListItem>.Failure("Không tìm thấy sản phẩm.");
        }

        var validation = await ValidateAsync(input, id);
        if (!validation.IsValid)
        {
            return ValidationResult<ProductListItem>.Failure(validation.ErrorMessage!);
        }

        ApplyInput(product, input);
        await _dbContext.SaveChangesAsync();
        return ValidationResult<ProductListItem>.Success(product.ToListItem());
    }

    public async Task<ValidationResult> DeleteAsync(int id)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaHang == id);
        if (product is null)
        {
            return ValidationResult.Failure("Không tìm thấy sản phẩm.");
        }

        var hasOrders = await _dbContext.OrderDetails.AnyAsync(od => od.MaHang == id);
        var hasReceipts = await _dbContext.InventoryReceiptDetails.AnyAsync(ird => ird.MaHang == id);
        if (hasOrders || hasReceipts)
        {
            return ValidationResult.Failure("Sản phẩm đã phát sinh giao dịch nên không thể xóa.");
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
        return ValidationResult.Success();
    }

    private IQueryable<Product> ProductQuery()
    {
        return _dbContext.Products.Include(p => p.Category);
    }

    private async Task<ValidationResult> ValidateAsync(ProductInput input, int? existingId = null)
    {
        if (string.IsNullOrWhiteSpace(input.Code))
        {
            return ValidationResult.Failure("Mã sản phẩm là bắt buộc.");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return ValidationResult.Failure("Tên sản phẩm là bắt buộc.");
        }

        if (string.IsNullOrWhiteSpace(input.Unit))
        {
            return ValidationResult.Failure("Đơn vị tính là bắt buộc.");
        }

        if (input.Price < 0)
        {
            return ValidationResult.Failure("Đơn giá bán không được âm.");
        }

        if (input.Stock < 0)
        {
            return ValidationResult.Failure("Tồn kho không được âm.");
        }

        if (!await _dbContext.Categories.AnyAsync(c => c.MaNhaCungCap == input.CategoryId))
        {
            return ValidationResult.Failure("Nhóm hàng không hợp lệ.");
        }

        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        var normalizedName = input.Name.Trim().ToLowerInvariant();
        var duplicateCode = await _dbContext.Products.AnyAsync(p =>
            p.MaSanPham.ToUpper() == normalizedCode &&
            (!existingId.HasValue || p.MaHang != existingId.Value));
        if (duplicateCode)
        {
            return ValidationResult.Failure("Mã sản phẩm đã tồn tại.");
        }

        var duplicateName = await _dbContext.Products.AnyAsync(p =>
            p.TenHang.ToLower() == normalizedName &&
            (!existingId.HasValue || p.MaHang != existingId.Value));
        if (duplicateName)
        {
            return ValidationResult.Failure("Tên sản phẩm đã tồn tại.");
        }

        return ValidationResult.Success();
    }

    private static void ApplyInput(Product product, ProductInput input)
    {
        product.MaSanPham = input.Code.Trim().ToUpperInvariant();
        product.TenHang = input.Name.Trim();
        product.DonViTinh = input.Unit.Trim();
        product.GiaBan = input.Price;
        product.SoLuongTon = input.Stock;
        product.MaNhaCungCap = input.CategoryId;
        product.MaLoai = string.IsNullOrWhiteSpace(input.CategoryCode)
            ? input.Code.Trim().ToUpperInvariant()
            : input.CategoryCode.Trim().ToUpperInvariant();
    }

    private static string Normalize(string? value)
    {
        return value?.Trim().ToLowerInvariant() ?? "";
    }
}

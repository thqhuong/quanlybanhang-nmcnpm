using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;
using quanlybanhang_nmcnpm.Models;

namespace quanlybanhang_nmcnpm.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CustomerListItem>> GetAllAsync()
    {
        var customers = await CustomerQuery()
            .OrderBy(c => c.TenKH)
            .ToListAsync();

        return customers.Select(c => c.ToListItem()).ToList();
    }

    public async Task<IReadOnlyList<CustomerListItem>> SearchAsync(string? searchText)
    {
        var query = CustomerQuery();
        var normalized = searchText?.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            query = query.Where(c =>
                c.TenKH.ToLower().Contains(normalized) ||
                c.SoDienThoai.ToLower().Contains(normalized));
        }

        var customers = await query
            .OrderBy(c => c.TenKH)
            .ToListAsync();

        return customers.Select(c => c.ToListItem()).ToList();
    }

    public async Task<ValidationResult<CustomerListItem>> CreateAsync(CustomerInput input)
    {
        var validation = await ValidateAsync(input);
        if (!validation.IsValid)
        {
            return ValidationResult<CustomerListItem>.Failure(validation.ErrorMessage!);
        }

        var customer = new Customer();
        ApplyInput(customer, input);
        customer.NgayDangKy = DateTime.Today;
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        return ValidationResult<CustomerListItem>.Success(customer.ToListItem());
    }

    public async Task<ValidationResult<CustomerListItem>> UpdateAsync(int id, CustomerInput input)
    {
        var customer = await CustomerQuery().FirstOrDefaultAsync(c => c.MaKH == id);
        if (customer is null)
        {
            return ValidationResult<CustomerListItem>.Failure("Không tìm thấy khách hàng.");
        }

        var validation = await ValidateAsync(input, id);
        if (!validation.IsValid)
        {
            return ValidationResult<CustomerListItem>.Failure(validation.ErrorMessage!);
        }

        ApplyInput(customer, input);
        await _dbContext.SaveChangesAsync();
        return ValidationResult<CustomerListItem>.Success(customer.ToListItem());
    }

    public async Task<ValidationResult> DeleteAsync(int id)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.MaKH == id);
        if (customer is null)
        {
            return ValidationResult.Failure("Không tìm thấy khách hàng.");
        }

        if (await _dbContext.Orders.AnyAsync(o => o.MaKH == id))
        {
            return ValidationResult.Failure("Khách hàng đã có đơn hàng nên không thể xóa.");
        }

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync();
        return ValidationResult.Success();
    }

    private IQueryable<Customer> CustomerQuery()
    {
        return _dbContext.Customers.Include(c => c.Orders);
    }

    private async Task<ValidationResult> ValidateAsync(CustomerInput input, int? existingId = null)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return ValidationResult.Failure("Tên khách hàng là bắt buộc.");
        }

        if (string.IsNullOrWhiteSpace(input.Phone))
        {
            return ValidationResult.Failure("Số điện thoại là bắt buộc.");
        }

        var phone = input.Phone.Trim();
        if (phone.Length < 8 || phone.Any(ch => !char.IsDigit(ch)))
        {
            return ValidationResult.Failure("Số điện thoại không hợp lệ.");
        }

        if (!string.IsNullOrWhiteSpace(input.Email) && !input.Email.Contains('@'))
        {
            return ValidationResult.Failure("Email không hợp lệ.");
        }

        if (input.Points < 0)
        {
            return ValidationResult.Failure("Điểm tích lũy không được âm.");
        }

        var duplicatePhone = await _dbContext.Customers.AnyAsync(c =>
            c.SoDienThoai == phone &&
            (!existingId.HasValue || c.MaKH != existingId.Value));
        if (duplicatePhone)
        {
            return ValidationResult.Failure("Số điện thoại đã tồn tại.");
        }

        return ValidationResult.Success();
    }

    private static void ApplyInput(Customer customer, CustomerInput input)
    {
        customer.TenKH = input.Name.Trim();
        customer.SoDienThoai = input.Phone.Trim();
        customer.Email = input.Email.Trim();
        customer.DiaChiKH = input.Address.Trim();
        customer.NgaySinh = input.BirthDate;
        customer.DiemTichLuy = input.Points;
    }
}

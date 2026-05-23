using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;
using quanlybanhang_nmcnpm.Models;

namespace quanlybanhang_nmcnpm.Services;

public sealed class AccountService : IAccountService
{
    private readonly ApplicationDbContext _dbContext;

    public AccountService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AccountListItem>> GetAllAsync()
    {
        var users = await _dbContext.Users
            .Include(u => u.Role)
            .OrderBy(u => u.TenDangNhap)
            .ToListAsync();

        return users.Select(u => u.ToListItem()).ToList();
    }

    public async Task<IReadOnlyList<CategoryOption>> GetRolesAsync()
    {
        return await _dbContext.Roles
            .OrderBy(r => r.TenVaiTro)
            .Select(r => new CategoryOption(r.MaVaiTro, r.TenVaiTro))
            .ToListAsync();
    }

    public async Task<ValidationResult<AccountListItem>> CreateAsync(AccountInput input)
    {
        var validation = await ValidateAsync(input);
        if (!validation.IsValid)
        {
            return ValidationResult<AccountListItem>.Failure(validation.ErrorMessage!);
        }

        var user = new User();
        ApplyInput(user, input);
        user.NgayDangKy = DateTime.Today;
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        await _dbContext.Entry(user).Reference(u => u.Role).LoadAsync();

        return ValidationResult<AccountListItem>.Success(user.ToListItem());
    }

    public async Task<ValidationResult<AccountListItem>> UpdateAsync(int id, AccountInput input)
    {
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.MaNhanVien == id);
        if (user is null)
        {
            return ValidationResult<AccountListItem>.Failure("Không tìm thấy tài khoản.");
        }

        var validation = await ValidateAsync(input, id);
        if (!validation.IsValid)
        {
            return ValidationResult<AccountListItem>.Failure(validation.ErrorMessage!);
        }

        ApplyInput(user, input);
        await _dbContext.SaveChangesAsync();
        await _dbContext.Entry(user).Reference(u => u.Role).LoadAsync();
        return ValidationResult<AccountListItem>.Success(user.ToListItem());
    }

    public async Task<ValidationResult> SetActiveAsync(int id, bool isActive)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.MaNhanVien == id);
        if (user is null)
        {
            return ValidationResult.Failure("Không tìm thấy tài khoản.");
        }

        user.IsActive = isActive;
        await _dbContext.SaveChangesAsync();
        return ValidationResult.Success();
    }

    private async Task<ValidationResult> ValidateAsync(AccountInput input, int? existingId = null)
    {
        if (string.IsNullOrWhiteSpace(input.Username))
        {
            return ValidationResult.Failure("Tên đăng nhập là bắt buộc.");
        }

        if (string.IsNullOrWhiteSpace(input.FullName))
        {
            return ValidationResult.Failure("Họ tên là bắt buộc.");
        }

        if (!await _dbContext.Roles.AnyAsync(r => r.MaVaiTro == input.RoleId))
        {
            return ValidationResult.Failure("Vai trò không hợp lệ.");
        }

        var username = input.Username.Trim().ToLowerInvariant();
        var duplicateUsername = await _dbContext.Users.AnyAsync(u =>
            u.TenDangNhap.ToLower() == username &&
            (!existingId.HasValue || u.MaNhanVien != existingId.Value));
        if (duplicateUsername)
        {
            return ValidationResult.Failure("Tên đăng nhập đã tồn tại.");
        }

        return ValidationResult.Success();
    }

    private static void ApplyInput(User user, AccountInput input)
    {
        user.TenDangNhap = input.Username.Trim().ToLowerInvariant();
        user.TenNV = input.FullName.Trim();
        user.SoDienThoai = input.Phone.Trim();
        user.Email = input.Email.Trim();
        user.MaVaiTro = input.RoleId;
        user.IsActive = input.IsActive;
    }
}

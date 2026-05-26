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
            .Include(user => user.Role)
            .OrderBy(user => user.TenDangNhap)
            .ToListAsync();

        return users.Select(user => user.ToListItem()).ToList();
    }

    public async Task<IReadOnlyList<CategoryOption>> GetRolesAsync()
    {
        return await _dbContext.Roles
            .OrderBy(role => role.TenVaiTro)
            .Select(role => new CategoryOption(role.MaVaiTro, role.TenVaiTro))
            .ToListAsync();
    }

    public async Task<ValidationResult<AccountListItem>> CreateAsync(AccountInput input)
    {
        var validation = await ValidateAsync(input);
        if (!validation.IsValid)
        {
            return ValidationResult<AccountListItem>.Failure(validation.ErrorMessage!);
        }

        var user = new User { NgayDangKy = DateTime.Today };
        ApplyInput(user, input);
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
        if (!string.IsNullOrWhiteSpace(input.Password))
        {
            user.PasswordHash = PasswordHasher.HashPassword(input.Password);
        }
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

    public async Task<ValidationResult<UserSession>> AuthenticateAsync(LoginInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Username) || string.IsNullOrWhiteSpace(input.Password))
        {
            return ValidationResult<UserSession>.Failure("Vui lòng nhập tên đăng nhập và mật khẩu.");
        }

        var username = input.Username.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.TenDangNhap.ToLower() == username);

        if (user is null || !PasswordHasher.VerifyPassword(input.Password, user.PasswordHash))
        {
            return ValidationResult<UserSession>.Failure("Sai tên đăng nhập hoặc mật khẩu.");
        }

        if (!user.IsActive)
        {
            return ValidationResult<UserSession>.Failure("Tài khoản đã bị khóa.");
        }

        user.LastLoginAt = DateTime.Now;
        await _dbContext.SaveChangesAsync();

        return ValidationResult<UserSession>.Success(new UserSession(
            user.MaNhanVien,
            user.TenDangNhap,
            user.TenNV,
            user.Role?.TenVaiTro ?? string.Empty,
            user.MaVaiTro));
    }

    private async Task<ValidationResult> ValidateAsync(AccountInput input, int? existingId = null)
    {
        var username = input.Username.Trim().ToLowerInvariant();
        var fullName = input.FullName.Trim();
        var phone = input.Phone.Trim();
        var email = input.Email.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            return ValidationResult.Failure("Tên đăng nhập là bắt buộc.");
        }

        if (username.Length < 3 || username.Any(ch => !char.IsLetterOrDigit(ch) && ch != '_' && ch != '.'))
        {
            return ValidationResult.Failure("Tên đăng nhập phải có ít nhất 3 ký tự và chỉ gồm chữ, số, dấu chấm hoặc gạch dưới.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ValidationResult.Failure("Họ tên là bắt buộc.");
        }

        if (!string.IsNullOrWhiteSpace(phone) && (phone.Length < 8 || phone.Any(ch => !char.IsDigit(ch))))
        {
            return ValidationResult.Failure("Số điện thoại không hợp lệ.");
        }

        if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
        {
            return ValidationResult.Failure("Email không hợp lệ.");
        }

        if (!await _dbContext.Roles.AnyAsync(role => role.MaVaiTro == input.RoleId))
        {
            return ValidationResult.Failure("Vai trò không hợp lệ.");
        }

        var duplicateUsername = await _dbContext.Users.AnyAsync(user =>
            user.TenDangNhap.ToLower() == username &&
            (!existingId.HasValue || user.MaNhanVien != existingId.Value));
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

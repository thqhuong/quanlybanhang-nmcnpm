namespace quanlybanhang_nmcnpm.Models;

public class User
{
    public int MaNhanVien { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public DateTime? NgaySinh { get; set; }
    public DateTime NgayDangKy { get; set; }
    public string SoDienThoai { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int MaVaiTro { get; set; }

    public virtual Role? Role { get; set; }
}

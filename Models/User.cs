namespace quanlybanhang_nmcnpm.Models;

public class User
{
    public int MaNhanVien { get; set; }
    public string TenNV { get; set; } = string.Empty;
    public DateTime NgaySinh { get; set; }
    public DateTime NgayDangKy { get; set; }
    public int MaKH { get; set; }
    public string TenKH { get; set; } = string.Empty;
    public string DiaChiKH { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int MaVaiTro { get; set; }

    // Navigation Properties
    public virtual Role? Role { get; set; }
}

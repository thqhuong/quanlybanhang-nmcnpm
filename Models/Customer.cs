namespace quanlybanhang_nmcnpm.Models;

public class Customer
{
    public int MaKH { get; set; }
    public string TenKH { get; set; } = string.Empty;
    public DateTime? NgaySinh { get; set; }
    public DateTime NgayDangKy { get; set; }
    public string DiaChiKH { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DiemTichLuy { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
}

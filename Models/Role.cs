namespace quanlybanhang_nmcnpm.Models;

public class Role
{
    public int MaVaiTro { get; set; }
    public string TenVaiTro { get; set; } = string.Empty;

    // Navigation Properties
    public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
}

namespace quanlybanhang_nmcnpm.Models;

public class Category
{
    public int MaNhaCungCap { get; set; }
    public string TenNCC { get; set; } = string.Empty;

    // Navigation Properties
    public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
}

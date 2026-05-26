namespace quanlybanhang_nmcnpm.Models;

public class Category
{
    public int MaNhaCungCap { get; set; }
    public string TenNCC { get; set; } = string.Empty;

    public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
    public virtual ICollection<InventoryReceipt> InventoryReceipts { get; set; } = new HashSet<InventoryReceipt>();
}

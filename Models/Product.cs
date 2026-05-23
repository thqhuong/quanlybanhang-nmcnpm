namespace quanlybanhang_nmcnpm.Models;

public class Product
{
    public int MaHang { get; set; }
    public string MaSanPham { get; set; } = string.Empty;
    public string TenHang { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public int MaNhaCungCap { get; set; }
    public string MaLoai { get; set; } = string.Empty;

    public virtual Category? Category { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
    public virtual ICollection<InventoryReceiptDetail> InventoryReceiptDetails { get; set; } = new HashSet<InventoryReceiptDetail>();
}

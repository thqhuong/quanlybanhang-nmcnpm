namespace quanlybanhang_nmcnpm.Models;

public class InventoryReceipt
{
    public int MaPhieuNhapKho { get; set; }
    public DateTime NgayNhapKho { get; set; }
    public int TongTien { get; set; }
    public int MaNhanVien { get; set; }
    public int MaNhaCungCap { get; set; }

    // Navigation Properties
    public virtual ICollection<InventoryReceiptDetail> InventoryReceiptDetails { get; set; } = new HashSet<InventoryReceiptDetail>();
}

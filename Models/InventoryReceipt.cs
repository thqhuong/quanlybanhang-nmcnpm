namespace quanlybanhang_nmcnpm.Models;

public class InventoryReceipt
{
    public int MaPhieuNhapKho { get; set; }
    public DateTime NgayNhapKho { get; set; }
    public decimal TongTien { get; set; }
    public int MaNhanVien { get; set; }
    public int MaNhaCungCap { get; set; }
    public string NguoiGiao { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;

    public virtual Category? Category { get; set; }
    public virtual ICollection<InventoryReceiptDetail> InventoryReceiptDetails { get; set; } = new HashSet<InventoryReceiptDetail>();
}

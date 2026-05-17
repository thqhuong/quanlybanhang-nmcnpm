namespace quanlybanhang_nmcnpm.Models;

public class InventoryReceiptDetail
{
    public int SoLuongNhap { get; set; }
    public int DonGiaNhap { get; set; }
    public int ThanhTien { get; set; }
    public int MaPhieuNhapKho { get; set; }
    public int MaHang { get; set; }

    // Navigation Properties
    public virtual InventoryReceipt? InventoryReceipt { get; set; }
    public virtual Product? Product { get; set; }
}

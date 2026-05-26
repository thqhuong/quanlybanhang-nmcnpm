namespace quanlybanhang_nmcnpm.Models;

public class InventoryReceiptDetail
{
    public int SoLuongNhap { get; set; }
    public decimal DonGiaNhap { get; set; }
    public decimal ThanhTien { get; set; }
    public int MaPhieuNhapKho { get; set; }
    public int MaHang { get; set; }

    public virtual InventoryReceipt? InventoryReceipt { get; set; }
    public virtual Product? Product { get; set; }
}

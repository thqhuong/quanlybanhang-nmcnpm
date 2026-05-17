namespace quanlybanhang_nmcnpm.Models;

public class OrderDetail
{
    public int SoLuong { get; set; }
    public int DonGiaBan { get; set; }
    public int ThanhTien { get; set; }
    public int MaDonHang { get; set; }
    public int MaHang { get; set; }

    // Navigation Properties
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
}

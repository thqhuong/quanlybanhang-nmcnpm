namespace quanlybanhang_nmcnpm.Models;

public class OrderDetail
{
    public int SoLuong { get; set; }
    public decimal DonGiaBan { get; set; }
    public decimal ThanhTien { get; set; }
    public int MaDonHang { get; set; }
    public int MaHang { get; set; }

    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
}

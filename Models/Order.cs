namespace quanlybanhang_nmcnpm.Models;

public class Order
{
    public int MaDonHang { get; set; }
    public int MaKH { get; set; }
    public int MaNhanVien { get; set; }
    public DateTime NgayLap { get; set; }
    public decimal TongTien { get; set; }

    // Navigation Properties
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
}

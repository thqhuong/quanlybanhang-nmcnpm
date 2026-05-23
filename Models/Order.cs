namespace quanlybanhang_nmcnpm.Models;

public class Order
{
    public int MaDonHang { get; set; }
    public int MaKH { get; set; }
    public int MaNhanVien { get; set; }
    public DateTime NgayLap { get; set; }
    public decimal TamTinh { get; set; }
    public decimal GiamGia { get; set; }
    public decimal VatRate { get; set; }
    public decimal TienVat { get; set; }
    public decimal TongTien { get; set; }

    public virtual Customer? Customer { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
}

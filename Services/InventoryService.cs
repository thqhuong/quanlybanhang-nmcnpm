using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using quanlybanhang_nmcnpm.Database;
using quanlybanhang_nmcnpm.Models;

namespace quanlybanhang_nmcnpm.Services;

public sealed class InventoryService : IInventoryService
{
    private const int DefaultLowStockThreshold = 20;
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserSessionService? _sessionService;

    public InventoryService(ApplicationDbContext dbContext, IUserSessionService? sessionService = null)
    {
        _dbContext = dbContext;
        _sessionService = sessionService;
    }

    public async Task<IReadOnlyList<CategoryOption>> GetSuppliersAsync()
    {
        EnsureWarehouseAccess();

        return await _dbContext.Categories
            .OrderBy(category => category.TenNCC)
            .Select(category => new CategoryOption(category.MaNhaCungCap, category.TenNCC))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<LowStockReportItem>> GetLowStockAsync(int threshold = DefaultLowStockThreshold)
    {
        return await _dbContext.Products
            .Where(product => product.SoLuongTon <= threshold)
            .OrderBy(product => product.SoLuongTon)
            .ThenBy(product => product.MaSanPham)
            .Select(product => new LowStockReportItem(
                product.MaSanPham,
                product.TenHang,
                product.SoLuongTon,
                product.DonViTinh))
            .ToListAsync();
    }

    public async Task<ValidationResult<decimal>> CreateReceiptAsync(CreateInventoryReceiptInput input)
    {
        if (!HasWarehouseAccess())
        {
            return ValidationResult<decimal>.Failure("Bạn không có quyền truy cập kho.");
        }

        var validation = await ValidateAsync(input);
        if (!validation.IsValid)
        {
            return ValidationResult<decimal>.Failure(validation.ErrorMessage!);
        }

        var productIds = input.Lines.Select(line => line.ProductId).Distinct().ToList();
        var products = await _dbContext.Products
            .Where(product => productIds.Contains(product.MaHang))
            .ToDictionaryAsync(product => product.MaHang);
        if (products.Count != productIds.Count)
        {
            return ValidationResult<decimal>.Failure("Sản phẩm không hợp lệ.");
        }

        var total = input.Lines.Sum(line => line.Quantity * line.UnitCost);
        var receipt = new InventoryReceipt
        {
            MaNhaCungCap = input.SupplierId,
            MaNhanVien = input.EmployeeId,
            NgayNhapKho = input.ReceiptDate,
            NguoiGiao = input.DeliveredBy.Trim(),
            GhiChu = input.Note.Trim(),
            TongTien = total
        };

        foreach (var line in input.Lines)
        {
            var product = products[line.ProductId];
            product.SoLuongTon += line.Quantity;
            receipt.InventoryReceiptDetails.Add(new InventoryReceiptDetail
            {
                MaHang = product.MaHang,
                SoLuongNhap = line.Quantity,
                DonGiaNhap = line.UnitCost,
                ThanhTien = line.Quantity * line.UnitCost
            });
        }

        _dbContext.InventoryReceipts.Add(receipt);
        await _dbContext.SaveChangesAsync();
        await ExportReceiptFileAsync(receipt);

        return ValidationResult<decimal>.Success(total);
    }

    public void OpenReceiptFolder()
    {
        var folder = GetReceiptFolder();
        Directory.CreateDirectory(folder);
        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true
        });
    }

    public void Print(InventoryReceiptPrintout receipt)
    {
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true)
        {
            return;
        }

        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12,
            PagePadding = new Thickness(40),
            ColumnWidth = printDialog.PrintableAreaWidth
        };

        document.Blocks.Add(new Paragraph(new Run("CUA HANG QUAN LY BAN HANG"))
        {
            FontWeight = FontWeights.Bold,
            FontSize = 16,
            TextAlignment = TextAlignment.Center
        });
        document.Blocks.Add(new Paragraph(new Run($"PHIEU NHAP KHO - {receipt.ReceiptDate:dd/MM/yyyy}"))
        {
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center
        });
        document.Blocks.Add(new Paragraph(new Run($"Nha cung cap: {receipt.SupplierName}")));
        document.Blocks.Add(new Paragraph(new Run($"Ben giao: {receipt.DeliveredBy}")));

        var table = new Table();
        table.Columns.Add(new TableColumn { Width = new GridLength(80) });
        table.Columns.Add(new TableColumn { Width = new GridLength(180) });
        table.Columns.Add(new TableColumn { Width = new GridLength(50) });
        table.Columns.Add(new TableColumn { Width = new GridLength(70) });
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });
        table.Columns.Add(new TableColumn { Width = new GridLength(110) });

        var rowGroup = new TableRowGroup();
        table.RowGroups.Add(rowGroup);
        rowGroup.Rows.Add(CreatePrintRow("Ma SP", "Ten hang", "DVT", "SL", "Don gia", "Thanh tien", true));
        foreach (var line in receipt.Lines)
        {
            rowGroup.Rows.Add(CreatePrintRow(
                line.ProductCode,
                line.ProductName,
                line.Unit,
                line.Quantity.ToString(),
                FormatMoney(line.UnitCost, CultureInfo.GetCultureInfo("vi-VN")),
                FormatMoney(line.LineTotal, CultureInfo.GetCultureInfo("vi-VN")),
                false));
        }

        document.Blocks.Add(table);
        document.Blocks.Add(new Paragraph(new Run($"Tong tien: {FormatMoney(receipt.Total, CultureInfo.GetCultureInfo("vi-VN"))}"))
        {
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Right
        });

        if (!string.IsNullOrWhiteSpace(receipt.Note))
        {
            document.Blocks.Add(new Paragraph(new Run($"Ghi chu: {receipt.Note}")));
        }

        printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "Phieu nhap kho");
    }

    private async Task<ValidationResult> ValidateAsync(CreateInventoryReceiptInput input)
    {
        if (!await _dbContext.Categories.AnyAsync(category => category.MaNhaCungCap == input.SupplierId))
        {
            return ValidationResult.Failure("Nhà cung cấp không hợp lệ.");
        }

        if (!await _dbContext.Users.AnyAsync(user => user.MaNhanVien == input.EmployeeId && user.IsActive))
        {
            return ValidationResult.Failure("Nhân viên không hợp lệ.");
        }

        if (input.Lines.Count == 0)
        {
            return ValidationResult.Failure("Phiếu nhập cần ít nhất một sản phẩm.");
        }

        var duplicateProduct = input.Lines
            .GroupBy(line => line.ProductId)
            .Any(group => group.Count() > 1);
        if (duplicateProduct)
        {
            return ValidationResult.Failure("Mỗi sản phẩm chỉ nên xuất hiện một lần trong phiếu nhập.");
        }

        foreach (var line in input.Lines)
        {
            if (line.Quantity <= 0)
            {
                return ValidationResult.Failure("Số lượng nhập phải lớn hơn 0.");
            }

            if (line.UnitCost <= 0)
            {
                return ValidationResult.Failure("Đơn giá nhập phải lớn hơn 0.");
            }
        }

        return ValidationResult.Success();
    }

    private async Task ExportReceiptFileAsync(InventoryReceipt receipt)
    {
        var folder = GetReceiptFolder();
        Directory.CreateDirectory(folder);

        var savedReceipt = await _dbContext.InventoryReceipts
            .AsNoTracking()
            .Include(item => item.Category)
            .Include(item => item.InventoryReceiptDetails)
            .ThenInclude(detail => detail.Product)
            .FirstAsync(item => item.MaPhieuNhapKho == receipt.MaPhieuNhapKho);

        var path = Path.Combine(folder, $"PN-{savedReceipt.MaPhieuNhapKho:000000}.txt");
        await File.WriteAllTextAsync(path, BuildReceiptText(savedReceipt), Encoding.UTF8);
    }

    private static string BuildReceiptText(InventoryReceipt receipt)
    {
        var culture = CultureInfo.GetCultureInfo("vi-VN");
        var builder = new StringBuilder();
        builder.AppendLine("CUA HANG QUAN LY BAN HANG");
        builder.AppendLine($"Phieu nhap: #{receipt.MaPhieuNhapKho:000000}");
        builder.AppendLine($"Ngay nhap: {receipt.NgayNhapKho:dd/MM/yyyy HH:mm}");
        builder.AppendLine($"Nha cung cap: {receipt.Category?.TenNCC ?? ""}");
        builder.AppendLine($"Nguoi giao: {receipt.NguoiGiao}");
        builder.AppendLine(new string('-', 42));

        foreach (var line in receipt.InventoryReceiptDetails.OrderBy(detail => detail.Product?.MaSanPham))
        {
            builder.AppendLine($"{line.Product?.MaSanPham ?? ""} - {line.Product?.TenHang ?? ""}");
            builder.AppendLine($"  {line.SoLuongNhap} x {FormatMoney(line.DonGiaNhap, culture)} = {FormatMoney(line.ThanhTien, culture)}");
        }

        builder.AppendLine(new string('-', 42));
        builder.AppendLine($"Tong tien: {FormatMoney(receipt.TongTien, culture)}");
        if (!string.IsNullOrWhiteSpace(receipt.GhiChu))
        {
            builder.AppendLine($"Ghi chu: {receipt.GhiChu}");
        }

        return builder.ToString();
    }

    private static string FormatMoney(decimal value, CultureInfo culture)
    {
        return string.Format(culture, "{0:N0} d", value);
    }

    private static TableRow CreatePrintRow(
        string productCode,
        string productName,
        string unit,
        string quantity,
        string unitCost,
        string lineTotal,
        bool isHeader)
    {
        var row = new TableRow();
        foreach (var value in new[] { productCode, productName, unit, quantity, unitCost, lineTotal })
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(value)))
            {
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                Padding = new Thickness(2)
            });
        }

        return row;
    }

    private static string GetReceiptFolder()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "QuanLyBanHang",
            "InventoryReceipts");
    }

    private bool HasWarehouseAccess()
    {
        return _sessionService is null || _sessionService.IsInRole(RoleNames.Admin, RoleNames.Storekeeper);
    }

    private void EnsureWarehouseAccess()
    {
        if (!HasWarehouseAccess())
        {
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập kho.");
        }
    }
}

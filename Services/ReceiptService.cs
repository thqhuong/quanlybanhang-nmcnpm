using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace quanlybanhang_nmcnpm.Services;

public sealed class ReceiptService : IReceiptService
{
    public async Task<string> ExportAsync(OrderReceipt receipt)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "QuanLyBanHang",
            "Receipts");
        Directory.CreateDirectory(folder);

        var path = Path.Combine(folder, $"HD-{receipt.OrderId:000000}.txt");
        await File.WriteAllTextAsync(path, BuildReceiptText(receipt), Encoding.UTF8);
        return path;
    }

    public void Print(OrderReceipt receipt)
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
            PagePadding = new Thickness(24),
            ColumnWidth = printDialog.PrintableAreaWidth
        };
        document.Blocks.Add(new Paragraph(new Run(BuildReceiptText(receipt))));

        var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
        printDialog.PrintDocument(paginator, $"Hoa don #{receipt.OrderId}");
    }

    private static string BuildReceiptText(OrderReceipt receipt)
    {
        var culture = CultureInfo.GetCultureInfo("vi-VN");
        var builder = new StringBuilder();
        builder.AppendLine("CUA HANG QUAN LY BAN HANG");
        builder.AppendLine($"Hoa don: #{receipt.OrderId:000000}");
        builder.AppendLine($"Ngay lap: {receipt.CreatedAt:dd/MM/yyyy HH:mm}");
        builder.AppendLine($"Thu ngan: {receipt.CashierName}");
        builder.AppendLine($"Khach hang: {receipt.CustomerName}");
        builder.AppendLine(new string('-', 42));

        foreach (var line in receipt.Lines)
        {
            builder.AppendLine($"{line.ProductCode} - {line.ProductName}");
            builder.AppendLine($"  {line.Quantity} {line.Unit} x {FormatMoney(line.UnitPrice, culture)} = {FormatMoney(line.LineTotal, culture)}");
        }

        builder.AppendLine(new string('-', 42));
        builder.AppendLine($"Tam tinh: {FormatMoney(receipt.Subtotal, culture)}");
        builder.AppendLine($"Giam gia: {FormatMoney(receipt.Discount, culture)}");
        builder.AppendLine($"VAT:      {FormatMoney(receipt.Vat, culture)}");
        builder.AppendLine($"Tong:     {FormatMoney(receipt.Total, culture)}");
        builder.AppendLine($"Da tra:   {FormatMoney(receipt.PaidAmount, culture)}");
        builder.AppendLine($"Tien thoi:{FormatMoney(receipt.Change, culture)}");
        builder.AppendLine(new string('-', 42));
        builder.AppendLine("Cam on quy khach!");
        return builder.ToString();
    }

    private static string FormatMoney(decimal value, CultureInfo culture)
    {
        return string.Format(culture, "{0:N0} d", value);
    }
}

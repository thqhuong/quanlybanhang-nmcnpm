using System.Collections.ObjectModel;
using System.Globalization;
using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm.ViewModels;

public sealed class ImportViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private readonly IInventoryService _inventoryService;
    private readonly IUserSessionService _sessionService;
    private readonly List<ProductListItem> _products = new();
    private int _employeeId;
    private CategoryOption? _selectedSupplier;
    private ReceiptLine? _selectedLine;
    private string _productCode = "";
    private string _quantityText = "";
    private string _unitCostText = "";
    private string _receiptDateText = DateTime.Today.ToString("dd/MM/yyyy");
    private string _deliveredBy = "";
    private string _note = "";
    private decimal _total;
    private string _statusMessage = "";
    private ReceiptPrintData? _printData;

    public ImportViewModel(
        IProductService productService,
        IInventoryService inventoryService,
        IUserSessionService sessionService)
    {
        _productService = productService;
        _inventoryService = inventoryService;
        _sessionService = sessionService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddLineCommand = new AsyncRelayCommand(AddLineAsync);
        RemoveLineCommand = new RelayCommand(RemoveSelectedLine, () => SelectedLine is not null);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => Lines.Count > 0);
        NewCommand = new RelayCommand(ClearReceipt);
        PrintCommand = new RelayCommand(PrintReceipt, () => Lines.Count > 0);
        OpenReceiptFolderCommand = new RelayCommand(OpenReceiptFolder);
    }

    public ObservableCollection<CategoryOption> Suppliers { get; } = new();
    public ObservableCollection<ReceiptLine> Lines { get; } = new();
    public ObservableCollection<LowStockReportItem> LowStockItems { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand AddLineCommand { get; }
    public RelayCommand RemoveLineCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand PrintCommand { get; }
    public RelayCommand OpenReceiptFolderCommand { get; }

    public CategoryOption? SelectedSupplier
    {
        get => _selectedSupplier;
        set => SetProperty(ref _selectedSupplier, value);
    }

    public ReceiptLine? SelectedLine
    {
        get => _selectedLine;
        set
        {
            if (SetProperty(ref _selectedLine, value))
            {
                RemoveLineCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ProductCode
    {
        get => _productCode;
        set => SetProperty(ref _productCode, value);
    }

    public string QuantityText
    {
        get => _quantityText;
        set => SetProperty(ref _quantityText, value);
    }

    public string UnitCostText
    {
        get => _unitCostText;
        set => SetProperty(ref _unitCostText, value);
    }

    public string ReceiptDateText
    {
        get => _receiptDateText;
        set => SetProperty(ref _receiptDateText, value);
    }

    public string DeliveredBy
    {
        get => _deliveredBy;
        set => SetProperty(ref _deliveredBy, value);
    }

    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public decimal Total
    {
        get => _total;
        private set => SetProperty(ref _total, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public int LineCount => Lines.Count;
    public int LowStockCount => LowStockItems.Count;

    private async Task LoadAsync()
    {
        _products.Clear();
        _products.AddRange(await _productService.GetAllAsync());
        Suppliers.ResetWith(await _inventoryService.GetSuppliersAsync());
        await RefreshLowStockAsync();
        SelectedSupplier ??= Suppliers.FirstOrDefault();

        _employeeId = _sessionService.CurrentUser?.Id ?? 0;
    }

    private async Task AddLineAsync()
    {
        if (!int.TryParse(QuantityText, out var quantity) || quantity <= 0)
        {
            StatusMessage = "Số lượng nhập phải lớn hơn 0.";
            return;
        }

        if (!decimal.TryParse(UnitCostText, out var unitCost) || unitCost <= 0)
        {
            StatusMessage = "Đơn giá nhập phải lớn hơn 0.";
            return;
        }

        var code = ProductCode.Trim();
        var product = _products.FirstOrDefault(p =>
            p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

        if (product is null && !string.IsNullOrWhiteSpace(code))
        {
            var searchResult = await _productService.SearchAsync(code);
            product = searchResult.FirstOrDefault(p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
                ?? searchResult.FirstOrDefault();
        }

        if (product is null)
        {
            StatusMessage = "Không tìm thấy sản phẩm.";
            return;
        }

        var existing = Lines.FirstOrDefault(line => line.ProductId == product.Id);
        if (existing is not null)
        {
            existing.Quantity += quantity;
            existing.UnitCost = unitCost;
        }
        else
        {
            Lines.Add(new ReceiptLine(product.Id, product.Code, product.Name, product.Unit, quantity, unitCost));
        }

        ProductCode = "";
        QuantityText = "";
        UnitCostText = "";
        StatusMessage = "Đã thêm mặt hàng vào phiếu nhập.";
        RecalculateTotal();
        SaveCommand.RaiseCanExecuteChanged();
        PrintCommand.RaiseCanExecuteChanged();
    }

    private async Task SaveAsync()
    {
        if (SelectedSupplier is null)
        {
            StatusMessage = "Vui lòng chọn nhà cung cấp.";
            return;
        }

        if (_employeeId <= 0)
        {
            StatusMessage = "Không tìm thấy nhân viên thủ kho.";
            return;
        }

        if (!TryParseReceiptDate(out var receiptDate))
        {
            StatusMessage = "Ngày nhập không hợp lệ. Vui lòng nhập theo định dạng dd/MM/yyyy.";
            return;
        }

        var result = await _inventoryService.CreateReceiptAsync(new CreateInventoryReceiptInput(
            SelectedSupplier.Id,
            _employeeId,
            receiptDate,
            DeliveredBy,
            Note,
            Lines.Select(line => new InventoryReceiptLineInput(line.ProductId, line.Quantity, line.UnitCost)).ToList()));

        StatusMessage = result.IsValid ? "Đã lưu phiếu nhập." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            _printData = new ReceiptPrintData(
                SelectedSupplier.Name,
                _sessionService.CurrentUser?.FullName ?? "",
                DeliveredBy,
                Note,
                DateTime.Now,
                Total,
                Lines.ToList());
            PrintCommand.RaiseCanExecuteChanged();
            ClearReceipt();
            await LoadAsync();
        }
    }

    private void PrintReceipt()
    {
        if (SelectedSupplier is null)
        {
            StatusMessage = "Vui lòng chọn nhà cung cấp.";
            return;
        }

        if (!TryParseReceiptDate(out var receiptDate))
        {
            StatusMessage = "Ngày nhập không hợp lệ. Vui lòng nhập theo định dạng dd/MM/yyyy.";
            return;
        }

        try
        {
            _inventoryService.Print(new InventoryReceiptPrintout(
                receiptDate,
                SelectedSupplier.Name,
                DeliveredBy,
                Note,
                Total,
                Lines.Select(line => new InventoryReceiptPrintLine(
                    line.Code,
                    line.Name,
                    line.Unit,
                    line.Quantity,
                    line.UnitCost,
                    line.LineTotal)).ToList()));
            StatusMessage = "Đã gửi phiếu nhập đến máy in hoặc đã hủy hộp thoại in.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Không thể in phiếu nhập: {ex.Message}";
        }
    }

    private void OpenReceiptFolder()
    {
        _inventoryService.OpenReceiptFolder();
    }

    private void RemoveSelectedLine()
    {
        if (SelectedLine is null)
        {
            return;
        }

        Lines.Remove(SelectedLine);
        SelectedLine = null;
        RecalculateTotal();
        SaveCommand.RaiseCanExecuteChanged();
        PrintCommand.RaiseCanExecuteChanged();
    }

    private void ClearReceipt()
    {
        Lines.Clear();
        ProductCode = "";
        QuantityText = "";
        UnitCostText = "";
        ReceiptDateText = DateTime.Today.ToString("dd/MM/yyyy");
        DeliveredBy = "";
        Note = "";
        RecalculateTotal();
        SaveCommand.RaiseCanExecuteChanged();
        PrintCommand.RaiseCanExecuteChanged();
    }

    private void RecalculateTotal()
    {
        Total = Lines.Sum(line => line.LineTotal);
        OnPropertyChanged(nameof(LineCount));
    }

    private bool TryParseReceiptDate(out DateTime receiptDate)
    {
        if (DateTime.TryParseExact(
                ReceiptDateText.Trim(),
                "dd/MM/yyyy",
                CultureInfo.GetCultureInfo("vi-VN"),
                DateTimeStyles.None,
                out var parsed))
        {
            receiptDate = parsed.Date.Add(DateTime.Now.TimeOfDay);
            return true;
        }

        return DateTime.TryParse(
            ReceiptDateText,
            CultureInfo.GetCultureInfo("vi-VN"),
            DateTimeStyles.None,
            out receiptDate);
    }

    private async Task RefreshLowStockAsync()
    {
        LowStockItems.ResetWith(await _inventoryService.GetLowStockAsync());
        OnPropertyChanged(nameof(LowStockCount));
    }

    private sealed record ReceiptPrintData(
        string SupplierName,
        string EmployeeName,
        string DeliveredBy,
        string Note,
        DateTime Date,
        decimal Total,
        IReadOnlyList<ReceiptLine> Lines);
}

public sealed class ReceiptLine : ViewModelBase
{
    private int _quantity;
    private decimal _unitCost;

    public ReceiptLine(int productId, string code, string name, string unit, int quantity, decimal unitCost)
    {
        ProductId = productId;
        Code = code;
        Name = name;
        Unit = unit;
        _quantity = quantity;
        _unitCost = unitCost;
    }

    public int ProductId { get; }
    public string Code { get; }
    public string Name { get; }
    public string Unit { get; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
            {
                OnPropertyChanged(nameof(LineTotal));
            }
        }
    }

    public decimal UnitCost
    {
        get => _unitCost;
        set
        {
            if (SetProperty(ref _unitCost, value))
            {
                OnPropertyChanged(nameof(LineTotal));
            }
        }
    }

    public decimal LineTotal => Quantity * UnitCost;
}

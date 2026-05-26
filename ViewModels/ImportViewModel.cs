using System.Collections.ObjectModel;
using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm.ViewModels;

public sealed class ImportViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private readonly IInventoryService _inventoryService;
    private readonly IAccountService _accountService;
    private readonly List<ProductListItem> _products = new();
    private int _employeeId;
    private CategoryOption? _selectedSupplier;
    private ReceiptLine? _selectedLine;
    private string _productCode = "";
    private string _quantityText = "1";
    private string _unitCostText = "0";
    private string _deliveredBy = "";
    private string _note = "";
    private decimal _total;
    private string _statusMessage = "";

    public ImportViewModel(
        IProductService productService,
        IInventoryService inventoryService,
        IAccountService accountService)
    {
        _productService = productService;
        _inventoryService = inventoryService;
        _accountService = accountService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddLineCommand = new AsyncRelayCommand(AddLineAsync);
        RemoveLineCommand = new RelayCommand(RemoveSelectedLine, () => SelectedLine is not null);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => Lines.Count > 0);
        NewCommand = new RelayCommand(ClearReceipt);
    }

    public ObservableCollection<CategoryOption> Suppliers { get; } = new();
    public ObservableCollection<ReceiptLine> Lines { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand AddLineCommand { get; }
    public RelayCommand RemoveLineCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand NewCommand { get; }

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

    private async Task LoadAsync()
    {
        _products.Clear();
        _products.AddRange(await _productService.GetAllAsync());
        Suppliers.ResetWith(await _inventoryService.GetSuppliersAsync());
        SelectedSupplier ??= Suppliers.FirstOrDefault();

        var accounts = await _accountService.GetAllAsync();
        _employeeId = accounts.FirstOrDefault(a => a.Role == "Storekeeper" && a.IsActive)?.Id
            ?? accounts.FirstOrDefault(a => a.IsActive)?.Id
            ?? 0;
    }

    private async Task AddLineAsync()
    {
        if (!int.TryParse(QuantityText, out var quantity) || quantity <= 0)
        {
            StatusMessage = "Số lượng nhập phải lớn hơn 0.";
            return;
        }

        if (!decimal.TryParse(UnitCostText, out var unitCost) || unitCost < 0)
        {
            StatusMessage = "Đơn giá nhập không hợp lệ.";
            return;
        }

        var code = ProductCode.Trim();
        var product = _products.FirstOrDefault(p =>
            p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

        if (product is null)
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
        QuantityText = "1";
        UnitCostText = "0";
        StatusMessage = "Đã thêm mặt hàng vào phiếu nhập.";
        RecalculateTotal();
        SaveCommand.RaiseCanExecuteChanged();
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

        var result = await _inventoryService.CreateReceiptAsync(new CreateInventoryReceiptInput(
            SelectedSupplier.Id,
            _employeeId,
            DeliveredBy,
            Note,
            Lines.Select(line => new InventoryReceiptLineInput(line.ProductId, line.Quantity, line.UnitCost)).ToList()));

        StatusMessage = result.IsValid ? "Đã lưu phiếu nhập." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            ClearReceipt();
            await LoadAsync();
        }
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
    }

    private void ClearReceipt()
    {
        Lines.Clear();
        ProductCode = "";
        QuantityText = "1";
        UnitCostText = "0";
        DeliveredBy = "";
        Note = "";
        RecalculateTotal();
        SaveCommand.RaiseCanExecuteChanged();
    }

    private void RecalculateTotal()
    {
        Total = Lines.Sum(line => line.LineTotal);
        OnPropertyChanged(nameof(LineCount));
    }
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

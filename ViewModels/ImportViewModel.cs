using System.Collections.ObjectModel;
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
    private string _productName = "";
    private string _quantityText = "";
    private string _deliveredBy = "";
    private string _note = "";
    private decimal _total;
    private string _statusMessage = "";
    private int? _lastReceiptId;
    private bool _isSuggestionPopupOpen;
    private ProductListItem? _selectedSuggestion;
    private DateTime? _receiptDate;

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
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        NewCommand = new RelayCommand(ClearReceipt);
        PrintCommand = new RelayCommand(PrintReceipt, () => _lastReceiptId.HasValue);
        SelectSuggestionCommand = new RelayCommand(() => SelectSuggestion(_selectedSuggestion), () => _selectedSuggestion is not null);
    }

    public ObservableCollection<CategoryOption> Suppliers { get; } = new();
    public ObservableCollection<ReceiptLine> Lines { get; } = new();
    public ObservableCollection<ProductListItem> SearchSuggestions { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand AddLineCommand { get; }
    public RelayCommand RemoveLineCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand PrintCommand { get; }
    public RelayCommand SelectSuggestionCommand { get; }

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
        set
        {
            if (SetProperty(ref _productCode, value))
            {
                UpdateSuggestions();
            }
        }
    }

    public string ProductName
    {
        get => _productName;
        set
        {
            if (SetProperty(ref _productName, value))
            {
                UpdateSuggestions();
            }
        }
    }

    public string QuantityText
    {
        get => _quantityText;
        set => SetProperty(ref _quantityText, value);
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

    public bool IsSuggestionPopupOpen
    {
        get => _isSuggestionPopupOpen;
        set => SetProperty(ref _isSuggestionPopupOpen, value);
    }

    public ProductListItem? SelectedSuggestion
    {
        get => _selectedSuggestion;
        set
        {
            if (SetProperty(ref _selectedSuggestion, value) && value is not null)
            {
                SelectSuggestion(value);
                SelectSuggestionCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public DateTime? ReceiptDate
    {
        get => _receiptDate;
        set => SetProperty(ref _receiptDate, value);
    }

    private async Task LoadAsync()
    {
        _products.Clear();
        _products.AddRange(await _productService.GetAllAsync());
        Suppliers.ResetWith(await _inventoryService.GetSuppliersAsync());
        SelectedSupplier ??= Suppliers.FirstOrDefault();
        ReceiptDate ??= DateTime.Today;

        _employeeId = _sessionService.CurrentUser?.Id ?? 0;
    }

    private async Task AddLineAsync()
    {
        if (!int.TryParse(QuantityText, out var quantity) || quantity <= 0)
        {
            StatusMessage = "Số lượng nhập phải lớn hơn 0.";
            return;
        }

        var code = ProductCode.Trim();
        var name = ProductName.Trim();
        var product = _products.FirstOrDefault(p =>
            p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

        if (product is null && !string.IsNullOrEmpty(name))
        {
            product = _products.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        if (product is null)
        {
            if (!string.IsNullOrEmpty(code))
            {
                var searchResult = await _productService.SearchAsync(code);
                product = searchResult.FirstOrDefault(p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
                    ?? searchResult.FirstOrDefault();
            }
            else if (!string.IsNullOrEmpty(name))
            {
                var searchResult = await _productService.SearchAsync(name);
                product = searchResult.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ?? searchResult.FirstOrDefault();
            }
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
        }
        else
        {
            Lines.Add(new ReceiptLine(product.Id, product.Code, product.Name, product.Unit, quantity, product.Price));
        }

        ProductCode = "";
        ProductName = "";
        QuantityText = "";
        IsSuggestionPopupOpen = false;
        StatusMessage = "Đã thêm mặt hàng vào phiếu nhập.";
        RecalculateTotal();
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

        if (Lines.Count == 0)
        {
            StatusMessage = "Vui lòng thêm ít nhất một mặt hàng.";
            return;
        }

        var result = await _inventoryService.CreateReceiptAsync(new CreateInventoryReceiptInput(
            SelectedSupplier.Id,
            _employeeId,
            DeliveredBy,
            Note,
            Lines.Select(line => new InventoryReceiptLineInput(line.ProductId, line.Quantity, line.UnitCost)).ToList()));

        if (result.IsValid)
        {
            var receiptId = result.Value;
            ClearReceipt();
            _lastReceiptId = receiptId;
            StatusMessage = $"Đã lưu phiếu nhập #{_lastReceiptId}.";
            PrintCommand.RaiseCanExecuteChanged();
            await LoadAsync();
        }
        else
        {
            StatusMessage = result.ErrorMessage ?? "";
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
    }

    private void ClearReceipt()
    {
        Lines.Clear();
        ProductCode = "";
        ProductName = "";
        QuantityText = "";
        DeliveredBy = "";
        Note = "";
        _lastReceiptId = null;
        PrintCommand.RaiseCanExecuteChanged();
        RecalculateTotal();
    }

    private void PrintReceipt()
    {
        if (!_lastReceiptId.HasValue)
        {
            StatusMessage = "Chưa có phiếu nhập nào để in.";
            return;
        }

        StatusMessage = $"Phiếu nhập #{_lastReceiptId} đã lưu. Tính năng in đang phát triển.";
    }

    private void RecalculateTotal()
    {
        Total = Lines.Sum(line => line.LineTotal);
        OnPropertyChanged(nameof(LineCount));
    }

    private void UpdateSuggestions()
    {
        var code = ProductCode.Trim().ToLowerInvariant();
        var name = ProductName.Trim().ToLowerInvariant();

        IEnumerable<ProductListItem> query = _products;

        if (!string.IsNullOrEmpty(code))
            query = query.Where(p => p.Code.ToLowerInvariant().Contains(code));

        if (!string.IsNullOrEmpty(name))
            query = query.Where(p => p.Name.ToLowerInvariant().Contains(name));

        var results = query.Take(10).ToList();
        SearchSuggestions.ResetWith(results);
        IsSuggestionPopupOpen = results.Count > 0 && (!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(name));
    }

    private void SelectSuggestion(ProductListItem? product)
    {
        if (product is null)
            return;

        ProductCode = product.Code;
        ProductName = product.Name;
        IsSuggestionPopupOpen = false;
    }
}

public sealed class ReceiptLine : ViewModelBase
{
    private int _quantity;

    public ReceiptLine(int productId, string code, string name, string unit, int quantity, decimal unitCost)
    {
        ProductId = productId;
        Code = code;
        Name = name;
        Unit = unit;
        _quantity = quantity;
        UnitCost = unitCost;
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

    public decimal UnitCost { get; }

    public decimal LineTotal => Quantity * UnitCost;
}

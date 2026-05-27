using System.Collections.ObjectModel;
using System.Globalization;
using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm.ViewModels;

public sealed class SalesViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;
    private readonly IReceiptService _receiptService;
    private readonly IUserSessionService _sessionService;
    private readonly List<ProductListItem> _products = new();
    private int _employeeId;
    private string _productCode = "";
    private string _productName = "";
    private string _quantityText = "1";
    private CustomerListItem? _selectedCustomer;
    private CartLine? _selectedCartLine;
    private string _discountText = "0";
    private string _paymentText = "0";
    private decimal _subtotal;
    private decimal _vat;
    private decimal _total;
    private decimal _change;
    private string _statusMessage = "";
    private OrderReceipt? _lastReceipt;
    private decimal _vatRate;

    public SalesViewModel(
        IProductService productService,
        ICustomerService customerService,
        IOrderService orderService,
        IReceiptService receiptService,
        IUserSessionService sessionService)
    {
        _productService = productService;
        _customerService = customerService;
        _orderService = orderService;
        _receiptService = receiptService;
        _sessionService = sessionService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddProductCommand = new AsyncRelayCommand(AddProductAsync);
        RemoveLineCommand = new RelayCommand(RemoveSelectedLine);
        CheckoutCommand = new AsyncRelayCommand(CheckoutAsync, () => CartLines.Count > 0);
        NewOrderCommand = new RelayCommand(ClearOrder);
        ExportReceiptCommand = new AsyncRelayCommand(ExportReceiptAsync, () => LastReceipt is not null);
        PrintReceiptCommand = new RelayCommand(PrintReceipt, () => LastReceipt is not null);
        SelectSuggestionCommand = new RelayCommand(() => SelectSuggestion(_selectedSuggestion), () => _selectedSuggestion is not null);
    }

    public ObservableCollection<CustomerListItem> Customers { get; } = new();
    public ObservableCollection<CartLine> CartLines { get; } = new();
    public ObservableCollection<ProductListItem> SearchSuggestions { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand AddProductCommand { get; }
    public RelayCommand RemoveLineCommand { get; }
    public AsyncRelayCommand CheckoutCommand { get; }
    public RelayCommand NewOrderCommand { get; }
    public AsyncRelayCommand ExportReceiptCommand { get; }
    public RelayCommand PrintReceiptCommand { get; }
    public RelayCommand SelectSuggestionCommand { get; }

    public string ProductCode
    {
        get => _productCode;
        set
        {
            if (SetProperty(ref _productCode, value))
                UpdateSuggestions();
        }
    }

    public string ProductName
    {
        get => _productName;
        set
        {
            if (SetProperty(ref _productName, value))
                UpdateSuggestions();
        }
    }

    public string QuantityText
    {
        get => _quantityText;
        set => SetProperty(ref _quantityText, value);
    }

    public CustomerListItem? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    public CartLine? SelectedCartLine
    {
        get => _selectedCartLine;
        set
        {
            SetProperty(ref _selectedCartLine, value);
        }
    }

    public string DiscountText
    {
        get => _discountText;
        set
        {
            if (SetProperty(ref _discountText, value))
            {
                RecalculateTotals();
            }
        }
    }

    public string PaymentText
    {
        get => _paymentText;
        set
        {
            if (SetProperty(ref _paymentText, value))
            {
                UpdateChange();
            }
        }
    }

    public decimal Subtotal
    {
        get => _subtotal;
        private set => SetProperty(ref _subtotal, value);
    }

    public decimal Vat
    {
        get => _vat;
        private set => SetProperty(ref _vat, value);
    }

    public decimal Total
    {
        get => _total;
        private set => SetProperty(ref _total, value);
    }

    public decimal Change
    {
        get => _change;
        private set => SetProperty(ref _change, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string VatRateText => $"VAT ({_vatRate:N0}%):";

    private bool _isSuggestionPopupOpen;
    public bool IsSuggestionPopupOpen
    {
        get => _isSuggestionPopupOpen;
        set => SetProperty(ref _isSuggestionPopupOpen, value);
    }

    private ProductListItem? _selectedSuggestion;
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

    public OrderReceipt? LastReceipt
    {
        get => _lastReceipt;
        private set
        {
            if (SetProperty(ref _lastReceipt, value))
            {
                ExportReceiptCommand.RaiseCanExecuteChanged();
                PrintReceiptCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private async Task LoadAsync()
    {
        _products.Clear();
        _products.AddRange(await _productService.GetAllAsync());
        Customers.ResetWith(await _customerService.GetAllAsync());
        SelectedCustomer = Customers.FirstOrDefault(customer => customer.Name == "Khách lẻ")
            ?? Customers.FirstOrDefault();

        _employeeId = _sessionService.CurrentUser?.Id ?? 0;
        _vatRate = GetVatRate();
    }

    private static decimal GetVatRate()
    {
        var envValue = Environment.GetEnvironmentVariable("QLBH_VAT_RATE");
        return decimal.TryParse(envValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate) && rate > 0 ? rate : 8m;
    }

    private async Task AddProductAsync()
    {
        if (!int.TryParse(QuantityText, out var quantity) || quantity <= 0)
        {
            StatusMessage = "Số lượng phải lớn hơn 0.";
            return;
        }

        var code = ProductCode.Trim();
        var name = ProductName.Trim();
        ProductListItem? product = null;

        if (!string.IsNullOrEmpty(code))
            product = _products.FirstOrDefault(p =>
                p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

        if (product is null && !string.IsNullOrEmpty(name))
            product = _products.FirstOrDefault(p =>
                p.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase));

        if (product is null)
        {
            StatusMessage = "Không tìm thấy sản phẩm.";
            return;
        }

        var existing = CartLines.FirstOrDefault(line => line.ProductId == product.Id);
        var nextQuantity = (existing?.Quantity ?? 0) + quantity;
        if (nextQuantity > product.Stock)
        {
            StatusMessage = $"Sản phẩm {product.Code} chỉ còn {product.Stock}.";
            return;
        }

        if (existing is not null)
        {
            existing.Quantity = nextQuantity;
        }
        else
        {
            CartLines.Add(new CartLine(product.Id, product.Code, product.Name, product.Unit, product.Price, quantity));
        }

        ProductCode = "";
        ProductName = "";
        QuantityText = "1";
        IsSuggestionPopupOpen = false;
        StatusMessage = "Đã thêm sản phẩm vào giỏ.";
        LastReceipt = null;
        RecalculateTotals();
        CheckoutCommand.RaiseCanExecuteChanged();
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

    private async Task CheckoutAsync()
    {
        if (SelectedCustomer is null)
        {
            StatusMessage = "Vui lòng chọn khách hàng.";
            return;
        }

        if (_employeeId <= 0)
        {
            StatusMessage = "Không tìm thấy nhân viên thu ngân.";
            return;
        }

        var paidAmount = ParseMoney(PaymentText);
        if (paidAmount < Total)
        {
            StatusMessage = "Số tiền khách thanh toán chưa đủ.";
            return;
        }

        var discount = ParseMoney(DiscountText);
        var result = await _orderService.CreateOrderAsync(new CreateOrderInput(
            SelectedCustomer.Id,
            _employeeId,
            discount,
            _vatRate,
            paidAmount,
            CartLines.Select(line => new OrderLineInput(line.ProductId, line.Quantity)).ToList()));

        if (!result.IsValid)
        {
            StatusMessage = result.ErrorMessage ?? "";
            return;
        }

        LastReceipt = await _orderService.GetReceiptAsync(result.Value!.OrderId, paidAmount);
        StatusMessage = $"Đã thanh toán đơn #{result.Value.OrderId}. Có thể in hoặc xuất hóa đơn.";
        ClearCartOnly();
        await LoadAsync();
    }

    private async Task ExportReceiptAsync()
    {
        if (LastReceipt is null)
        {
            return;
        }

        var path = await _receiptService.ExportAsync(LastReceipt);
        StatusMessage = $"Đã xuất hóa đơn: {path}";
    }

    private void PrintReceipt()
    {
        if (LastReceipt is null)
        {
            return;
        }

        try
        {
            _receiptService.Print(LastReceipt);
            StatusMessage = "Đã gửi hóa đơn đến máy in hoặc đã hủy hộp thoại in.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Không thể in hóa đơn: {ex.Message}";
        }
    }

    private void RemoveSelectedLine()
    {
        if (SelectedCartLine is null)
        {
            StatusMessage = "Vui lòng chọn mặt hàng cần xóa.";
            return;
        }

        CartLines.Remove(SelectedCartLine);
        SelectedCartLine = null;
        LastReceipt = null;
        RecalculateTotals();
        CheckoutCommand.RaiseCanExecuteChanged();
    }

    private void ClearOrder()
    {
        LastReceipt = null;
        ClearCartOnly();
    }

    private void ClearCartOnly()
    {
        CartLines.Clear();
        ProductCode = "";
        ProductName = "";
        QuantityText = "1";
        DiscountText = "0";
        PaymentText = "0";
        IsSuggestionPopupOpen = false;
        RecalculateTotals();
        CheckoutCommand.RaiseCanExecuteChanged();
    }

    private void RecalculateTotals()
    {
        Subtotal = CartLines.Sum(line => line.LineTotal);
        var discount = Math.Min(ParseMoney(DiscountText), Subtotal);
        Vat = decimal.Round((Subtotal - discount) * _vatRate / 100m, 2);
        Total = Subtotal - discount + Vat;

        var paid = ParseMoney(PaymentText);
        if (paid <= 0m || paid < Total)
        {
            PaymentText = Total.ToString("0.##");
        }

        UpdateChange();
    }

    private void UpdateChange()
    {
        Change = Math.Max(0m, ParseMoney(PaymentText) - Total);
    }

    private static decimal ParseMoney(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0m;
        value = value.Trim().Replace(",", "");
        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? Math.Max(0m, parsed)
            : 0m;
    }
}

public sealed class CartLine : ViewModelBase
{
    private int _quantity;

    public CartLine(int productId, string code, string name, string unit, decimal unitPrice, int quantity)
    {
        ProductId = productId;
        Code = code;
        Name = name;
        Unit = unit;
        UnitPrice = unitPrice;
        _quantity = quantity;
    }

    public int ProductId { get; }
    public string Code { get; }
    public string Name { get; }
    public string Unit { get; }
    public decimal UnitPrice { get; }

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

    public decimal LineTotal => UnitPrice * Quantity;
}

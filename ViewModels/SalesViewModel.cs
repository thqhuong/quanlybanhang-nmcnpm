using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
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
    private bool _isApplyingProductSuggestion;

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
        RemoveLineCommand = new RelayCommand(RemoveSelectedLine, () => SelectedCartLine is not null);
        CheckoutCommand = new AsyncRelayCommand(CheckoutAsync, () => CartLines.Count > 0);
        NewOrderCommand = new RelayCommand(ClearOrder);
        ExportReceiptCommand = new AsyncRelayCommand(ExportReceiptAsync, () => LastReceipt is not null);
        PrintReceiptCommand = new RelayCommand(PrintReceipt, () => LastReceipt is not null);
        OpenReceiptFolderCommand = new RelayCommand(OpenReceiptFolder);
    }

    public ObservableCollection<CustomerListItem> Customers { get; } = new();
    public ObservableCollection<CartLine> CartLines { get; } = new();
    public ObservableCollection<ProductSuggestion> ProductNameSuggestions { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand AddProductCommand { get; }
    public RelayCommand RemoveLineCommand { get; }
    public AsyncRelayCommand CheckoutCommand { get; }
    public RelayCommand NewOrderCommand { get; }
    public AsyncRelayCommand ExportReceiptCommand { get; }
    public RelayCommand PrintReceiptCommand { get; }
    public RelayCommand OpenReceiptFolderCommand { get; }

    public string ProductCode
    {
        get => _productCode;
        set => SetProperty(ref _productCode, value);
    }

    public string ProductName
    {
        get => _productName;
        set
        {
            if (SetProperty(ref _productName, value))
            {
                if (!_isApplyingProductSuggestion)
                {
                    ProductCode = "";
                    UpdateProductNameSuggestions();
                }
            }
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
            if (SetProperty(ref _selectedCartLine, value))
            {
                RemoveLineCommand.RaiseCanExecuteChanged();
            }
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
        ResetProductNameSuggestions(_products);

        Customers.ResetWith(await _customerService.GetAllAsync());
        SelectedCustomer = Customers.FirstOrDefault(customer => customer.Name == "Khách lẻ")
            ?? Customers.FirstOrDefault();

        _employeeId = _sessionService.CurrentUser?.Id ?? 0;
    }

    private async Task AddProductAsync()
    {
        if (!int.TryParse(QuantityText, out var quantity) || quantity <= 0)
        {
            StatusMessage = "Số lượng phải lớn hơn 0.";
            return;
        }

        var product = await FindProductAsync();
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

        QuantityText = "1";
        StatusMessage = "Đã thêm sản phẩm vào giỏ.";
        LastReceipt = null;
        RecalculateTotals();
        CheckoutCommand.RaiseCanExecuteChanged();
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
            8m,
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

    private void OpenReceiptFolder()
    {
        _receiptService.OpenReceiptFolder();
    }

    private void RemoveSelectedLine()
    {
        if (SelectedCartLine is null)
        {
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
        RecalculateTotals();
        CheckoutCommand.RaiseCanExecuteChanged();
    }

    private async Task<ProductListItem?> FindProductAsync()
    {
        var code = ProductCode.Trim();
        var name = ProductName.Trim();
        if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            var exactCode = _products.FirstOrDefault(product =>
                product.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (exactCode is not null)
            {
                return exactCode;
            }

            var searchResult = await _productService.SearchAsync(code);
            return searchResult.FirstOrDefault(product =>
                product.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        }

        var normalizedName = NormalizeForSearch(name);
        return _products.FirstOrDefault(product => NormalizeForSearch(product.Name) == normalizedName)
            ?? _products.FirstOrDefault(product => NormalizeForSearch(product.Name).Contains(normalizedName));
    }

    private void UpdateProductNameSuggestions()
    {
        ProductNameSuggestions.Clear();
        var normalized = NormalizeForSearch(ProductName);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            ResetProductNameSuggestions(_products);
            return;
        }

        ResetProductNameSuggestions(_products
            .Where(product => NormalizeForSearch(product.Name).Contains(normalized))
            .OrderBy(product => product.Name)
            .Take(12));
    }

    public void ApplyProductSuggestion(ProductSuggestion suggestion)
    {
        _isApplyingProductSuggestion = true;
        ProductCode = suggestion.Code;
        ProductName = suggestion.Name;
        _isApplyingProductSuggestion = false;
        ProductNameSuggestions.ResetWith(new[] { suggestion });
    }

    private void ResetProductNameSuggestions(IEnumerable<ProductListItem> products)
    {
        ProductNameSuggestions.Clear();
        foreach (var product in products.OrderBy(product => product.Name).Take(20))
        {
            ProductNameSuggestions.Add(new ProductSuggestion(product.Id, product.Code, product.Name));
        }
    }

    private void RecalculateTotals()
    {
        Subtotal = CartLines.Sum(line => line.LineTotal);
        var discount = Math.Min(ParseMoney(DiscountText), Subtotal);
        Vat = decimal.Round((Subtotal - discount) * 0.08m, 2);
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
        return decimal.TryParse(value, out var parsed) ? Math.Max(0m, parsed) : 0m;
    }

    private static string NormalizeForSearch(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch == 'đ' ? 'd' : ch);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
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

public sealed record ProductSuggestion(int ProductId, string Code, string Name)
{
    public string DisplayText => $"{Code} - {Name}";
}

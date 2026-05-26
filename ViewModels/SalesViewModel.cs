using System.Collections.ObjectModel;
using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm.ViewModels;

public sealed class SalesViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly IAccountService _accountService;
    private readonly IOrderService _orderService;
    private readonly IUserSessionService _sessionService;
    private readonly List<ProductListItem> _products = new();
    private int _employeeId;
    private string _productCode = "";
    private string _quantityText = "1";
    private CustomerListItem? _selectedCustomer;
    private CartLine? _selectedCartLine;
    private string _discountText = "0";
    private string _paymentText = "0";
    private decimal _subtotal;
    private decimal _vat;
    private decimal _total;
    private string _statusMessage = "";

    public SalesViewModel(
        IProductService productService,
        ICustomerService customerService,
        IAccountService accountService,
        IOrderService orderService,
        IUserSessionService sessionService)
    {
        _productService = productService;
        _customerService = customerService;
        _accountService = accountService;
        _orderService = orderService;
        _sessionService = sessionService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddProductCommand = new AsyncRelayCommand(AddProductAsync);
        RemoveLineCommand = new RelayCommand(RemoveSelectedLine, () => SelectedCartLine is not null);
        CheckoutCommand = new AsyncRelayCommand(CheckoutAsync, () => CartLines.Count > 0);
        NewOrderCommand = new RelayCommand(ClearOrder);
    }

    public ObservableCollection<CustomerListItem> Customers { get; } = new();
    public ObservableCollection<CartLine> CartLines { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand AddProductCommand { get; }
    public RelayCommand RemoveLineCommand { get; }
    public AsyncRelayCommand CheckoutCommand { get; }
    public RelayCommand NewOrderCommand { get; }

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
        set => SetProperty(ref _paymentText, value);
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

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private async Task LoadAsync()
    {
        _products.Clear();
        _products.AddRange(await _productService.GetAllAsync());
        Customers.ResetWith(await _customerService.GetAllAsync());
        SelectedCustomer = Customers.FirstOrDefault(c => c.Name == "Khách lẻ") ?? Customers.FirstOrDefault();
        _employeeId = _sessionService.CurrentUser?.Id ?? 0;
    }

    private async Task AddProductAsync()
    {
        if (!int.TryParse(QuantityText, out var quantity) || quantity <= 0)
        {
            StatusMessage = "Số lượng phải lớn hơn 0.";
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
        QuantityText = "1";
        StatusMessage = "Đã thêm sản phẩm vào giỏ.";
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

        var discount = ParseMoney(DiscountText);
        var result = await _orderService.CreateOrderAsync(new CreateOrderInput(
            SelectedCustomer.Id,
            _employeeId,
            discount,
            8m,
            CartLines.Select(line => new OrderLineInput(line.ProductId, line.Quantity)).ToList()));

        StatusMessage = result.IsValid
            ? $"Đã thanh toán đơn #{result.Value!.OrderId}."
            : result.ErrorMessage ?? "";

        if (result.IsValid)
        {
            ClearOrder();
            await LoadAsync();
        }
    }

    private void RemoveSelectedLine()
    {
        if (SelectedCartLine is null)
        {
            return;
        }

        CartLines.Remove(SelectedCartLine);
        SelectedCartLine = null;
        RecalculateTotals();
        CheckoutCommand.RaiseCanExecuteChanged();
    }

    private void ClearOrder()
    {
        CartLines.Clear();
        ProductCode = "";
        QuantityText = "1";
        DiscountText = "0";
        PaymentText = "0";
        RecalculateTotals();
        CheckoutCommand.RaiseCanExecuteChanged();
    }

    private void RecalculateTotals()
    {
        Subtotal = CartLines.Sum(line => line.LineTotal);
        var discount = Math.Min(ParseMoney(DiscountText), Subtotal);
        Vat = decimal.Round((Subtotal - discount) * 0.08m, 2);
        Total = Subtotal - discount + Vat;
        PaymentText = Total.ToString("0.##");
    }

    private static decimal ParseMoney(string value)
    {
        return decimal.TryParse(value, out var parsed) ? Math.Max(0m, parsed) : 0m;
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

using System.Collections.ObjectModel;
using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm.ViewModels;

public sealed class ProductsViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private string _searchText = "";
    private CategoryOption? _selectedCategory;
    private ProductListItem? _selectedProduct;
    private string _code = "";
    private string _name = "";
    private string _unit = "Cái";
    private string _priceText = "0";
    private string _stockText = "0";
    private string _statusMessage = "";

    public ProductsViewModel(IProductService productService)
    {
        _productService = productService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        AddCommand = new AsyncRelayCommand(AddAsync);
        UpdateCommand = new AsyncRelayCommand(UpdateAsync, () => SelectedProduct is not null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedProduct is not null);
        NewCommand = new RelayCommand(ClearForm);
    }

    public ObservableCollection<ProductListItem> Products { get; } = new();
    public ObservableCollection<CategoryOption> Categories { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand SearchCommand { get; }
    public AsyncRelayCommand AddCommand { get; }
    public AsyncRelayCommand UpdateCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }
    public RelayCommand NewCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public CategoryOption? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public ProductListItem? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value))
            {
                FillForm(value);
                UpdateCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Unit
    {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }

    public string PriceText
    {
        get => _priceText;
        set => SetProperty(ref _priceText, value);
    }

    public string StockText
    {
        get => _stockText;
        set => SetProperty(ref _stockText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public int ProductCount => Products.Count;

    private async Task LoadAsync()
    {
        var categories = await _productService.GetCategoriesAsync();
        Categories.ResetWith(new[] { new CategoryOption(0, "Tất cả nhóm") }.Concat(categories));
        SelectedCategory ??= Categories.FirstOrDefault();
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        var categoryId = SelectedCategory?.Id;
        var products = await _productService.SearchAsync(SearchText, categoryId);
        Products.ResetWith(products);
        OnPropertyChanged(nameof(ProductCount));
        StatusMessage = $"Tổng số bản ghi: {ProductCount}";
    }

    private async Task AddAsync()
    {
        var input = BuildInput();
        if (input is null)
        {
            return;
        }

        var result = await _productService.CreateAsync(input);
        StatusMessage = result.IsValid ? "Đã thêm sản phẩm." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            ClearForm();
            await SearchAsync();
        }
    }

    private async Task UpdateAsync()
    {
        if (SelectedProduct is null)
        {
            return;
        }

        var input = BuildInput();
        if (input is null)
        {
            return;
        }

        var result = await _productService.UpdateAsync(SelectedProduct.Id, input);
        StatusMessage = result.IsValid ? "Đã cập nhật sản phẩm." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            await SearchAsync();
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedProduct is null)
        {
            return;
        }

        var result = await _productService.DeleteAsync(SelectedProduct.Id);
        StatusMessage = result.IsValid ? "Đã xóa sản phẩm." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            ClearForm();
            await SearchAsync();
        }
    }

    private ProductInput? BuildInput()
    {
        var category = SelectedCategory?.Id == 0
            ? Categories.FirstOrDefault(c => c.Id > 0)
            : SelectedCategory;

        if (category is null)
        {
            StatusMessage = "Vui lòng chọn nhóm hàng.";
            return null;
        }

        if (!decimal.TryParse(PriceText, out var price))
        {
            StatusMessage = "Đơn giá bán không hợp lệ.";
            return null;
        }

        if (!int.TryParse(StockText, out var stock))
        {
            StatusMessage = "Tồn kho không hợp lệ.";
            return null;
        }

        return new ProductInput(Code, Name, Unit, price, stock, category.Id, category.Name);
    }

    private void FillForm(ProductListItem? product)
    {
        if (product is null)
        {
            return;
        }

        Code = product.Code;
        Name = product.Name;
        Unit = product.Unit;
        PriceText = product.Price.ToString("0.##");
        StockText = product.Stock.ToString();
        SelectedCategory = Categories.FirstOrDefault(c => c.Name == product.Category) ?? SelectedCategory;
    }

    private void ClearForm()
    {
        SelectedProduct = null;
        Code = "";
        Name = "";
        Unit = "Cái";
        PriceText = "0";
        StockText = "0";
        StatusMessage = "";
    }
}

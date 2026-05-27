using System.Collections.ObjectModel;
using System.Windows;
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
    private string _unit = "";
    private string _priceText = "";
    private string _stockText = "";
    private string _statusMessage = "";

    public ProductsViewModel(IProductService productService)
    {
        _productService = productService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        AddCommand = new AsyncRelayCommand(AddAsync);
        UpdateCommand = new AsyncRelayCommand(UpdateAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        NewCommand = new RelayCommand(ClearForm);
        AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);
        CancelAddCategoryCommand = new RelayCommand(CancelAddCategory);
    }

    public ObservableCollection<ProductListItem> Products { get; } = new();
    public ObservableCollection<CategoryOption> Categories { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand SearchCommand { get; }
    public AsyncRelayCommand AddCommand { get; }
    public AsyncRelayCommand UpdateCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }
    public RelayCommand NewCommand { get; }
    public AsyncRelayCommand AddCategoryCommand { get; }
    public RelayCommand CancelAddCategoryCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private bool _isAddingCategory;
    private string _newCategoryName = "";

    public CategoryOption? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                if (value?.Id == -1)
                {
                    IsAddingCategory = true;
                    NewCategoryName = "";
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == 0);
                }
            }
        }
    }

    public bool IsAddingCategory
    {
        get => _isAddingCategory;
        set => SetProperty(ref _isAddingCategory, value);
    }

    public string NewCategoryName
    {
        get => _newCategoryName;
        set => SetProperty(ref _newCategoryName, value);
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
        Categories.ResetWith(new[] { new CategoryOption(0, "Tất cả nhóm") }.Concat(categories).Append(new CategoryOption(-1, "+ Thêm nhóm mới")));
        SelectedCategory ??= Categories.FirstOrDefault(c => c.Id == 0);
        await SearchAsync();
    }

    private async Task AddCategoryAsync()
    {
        var name = NewCategoryName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Vui lòng nhập tên nhóm hàng mới.";
            return;
        }

        var result = await _productService.CreateCategoryAsync(name);
        if (!result.IsValid)
        {
            StatusMessage = result.ErrorMessage ?? "";
            return;
        }

        IsAddingCategory = false;
        NewCategoryName = "";
        StatusMessage = $"Đã thêm nhóm '{result.Value!.Name}'.";
        var categories = await _productService.GetCategoriesAsync();
        Categories.ResetWith(new[] { new CategoryOption(0, "Tất cả nhóm") }.Concat(categories).Append(new CategoryOption(-1, "+ Thêm nhóm mới")));
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == result.Value.Id);
    }

    private void CancelAddCategory()
    {
        IsAddingCategory = false;
        NewCategoryName = "";
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
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == 0);
            await SearchAsync();
        }
    }

    private async Task UpdateAsync()
    {
        if (SelectedProduct is null)
        {
            StatusMessage = "Vui lòng chọn sản phẩm cần cập nhật.";
            return;
        }

        var input = BuildInput();
        if (input is null)
        {
            return;
        }

        var updatedId = SelectedProduct.Id;
        var result = await _productService.UpdateAsync(updatedId, input);
        StatusMessage = result.IsValid ? "Đã cập nhật sản phẩm." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == 0);
            await SearchAsync();
            SelectedProduct = Products.FirstOrDefault(p => p.Id == updatedId);
        }
        else if (result.ErrorMessage == "Không tìm thấy sản phẩm.")
        {
            StatusMessage = "Sản phẩm chưa có trong cơ sở dữ liệu.";
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedProduct is null)
        {
            StatusMessage = "Vui lòng chọn sản phẩm cần xóa.";
            return;
        }

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn xóa \"{SelectedProduct.Name}\"?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var deleted = SelectedProduct;
        var result = await _productService.DeleteAsync(deleted.Id);
        StatusMessage = result.IsValid ? "Đã xóa sản phẩm." : result.ErrorMessage ?? "";
        if (result.IsValid)
        {
            ClearForm();
            Products.Remove(deleted);
            OnPropertyChanged(nameof(ProductCount));
            StatusMessage = $"Đã xóa sản phẩm. Tổng số bản ghi: {ProductCount}";
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
        PriceText = product.Price == 0 ? "" : product.Price.ToString("0.##");
        StockText = product.Stock == 0 ? "" : product.Stock.ToString();
        SelectedCategory = Categories.FirstOrDefault(c => c.Name == product.Category) ?? SelectedCategory;
    }

    private void ClearForm()
    {
        SelectedProduct = null;
        Code = "";
        Name = "";
        Unit = "";
        PriceText = "";
        StockText = "";
        StatusMessage = "";
    }
}

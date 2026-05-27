namespace quanlybanhang_nmcnpm.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductListItem>> GetAllAsync();
    Task<IReadOnlyList<ProductListItem>> SearchAsync(string? searchText, int? categoryId = null);
    Task<IReadOnlyList<CategoryOption>> GetCategoriesAsync();
    Task<ValidationResult<CategoryOption>> CreateCategoryAsync(string name);
    Task<ValidationResult<ProductListItem>> CreateAsync(ProductInput input);
    Task<ValidationResult<ProductListItem>> UpdateAsync(int id, ProductInput input);
    Task<ValidationResult> DeleteAsync(int id);
}

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerListItem>> GetAllAsync();
    Task<IReadOnlyList<CustomerListItem>> SearchAsync(string? searchText);
    Task<ValidationResult<CustomerListItem>> CreateAsync(CustomerInput input);
    Task<ValidationResult<CustomerListItem>> UpdateAsync(int id, CustomerInput input);
    Task<ValidationResult> DeleteAsync(int id);
}

public interface IOrderService
{
    Task<ValidationResult<OrderSummary>> CreateOrderAsync(CreateOrderInput input);
    Task<OrderReceipt?> GetReceiptAsync(int orderId, decimal? paidAmount = null);
}

public interface IInventoryService
{
    Task<IReadOnlyList<CategoryOption>> GetSuppliersAsync();
    Task<ValidationResult<int>> CreateReceiptAsync(CreateInventoryReceiptInput input);
}

public interface IAccountService
{
    Task<IReadOnlyList<AccountListItem>> GetAllAsync();
    Task<IReadOnlyList<CategoryOption>> GetRolesAsync();
    Task<ValidationResult<AccountListItem>> CreateAsync(AccountInput input);
    Task<ValidationResult<AccountListItem>> UpdateAsync(int id, AccountInput input);
    Task<ValidationResult> SetActiveAsync(int id, bool isActive);
    Task<ValidationResult> DeleteAsync(int id);
    Task<ValidationResult<UserSession>> AuthenticateAsync(LoginInput input);
}

public interface IUserSessionService
{
    UserSession? CurrentUser { get; }
    bool IsInRole(params string[] roles);
    void Start(UserSession userSession);
    void Clear();
}

public interface IOverviewService
{
    Task<OverviewMetrics> GetMetricsAsync(DateTime? from = null, DateTime? to = null);
}

public interface IReceiptService
{
    Task<string> ExportAsync(OrderReceipt receipt);
    void Print(OrderReceipt receipt);
}

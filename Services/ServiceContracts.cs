namespace quanlybanhang_nmcnpm.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductListItem>> GetAllAsync();
    Task<IReadOnlyList<ProductListItem>> SearchAsync(string? searchText, int? categoryId = null);
    Task<IReadOnlyList<CategoryOption>> GetCategoriesAsync();
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
}

public interface IInventoryService
{
    Task<IReadOnlyList<CategoryOption>> GetSuppliersAsync();
    Task<ValidationResult<decimal>> CreateReceiptAsync(CreateInventoryReceiptInput input);
}

public interface IAccountService
{
    Task<IReadOnlyList<AccountListItem>> GetAllAsync();
    Task<IReadOnlyList<CategoryOption>> GetRolesAsync();
    Task<ValidationResult<AccountListItem>> CreateAsync(AccountInput input);
    Task<ValidationResult<AccountListItem>> UpdateAsync(int id, AccountInput input);
    Task<ValidationResult> SetActiveAsync(int id, bool isActive);
}

public interface IOverviewService
{
    Task<OverviewMetrics> GetMetricsAsync();
}

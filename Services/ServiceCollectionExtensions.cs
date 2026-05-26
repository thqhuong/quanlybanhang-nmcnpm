using Microsoft.Extensions.DependencyInjection;

namespace quanlybanhang_nmcnpm.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IProductService, ProductService>();
        services.AddTransient<ICustomerService, CustomerService>();
        services.AddTransient<IOrderService, OrderService>();
        services.AddTransient<IInventoryService, InventoryService>();
        services.AddTransient<IAccountService, AccountService>();
        services.AddTransient<IOverviewService, OverviewService>();
        services.AddTransient<IReceiptService, ReceiptService>();
        services.AddSingleton<IUserSessionService, UserSessionService>();

        return services;
    }
}

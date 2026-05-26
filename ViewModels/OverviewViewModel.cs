using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm.ViewModels;

public sealed class OverviewViewModel : ViewModelBase
{
    private readonly IOverviewService _overviewService;
    private decimal _todayRevenue;
    private int _todayOrders;
    private int _lowStockProducts;
    private int _newCustomersThisMonth;
    private string _statusMessage = "";

    public OverviewViewModel(IOverviewService overviewService)
    {
        _overviewService = overviewService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    public AsyncRelayCommand LoadCommand { get; }

    public decimal TodayRevenue
    {
        get => _todayRevenue;
        private set => SetProperty(ref _todayRevenue, value);
    }

    public int TodayOrders
    {
        get => _todayOrders;
        private set => SetProperty(ref _todayOrders, value);
    }

    public int LowStockProducts
    {
        get => _lowStockProducts;
        private set => SetProperty(ref _lowStockProducts, value);
    }

    public int NewCustomersThisMonth
    {
        get => _newCustomersThisMonth;
        private set => SetProperty(ref _newCustomersThisMonth, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private async Task LoadAsync()
    {
        var metrics = await _overviewService.GetMetricsAsync();
        TodayRevenue = metrics.TodayRevenue;
        TodayOrders = metrics.TodayOrders;
        LowStockProducts = metrics.LowStockProducts;
        NewCustomersThisMonth = metrics.NewCustomersThisMonth;
        StatusMessage = $"Cập nhật lúc {DateTime.Now:HH:mm:ss}";
    }
}

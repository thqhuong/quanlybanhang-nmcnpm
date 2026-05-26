using System.Collections.ObjectModel;
using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm.ViewModels;

public sealed class OverviewViewModel : ViewModelBase
{
    private readonly IOverviewService _overviewService;
    private string _fromDateText = DateTime.Today.ToString("dd/MM/yyyy");
    private string _toDateText = DateTime.Today.ToString("dd/MM/yyyy");
    private decimal _revenue;
    private int _orderCount;
    private decimal _averageOrderValue;
    private int _lowStockProducts;
    private int _newCustomers;
    private string _statusMessage = "";

    public OverviewViewModel(IOverviewService overviewService)
    {
        _overviewService = overviewService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        GenerateReportCommand = new AsyncRelayCommand(LoadAsync);
        TodayCommand = new RelayCommand(UseToday);
        ThisMonthCommand = new RelayCommand(UseThisMonth);
    }

    public ObservableCollection<TopProductReportItem> TopProducts { get; } = new();
    public ObservableCollection<LowStockReportItem> LowStockItems { get; } = new();

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand GenerateReportCommand { get; }
    public RelayCommand TodayCommand { get; }
    public RelayCommand ThisMonthCommand { get; }

    public string FromDateText
    {
        get => _fromDateText;
        set => SetProperty(ref _fromDateText, value);
    }

    public string ToDateText
    {
        get => _toDateText;
        set => SetProperty(ref _toDateText, value);
    }

    public decimal Revenue
    {
        get => _revenue;
        private set => SetProperty(ref _revenue, value);
    }

    public int OrderCount
    {
        get => _orderCount;
        private set => SetProperty(ref _orderCount, value);
    }

    public decimal AverageOrderValue
    {
        get => _averageOrderValue;
        private set => SetProperty(ref _averageOrderValue, value);
    }

    public int LowStockProducts
    {
        get => _lowStockProducts;
        private set => SetProperty(ref _lowStockProducts, value);
    }

    public int NewCustomers
    {
        get => _newCustomers;
        private set => SetProperty(ref _newCustomers, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private async Task LoadAsync()
    {
        if (!TryParseDate(FromDateText, out var from) || !TryParseDate(ToDateText, out var to))
        {
            StatusMessage = "Vui lòng nhập ngày theo định dạng dd/MM/yyyy.";
            return;
        }

        var metrics = await _overviewService.GetMetricsAsync(from, to);
        Revenue = metrics.Revenue;
        OrderCount = metrics.OrderCount;
        AverageOrderValue = metrics.AverageOrderValue;
        LowStockProducts = metrics.LowStockProducts;
        NewCustomers = metrics.NewCustomers;
        TopProducts.ResetWith(metrics.TopProducts);
        LowStockItems.ResetWith(metrics.LowStockItems);
        StatusMessage = metrics.OrderCount == 0
            ? $"Không có dữ liệu bán hàng trong khoảng {metrics.From:dd/MM/yyyy} - {metrics.To:dd/MM/yyyy}."
            : $"Đã cập nhật báo cáo lúc {DateTime.Now:HH:mm:ss}.";
    }

    private void UseToday()
    {
        FromDateText = DateTime.Today.ToString("dd/MM/yyyy");
        ToDateText = DateTime.Today.ToString("dd/MM/yyyy");
        GenerateReportCommand.Execute(null);
    }

    private void UseThisMonth()
    {
        var today = DateTime.Today;
        FromDateText = new DateTime(today.Year, today.Month, 1).ToString("dd/MM/yyyy");
        ToDateText = today.ToString("dd/MM/yyyy");
        GenerateReportCommand.Execute(null);
    }

    private static bool TryParseDate(string value, out DateTime date)
    {
        return DateTime.TryParseExact(
            value.Trim(),
            "dd/MM/yyyy",
            null,
            System.Globalization.DateTimeStyles.None,
            out date);
    }
}

using System.Collections.ObjectModel;
using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm.ViewModels;

public sealed class OverviewViewModel : ViewModelBase
{
    private readonly IOverviewService _overviewService;
    private DateTime? _fromDate = DateTime.Today;
    private DateTime? _toDate = DateTime.Today;
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

    public DateTime? FromDate
    {
        get => _fromDate;
        set
        {
            if (SetProperty(ref _fromDate, value))
                OnPropertyChanged(nameof(ToDateDisplayStart));
        }
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set
        {
            if (SetProperty(ref _toDate, value))
                OnPropertyChanged(nameof(FromDateDisplayEnd));
        }
    }

    public DateTime? FromDateDisplayEnd => ToDate;
    public DateTime? ToDateDisplayStart => FromDate;
    public DateTime ToDateMax => DateTime.Today;

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
        if (FromDate > ToDate)
        {
            StatusMessage = "Từ ngày phải nhỏ hơn hoặc bằng đến ngày.";
            return;
        }

        var metrics = await _overviewService.GetMetricsAsync(FromDate, ToDate);
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
        FromDate = DateTime.Today;
        ToDate = DateTime.Today;
        GenerateReportCommand.Execute(null);
    }

    private void UseThisMonth()
    {
        var today = DateTime.Today;
        FromDate = new DateTime(today.Year, today.Month, 1);
        ToDate = today;
        GenerateReportCommand.Execute(null);
    }
}

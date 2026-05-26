using System.Windows;

namespace quanlybanhang_nmcnpm;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;

    public MainWindow() : this(App.Services)
    {
    }

    public MainWindow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
    }

    private void Admin_Click(object sender, RoutedEventArgs e)
    {
        OpenDashboard("Quản trị viên");
    }

    private void Cashier_Click(object sender, RoutedEventArgs e)
    {
        OpenDashboard("Thu ngân");
    }

    private void Storekeeper_Click(object sender, RoutedEventArgs e)
    {
        OpenDashboard("Thủ kho");
    }

    private void OpenDashboard(string roleName)
    {
        var dashboard = new DashboardWindow(roleName, _serviceProvider);
        dashboard.Show();
        Close();
    }
}

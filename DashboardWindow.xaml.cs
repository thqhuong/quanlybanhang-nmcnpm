using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using quanlybanhang_nmcnpm.Views;

namespace quanlybanhang_nmcnpm;

public partial class DashboardWindow : Window
{
    private readonly IServiceProvider _serviceProvider;

    public DashboardWindow(string roleName) : this(roleName, App.Services)
    {
    }

    public DashboardWindow(string roleName, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
        txtRoleName.Text = roleName;
        txtTitle.Text = $"Hệ thống Quản lý Bán hàng v1.0 - [Chế độ: {roleName}]";
        MainContentControl.Content = CreateView<OverviewControl>();
    }

    private void Menu_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb || MainContentControl is null)
        {
            return;
        }

        MainContentControl.Content = rb.Tag?.ToString() switch
        {
            "TongQuan" => CreateView<OverviewControl>(),
            "BanHang" => CreateView<SalesControl>(),
            "TaiKhoan" => CreateView<AccountsControl>(),
            "HangHoa" => CreateView<ProductsControl>(),
            "KhachHang" => CreateView<CustomersControl>(),
            "PhieuNhap" => CreateView<ImportControl>(),
            _ => new TextBlock
            {
                Text = "Chức năng đang được phát triển",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 24
            }
        };
    }

    private T CreateView<T>() where T : UserControl
    {
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider);
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = new MainWindow(_serviceProvider);
        mainWindow.Show();
        Close();
    }
}

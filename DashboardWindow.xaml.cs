using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using quanlybanhang_nmcnpm.Services;
using quanlybanhang_nmcnpm.Views;

namespace quanlybanhang_nmcnpm;

public partial class DashboardWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserSessionService _sessionService;

    public DashboardWindow() : this(App.Services)
    {
    }

    public DashboardWindow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _sessionService = serviceProvider.GetRequiredService<IUserSessionService>();
        InitializeComponent();
        var roleName = _sessionService.CurrentUser?.Role ?? "";
        txtRoleName.Text = roleName;
        txtTitle.Text = $"Hệ thống Quản lý Bán hàng v1.0 - [Chế độ: {roleName}]";
        ApplyRolePermissions();
        SelectDefaultView();
    }

    private void Menu_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb || MainContentControl is null)
        {
            return;
        }

        var menuKey = rb.Tag?.ToString() ?? "";
        if (!CanAccess(menuKey))
        {
            MainContentControl.Content = CreateAccessDeniedView();
            return;
        }

        MainContentControl.Content = menuKey switch
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

    private void ApplyRolePermissions()
    {
        MenuTongQuan.Visibility = CanAccess("TongQuan") ? Visibility.Visible : Visibility.Collapsed;
        MenuBanHang.Visibility = CanAccess("BanHang") ? Visibility.Visible : Visibility.Collapsed;
        MenuTaiKhoan.Visibility = CanAccess("TaiKhoan") ? Visibility.Visible : Visibility.Collapsed;
        MenuKhachHang.Visibility = CanAccess("KhachHang") ? Visibility.Visible : Visibility.Collapsed;
        MenuHangHoa.Visibility = CanAccess("HangHoa") ? Visibility.Visible : Visibility.Collapsed;
        MenuPhieuNhap.Visibility = CanAccess("PhieuNhap") ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SelectDefaultView()
    {
        var firstAllowed = new[]
        {
            MenuTongQuan,
            MenuBanHang,
            MenuTaiKhoan,
            MenuKhachHang,
            MenuHangHoa,
            MenuPhieuNhap
        }.FirstOrDefault(menu => menu.Visibility == Visibility.Visible);

        if (firstAllowed is null)
        {
            MainContentControl.Content = CreateAccessDeniedView();
            return;
        }

        firstAllowed.IsChecked = true;
        Menu_Checked(firstAllowed, new RoutedEventArgs());
    }

    private bool CanAccess(string menuKey)
    {
        return menuKey switch
        {
            "TaiKhoan" => _sessionService.IsInRole(RoleNames.Admin),
            "PhieuNhap" => _sessionService.IsInRole(RoleNames.Admin, RoleNames.Storekeeper),
            "HangHoa" => _sessionService.IsInRole(RoleNames.Admin, RoleNames.Cashier, RoleNames.Storekeeper),
            "TongQuan" or "BanHang" or "KhachHang" => _sessionService.IsInRole(RoleNames.Admin, RoleNames.Cashier),
            _ => false
        };
    }

    private static TextBlock CreateAccessDeniedView()
    {
        return new TextBlock
        {
            Text = "Bạn không có quyền truy cập chức năng này.",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 20
        };
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        _sessionService.Clear();
        var mainWindow = new MainWindow(_serviceProvider);
        mainWindow.Show();
        Close();
    }
}

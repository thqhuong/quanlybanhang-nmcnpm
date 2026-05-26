using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAccountService _accountService;
    private readonly IUserSessionService _sessionService;

    public MainWindow() : this(App.Services)
    {
    }

    public MainWindow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _accountService = serviceProvider.GetRequiredService<IAccountService>();
        _sessionService = serviceProvider.GetRequiredService<IUserSessionService>();
        InitializeComponent();
    }

    private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
    {
        PasswordPlaceholder.Visibility = Visibility.Collapsed;
    }

    private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(PasswordBox.Password))
            PasswordPlaceholder.Visibility = Visibility.Visible;
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = string.Empty;
        var username = UsernameBox.Text?.Trim() ?? string.Empty;
        var password = PasswordBox.Password ?? string.Empty;
        var result = await _accountService.AuthenticateAsync(new LoginInput(username, password));
        if (!result.IsValid || result.Value is null)
        {
            StatusText.Text = result.ErrorMessage ?? "Đăng nhập thất bại.";
            return;
        }

        _sessionService.Start(result.Value);
        var dashboard = new DashboardWindow(_serviceProvider);
        dashboard.Show();
        Close();
    }
}

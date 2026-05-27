using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using quanlybanhang_nmcnpm.Services;

namespace quanlybanhang_nmcnpm;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAccountService _accountService;
    private readonly IUserSessionService _sessionService;
    private bool _isLoggingIn;

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

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        UsernameBox.Focus();
    }

    private void Input_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.CapsLock)
        {
            UpdateCapsLockWarning();
        }
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        UpdateCapsLockWarning();
        if (e.Key == Key.Enter)
        {
            Login_Click(sender, e);
        }
    }

    private void UpdateCapsLockWarning()
    {
        CapsLockWarning.Visibility = Keyboard.IsKeyToggled(Key.CapsLock)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        if (_isLoggingIn)
        {
            return;
        }

        StatusText.Text = string.Empty;
        var username = UsernameBox.Text?.Trim() ?? string.Empty;
        var password = PasswordBox.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            StatusText.Text = "Vui lòng nhập tên đăng nhập và mật khẩu.";
            ShakePanel();
            return;
        }

        SetLoadingState(true);

        try
        {
            var result = await _accountService.AuthenticateAsync(new LoginInput(username, password));
            if (!result.IsValid || result.Value is null)
            {
                StatusText.Text = result.ErrorMessage ?? "Đăng nhập thất bại.";
                ShakePanel();
                return;
            }

            _sessionService.Start(result.Value);
            var dashboard = new DashboardWindow(_serviceProvider);
            dashboard.Show();
            Close();
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void SetLoadingState(bool loading)
    {
        _isLoggingIn = loading;
        UsernameBox.IsEnabled = !loading;
        PasswordBox.IsEnabled = !loading;
        LoginButton.IsEnabled = !loading;
        LoginButtonText.Text = loading ? "Đang đăng nhập..." : "Đăng nhập";
    }

    private void ShakePanel()
    {
        var storyboard = (Storyboard)FindResource("ShakeAnimation");
        storyboard.Begin(LoginPanel);
    }
}

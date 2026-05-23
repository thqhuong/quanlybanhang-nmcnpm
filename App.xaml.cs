using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using quanlybanhang_nmcnpm.Database;
using quanlybanhang_nmcnpm.Services;
using quanlybanhang_nmcnpm.ViewModels;

namespace quanlybanhang_nmcnpm;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public static IServiceProvider Services => ((App)Current)._serviceProvider
        ?? throw new InvalidOperationException("Application services are not initialized.");

    public App()
    {
        var services = new ServiceCollection();
        services.AddDatabaseServices();
        services.AddApplicationServices();
        services.AddViewModels();
        _serviceProvider = services.BuildServiceProvider();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (_serviceProvider is null)
        {
            Shutdown(1);
            return;
        }

        try
        {
            await _serviceProvider.InitializeDatabaseAsync();
            var mainWindow = new MainWindow(_serviceProvider);
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khởi tạo cơ sở dữ liệu: {ex.Message}", "Lỗi");
            Shutdown(1);
        }
    }
}

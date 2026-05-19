using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using quanlybanhang_nmcnpm.Database;

namespace quanlybanhang_nmcnpm;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        services.AddDatabaseServices();
        _serviceProvider = services.BuildServiceProvider();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (_serviceProvider != null)
        {
            try
            {
                await _serviceProvider.InitializeDatabaseAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization error: {ex.Message}", "Error");
                this.Shutdown(1);
            }
        }
    }
}


using System.Windows.Controls;
using quanlybanhang_nmcnpm.ViewModels;

namespace quanlybanhang_nmcnpm.Views;

public partial class SalesControl : UserControl
{
    private readonly SalesViewModel _viewModel;
    private bool _loaded;

    public SalesControl(SalesViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += SalesControl_Loaded;
    }

    private void SalesControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        _viewModel.LoadCommand.Execute(null);
    }
}

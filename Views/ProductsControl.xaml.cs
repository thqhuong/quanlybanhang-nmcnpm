using System.Windows.Controls;
using quanlybanhang_nmcnpm.ViewModels;

namespace quanlybanhang_nmcnpm.Views;

public partial class ProductsControl : UserControl
{
    private readonly ProductsViewModel _viewModel;
    private bool _loaded;

    public ProductsControl(ProductsViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += ProductsControl_Loaded;
    }

    private void ProductsControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        _viewModel.LoadCommand.Execute(null);
    }
}

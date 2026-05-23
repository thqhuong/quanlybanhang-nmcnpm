using System.Windows.Controls;
using quanlybanhang_nmcnpm.ViewModels;

namespace quanlybanhang_nmcnpm.Views;

public partial class CustomersControl : UserControl
{
    private readonly CustomersViewModel _viewModel;
    private bool _loaded;

    public CustomersControl(CustomersViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += CustomersControl_Loaded;
    }

    private void CustomersControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        _viewModel.LoadCommand.Execute(null);
    }
}

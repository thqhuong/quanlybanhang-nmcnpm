using System.Windows.Controls;
using quanlybanhang_nmcnpm.ViewModels;

namespace quanlybanhang_nmcnpm.Views;

public partial class AccountsControl : UserControl
{
    private readonly AccountsViewModel _viewModel;
    private bool _loaded;

    public AccountsControl(AccountsViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += AccountsControl_Loaded;
    }

    private void AccountsControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        _viewModel.LoadCommand.Execute(null);
    }
}

using System.Windows.Controls;
using quanlybanhang_nmcnpm.ViewModels;

namespace quanlybanhang_nmcnpm.Views;

public partial class OverviewControl : UserControl
{
    private readonly OverviewViewModel _viewModel;
    private bool _loaded;

    public OverviewControl(OverviewViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += OverviewControl_Loaded;
    }

    private void OverviewControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        _viewModel.LoadCommand.Execute(null);
    }
}

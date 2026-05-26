using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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

    private void ProductNameTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right or Key.Enter or Key.Escape or Key.Tab)
        {
            return;
        }

        Dispatcher.BeginInvoke(() =>
        {
            ProductSuggestionListBox.SelectedIndex = -1;
            ProductSuggestionPopup.IsOpen = !string.IsNullOrWhiteSpace(ProductNameTextBox.Text)
                && ProductSuggestionListBox.HasItems;
            MoveCaretToEnd();
        }, DispatcherPriority.Background);
    }

    private void ProductNameTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ProductSuggestionPopup.IsOpen = false;
            ProductSuggestionListBox.SelectedIndex = -1;
            return;
        }

        if (e.Key == Key.Enter && ProductSuggestionPopup.IsOpen)
        {
            CommitSelectedSuggestion();
            e.Handled = true;
            return;
        }

        if (e.Key is Key.Down or Key.Up or Key.Tab)
        {
            if (!ProductSuggestionListBox.HasItems)
            {
                return;
            }

            ProductSuggestionPopup.IsOpen = true;
            var step = e.Key == Key.Up || (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                ? -1
                : 1;
            MoveSuggestionSelection(step);
            e.Handled = true;
        }
    }

    private void ProductSuggestionListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        CommitSelectedSuggestion();
    }

    private void MoveSuggestionSelection(int step)
    {
        var itemCount = ProductSuggestionListBox.Items.Count;
        if (itemCount == 0)
        {
            return;
        }

        var nextIndex = ProductSuggestionListBox.SelectedIndex < 0
            ? (step > 0 ? 0 : itemCount - 1)
            : (ProductSuggestionListBox.SelectedIndex + step + itemCount) % itemCount;

        ProductSuggestionListBox.SelectedIndex = nextIndex;
        ProductSuggestionListBox.ScrollIntoView(ProductSuggestionListBox.SelectedItem);
    }

    private void CommitSelectedSuggestion()
    {
        if (ProductSuggestionListBox.SelectedItem is not ProductSuggestion suggestion)
        {
            if (ProductSuggestionListBox.Items.Count == 0)
            {
                return;
            }

            suggestion = (ProductSuggestion)ProductSuggestionListBox.Items[0];
        }

        _viewModel.ApplyProductSuggestion(suggestion);
        ProductSuggestionPopup.IsOpen = false;
        ProductSuggestionListBox.SelectedIndex = -1;
        ProductNameTextBox.Focus();
        MoveCaretToEnd();
    }

    private void MoveCaretToEnd()
    {
        ProductNameTextBox.SelectionStart = ProductNameTextBox.Text.Length;
        ProductNameTextBox.SelectionLength = 0;
    }
}

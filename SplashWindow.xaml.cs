using System.Windows;

namespace quanlybanhang_nmcnpm;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
    }

    public void SetStatus(string text)
    {
        StatusText.Text = text;
    }
}

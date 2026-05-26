using System.Windows;
using System.Windows.Controls;

namespace quanlybanhang_nmcnpm.Helpers;

public static class PasswordBoxHelper
{
    public static readonly DependencyProperty HasPasswordProperty =
        DependencyProperty.RegisterAttached(
            "HasPassword",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false));

    public static bool GetHasPassword(DependencyObject obj) =>
        (bool)obj.GetValue(HasPasswordProperty);

    private static void SetHasPassword(DependencyObject obj, bool value) =>
        obj.SetValue(HasPasswordProperty, value);

    public static readonly DependencyProperty MonitorPasswordProperty =
        DependencyProperty.RegisterAttached(
            "MonitorPassword",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false, OnMonitorPasswordChanged));

    public static bool GetMonitorPassword(DependencyObject obj) =>
        (bool)obj.GetValue(MonitorPasswordProperty);

    public static void SetMonitorPassword(DependencyObject obj, bool value) =>
        obj.SetValue(MonitorPasswordProperty, value);

    private static void OnMonitorPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            UpdateHasPassword(passwordBox);
        }
        else
        {
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
        }
    }

    private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            UpdateHasPassword(passwordBox);
        }
    }

    private static void UpdateHasPassword(PasswordBox passwordBox)
    {
        SetHasPassword(passwordBox, !string.IsNullOrEmpty(passwordBox.Password));
    }
}

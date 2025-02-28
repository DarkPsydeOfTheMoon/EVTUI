using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace EVTUI.Views;

public partial class ConfigWindow : Window
{
    public ConfigWindow()
    {
        InitializeComponent();
    }

    private void ToggleTheme(object? sender, RoutedEventArgs e)
    {
        if (Application.Current!.RequestedThemeVariant == ThemeVariant.Dark)
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        else
            Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

using EVTUI.ViewModels;

namespace EVTUI.Views;

public partial class EditorWindow : Window
{
    public EditorWindow()
    {
        InitializeComponent();
    }

    public async void SaveMod(object? sender, RoutedEventArgs args)
    {
        try
        {
            await ((EditorWindowViewModel)DataContext).SaveMod(((MenuItem)sender).Name);
            await Utils.RaiseModal(this, "Saved successfully!");
        }
        catch (IOException ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this, "Failed to save because the game files are in use.\nIf the game is currently open, close it before trying again.");
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this, $"Failed to save due to unhandled exception:\n{ex.ToString()}");
        }
    }

    private async void ToggleTheme(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (Application.Current!.RequestedThemeVariant == ThemeVariant.Dark)
                Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
            else
                Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this, $"Failed to toggle theme due to unhandled exception:\n{ex.ToString()}");
        }
    }

    private async void NormalizeDegrees(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        try
        {
            if (!(e.NewValue is null))
            {
                // why in the actual hell is % remainder and not modulo in C#. microsoft meet me in the denny's parking lot.
                decimal val = (((decimal)e.NewValue % 360) + 360) % 360;
                ((NumericUpDown)e.Source).Value = (val > 180) ? val - 360 : val;
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this, $"Failed to normalize degree value due to unhandled exception:\n{ex.ToString()}");
        }
    }
}

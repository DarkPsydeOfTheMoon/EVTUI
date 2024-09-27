using System;
using System.ComponentModel;
using Avalonia.Controls;

using EVTUI.ViewModels;

namespace EVTUI.Views;

public partial class EditorWindow : Window
{
    public EditorWindow()
    {
        InitializeComponent();
        this.Closing += this.ClearCache;
    }

    public void ClearCache(object? sender, CancelEventArgs args)
    {
        ((EditorWindowViewModel)DataContext).ClearCache();
    }
}

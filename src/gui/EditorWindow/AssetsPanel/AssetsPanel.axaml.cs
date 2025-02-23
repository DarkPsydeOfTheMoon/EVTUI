using System;
using System.ComponentModel;

using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using Avalonia.ReactiveUI;

using EVTUI.ViewModels;

namespace EVTUI.Views;

public partial class AssetsPanel : ReactiveUserControl<AssetsPanelViewModel>, INotifyPropertyChanged
{

    // INotifyPropertyChanged Implementation
    new public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public AssetsPanel()
    {
        InitializeComponent();
    }

    public void SaveChanges(object sender, EventArgs e)
    {
        Asset asset = (Asset)(((ContentPresenter)LogicalExtensions.GetLogicalParent(
            (Border)LogicalExtensions.GetLogicalParent(
                (DockPanel)LogicalExtensions.GetLogicalParent(
                    (StackPanel)LogicalExtensions.GetLogicalParent(
                        ((Flyout)sender).Target))))).Content);
        asset.SaveChanges();
    }

}

using System;
using System.ComponentModel;

using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
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

    private string _addType;
    public string AddType
    {
        get => _addType;
        set
        {
            _addType = value;
            OnPropertyChanged(nameof(AddType));
            OnPropertyChanged(nameof(AddTypeIsSelected));
        }
    }

    public bool AddTypeIsSelected { get => !(this.AddType is null); }

    private bool _modalIsOpen = false;
    public bool ModalIsOpen
    {
        get => _modalIsOpen;
        set
        {
            _modalIsOpen = value;
            OnPropertyChanged(nameof(ModalIsOpen));
        }
    }

    public AssetsPanel()
    {
        InitializeComponent();
    }

    public void OpenModal(object sender, RoutedEventArgs e)
    {
        this.ModalIsOpen = true;
    }

    public void CloseModal(object sender, RoutedEventArgs e)
    {
        this.ModalIsOpen = false;
    }

    public void AddAsset(object sender, RoutedEventArgs e)
    {
        if (!(this.AddType is null))
        {
            ViewModel!.AddAsset(this.AddType);
            this.ModalIsOpen = false;
            this.AddType = null;
        }
    }

    public void DeleteAsset(object sender, RoutedEventArgs e)
    {
        Asset asset = (Asset)(((ContentPresenter)LogicalExtensions.GetLogicalParent(
            (Border)LogicalExtensions.GetLogicalParent(
                (DockPanel)LogicalExtensions.GetLogicalParent(
                    (StackPanel)LogicalExtensions.GetLogicalParent(
                        (Button)sender))))).Content);
        ViewModel!.DeleteAsset(asset);
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

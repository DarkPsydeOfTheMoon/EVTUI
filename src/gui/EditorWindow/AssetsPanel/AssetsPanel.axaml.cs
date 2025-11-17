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

    public void Resort(object sender, RoutedEventArgs e)
    {
        if (ViewModel! is null)
            return;

        switch (((RadioButton)sender).Content)
        {
            case "Sort by ID (Ascending)":
                ViewModel!.SortMode = (true, true);
                break;
            case "Sort by ID (Descending)":
                ViewModel!.SortMode = (true, false);
                break;
            case "Sort by Type (Ascending)":
                ViewModel!.SortMode = (false, true);
                break;
            case "Sort by Type (Descending)":
                ViewModel!.SortMode = (false, false);
                break;
            default:
                break;
        }
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

    public void DuplicateAsset(object sender, RoutedEventArgs e)
    {
        AssetViewModel asset = (AssetViewModel)(LogicalExtensions.FindLogicalAncestorOfType<ContentPresenter>((MenuItem)sender).Content);
        ViewModel!.DuplicateAsset(asset);
    }

    public void DeleteAsset(object sender, RoutedEventArgs e)
    {
        AssetViewModel asset = (AssetViewModel)(LogicalExtensions.FindLogicalAncestorOfType<ContentPresenter>((MenuItem)sender).Content);
        ViewModel!.DeleteAsset(asset);
    }

}

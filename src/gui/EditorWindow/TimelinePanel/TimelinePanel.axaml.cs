using System;
using System.Collections.Generic;
using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.ReactiveUI;

using ReactiveUI;

using EVTUI.ViewModels;
using EVTUI.ViewModels.TimelineCommands;

namespace EVTUI.Views;

public partial class TimelinePanel : ReactiveUserControl<TimelinePanelViewModel>, INotifyPropertyChanged
{

    // INotifyPropertyChanged Implementation
    new public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private Vector _hPos = new Vector(0.0, 0.0);
    public Vector HPos
    {
        get => _hPos;
        set
        {
            _hPos = value;
            OnPropertyChanged(nameof(HPos));
        }
    }

    private Vector _vPos = new Vector(0.0, 0.0);
    public Vector VPos
    {
        get => _vPos;
        set
        {
            _vPos = value;
            OnPropertyChanged(nameof(VPos));
        }
    }

    private string _addCode;
    public string AddCode
    {
        get => _addCode;
        set
        {
            _addCode = value;
            OnPropertyChanged(nameof(AddCode));
            OnPropertyChanged(nameof(AddCodeIsSelected));
        }
    }

    public bool AddCodeIsSelected { get => !(this.AddCode is null); }

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

    private List<double> FramePositions;

    private int LastFrameClicked;

    public TimelinePanel()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            // TODO: move this to a method that can be run upon update
            // like if a frame is added or deleted
            this.FramePositions = new List<double>();
            foreach (var child in LogicalExtensions.GetLogicalChildren(this.FindControl<ItemsControl>("FramesHaver")))
                this.FramePositions.Add(((ContentPresenter)child).Bounds.X);
        });
    }

    public void DeleteCommand(object sender, RoutedEventArgs e)
    {
        Button target = (Button)LogicalExtensions.GetLogicalParent(
            (Control)(((Popup)LogicalExtensions.GetLogicalParent(
                (ContextMenu)LogicalExtensions.GetLogicalParent(
                    (MenuItem)sender))).PlacementTarget));
        CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent(target)).Content;
        Category cat = (Category)((ContentPresenter)LogicalExtensions.GetLogicalParent(
            (ItemsControl)LogicalExtensions.GetLogicalParent(
                (ContentPresenter)LogicalExtensions.GetLogicalParent(
                    target)))).Content;
        ViewModel!.DeleteCommand(cat, cmd);
    }

    public void CopyCommand(object sender, RoutedEventArgs e)
    {
        Button target = (Button)LogicalExtensions.GetLogicalParent(
            (Control)(((Popup)LogicalExtensions.GetLogicalParent(
                (ContextMenu)LogicalExtensions.GetLogicalParent(
                    (MenuItem)sender))).PlacementTarget));
        CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent(target)).Content;
        Category cat = (Category)((ContentPresenter)LogicalExtensions.GetLogicalParent(
            (ItemsControl)LogicalExtensions.GetLogicalParent(
                (ContentPresenter)LogicalExtensions.GetLogicalParent(
                    target)))).Content;
        ViewModel!.CopyCommand(cat, cmd, false);
    }

    public void CutCommand(object sender, RoutedEventArgs e)
    {
        Button target = (Button)LogicalExtensions.GetLogicalParent(
            (Control)(((Popup)LogicalExtensions.GetLogicalParent(
                (ContextMenu)LogicalExtensions.GetLogicalParent(
                    (MenuItem)sender))).PlacementTarget));
        CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent(target)).Content;
        Category cat = (Category)((ContentPresenter)LogicalExtensions.GetLogicalParent(
            (ItemsControl)LogicalExtensions.GetLogicalParent(
                (ContentPresenter)LogicalExtensions.GetLogicalParent(
                    target)))).Content;
        ViewModel!.CopyCommand(cat, cmd, true);
    }

    private void LogPosition(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);
        // boy, it sure feels silly to have to calculate it this way
        // ...but it is what it is!
        this.LastFrameClicked = (int)(point.Position.X / 85);
    }

    public void PasteCommand(object sender, RoutedEventArgs e)
    {
        ViewModel!.PasteCommand(this.LastFrameClicked);
    }

    public void OpenModal(object sender, RoutedEventArgs e)
    {
        this.ModalIsOpen = true;
    }

    public void CloseModal(object sender, RoutedEventArgs e)
    {
        this.ModalIsOpen = false;
    }

    public void NewCommand(object sender, RoutedEventArgs e)
    {
        if (!(this.AddCode is null))
        {
            ViewModel!.NewCommand(this.AddCode, this.LastFrameClicked);
            this.ModalIsOpen = false;
            this.AddCode = null;
        }
    }

    public void PopulateFlyout(object sender, EventArgs e)
    {
        Button target = (Button)((Flyout)sender).Target;
        target.Classes.Add("selected");
        CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent(((Flyout)sender).Target)).Content;
        ViewModel!.SetActiveCommand(cmd);
        ((Flyout)sender).Content = ViewModel!.ActiveCommand;
    }

    public void ClearFlyout(object sender, EventArgs e)
    {
        Button target = (Button)((Flyout)sender).Target;
        target.Classes.Remove("selected");
        ViewModel!.UnsetActiveCommand(true);
        ((Flyout)sender).Content = null;
    }

    public void PlaySFXTrack(object sender, RoutedEventArgs e)
    {
        Snd_ cmd = (Snd_)((ContentPresenter)LogicalExtensions.GetLogicalParent(
            (StackPanel)LogicalExtensions.GetLogicalParent(
                (StackPanel)LogicalExtensions.GetLogicalParent(
                    ((Button)sender))))).Content;
        ViewModel!.PlayCueFromSource(cmd.Source.Choice, cmd.CueID.Choice, 1);
    }

    public void PlayVoiceTrack(object sender, RoutedEventArgs e)
    {
        PagePreview page = (PagePreview)((ContentPresenter)LogicalExtensions.GetLogicalParent(
            (StackPanel)LogicalExtensions.GetLogicalParent(
                ((Button)sender)))).Content;
        if (!(page.Source is null) && !(page.CueID is null))
            ViewModel!.PlayCueFromSource((string)page.Source, (int)page.CueID, 1);
    }

    public void CloseMe(object sender, RoutedEventArgs e)
    {
        Popup flyout = (Popup)LogicalExtensions.GetLogicalParent(
            (FlyoutPresenter)LogicalExtensions.GetLogicalParent(
                (StackPanel)LogicalExtensions.GetLogicalParent(
                    ((Button)sender))));
        flyout.Close();
    }

    public void JumpToFrame(object sender, NumericUpDownValueChangedEventArgs e)
    {
        this.HPos = this.HPos.WithX(this.FramePositions[(int)e.NewValue]);
    }

    public void ShowAllCategories(object sender, RoutedEventArgs e)
    {
        foreach (Category cat in ViewModel!.TimelineContent.Categories)
           cat.IsOpen = true;
    }

    public void HideAllCategories(object sender, RoutedEventArgs e)
    {
        foreach (Category cat in ViewModel!.TimelineContent.Categories)
           cat.IsOpen = false;
    }
}

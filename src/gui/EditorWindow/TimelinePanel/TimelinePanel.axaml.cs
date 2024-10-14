using System;
using System.Collections.Generic;
using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
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

    private List<double> FramePositions;

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

    public void LoadModelInScene(object sender, EventArgs e)
    {
        //Console.WriteLine("Load!");
    }

    public void ClearScene(object sender, EventArgs e)
    {
        //Console.WriteLine("Clear!");
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

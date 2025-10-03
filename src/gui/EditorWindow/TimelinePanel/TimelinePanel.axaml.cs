using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
            // SIGH... because of binding/event shenanigans, this can't actually work
            // (not without double-scrolling when scrolling left and going back to zero sooo fast)
            // but i love this little binary search and want it to work so bad......
            /*int low = 0;
            int high = this.FramePositions.Count;
            int mid = 0;
            while (low <= high)
            {
                mid = (high + low) / 2;
                // if we're currently looking at a frame that starts prior to the scroll position
                if (this.FramePositions[mid] < _hPos.X)
                    if (mid >= this.FramePositions.Count-1 || this.FramePositions[mid+1] > _hPos.X)
                        break;
                    else
                        low = mid + 1;
                else if (this.FramePositions[mid] > _hPos.X)
                    high = mid - 1;
                else
                    break;
            }
            ViewModel!.TimelineContent.ActiveFrame = mid;*/
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

    public void ToggleMarker(object sender, PointerReleasedEventArgs e)
    {
        Frame frame = (Frame)((ContentPresenter)LogicalExtensions.GetLogicalParent(
            (Border)sender)).Content;
        ViewModel!.TimelineContent.TryToggleFrameMarker(frame.Index);
    }

    public void DeleteCommand(object sender, RoutedEventArgs e)
    {
        Button target = (Button)LogicalExtensions.GetLogicalParent(
            (Control)(((Popup)LogicalExtensions.GetLogicalParent(
                (ContextMenu)LogicalExtensions.GetLogicalParent(
                    (MenuItem)sender))).PlacementTarget));
        CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent(target)).Content;
        ViewModel!.DeleteCommand(cmd);
    }

    public void CopyCommand(object sender, RoutedEventArgs e)
    {
        Button target = (Button)LogicalExtensions.GetLogicalParent(
            (Control)(((Popup)LogicalExtensions.GetLogicalParent(
                (ContextMenu)LogicalExtensions.GetLogicalParent(
                    (MenuItem)sender))).PlacementTarget));
        CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent(target)).Content;
        ViewModel!.CopyCommand(cmd, false);
    }

    public void CutCommand(object sender, RoutedEventArgs e)
    {
        Button target = (Button)LogicalExtensions.GetLogicalParent(
            (Control)(((Popup)LogicalExtensions.GetLogicalParent(
                (ContextMenu)LogicalExtensions.GetLogicalParent(
                    (MenuItem)sender))).PlacementTarget));
        CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent(target)).Content;
        ViewModel!.CopyCommand(cmd, true);
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
        Category category = (Category)(((ItemsControl)LogicalExtensions.GetLogicalParent(
            (Control)(((Popup)LogicalExtensions.GetLogicalParent(
                (ContextMenu)LogicalExtensions.GetLogicalParent(
                    (MenuItem)sender))).PlacementTarget))).DataContext);
        ViewModel!.SetAddableCodes(category);
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
        if (LogicalExtensions.GetLogicalChildren(this.FindControl<ItemsControl>("FramesHaver")).Count() != this.FramePositions.Count)
        {
    	    this.FramePositions = new List<double>();
    	    foreach (var child in LogicalExtensions.GetLogicalChildren(this.FindControl<ItemsControl>("FramesHaver")))
    		    this.FramePositions.Add(((ContentPresenter)child).Bounds.X);
        }

        double newValue = this.FramePositions[(int)e.NewValue];
        if (newValue > this.Scrolly.Extent.Width - this.Scrolly.Bounds.Width)
            newValue = this.Scrolly.Extent.Width - this.Scrolly.Bounds.Width;
        this.HPos = this.HPos.WithX(newValue);
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

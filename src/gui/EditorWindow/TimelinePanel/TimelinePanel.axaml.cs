using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using ReactiveUI;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

using EVTUI.ViewModels;
using EVTUI.ViewModels.TimelineCommands;

namespace EVTUI.Views;

public partial class TimelinePanel : ReactiveUserControl<TimelinePanelViewModel>, INotifyPropertyChanged
{

    private Window topLevel;

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

    private List<double> FramePositions;

    private int LastFrameClicked;

    public TimelinePanel()
    {
        InitializeComponent();
        // goofy but it does seem to help a bit with memory pressure...
        this.Loaded += hello;
        this.Unloaded += goodbye;
        this.WhenActivated(d =>
        {
            var tl = TopLevel.GetTopLevel(this);
            if (tl is null) throw new NullReferenceException();
            this.topLevel = (Window)tl;

            // TODO: move this to a method that can be run upon update
            // like if a frame is added or deleted
            this.FramePositions = new List<double>();
            foreach (var child in LogicalExtensions.GetLogicalChildren(this.FindControl<ItemsControl>("FramesHaver")))
                this.FramePositions.Add(((ContentPresenter)child).Bounds.X);

        });
    }

    public void hello(object sender, RoutedEventArgs e)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    public void goodbye(object sender, RoutedEventArgs e)
    {
        this.topLevel = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    public async void ToggleMarker(object sender, PointerReleasedEventArgs e)
    {
        try
        {
            Frame frame = (Frame)((ContentPresenter)LogicalExtensions.GetLogicalParent(
                (Border)sender)).Content;
            ViewModel!.TimelineContent.TryToggleFrameMarker(frame.Index);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to toggle frame marker due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void OpenInsertFramesModal(object sender, RoutedEventArgs e)
    {
        try
        {
            AfterFrame.Value = this.LastFrameClicked;
            Modal.IsVisible = true;
            InsertFramesModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to open menu for inserting frames due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void NewFrames(object sender, RoutedEventArgs e)
    {
        try
        {
            this.topLevel.Cursor = new Cursor(StandardCursorType.Wait);
            ViewModel!.InsertFrames((int)AfterFrame.Value, (int)InsertFrames.Value);
            AfterFrame.Value = 0;
            Modal.IsVisible = false;
            InsertFramesModal.IsVisible = false;
            this.topLevel.Cursor = Cursor.Default;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to insert frames due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void CloseInsertFramesModal(object sender, RoutedEventArgs e)
    {
        try
        {
            AfterFrame.Value = 0;
            Modal.IsVisible = false;
            InsertFramesModal.IsVisible = false;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to close menu for inserting frames due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void OpenClearFramesModal(object sender, RoutedEventArgs e)
    {
        try
        {
            StartingFrame.Value = this.LastFrameClicked;
            EndingFrame.Value = this.LastFrameClicked;
            Modal.IsVisible = true;
            ClearFramesModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to open menu for clearing frames due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void EnforceEndingMinimum(object sender, NumericUpDownValueChangedEventArgs e)
    {
        try
        {
            if (!(EndingFrame is null) && EndingFrame.Value < StartingFrame.Value)
                EndingFrame.Value = StartingFrame.Value;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to enforce frame range due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void ClearFrames(object sender, RoutedEventArgs e)
    {
        try
        {
            this.topLevel.Cursor = new Cursor(StandardCursorType.Wait);
            ViewModel!.ClearFrames((int)StartingFrame.Value, (int)EndingFrame.Value, (bool)DeleteFrames.IsChecked);
            StartingFrame.Value = 0;
            EndingFrame.Value = 0;
            Modal.IsVisible = false;
            ClearFramesModal.IsVisible = false;
            this.topLevel.Cursor = Cursor.Default;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to clear frames due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void CloseClearFramesModal(object sender, RoutedEventArgs e)
    {
       try
       {
            StartingFrame.Value = 0;
            EndingFrame.Value = 0;
            Modal.IsVisible = false;
            ClearFramesModal.IsVisible = false;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to close menu for clearing frames due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void DeleteCommand(object sender, RoutedEventArgs e)
    {
        try
        {
            Button target = LogicalExtensions.FindLogicalAncestorOfType<Button>((MenuItem)sender, true);
            CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent(target)).Content;
            ViewModel!.DeleteCommand(cmd);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to delete command due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void CopyCommand(object sender, RoutedEventArgs e)
    {
        try
        {
            Button target = LogicalExtensions.FindLogicalAncestorOfType<Button>((MenuItem)sender, true);
            CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent(target)).Content;
            ViewModel!.CopyCommand(cmd);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to copy command due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void CopyPosition(object sender, RoutedEventArgs e)
    {
        try
        {
            Expander target = LogicalExtensions.FindLogicalAncestorOfType<Expander>((MenuItem)sender, true);
            Position3D pos = (Position3D)target.DataContext;
            ViewModel!.CopyPosition(pos);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to copy position due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void CopyRotation(object sender, RoutedEventArgs e)
    {
        try
        {
            Expander target = LogicalExtensions.FindLogicalAncestorOfType<Expander>((MenuItem)sender, true);
            RotationWidget rot = (RotationWidget)target.DataContext;
            ViewModel!.CopyRotation(rot);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to copy rotation due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void CutCommand(object sender, RoutedEventArgs e)
    {
        try
        {
            Button target = (Button)LogicalExtensions.GetLogicalParent(
                (Control)(((Popup)LogicalExtensions.GetLogicalParent(
                    (ContextMenu)LogicalExtensions.GetLogicalParent(
                        (MenuItem)sender))).PlacementTarget));
            CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent(target)).Content;
            ViewModel!.CopyCommand(cmd);
            ViewModel!.DeleteCommand(cmd);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to cut command due to unhandled exception:\n{ex.ToString()}");
        }
    }

    private async void LogPosition(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            var point = e.GetCurrentPoint(sender as Control);
            // boy, it sure feels silly to have to calculate it this way
            // ...but it is what it is!
            this.LastFrameClicked = (int)(point.Position.X / 85);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to log position due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void PasteCommand(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel!.PasteCommand(this.LastFrameClicked);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to paste command due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void PastePosition(object sender, RoutedEventArgs e)
    {
        try
        {
            Expander target = LogicalExtensions.FindLogicalAncestorOfType<Expander>((MenuItem)sender, true);
            Position3D pos = (Position3D)target.DataContext;
            ViewModel!.PastePosition(pos);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to paste position due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void PasteRotation(object sender, RoutedEventArgs e)
    {
        try
        {
            Expander target = LogicalExtensions.FindLogicalAncestorOfType<Expander>((MenuItem)sender, true);
            RotationWidget rot = (RotationWidget)target.DataContext;
            ViewModel!.PasteRotation(rot);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to paste rotation due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void OpenAddCodeModal(object sender, RoutedEventArgs e)
    {
        try
        {
            Category category = (Category)(((ItemsControl)LogicalExtensions.GetLogicalParent(
                (Control)(((Popup)LogicalExtensions.GetLogicalParent(
                    (ContextMenu)LogicalExtensions.GetLogicalParent(
                        (MenuItem)sender))).PlacementTarget))).DataContext);
            ViewModel!.SetAddableCodes(category);
            AddCode.SelectedIndex = 0;
            Modal.IsVisible = true;
            AddCodeModal.IsVisible = true;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to open menu for adding commands due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void CloseAddCodeModal(object sender, RoutedEventArgs e)
    {
        try
        {
            Modal.IsVisible = false;
            AddCodeModal.IsVisible = false;
            AddCode.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to close menu for adding commands due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void NewCommand(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!(String.IsNullOrEmpty(AddCode.SelectedItem.ToString())))
            {
                ViewModel!.NewCommand(AddCode.SelectedItem.ToString(), this.LastFrameClicked);
                Modal.IsVisible = false;
                AddCodeModal.IsVisible = false;
                AddCode.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to add new command due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void PopulateFlyout(object sender, EventArgs e)
    {
        try
        {
            Button target = (Button)((Flyout)sender).Target;
            target.Classes.Add("selected");
            CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent(((Flyout)sender).Target)).Content;
            ViewModel!.SetActiveCommand(cmd);
            ((Flyout)sender).Content = ViewModel!.ActiveCommand;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to open command editor due to unhandled exception:\n{ex.ToString()}");
        }
    }

    // TODO lol
    /*public async void PopulateCommandEditor(object sender, RoutedEventArgs e)
    {
        try
        {
            bool actuallyJustClearIt = (((Button)sender).Classes.Contains("selected"));

            ViewModel!.UnsetActiveCommand();
            CommandEditor.Content = null;
            foreach (Button b in Scrolly.GetVisualDescendants().OfType<Button>())
                b.Classes.Remove("selected");

            if (actuallyJustClearIt)
                return;

            ((Button)sender).Classes.Add("selected");
            CommandPointer cmd = (CommandPointer)((ContentPresenter)LogicalExtensions.GetLogicalParent((Button)sender)).Content;
            ViewModel!.SetActiveCommand(cmd);
            CommandEditor.Content = ViewModel!.ActiveCommand;
            //CommandEditor.IsExpanded = true;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to open command editor due to unhandled exception:\n{ex.ToString()}");
        }
    }*/

    public async void ClearFlyout(object sender, EventArgs e)
    {
        try
        {
            Button target = (Button)((Flyout)sender).Target;
            target.Classes.Remove("selected");
            ViewModel!.UnsetActiveCommand();
            ((Flyout)sender).Content = null;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to close command editor due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void PlaySFXTrack(object sender, RoutedEventArgs e)
    {
        /*Snd_ cmd = (Snd_)((ContentPresenter)LogicalExtensions.GetLogicalParent(
            (StackPanel)LogicalExtensions.GetLogicalParent(
                (StackPanel)LogicalExtensions.GetLogicalParent(
                    ((Button)sender))))).Content;
        ViewModel!.PlayCueFromSource(cmd.SourceType.Choice, cmd.CueID.Choice, 1);*/
        try
        {
            if (!(ViewModel!.ActiveCommand is null))
            {
                Snd_ cmd = (Snd_)ViewModel!.ActiveCommand;
                ViewModel!.PlayCueFromSource(cmd.SourceType.Choice, (int)cmd.CueID.Value, 1);
            }
        }
        catch (KeyNotFoundException ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, "Cue ID not found in selected ACB.");
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to play audio due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void PlayVoiceTrack(object sender, RoutedEventArgs e)
    {
        try
        {
            PagePreview page = (PagePreview)((ContentPresenter)LogicalExtensions.GetLogicalParent(
                (StackPanel)LogicalExtensions.GetLogicalParent(
                    ((Button)sender)))).Content;
            if (!(page.Source is null) && !(page.CueID is null))
                ViewModel!.PlayCueFromSource((string)page.Source, (int)page.CueID, 1);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to play dialogue due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void CloseMe(object sender, RoutedEventArgs e)
    {
        try
        {
            Popup flyout = (Popup)LogicalExtensions.GetLogicalParent(
                (FlyoutPresenter)LogicalExtensions.GetLogicalParent(
                    (StackPanel)LogicalExtensions.GetLogicalParent(
                        ((Button)sender))));
            flyout.Close();
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to close preview due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void JumpToFrame(object sender, NumericUpDownValueChangedEventArgs e)
    {
        try
        {
            // dumb workaround for new avalonia having this run before whenactivated
            if (this.topLevel is null)
                return;

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
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to jump to frame due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void ShowAllCategories(object sender, RoutedEventArgs e)
    {
        try
        {
            foreach (Category cat in ViewModel!.TimelineContent.Categories)
               cat.IsOpen = true;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to show all categories due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void HideAllCategories(object sender, RoutedEventArgs e)
    {
        try
        {
            foreach (Category cat in ViewModel!.TimelineContent.Categories)
               cat.IsOpen = false;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to hide all categories due to unhandled exception:\n{ex.ToString()}");
        }
    }
}

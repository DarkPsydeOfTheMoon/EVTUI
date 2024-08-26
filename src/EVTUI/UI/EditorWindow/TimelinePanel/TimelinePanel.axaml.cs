using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.ReactiveUI;

using EVTUI.ViewModels;
using EVTUI.ViewModels.TimelineCommands;

namespace EVTUI.Views;

public partial class TimelinePanel : ReactiveUserControl<TimelinePanelViewModel>
{

    public TimelinePanel()
    {
        InitializeComponent();
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
        MessagePreview msg = (MessagePreview)((ContentControl)LogicalExtensions.GetLogicalParent(
            (StackPanel)LogicalExtensions.GetLogicalParent(
                (StackPanel)LogicalExtensions.GetLogicalParent(
                    ((Button)sender))))).Content;
        if (!(msg.Source is null) && !(msg.CueID is null))
            ViewModel!.PlayCueFromSource((string)msg.Source, (int)msg.CueID, 1);
    }

    public void LoadModelInScene(object sender, EventArgs e)
    {
        //Console.WriteLine("Load!");
    }

    public void ClearScene(object sender, EventArgs e)
    {
        //Console.WriteLine("Clear!");
    }

}

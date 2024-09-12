using System;
using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.ReactiveUI;

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
    //private double _hPos = 0.0;
    //public double HPos
    {
        get => _hPos;
        set
        {
            _hPos = value;
            OnPropertyChanged(nameof(HPos));
            //Console.WriteLine(HPos);
        }
    }

    private Vector _vPos = new Vector(0.0, 0.0);
    public Vector VPos
    //private double _vPos = 0.0;
    //public double VPos
    {
        get => _vPos;
        set
        {
            _vPos = value;
            OnPropertyChanged(nameof(VPos));
            //Console.WriteLine(VPos);
        }
    }

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

}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using ReactiveUI;

using EVTUI.ViewModels;

namespace EVTUI.Views;

public partial class LandingPage : ReactiveUserControl<LandingPageViewModel>
{

    private Window topLevel;
    private bool _mouseDownForWindowMoving = false;
    private PointerPoint _originalPoint;

    //private List<EditorWindow> editorWindows;
    private Dictionary<EditorWindow, (string GamePath, string? ModPath, int MajorId, int MinorId)> editorWindows;
    private HashSet<(string GamePath, string? ModPath, int MajorId, int MinorId)> openStuff;

    private Clipboard SharedClipboard;

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public LandingPage()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            var tl = TopLevel.GetTopLevel(this);
            if (tl is null) throw new NullReferenceException();
            this.topLevel = (Window)tl;
            this.topLevel.Closing += CloseAll;

            //this.editorWindows = new List<EditorWindow>();
            this.editorWindows = new Dictionary<EditorWindow, (string GamePath, string? ModPath, int MajorId, int MinorId)>();
            this.openStuff = new HashSet<(string GamePath, string? ModPath, int MajorId, int MinorId)>();

            this.SharedClipboard = new Clipboard();
        });
    }

    // If we need to pop boxes like this from multiple places, we should add this to
    // a View base class or something.
    public async Task<int> RaiseConfigModal(string configtype)
    {
        //DataManager config = ((LandingPageViewModel)DataContext).Config;
        User userData = ((LandingPageViewModel)DataContext).UserData;
        DataManager config = new DataManager(userData);

        ConfigWindowViewModel configWindowVM   = new ConfigWindowViewModel(
            config, configtype);
        ConfigWindow          configWindowView = new ConfigWindow
            { DataContext = configWindowVM };

        // Launch window and get a return code to distinguish how the window
        // was closed.
        int? res = await ((Window)configWindowView).ShowDialog<int?>(this.topLevel);
        if (res is null)
        {
            //config.Reset();
            return 1;
        }

        (string GamePath, string? ModPath, int MajorId, int MinorId) newOpenThing = (config.ProjectManager.ActiveGame.Path, config.ModPath, config.ProjectManager.ActiveEvent.MajorId, config.ProjectManager.ActiveEvent.MinorId);
        if (this.openStuff.Contains(newOpenThing))
        {
            if (configtype == "read-only")
                await Utils.RaiseModal(this.topLevel, $"{config.ActiveEventId} is already open for this copy of {config.ProjectManager.ActiveGame.Type}.");
            else
                await Utils.RaiseModal(this.topLevel, $"{config.ActiveEventId} is already open for project \"{config.ProjectManager.ActiveProject.Name}.\"");
            //config.Reset();
            return 1;
        }

        EditorWindowViewModel editorWindowVM   = new EditorWindowViewModel(
            config, this.SharedClipboard);
        EditorWindow          editorWindowView = new EditorWindow
            { DataContext = editorWindowVM };

        if (configtype == "read-only")
            editorWindowView.Title = $"EVTUI: {config.ActiveEventId} (read-only)";
        else
            editorWindowView.Title = $"EVTUI: {config.ActiveEventId} ({config.ProjectManager.ActiveProject.Name})";

        /*res = await ((Window)editorWindowView).ShowDialog<int?>(this.topLevel);
        config.Reset();
        if (res is null)
            return 1;
        else
            return (int)res;*/

        editorWindowView.Closing += this.EditorClosed;
        editorWindowView.Show();
        this.editorWindows[editorWindowView] = newOpenThing;
        this.openStuff.Add(newOpenThing);
        return 1;
    }

    /////////////////////////////
    // *** PRIVATE METHODS *** //
    /////////////////////////////

    private void CloseAll(object? sender, CancelEventArgs e)
    {
        foreach (EditorWindow window in this.editorWindows.Keys)
        {
            window.Close();
            ((EditorWindowViewModel)(window.DataContext)).Config.Reset();
        }
        this.openStuff.Clear();
        this.editorWindows.Clear();
    }

    private void EditorClosed(object? sender, CancelEventArgs e)
    {
        (string GamePath, string? ModPath, int MajorId, int MinorId) thingToClose = this.editorWindows[(EditorWindow)sender];
        //if (!(thingToClose is null))
        this.openStuff.Remove(thingToClose);
        this.editorWindows.Remove((EditorWindow)sender);
        if (this.editorWindows.Count == 0)
            ((EditorWindowViewModel)(((EditorWindow)sender).DataContext)).Config.Reset();
    }

    private async void NewProjectClicked(object? sender, PointerReleasedEventArgs e)
    {
        await this.RaiseConfigModal("new-proj");
    }

    private async void OpenProjectClicked(object? sender, PointerReleasedEventArgs e)
    {
        await this.RaiseConfigModal("open-proj");
    }

    private async void ReadOnlyClicked(object? sender, PointerReleasedEventArgs e)
    {
        await this.RaiseConfigModal("read-only");
    }

    private void ExitClicked(object? sender, PointerReleasedEventArgs e)
    {
        this.topLevel.Close();
    }

    // credit: https://github.com/AvaloniaUI/Avalonia/discussions/8441#discussioncomment-3081536
    private void DragLauncher(object? sender, PointerEventArgs e)
    {
        if (!_mouseDownForWindowMoving)
            return;

        PointerPoint currentPoint = e.GetCurrentPoint(this);
        this.topLevel.Position = new PixelPoint(this.topLevel.Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
            this.topLevel.Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
    }

    private void StartDragLauncher(object? sender, PointerPressedEventArgs e)
    {
        if (this.topLevel.WindowState == WindowState.Maximized || this.topLevel.WindowState == WindowState.FullScreen)
            return;

        _mouseDownForWindowMoving = true;
        _originalPoint = e.GetCurrentPoint(this);
    }

    private void StopDragLauncher(object? sender, PointerReleasedEventArgs e)
    {
        _mouseDownForWindowMoving = false;
    }

}

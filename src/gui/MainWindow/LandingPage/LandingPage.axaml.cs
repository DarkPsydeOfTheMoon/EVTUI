using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

using ReactiveUI;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using EVTUI.ViewModels;

namespace EVTUI.Views;

public partial class LandingPage : ReactiveUserControl<LandingPageViewModel>
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    private Window topLevel;
    private bool _mouseDownForWindowMoving = false;
    private PointerPoint _originalPoint;

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

            this.editorWindows = new Dictionary<EditorWindow, (string GamePath, string? ModPath, int MajorId, int MinorId)>();
            this.openStuff = new HashSet<(string GamePath, string? ModPath, int MajorId, int MinorId)>();

            this.SharedClipboard = new Clipboard();
        });
    }

    // If we need to pop boxes like this from multiple places, we should add this to
    // a View base class or something.
    public async Task<int> RaiseConfigModal(string configtype)
    {
        while (true)
        {
            User userData = ((LandingPageViewModel)DataContext).UserData;
            DataManager config = new DataManager(userData, this.openStuff);

            if (this.openStuff.Count == 0)
                config.ClearCache();

            ConfigWindowViewModel configWindowVM   = new ConfigWindowViewModel(
                config, configtype);
            ConfigWindow          configWindowView = new ConfigWindow
                { DataContext = configWindowVM };

            // Launch window and get a return code to distinguish how the window
            // was closed.
            int? res = await ((Window)configWindowView).ShowDialog<int?>(this.topLevel);
            configWindowVM.Dispose();
            if (res is null)
            {
                config.Reset();
                config = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return 1;
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();

            (string GamePath, string? ModPath, int MajorId, int MinorId) newOpenThing = (config.ProjectManager.ActiveGame.Path, config.ModPath, config.ProjectManager.ActiveEvent.MajorId, config.ProjectManager.ActiveEvent.MinorId);
            if (this.openStuff.Contains(newOpenThing))
            {
                // TODO: do this check earlier, actually
                if (configtype == "read-only")
                    await Utils.RaiseModal(this.topLevel, $"{config.ActiveEventId} is already open for this copy of {config.ProjectManager.ActiveGame.Type}.");
                else
                    await Utils.RaiseModal(this.topLevel, $"{config.ActiveEventId} is already open for project \"{config.ProjectManager.ActiveProject.Name}.\"");
            }
            else
            {
                EditorWindowViewModel editorWindowVM   = new EditorWindowViewModel(
                    config, this.SharedClipboard);
                EditorWindow          editorWindowView = new EditorWindow
                    { DataContext = editorWindowVM };

                if (configtype == "read-only")
                    editorWindowView.Title = $"EVTUI: {config.ActiveEventId} (read-only)";
                else
                    editorWindowView.Title = $"EVTUI: {config.ActiveEventId} ({config.ProjectManager.ActiveProject.Name})";

                editorWindowView.Closing += this.EditorClosed;
                editorWindowView.Show();
                this.editorWindows[editorWindowView] = newOpenThing;
                this.openStuff.Add(newOpenThing);
                return 1;
            }
        }
    }

    /////////////////////////////
    // *** PRIVATE METHODS *** //
    /////////////////////////////

    private async void CloseAll(object? sender, CancelEventArgs e)
    {
        try
        {
            foreach (EditorWindow window in this.editorWindows.Keys)
            {
                window.Close();
                ((EditorWindowViewModel)(window.DataContext)).Config.ClearCache();
                ((EditorWindowViewModel)(window.DataContext)).Dispose();
            }
            this.openStuff.Clear();
            this.editorWindows.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            if (!(this.topLevel is null))
                await Utils.RaiseModal(this.topLevel, $"Failed to close all editor windows due to unhandled exception:\n{ex.ToString()}");
        }
    }

    private async void EditorClosed(object? sender, CancelEventArgs e)
    {
        try
        {
            (string GamePath, string? ModPath, int MajorId, int MinorId) thingToClose = this.editorWindows[(EditorWindow)sender];
            this.openStuff.Remove(thingToClose);
            this.editorWindows.Remove((EditorWindow)sender);
            if (this.editorWindows.Count == 0)
                ((EditorWindowViewModel)(((EditorWindow)sender).DataContext)).Config.ClearCache();
            ((EditorWindowViewModel)(((EditorWindow)sender).DataContext)).Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to close editor window due to unhandled exception:\n{ex.ToString()}");
        }
    }

    private async void NewProjectClicked(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            await this.RaiseConfigModal("new-proj");
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"New project configuration failed with an error:\n{ex.ToString()}");
        }
    }

    private async void OpenProjectClicked(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            await this.RaiseConfigModal("open-proj");
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Project loading configuration failed with an error:\n{ex.ToString()}");
        }
    }

    private async void ReadOnlyClicked(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            await this.RaiseConfigModal("read-only");
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Read-only configuration failed with an error:\n{ex.ToString()}");
        }
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

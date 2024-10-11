using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using EVTUI.ViewModels;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

using Avalonia.Input;

namespace EVTUI.Views;

public partial class LandingPage : ReactiveUserControl<LandingPageViewModel>
{

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public LandingPage()
    {
        InitializeComponent();
    }

    // If we need to pop boxes like this from multiple places, we should add this to
    // a View base class or something.
    public async Task<int> RaiseConfigModal(string configtype)
    {
        var tl = TopLevel.GetTopLevel(this);
        if (tl is null) throw new NullReferenceException();
        var topLevel = (MainWindow)tl;

        DataManager config = ((LandingPageViewModel)DataContext).Config;

        ConfigWindowViewModel configWindowVM   = new ConfigWindowViewModel(
            config, configtype);
        ConfigWindow          configWindowView = new ConfigWindow
            { DataContext = configWindowVM };

        // Launch window and get a return code to distinguish how the window
        // was closed.
        int? res = await ((Window)configWindowView).ShowDialog<int?>(topLevel);
        if (res is null)
        {
            config.Reset();
            return 1;
        }

        EditorWindowViewModel editorWindowVM   = new EditorWindowViewModel(
            config);
        EditorWindow          editorWindowView = new EditorWindow
            { DataContext = editorWindowVM };

        if (configtype == "read-only")
            editorWindowView.Title = $"EVTUI: {config.ActiveEventId} (read-only)";
        else
            editorWindowView.Title = $"EVTUI: {config.ActiveEventId} ({config.ProjectManager.ActiveProject.Name})";

        res = await ((Window)editorWindowView).ShowDialog<int?>(topLevel);
        config.Reset();
        if (res is null)
            return 1;
        else
            return (int)res;
    }

    /////////////////////////////
    // *** PRIVATE METHODS *** //
    /////////////////////////////

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
        var tl = TopLevel.GetTopLevel(this);
        if (tl is null) throw new NullReferenceException();
        var topLevel = (MainWindow)tl;
        topLevel.Close();
    }

}

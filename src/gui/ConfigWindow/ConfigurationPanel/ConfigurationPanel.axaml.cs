using System;
using System.Threading.Tasks;

using ReactiveUI;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using EVTUI.ViewModels;

namespace EVTUI.Views;

public partial class ConfigurationPanel : ReactiveUserControl<ConfigurationPanelViewModel>
{

    private Window topLevel;

    public ConfigurationPanel()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            var tl = TopLevel.GetTopLevel(this);
            if (tl is null) throw new NullReferenceException();
            this.topLevel = (Window)tl;

            switch (ViewModel!.ConfigType)
            {
                case "new-proj":
                    pages.SelectedIndex = 1;
                    break;
                case "open-proj":
                    pages.SelectedIndex = 2;
                    break;
                case "read-only":
                    pages.SelectedIndex = 3;
                    break;
                default:
                    pages.SelectedIndex = 0;
                    break;
            }
        });
    }

    private void GoToEventPage()
    {
        ViewModel!.DisplayEvents();
        pages.SelectedIndex = 4;
    }

    ///////////////////////////////////
    // *** PROJECT CREATION FORM *** //
    ///////////////////////////////////

    public async void GetModDirPathFromDialog(object sender, RoutedEventArgs e)
    {
        // Start async operation to open the dialog.
        var dirs = await this.topLevel.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            Title = "Open Mod Directory",
            AllowMultiple = false
        });

        if (dirs.Count > 0)
        {
            var retTuple = ViewModel!.TrySetModDir(dirs[0].Path.LocalPath);
            if (retTuple.Status != 0)
                await Utils.RaiseModal(this.topLevel, retTuple.Message);
        }
    }

    public async void CreateProject(object sender, RoutedEventArgs e)
    {
        var retTuple = ViewModel!.TryCreateProject();
        if (retTuple.Status == 0)
            this.GoToEventPage();
        else
            await Utils.RaiseModal(this.topLevel, retTuple.Message);
    }

    ////////////////////////////
    // *** CPK SETUP FORM *** //
    ////////////////////////////

    public async void GetSelectedGame(object sender, RoutedEventArgs e)
    {
        var retTuple = ViewModel!.TryUseCPKDir(null, null);
        if (retTuple.Status != 0)
            await Utils.RaiseModal(this.topLevel, retTuple.Message);
        else if (ViewModel!.ConfigType == "read-only")
            this.GoToEventPage();
    }

    public async void GetGamePathFromDialog(object sender, RoutedEventArgs e)
    {
        // Start async operation to open the dialog.
        var dirs = await this.topLevel.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            Title = "Open CPK Directory",
            AllowMultiple = false
        });

        if (dirs.Count > 0)
        {
            var retTuple = ViewModel!.TryUseCPKDir(dirs[0].Path.LocalPath, ViewModel!.newProjectConfig.GameType);
            if (retTuple.Status != 0)
                await Utils.RaiseModal(this.topLevel, retTuple.Message);
            else if (ViewModel!.ConfigType == "read-only")
                this.GoToEventPage();
        }
    }

    ////////////////////////////////////
    // *** PROJECT SELECTION FORM *** //
    ////////////////////////////////////

    public async void UseProject(object sender, RoutedEventArgs e)
    {
        var retTuple = ViewModel!.TryLoadProject();
        if (retTuple.Status == 0)
        {
            this.topLevel.Title = $"EVTUI ({ViewModel!.Config.ProjectManager.ActiveProject.Name})";
            this.GoToEventPage();
        }
        else
            await Utils.RaiseModal(this.topLevel, retTuple.Message);
    }

    //////////////////////////////////
    // *** EVENT SELECTION FORM *** //
    //////////////////////////////////

    public async void UseSelectedEvent(object sender, RoutedEventArgs e)
    {
        var retTuple = ViewModel!.TryLoadEvent(true);
        if (retTuple.Status == 0)
            this.topLevel.Close(0);
        else
            await Utils.RaiseModal(this.topLevel, retTuple.Message);
    }

    public async void UseEnteredEvent(object sender, RoutedEventArgs e)
    {
        var retTuple = ViewModel!.TryLoadEvent(false);
        if (retTuple.Status == 0)
            this.topLevel.Close(0);
        else
            await Utils.RaiseModal(this.topLevel, retTuple.Message);
    }

}

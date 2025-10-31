using System;
using System.Diagnostics;
using System.Threading.Tasks;

using ReactiveUI;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
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
        try
        {
            // Start async operation to open the dialog.
            var dirs = await this.topLevel.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                Title = "Open Mod Directory",
                AllowMultiple = false
            });

            if (dirs.Count > 0)
            {
                switch (ViewModel!.TrySetModDir(dirs[0].Path.LocalPath))
                {
                    case 0:
                        break;
                    case 1:
                        await Utils.RaiseModal(this.topLevel, "Selected mod directory is used in another project and cannot be reused (unless you delete that project).");
                        break;
                    default:
                        await Utils.RaiseModal(this.topLevel, "Mod directory selection failed for an unknown reason.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Mod directory selection failed with an error:\n{ex.ToString()}");
        }
    }

    public async void CreateProject(object sender, RoutedEventArgs e)
    {
        try
        {
            switch (ViewModel!.TryCreateProject())
            {
                case 0:
                    this.GoToEventPage();
                    break;
                case 1:
                    await Utils.RaiseModal(this.topLevel, "Project name hasn't been set.");
                    break;
                case 2:
                    await Utils.RaiseModal(this.topLevel, "Game (CPK) folder hasn't been set.");
                    break;
                case 3:
                    await Utils.RaiseModal(this.topLevel, "Mod folder hasn't been set.");
                    break;
                case 4:
                    await Utils.RaiseModal(this.topLevel, "Something is wrong with the provided folders. Project could not be created.");
                    break;
                case 5:
                    await Utils.RaiseModal(this.topLevel, "No project loaded.");
                    break;
                default:
                    await Utils.RaiseModal(this.topLevel, "Project creation failed for an unknown reason.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Project creation failed with an error:\n{ex.ToString()}");
        }
    }

    ////////////////////////////
    // *** CPK SETUP FORM *** //
    ////////////////////////////

    public async void GetSelectedGame(object sender, RoutedEventArgs e)
    {
        try
        {
            switch (ViewModel!.TryUseCPKDir(null, null))
            {
                case 0:
                    if (ViewModel!.ConfigType == "read-only")
                        this.GoToEventPage();
                    break;
                case 1:
                    await Utils.RaiseModal(this.topLevel, "No game folder selected.");
                    break;
                case 2:
                    await Utils.RaiseModal(this.topLevel, "Game folder selection is invalid.");
                    break;
                case 3:
                    await Utils.RaiseModal(this.topLevel, "No CPKs in selected folder.");
                    break;
                default:
                    await Utils.RaiseModal(this.topLevel, "Game folder selection failed for an unknown reason.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Game folder selection failed with an error:\n{ex.ToString()}");
        }
    }

    public async void GetGamePathFromDialog(object sender, RoutedEventArgs e)
    {
        try
        {
            // Start async operation to open the dialog.
            var dirs = await this.topLevel.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                Title = "Open CPK Directory",
                AllowMultiple = false
            });

            if (dirs.Count > 0)
            {
                switch (ViewModel!.TryUseCPKDir(dirs[0].Path.LocalPath, ViewModel!.newProjectConfig.GameType))
                {
                    case 0:
                        if (ViewModel!.ConfigType == "read-only")
                            this.GoToEventPage();
                        break;
                    case 1:
                        await Utils.RaiseModal(this.topLevel, "No game folder selected.");
                        break;
                    case 2:
                        await Utils.RaiseModal(this.topLevel, "Game folder selection is invalid.");
                        break;
                    case 3:
                        await Utils.RaiseModal(this.topLevel, "No CPKs in selected folder.");
                        break;
                    default:
                        await Utils.RaiseModal(this.topLevel, "Game folder selection failed for an unknown reason.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Game folder selection failed with an error:\n{ex.ToString()}");
        }
    }

    ////////////////////////////////////
    // *** PROJECT SELECTION FORM *** //
    ////////////////////////////////////

    public async void UseProject(object sender, RoutedEventArgs e)
    {
        try
        {
            switch (ViewModel!.TryLoadProject())
            {
                case 0:
                    this.topLevel.Title = $"EVTUI ({ViewModel!.Config.ProjectManager.ActiveProject.Name})";
                    this.GoToEventPage();
                    break;
                case 1:
                    await Utils.RaiseModal(this.topLevel, "No project selected.");
                    break;
                case 2:
                    await Utils.RaiseModal(this.topLevel, "Project has no game path set.");
                    break;
                case 3:
                    await Utils.RaiseModal(this.topLevel, "No CPKs in project's game folder.");
                    break;
                case 4:
                    await Utils.RaiseModal(this.topLevel, "No project loaded.");
                    break;
                default:
                    await Utils.RaiseModal(this.topLevel, "Project selection failed for an unknown reason.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Project selection failed with an error:\n{ex.ToString()}");
        }
    }

    public async void DeleteProject(object sender, RoutedEventArgs e)
    {
        try
        {
            DisplayableProject project = (DisplayableProject)((DataGridRow)LogicalExtensions.GetLogicalParent(
                (Border)LogicalExtensions.GetLogicalParent(
                    (DataGridFrozenGrid)LogicalExtensions.GetLogicalParent(
                        (DataGridCellsPresenter)LogicalExtensions.GetLogicalParent(
                            (DataGridCell)LogicalExtensions.GetLogicalParent(
                                (Control)(((Popup)LogicalExtensions.GetLogicalParent(
                                    (ContextMenu)LogicalExtensions.GetLogicalParent(
                                        (MenuItem)sender))).PlacementTarget))))))).DataContext;
            int? check = await Utils.RaiseDoubleCheck(this.topLevel, $"Are you sure you want to delete the project \"{project.Name}\"?\n(This will not delete any files, only EVTUI's metadata about the project.)", "Yes", "No");
            if (check == 0)
            {
                switch (ViewModel!.TryDeleteProject(project))
                {
                    case 0:
                        await Utils.RaiseModal(this.topLevel, $"Successfully deleted the project \"{project.Name}\".");
                        break;
                    case 1:
                        await Utils.RaiseModal(this.topLevel, $"Couldn't delete the project \"{project.Name}\" because it appears to be open. Close any open editors for the project if you really want to delete it.");
                        break;
                    case 2:
                        await Utils.RaiseModal(this.topLevel, "Project deletion didn't work.");
                        break;
                    default:
                        await Utils.RaiseModal(this.topLevel, "Project deletion failed for an unknown reason.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Project deletion failed with an error:\n{ex.ToString()}");
        }
    }

    //////////////////////////////////
    // *** EVENT SELECTION FORM *** //
    //////////////////////////////////

    public async void UseSelectedEvent(object sender, RoutedEventArgs e)
    {
        try
        {
            this.topLevel.Cursor = new Cursor(StandardCursorType.Wait);
            switch (ViewModel!.TryLoadEvent(true))
            {
                case 0:
                    this.topLevel.Close(0);
                    break;
                case 1:
                    await Utils.RaiseModal(this.topLevel, "No event selected.");
                    break;
                case 2:
                    await Utils.RaiseModal(this.topLevel, "Must have a loaded project or be in read-only mode to load an event.");
                    break;
                case 3:
                    await Utils.RaiseModal(this.topLevel, $"Event E{ViewModel!.EventMajorId:000}_{ViewModel!.EventMinorId:000} does not exist and could not be loaded.");
                    break;
                case 4:
                    await Utils.RaiseModal(this.topLevel, "Failed to load event because the game files are in use.\nIf the game is currently open, close it before trying again.");
                    break;
                case 5:
                    await Utils.RaiseModal(this.topLevel, "No event loaded.");
                    break;
                default:
                    await Utils.RaiseModal(this.topLevel, "Event loading failed for an unknown reason.");
                    break;
            }
            this.topLevel.Cursor = Cursor.Default;
        }
        catch (Exception ex)
        {
            this.topLevel.Cursor = Cursor.Default;
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Event loading failed with an error:\n{ex.ToString()}");
        }
    }

    public async void UseEnteredEvent(object sender, RoutedEventArgs e)
    {
        try
        {
            this.topLevel.Cursor = new Cursor(StandardCursorType.Wait);
            switch (ViewModel!.TryLoadEvent(false))
            {
                case 0:
                    this.topLevel.Close(0);
                    break;
                case 1:
                    await Utils.RaiseModal(this.topLevel, "No event selected.");
                    break;
                case 2:
                    await Utils.RaiseModal(this.topLevel, "Must have a loaded project or be in read-only mode to load an event.");
                    break;
                case 3:
                    await Utils.RaiseModal(this.topLevel, $"Event E{ViewModel!.EventMajorId:000}_{ViewModel!.EventMinorId:000} does not exist and could not be loaded.");
                    break;
                case 4:
                    await Utils.RaiseModal(this.topLevel, "Failed to load event because the game files are in use.\nIf the game is currently open, close it before trying again.");
                    break;
                case 5:
                    await Utils.RaiseModal(this.topLevel, "No event loaded.");
                    break;
                default:
                    await Utils.RaiseModal(this.topLevel, "Event loading failed for an unknown reason.");
                    break;
            }
            this.topLevel.Cursor = Cursor.Default;
        }
        catch (Exception ex)
        {
            this.topLevel.Cursor = Cursor.Default;
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Event loading failed with an error:\n{ex.ToString()}");
        }
    }

}

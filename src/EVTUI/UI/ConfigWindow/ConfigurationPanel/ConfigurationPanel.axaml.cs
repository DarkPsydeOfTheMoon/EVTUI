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

    // If we need to pop boxes like this from multiple places, we should add this to
    // a View base class or something.
    public async Task<int> RaiseModal(string text)
    {
        Window sampleWindow =
            new Window 
            { 
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
        sampleWindow.Content = new MessageBox(text);

        // Launch window and get a return code to distinguish how the window
        // was closed.
        int? res = await sampleWindow.ShowDialog<int?>(this.topLevel);
        if (res is null)
            return 1;
        else
            return (int)res;
    }

    private void GoToEventPage()
    {
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
            var retTuple = ViewModel!.TrySetModDir(dirs[0].Path.AbsolutePath);
            if (retTuple.Status != 0)
                await RaiseModal(retTuple.Message);
        }
    }

    public async void CreateProject(object sender, RoutedEventArgs e)
    {
        var retTuple = ViewModel!.TryCreateProject();
        if (retTuple.Status == 0)
            this.GoToEventPage();
        else
            await RaiseModal(retTuple.Message);
    }

    ////////////////////////////
    // *** CPK SETUP FORM *** //
    ////////////////////////////

    public async void GetSelectedCpkDir(object sender, RoutedEventArgs e)
    {
        var retTuple = ViewModel!.TryUseCPKDir(null);
        if (retTuple.Status != 0)
            await RaiseModal(retTuple.Message);
        else if (ViewModel!.ConfigType == "read-only")
            this.GoToEventPage();
    }

    public async void GetCpkDirPathFromDialog(object sender, RoutedEventArgs e)
    {
        // Start async operation to open the dialog.
        var dirs = await this.topLevel.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            Title = "Open CPK Directory",
            AllowMultiple = false
        });

        if (dirs.Count > 0)
        {
            var retTuple = ViewModel!.TryUseCPKDir(dirs[0].Path.AbsolutePath);
            if (retTuple.Status != 0)
                await RaiseModal(retTuple.Message);
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
            this.GoToEventPage();
        else
            await RaiseModal(retTuple.Message);
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
            await RaiseModal(retTuple.Message);
    }

    public async void UseEnteredEvent(object sender, RoutedEventArgs e)
    {
        var retTuple = ViewModel!.TryLoadEvent(false);
        if (retTuple.Status == 0)
            this.topLevel.Close(0);
        else
            await RaiseModal(retTuple.Message);
    }

}

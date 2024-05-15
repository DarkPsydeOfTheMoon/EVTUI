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

namespace EVTUI.Views;

public partial class ConfigurationPanel : ReactiveUserControl<ConfigurationPanelViewModel>
{

    private string ConfigType
    {
        get
        {
            return ((ConfigurationPanelViewModel)DataContext).ConfigType;
        }
    }

    public ConfigurationPanel()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            if (this.ConfigType == "new-proj")
                pages.SelectedIndex = 1;
            else if (this.ConfigType == "open-proj")
                pages.SelectedIndex = 2;
            else if (this.ConfigType == "read-only")
                pages.SelectedIndex = 3;
            else
                pages.SelectedIndex = 0;
            d(ViewModel!.GetCPKDirectoryFromView.RegisterHandler(GetCpkDirPathFromDialog));
            d(ViewModel!.GetModDirectoryFromView.RegisterHandler(GetModDirPathFromDialog));
            d(ViewModel!.DisplayMessage.RegisterHandler(DisplayMessageBox));
            d(ViewModel!.OpenEventConfig.RegisterHandler(GoToEventPage));
            d(ViewModel!.FinishConfig.RegisterHandler(CloseModal));
        });
    }

    private async Task GoToEventPage(InteractionContext<Unit, bool> interaction)
    {
        pages.SelectedIndex = 4;
        interaction.SetOutput(true);
    }

    private async Task GetCpkDirPathFromDialog(InteractionContext<Unit, string?> interaction)
    {
        var tl = TopLevel.GetTopLevel(this);
        if (tl is null) throw new NullReferenceException();
        var topLevel = (Window)tl;

        // Start async operation to open the dialog.
        var dirs = await topLevel.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            Title = "Open CPK Directory",
            AllowMultiple = false
        });

        if (dirs.Count > 0)
            interaction.SetOutput(dirs[0].Path.AbsolutePath);
        else
            interaction.SetOutput(null);
    }

    private async Task GetModDirPathFromDialog(InteractionContext<Unit, string?> interaction)
    {
        var tl = TopLevel.GetTopLevel(this);
        if (tl is null) throw new NullReferenceException();
        var topLevel = (Window)tl;

        // Start async operation to open the dialog.
        var dirs = await topLevel.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
        {
            Title = "Open Mod Directory",
            AllowMultiple = false
        });

        if (dirs.Count > 0)
            interaction.SetOutput(dirs[0].Path.AbsolutePath);
        else
            interaction.SetOutput(null);
    }

    public async Task DisplayMessageBox(InteractionContext<string, bool> interaction)
    {
        // Hiding RaiseModal inside this function suppresses a
        // "[IME] Error while destroying the context:
        // Tmds.DBus.Protocol.DBusException: org.freedesktop.DBus.Error.UnknownMethod: 
        // Method Destroy is not implemented on interface org.freedesktop.IBus.Service"
        // exception apparently caused by a bug in the Avalonia 11.0 release.
        int closecode = await RaiseModal(interaction.Input);
        interaction.SetOutput(closecode == 0);
        return;
    }

    // If we need to pop boxes like this from multiple places, we should add this to
    // a View base class or something.
    public async Task<int> RaiseModal(string text)
    {
        var tl = TopLevel.GetTopLevel(this);
        if (tl is null) throw new NullReferenceException();
        var topLevel = (Window)tl;
        Window sampleWindow =
            new Window 
            { 
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
        sampleWindow.Content = new MessageBox(text);

        // Launch window and get a return code to distinguish how the window
        // was closed.
        int? res = await sampleWindow.ShowDialog<int?>(topLevel);
        if (res is null)
            return 1;
        else
            return (int)res;
    }

    public async Task CloseModal(InteractionContext<int?,bool> interaction)
    {
        var tl = TopLevel.GetTopLevel(this);
        if (tl is null) throw new NullReferenceException();
        var topLevel = (Window)tl;
        interaction.SetOutput(true);
        topLevel.Close(interaction.Input);
    }

}

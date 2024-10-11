using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EVTUI.ViewModels;
using EVTUI.Views;

namespace EVTUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // TODO: if dataManager.ProjectManager.UserData is null...
            // ...then the formatting is bad and we should check
            // if the user want to start fresh or exit the program
            DataManager         dataManager     = new DataManager();
            MainWindowViewModel mainWindowVM    = new MainWindowViewModel(dataManager);
            MainWindow          mainWindow      = new MainWindow { DataContext = mainWindowVM };

            desktop.MainWindow = (Window)mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}

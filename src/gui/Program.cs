using Avalonia;
using Avalonia.ReactiveUI;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;

namespace EVTUI;

sealed class Program
{
    private static string AppName = "Global\\EVTUI";
    private static Mutex AppMutex = new Mutex(initiallyOwned: true, AppName);

    private static string LocalDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EVTUI");

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (!Directory.Exists(LocalDir))
            Directory.CreateDirectory(LocalDir);

        if (AppMutex.WaitOne(TimeSpan.Zero, true))
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        else
            // I'd put this in the trace, but there's no listener 'til later in the process, so...
            Console.WriteLine("EVTUI instance already open.");
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
#if (OS_WINDOWS)
            .With(new Win32PlatformOptions { RenderingMode = new Collection<Win32RenderingMode> { Win32RenderingMode.Wgl } })
#endif
            .LogToTrace()
            .UseReactiveUI()
            .With(new X11PlatformOptions
            {
                UseDBusFilePicker = false // to disable FreeDesktop file picker
            });
}

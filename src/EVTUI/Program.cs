using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace EVTUI;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

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

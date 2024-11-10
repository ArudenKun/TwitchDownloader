using System;
using Avalonia;
using AvaloniaExtras.Hosting;
using Serilog;
using TwitchDownloader.Utilities;
using Velopack;

namespace TwitchDownloader;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var appBuilder = BuildAvaloniaApp();

        try
        {
            VelopackApp.Build().Run();
            appBuilder.StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            if (OperatingSystem.IsWindows())
                _ = NativeMethods.Windows.MessageBox(
                    0,
                    ex.ToString(),
                    "TwitchDownloader Fatal Error",
                    0x10
                );
            Log.Logger.Error(ex, "Unhandled exception");
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect().UseHosting().LogToTrace();
}

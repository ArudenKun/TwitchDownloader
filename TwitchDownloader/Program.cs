using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.FileEx;
using SukiUI;
using TwitchDownloader.Core;
using TwitchDownloader.Extensions;
using TwitchDownloader.Helpers;
using Velopack;
using Velopack.Sources;

namespace TwitchDownloader;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ConfigureLogger();

        try
        {
            VelopackApp.Build().Run();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Logger.Fatal(e, "Startup Error");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .WithInterFont()
            .LogToTrace()
            .UsePlatformDetect()
            .UseMicrosoftDependencyInjection(
                services =>
                {
                    services.AddCore();
                    services.AddSingleton(
                        new UpdateManager(
                            new GithubSource(
                                "https://github.com/ArudenKun/TwitchDownloader",
                                null,
                                true
                            )
                        )
                    );

                    services.AddLogging(builder =>
                    {
                        builder.ClearProviders();
                        builder.AddSerilog(dispose: true);
                    });
                },
                _ =>
                {
                    SukiTheme.GetInstance().SetBackgroundAnimationsEnabled(true);
                }
            );

    private static void ConfigureLogger()
    {
        const string OUTPUT_TEMPLATE =
            "[{Timestamp:HH:mm:ss} {Level:u3}][{SourceContext}]: {Message:lj}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Debug)
            .WriteTo.Console(outputTemplate: OUTPUT_TEMPLATE)
            .WriteTo.FileEx(
                EnvironmentHelper.GetApplicationDataPath("logs", "logs.log"),
                "_yyyy-MM-dd",
                outputTemplate: OUTPUT_TEMPLATE,
                rollOnFileSizeLimit: true,
                rollingInterval: RollingInterval.Day
            )
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}

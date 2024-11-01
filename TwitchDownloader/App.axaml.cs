using System;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using TwitchDownloader.Helpers;
using TwitchDownloader.Hosting;
using TwitchDownloader.ViewModels;
using TwitchDownloader.Views;

namespace TwitchDownloader;

public sealed class App : AvaloniaHostingApplication<MainWindow>
{
    public override void Initialize()
    {
        ServicePointManager.DefaultConnectionLimit = 20;
        AvaloniaXamlLoader.Load(this);
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();

        services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);
    }

    protected override void ConfigureLogging(ILoggingBuilder builder)
    {
        const string TEMPLATE =
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {SourceContext} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        var loggingLevelSwitch =
            new LoggingLevelSwitch(EnvironmentHelper.IsDebug
                ? Serilog.Events.LogEventLevel.Debug
                : Serilog.Events.LogEventLevel.Information);

        builder.Services.AddSingleton(_ => loggingLevelSwitch);

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Version", AppInfo.AppVersion)
            .MinimumLevel.ControlledBy(loggingLevelSwitch)
            .WriteTo.Console(outputTemplate: TEMPLATE)
            // .WriteTo.PersistentFile(
            //     AppInfo.LogsDir.Path.JoinPath("logs.txt"),
            //     outputTemplate: TEMPLATE,
            //     persistentFileRollingInterval: PersistentFileRollingInterval.Day,
            //     rollOnFileSizeLimit: true,
            //     retainedFileCountLimit: 61
            // )
            .CreateLogger();

        builder.ClearProviders().AddSerilog(dispose: true);
    }

    protected override void OnInitialized(IServiceProvider services)
    {
        services.GetRequiredService<ILogger<App>>().LogInformation("Twitch Downloader Initialized");
    }

    protected override Task ConfigureMainWindow(MainWindow mainWindow, IServiceProvider services)
    {
        var viewModel = services.GetRequiredService<MainWindowViewModel>();
        mainWindow.ViewModel = viewModel;
        viewModel.Bind(mainWindow);
        return Task.CompletedTask;
    }
}
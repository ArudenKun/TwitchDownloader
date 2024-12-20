using System;
using System.Threading.Tasks;
using AsyncImageLoader;
using AsyncImageLoader.Loaders;
using Avalonia.Markup.Xaml;
using AvaloniaExtras.Hosting;
using AvaloniaExtras.Localization;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using ServiceScan.SourceGenerator;
using TwitchDownloader.Helpers;
using TwitchDownloader.Services;
using TwitchDownloader.Services.Logging;
using TwitchDownloader.Translations;
using TwitchDownloader.ViewModels;
using TwitchDownloader.ViewModels.Abstractions;
using TwitchDownloader.ViewModels.Dialogs;
using TwitchDownloader.Views;

namespace TwitchDownloader;

public sealed partial class App : AvaloniaHostingApplication<MainView>
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Localizer.SetLocalizer(new ResXLocalizer());

        var diskCacheImageLoader = new DiskCachedWebImageLoader(AppInfo.CachesDir);
        ImageLoader.AsyncImageLoader = diskCacheImageLoader;
        ImageBrushLoader.AsyncImageLoader = diskCacheImageLoader;
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        AddScannedViewModels(services);
        services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);
        services.AddSingleton<SettingsService>();
    }

    protected override void ConfigureLogging(ILoggingBuilder builder)
    {
        const string TEMPLATE =
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {SourceContext} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        var loggingLevelSwitch = new LoggingLevelSwitch(
            EnvironmentHelper.IsDebug ? LogEventLevel.Debug : LogEventLevel.Information
        );

        builder.Services.AddSingleton(loggingLevelSwitch);

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Version", AppInfo.AppVersion)
            .MinimumLevel.ControlledBy(loggingLevelSwitch)
            .WriteTo.Console(outputTemplate: TEMPLATE)
            .WriteTo.PersistentFile(
                AppInfo.LogsDir.Path.JoinPath("logs.txt"),
                outputTemplate: TEMPLATE,
                persistentFileRollingInterval: PersistentFileRollingInterval.Day,
                preserveLogFilename: true,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 61
            )
            .CreateLogger();

        builder.ClearProviders().AddSerilog(dispose: true);
    }

    protected override Task ConfigureMainWindow(MainView mainWindow, IServiceProvider services)
    {
        var viewModel = services.GetRequiredService<MainViewModel>();
        mainWindow.ViewModel = viewModel;
        viewModel.Bind(mainWindow);
        return Task.CompletedTask;
    }

    protected override void OnStartup(IServiceProvider services)
    {
        Avalonia.Logging.Logger.Sink = new AvaloniaSerilogAdapter();
        services.GetRequiredService<ILogger<App>>().LogInformation("TwitchDownloader Initialized");
        var settingsService = services.GetRequiredService<SettingsService>();
        var settingsViewModel = services.GetRequiredService<SettingsViewModel>();
        Localizer.Language = settingsViewModel.Language;
        settingsViewModel.ChangeBaseThemeCommand.Execute(settingsService.Theme);
    }

    protected override void OnExit(IServiceProvider services)
    {
        services.GetRequiredService<SettingsService>().Save();
    }

    [GenerateServiceRegistrations(
        AssignableTo = typeof(ISingletonViewModel),
        Lifetime = ServiceLifetime.Singleton,
        AsSelf = true,
        AsImplementedInterfaces = true
    )]
    // [GenerateServiceRegistrations(
    //     AssignableTo = typeof(ITransientViewModel),
    //     Lifetime = ServiceLifetime.Transient,
    //     AsSelf = true,
    //     AsImplementedInterfaces = true
    // )]
    private partial void AddScannedViewModels(IServiceCollection services);
}

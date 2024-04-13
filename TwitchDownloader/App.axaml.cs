using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using TwitchDownloader.Core;
using TwitchDownloader.Services;
using TwitchDownloader.ViewModels;

namespace TwitchDownloader;

public sealed class App : Application
{
    private IServiceProvider _serviceProvider = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _serviceProvider = BuildServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            dialogService.Show(null, dialogService.CreateViewModel<MainWindowViewModel>());
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddCore();
        services.AddSingleton<IDialogService>(sp =>
            new DialogService(new DialogManager((ViewLocator)Current!.DataTemplates.First(),
                new DialogFactory().AddFluent()), sp.GetRequiredService));
        services.AddSingleton<IRouterService, RouterService>();
        services.AddSingleton<INavigationPageFactory, NavigationPageFactory>();
        
        return services.BuildServiceProvider();
    }
}
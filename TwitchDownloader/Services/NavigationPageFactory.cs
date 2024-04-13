using System.Linq;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace TwitchDownloader.Services;

public sealed class NavigationPageFactory : INavigationPageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationPageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Control GetPage(Type srcType) => (Control)_serviceProvider.GetRequiredService(srcType);

    public Control GetPageFromObject(object target)
    {
        var viewLocator = Application.Current?.DataTemplates.FirstOrDefault() as ViewLocator;
        ArgumentNullException.ThrowIfNull(viewLocator);
        return viewLocator.Build(target);
    }
}
using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using TwitchDownloader.Hosting.Internals;

namespace TwitchDownloader.Hosting;

public static class AppBuilderExtensions
{
    public static AppBuilder UseHosting(this AppBuilder appBuilder,
        Action<IServiceCollection>? configureServices = null)
    {
        appBuilder.AfterSetup(_ =>
        {
            if (appBuilder.Instance is IAvaloniaHostingApplicationInitializer avaloniaHostingApplicationInitializer)
            {
                avaloniaHostingApplicationInitializer.InitializeHost(configureServices);
            }
            else
            {
                throw new NotSupportedException("Application must inherit from AvaloniaHostingApplication.");
            }
        });
        return appBuilder;
    }
}
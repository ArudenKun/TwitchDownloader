using Avalonia;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace TwitchDownloader.Extensions;

public static class AppBuilderExtensions
{
    public static AppBuilder UseMicrosoftDependencyInjection(
        this AppBuilder builder,
        Action<IServiceCollection> containerConfig,
        Action<IServiceProvider>? withResolver = null
    ) =>
        builder switch
        {
            null => throw new ArgumentNullException(nameof(builder)),
            _
                => builder.AfterPlatformServicesSetup(_ =>
                {
                    ArgumentNullException.ThrowIfNull(containerConfig);
                    IServiceCollection serviceCollection = new ServiceCollection();
                    containerConfig(serviceCollection);
                    var serviceProvider = serviceCollection.BuildServiceProvider();
                    Ioc.Default.ConfigureServices(serviceProvider);

                    builder.AfterSetup(_ =>
                    {
                        withResolver?.Invoke(serviceProvider);
                    });
                })
        };
}

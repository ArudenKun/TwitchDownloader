using Splat;

namespace TwitchDownloader.Extensions;

public static class SplatExtensions
{
    /// <summary>
    /// Gets an instance of the given T. Must throw if the service is not available (must throw).
    /// </summary>
    /// <param name="resolver"></param>
    /// <param name="contract"></param>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TService GetRequiredService<TService>(
        this IReadonlyDependencyResolver resolver,
        string? contract = null
    )
    {
        var service = resolver.GetService<TService>(contract);
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service), $@"{typeof(TService).Name}");
        }
        return service;
    }

    /// <summary>
    /// Gets an instance of the given serviceType. Must throw if the service is not available (must throw).
    /// </summary>
    /// <param name="resolver"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static object GetRequiredService(this IReadonlyDependencyResolver resolver, Type type)
    {
        var service = resolver.GetService(type);
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service), $@"{type.Name}");
        }

        return service;
    }
}
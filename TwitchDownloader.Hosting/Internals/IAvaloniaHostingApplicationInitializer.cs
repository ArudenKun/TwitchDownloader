using System;
using Microsoft.Extensions.DependencyInjection;

namespace TwitchDownloader.Hosting.Internals;

internal interface IAvaloniaHostingApplicationInitializer
{
    internal void InitializeHost(Action<IServiceCollection>? configureServices); 
}
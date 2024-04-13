using Microsoft.CodeAnalysis;
using SourceGenerator.Helper.CopyCode;
using TwitchDownloader.SourceGenerators.Attributes;

namespace TwitchDownloader.SourceGenerators.Generators;

[Generator]
internal sealed class StaticFileGenerator : IIncrementalGenerator
{
    private const string USINGS_TEXT = """
        global using System;
        """;

    private const string SERVICE_COLLECTION_EXTENSIONS_TEXT = $$"""
        using Microsoft.Extensions.DependencyInjection;

        namespace {{MetadataNames.TWITCH_DOWNLOADER}}.Core;

        public static partial class ServiceCollectionExtensions
        {
            static partial void AddViewModels(IServiceCollection services);

            public static IServiceCollection AddCore(this IServiceCollection services)
            {
                AddViewModels(services);
                return services;
            }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{MetadataNames.TWITCH_DOWNLOADER}.Core.Usings.g.cs", USINGS_TEXT);
            ctx.AddSource(
                $"{MetadataNames.TWITCH_DOWNLOADER}.Core.ServiceCollectionExtensions.g.cs",
                SERVICE_COLLECTION_EXTENSIONS_TEXT
            );
            ctx.AddSource(
                FileName(nameof(SingletonAttribute)),
                Copy.TwitchDownloaderSourceGeneratorsAttributesSingletonAttribute
            );
            ctx.AddSource(
                FileName(nameof(StaticViewLocatorAttribute)),
                Copy.TwitchDownloaderSourceGeneratorsAttributesStaticViewLocatorAttribute
            );
            ctx.AddSource(
                FileName(nameof(IgnoreAttribute)),
                Copy.TwitchDownloaderSourceGeneratorsAttributesIgnoreAttribute
            );
        });
    }

    private static string ParseAttributeName(string attribute) =>
        attribute.Replace("Attribute", "");

    private static string FileName(string attributeName) =>
        $"{MetadataNames.TWITCH_DOWNLOADER}.Core.Attributes.{ParseAttributeName(attributeName)}.g.cs";
}

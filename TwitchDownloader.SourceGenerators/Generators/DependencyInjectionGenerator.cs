using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TwitchDownloader.SourceGenerators.Attributes;
using TwitchDownloader.SourceGenerators.Extensions;
using TwitchDownloader.SourceGenerators.Utilities;

namespace TwitchDownloader.SourceGenerators.Generators;

[Generator]
internal sealed class DependencyInjectionGenerator
    : SourceGeneratorForDeclaredMember<ClassDeclarationSyntax>
{
    protected override (string FileName, string GeneratedCode) GenerateCode(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> nodes,
        AnalyzerConfigOptions options
    )
    {
        var observableObject = compilation.GetTypeByMetadataName(MetadataNames.OBSERVABLE_OBJECT);

        var viewModels = GetAll<INamedTypeSymbol>(nodes)
            .Where(x => !x.IsAbstract)
            .Where(x => x.Name.EndsWith("ViewModel"))
            .Where(x => x.IsOfBaseType(observableObject))
            .Where(x => !x.IsAbstract)
            .ToArray();

        var source = new SourceStringBuilder();

        source.Line();
        source.Line("using Microsoft.Extensions.DependencyInjection;");
        source.Line();

        source.NamespaceBlockBrace(
            $"{MetadataNames.TWITCH_DOWNLOADER}.Core",
            () =>
            {
                source.Line("public static partial class ServiceCollectionExtensions");
                source.BlockBrace(() =>
                {
                    source.Line("static partial void AddViewModels(IServiceCollection services)");
                    source.BlockBrace(() =>
                    {
                        foreach (var viewModel in viewModels)
                        {
                            source.Line(
                                viewModel.HasAttribute(nameof(SingletonAttribute))
                                    ? $"services.AddSingleton<{viewModel.ToFullDisplayString()}>();"
                                    : $"services.AddTransient<{viewModel.ToFullDisplayString()}>();"
                            );

                            if (viewModel.BaseType is { } baseType)
                            {
                                source.Line(
                                    viewModel.HasAttribute(nameof(SingletonAttribute))
                                        ? $"services.AddSingleton<{baseType.ToFullDisplayString()}>(sp => sp.GetRequiredService<{viewModel.ToFullDisplayString()}>());"
                                        : $"services.AddTransient<{baseType.ToFullDisplayString()}>(sp => sp.GetRequiredService<{viewModel.ToFullDisplayString()}>());"
                                );
                            }
                        }
                    });
                });
            }
        );

        return ($"{MetadataNames.TWITCH_DOWNLOADER}.Core.AddViewModels.g.cs", source.ToString());
    }
}

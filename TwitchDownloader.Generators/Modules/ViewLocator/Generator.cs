using System;
using System.Collections.Generic;
using System.Linq;
using H;
using H.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TwitchDownloader.Generators.Abstractions;
using TwitchDownloader.Generators.Extensions;
using TwitchDownloader.Generators.Modules.ViewLocator.Attributes;

namespace TwitchDownloader.Generators.Modules.ViewLocator;

[Generator]
public sealed class Generator
    : SourceGeneratorForTypeWithAttribute<ViewLocatorAttribute>
{
    protected override string Id => "SVLG";

    protected override IEnumerable<FileWithName> StaticSources =>
    [
        new(
            $"{typeof(ViewLocatorAttribute).FullName}",
            Resources.ViewLocatorAttribute_cs.AsString()
        ),
    ];

    protected override string GenerateCode(
        Compilation compilation,
        TypeDeclarationSyntax node,
        INamedTypeSymbol symbol,
        ViewLocatorAttribute attribute,
        AnalyzerConfigOptions options
    )
    {
        var targetSymbol = compilation.GetTypeByMetadataName("CommunityToolkit.Mvvm.ComponentModel.ObservableObject");
        var viewModelSymbols = compilation
            .GlobalNamespace.CollectTypeSymbols(targetSymbol)
            .Where(x => !x.IsAbstract)
            .OrderBy(x => x.ToDisplayString())
            .ToArray();

        var source = new SourceStringBuilder(symbol);

        source.Line();
        source.Line("using System;");
        source.Line("using System.Collections.Generic;");
        source.Line("using Avalonia.Controls;");
        source.Line("using Avalonia.Controls.Templates;");
        source.Line();

        source.PartialTypeBlockBrace(
            "IDataTemplate",
            () =>
            {
                var count = viewModelSymbols
                    .Select(viewModelSymbol => GetView(viewModelSymbol, compilation))
                    .OfType<INamedTypeSymbol>()
                    .Count();

                source.Line(
                    $"public static IReadOnlyDictionary<Type, Func<object, Control>> ViewMap {{ get; }} = new Dictionary<Type, Func<object, Control>>({count})"
                );
                source.BlockDecl(() =>
                {
                    foreach (var viewModelSymbol in viewModelSymbols)
                    {
                        var viewSymbol = GetView(viewModelSymbol, compilation);

                        if (viewSymbol is null)
                            continue;

                        source.Line(
                            $"[typeof({viewModelSymbol.ToFullDisplayString()})] = (viewModel) => new {viewSymbol.ToFullDisplayString()}() {{ ViewModel = ({viewModelSymbol.ToFullDisplayString()})viewModel }},"
                        );
                    }
                });

                source.Line(
                    $"public static IReadOnlyDictionary<Type, Type> ViewTypeMap {{ get; }} = new Dictionary<Type, Type>({count})"
                );
                source.BlockDecl(() =>
                {
                    foreach (var viewModelSymbol in viewModelSymbols)
                    {
                        var viewSymbol = GetView(viewModelSymbol, compilation);

                        if (viewSymbol is null)
                            continue;
                        source.Line(
                            $"[typeof({viewModelSymbol.ToFullDisplayString()})] = typeof({viewSymbol.ToFullDisplayString()}),"
                        );
                    }
                });
            }
        );

        return source.ToString();
    }

    private static INamedTypeSymbol? GetView(ISymbol symbol, Compilation compilation)
    {
        var viewName = symbol.ToDisplayString().Replace("ViewModel", "View");
        var viewSymbol = compilation.GetTypeByMetadataName(viewName);

        if (viewSymbol is not null)
            return viewSymbol;

        viewName = symbol.ToDisplayString().Replace(".ViewModels.", ".Views.");
        viewName = viewName.Remove(viewName.IndexOf("ViewModel", StringComparison.Ordinal));
        return compilation.GetTypeByMetadataName(viewName);
    }
}
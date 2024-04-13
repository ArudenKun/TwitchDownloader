using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TwitchDownloader.SourceGenerators.Attributes;
using TwitchDownloader.SourceGenerators.Extensions;
using TwitchDownloader.SourceGenerators.Utilities;

namespace TwitchDownloader.SourceGenerators.Generators;

[Generator]
internal sealed class StaticViewLocatorGenerator
    : SourceGeneratorForDeclaredTypeWithAttribute<StaticViewLocatorAttribute>
{
    protected override (string GeneratedCode, DiagnosticDetail Error) GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        INamedTypeSymbol symbol,
        AttributeData attribute,
        AnalyzerConfigOptions options
    )
    {
        var observableObject = compilation.GetTypeByMetadataName(MetadataNames.OBSERVABLE_OBJECT);

        var viewModelSymbols = compilation
            .GetSymbolsWithName(x => x.EndsWith("ViewModel"))
            .OfType<INamedTypeSymbol>()
            .Where(x => x.IsOfBaseType(observableObject))
            .Where(x => !x.IsAbstract)
            .ToArray();

        var source = new SourceStringBuilder(symbol);

        source.Line();
        source.Line("using System;");
        source.Line("using System.Collections.Generic;");
        source.Line("using Avalonia.Controls;");
        // source.Line("using HanumanInstitute.MvvmDialogs.Avalonia;");
        source.Line();

        source.PartialTypeBlockBrace(() =>
        {
            // source.Constructor(() =>
            // {
            //     foreach (var viewModelSymbol in viewModelSymbols)
            //     {
            //         var viewName = GetViewName(viewModelSymbol);
            //         var viewSymbol = compilation.GetTypeByMetadataName(viewName);
            //         
            //         if (viewSymbol is null)
            //         {
            //             continue;
            //         }
            //         
            //         source.Line($"Register<{viewModelSymbol.ToFullDisplayString()}, {viewSymbol.ToFullDisplayString()}>();");
            //     }
            // });
            source.Line(
                "public static Dictionary<Type, Func<Control>> Registrations { get; } = new()"
            );
            source.BlockDecl(() =>
            {
                foreach (var viewModelSymbol in viewModelSymbols)
                {
                    var viewName = GetViewName(viewModelSymbol);
                    var view = compilation.GetTypeByMetadataName(viewName);

                    if (view is null)
                    {
                        continue;
                    }

                    source.Line(
                        $"[typeof({viewModelSymbol.ToFullDisplayString()})] = () => new {view.ToFullDisplayString()}(),"
                    );
                }
            });
        });

        return (source.ToString(), null);
    }

    private static string GetViewName(ISymbol symbol)
    {
        var name = symbol.ToDisplayString();

        if (!name.Contains("Window"))
            return name.Replace("ViewModel", "View");

        name = name.Replace(".ViewModels.", ".Views.");
        return name.Remove(name.IndexOf("ViewModel", StringComparison.Ordinal));
    }
}
using System;
using System.Threading;
using CodeGenHelpers;
using H.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TwitchDownloader.Generators.Extensions;

namespace TwitchDownloader.Generators.Modules.Property.ViewModel;

[Generator]
public sealed class Generator : IIncrementalGenerator
{
    private const string ID = "Property.ViewModel.Generator";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.SyntaxProvider.CreateSyntaxProvider(IsSyntaxTarget, (syntaxContext, _) => syntaxContext)
            .Combine(context.CompilationProvider)
            .SelectAndReportExceptions((tuple, _) => Generate(tuple.Left, tuple.Right), context, ID)
            .AddSource(context);
    }

    private static bool IsSyntaxTarget(SyntaxNode node, CancellationToken ct)
    {
        if (node is ClassDeclarationSyntax baseTypeDeclarationSyntax)
        {
            return baseTypeDeclarationSyntax.Identifier.Text.EndsWith("ViewModel");
        }

        return false;
    }

    private static FileWithName Generate(GeneratorSyntaxContext context, Compilation compilation)
    {
        var viewModelSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node);

        if (viewModelSymbol is null)
        {
            return FileWithName.Empty;
        }

        var viewSymbol = GetView(compilation, viewModelSymbol);
        if (viewSymbol is null)
            return FileWithName.Empty;

        var builder = CodeBuilder.Create(viewSymbol);

        builder.AddProperty("ViewModel")
            .SetType((INamedTypeSymbol)viewModelSymbol)
            .WithAccessModifier(Accessibility.Public)
            .WithGetterExpression(
                $"DataContext as {viewModelSymbol.ToFullDisplayString()} ?? throw new global::System.ArgumentNullException(nameof(DataContext))")
            .WithSetterExpression("DataContext = value");


        return new FileWithName($"{viewSymbol.ToDisplayString()}.Property.ViewModel.g.cs", builder.Build());
    }

    private static INamedTypeSymbol? GetView(Compilation compilation, ISymbol? symbol)
    {
        if (symbol is null)
        {
            return null;
        }

        var viewName = symbol.ToDisplayString().Replace("ViewModel", "View");
        var viewSymbol = compilation.GetTypeByMetadataName(viewName);

        if (viewSymbol is not null)
            return viewSymbol;

        viewName = symbol.ToDisplayString().Replace(".ViewModels.", ".Views.");
        viewName = viewName.Remove(viewName.IndexOf("ViewModel", StringComparison.Ordinal));
        return compilation.GetTypeByMetadataName(viewName);
    }
}
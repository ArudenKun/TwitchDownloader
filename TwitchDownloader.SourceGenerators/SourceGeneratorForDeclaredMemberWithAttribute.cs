﻿using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TwitchDownloader.SourceGenerators.Extensions;
using TwitchDownloader.SourceGenerators.Utilities;
using GeneratorContext = Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext;

namespace TwitchDownloader.SourceGenerators;

public abstract class SourceGeneratorForDeclaredMemberWithAttribute<TAttribute, TDeclarationSyntax>
    : IIncrementalGenerator
    where TAttribute : Attribute
    where TDeclarationSyntax : MemberDeclarationSyntax
{
    protected static readonly string AttributeType = typeof(TAttribute).Name;
    protected static readonly string AttributeName = Regex.Replace(
        AttributeType,
        "Attribute$",
        "",
        RegexOptions.Compiled
    );

    protected virtual IEnumerable<(string Name, string Source)> StaticSources =>
        Enumerable.Empty<(string Name, string Source)>();

    public void Initialize(GeneratorContext context)
    {
        foreach (var (name, source) in StaticSources)
            context.RegisterPostInitializationOutput(x => x.AddSource($"{name}.g.cs", source));

        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            IsSyntaxTarget,
            GetSyntaxTarget
        );

        var compilationProvider = context
            .CompilationProvider.Combine(syntaxProvider.Collect())
            .Combine(context.AnalyzerConfigOptionsProvider);
        context.RegisterImplementationSourceOutput(
            compilationProvider,
            (context, provider) =>
                OnExecute(context, provider.Left.Left, provider.Left.Right, provider.Right)
        );

        static bool IsSyntaxTarget(SyntaxNode node, CancellationToken _)
        {
            return node is TDeclarationSyntax type && HasAttributeType();

            bool HasAttributeType()
            {
                if (type.AttributeLists.Count is 0)
                    return false;

                return type
                    .AttributeLists.SelectMany(attributeList => attributeList.Attributes)
                    .Any(attribute => attribute.Name.ToString() == AttributeName);
            }
        }

        static TDeclarationSyntax GetSyntaxTarget(
            GeneratorSyntaxContext context,
            CancellationToken _
        ) => (TDeclarationSyntax)context.Node;

        void OnExecute(
            SourceProductionContext sourceProductionContext,
            Compilation compilation,
            ImmutableArray<TDeclarationSyntax> nodes,
            AnalyzerConfigOptionsProvider options
        )
        {
            try
            {
                foreach (var node in nodes.Distinct())
                {
                    if (sourceProductionContext.CancellationToken.IsCancellationRequested)
                        return;

                    var model = compilation.GetSemanticModel(node.SyntaxTree);
                    var symbol = model.GetDeclaredSymbol(Node(node));
                    var attribute = symbol
                        ?.GetAttributes()
                        .SingleOrDefault(x => x.AttributeClass?.Name == AttributeType);
                    if (attribute is null)
                        continue;

                    var (generatedCode, error) = _GenerateCode(
                        compilation,
                        node,
                        symbol,
                        attribute,
                        options.GlobalOptions
                    );

                    if (generatedCode is null)
                    {
                        var descriptor = new DiagnosticDescriptor(
                            error.Id ?? AttributeName,
                            error.Title,
                            error.Message,
                            error.Category ?? "Usage",
                            DiagnosticSeverity.Error,
                            true
                        );
                        var diagnostic = Diagnostic.Create(
                            descriptor,
                            attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                        );
                        sourceProductionContext.ReportDiagnostic(diagnostic);
                        continue;
                    }

                    sourceProductionContext.AddSource(GenerateFilename(symbol), generatedCode);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }
    }

    protected abstract (string GeneratedCode, DiagnosticDetail Error) GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        ISymbol symbol,
        AttributeData attribute,
        AnalyzerConfigOptions options
    );

    private (string GeneratedCode, DiagnosticDetail Error) _GenerateCode(
        Compilation compilation,
        SyntaxNode node,
        ISymbol symbol,
        AttributeData attribute,
        AnalyzerConfigOptions options
    )
    {
        try
        {
            return GenerateCode(compilation, node, symbol, attribute, options);
        }
        catch (Exception e)
        {
            Log.Error(e);
            return (null, InternalError(e));
        }

        static DiagnosticDetail InternalError(Exception e) =>
            new() { Title = "Internal Error", Message = e.Message };
    }

    private const string EXT = ".g.cs";
    private const int MAX_FILE_LENGTH = 255;

    protected virtual string GenerateFilename(ISymbol symbol)
    {
        var gn = $"{Format(symbol)}{EXT}";
        Log.Debug($"Generated Filename ({gn.Length}): {gn}\n");
        return gn;

        static string Format(ISymbol symbol) =>
            string.Join("_", $"{symbol}".Split(InvalidFileNameChars))
                .Truncate(MAX_FILE_LENGTH - EXT.Length);
    }

    protected virtual SyntaxNode Node(TDeclarationSyntax node) => node;

    private static readonly char[] InvalidFileNameChars =
    [
        '\"',
        '<',
        '>',
        '|',
        '\0',
        (char)1,
        (char)2,
        (char)3,
        (char)4,
        (char)5,
        (char)6,
        (char)7,
        (char)8,
        (char)9,
        (char)10,
        (char)11,
        (char)12,
        (char)13,
        (char)14,
        (char)15,
        (char)16,
        (char)17,
        (char)18,
        (char)19,
        (char)20,
        (char)21,
        (char)22,
        (char)23,
        (char)24,
        (char)25,
        (char)26,
        (char)27,
        (char)28,
        (char)29,
        (char)30,
        (char)31,
        ':',
        '*',
        '?',
        '\\',
        '/'
    ];
}

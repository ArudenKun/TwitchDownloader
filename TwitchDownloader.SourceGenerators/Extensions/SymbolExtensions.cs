using Microsoft.CodeAnalysis;

namespace TwitchDownloader.SourceGenerators.Extensions;

internal static class SymbolExtensions
{
    public static string NamespaceOrNull(this ISymbol symbol) =>
        symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : string.Join(".", symbol.ContainingNamespace.ConstituentNamespaces);

    public static bool HasAttribute(this ISymbol symbol, string attributeName) =>
        symbol.GetAttributes().Any(x => x.AttributeClass?.Name == attributeName);

    public static string ToFullDisplayString(this ISymbol s) =>
        s.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static string AsCommaSeparated<T>(this IEnumerable<T> items, string suffixes = null) =>
        string.Join($",{suffixes}", items);

    public static bool InheritsFrom(this ITypeSymbol symbol, string type)
    {
        var baseType = symbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == type)
                return true;

            baseType = baseType.BaseType;
        }

        return false;
    }

    public static bool IsOfBaseType(this ITypeSymbol type, ITypeSymbol baseType)
    {
        if (type is ITypeParameterSymbol p)
            return p.ConstraintTypes.Any(ct => ct.IsOfBaseType(baseType));

        while (type != null)
        {
            if (SymbolEqualityComparer.Default.Equals(type, baseType))
                return true;

            type = type.BaseType;
        }

        return false;
    }
}

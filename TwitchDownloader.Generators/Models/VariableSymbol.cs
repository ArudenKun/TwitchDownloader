using Dunet;
using Microsoft.CodeAnalysis;

namespace TwitchDownloader.Generators.Models;

[Union]
public partial record VariableSymbol
{
    partial record Property(IPropertySymbol PropertySymbol);

    partial record Field(IFieldSymbol FieldSymbol);
}
using Lucdem.Avalonia.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;
using System;

namespace Lucdem.Avalonia.SourceGenerators.Models;

internal record AvaloniaPropertyInfo(
    HierarchyInfo HierarchyInfo,
    string TypeName,
    string PropertyName,
    string AccessorName,
    string? FieldName)
{
    internal static AvaloniaPropertyInfo FromFieldSymbol(IFieldSymbol fieldSymbol)
    {
        string typeNameWithNullabilityAnnotations = fieldSymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
        string clrAccessorName = GetAccessorsName(fieldSymbol);
        string propertyName = $"{clrAccessorName}Property";

        return new(
            HierarchyInfo.From(fieldSymbol.ContainingType),
            typeNameWithNullabilityAnnotations, 
            propertyName,
            clrAccessorName,
            fieldSymbol.Name);
    }

    /// <summary> Get the name for the generated accessor </summary>
    /// <param name="fieldSymbol"> The field symbol to be converted into an AvaloniaProperty</param>
    /// <returns> The name used for the property accessors </returns>
    private static string GetAccessorsName(IFieldSymbol fieldSymbol)
    {
        var span = fieldSymbol.Name.AsSpan();
        if (fieldSymbol.Name.StartsWith("_"))
        {
            span = span.Slice(1);
        }
        var chars = span.ToArray();
        chars[0] = char.ToUpperInvariant(chars[0]);
        return new string(chars);
    }
}

using Microsoft.CodeAnalysis;

namespace Lucdem.Avalonia.SourceGenerators.Extensions;

internal static class ISymbolExtensions
{
    /// <summary> Gets the fully qualified name for a given symbol. </summary>
    /// <param name="symbol"> The input <see cref="ISymbol"/> instance. </param>
    /// <returns> The fully qualified name for <paramref name="symbol"/>. </returns>
    internal static string GetFullyQualifiedName(this ISymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    /// <summary>
    ///     Gets the fully qualified name for a given symbol, including nullability annotations
    /// </summary>
    /// <param name="symbol"> The input <see cref="ISymbol"/> instance. </param>
    /// <returns> The fully qualified name for <paramref name="symbol"/>. </returns>
    internal static string GetFullyQualifiedNameWithNullabilityAnnotations(this ISymbol symbol)
    {
        var format = SymbolDisplayFormat
            .FullyQualifiedFormat
            .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
        return symbol.ToDisplayString(format);
    }
}

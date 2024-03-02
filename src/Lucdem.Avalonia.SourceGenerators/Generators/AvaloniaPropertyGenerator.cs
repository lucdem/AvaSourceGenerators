using Lucdem.Avalonia.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace Lucdem.Avalonia.SourceGenerators.Generators;

internal abstract class AvaloniaPropertyGenerator
{
    /// <summary>
    ///     Looks for fields that have an attribute with <paramref name="attributeName"/>, and 
    ///     returns a <see cref="IncrementalValuesProvider{AvaloniaPropertyInfo}"/> with the information
    ///     required to generate the properties
    /// </summary>
    /// <param name="context">The source generator context</param>
    /// <param name="attributeName">Fully qualified name of the attribute being looked for</param>
    /// <returns>
    ///     <see cref="IncrementalValuesProvider{AvaloniaPropertyInfo}"/> with the info of the properties that must be generated
    /// </returns>
    protected static IncrementalValuesProvider<AvaloniaPropertyInfo> GetPropertiesFromGeneratorContext
        (IncrementalGeneratorInitializationContext context, string attributeName)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(
            attributeName,
            IsAttrField,
            GetPropertyInfo);
    }

    private static bool IsAttrField(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return syntaxNode is VariableDeclaratorSyntax
        {
            Parent: VariableDeclarationSyntax
            {
                Parent: FieldDeclarationSyntax
                {
                    Parent: ClassDeclarationSyntax or RecordDeclarationSyntax,
                    AttributeLists.Count: > 0
                }
            }
        };
    }

    private static AvaloniaPropertyInfo GetPropertyInfo(GeneratorAttributeSyntaxContext context, CancellationToken token)
        => AvaloniaPropertyInfo.FromFieldSymbol((IFieldSymbol)context.TargetSymbol);
}

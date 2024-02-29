using Lucdem.Avalonia.SourceGenerators.Extensions;
using Lucdem.Avalonia.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lucdem.Avalonia.SourceGenerators.Generators;

[Generator]
internal class StyledPropertyGenerator : IIncrementalGenerator
{
    private const string _attrName = "Lucdem.Avalonia.SourceGenerators.Attributes.AvaStyledPropertyAttribute";

    record StyledPropertyInfo(
        HierarchyInfo HierarchyInfo,
        string TypeName,
        string PropertyName,
        string ClrAccessorName);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var propertyInfo = context.SyntaxProvider.ForAttributeWithMetadataName(
            _attrName,
            IsAttrField,
            GetStyledPropertyInfo);

        // Split and group by containing type
        var groupedPropertyInfo = propertyInfo.GroupBy(static p => p.HierarchyInfo, static p => p);

        context.RegisterSourceOutput(groupedPropertyInfo, Execute);
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

    private static StyledPropertyInfo GetStyledPropertyInfo(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var fieldSymbol = (IFieldSymbol)context.TargetSymbol;

        string typeNameWithNullabilityAnnotations = fieldSymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
        string clrAccessorName = GetGeneratedClrAccessorName(fieldSymbol);
        string propertyName = $"{clrAccessorName}Property";

        return new(HierarchyInfo.From(fieldSymbol.ContainingType), typeNameWithNullabilityAnnotations, propertyName, clrAccessorName);
    }

    private static void Execute(
        SourceProductionContext context,
        (HierarchyInfo, ImmutableArray<StyledPropertyInfo>) propsPerClass)
    {
        var (hierarchy, props) = propsPerClass;

        var declarations = StyledPropsAndClrAccessorsDeclarations(props)
            .ToImmutableArray();

        // Insert all members into the same partial type declaration
        var compilationUnit = hierarchy.GetCompilationUnit(declarations);

        var srcText = compilationUnit.GetText(Encoding.UTF8).ToString();

        context.AddSource($"{hierarchy.FilenameHint}.g.cs", srcText);
    }

    /// <summary> Get the name for the generated CLR accessor </summary>
    /// <param name="fieldSymbol"> The field symbol to be converted into an Avalonia styled property</param>
    /// <returns> The generated CLR accessor name for the styled property </returns>
    private static string GetGeneratedClrAccessorName(IFieldSymbol fieldSymbol)
    {
        var chars = fieldSymbol.Name.ToCharArray();
        chars[0] = char.ToUpperInvariant(chars[0]);
        return new string(chars);
    }

    private static IEnumerable<MemberDeclarationSyntax> StyledPropsAndClrAccessorsDeclarations(IEnumerable<StyledPropertyInfo> props)
    {
        foreach (var p in props)
        {
            yield return SyntaxFactories.StyledProperty.Declaration(
                p.HierarchyInfo.Hierarchy[0].QualifiedName,
                p.TypeName,
                p.PropertyName,
                p.ClrAccessorName);

            yield return SyntaxFactories.AvaloniaObject.ClrPropertyDeclaration(
                typeof(StyledPropertyGenerator),
                p.TypeName,
                p.PropertyName,
                p.ClrAccessorName);
        }
    }
}
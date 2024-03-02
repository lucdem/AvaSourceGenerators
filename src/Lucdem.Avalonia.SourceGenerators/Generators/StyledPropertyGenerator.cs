using Lucdem.Avalonia.SourceGenerators.Extensions;
using Lucdem.Avalonia.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Lucdem.Avalonia.SourceGenerators.Generators;

[Generator]
internal class StyledPropertyGenerator : AvaloniaPropertyGenerator, IIncrementalGenerator
{
    private const string _attrName = "Lucdem.Avalonia.SourceGenerators.Attributes.AvaStyledPropertyAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var propertyInfo = GetPropertiesFromGeneratorContext(context, _attrName);

        // Split and group by containing type
        var groupedPropertyInfo = propertyInfo.GroupBy(static p => p.HierarchyInfo, static p => p);

        context.RegisterSourceOutput(groupedPropertyInfo, Execute);
    }

    private static void Execute(
        SourceProductionContext context,
        (HierarchyInfo, ImmutableArray<AvaloniaPropertyInfo>) propsPerClass)
    {
        var (hierarchy, props) = propsPerClass;

        var declarations = StyledPropsAndClrAccessorsDeclarations(props)
            .ToImmutableArray();

        // Insert all members into the same partial type declaration
        var compilationUnit = hierarchy.GetCompilationUnit(declarations);

        var srcText = compilationUnit.GetText(Encoding.UTF8).ToString();

        context.AddSource($"{hierarchy.FilenameHint}.g.cs", srcText);
    }

    private static IEnumerable<MemberDeclarationSyntax> StyledPropsAndClrAccessorsDeclarations(IEnumerable<AvaloniaPropertyInfo> props)
    {
        foreach (var p in props)
        {
            yield return SyntaxFactories.StyledProperty.Declaration(
                p.HierarchyInfo.Hierarchy[0].QualifiedName,
                p.TypeName,
                p.PropertyName,
                p.AccessorName);

            yield return SyntaxFactories.AvaloniaObject.GetSetValuePropertyDeclaration(
                typeof(StyledPropertyGenerator),
                p.TypeName,
                p.PropertyName,
                p.AccessorName);
        }
    }
}
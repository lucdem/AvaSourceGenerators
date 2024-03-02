using Lucdem.Avalonia.SourceGenerators.Extensions;
using Lucdem.Avalonia.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Lucdem.Avalonia.SourceGenerators.Utils;
using Microsoft.CodeAnalysis.CSharp;

namespace Lucdem.Avalonia.SourceGenerators.Generators;

[Generator]
internal class DirectPropertyGenerator : AvaloniaPropertyGenerator, IIncrementalGenerator
{
    private const string _attrName = "Lucdem.Avalonia.SourceGenerators.Attributes.AvaDirectPropertyAttribute";

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

        var declarations = PropertyAndAccessorsDeclarations(props)
            .ToImmutableArray();

        // Insert all members into the same partial type declaration
        var compilationUnit = hierarchy.GetCompilationUnit(declarations);

        var srcText = compilationUnit.GetText(Encoding.UTF8).ToString();

        context.AddSource($"{hierarchy.FilenameHint}.g.cs", srcText);
    }

    private static PropertyDeclarationSyntax PropertyAccessorDeclaration(
        string propertyTypeName,
        string propertyName,
        string accessorName,
        string backingFieldName)
    {
        var basicGetter = SyntaxFactoryUtils.BasicGetterDeclaration(backingFieldName);
        var setter = SyntaxFactories.AvaloniaObject.SetAndRaiseSetterDeclaration(propertyName, backingFieldName);

        var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.IdentifierName(propertyTypeName),
            SyntaxFactory.Identifier(accessorName));
        
        return propertyDeclaration
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAttributeLists(
                SyntaxFactory.AttributeList(SyntaxFactoryUtils.ExcludeFromCodeCoverageAttributeSyntax()),
                SyntaxFactory.AttributeList(SyntaxFactoryUtils.CompilerGeneratedAttributeSyntax(typeof(DirectPropertyGenerator))))
            .AddAccessorListAccessors(basicGetter, setter);
    }
    
    private static IEnumerable<MemberDeclarationSyntax> PropertyAndAccessorsDeclarations(IEnumerable<AvaloniaPropertyInfo> props)
    {
        foreach (var p in props)
        {
            yield return SyntaxFactories.DirectProperty.Declaration(
                p.HierarchyInfo.Hierarchy[0].QualifiedName,
                p.TypeName,
                p.PropertyName,
                p.AccessorName);

            yield return PropertyAccessorDeclaration(
                p.TypeName,
                p.PropertyName,
                p.AccessorName,
                p.FieldName!);
        }
    }
}
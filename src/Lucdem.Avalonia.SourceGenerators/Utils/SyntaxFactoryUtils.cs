using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lucdem.Avalonia.SourceGenerators.Utils;

internal static class SyntaxFactoryUtils
{
    internal static SeparatedSyntaxList<AttributeSyntax> CompilerGeneratedAttributeSyntax(Type generatorType)
    {
        var generatorNameArg = AttributeArgument(
            LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(generatorType.FullName!)));
        var generatorVersionArg = AttributeArgument(
            LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(generatorType.Assembly.GetName().Version.ToString())));

        var generatedAttribute = Attribute(IdentifierName("global::System.CodeDom.Compiler.GeneratedCode"))
            .AddArgumentListArguments(generatorNameArg, generatorVersionArg);
        return SingletonSeparatedList(generatedAttribute);
    }

    internal static SeparatedSyntaxList<AttributeSyntax> ExcludeFromCodeCoverageAttributeSyntax()
    {
        var attribute = Attribute(IdentifierName("global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage"));
        return SingletonSeparatedList(attribute);
    }
    
    /// <summary>
    /// Create the declaration for a basic getter returning a backing field (get => backingField;)
    /// </summary>
    /// <param name="backingFieldName">The name of the backing field</param>
    /// <returns>The getter declaration</returns>
    internal static AccessorDeclarationSyntax BasicGetterDeclaration(string backingFieldName)
    {
        return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithExpressionBody(ArrowExpressionClause(IdentifierName(backingFieldName)))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
}

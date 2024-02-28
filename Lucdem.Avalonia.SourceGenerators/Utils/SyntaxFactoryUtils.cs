﻿using Microsoft.CodeAnalysis;
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
                Literal(generatorType.FullName)));
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
}

using Lucdem.Avalonia.SourceGenerators.Utils;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace Lucdem.Avalonia.SourceGenerators;

internal static class SyntaxFactories
{
    internal static class AvaloniaObject
    {
        static InvocationExpressionSyntax GetValueDeclaration(string propertyName)
        {
            var getterId = IdentifierName("GetValue");
            var argumentId = IdentifierName(propertyName);
            return InvocationExpression(getterId)
                .AddArgumentListArguments(Argument(argumentId));
        }

        static AccessorDeclarationSyntax ClrGetterDeclaration(string propertyName)
        {
            var getterSyntax = GetValueDeclaration(propertyName);
            return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithExpressionBody(ArrowExpressionClause(getterSyntax))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        static InvocationExpressionSyntax SetValueDeclaration(string propertyName)
        {
            var setterId = IdentifierName("SetValue");
            var argumentId = IdentifierName(propertyName);
            return InvocationExpression(setterId)
                .AddArgumentListArguments(Argument(argumentId), Argument(IdentifierName("value")));
        }

        static AccessorDeclarationSyntax ClrSetterDeclaration(string propertyName)
        {
            var setterSyntax = SetValueDeclaration(propertyName);
            return AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithExpressionBody(ArrowExpressionClause(setterSyntax))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        static AccessorDeclarationSyntax[] ClrAccessorsDeclaration(string propertyName)
        {
            return [ClrGetterDeclaration(propertyName), ClrSetterDeclaration(propertyName)];
        }

        internal static PropertyDeclarationSyntax ClrPropertyDeclaration(Type generatorType, string propertyType, string avaloniaPropertyName, string clrAccessorsName)
        {
            return PropertyDeclaration(IdentifierName(propertyType), Identifier(clrAccessorsName))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAttributeLists(
                    AttributeList(SyntaxFactoryUtils.ExcludeFromCodeCoverageAttributeSyntax()),
                    AttributeList(SyntaxFactoryUtils.CompilerGeneratedAttributeSyntax(generatorType)))
                .AddAccessorListAccessors(ClrAccessorsDeclaration(avaloniaPropertyName));
        }
    }

    internal static class StyledProperty
    {
        static InvocationExpressionSyntax RegisterMethodDeclaration(string controlName, string propertyTypeName, string clrAccessorName)
        {
            TypeSyntax controlType = IdentifierName(controlName);
            TypeSyntax propertyType = IdentifierName(propertyTypeName);
            var typeArgs = SeparatedList([controlType, propertyType]);
            var method = GenericName(
                Identifier("global::Avalonia.AvaloniaProperty.Register"),
                TypeArgumentList(typeArgs));

            var clrLiteralExpression = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(clrAccessorName));
            var methodArgs = SeparatedList([Argument(clrLiteralExpression)]);
            return InvocationExpression(method)
                .WithArgumentList(ArgumentList(methodArgs));
        }

        internal static FieldDeclarationSyntax Declaration(string controlName, string propertyTypeName, string propertyName, string clrAccessorName)
        {
            TypeSyntax propType = IdentifierName(Identifier(propertyTypeName));
            var styledPropType = GenericName(
                Identifier("global::Avalonia.StyledProperty"),
                TypeArgumentList(SeparatedList([propType])));

            var invocation = RegisterMethodDeclaration(controlName, propertyTypeName, clrAccessorName);

            var varDeclarator = VariableDeclarator(Identifier(propertyName))
                .WithInitializer(EqualsValueClause(invocation));

            var variable = VariableDeclaration(styledPropType)
                .WithVariables(SingletonSeparatedList(varDeclarator));

            return FieldDeclaration(variable)
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword),
                    Token(SyntaxKind.ReadOnlyKeyword));
        }
    }
}

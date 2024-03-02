using Lucdem.Avalonia.SourceGenerators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace Lucdem.Avalonia.SourceGenerators;

internal static class SyntaxFactories
{
    internal static class AvaloniaObject
    {
        private static InvocationExpressionSyntax GetValueDeclaration(string propertyName)
        {
            var getterId = IdentifierName("GetValue");
            var argumentId = IdentifierName(propertyName);
            return InvocationExpression(getterId)
                .AddArgumentListArguments(Argument(argumentId));
        }

        private static AccessorDeclarationSyntax GetValueGetterDeclaration(string propertyName)
        {
            var getterSyntax = GetValueDeclaration(propertyName);
            return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithExpressionBody(ArrowExpressionClause(getterSyntax))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        private static InvocationExpressionSyntax SetValueDeclaration(string propertyName)
        {
            var setterId = IdentifierName("SetValue");
            var argumentId = IdentifierName(propertyName);
            return InvocationExpression(setterId)
                .AddArgumentListArguments(Argument(argumentId), Argument(IdentifierName("value")));
        }

        private static AccessorDeclarationSyntax SetValueSetterDeclaration(string propertyName)
        {
            var setterSyntax = SetValueDeclaration(propertyName);
            return AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithExpressionBody(ArrowExpressionClause(setterSyntax))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        private static AccessorDeclarationSyntax[] GetSetValueAccessorsDeclarations(string propertyName)
        {
            return [GetValueGetterDeclaration(propertyName), SetValueSetterDeclaration(propertyName)];
        }

        /// <summary>
        /// Create a <see cref="PropertyDeclarationSyntax"/> with acessors for an AvaloniaProperty
        /// using the AvaloniaObject GetValue and SetValue methods 
        /// </summary>
        /// <param name="generatorType">The type of the source generator creating the property</param>
        /// <param name="propertyTypeName">The type name of the AvaloniaProperty</param>
        /// <param name="avaloniaPropertyName">The name of the AvaloniaProperty</param>
        /// <param name="accessorsName">The name used for the property accessors</param>
        /// <returns>The resulting <see cref="PropertyDeclarationSyntax"/></returns>
        internal static PropertyDeclarationSyntax GetSetValuePropertyDeclaration(
            Type generatorType,
            string propertyTypeName,
            string avaloniaPropertyName,
            string accessorsName)
        {
            return PropertyDeclaration(IdentifierName(propertyTypeName), Identifier(accessorsName))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAttributeLists(
                    AttributeList(SyntaxFactoryUtils.ExcludeFromCodeCoverageAttributeSyntax()),
                    AttributeList(SyntaxFactoryUtils.CompilerGeneratedAttributeSyntax(generatorType)))
                .AddAccessorListAccessors(GetSetValueAccessorsDeclarations(avaloniaPropertyName));
        }

        internal static AccessorDeclarationSyntax SetAndRaiseSetterDeclaration(
            string propertyName,
            string backingFieldName)
        {
            var arguments = ArgumentList(SeparatedList([
                Argument(IdentifierName(propertyName)),
                Argument(IdentifierName(backingFieldName)).WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword)),
                Argument(IdentifierName("value"))
            ]));
            var setAndRaiseInvocation = InvocationExpression(IdentifierName("SetAndRaise"))
                .WithArgumentList(arguments);
            return AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithExpressionBody(ArrowExpressionClause(setAndRaiseInvocation))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }
    }

    internal static class StyledProperty
    {
        private static InvocationExpressionSyntax RegisterMethodDeclaration(
            string controlTypeName,
            string propertyTypeName,
            string clrAccessorName)
        {
            TypeSyntax controlType = IdentifierName(controlTypeName);
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

        internal static FieldDeclarationSyntax Declaration(
            string controlTypeName,
            string propertyTypeName,
            string propertyName,
            string clrAccessorName)
        {
            TypeSyntax propType = IdentifierName(Identifier(propertyTypeName));
            var styledPropType = GenericName(
                Identifier("global::Avalonia.StyledProperty"),
                TypeArgumentList(SeparatedList([propType])));

            var invocation = RegisterMethodDeclaration(controlTypeName, propertyTypeName, clrAccessorName);

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

    internal static class DirectProperty
    {
        private static SimpleLambdaExpressionSyntax GetterLambdaExpression(string accessorName)
        {
            const string objName = "o";
            return SimpleLambdaExpression(Parameter(Identifier(objName)))
                .WithExpressionBody(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(objName),
                        IdentifierName(accessorName)));
        }

        private static ParenthesizedLambdaExpressionSyntax SetterLambdaExpression(string accessorName)
        {
            const string objName = "o";
            const string valueName = "v";
            return ParenthesizedLambdaExpression()
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(Identifier(objName)),
                                Token(SyntaxKind.CommaToken),
                                Parameter(Identifier(valueName))
                            })))
                .WithExpressionBody(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(objName),
                            IdentifierName(accessorName)),
                        IdentifierName(valueName)));
        }

        private static InvocationExpressionSyntax RegisterMethodDeclaration(
            string controlTypeName,
            string propertyTypeName,
            string clrAccessorName)
        {
            TypeSyntax controlType = IdentifierName(controlTypeName);
            TypeSyntax propertyType = IdentifierName(propertyTypeName);
            var typeArgs = SeparatedList([controlType, propertyType]);
            var method = GenericName(
                Identifier("global::Avalonia.AvaloniaProperty.RegisterDirect"),
                TypeArgumentList(typeArgs));

            var clrLiteralExpression =
                LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(clrAccessorName));
            var methodArgs = SeparatedList([
                Argument(clrLiteralExpression),
                Argument(GetterLambdaExpression(clrAccessorName)),
                Argument(SetterLambdaExpression(clrAccessorName))
            ]);

            return InvocationExpression(method)
                .WithArgumentList(ArgumentList(methodArgs));
        }

        internal static FieldDeclarationSyntax Declaration(
            string controlTypeName,
            string propertyTypeName,
            string propertyName,
            string clrAccessorName)
        {
            TypeSyntax controlType = IdentifierName(Identifier(controlTypeName));
            TypeSyntax propType = IdentifierName(Identifier(propertyTypeName));
            var styledPropType = GenericName(
                Identifier("global::Avalonia.DirectProperty"),
                TypeArgumentList(SeparatedList([controlType, propType])));

            var invocation = RegisterMethodDeclaration(controlTypeName, propertyTypeName, clrAccessorName);

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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Lucdem.Avalonia.SourceGenerators.Models;

internal static class TypeSymbolExtension
{
    internal static string GetFullyQualifiedMetadataName(this ITypeSymbol symbol)
    {
        var builder = new StringBuilder();

        static void BuildFrom(ISymbol? symbol, in StringBuilder builder)
        {
            switch (symbol)
            {
            // Namespaces that are nested also append a leading '.'
            case INamespaceSymbol { ContainingNamespace.IsGlobalNamespace: false }:
                BuildFrom(symbol.ContainingNamespace, in builder);
                builder.Append('.');
                builder.Append(symbol.MetadataName);
                break;

            // Other namespaces (ie. the one right before global) skip the leading '.'
            case INamespaceSymbol { IsGlobalNamespace: false }:
                builder.Append(symbol.MetadataName);
                break;

            // Types with no namespace just have their metadata name directly written
            case ITypeSymbol { ContainingSymbol: INamespaceSymbol { IsGlobalNamespace: true } }:
                builder.Append(symbol.MetadataName);
                break;

            // Types with a containing non-global namespace also append a leading '.'
            case ITypeSymbol { ContainingSymbol: INamespaceSymbol namespaceSymbol }:
                BuildFrom(namespaceSymbol, in builder);
                builder.Append('.');
                builder.Append(symbol.MetadataName);
                break;

            // Nested types append a leading '+'
            case ITypeSymbol { ContainingSymbol: ITypeSymbol typeSymbol }:
                BuildFrom(typeSymbol, in builder);
                builder.Append('+');
                builder.Append(symbol.MetadataName);
                break;
            default:
                break;
            }
        }

        BuildFrom(symbol, in builder);

        return builder.ToString();
    }
}

internal sealed record HierarchyInfo(string FilenameHint, string MetadataName, string Namespace, ImmutableArray<TypeInfo> Hierarchy)
{
    /// <summary>
    /// Creates a new <see cref="HierarchyInfo"/> instance from a given <see cref="INamedTypeSymbol"/>.
    /// </summary>
    /// <param name="typeSymbol">The input <see cref="INamedTypeSymbol"/> instance to gather info for.</param>
    /// <returns>A <see cref="HierarchyInfo"/> instance describing <paramref name="typeSymbol"/>.</returns>
    public static HierarchyInfo From(INamedTypeSymbol typeSymbol)
    {
        var hierarchy = ImmutableArray.CreateBuilder<TypeInfo>();

        for (INamedTypeSymbol? parent = typeSymbol;
             parent is not null;
             parent = parent.ContainingType)
        {
            hierarchy.Add(new TypeInfo(
                parent.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                parent.TypeKind,
                parent.IsRecord));
        }

        var displayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        return new(
            typeSymbol.GetFullyQualifiedMetadataName(),
            typeSymbol.MetadataName,
            typeSymbol.ContainingNamespace.ToDisplayString(displayFormat),
            hierarchy.ToImmutable());
    }

    /// <summary>
    /// Creates a <see cref="CompilationUnitSyntax"/> instance wrapping the given members.
    /// </summary>
    /// <param name="memberDeclarations">The input <see cref="MemberDeclarationSyntax"/> instances to use.</param>
    /// <param name="baseList">The optional <see cref="BaseListSyntax"/> instance to add to generated types.</param>
    /// <returns>A <see cref="CompilationUnitSyntax"/> object wrapping <paramref name="memberDeclarations"/>.</returns>
    public CompilationUnitSyntax GetCompilationUnit(
        ImmutableArray<MemberDeclarationSyntax> memberDeclarations,
        BaseListSyntax? baseList = null)
    {
        // Create the partial type declaration with the given member declarations.
        // This code produces a class declaration as follows:
        //
        // /// <inheritdoc/>
        // partial <TYPE_KIND> TYPE_NAME>
        // {
        //     <MEMBERS>
        // }
        TypeDeclarationSyntax typeDeclarationSyntax =
            Hierarchy[0].GetSyntax()
            .AddModifiers(Token(TriviaList(Comment("/// <inheritdoc/>")), SyntaxKind.PartialKeyword, TriviaList()))
            .AddMembers(memberDeclarations.ToArray());

        // Add the base list, if present
        if (baseList is not null)
        {
            typeDeclarationSyntax = typeDeclarationSyntax.WithBaseList(baseList);
        }

        // Add all parent types in ascending order, if any
        foreach (TypeInfo parentType in Hierarchy.AsSpan().Slice(1))
        {
            typeDeclarationSyntax =
                parentType.GetSyntax()
                .AddModifiers(Token(TriviaList(Comment("/// <inheritdoc/>")), SyntaxKind.PartialKeyword, TriviaList()))
                .AddMembers(typeDeclarationSyntax);
        }

        // Prepare the leading trivia for the generated compilation unit.
        // This will produce code as follows:
        //
        // <auto-generated/>
        // #pragma warning disable
        // #nullable enable
        SyntaxTriviaList syntaxTriviaList = TriviaList(
            Comment("// <auto-generated/>"),
            Trivia(PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)),
            Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)));

        if (Namespace is "")
        {
            // Special case for top level types with no namespace: we need to re-add the
            // inheritdoc XML comment, as otherwise the call below would remove it.
            syntaxTriviaList = syntaxTriviaList.Add(Comment("/// <inheritdoc/>"));

            // If there is no namespace, attach the pragma directly to the declared type,
            // and skip the namespace declaration. This will produce code as follows:
            //
            // <SYNTAX_TRIVIA>
            // <TYPE_HIERARCHY>
            return
                CompilationUnit()
                .AddMembers(typeDeclarationSyntax.WithLeadingTrivia(syntaxTriviaList))
                .NormalizeWhitespace();
        }

        // Create the compilation unit with disabled warnings, target namespace and generated type.
        // This will produce code as follows:
        //
        // <SYNTAX_TRIVIA>
        // namespace <NAMESPACE>
        // {
        //     <TYPE_HIERARCHY>
        // }
        return
            CompilationUnit().AddMembers(
            NamespaceDeclaration(IdentifierName(Namespace))
            .WithLeadingTrivia(syntaxTriviaList)
            .AddMembers(typeDeclarationSyntax))
            .NormalizeWhitespace();
    }
}

/// <summary>
/// A model describing a type info in a type hierarchy.
/// </summary>
/// <param name="QualifiedName">The qualified name for the type.</param>
/// <param name="Kind">The type of the type in the hierarchy.</param>
/// <param name="IsRecord">Whether the type is a record type.</param>
internal sealed record TypeInfo(string QualifiedName, TypeKind Kind, bool IsRecord)
{
    /// <summary>
    /// Creates a <see cref="TypeDeclarationSyntax"/> instance for the current info.
    /// </summary>
    /// <returns>A <see cref="TypeDeclarationSyntax"/> instance for the current info.</returns>
    public TypeDeclarationSyntax GetSyntax()
    {
        // Create the partial type declaration with the kind.
        // This code produces a class declaration as follows:
        //
        // <TYPE_KIND> <TYPE_NAME>
        // {
        // }
        //
        // Note that specifically for record declarations, we also need to explicitly add the open
        // and close brace tokens, otherwise member declarations will not be formatted correctly.
        return Kind switch
        {
            TypeKind.Struct => StructDeclaration(QualifiedName),
            TypeKind.Interface => InterfaceDeclaration(QualifiedName),
            TypeKind.Class when IsRecord =>
                RecordDeclaration(Token(SyntaxKind.RecordKeyword), QualifiedName)
                .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken)),
            _ => ClassDeclaration(QualifiedName)
        };
    }
}

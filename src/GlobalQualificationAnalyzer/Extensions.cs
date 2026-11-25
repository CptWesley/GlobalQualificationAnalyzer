using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GlobalQualificationAnalyzer;

/// <summary>
/// Provides various extension methods.
/// </summary>
internal static class Extensions
{
    /// <summary>Returns true if the leftmost part of <paramref name="node"/> is <c>global::</c>.</summary>
    /// <param name="node">The name or member access to inspect.</param>
    /// <returns>True if globally qualified.</returns>
    public static bool IsGloballyQualified(this ExpressionSyntax node) => node switch
    {
        QualifiedNameSyntax qualified => IsGloballyQualified(qualified.Left),
        MemberAccessExpressionSyntax access => IsGloballyQualified(access.Expression),
        AliasQualifiedNameSyntax aliasQualified => aliasQualified.Alias.Identifier.IsKind(SyntaxKind.GlobalKeyword),
        _ => false,
    };

    /// <summary>Gets the globally qualified name of a built-in type alias.</summary>
    /// <param name="keyword">The keyword of the built-in type alias.</param>
    /// <returns>The globally qualified name, or null for <c>void</c>.</returns>
    public static string? QualifiedName(this SyntaxKind keyword) => keyword switch
    {
        SyntaxKind.BoolKeyword => "global::System.Boolean",
        SyntaxKind.ByteKeyword => "global::System.Byte",
        SyntaxKind.SByteKeyword => "global::System.SByte",
        SyntaxKind.CharKeyword => "global::System.Char",
        SyntaxKind.DecimalKeyword => "global::System.Decimal",
        SyntaxKind.DoubleKeyword => "global::System.Double",
        SyntaxKind.FloatKeyword => "global::System.Single",
        SyntaxKind.IntKeyword => "global::System.Int32",
        SyntaxKind.UIntKeyword => "global::System.UInt32",
        SyntaxKind.LongKeyword => "global::System.Int64",
        SyntaxKind.ULongKeyword => "global::System.UInt64",
        SyntaxKind.ShortKeyword => "global::System.Int16",
        SyntaxKind.UShortKeyword => "global::System.UInt16",
        SyntaxKind.ObjectKeyword => "global::System.Object",
        SyntaxKind.StringKeyword => "global::System.String",
        _ => null,
    };
}

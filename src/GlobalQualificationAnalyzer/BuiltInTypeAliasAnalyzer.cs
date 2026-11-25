using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GlobalQualificationAnalyzer;

/// <summary>
/// Verifies that built-in type aliases (e.g. <c>string</c>) are replaced by
/// their globally qualified equivalent (e.g. <c>global::System.String</c>).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BuiltInTypeAliasAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule
        = RuleHelper.Create(
            2,
            "Use globally qualified type instead of built-in type alias",
            "Built-in type alias '{0}' must be replaced by '{1}'");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            AnalyzePredefinedType,
            SyntaxKind.PredefinedType);
    }

    private static void AnalyzePredefinedType(SyntaxNodeAnalysisContext context)
    {
        var node = (PredefinedTypeSyntax)context.Node;

        // Using directives are covered by GQ0004.
        if (node.IsInUsingDirective())
        {
            return;
        }

        // void has no alternative: global::System.Void is not allowed (CS0673).
        if (QualifiedName(node.Keyword.Kind()) is not { } qualified)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            Rule,
            node.GetLocation(),
            node.Keyword.Text,
            qualified);

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>Gets the globally qualified name of a built-in type alias.</summary>
    /// <param name="keyword">The keyword of the built-in type alias.</param>
    /// <returns>The globally qualified name, or null for <c>void</c>.</returns>
    internal static string? QualifiedName(SyntaxKind keyword) => keyword switch
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

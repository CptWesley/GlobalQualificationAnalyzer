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
        if (node.Keyword.Kind().QualifiedName() is not { } qualified)
        {
            return;
        }

        context.Report(Rule, node, node.Keyword.Text, qualified);
    }
}

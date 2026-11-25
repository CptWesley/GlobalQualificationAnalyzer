using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GlobalQualificationAnalyzer;

/// <summary>
/// Verifies that built-in type aliases (e.g. <c>int</c>) in using directives
/// (e.g. <c>using I = int;</c>) are replaced by their globally qualified
/// equivalent (e.g. <c>global::System.Int32</c>).
/// </summary>
/// <remarks>
/// The using directive equivalent of <see cref="BuiltInTypeAliasAnalyzer"/>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UsingBuiltInTypeAliasAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule
        = RuleHelper.Create(
            4,
            "Use globally qualified type instead of built-in type alias in using directive",
            "Built-in type alias '{0}' in using directive must be replaced by '{1}'");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            AnalyzeUsingDirective,
            SyntaxKind.UsingDirective);
    }

    private static void AnalyzeUsingDirective(SyntaxNodeAnalysisContext context)
    {
        var directive = (UsingDirectiveSyntax)context.Node;

        foreach (var node in directive.NamespaceOrType.DescendantNodesAndSelf().OfType<PredefinedTypeSyntax>())
        {
            // void has no alternative: global::System.Void is not allowed (CS0673).
            if (BuiltInTypeAliasAnalyzer.QualifiedName(node.Keyword.Kind()) is { } qualified)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    node.GetLocation(),
                    node.Keyword.Text,
                    qualified));
            }
        }
    }
}

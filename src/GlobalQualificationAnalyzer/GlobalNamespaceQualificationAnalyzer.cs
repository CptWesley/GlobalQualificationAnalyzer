using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GlobalQualificationAnalyzer;

/// <summary>
/// Verifies that namespaces and types referenced by using directives
/// (<c>using</c>, <c>using static</c> and using aliases) are globally qualified.
/// </summary>
/// <remarks>
/// The using directive equivalent of <see cref="GlobalTypeQualificationAnalyzer"/>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GlobalNamespaceQualificationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule
        = RuleHelper.Create(
            3,
            "Use globally qualified namespace or type in using directive",
            "Namespace or type '{0}' in using directive must be replaced by '{1}'");

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

        // Only inspect the outermost name, not every component of a qualified name.
        var names = directive.NamespaceOrType
            .DescendantNodesAndSelf()
            .OfType<NameSyntax>()
            .Where(name => name.Parent is not (QualifiedNameSyntax or AliasQualifiedNameSyntax));

        foreach (var name in names)
        {
            AnalyzeName(context, name);
        }
    }

    private static void AnalyzeName(SyntaxNodeAnalysisContext context, NameSyntax name)
    {
        // Contextual keywords that bind to types cannot be qualified.
        if (name.IsVar || name.IsNint || name.IsNuint || name.IsGloballyQualified())
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(name, context.CancellationToken).Symbol is not INamespaceOrTypeSymbol symbol)
        {
            return;
        }

        context.Report(Rule, name, name.ToString(), symbol.ToDisplayString(GlobalTypeQualificationAnalyzer.QualifiedFormat));
    }
}

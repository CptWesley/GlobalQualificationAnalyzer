using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GlobalQualificationAnalyzer;

/// <summary>
/// Verifies that all type references are globally qualified.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GlobalTypeQualificationAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule
        = RuleHelper.Create(
            1,
            "Use global type qualification",
            "Type reference '{0}' must use global:: qualification");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            AnalyzeName,
            SyntaxKind.IdentifierName,
            SyntaxKind.QualifiedName,
            SyntaxKind.AliasQualifiedName,
            SyntaxKind.GenericName);
    }

    private static void AnalyzeName(SyntaxNodeAnalysisContext context)
    {
        var ct = context.CancellationToken;

        // Only name syntax nodes.
        if (context.Node is not NameSyntax node)
        {
            return;
        }

        // Only inspect names that resolve to types.
        var symbol = context.SemanticModel.GetSymbolInfo(node, ct).Symbol;

        if (symbol is not ITypeSymbol)
        {
            return;
        }

        // Already globally qualified.
        if (node is AliasQualifiedNameSyntax aliasQualified &&
            aliasQualified.Alias.Identifier.ValueText == "global")
        {
            return;
        }

        // Avoid reporting every component of a qualified name.
        if (node.Parent is QualifiedNameSyntax parent &&
            parent.Left == node)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            Rule,
            node.GetLocation(),
            node.ToString());

        context.ReportDiagnostic(diagnostic);
    }
}

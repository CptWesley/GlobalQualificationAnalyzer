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
            "Type reference '{0}' must be replaced by '{1}'");

    // Built-in aliases in type arguments are spelled out, matching GQ0002.
    private static readonly SymbolDisplayFormat QualifiedFormat
        = SymbolDisplayFormat.FullyQualifiedFormat
            .RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

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

        // Only inspect the outermost name, not every component of a qualified name.
        if (node.Parent is QualifiedNameSyntax or AliasQualifiedNameSyntax)
        {
            return;
        }

        // Contextual keywords that bind to types cannot be qualified.
        if (node.IsVar || node.IsNint || node.IsNuint)
        {
            return;
        }

        // Already globally qualified.
        if (IsGloballyQualified(node))
        {
            return;
        }

        // Only inspect names that resolve to types.
        var symbol = context.SemanticModel.GetSymbolInfo(node, ct).Symbol;

        if (symbol is not ITypeSymbol type || type is IDynamicTypeSymbol)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            Rule,
            node.GetLocation(),
            node.ToString(),
            type.ToDisplayString(QualifiedFormat));

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsGloballyQualified(NameSyntax node)
    {
        while (node is QualifiedNameSyntax qualified)
        {
            node = qualified.Left;
        }

        return node is AliasQualifiedNameSyntax aliasQualified
            && aliasQualified.Alias.Identifier.IsKind(SyntaxKind.GlobalKeyword);
    }
}

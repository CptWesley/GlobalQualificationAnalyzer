using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GlobalQualificationAnalyzer;

/// <summary>
/// Provides helpers shared by the analyzers.
/// </summary>
internal static class RuleHelper
{
#pragma warning disable S1075 // URIs should not be hardcoded
    private const string UrlBase = "https://github.com/CptWesley/GlobalQualificationAnalyzer/blob/main/docs/rules";
#pragma warning restore S1075 // URIs should not be hardcoded

    /// <summary>Key of the diagnostic property that holds the globally qualified replacement.</summary>
    public const string ReplacementKey = "Replacement";

    /// <summary>
    /// Reports that <paramref name="node"/> must be replaced by <paramref name="replacement"/>.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="rule">The violated rule; its message takes the original and the replacement.</param>
    /// <param name="node">The node to replace.</param>
    /// <param name="original">The original text, shown in the message.</param>
    /// <param name="replacement">The replacement, shown in the message and applied by the code fix.</param>
    public static void Report(
        this SyntaxNodeAnalysisContext context,
        DiagnosticDescriptor rule,
        SyntaxNode node,
        string original,
        string replacement)
        => context.ReportDiagnostic(Diagnostic.Create(
            rule,
            node.GetLocation(),
            ImmutableDictionary<string, string?>.Empty.Add(ReplacementKey, replacement),
            original,
            replacement));

    /// <summary>
    /// Creates a new rule descriptor.
    /// </summary>
    /// <param name="id">The rule number, formatted as <c>GQ0000</c>.</param>
    /// <param name="title">The rule title.</param>
    /// <param name="message">The message format; defaults to <paramref name="title"/>.</param>
    /// <param name="description">The optional rule description.</param>
    /// <param name="category">The rule category.</param>
    /// <param name="severity">The default severity.</param>
    /// <param name="isEnabledByDefault">Whether the rule is enabled by default.</param>
    /// <returns>The created rule descriptor.</returns>
    public static DiagnosticDescriptor Create(
        int id,
        string title,
        string? message = null,
        string? description = null,
        string category = "Naming",
        DiagnosticSeverity severity = DiagnosticSeverity.Warning,
        bool isEnabledByDefault = true)
    {
        var fullId = $"GQ{id.ToString("D4", CultureInfo.InvariantCulture)}";

        return new(
            id: fullId,
            title: title,
            messageFormat: message ?? title,
            category: category,
            defaultSeverity: severity,
            isEnabledByDefault: isEnabledByDefault,
            description: description,
            helpLinkUri: $"{UrlBase}/{fullId}.md");
    }

    /// <summary>
    /// Returns true if <paramref name="node"/> is part of a using directive.
    /// </summary>
    /// <param name="node">The node to inspect.</param>
    /// <returns>True if the node is part of a using directive.</returns>
    /// <remarks>
    /// The search stops at the first member declaration, so names in code do not walk up to the compilation unit.
    /// </remarks>
    public static bool IsInUsingDirective(this SyntaxNode node)
        => node.FirstAncestorOrSelf<SyntaxNode>(n => n is UsingDirectiveSyntax or MemberDeclarationSyntax) is UsingDirectiveSyntax;
}

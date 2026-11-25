using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GlobalQualificationAnalyzer;

/// <summary>
/// Provides helpers shared by the analyzers.
/// </summary>
internal static class RuleHelper
{
    /// <summary>
    /// Creates a new rule descriptor.
    /// </summary>
    public static DiagnosticDescriptor Create(
        int id,
        string title,
        string? message = null,
        string? description = null,
        string category = "Naming",
        DiagnosticSeverity severity = DiagnosticSeverity.Warning,
        bool isEnabledByDefault = true)
        => new(
            id: $"GQ{id.ToString("D4", CultureInfo.InvariantCulture)}",
            title: title,
            messageFormat: message ?? title,
            category: category,
            defaultSeverity: severity,
            isEnabledByDefault: isEnabledByDefault,
            description: description);

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

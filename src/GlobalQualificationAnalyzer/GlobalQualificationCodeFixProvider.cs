using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace GlobalQualificationAnalyzer;

/// <summary>
/// Replaces the node reported by GQ0001 to GQ0004 with its globally qualified equivalent.
/// </summary>
/// <remarks>
/// The analyzers compute the replacement and store it in the diagnostic properties,
/// so the fix is a plain text replacement of the diagnostic span.
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class GlobalQualificationCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ["GQ0001", "GQ0002", "GQ0003", "GQ0004"];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => FixAllProvider.Create(FixAllAsync);

    /// <inheritdoc />
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            if (Replacement(diagnostic) is { } replacement)
            {
                // The equivalence key is shared by all diagnostics of a rule, so fix-all applies to all of them.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        $"Replace with '{replacement}'",
                        ct => ReplaceAsync(context.Document, [diagnostic], ct),
                        equivalenceKey: diagnostic.Id),
                    diagnostic);
            }
        }

        return Task.CompletedTask;
    }

    private static async Task<Document?> FixAllAsync(FixAllContext context, Document document, ImmutableArray<Diagnostic> diagnostics)
        => await ReplaceAsync(document, diagnostics, context.CancellationToken).ConfigureAwait(false);

    private static async Task<Document> ReplaceAsync(Document document, IReadOnlyCollection<Diagnostic> diagnostics, CancellationToken ct)
    {
        var replacements = diagnostics
            .Select(d => (d.Location.SourceSpan, Replacement: Replacement(d)))
            .Where(r => r.Replacement is not null)
            .Distinct()
            .ToArray();

        // A nested diagnostic (e.g. the type argument of a reported generic type) is already
        // covered by the replacement of the outer one; applying both would overlap.
        // ponytail: O(n²) per document, sort by span if files with thousands of diagnostics get slow.
        var changes = replacements
            .Where(r => !replacements.Any(outer => outer.SourceSpan != r.SourceSpan && outer.SourceSpan.Contains(r.SourceSpan)))
            .Select(r => new TextChange(r.SourceSpan, r.Replacement!));

        var text = await document.GetTextAsync(ct).ConfigureAwait(false);
        return document.WithText(text.WithChanges(changes));
    }

    private static string? Replacement(Diagnostic diagnostic)
        => diagnostic.Properties.TryGetValue(RuleHelper.ReplacementKey, out var replacement) ? replacement : null;
}

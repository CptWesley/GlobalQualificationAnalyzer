namespace GlobalQualificationAnalyzer;

/// <summary>
/// Provides helpers for generating rule descriptions.
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
}

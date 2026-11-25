using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GlobalQualificationAnalyzer;

/// <summary>
/// Verifies that all type references are globally qualified.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GlobalTypeQualificationAnalyzer : DiagnosticAnalyzer
{
    private const string AttributeSuffix = "Attribute";

    private static readonly DiagnosticDescriptor Rule
        = RuleHelper.Create(
            1,
            "Use globally qualified type",
            "Type reference '{0}' must be replaced by '{1}'");

    // Built-in aliases in type arguments are spelled out, matching GQ0002.
    // Nullable reference type arguments keep their '?' so the replacement does not change nullability.
    private static readonly SymbolDisplayFormat QualifiedFormat
        = SymbolDisplayFormat.FullyQualifiedFormat
            .RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes)
            .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

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
            SyntaxKind.GenericName,
            SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeName(SyntaxNodeAnalysisContext context)
    {
        var ct = context.CancellationToken;

        var node = (ExpressionSyntax)context.Node;

        // Only inspect the outermost name, not every component of a qualified name.
        if (node.Parent is QualifiedNameSyntax or AliasQualifiedNameSyntax)
        {
            return;
        }

        // In 'System.Console.WriteLine()' only the outermost access that resolves to a type is inspected.
        if (node.Parent is MemberAccessExpressionSyntax parent
            && (parent.Name == node || context.SemanticModel.GetSymbolInfo(parent, ct).Symbol is ITypeSymbol))
        {
            return;
        }

        // Contextual keywords that bind to types cannot be qualified.
        if (node is NameSyntax name && (name.IsVar || name.IsNint || name.IsNuint))
        {
            return;
        }

        // Already globally qualified.
        if (IsGloballyQualified(node))
        {
            return;
        }

        // Only inspect names that resolve to types; type parameters cannot be qualified.
        var symbol = context.SemanticModel.GetSymbolInfo(node, ct).Symbol;

        // An attribute name binds to the attribute constructor.
        if (node.Parent is AttributeSyntax && symbol is IMethodSymbol constructor)
        {
            symbol = constructor.ContainingType;
        }

        if (symbol is not ITypeSymbol type || type is IDynamicTypeSymbol or ITypeParameterSymbol)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            Rule,
            node.GetLocation(),
            node.ToString(),
            QualifiedName(node, type));

        context.ReportDiagnostic(diagnostic);
    }

    private static string QualifiedName(ExpressionSyntax node, ITypeSymbol type)
    {
        var qualified = type.ToDisplayString(QualifiedFormat);

        // Keep the 'Attribute' suffix omitted when the source omits it: '[Obsolete]' becomes '[global::System.Obsolete]'.
        return node.Parent is AttributeSyntax
            && qualified.EndsWith(AttributeSuffix, StringComparison.Ordinal)
            && !node.ToString().EndsWith(AttributeSuffix, StringComparison.Ordinal)
            ? qualified.Substring(0, qualified.Length - AttributeSuffix.Length)
            : qualified;
    }

    private static bool IsGloballyQualified(ExpressionSyntax node) => node switch
    {
        QualifiedNameSyntax qualified => IsGloballyQualified(qualified.Left),
        MemberAccessExpressionSyntax access => IsGloballyQualified(access.Expression),
        AliasQualifiedNameSyntax aliasQualified => aliasQualified.Alias.Identifier.IsKind(SyntaxKind.GlobalKeyword),
        _ => false,
    };
}

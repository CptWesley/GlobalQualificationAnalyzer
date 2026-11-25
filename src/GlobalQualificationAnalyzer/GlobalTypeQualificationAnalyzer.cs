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

    /// <summary>Format of the globally qualified replacement, shared with GQ0003.</summary>
    /// <remarks>
    /// Built-in aliases in type arguments are spelled out, matching GQ0002.
    /// Nullable reference type arguments keep their '?' so the replacement does not change nullability.
    /// </remarks>
    internal static readonly SymbolDisplayFormat QualifiedFormat
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

        // Only inspect names that resolve to types; type parameters cannot be qualified.
        var symbol = context.SemanticModel.GetSymbolInfo(node, ct).Symbol;

        // An attribute name binds to the attribute constructor.
        if (node.Parent is AttributeSyntax && symbol is IMethodSymbol constructor)
        {
            symbol = constructor.ContainingType;
        }

        // Using directives are covered by GQ0003.
        if (symbol is not ITypeSymbol type || type is IDynamicTypeSymbol or ITypeParameterSymbol || node.IsInUsingDirective())
        {
            return;
        }

        // Already globally qualified, and attributes spelled with their full type name.
        if (node.IsGloballyQualified() && !OmitsAttributeSuffix(node, type))
        {
            return;
        }

        context.Report(Rule, node, node.ToString(), type.ToDisplayString(QualifiedFormat));
    }

    /// <summary>'[global::System.Obsolete]' omits the suffix of 'ObsoleteAttribute'.</summary>
    private static bool OmitsAttributeSuffix(ExpressionSyntax node, ITypeSymbol type)
        => node.Parent is AttributeSyntax
            && type.Name.EndsWith(AttributeSuffix, StringComparison.Ordinal)
            && !RightmostName(node).Identifier.ValueText.EndsWith(AttributeSuffix, StringComparison.Ordinal);

    private static SimpleNameSyntax RightmostName(ExpressionSyntax node) => node switch
    {
        QualifiedNameSyntax qualified => qualified.Right,
        AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name,
        _ => (SimpleNameSyntax)node,
    };
}

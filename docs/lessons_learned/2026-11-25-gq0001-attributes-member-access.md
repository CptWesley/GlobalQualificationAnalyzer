# GQ0001: attributes and static member access

## Task

Verify GQ0001 covers `[Obsolete]` -> `[global::System.Obsolete]` and `Console.WriteLine()` -> `global::System.Console.WriteLine()`.

## Findings

Tests showed three gaps:

1. **Attributes not reported.** `SemanticModel.GetSymbolInfo` on an attribute name returns the attribute *constructor* (`IMethodSymbol`), not the type, so the `ITypeSymbol` check skipped it. Fix: when `node.Parent is AttributeSyntax`, use `constructor.ContainingType`.
2. **`System.Console.WriteLine()` reported only `Console`.** In expression context a dotted type is a `MemberAccessExpressionSyntax`, not a `QualifiedNameSyntax`, so the "outermost name only" logic did not apply. Fix: also register `SyntaxKind.SimpleMemberAccessExpression`; skip a node when it is the `.Name` of its parent member access, or when the parent member access itself resolves to a type (so `Outer.Inner` reports once).
3. **False positive on `global::System.Console.WriteLine()`.** `IsGloballyQualified` only walked `QualifiedNameSyntax.Left`. Fix: also walk `MemberAccessExpressionSyntax.Expression`.

Plain `Console.WriteLine()` was already reported correctly.

## Attribute suffix preferred

The full type name is preferred: `[Obsolete]` and `[global::System.Obsolete]` both report, suggesting `global::System.ObsoleteAttribute`. Attribute types without the suffix in their name (`class Marker : Attribute`) are fine as `[global::Marker]`. Compare the suffix on `type.Name` and the rightmost `SimpleNameSyntax` identifier, not on `ToString()`, so generic attributes (`[global::My<int>]`) work. The globally-qualified check therefore runs after symbol resolution.

## Analyzer warnings hit

- SA1203: `const` must come before `static readonly` fields.
- S3776: `AnalyzeName` crossed cognitive complexity 15; suffix logic lives in `OmitsAttributeSuffix(node, type)`.

# GQ0001: attributes and static member access

## Task

Verify GQ0001 covers `[Obsolete]` -> `[global::System.Obsolete]` and `Console.WriteLine()` -> `global::System.Console.WriteLine()`.

## Findings

Tests showed three gaps:

1. **Attributes not reported.** `SemanticModel.GetSymbolInfo` on an attribute name returns the attribute *constructor* (`IMethodSymbol`), not the type, so the `ITypeSymbol` check skipped it. Fix: when `node.Parent is AttributeSyntax`, use `constructor.ContainingType`.
2. **`System.Console.WriteLine()` reported only `Console`.** In expression context a dotted type is a `MemberAccessExpressionSyntax`, not a `QualifiedNameSyntax`, so the "outermost name only" logic did not apply. Fix: also register `SyntaxKind.SimpleMemberAccessExpression`; skip a node when it is the `.Name` of its parent member access, or when the parent member access itself resolves to a type (so `Outer.Inner` reports once).
3. **False positive on `global::System.Console.WriteLine()`.** `IsGloballyQualified` only walked `QualifiedNameSyntax.Left`. Fix: also walk `MemberAccessExpressionSyntax.Expression`.

Plain `Console.WriteLine()` was already reported correctly.

## Message detail

`ToDisplayString` gives `global::System.ObsoleteAttribute`. When the source omits the `Attribute` suffix, the expected name drops it too, so `[Obsolete]` suggests `global::System.Obsolete`.

## Analyzer warnings hit

- SA1203: `const` must come before `static readonly` fields.
- S3776: `AnalyzeName` crossed cognitive complexity 15; suffix logic moved to `QualifiedName(node, type)`.

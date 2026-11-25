# GQ0002: built-in type alias analyzer

## What was built

- `BuiltInTypeAliasAnalyzer` (GQ0002) reports every `PredefinedTypeSyntax` (`int`, `string`, ...) and names its `global::System.X` replacement.
- Syntax-only: a keyword-to-name switch, no semantic model needed.
- Shared test helper moved to `GlobalQualificationAnalyzer.Tests/AnalyzerRunner.cs` (takes the analyzer as a parameter; compilation allows unsafe code).

## Pitfalls

- `void` must be skipped: `global::System.Void` is not allowed in C# (CS0673).
- `dynamic`, `nint`, `nuint` and `var` are not `PredefinedTypeSyntax` (they are `IdentifierName`), so they never reach GQ0002.
- `enum E : global::System.Int16`, `sizeof(global::System.Int64)` and `const global::System.Int32` compile, so no exemptions are needed there; the `Replacements_compile` test guards this.
- Analyzer diagnostics are not returned in source order. Tests that expect several diagnostics must use `BeEquivalentTo`, not `Equal`.

# Analyzer test project

## Setup

- `src/GlobalQualificationAnalyzer.Tests` imports `../tests.props` (NUnit, AwesomeAssertions, net8.0–net11.0) and is added to the `.slnx`.
- No `Microsoft.CodeAnalysis.*.Testing` package: tests build a `CSharpCompilation` against `TRUSTED_PLATFORM_ASSEMBLIES` and call `WithAnalyzers(...).GetAnalyzerDiagnosticsAsync()`.
- The test project needs its own `PackageReference` to `Microsoft.CodeAnalysis.CSharp`: the analyzer project references it with `PrivateAssets="all"`, so it does not flow through the `ProjectReference`.
- `WithAnalyzers` is an extension in `Microsoft.CodeAnalysis.Diagnostics` (CS1061 without that using).
- The helper asserts the test source compiles without errors, otherwise a typo silently yields zero diagnostics and "ignores X" tests pass for the wrong reason.

## Analyzer bugs found by the tests (fixed)

- `global::System.String` reported: outer `QualifiedName` is not `AliasQualifiedName`, only its leftmost part is.
- `global::C` reported: the `C` `IdentifierName` inside the alias-qualified name is checked separately.
- `System.String` reported twice: the right-hand `String` of a `QualifiedName` is checked too.
- `var` reported: it binds to the inferred type symbol.

Fix in `GlobalTypeQualificationAnalyzer.AnalyzeName`: skip any name whose parent is a `QualifiedName` or `AliasQualifiedName` (report only the outermost name; type arguments still get checked because their parent is a `TypeArgumentList`), check `global::` on the leftmost segment, and skip `IsVar`/`IsNint`/`IsNuint` and `IDynamicTypeSymbol`. Syntax checks run before `GetSymbolInfo` to skip the semantic model where possible.

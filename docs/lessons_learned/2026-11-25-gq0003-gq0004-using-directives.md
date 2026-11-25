# GQ0003 / GQ0004: using directives

## What was built

- `GlobalNamespaceQualificationAnalyzer` (GQ0003): using directive equivalent of GQ0001. Reports names in `using`, `global using`, `using static` and using aliases (namespace and C# 12 any-type aliases) that are not `global::` qualified. Replacement from `INamespaceOrTypeSymbol.ToDisplayString(GlobalTypeQualificationAnalyzer.QualifiedFormat)`.
- `UsingBuiltInTypeAliasAnalyzer` (GQ0004): using directive equivalent of GQ0002. Reports `int`, `string`, ... inside using directives (`using I = int;`, `using P = (string, int[]);`, `using static global::System.Collections.Generic.List<byte>;`). Reuses `BuiltInTypeAliasAnalyzer.QualifiedName`.
- Both register `SyntaxKind.UsingDirective` and walk `directive.NamespaceOrType.DescendantNodesAndSelf()`, so the alias name (`X` in `using X = ...`) is never visited.

## Ownership split

GQ0001 already reported types in `using static System.Math;` and alias targets, GQ0002 reported `using I = int;`. To avoid duplicate diagnostics, GQ0001/GQ0002 now skip nodes inside using directives (`RuleHelper.IsInUsingDirective`). That helper stops its ancestor walk at the first `MemberDeclarationSyntax`, so names in code do not walk up to the compilation unit.

## Pitfalls

- S3776 (cognitive complexity) on `GlobalTypeQualificationAnalyzer.AnalyzeName`: a new `if` or a new `||` sequence pushes it to 16. The using-directive check was appended to the existing `||` chain on the type-symbol check (same-operator sequences count once).
- StyleCop wants `<param>`/`<returns>` on members made `internal`, and SA1514 rejects `//` comments directly above a `///` header: move them into `<remarks>`.
- `RuleHelper.Create` still has pre-existing SA1611/SA1615 warnings.

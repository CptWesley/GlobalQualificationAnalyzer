# Code fixes for GQ0001–GQ0004

## Design

- One provider, `GlobalQualificationCodeFixProvider`, fixes all four rules.
- The analyzers already compute the replacement for the message. `RuleHelper.Report` now also stores it in the diagnostic properties under `RuleHelper.ReplacementKey`, so the fix just replaces the text of the diagnostic span. There is no second semantic lookup and no syntax rebuilding, and trivia stays intact because the span excludes it.
- The equivalence key is `diagnostic.Id`, not the title. The title holds the replacement, which differs for each diagnostic, and fix-all only groups actions that share a key.

## Fix-all and nested diagnostics

- GQ0001 and GQ0003 report a generic type and also its unqualified type arguments, for example `Dictionary<string, List<object?>?>` and `List<object?>`. The outer replacement already qualifies the inner one.
- `WellKnownFixAllProviders.BatchFixer` would drop one of the two overlapping changes, and it is not obvious which. `SourceText.WithChanges` throws on overlapping changes.
- Fix: `FixAllProvider.Create(...)` with a custom delegate. It skips every diagnostic whose span lies inside another diagnostic's span, then applies all the remaining changes at once. The tests `*_fix_all_handles_nested_diagnostics` fail without that filter (checked by mutating it).

## RS1038

- `CodeFixProvider` needs `Microsoft.CodeAnalysis.Workspaces.Common`. With `EnforceExtendedAnalyzerRules`, referencing it from the analyzer assembly triggers RS1038 on every analyzer.
- Suppressed in `src/.globalconfig`, because the analyzer types never touch Workspaces types and csc does not load the code fix type. If an analyzer ever uses a Workspaces API, move the code fixes into a separate assembly.

## Tests

- The test project references `Microsoft.CodeAnalysis.CSharp.Workspaces` in place of `Microsoft.CodeAnalysis.CSharp`; it brings the C# compiler along transitively. There is still no `Microsoft.CodeAnalysis.Testing` package.
- `CodeFixRunner` builds an `AdhocWorkspace` project with `filePath` set on each document, so diagnostics map back to documents by `SourceTree.FilePath`.
- Fix-all is driven through a public `FixAllContext` constructor with a small `FixAllContext.DiagnosticProvider` subclass that returns the precomputed analyzer diagnostics.
- `FixAllInDocument` re-runs `AnalyzerRunner.Analyze` on the result, which asserts that the result compiles and reports nothing.

## Known limitation

- GQ0002 on a cast of a negative operand, `(byte)-1`, produces `(global::System.Byte)-1`. That parses as a subtraction and does not compile. The analyzer message suggests the same replacement, so the fix is no worse than the message. It is not handled yet.

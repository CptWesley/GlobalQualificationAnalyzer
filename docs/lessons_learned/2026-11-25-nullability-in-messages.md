# Nullability in analyzer messages

## Task

Make GQ0001 and GQ0002 handle nullable types correctly, including the replacement named in the message, and cover it with tests.

## Findings

- `SymbolDisplayFormat.FullyQualifiedFormat` has no `IncludeNullableReferenceTypeModifier`, so GQ0001 suggested
  `List<global::System.String>` for `List<string?>`, silently dropping the annotation. Added the option to `QualifiedFormat`.
- The top-level `?` is never part of the reported node: `String?` is a `NullableTypeSyntax` around `IdentifierName`, and the
  symbol of the inner name is not annotated. The message names `String` → `global::System.String`; the `?` stays in place.
- Nullable value types (`int?`, `List<int?>`) were already fine: display prints `global::System.Int32?`.
- In a `#nullable disable` context no `?` is printed, as expected.
- GQ0001 reported type parameters (`T` → `T`). Excluded `ITypeParameterSymbol`.
- Nested generic type arguments are reported separately (each `GenericName` is an outermost name), so a test with
  `Dictionary<string, List<object?>?>` expects two diagnostics; use `BeEquivalentTo`.
- GQ0002 is syntax-only (`PredefinedTypeSyntax`), so `string?` reports `string`; no change was needed, only tests.

## Pitfalls

- Test sources enable nullability with a `#nullable enable\n` prefix; `AnalyzerRunner` keeps the default (disabled) context.
- Bash heredocs and `perl -e` in this Git Bash environment mangled `\n` escapes and wrote CRLF into LF files. Write helper
  scripts to the scratchpad with the Write tool, run them with `python -I`, and check `git ls-files --eol` afterwards.

# Test coverage for rule documentation

Compared every case named in `docs/rules/GQ000*.md` (explanation, rejected, accepted, not reported) with the analyzer tests and added the missing ones.

## Added

- GQ0001: type in property, parameter, local, return type, base type, cast and `new` (one `TestCase` each); type argument of a namespace-qualified (not global) generic type.
- GQ0002: alias in `sizeof` as a reported case (was only in the compile check).
- GQ0003: contextual keywords `var`, `nint`, `nuint` ignored; ignore test now holds every line of the doc's accepted sample.
- GQ0004: all 15 aliases of the table (was 6).

## Issues

- The GQ0001 doc claimed `System.Collections.Generic.List<String>` gives one diagnostic. The analyzer reports type arguments always (their parent is a `TypeArgumentList`, not a qualified name), so it gives two. Fixed the doc, not the code: GQ0003 already documents the same behavior, and fix-all handles the nested diagnostic.
- `using V = var;` does not compile without a type named `var`; the test declares `class var { }` (only warning CS8981) because `AnalyzerRunner` fails on compile errors.

## Notes for next session

- When checking docs against tests, write the test expecting what the code does and run it; a mismatch with the doc is a doc bug or an analyzer bug to decide on, not something to paper over.

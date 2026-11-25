# Lessons Learned Previous Sessions

Index of past-session lessons, referenced from the **Lessons Learned Previous Sessions** section of [../../AGENTS.md](../../AGENTS.md). One line per entry — check here for a similar prior task before starting new work, then open the linked file for the full write-up.

## File Format

File name: `<year>-<month>-<day>-<name>.md`

## Index Format

Format: `- [<file-name>](<file-name>): <short-1-sentence-description>`

## Index

Verify any file/type/command still exists before relying on an entry.
- [2026-11-25-dotnetprojectfile-net-csproj.md](2026-11-25-dotnetprojectfile-net-csproj.md): Setting up DotNetProjectFile via `src/.net.csproj`, fixing the broken `analyzer.props` import and the ambiguous `dotnet build`.
- [2026-11-25-analyzer-test-project.md](2026-11-25-analyzer-test-project.md): Test project driving the analyzer through `CSharpCompilation.WithAnalyzers`, plus the analyzer bugs those tests exposed and how they were fixed.
- [2026-11-25-gq0002-builtin-type-alias.md](2026-11-25-gq0002-builtin-type-alias.md): GQ0002 analyzer for built-in type aliases, the `void` exemption and unordered analyzer diagnostics in tests.
- [2026-11-25-gq0001-expected-type-in-message.md](2026-11-25-gq0001-expected-type-in-message.md): GQ0001 message now names the fully qualified replacement; drop `UseSpecialTypes` from `FullyQualifiedFormat`.
- [2026-11-25-nullability-in-messages.md](2026-11-25-nullability-in-messages.md): GQ0001 kept `?` on nullable reference type arguments and skips type parameters; nullability tests for both analyzers.
- [2026-11-25-gq0001-attributes-member-access.md](2026-11-25-gq0001-attributes-member-access.md): GQ0001 now reports attribute names (bound to ctor, `Attribute` suffix preferred) and dotted static member access (`MemberAccessExpression`, not `QualifiedName`).
- [2026-11-25-gq0003-gq0004-using-directives.md](2026-11-25-gq0003-gq0004-using-directives.md): GQ0003/GQ0004 analyzers for names and built-in aliases in using directives; GQ0001/GQ0002 now skip using directives.
- [2026-11-25-fix-build-warnings.md](2026-11-25-fix-build-warnings.md): Moved diagnostic severities from `.editorconfig` to `.globalconfig`, pruned unused packages and fixed the other DotNetProjectFile/StyleCop warnings; CRLF pitfall with Git Bash `sed -i`.
- [2026-11-25-code-fixes.md](2026-11-25-code-fixes.md): One text-replacement code fix for GQ0001–GQ0004 via a replacement in diagnostic properties; custom fix-all skips nested diagnostics; RS1038 suppressed.
- [2026-11-25-rule-docs.md](2026-11-25-rule-docs.md): Rule documentation in `docs/rules/` (one file per GQ rule plus `INDEX.md`), samples taken from the analyzer tests.
- [2026-11-25-doc-test-coverage.md](2026-11-25-doc-test-coverage.md): Tests added for every rule-doc case not yet covered; GQ0001 doc corrected on type arguments of qualified names.
- [2026-11-25-readme.md](2026-11-25-readme.md): Root README with rule table, NuGet badges, install and configuration; keep its rule table in sync with `docs/rules/INDEX.md`.

# Fix build warnings

Took the solution from ~107 build warnings to 1.

## What was fixed

- **Proj4052** (~95 warnings): every `dotnet_diagnostic.*.severity` entry moved from `src/.editorconfig` to `src/.globalconfig`. `.editorconfig` now holds only formatting settings. The SDK finds `.globalconfig` itself (no project reference needed); the build showed no new StyleCop/Sonar warnings afterwards, which confirms the severities still apply.
- **Proj1009**: dropped `S2583`, which no longer exists in Sonar.
- **Proj0810**: removed unused `PackageVersion` entries (Farkle, NetAnalyzers, Parlot*, ParsecSharp, Pidgin, Verify*).
- **Proj1101**: BenchmarkDotNet `0.16.0-preview.2` → `0.15.8` (latest stable). It is used only by `benchmark.props`, and no project imports that file yet, so no lock file changed.
- **Proj1702**: removed the `<?xml ...?>` declaration from `NuGet.config`.
- **Proj3000**: removed the UTF-8 BOM from `GlobalQualificationAnalyzer.csproj`.
- **SA1611/SA1615**: added `<param>`/`<returns>` docs to `RuleHelper.Create`.

## Left alone

- **Proj0022**: `common.props` packs `../../logo_128x128.png`, which does not exist. Fixing it means adding a logo or dropping `PackageIcon`, and that is the owner's call.

## Pitfalls

- Git Bash `sed -i` on Windows rewrote CRLF files as LF. After editing with sed, normalize with `perl -pi -e 's/\r?\n/\r\n/' <file>` (the repo uses `end_of_line = crlf`).
- Build output puts the project path in trailing `[...]`. To dedupe warnings, strip it with `sed -e 's# \[[^]]*\]$##'`.

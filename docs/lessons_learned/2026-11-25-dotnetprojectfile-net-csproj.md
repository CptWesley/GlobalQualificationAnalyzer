# DotNetProjectFile `.net.csproj` setup

## Issues and fixes

- **Restore failed with `MSB4181: The "RestoreTask" task returned false but did not log an error`.** Pre-existing: `GlobalQualificationAnalyzer.csproj` imported `analyzer.props` relative to its own folder. Running `dotnet restore <csproj>` directly (not the solution) surfaced the real error `MSB4019 ... analyzer.props was not found`. Fix: `<Import Project="../analyzer.props" />`. Lesson: when a solution restore fails silently, restore the single project to get the real error.
- **`MSB1011: Specify which project or solution file to use`.** `src/` now holds both `.net.csproj` and `GlobalQualificationAnalyzer.slnx`, so bare `dotnet build` is ambiguous. Always pass `GlobalQualificationAnalyzer.slnx`.
- **`Proj0006: Add 'common.props' to the additional files`.** Appears when DotNetProjectFile is referenced per project but the imported props are not `AdditionalFiles`. With `.net.csproj` analyzing the whole `src/` tree, the per-project reference (and the manual `AdditionalFiles` links in `common.props`) were redundant and also duplicated warnings per target framework. Removed both; diagnostics coverage was identical apart from those duplicates.
- `.net.csproj` sits under `src/` (not repo root) so it picks up `src/Directory.Packages.props` (central version) and `src/nuget.config`.

## Open findings now reported (not fixed)

Proj4052 (85x, move `.editorconfig` severities to `.globalconfig`), Proj0810 (unused `PackageVersion`s), Proj1009 (S2583 gone), Proj1101 (BenchmarkDotNet preview), Proj0022 (missing `logo_128x128.png`), Proj0045 (`nuget.config` → `NuGet.config`), Proj1702, Proj3000 (BOM in csproj).

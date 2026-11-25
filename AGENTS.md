# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

GlobalQualificationAnalyzer is a Roslyn analyzer and code fix library for C# projects that enforces globally qualifying all namespaces and types.

## Communication Style

Respond terse like smart caveman. All technical substance stay. Only fluff die.

Rules:
- Drop: articles (a/an/the), filler (just/really/basically), pleasantries, hedging
- Fragments OK. Short synonyms. Technical terms exact. Code unchanged.
- Pattern: [thing] [action] [reason]. [next step].
- Not: "Sure! I'd be happy to help you with that."
- Yes: "Bug in auth middleware. Fix:"

Switch level: /caveman lite|full|ultra|wenyan
Stop: "stop caveman" or "normal mode"

Auto-Clarity: drop caveman for security warnings, irreversible actions, user confused. Resume after.

Boundaries: code/commits/PRs written normal.

## Code Navigation

Activate the current dir as project using Serena.

Prefer Roslynaut, LSP or Serena over bash commands, grep and glob when navigating or editing the C# code base. Roslynaut sees the code as the compiler does: use `document_symbols`, `symbol_source`, `find_symbols`, `workspace_symbols`, `references` and `callers` instead of reading and grepping `.cs` files, and `diagnostics` instead of building after an edit. Pass projectPath `src/GlobalQualificationAnalyzer.slnx` (absolute path) and keep passing the same path so the loaded workspace is reused.

## Lessons Learned Previous Sessions

Check [docs/lessons_learned/INDEX.md](docs/lessons_learned/INDEX.md) for a similar prior task before starting new work — one line per entry, links to the full write-up.

After each session, create a "lessons learned" markdown file in `docs/lessons_learned/`. It should give a clear summary of issues you (the AI) ran into and how we solved them, so a next session doesn't run into them again. Add a reference to the file with a single, short sentence summary to `docs/lessons_learned/INDEX.md`.

## Commands

Run everything from `src/`, where the solution `GlobalQualificationAnalyzer.slnx` lives.

```sh
dotnet restore GlobalQualificationAnalyzer.slnx
dotnet build GlobalQualificationAnalyzer.slnx
dotnet test GlobalQualificationAnalyzer.slnx               # all test projects
```

- The library targets `netstandard2.0`.
- Packages are managed centrally in `src/Directory.Packages.props`, and every project has a `packages.lock.json`. After you change a dependency, restore so the lock files are regenerated. CI builds run in locked mode (`ContinuousIntegrationBuild=true`).
- Analyzers run on every build: StyleCop (packable projects only), Sonar, Qowaiv, AsyncFixer and DotNetProjectFile. Rule overrides live in `src/.editorconfig` and `src/.globalconfig`.
- DotNetProjectFile runs only from `src/.net.csproj` (an SDK project with no build output), which analyzes every project, props, `.slnx`, `.editorconfig` and `NuGet.config` under `src/`. Because `src/` holds both `.net.csproj` and the `.slnx`, pass the solution explicitly to `dotnet` commands.

## Build layout

Shared MSBuild settings are split into `src/*.props`, which each project imports instead of using `Directory.Build.props`:
- `common.props`: imported by everything. Sets LangVersion 13, nullable, `ImplicitUsings` disabled, and the analyzers.
- `package.props`: packable libraries. Sets target frameworks, version, SourceLink and package validation.
- `sourcegenerator.props`: Roslyn generator, `netstandard2.0`.
- `tests.props` and `executable.props`: NUnit with AwesomeAssertions and AssertNet (`using static AssertNet.Assertions`), plus coverlet.
- `benchmarks.props`: benchmark project settings.

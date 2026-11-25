| Package                     | Stable | Preview | Downloads |
|-----------------------------|--------|---------|-----------|
| GlobalQualificationAnalyzer | [![NuGet](https://img.shields.io/nuget/v/GlobalQualificationAnalyzer.svg)](https://www.nuget.org/packages/GlobalQualificationAnalyzer/) | [![NuGet](https://img.shields.io/nuget/vpre/GlobalQualificationAnalyzer.svg)](https://www.nuget.org/packages/GlobalQualificationAnalyzer/absoluteLatest) | [![NuGet](https://img.shields.io/nuget/dt/GlobalQualificationAnalyzer.svg)](https://www.nuget.org/packages/GlobalQualificationAnalyzer) |

[![GlobalQualificationAnalyzer Logo](logo_128x128.png)](https://github.com/CptWesley/GlobalQualificationAnalyzer)

# [GlobalQualificationAnalyzer](https://github.com/CptWesley/GlobalQualificationAnalyzer)
Roslyn analyzer and code fix for enforcing global qualification of types and namespaces in C#.

## What is it?
GlobalQualificationAnalyzer is a set of Roslyn analyzers that report every type and namespace in your C# code that is not written as `global::` followed by its full name, together with a code fix that rewrites it for you.

## Why would you want it?
A name like `String` or `System.String` depends on the `using` directives in scope and on the namespace the code lives in. Add a type or namespace called `System` closer to the code, and the same name silently binds to something else, or no longer compiles. `global::System.String` always means the same type, wherever it is written.

In your own code you control every name, so this rarely matters. It does matter for code that ends up in someone else's compilation:

- **Source generators** add their output to the consumer's project. The generator cannot know which types and namespaces that project declares. If the consumer has a class called `Task`, a namespace `MyApp.System`, or a `using` alias called `List`, generated code that writes `Task` or `System.Collections.Generic.List<T>` binds to the wrong symbol or fails to compile, in a project whose author never wrote that code and cannot fix it.
- **Source-only packages** (packages that ship `.cs` files, compiled into the consumer's project instead of referenced as a DLL) have the same problem: their code is compiled next to names they have never seen.
- **Templates** that a generator copies into its output, such as `.cs` files embedded as resources or kept in the generator project, are regular C# while you write them but generated code once they reach the consumer.

Writing everything as `global::` plus the full name rules these clashes out, which is why the Roslyn and .NET source generators emit their code that way. Doing that by hand is easy to forget, and one missed name only breaks in a consumer project with an unlucky name. This analyzer reports every name that is not globally qualified while you write the code, and fixes it for you.

The analyzer checks C# source, not the contents of string literals, so it helps when the code that will be generated exists as `.cs` files in a project: templates, source-only packages, or types a generator also compiles into itself. Code that the compiler marks as generated is not analyzed, so add the analyzer to the project that holds that source, not to the consumer of the generated output.

## Example

```csharp
// Before
using System;
using System.Collections.Generic;

class C
{
    List<int> items = new List<int>();

    void M() => Console.WriteLine(items.Count);
}
```

```csharp
// After
using global::System;
using global::System.Collections.Generic;

class C
{
    global::System.Collections.Generic.List<global::System.Int32> items = new global::System.Collections.Generic.List<global::System.Int32>();

    void M() => global::System.Console.WriteLine(items.Count);
}
```

## Rules
All rules are in category `Naming`, have default severity `Warning` and are enabled by default. Generated code is not analyzed. Every rule has a code fix that replaces the reported code with the globally qualified equivalent shown in the message, and supports *Fix all*.

| Rule | Description |
|------|-------------|
| [GQ0001](docs/rules/GQ0001.md) | Type references in code must be globally qualified (`global::System.String` instead of `String` or `System.String`). |
| [GQ0002](docs/rules/GQ0002.md) | Built-in type aliases in code must be replaced by their globally qualified type (`global::System.Int32` instead of `int`). |
| [GQ0003](docs/rules/GQ0003.md) | Namespaces and types in using directives must be globally qualified (`using global::System;` instead of `using System;`). |
| [GQ0004](docs/rules/GQ0004.md) | Built-in type aliases in using directives must be replaced by their globally qualified type (`using I = global::System.Int32;` instead of `using I = int;`). |

Each rule document explains the rule, shows the rejected and accepted cases, and lists what is deliberately not reported. See also the [rule index](docs/rules/INDEX.md).

## Installation
1. Add the package to your project. It is in preview, so include prereleases:

   ```sh
   dotnet add package GlobalQualificationAnalyzer --prerelease
   ```

   The package is a development dependency: it only contains the analyzers and adds nothing to your build output or to the dependencies of your own package.

2. To use it in every project of a solution, add the reference to a `Directory.Build.props` next to your solution instead:

   ```xml
   <Project>
     <ItemGroup>
       <PackageReference Include="GlobalQualificationAnalyzer" Version="1.0.0-preview.1" PrivateAssets="all" />
     </ItemGroup>
   </Project>
   ```

3. Build. Every name that is not globally qualified is reported as a warning, in the IDE and in `dotnet build`.

4. Fix the existing code. In the IDE, use the light bulb on a diagnostic and choose *Fix all occurrences in Solution*. From the command line, run:

   ```sh
   dotnet format analyzers --diagnostics GQ0001 GQ0002 GQ0003 GQ0004
   ```

## Configuration
Change the severity of a rule in an `.editorconfig` or `.globalconfig`. For example, to fail the build on any violation:

```ini
[*.cs]
dotnet_diagnostic.GQ0001.severity = error
dotnet_diagnostic.GQ0002.severity = error
dotnet_diagnostic.GQ0003.severity = error
dotnet_diagnostic.GQ0004.severity = error
```

Or to keep built-in type aliases such as `int` and `string`, turn off the alias rules:

```ini
[*.cs]
dotnet_diagnostic.GQ0002.severity = none
dotnet_diagnostic.GQ0004.severity = none
```

Suppress a single occurrence in code:

```csharp
#pragma warning disable GQ0001
String name;
#pragma warning restore GQ0001
```

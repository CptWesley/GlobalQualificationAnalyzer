# Rules

Index of the analyzer rules. One line per rule; open the linked file for the explanation, the rejected cases and how to rewrite them.

All rules are in category `Naming`, have default severity `Warning` and are enabled by default. Generated code is not analyzed. Every rule has a code fix that replaces the reported code with the globally qualified equivalent shown in the message; *Fix all* is supported.

## Index Format

Format: `- [<rule-id>](<rule-id>.md): <short-1-sentence-description>`

## Index

- [GQ0001](GQ0001.md): Type references in code must be globally qualified (`global::System.String` instead of `String` or `System.String`).
- [GQ0002](GQ0002.md): Built-in type aliases in code must be replaced by their globally qualified type (`global::System.Int32` instead of `int`).
- [GQ0003](GQ0003.md): Namespaces and types in using directives must be globally qualified (`using global::System;` instead of `using System;`).
- [GQ0004](GQ0004.md): Built-in type aliases in using directives must be replaced by their globally qualified type (`using I = global::System.Int32;` instead of `using I = int;`).

## Configuration

Change the severity of a rule in an `.editorconfig` or `.globalconfig`:

```ini
dotnet_diagnostic.GQ0001.severity = none
```

Or suppress a single occurrence in code:

```csharp
#pragma warning disable GQ0001
```

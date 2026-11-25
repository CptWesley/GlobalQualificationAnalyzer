using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GlobalQualificationAnalyzer.Tests;

internal static class AnalyzerRunner
{
    /// <summary>The assemblies of the running framework, referenced by every test compilation.</summary>
    public static readonly MetadataReference[] References = [.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator)
        .Select(path => MetadataReference.CreateFromFile(path))];

    /// <summary>Compiles <paramref name="source"/> and returns the analyzer's diagnostic messages.</summary>
    public static async Task<string[]> Analyze(DiagnosticAnalyzer analyzer, string source)
    {
        var compilation = CSharpCompilation.Create(
            "Test",
            [CSharpSyntaxTree.ParseText(source)],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        // Broken test source would silently produce no diagnostics.
        compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Should().BeEmpty();

        var diagnostics = await compilation
            .WithAnalyzers([analyzer])
            .GetAnalyzerDiagnosticsAsync();

        return [.. diagnostics.Select(d => d.GetMessage(CultureInfo.InvariantCulture))];
    }
}

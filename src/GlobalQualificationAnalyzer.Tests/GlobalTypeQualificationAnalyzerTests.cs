using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GlobalQualificationAnalyzer.Tests;

public sealed class GlobalTypeQualificationAnalyzerTests
{
    [Test]
    public async Task Reports_unqualified_type()
        => (await Analyze("using System; class C { String f; }"))
            .Should().Equal("Type reference 'String' must use global:: qualification");

    [Test]
    public async Task Reports_namespace_qualified_type_once()
        => (await Analyze("class C { System.String f; }"))
            .Should().Equal("Type reference 'System.String' must use global:: qualification");

    [Test]
    public async Task Reports_built_in_types()
    => (await Analyze("class C { string f; }"))
        .Should().Equal("Type reference 'string' must use global:: qualification");

    [Test]
    public async Task Reports_unqualified_generic_type()
        => (await Analyze("using System.Collections.Generic; class C { List<int> f; }"))
            .Should().Equal("Type reference 'List<int>' must use global:: qualification");

    [Test]
    public async Task Reports_unqualified_type_argument()
        => (await Analyze("using System; class C { global::System.Collections.Generic.List<String> f; }"))
            .Should().Equal("Type reference 'String' must use global:: qualification");

    [Test]
    public async Task Reports_type_in_typeof()
        => (await Analyze("using System; class C { object f = typeof(String); }"))
            .Should().Equal("Type reference 'String' must use global:: qualification");

    [Test]
    public async Task Reports_type_declared_in_same_compilation()
        => (await Analyze("namespace N { class A { } class B { A f; } }"))
            .Should().Equal("Type reference 'A' must use global:: qualification");

    [Test]
    public async Task Ignores_global_alias_on_type()
        => (await Analyze("class C { global::C f; }"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_globally_qualified_type()
        => (await Analyze("class C { global::System.String f; }"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_globally_qualified_generic_type()
        => (await Analyze("class C { global::System.Collections.Generic.List<int> f; }"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_predefined_types()
        => (await Analyze("class C { int a; string b; object c; }"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_var()
        => (await Analyze("class C { void M() { var x = 1; } }"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_namespaces_in_using_directives()
        => (await Analyze("using System; class C { }"))
            .Should().BeEmpty();

    private static async Task<string[]> Analyze(string source)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));

        var compilation = CSharpCompilation.Create(
            "Test",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Broken test source would silently produce no diagnostics.
        compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Should().BeEmpty();

        var diagnostics = await compilation
            .WithAnalyzers([new GlobalTypeQualificationAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();

        return [.. diagnostics.Select(d => d.GetMessage(CultureInfo.InvariantCulture))];
    }
}

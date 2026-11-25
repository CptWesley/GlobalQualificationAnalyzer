using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GlobalQualificationAnalyzer.Tests;

public sealed class GlobalQualificationCodeFixProviderTests
{
    [Test]
    public async Task GQ0001_fixes_unqualified_type_and_keeps_trivia()
        => (await CodeFixRunner.Fix(new GlobalTypeQualificationAnalyzer(), "using System; class C { /* a */ String /* b */ f; }"))
            .Should().Be("using System; class C { /* a */ global::System.String /* b */ f; }");

    [Test]
    public async Task GQ0001_fixes_only_selected_diagnostic()
        => (await CodeFixRunner.Fix(new GlobalTypeQualificationAnalyzer(), "using System; class C { String a; Int32 b; }"))
            .Should().Be("using System; class C { global::System.String a; Int32 b; }");

    [TestCase("using System; [Obsolete] class C { }", "using System; [global::System.ObsoleteAttribute] class C { }")]
    [TestCase("[global::System.Obsolete] class C { }", "[global::System.ObsoleteAttribute] class C { }")]
    public async Task GQ0001_fixes_attribute(string source, string expected)
        => (await FixAllInDocument(new GlobalTypeQualificationAnalyzer(), source))
            .Should().Be(expected);

    [Test]
    public async Task GQ0001_fixes_static_member_access()
        => (await FixAllInDocument(new GlobalTypeQualificationAnalyzer(), "using System; class C { void M() { Console.WriteLine(); System.Math.Abs(1); } }"))
            .Should().Be("using System; class C { void M() { global::System.Console.WriteLine(); global::System.Math.Abs(1); } }");

    /// <summary>Type arguments are reported separately, but the fix of the outer type already qualifies them.</summary>
    [Test]
    public async Task GQ0001_fix_all_handles_nested_diagnostics()
        => (await FixAllInDocument(
            new GlobalTypeQualificationAnalyzer(),
            "#nullable enable\nusing System.Collections.Generic; class C { Dictionary<string, List<object?>?> a = new(); List<int> b = new(); }"))
            .Should().Be("#nullable enable\nusing System.Collections.Generic; class C { global::System.Collections.Generic.Dictionary<global::System.String, global::System.Collections.Generic.List<global::System.Object?>?> a = new(); global::System.Collections.Generic.List<global::System.Int32> b = new(); }");

    [Test]
    public async Task GQ0002_fixes_alias()
        => (await CodeFixRunner.Fix(new BuiltInTypeAliasAnalyzer(), "class C { int f; }"))
            .Should().Be("class C { global::System.Int32 f; }");

    [Test]
    public async Task GQ0002_fix_all_replaces_every_alias()
        => (await FixAllInDocument(
            new BuiltInTypeAliasAnalyzer(),
            "enum E : short { } class C { long? a; global::System.Collections.Generic.List<int[]> b; object c = string.Empty; object d = (byte)1; object e = typeof(char); void M() { } }"))
            .Should().Be("enum E : global::System.Int16 { } class C { global::System.Int64? a; global::System.Collections.Generic.List<global::System.Int32[]> b; global::System.Object c = global::System.String.Empty; global::System.Object d = (global::System.Byte)1; global::System.Object e = typeof(global::System.Char); void M() { } }");

    [Test]
    public async Task GQ0003_fixes_namespace()
        => (await CodeFixRunner.Fix(new GlobalNamespaceQualificationAnalyzer(), "using System.Text;"))
            .Should().Be("using global::System.Text;");

    [Test]
    public async Task GQ0003_fix_all_handles_nested_diagnostics()
        => (await FixAllInDocument(
            new GlobalNamespaceQualificationAnalyzer(),
            """
            global using System;
            using static System.Math;
            using S = global::System;
            using L = System.Collections.Generic.List<System.String>;
            using P = (System.String, System.Int32[]);
            namespace N { using S.Text; }
            """))
            .Should().Be("""
            global using global::System;
            using static global::System.Math;
            using S = global::System;
            using L = global::System.Collections.Generic.List<global::System.String>;
            using P = (global::System.String, global::System.Int32[]);
            namespace N { using global::System.Text; }
            """);

    [Test]
    public async Task GQ0004_fixes_alias()
        => (await CodeFixRunner.Fix(new UsingBuiltInTypeAliasAnalyzer(), "using I = int;"))
            .Should().Be("using I = global::System.Int32;");

    [Test]
    public async Task GQ0004_fix_all_replaces_every_alias()
        => (await FixAllInDocument(
            new UsingBuiltInTypeAliasAnalyzer(),
            "using P = (string, int[], double?); using L = global::System.Collections.Generic.List<long>; using unsafe V = void*;"))
            .Should().Be("using P = (global::System.String, global::System.Int32[], global::System.Double?); using L = global::System.Collections.Generic.List<global::System.Int64>; using unsafe V = void*;");

    [TestCase(FixAllScope.Project)]
    [TestCase(FixAllScope.Solution)]
    public async Task Fix_all_in_project_or_solution_fixes_every_document(FixAllScope scope)
        => (await CodeFixRunner.FixAll(new BuiltInTypeAliasAnalyzer(), scope, "class A { int f; }", "class B { string f; }"))
            .Should().Equal("class A { global::System.Int32 f; }", "class B { global::System.String f; }");

    [Test]
    public async Task Fix_all_in_document_leaves_other_documents()
        => (await CodeFixRunner.FixAll(new BuiltInTypeAliasAnalyzer(), FixAllScope.Document, "class A { int f; bool g; }", "class B { string f; }"))
            .Should().Equal("class A { global::System.Int32 f; global::System.Boolean g; }", "class B { string f; }");

    /// <summary>Applies fix-all to a single document and verifies the result compiles and is no longer reported.</summary>
    private static async Task<string> FixAllInDocument(DiagnosticAnalyzer analyzer, string source)
    {
        var fixedSource = (await CodeFixRunner.FixAll(analyzer, FixAllScope.Document, source)).Single();
        (await AnalyzerRunner.Analyze(analyzer, fixedSource)).Should().BeEmpty();
        return fixedSource;
    }
}

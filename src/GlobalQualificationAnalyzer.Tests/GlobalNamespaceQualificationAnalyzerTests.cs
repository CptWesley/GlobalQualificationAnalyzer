namespace GlobalQualificationAnalyzer.Tests;

public sealed class GlobalNamespaceQualificationAnalyzerTests
{
    [Test]
    public async Task Reports_unqualified_namespace()
        => (await Analyze("using System;"))
            .Should().Equal("Namespace or type 'System' in using directive must be replaced by 'global::System'");

    [Test]
    public async Task Reports_qualified_namespace_once()
        => (await Analyze("using System.Text;"))
            .Should().Equal("Namespace or type 'System.Text' in using directive must be replaced by 'global::System.Text'");

    [Test]
    public async Task Reports_global_using()
        => (await Analyze("global using System;"))
            .Should().Equal("Namespace or type 'System' in using directive must be replaced by 'global::System'");

    [Test]
    public async Task Reports_using_in_namespace_declaration()
        => (await Analyze("namespace N { using System; }"))
            .Should().Equal("Namespace or type 'System' in using directive must be replaced by 'global::System'");

    [Test]
    public async Task Reports_using_static()
        => (await Analyze("using static System.Math;"))
            .Should().Equal("Namespace or type 'System.Math' in using directive must be replaced by 'global::System.Math'");

    [Test]
    public async Task Reports_namespace_alias()
        => (await Analyze("using T = System.Text;"))
            .Should().Equal("Namespace or type 'System.Text' in using directive must be replaced by 'global::System.Text'");

    [Test]
    public async Task Reports_type_alias()
        => (await Analyze("using S = System.String;"))
            .Should().Equal("Namespace or type 'System.String' in using directive must be replaced by 'global::System.String'");

    [Test]
    public async Task Reports_generic_type_alias_and_type_argument()
        => (await Analyze("using L = System.Collections.Generic.List<System.String>;"))
            .Should().BeEquivalentTo(
                "Namespace or type 'System.Collections.Generic.List<System.String>' in using directive must be replaced by 'global::System.Collections.Generic.List<global::System.String>'",
                "Namespace or type 'System.String' in using directive must be replaced by 'global::System.String'");

    [Test]
    public async Task Reports_type_argument_of_globally_qualified_type()
        => (await Analyze("using L = global::System.Collections.Generic.List<System.String>;"))
            .Should().Equal("Namespace or type 'System.String' in using directive must be replaced by 'global::System.String'");

    [Test]
    public async Task Reports_tuple_and_array_alias()
        => (await Analyze("using P = (System.String, System.Int32[]);"))
            .Should().BeEquivalentTo(
                "Namespace or type 'System.String' in using directive must be replaced by 'global::System.String'",
                "Namespace or type 'System.Int32' in using directive must be replaced by 'global::System.Int32'");

    [Test]
    public async Task Reports_name_through_outer_alias()
        => (await Analyze("using S = global::System; namespace N { using S.Text; }"))
            .Should().Equal("Namespace or type 'S.Text' in using directive must be replaced by 'global::System.Text'");

    [Test]
    public async Task Ignores_globally_qualified_using_directives()
        => (await Analyze("""
            global using global::System.IO;
            using global::System;
            using static global::System.Math;
            using T = global::System.Text;
            using L = global::System.Collections.Generic.List<global::System.String>;
            """))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_built_in_type_aliases()
        => (await Analyze("using I = int; using L = global::System.Collections.Generic.List<string>;"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_types_outside_using_directives()
        => (await Analyze("using global::System; class C { String f; }"))
            .Should().BeEmpty();

    private static Task<string[]> Analyze(string source)
        => AnalyzerRunner.Analyze(new GlobalNamespaceQualificationAnalyzer(), source);
}

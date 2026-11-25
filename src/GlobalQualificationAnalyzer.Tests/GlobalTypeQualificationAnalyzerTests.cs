namespace GlobalQualificationAnalyzer.Tests;

public sealed class GlobalTypeQualificationAnalyzerTests
{
    [Test]
    public async Task Reports_unqualified_type()
        => (await Analyze("using System; class C { String f; }"))
            .Should().Equal("Type reference 'String' must be replaced by 'global::System.String'");

    [Test]
    public async Task Reports_namespace_qualified_type_once()
        => (await Analyze("class C { System.String f; }"))
            .Should().Equal("Type reference 'System.String' must be replaced by 'global::System.String'");

    [Test]
    public async Task Reports_unqualified_generic_type()
        => (await Analyze("using System.Collections.Generic; class C { List<int> f; }"))
            .Should().Equal("Type reference 'List<int>' must be replaced by 'global::System.Collections.Generic.List<global::System.Int32>'");

    [Test]
    public async Task Reports_unqualified_type_argument()
        => (await Analyze("using System; class C { global::System.Collections.Generic.List<String> f; }"))
            .Should().Equal("Type reference 'String' must be replaced by 'global::System.String'");

    [Test]
    public async Task Reports_type_in_typeof()
        => (await Analyze("using System; class C { object f = typeof(String); }"))
            .Should().Equal("Type reference 'String' must be replaced by 'global::System.String'");

    [Test]
    public async Task Reports_type_declared_in_same_compilation()
        => (await Analyze("namespace N { class A { } class B { A f; } }"))
            .Should().Equal("Type reference 'A' must be replaced by 'global::N.A'");

    [Test]
    public async Task Reports_nullable_reference_type_without_modifier()
        => (await Analyze("#nullable enable\nusing System; class C { String? f; }"))
            .Should().Equal("Type reference 'String' must be replaced by 'global::System.String'");

    [Test]
    public async Task Reports_nullable_value_type_without_modifier()
        => (await Analyze("using System; class C { Int32? f; }"))
            .Should().Equal("Type reference 'Int32' must be replaced by 'global::System.Int32'");

    [Test]
    public async Task Reports_nullable_generic_type_without_modifier()
        => (await Analyze("#nullable enable\nusing System.Collections.Generic; class C { List<int>? f; }"))
            .Should().Equal("Type reference 'List<int>' must be replaced by 'global::System.Collections.Generic.List<global::System.Int32>'");

    [Test]
    public async Task Keeps_nullable_reference_type_argument()
        => (await Analyze("#nullable enable\nusing System.Collections.Generic; class C { List<string?> f = new(); }"))
            .Should().Equal("Type reference 'List<string?>' must be replaced by 'global::System.Collections.Generic.List<global::System.String?>'");

    [Test]
    public async Task Keeps_nullable_value_type_argument()
        => (await Analyze("using System.Collections.Generic; class C { List<int?> f; }"))
            .Should().Equal("Type reference 'List<int?>' must be replaced by 'global::System.Collections.Generic.List<global::System.Int32?>'");

    [Test]
    public async Task Keeps_nested_nullable_type_arguments()
        => (await Analyze("#nullable enable\nusing System.Collections.Generic; class C { Dictionary<string, List<object?>?> f = new(); }"))
            .Should().BeEquivalentTo(
                "Type reference 'Dictionary<string, List<object?>?>' must be replaced by 'global::System.Collections.Generic.Dictionary<global::System.String, global::System.Collections.Generic.List<global::System.Object?>?>'",
                "Type reference 'List<object?>' must be replaced by 'global::System.Collections.Generic.List<global::System.Object?>'");

    [Test]
    public async Task Omits_modifier_in_disabled_nullable_context()
        => (await Analyze("#nullable disable\nusing System.Collections.Generic; class C { List<string> f; }"))
            .Should().Equal("Type reference 'List<string>' must be replaced by 'global::System.Collections.Generic.List<global::System.String>'");

    [Test]
    public async Task Ignores_type_parameters()
        => (await Analyze("#nullable enable\nclass C<T> { T? a; T b = default!; }"))
            .Should().BeEmpty();

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
    public async Task Ignores_contextual_type_keywords()
        => (await Analyze("class C { dynamic a; nint b; nuint c; }"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_var()
        => (await Analyze("class C { void M() { var x = 1; } }"))
            .Should().BeEmpty();

    [Test]
    public async Task Reports_unqualified_attribute()
        => (await Analyze("using System; [Obsolete] class C { }"))
            .Should().Equal("Type reference 'Obsolete' must be replaced by 'global::System.ObsoleteAttribute'");

    [Test]
    public async Task Reports_unqualified_attribute_with_suffix()
        => (await Analyze("using System; [ObsoleteAttribute] class C { }"))
            .Should().Equal("Type reference 'ObsoleteAttribute' must be replaced by 'global::System.ObsoleteAttribute'");

    [Test]
    public async Task Reports_namespace_qualified_attribute()
        => (await Analyze("[System.Obsolete] class C { }"))
            .Should().Equal("Type reference 'System.Obsolete' must be replaced by 'global::System.ObsoleteAttribute'");

    [Test]
    public async Task Reports_globally_qualified_attribute_without_suffix()
        => (await Analyze("[global::System.Obsolete] class C { }"))
            .Should().Equal("Type reference 'global::System.Obsolete' must be replaced by 'global::System.ObsoleteAttribute'");

    [Test]
    public async Task Ignores_globally_qualified_attribute_with_suffix()
        => (await Analyze("[global::System.ObsoleteAttribute] class C { }"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_globally_qualified_attribute_type_without_suffix_in_name()
        => (await Analyze("class Marker : global::System.Attribute { } [global::Marker] class C { }"))
            .Should().BeEmpty();

    [Test]
    public async Task Reports_globally_qualified_generic_attribute_without_suffix()
        => (await Analyze("class MyAttribute<T> : global::System.Attribute { } [global::My<int>] class C { }"))
            .Should().Equal("Type reference 'global::My<int>' must be replaced by 'global::MyAttribute<global::System.Int32>'");

    [Test]
    public async Task Reports_unqualified_static_member_access()
        => (await Analyze("using System; class C { void M() { Console.WriteLine(); } }"))
            .Should().Equal("Type reference 'Console' must be replaced by 'global::System.Console'");

    [Test]
    public async Task Reports_namespace_qualified_static_member_access()
        => (await Analyze("class C { void M() { System.Console.WriteLine(); } }"))
            .Should().Equal("Type reference 'System.Console' must be replaced by 'global::System.Console'");

    [Test]
    public async Task Ignores_globally_qualified_static_member_access()
        => (await Analyze("class C { void M() { global::System.Console.WriteLine(); } }"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_namespaces_in_using_directives()
        => (await Analyze("using System; class C { }"))
            .Should().BeEmpty();

    /// <summary>Using directives are covered by GQ0003.</summary>
    [Test]
    public async Task Ignores_types_in_using_directives()
        => (await Analyze("using static System.Math; using S = System.String; using L = global::System.Collections.Generic.List<System.String>;"))
            .Should().BeEmpty();

    private static Task<string[]> Analyze(string source)
        => AnalyzerRunner.Analyze(new GlobalTypeQualificationAnalyzer(), source);
}

namespace GlobalQualificationAnalyzer.Tests;

public sealed class UsingBuiltInTypeAliasAnalyzerTests
{
    [TestCase("bool", "global::System.Boolean")]
    [TestCase("char", "global::System.Char")]
    [TestCase("float", "global::System.Single")]
    [TestCase("int", "global::System.Int32")]
    [TestCase("object", "global::System.Object")]
    [TestCase("string", "global::System.String")]
    public async Task Reports_alias(string alias, string qualified)
        => (await Analyze($"using A = {alias};"))
            .Should().Equal($"Built-in type alias '{alias}' in using directive must be replaced by '{qualified}'");

    [Test]
    public async Task Reports_alias_in_type_argument()
        => (await Analyze("using L = global::System.Collections.Generic.List<long>;"))
            .Should().Equal("Built-in type alias 'long' in using directive must be replaced by 'global::System.Int64'");

    [Test]
    public async Task Reports_alias_in_using_static()
        => (await Analyze("using static global::System.Collections.Generic.List<byte>;"))
            .Should().Equal("Built-in type alias 'byte' in using directive must be replaced by 'global::System.Byte'");

    [Test]
    public async Task Reports_alias_in_tuple_array_and_nullable()
        => (await Analyze("using P = (string, int[], double?);"))
            .Should().BeEquivalentTo(
                "Built-in type alias 'string' in using directive must be replaced by 'global::System.String'",
                "Built-in type alias 'int' in using directive must be replaced by 'global::System.Int32'",
                "Built-in type alias 'double' in using directive must be replaced by 'global::System.Double'");

    [Test]
    public async Task Ignores_void_pointer()
        => (await Analyze("using unsafe P = void*;"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_globally_qualified_types()
        => (await Analyze("using I = global::System.Int32; using L = global::System.Collections.Generic.List<global::System.String>;"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_aliases_outside_using_directives()
        => (await Analyze("class C { int f; }"))
            .Should().BeEmpty();

    private static Task<string[]> Analyze(string source)
        => AnalyzerRunner.Analyze(new UsingBuiltInTypeAliasAnalyzer(), source);
}

namespace GlobalQualificationAnalyzer.Tests;

public sealed class BuiltInTypeAliasAnalyzerTests
{
    [TestCase("bool", "global::System.Boolean")]
    [TestCase("byte", "global::System.Byte")]
    [TestCase("sbyte", "global::System.SByte")]
    [TestCase("char", "global::System.Char")]
    [TestCase("decimal", "global::System.Decimal")]
    [TestCase("double", "global::System.Double")]
    [TestCase("float", "global::System.Single")]
    [TestCase("int", "global::System.Int32")]
    [TestCase("uint", "global::System.UInt32")]
    [TestCase("long", "global::System.Int64")]
    [TestCase("ulong", "global::System.UInt64")]
    [TestCase("short", "global::System.Int16")]
    [TestCase("ushort", "global::System.UInt16")]
    [TestCase("object", "global::System.Object")]
    [TestCase("string", "global::System.String")]
    public async Task Reports_alias(string alias, string qualified)
        => (await Analyze($"class C {{ {alias} f; }}"))
            .Should().Equal($"Built-in type alias '{alias}' must be replaced by '{qualified}'");

    [Test]
    public async Task Reports_alias_in_type_argument_nullable_and_array()
        => (await Analyze("class C { global::System.Collections.Generic.List<int?[]> f; }"))
            .Should().Equal("Built-in type alias 'int' must be replaced by 'global::System.Int32'");

    [Test]
    public async Task Reports_alias_in_nullable_reference_types()
        => (await Analyze("#nullable enable\nclass C { string? a; global::System.Collections.Generic.List<object?>? b; }"))
            .Should().BeEquivalentTo(
                "Built-in type alias 'string' must be replaced by 'global::System.String'",
                "Built-in type alias 'object' must be replaced by 'global::System.Object'");

    [Test]
    public async Task Reports_alias_in_nullable_value_type()
        => (await Analyze("class C { long? f; }"))
            .Should().Equal("Built-in type alias 'long' must be replaced by 'global::System.Int64'");

    [Test]
    public async Task Reports_alias_in_member_access_typeof_and_cast()
        => (await Analyze("class C { object a = string.Empty; object b = typeof(long); object c = (byte)1; }"))
            .Should().BeEquivalentTo(
                "Built-in type alias 'object' must be replaced by 'global::System.Object'",
                "Built-in type alias 'string' must be replaced by 'global::System.String'",
                "Built-in type alias 'object' must be replaced by 'global::System.Object'",
                "Built-in type alias 'long' must be replaced by 'global::System.Int64'",
                "Built-in type alias 'object' must be replaced by 'global::System.Object'",
                "Built-in type alias 'byte' must be replaced by 'global::System.Byte'");

    [Test]
    public async Task Reports_alias_in_sizeof()
        => (await Analyze("class C { const global::System.Int32 X = sizeof(long); }"))
            .Should().Equal("Built-in type alias 'long' must be replaced by 'global::System.Int64'");

    [Test]
    public async Task Reports_enum_underlying_type()
        => (await Analyze("enum E : short { }"))
            .Should().Equal("Built-in type alias 'short' must be replaced by 'global::System.Int16'");

    [Test]
    public async Task Ignores_void()
        => (await Analyze("class C { void M() { } unsafe void* P() => null; }"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_globally_qualified_types()
        => (await Analyze("class C { global::System.String a; global::System.Int32 b; }"))
            .Should().BeEmpty();

    [Test]
    public async Task Ignores_contextual_type_keywords()
        => (await Analyze("class C { dynamic a; nint b; nuint c; void M() { var x = 1; } }"))
            .Should().BeEmpty();

    /// <summary>Using directives are covered by GQ0004.</summary>
    [Test]
    public async Task Ignores_alias_in_using_directives()
        => (await Analyze("using I = int; using L = global::System.Collections.Generic.List<string>;"))
            .Should().BeEmpty();

    /// <summary>Every replacement must compile, including where only aliases used to be allowed.</summary>
    [Test]
    public async Task Replacements_compile()
        => (await Analyze("""
            enum E : global::System.Int16 { }
            class C
            {
                const global::System.Int32 X = sizeof(global::System.Int64);
                global::System.Object a = global::System.String.Empty;
            }
            """))
            .Should().BeEmpty();

    private static Task<string[]> Analyze(string source)
        => AnalyzerRunner.Analyze(new BuiltInTypeAliasAnalyzer(), source);
}

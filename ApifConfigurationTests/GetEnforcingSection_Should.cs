namespace ApifConfigurationTests;

using ApifConfiguration;
using Sut = ApifConfiguration.ApifConfiguration;

public sealed class GetEnforcingSection_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [Fact]
    public void ReturnApifConfiguration_WhenSectionExists()
    {
        var config = Build(new() { ["section:key"] = "value" });

        var result = config.GetEnforcingSection("section");

        _ = result.Should().NotBeNull();
        _ = result.Should().BeOfType<Sut>();
    }

    [Fact]
    public void ReturnCorrectValue_WhenSectionExists()
    {
        var config = Build(new() { ["section:key"] = "hello" });

        var result = config.GetEnforcingSection("section");

        _ = result["key"].Should().Be("hello");
    }

    [Fact]
    public void ReturnEnforcingInstance_ThatThrowsOnMissingKey()
    {
        var config = Build(new() { ["section:key"] = "value" });

        var result = config.GetEnforcingSection("section");

        Action act = () => _ = result["missing"];
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>();
    }

    [Fact]
    public void Throw_WhenSectionIsMissing()
    {
        var config = Build([]);

        Action act = () => config.GetEnforcingSection("missing");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("missing");
    }

    [Fact]
    public void ProvideOptionalAccess_OnReturnedSection()
    {
        var config = Build(new() { ["section:key"] = "value" });

        var result = config.GetEnforcingSection("section");

        _ = result.Optional["key"].Should().Be("value");
        _ = result.Optional["missing"].Should().BeNull();
    }

    [Fact]
    public void ReturnNestedSection()
    {
        var config = Build(new() { ["a:b:c"] = "deep" });

        var sectionA = config.GetEnforcingSection("a");
        var sectionB = sectionA.GetEnforcingSection("b");

        _ = sectionB["c"].Should().Be("deep");
    }

    [Fact]
    public void ReturnCorrectKey()
    {
        var config = Build(new() { ["section:key"] = "value" });

        var result = config.GetEnforcingSection("section");

        _ = result.Key.Should().Be("section");
    }

    [Fact]
    public void ReturnCorrectPath()
    {
        var config = Build(new() { ["parent:child:key"] = "value" });

        var parent = config.GetEnforcingSection("parent");
        var child = parent.GetEnforcingSection("child");

        _ = child.Path.Should().Be("parent:child");
    }
}

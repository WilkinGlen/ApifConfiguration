namespace ApifConfigurationTests;

using ApifConfiguration;
using Sut = ApifConfiguration.ApifConfiguration;

public sealed class GetEnforcingChildren_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [Fact]
    public void ReturnApifConfigurationInstances()
    {
        var config = Build(new() { ["a"] = "1", ["b"] = "2" });

        var children = config.GetEnforcingChildren().ToList();

        _ = children.Should().AllBeOfType<Sut>();
    }

    [Fact]
    public void ReturnCorrectCount()
    {
        var config = Build(new() { ["a"] = "1", ["b"] = "2", ["c"] = "3" });

        var children = config.GetEnforcingChildren().ToList();

        _ = children.Should().HaveCount(3);
    }

    [Fact]
    public void ReturnEmpty_WhenNoChildren()
    {
        var config = Build([]);

        var children = config.GetEnforcingChildren().ToList();

        _ = children.Should().BeEmpty();
    }

    [Fact]
    public void ReturnChildrenWithCorrectKeys()
    {
        var config = Build(new() { ["first"] = "1", ["second"] = "2" });

        var keys = config.GetEnforcingChildren().Select(c => c.Key).ToList();

        _ = keys.Should().Contain("first");
        _ = keys.Should().Contain("second");
    }

    [Fact]
    public void ReturnEnforcingChildren_ThatThrowOnMissingKey()
    {
        var config = Build(new() { ["section:key"] = "value" });

        var child = config.GetEnforcingChildren().First();

        Action act = () => _ = child["missing"];
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>();
    }

    [Fact]
    public void ReturnChildren_WithOptionalAccess()
    {
        var config = Build(new() { ["section:key"] = "value" });

        var child = config.GetEnforcingChildren().First();

        _ = child.Optional["key"].Should().Be("value");
        _ = child.Optional["missing"].Should().BeNull();
    }

    [Fact]
    public void ReturnChildrenOfSection()
    {
        var config = Build(new() { ["parent:a"] = "1", ["parent:b"] = "2" });

        var parent = config.GetEnforcingSection("parent");
        var children = parent.GetEnforcingChildren().ToList();

        _ = children.Should().HaveCount(2);
        _ = children.Select(c => c.Key).Should().Contain("a");
        _ = children.Select(c => c.Key).Should().Contain("b");
    }

    [Fact]
    public void ReturnDeeplyNestedChildren()
    {
        var config = Build(new()
        {
            ["level1:level2:a"] = "deep1",
            ["level1:level2:b"] = "deep2",
        });

        var deepChildren = config
            .GetEnforcingSection("level1")
            .GetEnforcingSection("level2")
            .GetEnforcingChildren()
            .ToList();

        _ = deepChildren.Should().HaveCount(2);
        _ = deepChildren.Select(c => c.Value).Should().Contain("deep1");
        _ = deepChildren.Select(c => c.Value).Should().Contain("deep2");
    }
}

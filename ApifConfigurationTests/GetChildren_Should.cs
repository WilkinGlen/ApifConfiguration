namespace ApifConfigurationTests;

using ApifConfiguration;
using Sut = ApifConfiguration.ApifConfiguration;

public sealed class GetChildren_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [Fact]
    public void ReturnApifConfigurationInstances_WhenChildrenExist()
    {
        var config = Build(new() { ["A:Key"] = "1", ["B:Key"] = "2" });

        _ = config.GetChildren().Should().AllBeOfType<Sut>();
    }

    [Fact]
    public void ReturnCorrectCount_WhenChildrenExist()
    {
        var config = Build(new() { ["A:Key"] = "1", ["B:Key"] = "2" });

        _ = config.GetChildren().Should().HaveCount(2);
    }

    [Fact]
    public void ReturnEmptySequence_WhenNoChildrenExist()
    {
        var config = Build([]);

        _ = config.GetChildren().Should().BeEmpty();
    }

    [Fact]
    public void ReturnEnforcingChildren_ThatResolveExistingKeys()
    {
        var config = Build(new() { ["A:Key"] = "value" });

        var child = (Sut)config.GetChildren().Single();

        _ = child.Get("Key").Should().Be("value");
    }

    [Fact]
    public void ReturnEnforcingChildren_ThatThrowOnMissingKeys()
    {
        var config = Build(new() { ["A:Key"] = "value" });

        var child = (Sut)config.GetChildren().Single();

        Action act = () => child.Get("Missing");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>();
    }

    [Fact]
    public void ReturnOnlyImmediateChildren_WhenDeeplyNested()
    {
        var config = Build(new() { ["A:B:Key"] = "1", ["C:Key"] = "2" });

        _ = config.GetChildren().Should().HaveCount(2);
    }
}

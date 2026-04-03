namespace ApifConfigurationTests;

using ApifConfiguration;
using Sut = ApifConfiguration.ApifConfiguration;

public sealed class LazyOptional_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [Fact]
    public void ReturnSameInstance_OnMultipleAccesses()
    {
        var config = Build(new() { ["key"] = "value" });

        var first = config.Optional;
        var second = config.Optional;

        _ = first.Should().BeSameAs(second);
    }

    [Fact]
    public void ReturnFunctionalOptional()
    {
        var config = Build(new() { ["key"] = "value" });

        _ = config.Optional["key"].Should().Be("value");
        _ = config.Optional["missing"].Should().BeNull();
    }
}

namespace ApifConfigurationTests;

using ApifConfiguration;
using Sut = ApifConfiguration.ApifConfigurationWrapper;

public sealed class Indexer_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [Fact]
    public void ReturnValue_WhenKeyExists()
    {
        var config = Build(new() { ["key"] = "value" });

        _ = config["key"].Should().Be("value");
    }

    [Fact]
    public void Throw_WhenKeyIsMissing()
    {
        var config = Build([]);

        Action act = () => _ = config["missing"];
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("missing");
    }

    [Fact]
    public void Throw_WhenValueIsEmpty()
    {
        var config = Build(new() { ["key"] = "" });

        Action act = () => _ = config["key"];
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>();
    }

    [Fact]
    public void Throw_WhenValueIsNull()
    {
        var config = Build(new() { ["key"] = null });

        Action act = () => _ = config["key"];
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>();
    }

    [Fact]
    public void SetValue_WhenKeyIsAssigned()
    {
        var config = Build(new() { ["key"] = "old" });

        config["key"] = "new";

        _ = config.Get("key").Should().Be("new");
    }

    [Fact]
    public void Optional_ReturnValue_WhenKeyExists()
    {
        var config = Build(new() { ["key"] = "value" });

        _ = config.Optional["key"].Should().Be("value");
    }

    [Fact]
    public void Optional_ReturnNull_WhenKeyIsMissing()
    {
        var config = Build([]);

        _ = config.Optional["missing"].Should().BeNull();
    }

    [Fact]
    public void Optional_ReturnNull_WhenValueIsEmpty()
    {
        var config = Build(new() { ["key"] = "" });

        _ = config.Optional["key"].Should().BeNull();
    }

    [Fact]
    public void Optional_ReturnNull_WhenValueIsNull()
    {
        var config = Build(new() { ["key"] = null });

        _ = config.Optional["key"].Should().BeNull();
    }
}

namespace ApifConfigurationTests;

using ApifConfiguration;
using Sut = ApifConfiguration.ApifConfigurationWrapper;

public sealed class GetConnectionString_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [Fact]
    public void ReturnConnectionString_WhenNameExists()
    {
        var config = Build(new() { ["ConnectionStrings:db"] = "Server=.;Database=Test" });

        _ = config.GetConnectionString("db").Should().Be("Server=.;Database=Test");
    }

    [Fact]
    public void Throw_WhenNameIsMissing()
    {
        var config = Build([]);

        Action act = () => config.GetConnectionString("missing");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("missing");
    }

    [Fact]
    public void Throw_WhenConnectionStringIsEmpty()
    {
        var config = Build(new() { ["ConnectionStrings:db"] = "" });

        Action act = () => config.GetConnectionString("db");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>();
    }

    [Fact]
    public void Throw_WhenConnectionStringIsNull()
    {
        var config = Build(new() { ["ConnectionStrings:db"] = null });

        Action act = () => config.GetConnectionString("db");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>();
    }

    // ----- Optional.GetConnectionString(string name) -----

    [Fact]
    public void Optional_ReturnConnectionString_WhenNameExists()
    {
        var config = Build(new() { ["ConnectionStrings:db"] = "Server=.;Database=Test" });

        _ = config.Optional.GetConnectionString("db").Should().Be("Server=.;Database=Test");
    }

    [Fact]
    public void Optional_ReturnNull_WhenNameIsMissing()
    {
        var config = Build([]);

        _ = config.Optional.GetConnectionString("missing").Should().BeNull();
    }

    [Fact]
    public void Optional_ReturnNull_WhenConnectionStringIsEmpty()
    {
        var config = Build(new() { ["ConnectionStrings:db"] = "" });

        _ = config.Optional.GetConnectionString("db").Should().BeNull();
    }

    [Fact]
    public void Optional_ReturnNull_WhenConnectionStringIsNull()
    {
        var config = Build(new() { ["ConnectionStrings:db"] = null });

        _ = config.Optional.GetConnectionString("db").Should().BeNull();
    }
}

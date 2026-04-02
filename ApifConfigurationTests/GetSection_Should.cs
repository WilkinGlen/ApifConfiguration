namespace ApifConfigurationTests;

using ApifConfiguration;
using Sut = ApifConfiguration.ApifConfiguration;

public sealed class GetSection_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [Fact]
    public void ReturnApifConfiguration_WhenSectionExists()
    {
        var config = Build(new() { ["Section:Key"] = "value" });

        var section = config.GetSection("Section");

        _ = section.Should().BeOfType<Sut>();
    }

    [Fact]
    public void ReturnSectionWithCorrectValue_WhenSectionExists()
    {
        var config = Build(new() { ["Section:Key"] = "value" });

        var section = (Sut)config.GetSection("Section");

        _ = section.Get("Key").Should().Be("value");
    }

    [Fact]
    public void ReturnEnforcingSection_ThatThrowsOnMissingKey()
    {
        var config = Build(new() { ["Section:Key"] = "value" });

        var section = (Sut)config.GetSection("Section");

        Action act = () => section.Get("Missing");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("Missing");
    }

    [Fact]
    public void Throw_WhenSectionDoesNotExist()
    {
        var config = Build([]);

        Action act = () => config.GetSection("Missing");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("Missing");
    }

    [Fact]
    public void Optional_ReturnApifConfiguration_WhenSectionExists()
    {
        var config = Build(new() { ["Section:Key"] = "value" });

        _ = config.Optional.GetSection("Section").Should().BeOfType<Sut>();
    }

    [Fact]
    public void Optional_ReturnSectionWithCorrectValue_WhenSectionExists()
    {
        var config = Build(new() { ["Section:Key"] = "value" });

        var section = config.Optional.GetSection("Section");

        _ = section!.Get("Key").Should().Be("value");
    }

    [Fact]
    public void Optional_ReturnEnforcingSection_ThatThrowsOnMissingKey()
    {
        var config = Build(new() { ["Section:Key"] = "value" });

        var section = config.Optional.GetSection("Section");

        Action act = () => section!.Get("Missing");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("Missing");
    }

    [Fact]
    public void Optional_ReturnNull_WhenSectionDoesNotExist()
    {
        var config = Build([]);

        _ = config.Optional.GetSection("Missing").Should().BeNull();
    }
}

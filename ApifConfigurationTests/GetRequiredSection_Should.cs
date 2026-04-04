namespace ApifConfigurationTests;

using ApifConfiguration;
using Sut = ApifConfiguration.ApifConfigurationWrapper;

public sealed class GetRequiredSection_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [Fact]
    public void ReturnApifConfiguration_WhenSectionExists()
    {
        var config = Build(new() { ["Section:Key"] = "value" });

        var section = config.GetRequiredSection("Section");

        _ = section.Should().BeOfType<Sut>();
    }

    [Fact]
    public void ReturnSectionWithCorrectValue_WhenSectionExists()
    {
        var config = Build(new() { ["Section:Key"] = "value" });

        var section = config.GetRequiredSection("Section");

        _ = section.Get("Key").Should().Be("value");
    }

    [Fact]
    public void ThrowConfigurationKeyNotFoundException_WhenSectionIsMissing()
    {
        var config = Build([]);

        Action act = () => config.GetRequiredSection("Missing");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("Missing");
    }

    [Fact]
    public void WrapInvalidOperationException_WhenSectionIsMissing()
    {
        var config = Build([]);

        Action act = () => config.GetRequiredSection("Missing");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .WithInnerException<InvalidOperationException>();
    }

    [Fact]
    public void Optional_ReturnApifConfiguration_WhenSectionExists()
    {
        var config = Build(new() { ["Section:Key"] = "value" });

        _ = config.Optional.GetRequiredSection("Section").Should().BeOfType<Sut>();
    }

    [Fact]
    public void Optional_ReturnNull_WhenSectionIsMissing()
    {
        var config = Build([]);

        _ = config.Optional.GetRequiredSection("Missing").Should().BeNull();
    }
}

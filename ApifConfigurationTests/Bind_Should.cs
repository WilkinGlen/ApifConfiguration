namespace ApifConfigurationTests;

using ApifConfiguration;
using Sut = ApifConfiguration.ApifConfigurationWrapper;

public sealed class Bind_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [Fact]
    public void PopulateInstance_WhenKeysMatch()
    {
        var config = Build(new() { ["Name"] = "Alice", ["Age"] = "30" });
        var settings = new TestSettings();

        config.Bind(settings);

        _ = settings.Name.Should().Be("Alice");
        _ = settings.Age.Should().Be(30);
    }

    [Fact]
    public void LeaveDefaultValues_WhenConfigurationIsEmpty()
    {
        var config = Build([]);
        var settings = new TestSettings { Name = "original", Age = 7 };

        config.Bind(settings);

        _ = settings.Name.Should().Be("original");
        _ = settings.Age.Should().Be(7);
    }

    [Fact]
    public void LeaveUnmatchedProperties_WhenConfigHasPartialKeys()
    {
        var config = Build(new() { ["Name"] = "Alice" });
        var settings = new TestSettings { Age = 7 };

        config.Bind(settings);

        _ = settings.Name.Should().Be("Alice");
        _ = settings.Age.Should().Be(7);
    }

    [Fact]
    public void PopulateInstance_WhenSectionExists()
    {
        var config = Build(new() { ["Section:Name"] = "Alice", ["Section:Age"] = "30" });
        var settings = new TestSettings();

        config.Bind("Section", settings);

        _ = settings.Name.Should().Be("Alice");
        _ = settings.Age.Should().Be(30);
    }

    [Fact]
    public void LeaveUnmatchedProperties_WhenSectionHasPartialKeys()
    {
        var config = Build(new() { ["Section:Name"] = "Alice" });
        var settings = new TestSettings { Age = 7 };

        config.Bind("Section", settings);

        _ = settings.Name.Should().Be("Alice");
        _ = settings.Age.Should().Be(7);
    }

    [Fact]
    public void Throw_WhenSectionKeyIsMissing()
    {
        var config = Build([]);

        Action act = () => config.Bind("Missing", new TestSettings());
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("Missing");
    }

    [Fact]
    public void Optional_PopulateInstance_WhenSectionExists()
    {
        var config = Build(new() { ["Section:Name"] = "Alice", ["Section:Age"] = "30" });
        var settings = new TestSettings();

        config.Optional.Bind("Section", settings);

        _ = settings.Name.Should().Be("Alice");
        _ = settings.Age.Should().Be(30);
    }

    [Fact]
    public void Optional_LeaveInstanceUnchanged_WhenSectionIsMissing()
    {
        var config = Build([]);
        var settings = new TestSettings { Name = "original", Age = 7 };

        config.Optional.Bind("Missing", settings);

        _ = settings.Name.Should().Be("original");
        _ = settings.Age.Should().Be(7);
    }

    private sealed class TestSettings
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }
}

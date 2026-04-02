#pragma warning disable S3459 // Unassigned members should be removed
#pragma warning disable S1144 // Unused private types or members should be removed

namespace ApifConfigurationTests;

using ApifConfiguration;
using Sut = ApifConfiguration.ApifConfiguration;

public sealed class Get_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [Fact]
    public void ReturnValue_WhenKeyExists()
    {
        var config = Build(new() { ["key"] = "value" });

        _ = config.Get("key").Should().Be("value");
    }

    [Fact]
    public void Throw_WhenKeyIsMissing()
    {
        var config = Build([]);

        Action act = () => config.Get("missing");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("missing");
    }

    [Fact]
    public void Throw_WhenValueIsEmpty()
    {
        var config = Build(new() { ["key"] = "" });

        Action act = () => config.Get("key");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>();
    }

    [Fact]
    public void Throw_WhenValueIsNull()
    {
        var config = Build(new() { ["key"] = null });

        Action act = () => config.Get("key");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>();
    }

    [Fact]
    public void BindConfigurationToType_WhenKeysMatch()
    {
        var config = Build(new() { ["Name"] = "Alice", ["Age"] = "30" });

        var result = config.Get<TestSettings>();

        _ = result.Should().NotBeNull();
        _ = result!.Name.Should().Be("Alice");
        _ = result.Age.Should().Be(30);
    }

    [Fact]
    public void ReturnNull_WhenConfigurationIsEmpty()
    {
        var config = Build([]);

        _ = config.Get<TestSettings>().Should().BeNull();
    }

    [Fact]
    public void BindConfigurationToRecord_WhenKeysMatch()
    {
        var config = Build(new() { ["Name"] = "Alice", ["Age"] = "30" });

        var result = config.Get<TestRecord>();

        _ = result.Should().NotBeNull();
        _ = result!.Name.Should().Be("Alice");
        _ = result.Age.Should().Be(30);
    }

    [Fact]
    public void BindConfigurationToTypeWithCollection_WhenIndexedKeysMatch()
    {
        var config = Build(new() { ["Tags:0"] = "a", ["Tags:1"] = "b", ["Tags:2"] = "c" });

        var result = config.Get<SettingsWithCollection>();

        _ = result.Should().NotBeNull();
        _ = result!.Tags.Should().Equal("a", "b", "c");
    }

    [Fact]
    public void BindConfigurationToTypeWithNestedObject_WhenNestedKeysMatch()
    {
        var config = Build(new() { ["Name"] = "Alice", ["Address:Street"] = "Main St", ["Address:City"] = "Springfield" });

        var result = config.Get<SettingsWithNested>();

        _ = result.Should().NotBeNull();
        _ = result!.Name.Should().Be("Alice");
        _ = result.Address.Should().NotBeNull();
        _ = result.Address!.Street.Should().Be("Main St");
        _ = result.Address.City.Should().Be("Springfield");
    }

    [Fact]
    public void BindConfigurationToTypeWithEnum_WhenEnumKeyMatches()
    {
        var config = Build(new() { ["Level"] = "High" });

        var result = config.Get<SettingsWithEnum>();

        _ = result.Should().NotBeNull();
        _ = result!.Level.Should().Be(TestLevel.High);
    }

    [Fact]
    public void BindConfigurationToTypeWithBool_WhenBoolKeyMatches()
    {
        var config = Build(new() { ["Enabled"] = "true" });

        var result = config.Get<SettingsWithBool>();

        _ = result.Should().NotBeNull();
        _ = result!.Enabled.Should().BeTrue();
    }

    [Fact]
    public void BindConfigurationToTypeWithGuid_WhenGuidKeyMatches()
    {
        var id = Guid.NewGuid();
        var config = Build(new() { ["Id"] = id.ToString() });

        var result = config.Get<SettingsWithGuid>();

        _ = result.Should().NotBeNull();
        _ = result!.Id.Should().Be(id);
    }

    [Fact]
    public void BindConfigurationToTypeWithTimeSpan_WhenTimeSpanKeyMatches()
    {
        var config = Build(new() { ["Timeout"] = "00:01:30" });

        var result = config.Get<SettingsWithTimeSpan>();

        _ = result.Should().NotBeNull();
        _ = result!.Timeout.Should().Be(TimeSpan.FromSeconds(90));
    }

    [Fact]
    public void BindConfigurationToTypeWithRequiredProperties_WhenKeysMatch()
    {
        var config = Build(new() { ["Name"] = "Alice", ["Age"] = "30" });

        var result = config.Get<RequiredSettings>();

        _ = result.Should().NotBeNull();
        _ = result!.Name.Should().Be("Alice");
        _ = result.Age.Should().Be(30);
    }

    [Fact]
    public void BindConfigurationToTypeWithNullableProperty_WhenOptionalKeyIsAbsent()
    {
        var config = Build(new() { ["Name"] = "Alice" });

        var result = config.Get<SettingsWithOptionalCount>();

        _ = result.Should().NotBeNull();
        _ = result!.Name.Should().Be("Alice");
        _ = result.Count.Should().BeNull();
    }

    [Fact]
    public void Optional_ReturnValue_WhenKeyExists()
    {
        var config = Build(new() { ["key"] = "value" });

        _ = config.Optional.Get("key").Should().Be("value");
    }

    [Fact]
    public void Optional_ReturnNull_WhenKeyIsMissing()
    {
        var config = Build([]);

        _ = config.Optional.Get("missing").Should().BeNull();
    }

    [Fact]
    public void Optional_ReturnNull_WhenValueIsEmpty()
    {
        var config = Build(new() { ["key"] = "" });

        _ = config.Optional.Get("key").Should().BeNull();
    }

    [Fact]
    public void Optional_ReturnNull_WhenValueIsNull()
    {
        var config = Build(new() { ["key"] = null });

        _ = config.Optional.Get("key").Should().BeNull();
    }

    private sealed class TestSettings
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    private sealed record TestRecord(string Name, int Age);

    private sealed class SettingsWithCollection
    {
        public List<string> Tags { get; set; } = [];
    }

    private sealed class SettingsWithNested
    {
        public string? Name { get; set; }
        public NestedAddress? Address { get; set; }
    }

    private sealed class NestedAddress
    {
        public string? Street { get; set; }
        public string? City { get; set; }
    }

    private enum TestLevel { Low, Medium, High }

    private sealed class SettingsWithEnum
    {
        public TestLevel Level { get; set; }
    }

    private sealed class SettingsWithBool
    {
        public bool Enabled { get; set; }
    }

    private sealed class SettingsWithGuid
    {
        public Guid Id { get; set; }
    }

    private sealed class SettingsWithTimeSpan
    {
        public TimeSpan Timeout { get; set; }
    }

    private sealed class RequiredSettings
    {
        public required string Name { get; init; }
        public required int Age { get; init; }
    }

    private sealed class SettingsWithOptionalCount
    {
        public string? Name { get; set; }
        public int? Count { get; set; }
    }
}

namespace ApifConfigurationTests;

using ApifConfiguration;
using Sut = ApifConfiguration.ApifConfiguration;

public sealed class GetValue_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    [Fact]
    public void ReturnTypedValue_WhenKeyExists()
    {
        var config = Build(new() { ["count"] = "42" });

        _ = config.GetValue<int>("count").Should().Be(42);
    }

    [Fact]
    public void ReturnStringValue_WhenKeyExists()
    {
        var config = Build(new() { ["name"] = "Alice" });

        _ = config.GetValue<string>("name").Should().Be("Alice");
    }

    [Fact]
    public void ReturnBoolValue_WhenKeyExists()
    {
        var config = Build(new() { ["enabled"] = "true" });

        _ = config.GetValue<bool>("enabled").Should().BeTrue();
    }

    [Fact]
    public void ReturnDoubleValue_WhenKeyExists()
    {
        var config = Build(new() { ["rate"] = "3.14" });

        _ = config.GetValue<double>("rate").Should().Be(3.14);
    }

    [Fact]
    public void ReturnEnumValue_WhenKeyExists()
    {
        var config = Build(new() { ["level"] = "High" });

        _ = config.GetValue<TestLevel>("level").Should().Be(TestLevel.High);
    }

    [Fact]
    public void Throw_WhenKeyIsMissing()
    {
        var config = Build([]);

        Action act = () => config.GetValue<int>("missing");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("missing");
    }

    [Fact]
    public void Throw_WhenValueIsEmpty()
    {
        var config = Build(new() { ["count"] = "" });

        Action act = () => config.GetValue<int>("count");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>();
    }

    [Fact]
    public void Throw_WhenValueIsNull()
    {
        var config = Build(new() { ["count"] = null });

        Action act = () => config.GetValue<int>("count");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>();
    }

    [Fact]
    public void WrapInvalidOperationException_WhenValueIsUnparseable()
    {
        var config = Build(new() { ["count"] = "abc" });

        Action act = () => config.GetValue<int>("count");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("count");
    }

    [Fact]
    public void WrapInvalidOperationException_WithInnerException_WhenValueIsUnparseable()
    {
        var config = Build(new() { ["count"] = "abc" });

        Action act = () => config.GetValue<int>("count");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .WithInnerException<InvalidOperationException>();
    }

    [Fact]
    public void ReturnTypedValue_WhenKeyExistsWithDefault()
    {
        var config = Build(new() { ["count"] = "42" });

        _ = config.GetValue("count", 99).Should().Be(42);
    }

    [Fact]
    public void ReturnStringValue_WhenKeyExistsWithDefault()
    {
        var config = Build(new() { ["name"] = "Alice" });

        _ = config.GetValue("name", "fallback").Should().Be("Alice");
    }

    [Fact]
    public void ReturnEnumValue_WhenKeyExistsWithDefault()
    {
        var config = Build(new() { ["level"] = "High" });

        _ = config.GetValue("level", TestLevel.Low).Should().Be(TestLevel.High);
    }

    [Fact]
    public void Throw_WhenKeyIsMissingWithDefault()
    {
        var config = Build([]);

        Action act = () => config.GetValue("missing", 99);
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("missing");
    }

    [Fact]
    public void Throw_WhenStringKeyIsMissingWithDefault()
    {
        var config = Build([]);

        Action act = () => config.GetValue("missing", "fallback");
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("missing");
    }

    [Fact]
    public void Throw_WhenBoolKeyIsMissingWithDefault()
    {
        var config = Build([]);

        Action act = () => config.GetValue("missing", true);
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("missing");
    }

    [Fact]
    public void Throw_WhenEnumKeyIsMissingWithDefault()
    {
        var config = Build([]);

        Action act = () => config.GetValue("missing", TestLevel.Medium);
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("missing");
    }

    [Fact]
    public void Throw_WhenValueIsNullWithDefault()
    {
        var config = Build(new() { ["count"] = null });

        Action act = () => config.GetValue("count", 99);
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("count");
    }

    [Fact]
    public void WrapInvalidOperationException_WhenValueIsUnparseableWithDefault()
    {
        var config = Build(new() { ["count"] = "abc" });

        Action act = () => config.GetValue("count", 99);
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .Which.Key.Should().Be("count");
    }

    [Fact]
    public void WrapInvalidOperationException_WithInnerException_WhenValueIsUnparseableWithDefault()
    {
        var config = Build(new() { ["count"] = "abc" });

        Action act = () => config.GetValue("count", 99);
        _ = act.Should().Throw<ConfigurationKeyNotFoundException>()
            .WithInnerException<InvalidOperationException>();
    }

    [Fact]
    public void Optional_ReturnTypedValue_WhenKeyExists()
    {
        var config = Build(new() { ["count"] = "42" });

        _ = config.Optional.GetValue<int>("count").Should().Be(42);
    }

    [Fact]
    public void Optional_ReturnStringValue_WhenKeyExists()
    {
        var config = Build(new() { ["name"] = "Alice" });

        _ = config.Optional.GetValue<string>("name").Should().Be("Alice");
    }

    [Fact]
    public void Optional_ReturnBoolValue_WhenKeyExists()
    {
        var config = Build(new() { ["enabled"] = "true" });

        _ = config.Optional.GetValue<bool>("enabled").Should().BeTrue();
    }

    [Fact]
    public void Optional_ReturnEnumValue_WhenKeyExists()
    {
        var config = Build(new() { ["level"] = "High" });

        _ = config.Optional.GetValue<TestLevel>("level").Should().Be(TestLevel.High);
    }

    [Fact]
    public void Optional_ReturnDefault_WhenKeyIsMissing()
    {
        var config = Build([]);

        _ = config.Optional.GetValue<int>("missing").Should().Be(0);
    }

    [Fact]
    public void Optional_ReturnDefault_WhenValueIsEmpty()
    {
        var config = Build(new() { ["count"] = "" });

        _ = config.Optional.GetValue<int>("count").Should().Be(0);
    }

    [Fact]
    public void Optional_ReturnDefault_WhenValueIsNull()
    {
        var config = Build(new() { ["count"] = null });

        _ = config.Optional.GetValue<int>("count").Should().Be(0);
    }

    [Fact]
    public void Optional_ReturnTypedValue_WhenKeyExistsWithDefault()
    {
        var config = Build(new() { ["count"] = "42" });

        _ = config.Optional.GetValue("count", 99).Should().Be(42);
    }

    [Fact]
    public void Optional_ReturnDefault_WhenKeyIsMissingWithDefault()
    {
        var config = Build([]);

        _ = config.Optional.GetValue("missing", 99).Should().Be(99);
    }

    [Fact]
    public void Optional_ReturnDefault_WhenValueIsNullWithDefault()
    {
        var config = Build(new() { ["count"] = null });

        _ = config.Optional.GetValue("count", 99).Should().Be(99);
    }

    [Fact]
    public void Optional_ReturnDefault_WhenValueIsEmptyWithDefault()
    {
        var config = Build(new() { ["count"] = "" });

        _ = config.Optional.GetValue("count", 99).Should().Be(99);
    }

    [Fact]
    public void Optional_ReturnDefault_WhenValueIsUnparseableWithDefault()
    {
        var config = Build(new() { ["count"] = "abc" });

        _ = config.Optional.GetValue("count", 99).Should().Be(99);
    }

    private enum TestLevel { Low, Medium, High }
}

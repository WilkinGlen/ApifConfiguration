namespace ApifConfigurationTests;

using Sut = ApifConfiguration.ApifConfigurationWrapper;

public sealed class Value_Should
{
    private static Sut Build(Dictionary<string, string?> data)
    {
        return new(new ConfigurationBuilder().AddInMemoryCollection(data).Build());
    }

    private static Sut BuildSection(Dictionary<string, string?> data, string sectionKey)
    {
        var root = new ConfigurationBuilder().AddInMemoryCollection(data).Build();
        return new(root.GetSection(sectionKey));
    }

    [Fact]
    public void ReturnNull_WhenWrappingRootConfiguration()
    {
        var config = Build(new() { ["key"] = "value" });

        _ = config.Value.Should().BeNull();
    }

    [Fact]
    public void ReturnValue_WhenWrappingSection()
    {
        var config = BuildSection(new() { ["section"] = "hello" }, "section");

        _ = config.Value.Should().Be("hello");
    }

    [Fact]
    public void ReturnNull_WhenSectionHasNoValue()
    {
        var config = BuildSection(new() { ["section:child"] = "nested" }, "section");

        _ = config.Value.Should().BeNull();
    }

    [Fact]
    public void ThrowInvalidOperationException_WhenSettingValueOnRoot()
    {
        var config = Build(new() { ["key"] = "value" });

        Action act = () => config.Value = "new";
        _ = act.Should().Throw<InvalidOperationException>()
            .WithMessage("*root*");
    }

    [Fact]
    public void SetValue_WhenWrappingSection()
    {
        var config = BuildSection(new() { ["section"] = "old" }, "section");

        config.Value = "new";

        _ = config.Value.Should().Be("new");
    }

    [Fact]
    public void SetValueToNull_WhenWrappingSection()
    {
        var config = BuildSection(new() { ["section"] = "old" }, "section");

        config.Value = null;

        _ = config.Value.Should().BeNull();
    }

    [Fact]
    public void ReturnEmptyKey_WhenWrappingRoot()
    {
        var config = Build(new() { ["key"] = "value" });

        _ = config.Key.Should().BeEmpty();
    }

    [Fact]
    public void ReturnKey_WhenWrappingSection()
    {
        var config = BuildSection(new() { ["section"] = "value" }, "section");

        _ = config.Key.Should().Be("section");
    }

    [Fact]
    public void ReturnEmptyPath_WhenWrappingRoot()
    {
        var config = Build(new() { ["key"] = "value" });

        _ = config.Path.Should().BeEmpty();
    }

    [Fact]
    public void ReturnPath_WhenWrappingSection()
    {
        var config = BuildSection(new() { ["parent:child"] = "value" }, "parent:child");

        _ = config.Path.Should().Be("parent:child");
    }
}

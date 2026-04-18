namespace ApifConfigurationTests;

using Sut = ApifConfiguration.ApifConfiguration;

public sealed class NullArgument_Should
{
    private static Sut CreateSut()
    {
        var builder = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Key"] = "Value" });

        return new Sut(builder.Build());
    }

    [Fact]
    public void Constructor_ThrowOnNullConfiguration()
    {
        var act = () => new Sut(null!);

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Indexer_Get_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut[key!];

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Indexer_Set_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut[key!] = "v";

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Get_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.Get(key!);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetValueT_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.GetValue<int>(key!);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetValueTWithDefault_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.GetValue(key!, 0);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Bind_ThrowOnNullInstance()
    {
        var sut = CreateSut();

        var act = () => sut.Bind(null!);

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void BindSection_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.Bind(key!, new object());

        _ = act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void BindSection_ThrowOnNullInstance()
    {
        var sut = CreateSut();

        var act = () => sut.Bind("Key", null!);

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetSection_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.GetSection(key!);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetRequiredSection_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.GetRequiredSection(key!);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetEnforcingSection_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.GetEnforcingSection(key!);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetConnectionString_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.GetConnectionString(key!);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Optional_Indexer_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.Optional[key!];

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Optional_Get_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.Optional.Get(key!);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Optional_GetValueT_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.Optional.GetValue<int>(key!);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Optional_GetValueTWithDefault_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.Optional.GetValue(key!, 0);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Optional_GetSection_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.Optional.GetSection(key!);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Optional_GetConnectionString_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.Optional.GetConnectionString(key!);

        _ = act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Optional_BindSection_ThrowOnNullOrEmptyKey(string? key)
    {
        var sut = CreateSut();

        var act = () => sut.Optional.Bind(key!, new object());

        _ = act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Optional_BindSection_ThrowOnNullInstance()
    {
        var sut = CreateSut();

        var act = () => sut.Optional.Bind("Key", null!);

        _ = act.Should().Throw<ArgumentNullException>();
    }
}

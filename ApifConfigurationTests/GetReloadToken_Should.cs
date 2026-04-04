namespace ApifConfigurationTests;

using Sut = ApifConfiguration.ApifConfigurationWrapper;

public sealed class GetReloadToken_Should
{
    [Fact]
    public void ReturnNonNullToken()
    {
        var config = new Sut(new ConfigurationBuilder().AddInMemoryCollection([]).Build());

        _ = config.GetReloadToken().Should().NotBeNull();
    }

    [Fact]
    public void DelegateToUnderlyingConfiguration()
    {
        IConfiguration inner = new ConfigurationBuilder().AddInMemoryCollection([]).Build();
        var config = new Sut(inner);

        var expected = inner.GetReloadToken();

        _ = config.GetReloadToken().Should().BeSameAs(expected);
    }
}

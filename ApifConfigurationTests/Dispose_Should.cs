namespace ApifConfigurationTests;

using Microsoft.Extensions.Primitives;
using Sut = ApifConfiguration.ApifConfigurationWrapper;

public sealed class Dispose_Should
{
    [Fact]
    public void DelegateToUnderlyingDisposable()
    {
        var config = new Sut(new ConfigurationBuilder().AddInMemoryCollection([]).Build());

        config.Dispose();

        _ = config.Should().NotBeNull();
    }

    [Fact]
    public void NotThrow_WhenUnderlyingIsNotDisposable()
    {
        var config = new Sut(new NonDisposableConfiguration());

        Action act = config.Dispose;

        _ = act.Should().NotThrow();
    }

    [Fact]
    public void DisposeUnderlyingOnlyOnce_WhenCalledMultipleTimes()
    {
        var config = new Sut(new ConfigurationBuilder().AddInMemoryCollection([]).Build());

        config.Dispose();
        Action act = config.Dispose;

        _ = act.Should().NotThrow();
    }

    private sealed class NonDisposableConfiguration : IConfiguration
    {
        public string? this[string key]
        {
            get => null;
            set
            {
                _ = value;
            }
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];

        public IChangeToken GetReloadToken() => new CancellationChangeToken(CancellationToken.None);

        public IConfigurationSection GetSection(string key) =>
            new ConfigurationBuilder().AddInMemoryCollection([]).Build().GetSection(key);
    }
}

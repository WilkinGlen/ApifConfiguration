namespace ApifConfiguration;

using Microsoft.Extensions.Configuration;

public sealed class OptionalConfiguration
{
    private readonly IConfiguration configuration;

    internal OptionalConfiguration(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public string? this[string key]
    {
        get
        {
            var value = this.configuration[key];

            return string.IsNullOrEmpty(value) ? null : value;
        }
    }

    public string? Get(string key)
    {
        return this[key];
    }

    public T? GetValue<T>(string key)
    {
        var section = this.configuration.GetSection(key);

        return string.IsNullOrEmpty(section.Value) ? default : section.Get<T>();
    }

    public ApifConfiguration? GetSection(string key)
    {
        var section = this.configuration.GetSection(key);

        return section.Exists() ? new ApifConfiguration(section) : null;
    }

    public string? GetConnectionString(string name)
    {
        var value = this.configuration.GetConnectionString(name);

        return string.IsNullOrEmpty(value) ? null : value;
    }

    public void Bind(string sectionKey, object instance)
    {
        var section = this.configuration.GetSection(sectionKey);

        if (section.Exists())
        {
            section.Bind(instance);
        }
    }
}

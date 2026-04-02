namespace ApifConfiguration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

public sealed class ApifConfiguration : IConfigurationSection
{
    private readonly IConfiguration configuration;
    private readonly IConfigurationSection? section;

    public OptionalConfiguration Optional { get; }

    public ApifConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        this.configuration = configuration;
        this.section = configuration as IConfigurationSection;
        this.Optional = new OptionalConfiguration(configuration);
    }

    public string Key => this.section?.Key ?? string.Empty;

    public string Path => this.section?.Path ?? string.Empty;

    public string? Value
    {
        get => this.section?.Value;
        set
        {
            if (this.section is not null)
            {
                this.section.Value = value;
            }
        }
    }

    public string? this[string key]
    {
        get
        {
            var value = this.configuration[key];

            return string.IsNullOrEmpty(value) ? throw new ConfigurationKeyNotFoundException(key) : value;
        }
        set => this.configuration[key] = value;
    }

    public string Get(string key) => this[key]!;

    public T? Get<T>()
    {
        return this.configuration.Get<T>();
    }

    public T? GetValue<T>(string key)
    {
        var section = this.configuration.GetSection(key);

        return string.IsNullOrEmpty(section.Value)
            ? throw new ConfigurationKeyNotFoundException(key)
            : section.Get<T>();
    }

    public T GetValue<T>(string key, T defaultValue)
    {
        return this.configuration.GetValue(key, defaultValue)!;
    }

    public void Bind(object instance)
    {
        this.configuration.Bind(instance);
    }

    public void Bind(string sectionKey, object instance)
    {
        var section = this.configuration.GetSection(sectionKey);

        if (!section.Exists())
        {
            throw new ConfigurationKeyNotFoundException(sectionKey);
        }

        section.Bind(instance);
    }

    public IConfigurationSection GetSection(string key)
    {
        var section = this.configuration.GetSection(key);

        return section.Exists() ? new ApifConfiguration(section) : throw new ConfigurationKeyNotFoundException(key);
    }

    public ApifConfiguration GetRequiredSection(string key)
    {
        try
        {
            return new(this.configuration.GetRequiredSection(key));
        }
        catch (InvalidOperationException ex)
        {
            throw new ConfigurationKeyNotFoundException(key, ex);
        }
    }

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        foreach (var section in this.configuration.GetChildren())
        {
            yield return new ApifConfiguration(section);
        }
    }

    public string GetConnectionString(string name)
    {
        var value = this.configuration.GetConnectionString(name);

        return string.IsNullOrEmpty(value) ? throw new ConfigurationKeyNotFoundException(name) : value;
    }

    public IChangeToken GetReloadToken()
    {
        return this.configuration.GetReloadToken();
    }
}

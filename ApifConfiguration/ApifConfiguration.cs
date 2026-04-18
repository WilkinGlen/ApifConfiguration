namespace ApifConfiguration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

/// <summary>
/// An enforcing wrapper around <see cref="IConfiguration"/> that throws
/// <see cref="ConfigurationKeyNotFoundException"/> when a required key is missing or empty.
/// Implements <see cref="IConfigurationSection"/> so it can be used as a drop-in replacement
/// and <see cref="IDisposable"/> to forward disposal to the underlying configuration root.
/// </summary>
public sealed class ApifConfiguration : IConfigurationSection, IDisposable
{
    private readonly IConfiguration configuration;
    private readonly IConfigurationSection? section;

    /// <summary>
    /// Gets the non-enforcing accessor that returns <c>null</c> or default values
    /// instead of throwing when a key is missing or empty.
    /// The instance is lazily allocated on first access.
    /// </summary>
    public OptionalConfiguration Optional => field ??= new OptionalConfiguration(this.configuration);

    /// <summary>
    /// Initialises a new <see cref="ApifConfiguration"/> wrapping the supplied
    /// <paramref name="configuration"/> instance.
    /// </summary>
    /// <param name="configuration">The underlying configuration to wrap.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <c>null</c>.</exception>
    public ApifConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        this.configuration = configuration;
        this.section = configuration as IConfigurationSection;
    }

    /// <summary>
    /// Gets the key this section occupies in its parent.
    /// Returns <see cref="string.Empty"/> when the instance wraps a root configuration.
    /// </summary>
    public string Key => this.section?.Key ?? string.Empty;

    /// <summary>
    /// Gets the full path of this section from the configuration root.
    /// Returns <see cref="string.Empty"/> when the instance wraps a root configuration.
    /// </summary>
    public string Path => this.section?.Path ?? string.Empty;

    /// <summary>
    /// Gets or sets the section value.
    /// The getter returns <c>null</c> when the instance wraps a root configuration.
    /// The setter throws <see cref="InvalidOperationException"/> when called on a root configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when setting the value on a root configuration.</exception>
    public string? Value
    {
        get => this.section?.Value;
        set
        {
            if (this.section is null)
            {
                throw new InvalidOperationException("Cannot set Value on a root configuration.");
            }

            this.section.Value = value;
        }
    }

    /// <summary>
    /// Gets or sets a configuration value by key.
    /// The getter throws <see cref="ConfigurationKeyNotFoundException"/> when the value is missing or empty.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <exception cref="ConfigurationKeyNotFoundException">The key is missing or has an empty value.</exception>
    public string? this[string key]
    {
        get => this.Get(key);
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            this.configuration[key] = value;
        }
    }

    /// <summary>
    /// Gets a required string configuration value by key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The non-empty configuration value.</returns>
    /// <exception cref="ConfigurationKeyNotFoundException">The key is missing or has an empty value.</exception>
    public string Get(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var value = this.configuration[key];

        return string.IsNullOrEmpty(value) ? throw new ConfigurationKeyNotFoundException(key) : value;
    }

    /// <summary>
    /// Binds the entire configuration to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to bind to.</typeparam>
    /// <returns>The bound instance, or <c>null</c> if binding produces no result.</returns>
    public T? Get<T>()
    {
        return this.configuration.Get<T>();
    }

    /// <summary>
    /// Gets a required typed value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="ConfigurationKeyNotFoundException">
    /// The key is missing, has an empty value, or the value cannot be converted to <typeparamref name="T"/>.
    /// </exception>
    public T? GetValue<T>(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var configSection = this.configuration.GetSection(key);

        if (string.IsNullOrEmpty(configSection.Value))
        {
            throw new ConfigurationKeyNotFoundException(key);
        }

        try
        {
            return configSection.Get<T>();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConfigurationKeyNotFoundException(key, ex);
        }
    }

    /// <summary>
    /// Gets a typed value by key, returning <paramref name="defaultValue"/> only when
    /// <c>Get&lt;T&gt;()</c> yields <c>null</c> for a present key.
    /// Throws <see cref="ConfigurationKeyNotFoundException"/> when the key is absent or empty.
    /// Use <see cref="OptionalConfiguration.GetValue{T}(string, T)"/> to fall back silently.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The fallback value used when the key is present but binding returns <c>null</c>.</param>
    /// <returns>The converted value, or <paramref name="defaultValue"/>.</returns>
    /// <exception cref="ConfigurationKeyNotFoundException">
    /// The key is absent, has an empty value, or the value cannot be converted to <typeparamref name="T"/>.
    /// </exception>
    public T GetValue<T>(string key, T defaultValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var configSection = this.configuration.GetSection(key);

        if (string.IsNullOrEmpty(configSection.Value))
        {
            throw new ConfigurationKeyNotFoundException(key);
        }

        try
        {
            return configSection.Get<T>() ?? defaultValue;
        }
        catch (InvalidOperationException ex)
        {
            throw new ConfigurationKeyNotFoundException(key, ex);
        }
    }

    /// <summary>
    /// Binds the entire configuration to an existing object instance.
    /// </summary>
    /// <param name="instance">The object to bind configuration values to.</param>
    public void Bind(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        this.configuration.Bind(instance);
    }

    /// <summary>
    /// Binds a configuration sub-section to an existing object instance.
    /// </summary>
    /// <param name="sectionKey">The key of the section to bind.</param>
    /// <param name="instance">The object to bind configuration values to.</param>
    /// <exception cref="ConfigurationKeyNotFoundException">The section does not exist.</exception>
    public void Bind(string sectionKey, object instance)
    {
        ArgumentException.ThrowIfNullOrEmpty(sectionKey);
        ArgumentNullException.ThrowIfNull(instance);
        var configSection = this.configuration.GetSection(sectionKey);

        if (!configSection.Exists())
        {
            throw new ConfigurationKeyNotFoundException(sectionKey);
        }

        configSection.Bind(instance);
    }

    /// <summary>
    /// Gets a configuration sub-section with the specified key, returned as
    /// <see cref="IConfigurationSection"/>. The returned instance is an
    /// enforcing <see cref="ApifConfiguration"/>.
    /// </summary>
    /// <param name="key">The section key.</param>
    /// <returns>An enforcing <see cref="ApifConfiguration"/> wrapping the section.</returns>
    /// <exception cref="ConfigurationKeyNotFoundException">The section does not exist.</exception>
    public IConfigurationSection GetSection(string key)
    {
        return this.GetEnforcingSection(key);
    }

    /// <summary>
    /// Gets a required configuration sub-section as a strongly-typed
    /// <see cref="ApifConfiguration"/> instance.
    /// </summary>
    /// <param name="key">The section key.</param>
    /// <returns>An enforcing <see cref="ApifConfiguration"/> wrapping the section.</returns>
    /// <exception cref="ConfigurationKeyNotFoundException">The section does not exist.</exception>
    public ApifConfiguration GetRequiredSection(string key)
    {
        return this.GetEnforcingSection(key);
    }

    /// <summary>
    /// Gets a required configuration sub-section as a strongly-typed
    /// <see cref="ApifConfiguration"/>, avoiding the need to cast from
    /// <see cref="IConfigurationSection"/>.
    /// </summary>
    /// <param name="key">The section key.</param>
    /// <returns>An enforcing <see cref="ApifConfiguration"/> wrapping the section.</returns>
    /// <exception cref="ConfigurationKeyNotFoundException">The section does not exist.</exception>
    public ApifConfiguration GetEnforcingSection(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var configSection = this.configuration.GetSection(key);

        return configSection.Exists() ? new ApifConfiguration(configSection) : throw new ConfigurationKeyNotFoundException(key);
    }

    /// <summary>
    /// Gets the immediate descendant configuration sub-sections as
    /// <see cref="IConfigurationSection"/> instances (each an enforcing <see cref="ApifConfiguration"/>).
    /// </summary>
    /// <returns>The child sections.</returns>
    public IEnumerable<IConfigurationSection> GetChildren()
    {
        return this.GetEnforcingChildren();
    }

    /// <summary>
    /// Gets the immediate descendant configuration sub-sections as strongly-typed
    /// <see cref="ApifConfiguration"/> instances, avoiding the need to cast.
    /// </summary>
    /// <returns>The child sections as <see cref="ApifConfiguration"/>.</returns>
    public IEnumerable<ApifConfiguration> GetEnforcingChildren()
    {
        foreach (var configSection in this.configuration.GetChildren())
        {
            yield return new ApifConfiguration(configSection);
        }
    }

    /// <summary>
    /// Gets a required connection string by name.
    /// </summary>
    /// <param name="name">The connection string name.</param>
    /// <returns>The connection string value.</returns>
    /// <exception cref="ConfigurationKeyNotFoundException">The connection string is missing or empty.</exception>
    public string GetConnectionString(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        var value = this.configuration.GetConnectionString(name);

        return string.IsNullOrEmpty(value) ? throw new ConfigurationKeyNotFoundException(name) : value;
    }

    /// <summary>
    /// Returns a <see cref="IChangeToken"/> that can be used to observe when this configuration is reloaded.
    /// </summary>
    /// <returns>A change token.</returns>
    public IChangeToken GetReloadToken()
    {
        return this.configuration.GetReloadToken();
    }

    /// <summary>
    /// Disposes the underlying configuration if it implements <see cref="IDisposable"/>.
    /// </summary>
    public void Dispose()
    {
        (this.configuration as IDisposable)?.Dispose();
    }
}

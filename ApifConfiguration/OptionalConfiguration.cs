namespace ApifConfiguration;

using Microsoft.Extensions.Configuration;

/// <summary>
/// A non-enforcing accessor that returns <c>null</c> or default values instead of
/// throwing when a configuration key is missing or empty.
/// Obtained via <see cref="ApifConfiguration.Optional"/>.
/// </summary>
public sealed class OptionalConfiguration
{
    private readonly IConfiguration configuration;

    internal OptionalConfiguration(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    /// <summary>
    /// Gets a configuration value by key, returning <c>null</c> when the value is missing or empty.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    public string? this[string key]
    {
        get
        {
            var value = this.configuration[key];

            return string.IsNullOrEmpty(value) ? null : value;
        }
    }

    /// <summary>
    /// Gets a string configuration value by key, returning <c>null</c> when the value is missing or empty.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The value, or <c>null</c>.</returns>
    public string? Get(string key)
    {
        return this[key];
    }

    /// <summary>
    /// Gets a typed value by key, returning <c>default</c> when the key is missing or empty.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <returns>The converted value, or <c>default</c>.</returns>
    public T? GetValue<T>(string key)
    {
        var section = this.configuration.GetSection(key);

        return string.IsNullOrEmpty(section.Value) ? default : section.Get<T>();
    }

    /// <summary>
    /// Gets a configuration sub-section by key, returning <c>null</c> when the section does not exist.
    /// </summary>
    /// <param name="key">The section key.</param>
    /// <returns>An enforcing <see cref="ApifConfiguration"/> wrapping the section, or <c>null</c>.</returns>
    public ApifConfiguration? GetSection(string key)
    {
        var section = this.configuration.GetSection(key);

        return section.Exists() ? new ApifConfiguration(section) : null;
    }

    /// <summary>
    /// Gets a connection string by name, returning <c>null</c> when it is missing or empty.
    /// </summary>
    /// <param name="name">The connection string name.</param>
    /// <returns>The connection string value, or <c>null</c>.</returns>
    public string? GetConnectionString(string name)
    {
        var value = this.configuration.GetConnectionString(name);

        return string.IsNullOrEmpty(value) ? null : value;
    }

    /// <summary>
    /// Binds a configuration sub-section to an existing object instance.
    /// Does nothing if the section does not exist.
    /// </summary>
    /// <param name="sectionKey">The key of the section to bind.</param>
    /// <param name="instance">The object to bind configuration values to.</param>
    public void Bind(string sectionKey, object instance)
    {
        var section = this.configuration.GetSection(sectionKey);

        if (section.Exists())
        {
            section.Bind(instance);
        }
    }
}

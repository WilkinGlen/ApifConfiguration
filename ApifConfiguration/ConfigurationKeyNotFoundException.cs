namespace ApifConfiguration;

/// <summary>
/// The exception thrown when a required configuration key is missing or has no value.
/// </summary>
public sealed class ConfigurationKeyNotFoundException : Exception
{
    /// <summary>
    /// Gets the configuration key that was not found.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Initialises a new instance of <see cref="ConfigurationKeyNotFoundException"/>
    /// for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The configuration key that was not found.</param>
    public ConfigurationKeyNotFoundException(string key)
        : base($"Required configuration key '{key}' is missing or has no value.")
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        this.Key = key;
    }

    /// <summary>
    /// Initialises a new instance of <see cref="ConfigurationKeyNotFoundException"/>
    /// for the specified <paramref name="key"/> with an inner exception.
    /// </summary>
    /// <param name="key">The configuration key that was not found.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public ConfigurationKeyNotFoundException(string key, Exception innerException)
        : base($"Required configuration key '{key}' is missing or has no value.", innerException)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        this.Key = key;
    }
}

namespace ApifConfiguration;

public sealed class ConfigurationKeyNotFoundException : Exception
{
    public string Key { get; }

    public ConfigurationKeyNotFoundException(string key)
        : base($"Required configuration key '{key}' is missing or has no value.")
    {
        this.Key = key;
    }

    public ConfigurationKeyNotFoundException(string key, Exception innerException)
        : base($"Required configuration key '{key}' is missing or has no value.", innerException)
    {
        this.Key = key;
    }
}

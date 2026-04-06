# ApifConfiguration

A strongly-typed, enforcing wrapper around `Microsoft.Extensions.Configuration` for .NET 10.

Every access that resolves a missing or empty value **throws** by default. An `.Optional` accessor is available on every instance for callers that prefer a `null` return instead.

---

## Installation

Add a project reference or copy the source into your solution. The library targets **net10.0** and depends only on:

- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.Configuration.Binder`

---

## Quick start

```csharp
IConfiguration root = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var config = new ApifConfiguration(root);

// Throws ConfigurationKeyNotFoundException if missing or empty.
string connStr = config.GetConnectionString("Default");

// Returns null if missing or empty — never throws.
string? featureFlag = config.Optional.Get("FeatureFlags:DarkMode");
```

---

## Using with ASP.NET Core dependency injection

### Registration

Register `ApifConfiguration` as a singleton in `Program.cs` by wrapping the `IConfiguration` that ASP.NET Core injects automatically:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ApifConfiguration>(
    sp => new ApifConfiguration(sp.GetRequiredService<IConfiguration>()));
```

### Eager startup validation

Validate all required keys inside the factory delegate so that a missing or empty value throws **before the application starts accepting requests**:

```csharp
// Program.cs
builder.Services.AddSingleton<ApifConfiguration>(sp =>
{
    var config = new ApifConfiguration(sp.GetRequiredService<IConfiguration>());

    // Throws ConfigurationKeyNotFoundException at startup if any key is absent.
    _ = config.GetConnectionString("Default");
    _ = config.Get("Auth:JwtSecret");
    _ = config.GetValue<int>("Auth:TokenLifetimeMinutes");

    return config;
});
```

### Injecting into services

Declare `ApifConfiguration` as a constructor parameter. The DI container resolves the registered singleton automatically:

```csharp
public sealed class OrderService
{
    private readonly ApifConfiguration config;

    public OrderService(ApifConfiguration config)
    {
        this.config = config;
    }

    public async Task ProcessAsync(Order order)
    {
        string endpoint = this.config.Get("Orders:ProcessingEndpoint");
        int retries     = this.config.Optional.GetValue("Orders:MaxRetries", 3);
        // ...
    }
}
```

Register the service as usual:

```csharp
builder.Services.AddScoped<OrderService>();
```

### Injecting into controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly ApifConfiguration config;
    private readonly IProductRepository repository;

    public ProductsController(ApifConfiguration config, IProductRepository repository)
    {
        this.config     = config;
        this.repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetPageAsync([FromQuery] int page = 1)
    {
        int maxPageSize = this.config.Optional.GetValue("Products:MaxPageSize", 50);
        // ...
    }
}
```

### Binding strongly-typed settings

Create a plain settings class, bind it once at registration time and register the result as its own singleton. Downstream services receive the concrete type instead of `ApifConfiguration`:

```json
// appsettings.json
{
  "Database": {
    "Host": "localhost",
    "Port": 5432,
    "Name": "orders"
  }
}
```

```csharp
public sealed class DatabaseSettings
{
    public string Host { get; set; } = string.Empty;
    public int    Port { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

```csharp
// Program.cs — bind and validate the section at startup.
builder.Services.AddSingleton<DatabaseSettings>(sp =>
{
    var config   = sp.GetRequiredService<ApifConfiguration>();
    var settings = new DatabaseSettings();
    config.Bind("Database", settings);  // throws if the section is absent
    return settings;
});
```

Inject `DatabaseSettings` directly where it is needed:

```csharp
public sealed class DatabaseConnectionFactory
{
    private readonly DatabaseSettings settings;

    public DatabaseConnectionFactory(DatabaseSettings settings)
    {
        this.settings = settings;
    }

    public string BuildConnectionString() =>
        $"Host={this.settings.Host};Port={this.settings.Port};Database={this.settings.Name}";
}
```

### Scoping access to a single section

Use `GetEnforcingSection` to narrow an `ApifConfiguration` down to one configuration slice before handing it to a service. The service then uses short relative keys and never has access to the rest of the configuration:

```csharp
// Program.cs
builder.Services.AddSingleton<IEmailService>(sp =>
{
    ApifConfiguration emailConfig =
        sp.GetRequiredService<ApifConfiguration>().GetEnforcingSection("Email");

    return new SmtpEmailService(emailConfig);
});
```

```csharp
public sealed class SmtpEmailService : IEmailService
{
    private readonly string host;
    private readonly int    port;
    private readonly string fromAddress;

    public SmtpEmailService(ApifConfiguration emailConfig)
    {
        // Keys are relative to the "Email" section.
        this.host        = emailConfig.Get("Host");
        this.port        = emailConfig.GetValue<int>("Port");
        this.fromAddress = emailConfig.Get("FromAddress");
    }
}
```

```json
// appsettings.json
{
  "Email": {
    "Host": "smtp.example.com",
    "Port": 587,
    "FromAddress": "no-reply@example.com"
  }
}
```

---

## Core concept: enforcing vs optional access

`ApifConfiguration` wraps any `IConfiguration` or `IConfigurationSection` and enforces that every resolved value is present and non-empty. The `.Optional` property exposes the same surface area but returns `null` (or `default(T)`) instead of throwing.

| Access | Missing / empty value |
|---|---|
| `config.Get(key)` | Throws `ConfigurationKeyNotFoundException` |
| `config.Optional.Get(key)` | Returns `null` |
| `config["key"]` | Throws `ConfigurationKeyNotFoundException` |
| `config.Optional["key"]` | Returns `null` |
| `config.GetValue<T>(key)` | Throws `ConfigurationKeyNotFoundException` |
| `config.Optional.GetValue<T>(key)` | Returns `default(T)` |
| `config.GetValue(key, defaultValue)` | Throws `ConfigurationKeyNotFoundException` |
| `config.Optional.GetValue(key, defaultValue)` | Returns `defaultValue` |
| `config.GetSection(key)` | Throws `ConfigurationKeyNotFoundException` |
| `config.Optional.GetSection(key)` | Returns `null` |
| `config.GetConnectionString(name)` | Throws `ConfigurationKeyNotFoundException` |
| `config.Optional.GetConnectionString(name)` | Returns `null` |
| `config.Bind(sectionKey, obj)` | Throws `ConfigurationKeyNotFoundException` |
| `config.Optional.Bind(sectionKey, obj)` | Does nothing |

---

## API reference

### `ApifConfiguration`

Implements `IConfigurationSection`. Construct it by passing any `IConfiguration`:

```csharp
var config = new ApifConfiguration(configuration);
```

#### Properties

| Member | Description |
|---|---|
| `Optional` | Non-enforcing accessor — see [`OptionalConfiguration`](#optionalconfiguration) |
| `Key` | Section key, or `string.Empty` when wrapping a root `IConfigurationRoot` |
| `Path` | Section path, or `string.Empty` when wrapping a root `IConfigurationRoot` |
| `Value` | Section scalar value |
| `this[string key]` | Gets the value for `key`; **throws** if missing or empty. Setter delegates to the underlying configuration |

#### Methods

```csharp
// Returns the string value for key. Throws if missing or empty.
string Get(string key)

// Binds the entire configuration to T. Returns null when no keys match.
T? Get<T>()

// Returns the typed value for key. Throws if missing or empty.
T? GetValue<T>(string key)

// Returns the typed value for key. Throws if the key is absent or empty.
// defaultValue is used only when the key is present but binding returns null.
// To fall back silently on a missing key use config.Optional.GetValue<T>(key, defaultValue).
T GetValue<T>(string key, T defaultValue)

// Populates instance from the root of this configuration.
void Bind(object instance)

// Populates instance from the named section. Throws if the section is missing.
void Bind(string sectionKey, object instance)

// Returns an enforcing ApifConfiguration for the named section. Throws if missing.
IConfigurationSection GetSection(string key)

// Returns an enforcing ApifConfiguration for the named section, wrapping any
// InvalidOperationException from the underlying call in ConfigurationKeyNotFoundException.
ApifConfiguration GetRequiredSection(string key)

// Returns the immediate child sections as enforcing ApifConfiguration instances.
IEnumerable<IConfigurationSection> GetChildren()

// Returns the immediate child sections as ApifConfiguration — no cast needed.
IEnumerable<ApifConfiguration> GetEnforcingChildren()

// Returns the named section as ApifConfiguration — no cast needed. Throws if missing.
ApifConfiguration GetEnforcingSection(string key)

// Returns the connection string for name. Throws if missing or empty.
string GetConnectionString(string name)

// Returns the change token from the underlying configuration.
IChangeToken GetReloadToken()
```

---

### `OptionalConfiguration`

Accessed via `config.Optional`. All members return `null` or `default(T)` instead of throwing.

#### Members

```csharp
// Returns the value for key, or null if missing or empty.
string? this[string key]

// Returns the value for key, or null if missing or empty.
string? Get(string key)

// Returns the typed value for key, or default(T) if missing or empty.
T? GetValue<T>(string key)

// Returns the typed value for key, or defaultValue if the key is missing, empty, or unparseable.
T GetValue<T>(string key, T defaultValue)

// Returns an enforcing ApifConfiguration for the named section, or null if missing.
ApifConfiguration? GetSection(string key)

// Returns an enforcing ApifConfiguration for the required section, or null if missing.
ApifConfiguration? GetRequiredSection(string key)

// Returns an enforcing ApifConfiguration for the named section, or null if missing.
ApifConfiguration? GetEnforcingSection(string key)

// Returns the connection string for name, or null if missing or empty.
string? GetConnectionString(string name)

// Populates instance from the named section. Does nothing if the section is missing.
void Bind(string sectionKey, object instance)
```

---

### `ConfigurationKeyNotFoundException`

Thrown by all enforcing members. Inherits from `Exception` and adds a `Key` property.

```csharp
public sealed class ConfigurationKeyNotFoundException : Exception
{
    // The key that was missing or had no value.
    public string Key { get; }
}
```

Catch it to handle missing configuration at a specific call site:

```csharp
try
{
    var section = config.GetSection("Logging");
}
catch (ConfigurationKeyNotFoundException ex)
{
    Console.Error.WriteLine($"Missing config key: {ex.Key}");
}
```

---

## Usage examples

### Scalar values

```csharp
// Enforcing — throws if absent or empty.
string host = config.Get("Database:Host");
int port     = config.GetValue<int>("Database:Port");
bool tls     = config.GetValue<bool>("Database:UseTls");

// Optional — null / default when absent.
string? overrideHost = config.Optional.Get("Database:HostOverride");
int     defaultPort  = config.Optional.GetValue("Database:Port", 5432);
```

### Binding to a settings class

```csharp
public class DatabaseSettings
{
    public string Host    { get; set; } = string.Empty;
    public int    Port    { get; set; }
    public bool   UseTls  { get; set; }
}

// Bind from a named section — throws if "Database" section is absent.
var db = new DatabaseSettings();
config.Bind("Database", db);

// Or bind via Get<T>():
DatabaseSettings? db2 = config.GetSection("Database").Get<DatabaseSettings>();
```

### Optional section binding

```csharp
// Does nothing when "Metrics" section is absent.
var metrics = new MetricsSettings();
config.Optional.Bind("Metrics", metrics);

// Null when section is absent.
ApifConfiguration? metricsSection = config.Optional.GetSection("Metrics");
if (metricsSection is not null)
{
    string endpoint = metricsSection.Get("Endpoint");
}
```

### Connection strings

```csharp
// Enforcing.
string connStr = config.GetConnectionString("Default");

// Optional.
string? readonlyConn = config.Optional.GetConnectionString("ReadOnly");
```

### Indexer

```csharp
// Read — throws if absent or empty.
string value = config["AppSettings:Theme"]!;

// Write — delegates to the underlying IConfiguration.
config["AppSettings:Theme"] = "dark";
```

### Working with children

```csharp
// GetEnforcingChildren() returns ApifConfiguration directly — no cast needed.
foreach (ApifConfiguration child in config.GetEnforcingChildren())
{
    string name = child.Key;
    string val  = child.Get("Value");
}

// GetChildren() satisfies IConfiguration and returns IConfigurationSection.
foreach (IConfigurationSection section in config.GetChildren())
{
    Console.WriteLine(section.Key);
}
```

---

## Error handling pattern

A common pattern is to validate all required keys at startup so that errors surface immediately rather than at the point of first use:

```csharp
var config = new ApifConfiguration(
    new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build());

// Validate eagerly — throws on the first missing key.
string host   = config.Get("Database:Host");
int    port   = config.GetValue<int>("Database:Port");
string secret = config.Get("Auth:Secret");
```

---

## License

MIT

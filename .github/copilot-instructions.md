# Copilot Instructions for ApifConfiguration

## Project Overview
ApifConfiguration is a strongly-typed, enforcing wrapper around `Microsoft.Extensions.Configuration` for .NET 10. It provides a fail-fast approach to configuration access with optional fallback modes.

- **Language version**: C# 14.0
- **Target framework**: .NET 10
- **Repository**: https://github.com/WilkinGlen/ApifConfiguration

## Code Style & Conventions

### C# Guidelines
- **Always add braces to `if` statements**, even single-line conditions
  ```csharp
  // Good
  if (this.section is null)
  {
      throw new InvalidOperationException(...);
  }
  
  // Bad
  if (this.section is null)
      throw new InvalidOperationException(...);
  ```
- **Use file-scoped namespaces** (e.g., `namespace ApifConfiguration;`) - already enforced in .editorconfig
- **Use sealed classes** (e.g., `public sealed class`) for immutability and performance

### Field Declarations
- **Use private fields without underscore prefix**
  ```csharp
  // Good
  private readonly IConfiguration configuration;
  
  // Bad
  private readonly IConfiguration _configuration;
  ```
- **Always qualify all member access with `this.`** for clarity (fields, properties, methods, indexers)
  ```csharp
  // Good
  return this.Get(key);
  var value = this.configuration[key];
  var section = this.GetEnforcingSection(key);
  var optional = this.Optional;
  
  // Bad
  return Get(key);
  var value = configuration[key];
  var section = GetEnforcingSection(key);
  var optional = Optional;
  ```

- **Use `ArgumentException.ThrowIfNullOrEmpty()`** for string parameters
- **Use `ArgumentNullException.ThrowIfNull()`** for object parameters

### Parameter Validation & Exception Handling

#### Validation Order
1. **String parameters**: Always validate first with `ArgumentException.ThrowIfNullOrEmpty(key)`
2. **Object parameters**: Always validate first with `ArgumentNullException.ThrowIfNull(instance)`
3. Perform validation at method entry before any business logic

```csharp
// Good - validation first, then logic
public string Get(string key)
{
    ArgumentException.ThrowIfNullOrEmpty(key);  // Validate immediately
    var value = this.configuration[key];        // Then execute logic
    return string.IsNullOrEmpty(value) ? throw new ConfigurationKeyNotFoundException(key) : value;
}

// Bad - logic before validation
public string Get(string key)
{
    var value = this.configuration[key];
    ArgumentException.ThrowIfNullOrEmpty(key);  // Too late
    return value;
}
```

#### Exception Throwing Patterns
- **Prefer ternary throw expressions** over if-blocks for concise, readable code (enforced as error):
```csharp
  // Good - ternary throw expression
  return string.IsNullOrEmpty(value)
      ? throw new ConfigurationKeyNotFoundException(key)
      : value;
  
  // Avoid - unnecessary if-block
  if (string.IsNullOrEmpty(value))
  {
      throw new ConfigurationKeyNotFoundException(key);
  }
  return value;
```

#### Exception Wrapping
- **Always preserve inner exceptions** when catching and re-throwing for better diagnostics:
```csharp
  // Good - inner exception preserved
  try
  {
      return configSection.Get<T>();
  }
  catch (InvalidOperationException ex)
  {
      throw new ConfigurationKeyNotFoundException(key, ex);
  }
  
  // Bad - exception context lost
  try
  {
      return configSection.Get<T>();
  }
  catch (InvalidOperationException ex)
  {
      throw new ConfigurationKeyNotFoundException(key);
  }
```
- **Catch only specific exceptions** (`InvalidOperationException`) when handling them
- **Never silently swallow exceptions** without explicit documented fallback behavior

#### Pattern Matching
- **Prefer pattern matching** over null comparison operators (`== null`) - enforced as suggestion:
```csharp
  // Good - pattern matching
  if (this.section is null)
  {
      throw new InvalidOperationException(...);
  }
  
  // Avoid - null comparison
  if (this.section == null)
  {
      throw new InvalidOperationException(...);
  }
```

#### Sealed Classes
- **Always seal classes** unless explicitly designed for inheritance (improves safety and performance) - enforced as suggestion:
```csharp
  // Good - sealed
  public sealed class ApifConfiguration : IConfigurationSection, IDisposable
  
  // Avoid - unsealed unless inheritance is intentional
  public class ApifConfiguration : IConfigurationSection, IDisposable
```

### Naming & File Organization
- **Class names must match filenames**
  - `OptionalConfiguration` class → `OptionalConfiguration.cs` file
  - `ApifConfiguration` class → `ApifConfiguration.cs` file
  - `ConfigurationKeyNotFoundException` class → `ConfigurationKeyNotFoundException.cs` file
- When renaming a class, update the filename to match

## Architecture & Design Principles

### Method Delegation (DRY Principle)
- **Eliminate code duplication** by delegating to a single core implementation
- Identify the method with the complete logic and have other methods call it
- Example pattern from this codebase:
  ```csharp
  // Core implementation with full logic
  public ApifConfiguration GetEnforcingSection(string key)
  {
      ArgumentException.ThrowIfNullOrEmpty(key);
      var configSection = this.configuration.GetSection(key);
      return configSection.Exists() ? new ApifConfiguration(configSection) : throw new ConfigurationKeyNotFoundException(key);
  }
  
  // Lightweight wrapper that delegates
  public IConfigurationSection GetSection(string key)
  {
      return this.GetEnforcingSection(key);
  }
  
  public ApifConfiguration GetRequiredSection(string key)
  {
      return this.GetEnforcingSection(key);
  }
  ```

### Exception Handling
- **Throw `ConfigurationKeyNotFoundException`** for missing or empty configuration values
- **Preserve inner exceptions** when wrapping lower-level errors
  ```csharp
  try
  {
      return configSection.Get<T>();
  }
  catch (InvalidOperationException ex)
  {
      throw new ConfigurationKeyNotFoundException(key, ex);
  }
  ```
- **Avoid unnecessary try/catch**; only catch `InvalidOperationException` when needed
- **No exception swallowing** without explicit documentation

### Lazy Initialization
- **Use field initializers with `??=` operator** for lazy-allocated singleton access
  ```csharp
  public OptionalConfiguration Optional => field ??= new OptionalConfiguration(this.configuration);
  ```
  
## Testing Requirements

### Mandatory Test Execution
- **Always run all tests after making changes** in the ApifConfiguration workspace
- Use the test runner with project filter: `ApifConfigurationTests`
- All tests in `ApifConfigurationTests` must pass (currently 168+ tests)
- Command pattern: Run tests after every refactoring or feature addition

### Test-Driven Refactoring
- When changing implementation details (e.g., introducing delegation patterns):
  - **Update tests that validate the public contract** to reflect the new behavior
  - **Remove tests that verify implementation internals** (e.g., tests checking for specific exception wrapping if the implementation changes how that wrapping occurs)
  - Keep all tests that verify the **observable behavior** the API promises to users
  
Example: When `GetRequiredSection` was refactored to delegate to `GetEnforcingSection`, we removed the test `WrapInvalidOperationException_WhenSectionIsMissing` because it verified an implementation detail (presence of an inner exception). The meaningful contract test `ThrowConfigurationKeyNotFoundException_WhenSectionIsMissing` remained.

### Test Organization
- Test files follow the pattern: `[MethodName]_Should.cs`
- Test classes: `[MethodName]_Should`
- Test methods: `[Behavior]_[WhenCondition]`

## Documentation

### XML Documentation Comments
- Include `<summary>`, `<param>`, `<returns>`, and `<exception>` tags for all public members
- Use `<see cref>` tags for cross-references to types and members
- **Always update `<see cref>` references when renaming types**
  - Example: When renaming `OptionalConfigurationWrapper` → `OptionalConfiguration`, search for all `<see cref="OptionalConfigurationWrapper` and update to `<see cref="OptionalConfiguration`

### README Alignment
- The **README.md is the source of truth** for public API documentation
- When renaming or changing public API:
  1. Update the class declaration and filename in code
  2. Update all `<see cref>` references in XML docs
  3. Update the README.md with new class names or API changes
  4. Verify README examples still compile and are accurate

## Performance Considerations
- Configuration access is **not** a hot path
- **Prioritize clarity and correctness** over micro-optimizations
- Avoid unnecessary caching; let the underlying `IConfiguration` handle it
- Delegation patterns are preferred even with trivial performance costs—correctness and maintainability win

## Common Architectural Patterns in This Codebase

### Public API Structure
- **`ApifConfiguration`** — enforcing wrapper, throws on missing/empty values
- **`ApifConfiguration.Optional`** — non-enforcing accessor via `OptionalConfiguration`, returns `null`/`default(T)`
- **`ConfigurationKeyNotFoundException`** — specific exception for configuration errors

### Section Retrieval Pattern
- **Core**: `GetEnforcingSection(string key)` — contains full logic
- **Delegates**: `GetSection(string key)` → `GetEnforcingSection(key)`
- **Delegates**: `GetRequiredSection(string key)` → `GetEnforcingSection(key)`

### Child Enumeration Pattern
- **Core**: `GetEnforcingChildren()` — contains iterator with `yield return`
- **Delegates**: `GetChildren()` → `GetEnforcingChildren()`

### Indexer Pattern
- **Core**: `Get(string key)` — contains null/empty validation logic
- **Delegates**: `this[string key]` (getter) → `Get(key)`
- **Setter**: Direct access to `this.configuration[key]`

## Git Workflow

### Commit Messages
- Be descriptive: "Refactor: Delegate GetSection to GetEnforcingSection to eliminate duplication"
- Reference the specific goal (DRY, bug fix, feature, etc.)

### Branch Strategy
- Primary branch: `master`
- Ensure all tests pass before pushing

## Resources & References
- **README.md**: Complete public API documentation, usage patterns, and examples
- **Test suite** (`ApifConfigurationTests`): Regression testing and real-world usage patterns
- **Implementation files**: `ApifConfiguration.cs`, `OptionalConfiguration.cs`, `ConfigurationKeyNotFoundException.cs`
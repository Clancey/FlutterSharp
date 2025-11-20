# Configuration System

This directory contains the configuration system for the FlutterSharp code generator.

## Files

### GeneratorConfig.cs
Main configuration class hierarchy containing all configuration options:

- **FlutterSdkConfig**: Flutter SDK management (auto/local/clone modes)
- **OutputPathsConfig**: Output directory paths for generated code
- **FiltersConfig**: Include/exclude patterns for packages, widgets, types, and namespaces
- **ThirdPartyPackagesConfig**: Configuration for third-party package sources (pub.dev, Git, local)
- **GenerationOptionsConfig**: Code generation options (namespace, documentation, C# features)
- **TypeMappingConfig**: Custom type mappings and mapping behaviors
- **AdvancedOptionsConfig**: Performance, caching, logging, and advanced features

### ConfigLoader.cs
Utility class for loading, validating, and managing configurations:

- Load from JSON files
- Merge configurations
- Apply command-line overrides
- Validate configuration
- Create default configuration files

### generator-config.json
Example configuration file showing all available options with comments.

## Usage

### Basic Loading

```csharp
using FlutterSharp.CodeGen.Config;

// Load from default location (searches current and parent directories)
var config = ConfigLoader.Load();

// Load from specific path
var config = ConfigLoader.Load("path/to/generator-config.json");

// Get default configuration
var config = ConfigLoader.GetDefaultConfig();
```

### Loading with Command-Line Overrides

```csharp
var overrides = new Dictionary<string, object>
{
    ["FlutterSdk.Mode"] = "local",
    ["FlutterSdk.Path"] = "/custom/flutter/path",
    ["OutputPaths.CSharpOutput"] = "./output/csharp",
    ["AdvancedOptions.LogLevel"] = "Debug"
};

var config = ConfigLoader.LoadWithOverrides("generator-config.json", overrides);
```

### Merging Configurations

```csharp
// Load base configuration
var baseConfig = ConfigLoader.Load("base-config.json");

// Load override configuration
var overrideConfig = ConfigLoader.Load("override-config.json");

// Merge (override takes precedence)
var merged = ConfigLoader.Merge(baseConfig, overrideConfig);
```

### Creating a Default Configuration File

```csharp
// Create in current directory
ConfigLoader.CreateDefaultConfigFile();

// Create in specific location
ConfigLoader.CreateDefaultConfigFile("path/to/generator-config.json");
```

### Saving Configuration

```csharp
var config = new GeneratorConfig
{
    FlutterSdk = new FlutterSdkConfig
    {
        Mode = "local",
        Path = "/path/to/flutter"
    },
    OutputPaths = new OutputPathsConfig
    {
        CSharpOutput = "./Generated/CSharp",
        DartOutput = "./Generated/Dart"
    }
};

ConfigLoader.Save(config, "my-config.json");
```

## Configuration Options

### Flutter SDK Modes

- **auto**: Automatically detect Flutter SDK from PATH
- **local**: Use a specific Flutter SDK installation
- **clone**: Clone Flutter SDK from Git to a local directory

### Filter Patterns

Filters support wildcard patterns:
- `*` matches any characters
- `?` matches a single character
- Examples: `*Internal*`, `Test*`, `*_private`

### Stability Levels

Minimum stability levels (from least to most stable):
- `alpha`: Include all APIs
- `beta`: Exclude alpha APIs
- `rc`: Exclude alpha and beta APIs
- `stable`: Only stable APIs (default)

### Log Levels

Available log levels (from most to least verbose):
- `Trace`: All messages including detailed traces
- `Debug`: Debug and higher
- `Information`: Informational messages and higher (default)
- `Warning`: Warnings and errors only
- `Error`: Errors and critical only
- `Critical`: Critical errors only

## Example Configuration Scenarios

### Minimal Configuration

```json
{
  "flutterSdk": {
    "mode": "auto"
  },
  "outputPaths": {
    "csharpOutput": "./Generated/CSharp"
  }
}
```

### Local Flutter SDK

```json
{
  "flutterSdk": {
    "mode": "local",
    "path": "/Users/username/flutter"
  }
}
```

### Clone Specific Flutter Version

```json
{
  "flutterSdk": {
    "mode": "clone",
    "version": "3.16.0",
    "cloneDirectory": "./.cache/flutter-sdk"
  }
}
```

### Filter Specific Packages

```json
{
  "filters": {
    "includePackages": ["flutter", "material"],
    "excludeWidgets": ["*Internal*", "*Debug*"]
  }
}
```

### Include Third-Party Packages

```json
{
  "thirdPartyPackages": {
    "enabled": true,
    "pubDevPackages": [
      {
        "name": "provider",
        "version": "^6.0.0",
        "enabled": true
      }
    ]
  }
}
```

### Development Configuration

```json
{
  "generationOptions": {
    "generateDocumentation": true,
    "generateDebuggerAttributes": true
  },
  "advancedOptions": {
    "logLevel": "Debug",
    "emitPerformanceDiagnostics": true,
    "validateGeneratedCode": true
  }
}
```

### Production/Performance Configuration

```json
{
  "advancedOptions": {
    "parallelGeneration": true,
    "maxDegreeOfParallelism": 8,
    "incrementalGeneration": true,
    "enableCaching": true,
    "formatGeneratedCode": false,
    "logLevel": "Warning"
  }
}
```

## Validation

The `ConfigLoader.ValidateConfig()` method validates:

- Flutter SDK mode is valid (auto/local/clone)
- Required paths exist when specified
- Output directories are configured
- Numeric values are within valid ranges
- Enum values are valid
- Package sources have required fields

Validation errors will throw an `InvalidOperationException` with detailed error messages.

## Best Practices

1. **Start with defaults**: Use `generator-config.json` as a template
2. **Version control**: Commit your configuration file to version control
3. **Environment-specific configs**: Use different config files for dev/staging/prod
4. **Override carefully**: Use command-line overrides for temporary changes only
5. **Validate early**: Load and validate configuration at application startup
6. **Document changes**: Add comments to explain non-obvious configuration choices
7. **Use relative paths**: Keep paths relative to the config file location when possible

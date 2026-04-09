using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlutterSharp.CodeGen.Config
{
	/// <summary>
	/// Loads and validates configuration from JSON files.
	/// </summary>
	public static class ConfigLoader
	{
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNameCaseInsensitive = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true,
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			Converters =
			{
				new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
			}
		};

		/// <summary>
		/// Default configuration file name.
		/// </summary>
		public const string DefaultConfigFileName = "generator-config.json";

		/// <summary>
		/// Loads configuration from a JSON file.
		/// </summary>
		/// <param name="configPath">Path to the configuration file. If null, looks for default file.</param>
		/// <returns>Loaded configuration or default configuration if file not found.</returns>
		/// <exception cref="InvalidOperationException">Thrown when configuration file is invalid.</exception>
		public static GeneratorConfig Load(string? configPath = null)
		{
			var path = ResolveConfigPath(configPath);

			if (path == null)
			{
				Console.WriteLine("No configuration file found. Using default configuration.");
				return GetDefaultConfig();
			}

			try
			{
				var json = File.ReadAllText(path);
				var config = JsonSerializer.Deserialize<GeneratorConfig>(json, JsonOptions);

				if (config == null)
				{
					throw new InvalidOperationException($"Failed to deserialize configuration from '{path}'");
				}

				Console.WriteLine($"Loaded configuration from: {path}");
				return ValidateConfig(config);
			}
			catch (JsonException ex)
			{
				throw new InvalidOperationException($"Invalid JSON in configuration file '{path}': {ex.Message}", ex);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Error loading configuration from '{path}': {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Loads configuration and merges with command-line options.
		/// </summary>
		/// <param name="configPath">Path to the configuration file.</param>
		/// <param name="overrides">Dictionary of configuration overrides from command-line.</param>
		/// <returns>Merged configuration.</returns>
		public static GeneratorConfig LoadWithOverrides(string? configPath, Dictionary<string, object>? overrides)
		{
			var config = Load(configPath);

			if (overrides == null || overrides.Count == 0)
			{
				return config;
			}

			// Apply overrides using builder pattern
			return ApplyOverrides(config, overrides);
		}

		/// <summary>
		/// Saves configuration to a JSON file.
		/// </summary>
		/// <param name="config">Configuration to save.</param>
		/// <param name="outputPath">Path where to save the file.</param>
		public static void Save(GeneratorConfig config, string outputPath)
		{
			try
			{
				var json = JsonSerializer.Serialize(config, JsonOptions);
				var directory = Path.GetDirectoryName(outputPath);

				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				File.WriteAllText(outputPath, json);
				Console.WriteLine($"Configuration saved to: {outputPath}");
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Error saving configuration to '{outputPath}': {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Creates and saves a default configuration file.
		/// </summary>
		/// <param name="outputPath">Path where to save the file. Defaults to current directory.</param>
		public static void CreateDefaultConfigFile(string? outputPath = null)
		{
			var path = outputPath ?? Path.Combine(Directory.GetCurrentDirectory(), DefaultConfigFileName);
			var config = GetDefaultConfig();
			Save(config, path);
		}

		/// <summary>
		/// Validates the configuration and returns it if valid.
		/// </summary>
		/// <param name="config">Configuration to validate.</param>
		/// <returns>The validated configuration.</returns>
		/// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
		public static GeneratorConfig ValidateConfig(GeneratorConfig config)
		{
			var errors = new List<string>();

			// Validate Flutter SDK configuration
			if (config.FlutterSdk.Mode != "auto" &&
			    config.FlutterSdk.Mode != "local" &&
			    config.FlutterSdk.Mode != "clone")
			{
				errors.Add($"Invalid FlutterSdk.Mode: '{config.FlutterSdk.Mode}'. Must be 'auto', 'local', or 'clone'.");
			}

			if (config.FlutterSdk.Mode == "local" && string.IsNullOrWhiteSpace(config.FlutterSdk.Path))
			{
				errors.Add("FlutterSdk.Path is required when Mode is 'local'.");
			}

			if (config.FlutterSdk.Mode == "local" &&
			    !string.IsNullOrWhiteSpace(config.FlutterSdk.Path) &&
			    !Directory.Exists(config.FlutterSdk.Path))
			{
				errors.Add($"FlutterSdk.Path directory does not exist: '{config.FlutterSdk.Path}'");
			}

			// Validate output paths
			if (string.IsNullOrWhiteSpace(config.OutputPaths.BaseDirectory))
			{
				errors.Add("OutputPaths.BaseDirectory cannot be empty.");
			}

			if (string.IsNullOrWhiteSpace(config.OutputPaths.CSharpOutput))
			{
				errors.Add("OutputPaths.CSharpOutput cannot be empty.");
			}

			if (string.IsNullOrWhiteSpace(config.OutputPaths.DartOutput))
			{
				errors.Add("OutputPaths.DartOutput cannot be empty.");
			}

			// Validate filters
			var validStabilities = new[] { "alpha", "beta", "rc", "stable" };
			if (!Array.Exists(validStabilities, s => s.Equals(config.Filters.MinimumStability, StringComparison.OrdinalIgnoreCase)))
			{
				errors.Add($"Invalid Filters.MinimumStability: '{config.Filters.MinimumStability}'. Must be one of: {string.Join(", ", validStabilities)}");
			}

			// Validate third-party packages
			if (config.ThirdPartyPackages.Enabled)
			{
				foreach (var pkg in config.ThirdPartyPackages.PubDevPackages)
				{
					if (string.IsNullOrWhiteSpace(pkg.Name))
					{
						errors.Add("PubDevPackages entry has empty Name.");
					}
				}

				foreach (var pkg in config.ThirdPartyPackages.GitPackages)
				{
					if (string.IsNullOrWhiteSpace(pkg.Url))
					{
						errors.Add("GitPackages entry has empty Url.");
					}
				}

				foreach (var pkg in config.ThirdPartyPackages.LocalPackages)
				{
					if (string.IsNullOrWhiteSpace(pkg.Path))
					{
						errors.Add("LocalPackages entry has empty Path.");
					}
				}
			}

			// Validate generation options
			if (config.GenerationOptions.IndentationSize < 1 || config.GenerationOptions.IndentationSize > 8)
			{
				errors.Add($"GenerationOptions.IndentationSize must be between 1 and 8, got {config.GenerationOptions.IndentationSize}");
			}

			var validLineEndings = new[] { "lf", "crlf", "auto" };
			if (!Array.Exists(validLineEndings, e => e.Equals(config.GenerationOptions.LineEnding, StringComparison.OrdinalIgnoreCase)))
			{
				errors.Add($"Invalid GenerationOptions.LineEnding: '{config.GenerationOptions.LineEnding}'. Must be one of: {string.Join(", ", validLineEndings)}");
			}

			var validIndentStyles = new[] { "tabs", "spaces" };
			if (!Array.Exists(validIndentStyles, s => s.Equals(config.GenerationOptions.IndentationStyle, StringComparison.OrdinalIgnoreCase)))
			{
				errors.Add($"Invalid GenerationOptions.IndentationStyle: '{config.GenerationOptions.IndentationStyle}'. Must be one of: {string.Join(", ", validIndentStyles)}");
			}

			// Validate advanced options
			if (config.AdvancedOptions.MaxDegreeOfParallelism < -1 || config.AdvancedOptions.MaxDegreeOfParallelism == 0)
			{
				errors.Add($"AdvancedOptions.MaxDegreeOfParallelism must be -1 or positive, got {config.AdvancedOptions.MaxDegreeOfParallelism}");
			}

			if (config.AdvancedOptions.ToolExecutionTimeout < 1)
			{
				errors.Add($"AdvancedOptions.ToolExecutionTimeout must be positive, got {config.AdvancedOptions.ToolExecutionTimeout}");
			}

			if (config.AdvancedOptions.MaxErrors < 1)
			{
				errors.Add($"AdvancedOptions.MaxErrors must be positive, got {config.AdvancedOptions.MaxErrors}");
			}

			var validLogLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical" };
			if (!Array.Exists(validLogLevels, l => l.Equals(config.AdvancedOptions.LogLevel, StringComparison.OrdinalIgnoreCase)))
			{
				errors.Add($"Invalid AdvancedOptions.LogLevel: '{config.AdvancedOptions.LogLevel}'. Must be one of: {string.Join(", ", validLogLevels)}");
			}

			if (errors.Count > 0)
			{
				throw new InvalidOperationException($"Configuration validation failed:\n  - {string.Join("\n  - ", errors)}");
			}

			return config;
		}

		/// <summary>
		/// Gets the default configuration.
		/// </summary>
		/// <returns>Default configuration instance.</returns>
		public static GeneratorConfig GetDefaultConfig()
		{
			return new GeneratorConfig();
		}

		/// <summary>
		/// Resolves the configuration file path.
		/// </summary>
		/// <param name="configPath">Explicit path or null to search for default.</param>
		/// <returns>Resolved path or null if not found.</returns>
		private static string? ResolveConfigPath(string? configPath)
		{
			// If explicit path provided, use it
			if (!string.IsNullOrWhiteSpace(configPath))
			{
				if (File.Exists(configPath))
				{
					return Path.GetFullPath(configPath);
				}

				throw new FileNotFoundException($"Configuration file not found: {configPath}");
			}

			// Search for default config file in current directory and parent directories
			var currentDir = Directory.GetCurrentDirectory();
			var searchDir = currentDir;

			while (searchDir != null)
			{
				var candidatePath = Path.Combine(searchDir, DefaultConfigFileName);
				if (File.Exists(candidatePath))
				{
					return candidatePath;
				}

				// Move to parent directory
				var parent = Directory.GetParent(searchDir);
				searchDir = parent?.FullName;
			}

			return null;
		}

		/// <summary>
		/// Applies command-line overrides to the configuration.
		/// </summary>
		/// <param name="config">Base configuration.</param>
		/// <param name="overrides">Overrides to apply.</param>
		/// <returns>Configuration with overrides applied.</returns>
		private static GeneratorConfig ApplyOverrides(GeneratorConfig config, Dictionary<string, object> overrides)
		{
			// Create a copy via JSON serialization/deserialization to enable modification
			var json = JsonSerializer.Serialize(config, JsonOptions);
			var mutableConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions);

			if (mutableConfig == null)
			{
				return config;
			}

			// Apply overrides (simple dot-notation path support)
			foreach (var (key, value) in overrides)
			{
				ApplyOverride(mutableConfig, key, value);
			}

			// Deserialize back to strongly-typed config
			var updatedJson = JsonSerializer.Serialize(mutableConfig, JsonOptions);
			var result = JsonSerializer.Deserialize<GeneratorConfig>(updatedJson, JsonOptions);

			return result ?? config;
		}

		/// <summary>
		/// Applies a single override to the configuration dictionary.
		/// </summary>
		/// <param name="config">Configuration dictionary.</param>
		/// <param name="path">Dot-notation path (e.g., "FlutterSdk.Mode").</param>
		/// <param name="value">Value to set.</param>
		private static void ApplyOverride(Dictionary<string, object> config, string path, object value)
		{
			var parts = path.Split('.');
			var current = config;

			for (var i = 0; i < parts.Length - 1; i++)
			{
				var part = parts[i];

				if (!current.ContainsKey(part))
				{
					current[part] = new Dictionary<string, object>();
				}

				if (current[part] is Dictionary<string, object> dict)
				{
					current = dict;
				}
				else
				{
					// Can't traverse further
					return;
				}
			}

			// Set the final value
			current[parts[^1]] = value;
		}

		/// <summary>
		/// Merges two configurations, with the second taking precedence.
		/// </summary>
		/// <param name="baseConfig">Base configuration.</param>
		/// <param name="overrideConfig">Override configuration.</param>
		/// <returns>Merged configuration.</returns>
		public static GeneratorConfig Merge(GeneratorConfig baseConfig, GeneratorConfig overrideConfig)
		{
			// Serialize both to JSON, merge, and deserialize back
			var baseJson = JsonSerializer.Serialize(baseConfig, JsonOptions);
			var overrideJson = JsonSerializer.Serialize(overrideConfig, JsonOptions);

			var baseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(baseJson, JsonOptions);
			var overrideDict = JsonSerializer.Deserialize<Dictionary<string, object>>(overrideJson, JsonOptions);

			if (baseDict == null || overrideDict == null)
			{
				return baseConfig;
			}

			var merged = MergeDictionaries(baseDict, overrideDict);
			var mergedJson = JsonSerializer.Serialize(merged, JsonOptions);
			var result = JsonSerializer.Deserialize<GeneratorConfig>(mergedJson, JsonOptions);

			return result ?? baseConfig;
		}

		/// <summary>
		/// Recursively merges two dictionaries.
		/// </summary>
		private static Dictionary<string, object> MergeDictionaries(
			Dictionary<string, object> baseDict,
			Dictionary<string, object> overrideDict)
		{
			var result = new Dictionary<string, object>(baseDict);

			foreach (var (key, value) in overrideDict)
			{
				if (result.ContainsKey(key) &&
				    result[key] is Dictionary<string, object> baseChild &&
				    value is Dictionary<string, object> overrideChild)
				{
					result[key] = MergeDictionaries(baseChild, overrideChild);
				}
				else
				{
					result[key] = value;
				}
			}

			return result;
		}
	}
}

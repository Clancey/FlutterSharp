using System;
using System.Collections.Generic;

namespace FlutterSharp.CodeGen.Config
{
	/// <summary>
	/// Example usage of the configuration system.
	/// This class demonstrates various ways to load, modify, and use configurations.
	/// </summary>
	public static class ConfigExample
	{
		/// <summary>
		/// Demonstrates basic configuration loading.
		/// </summary>
		public static void BasicLoading()
		{
			Console.WriteLine("=== Basic Configuration Loading ===\n");

			// Load from default location
			var config = ConfigLoader.Load();

			Console.WriteLine($"Flutter SDK Mode: {config.FlutterSdk.Mode}");
			Console.WriteLine($"C# Output Path: {config.OutputPaths.CSharpOutput}");
			Console.WriteLine($"Namespace Prefix: {config.GenerationOptions.NamespacePrefix}");
			Console.WriteLine($"Parallel Generation: {config.AdvancedOptions.ParallelGeneration}");
			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates loading with command-line overrides.
		/// </summary>
		public static void LoadingWithOverrides()
		{
			Console.WriteLine("=== Loading with Command-Line Overrides ===\n");

			var overrides = new Dictionary<string, object>
			{
				["FlutterSdk.Mode"] = "local",
				["FlutterSdk.Path"] = "/custom/flutter/path",
				["OutputPaths.CSharpOutput"] = "./CustomOutput/CSharp",
				["AdvancedOptions.LogLevel"] = "Debug",
				["GenerationOptions.GenerateDocumentation"] = false
			};

			var config = ConfigLoader.LoadWithOverrides(null, overrides);

			Console.WriteLine($"Flutter SDK Mode: {config.FlutterSdk.Mode}");
			Console.WriteLine($"Flutter SDK Path: {config.FlutterSdk.Path}");
			Console.WriteLine($"C# Output: {config.OutputPaths.CSharpOutput}");
			Console.WriteLine($"Log Level: {config.AdvancedOptions.LogLevel}");
			Console.WriteLine($"Generate Documentation: {config.GenerationOptions.GenerateDocumentation}");
			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates creating a custom configuration programmatically.
		/// </summary>
		public static void ProgrammaticConfiguration()
		{
			Console.WriteLine("=== Programmatic Configuration ===\n");

			var config = new GeneratorConfig
			{
				FlutterSdk = new FlutterSdkConfig
				{
					Mode = "clone",
					Version = "3.16.0",
					CloneDirectory = "./.cache/flutter-sdk"
				},
				OutputPaths = new OutputPathsConfig
				{
					BaseDirectory = "./Generated",
					CSharpOutput = "./Generated/CSharp",
					DartOutput = "./Generated/Dart",
					DocumentationOutput = "./Generated/Docs",
					OrganizeByPackage = true,
					CleanBeforeGenerate = true
				},
				Filters = new FiltersConfig
				{
					IncludePackages = new List<string> { "flutter", "material" },
					ExcludeWidgets = new List<string> { "*Internal*", "*Debug*" },
					IncludeDeprecated = false,
					MinimumStability = "stable"
				},
				GenerationOptions = new GenerationOptionsConfig
				{
					NamespacePrefix = "MyApp.Flutter",
					GenerateDocumentation = true,
					GenerateNullableAnnotations = true,
					CSharpVersion = "12",
					UseFileScopedNamespaces = true
				},
				AdvancedOptions = new AdvancedOptionsConfig
				{
					ParallelGeneration = true,
					MaxDegreeOfParallelism = 4,
					EnableCaching = true,
					FormatGeneratedCode = true,
					LogLevel = "Information"
				}
			};

			// Save to file
			ConfigLoader.Save(config, "custom-config.json");

			Console.WriteLine("Custom configuration created and saved to custom-config.json");
			Console.WriteLine($"Namespace: {config.GenerationOptions.NamespacePrefix}");
			Console.WriteLine($"Packages: {string.Join(", ", config.Filters.IncludePackages)}");
			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates merging multiple configurations.
		/// </summary>
		public static void MergingConfigurations()
		{
			Console.WriteLine("=== Merging Configurations ===\n");

			// Base configuration
			var baseConfig = new GeneratorConfig
			{
				FlutterSdk = new FlutterSdkConfig { Mode = "auto" },
				GenerationOptions = new GenerationOptionsConfig
				{
					NamespacePrefix = "FlutterSharp",
					GenerateDocumentation = true
				}
			};

			// Override configuration
			var overrideConfig = new GeneratorConfig
			{
				FlutterSdk = new FlutterSdkConfig
				{
					Mode = "local",
					Path = "/custom/flutter"
				},
				GenerationOptions = new GenerationOptionsConfig
				{
					NamespacePrefix = "MyApp.Flutter"
					// GenerateDocumentation not specified, will use base value
				}
			};

			var merged = ConfigLoader.Merge(baseConfig, overrideConfig);

			Console.WriteLine($"Flutter SDK Mode (overridden): {merged.FlutterSdk.Mode}");
			Console.WriteLine($"Flutter SDK Path (from override): {merged.FlutterSdk.Path}");
			Console.WriteLine($"Namespace (overridden): {merged.GenerationOptions.NamespacePrefix}");
			Console.WriteLine($"Generate Docs (from base): {merged.GenerationOptions.GenerateDocumentation}");
			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates validation and error handling.
		/// </summary>
		public static void ValidationExample()
		{
			Console.WriteLine("=== Configuration Validation ===\n");

			// Create an invalid configuration
			var invalidConfig = new GeneratorConfig
			{
				FlutterSdk = new FlutterSdkConfig
				{
					Mode = "invalid_mode" // Invalid mode
				},
				OutputPaths = new OutputPathsConfig
				{
					BaseDirectory = "", // Empty directory
					CSharpOutput = ""   // Empty directory
				},
				AdvancedOptions = new AdvancedOptionsConfig
				{
					MaxDegreeOfParallelism = 0, // Invalid value
					LogLevel = "InvalidLevel"   // Invalid log level
				}
			};

			try
			{
				ConfigLoader.ValidateConfig(invalidConfig);
				Console.WriteLine("Configuration is valid.");
			}
			catch (InvalidOperationException ex)
			{
				Console.WriteLine("Configuration validation failed:");
				Console.WriteLine(ex.Message);
			}

			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates third-party package configuration.
		/// </summary>
		public static void ThirdPartyPackagesExample()
		{
			Console.WriteLine("=== Third-Party Packages Configuration ===\n");

			var config = new GeneratorConfig
			{
				ThirdPartyPackages = new ThirdPartyPackagesConfig
				{
					Enabled = true,
					PubDevPackages = new List<PubPackageSource>
					{
						new() { Name = "provider", Version = "^6.0.0", Enabled = true },
						new() { Name = "riverpod", Version = "^2.0.0", Enabled = true },
						new() { Name = "get_it", Version = "^7.0.0", Enabled = false }
					},
					GitPackages = new List<GitPackageSource>
					{
						new()
						{
							Url = "https://github.com/example/flutter-package.git",
							Ref = "main",
							Name = "example_package",
							Enabled = true
						}
					},
					LocalPackages = new List<LocalPackageSource>
					{
						new()
						{
							Path = "/path/to/local/package",
							Name = "local_package",
							Enabled = true
						}
					},
					CacheDirectory = "./.cache/packages",
					EnableCaching = true,
					CacheExpirationHours = 24
				}
			};

			Console.WriteLine($"Third-party packages enabled: {config.ThirdPartyPackages.Enabled}");
			Console.WriteLine($"Pub.dev packages: {config.ThirdPartyPackages.PubDevPackages.Count}");
			Console.WriteLine($"Git packages: {config.ThirdPartyPackages.GitPackages.Count}");
			Console.WriteLine($"Local packages: {config.ThirdPartyPackages.LocalPackages.Count}");
			Console.WriteLine();

			foreach (var pkg in config.ThirdPartyPackages.PubDevPackages)
			{
				var status = pkg.Enabled ? "enabled" : "disabled";
				Console.WriteLine($"  - {pkg.Name} ({pkg.Version}) [{status}]");
			}

			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates all filter options.
		/// </summary>
		public static void FilteringExample()
		{
			Console.WriteLine("=== Filtering Configuration ===\n");

			var config = new GeneratorConfig
			{
				Filters = new FiltersConfig
				{
					IncludePackages = new List<string> { "flutter", "material", "cupertino" },
					ExcludePackages = new List<string> { "flutter_test" },
					ExcludeWidgets = new List<string>
					{
						"*Internal*",
						"*Private*",
						"*Debug*",
						"_*" // Exclude widgets starting with underscore
					},
					IncludeNamespaces = new List<string> { "material", "widgets", "painting" },
					ExcludeNamespaces = new List<string> { "dart:_*" },
					IncludeInternalApis = false,
					IncludeDeprecated = true,
					IncludeExperimental = false,
					MinimumStability = "stable"
				}
			};

			Console.WriteLine("Included packages:");
			foreach (var pkg in config.Filters.IncludePackages)
			{
				Console.WriteLine($"  - {pkg}");
			}

			Console.WriteLine("\nExcluded widget patterns:");
			foreach (var pattern in config.Filters.ExcludeWidgets)
			{
				Console.WriteLine($"  - {pattern}");
			}

			Console.WriteLine($"\nInclude deprecated: {config.Filters.IncludeDeprecated}");
			Console.WriteLine($"Include experimental: {config.Filters.IncludeExperimental}");
			Console.WriteLine($"Minimum stability: {config.Filters.MinimumStability}");
			Console.WriteLine();
		}

		/// <summary>
		/// Runs all examples.
		/// </summary>
		public static void RunAllExamples()
		{
			try
			{
				BasicLoading();
				LoadingWithOverrides();
				ProgrammaticConfiguration();
				MergingConfigurations();
				ValidationExample();
				ThirdPartyPackagesExample();
				FilteringExample();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error running examples: {ex.Message}");
			}
		}
	}
}

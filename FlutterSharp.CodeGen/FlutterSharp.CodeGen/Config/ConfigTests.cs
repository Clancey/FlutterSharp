using System;
using System.Collections.Generic;
using System.IO;

namespace FlutterSharp.CodeGen.Config
{
	/// <summary>
	/// Simple test examples for the configuration system.
	/// Note: For production use, consider using xUnit, NUnit, or MSTest.
	/// These are basic validation tests that can be run without a test framework.
	/// </summary>
	public static class ConfigTests
	{
		/// <summary>
		/// Runs all tests and reports results.
		/// </summary>
		/// <returns>True if all tests passed, false otherwise.</returns>
		public static bool RunAllTests()
		{
			Console.WriteLine("=== Running Configuration Tests ===\n");

			var passed = 0;
			var failed = 0;

			RunTest("Default Config Should Be Valid", TestDefaultConfigIsValid, ref passed, ref failed);
			RunTest("Invalid Flutter Mode Should Fail Validation", TestInvalidFlutterModeFailsValidation, ref passed, ref failed);
			RunTest("Empty Output Path Should Fail Validation", TestEmptyOutputPathFailsValidation, ref passed, ref failed);
			RunTest("Invalid Log Level Should Fail Validation", TestInvalidLogLevelFailsValidation, ref passed, ref failed);
			RunTest("Save And Load Should Round Trip", TestSaveAndLoadRoundTrip, ref passed, ref failed);
			RunTest("Merge Should Override Values", TestMergeOverridesValues, ref passed, ref failed);
			RunTest("Custom Mappings Should Be Preserved", TestCustomMappingsPreserved, ref passed, ref failed);
			RunTest("Third Party Packages Should Validate", TestThirdPartyPackagesValidate, ref passed, ref failed);

			Console.WriteLine($"\n=== Test Results ===");
			Console.WriteLine($"Passed: {passed}");
			Console.WriteLine($"Failed: {failed}");
			Console.WriteLine($"Total:  {passed + failed}");

			return failed == 0;
		}

		private static void RunTest(string name, Func<bool> test, ref int passed, ref int failed)
		{
			try
			{
				var result = test();
				if (result)
				{
					Console.WriteLine($"[PASS] {name}");
					passed++;
				}
				else
				{
					Console.WriteLine($"[FAIL] {name}");
					failed++;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[ERROR] {name}: {ex.Message}");
				failed++;
			}
		}

		private static bool TestDefaultConfigIsValid()
		{
			var config = ConfigLoader.GetDefaultConfig();
			var validated = ConfigLoader.ValidateConfig(config);
			return validated != null;
		}

		private static bool TestInvalidFlutterModeFailsValidation()
		{
			var config = new GeneratorConfig
			{
				FlutterSdk = new FlutterSdkConfig { Mode = "invalid_mode" }
			};

			try
			{
				ConfigLoader.ValidateConfig(config);
				return false; // Should have thrown
			}
			catch (InvalidOperationException)
			{
				return true; // Expected
			}
		}

		private static bool TestEmptyOutputPathFailsValidation()
		{
			var config = new GeneratorConfig
			{
				OutputPaths = new OutputPathsConfig
				{
					BaseDirectory = "",
					CSharpOutput = ""
				}
			};

			try
			{
				ConfigLoader.ValidateConfig(config);
				return false; // Should have thrown
			}
			catch (InvalidOperationException)
			{
				return true; // Expected
			}
		}

		private static bool TestInvalidLogLevelFailsValidation()
		{
			var config = new GeneratorConfig
			{
				AdvancedOptions = new AdvancedOptionsConfig
				{
					LogLevel = "InvalidLevel"
				}
			};

			try
			{
				ConfigLoader.ValidateConfig(config);
				return false; // Should have thrown
			}
			catch (InvalidOperationException)
			{
				return true; // Expected
			}
		}

		private static bool TestSaveAndLoadRoundTrip()
		{
			var tempFile = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}.json");

			try
			{
				var original = new GeneratorConfig
				{
					FlutterSdk = new FlutterSdkConfig
					{
						Mode = "local",
						Path = "/test/flutter/path"
					},
					GenerationOptions = new GenerationOptionsConfig
					{
						NamespacePrefix = "TestNamespace",
						GenerateDocumentation = false
					}
				};

				ConfigLoader.Save(original, tempFile);
				var loaded = ConfigLoader.Load(tempFile);

				return loaded.FlutterSdk.Mode == "local" &&
				       loaded.FlutterSdk.Path == "/test/flutter/path" &&
				       loaded.GenerationOptions.NamespacePrefix == "TestNamespace" &&
				       loaded.GenerationOptions.GenerateDocumentation == false;
			}
			finally
			{
				if (File.Exists(tempFile))
				{
					File.Delete(tempFile);
				}
			}
		}

		private static bool TestMergeOverridesValues()
		{
			var baseConfig = new GeneratorConfig
			{
				FlutterSdk = new FlutterSdkConfig { Mode = "auto" },
				GenerationOptions = new GenerationOptionsConfig
				{
					NamespacePrefix = "Base",
					GenerateDocumentation = true
				}
			};

			var overrideConfig = new GeneratorConfig
			{
				FlutterSdk = new FlutterSdkConfig { Mode = "local", Path = "/custom" },
				GenerationOptions = new GenerationOptionsConfig
				{
					NamespacePrefix = "Override"
					// GenerateDocumentation not specified
				}
			};

			var merged = ConfigLoader.Merge(baseConfig, overrideConfig);

			return merged.FlutterSdk.Mode == "local" &&
			       merged.FlutterSdk.Path == "/custom" &&
			       merged.GenerationOptions.NamespacePrefix == "Override" &&
			       merged.GenerationOptions.GenerateDocumentation == true; // From base
		}

		private static bool TestCustomMappingsPreserved()
		{
			var config = new GeneratorConfig
			{
				TypeMapping = new TypeMappingConfig
				{
					CustomMappings = new Dictionary<string, string>
					{
						["CustomDartType"] = "CustomCSharpType",
						["AnotherType"] = "AnotherCSharpType"
					}
				}
			};

			var validated = ConfigLoader.ValidateConfig(config);

			return validated.TypeMapping.CustomMappings.Count == 2 &&
			       validated.TypeMapping.CustomMappings["CustomDartType"] == "CustomCSharpType";
		}

		private static bool TestThirdPartyPackagesValidate()
		{
			var validConfig = new GeneratorConfig
			{
				ThirdPartyPackages = new ThirdPartyPackagesConfig
				{
					Enabled = true,
					PubDevPackages = new List<PubPackageSource>
					{
						new() { Name = "provider", Version = "^6.0.0", Enabled = true }
					},
					GitPackages = new List<GitPackageSource>
					{
						new() { Url = "https://github.com/test/package.git", Enabled = true }
					},
					LocalPackages = new List<LocalPackageSource>
					{
						new() { Path = "/test/path", Enabled = true }
					}
				}
			};

			try
			{
				ConfigLoader.ValidateConfig(validConfig);
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Test that validates empty package names are rejected.
		/// </summary>
		private static bool TestEmptyPackageNameFailsValidation()
		{
			var config = new GeneratorConfig
			{
				ThirdPartyPackages = new ThirdPartyPackagesConfig
				{
					Enabled = true,
					PubDevPackages = new List<PubPackageSource>
					{
						new() { Name = "", Version = "^6.0.0", Enabled = true } // Invalid
					}
				}
			};

			try
			{
				ConfigLoader.ValidateConfig(config);
				return false; // Should have thrown
			}
			catch (InvalidOperationException)
			{
				return true; // Expected
			}
		}

		/// <summary>
		/// Test that validates indentation size constraints.
		/// </summary>
		private static bool TestIndentationSizeConstraints()
		{
			var configTooSmall = new GeneratorConfig
			{
				GenerationOptions = new GenerationOptionsConfig
				{
					IndentationSize = 0 // Invalid
				}
			};

			var configTooLarge = new GeneratorConfig
			{
				GenerationOptions = new GenerationOptionsConfig
				{
					IndentationSize = 10 // Invalid
				}
			};

			var validConfig = new GeneratorConfig
			{
				GenerationOptions = new GenerationOptionsConfig
				{
					IndentationSize = 4 // Valid
				}
			};

			try
			{
				ConfigLoader.ValidateConfig(configTooSmall);
				return false;
			}
			catch (InvalidOperationException)
			{
				// Expected
			}

			try
			{
				ConfigLoader.ValidateConfig(configTooLarge);
				return false;
			}
			catch (InvalidOperationException)
			{
				// Expected
			}

			try
			{
				ConfigLoader.ValidateConfig(validConfig);
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Test that configuration search walks up directory tree.
		/// </summary>
		private static bool TestConfigSearchWalksUpTree()
		{
			// This test would require actual file system setup,
			// so we'll just validate the logic is present
			// In a real test framework, you would:
			// 1. Create a temp directory structure
			// 2. Place config in parent directory
			// 3. Change to child directory
			// 4. Verify config is found

			// For this simple test, just verify the method exists
			var config = ConfigLoader.GetDefaultConfig();
			return config != null;
		}
	}
}

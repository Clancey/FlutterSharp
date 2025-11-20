using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FlutterSharp.CodeGen.Models;

namespace FlutterSharp.CodeGen.Analysis
{
	/// <summary>
	/// Provides a C# API for analyzing Dart packages using the Dart analyzer script.
	/// </summary>
	public class DartAnalyzerHost
	{
		private const string DefaultAnalyzerScriptPath = "Tools/analyzer/package_scanner.dart";
		private const int DefaultTimeoutSeconds = 300; // 5 minutes

		private readonly string _analyzerScriptPath;
		private readonly int _timeoutSeconds;
		private readonly JsonSerializerOptions _jsonOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="DartAnalyzerHost"/> class.
		/// </summary>
		/// <param name="analyzerScriptPath">The path to the Dart analyzer script. If null, uses the default path.</param>
		/// <param name="timeoutSeconds">The timeout in seconds for analyzer operations. Default is 300 seconds (5 minutes).</param>
		public DartAnalyzerHost(string? analyzerScriptPath = null, int timeoutSeconds = DefaultTimeoutSeconds)
		{
			_analyzerScriptPath = analyzerScriptPath ?? DefaultAnalyzerScriptPath;
			_timeoutSeconds = timeoutSeconds;

			// Configure JSON serialization to handle Dart output
			_jsonOptions = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				NumberHandling = JsonNumberHandling.AllowReadingFromString,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				Converters =
				{
					new WidgetTypeConverter(),
					new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
				}
			};
		}

		/// <summary>
		/// Validates that Dart tools and the analyzer package are available.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task that represents the asynchronous validation operation. Returns true if all tools are available.</returns>
		/// <exception cref="DartToolsNotFoundException">Thrown when Dart is not installed or the analyzer script is not found.</exception>
		public async Task<bool> ValidateDartToolsAsync(CancellationToken cancellationToken = default)
		{
			// Check if Dart is installed
			try
			{
				var dartVersion = await ExecuteCommandAsync("dart", "--version", cancellationToken);
				LogDebug($"Dart version detected: {dartVersion}");
			}
			catch (Exception ex)
			{
				throw new DartToolsNotFoundException(
					"Dart is not installed or not found in PATH. Please install the Dart SDK.",
					ex);
			}

			// Check if analyzer script exists
			if (!File.Exists(_analyzerScriptPath))
			{
				throw new DartToolsNotFoundException(
					$"Analyzer script not found at path: {_analyzerScriptPath}");
			}

			// Check if required Dart packages are available
			var scriptDirectory = Path.GetDirectoryName(Path.GetFullPath(_analyzerScriptPath));
			if (scriptDirectory != null)
			{
				var pubspecPath = Path.Combine(scriptDirectory, "pubspec.yaml");
				if (!File.Exists(pubspecPath))
				{
					LogWarning($"pubspec.yaml not found at {pubspecPath}. Dependencies may not be installed.");
				}
			}

			LogDebug("Dart tools validation successful");
			return true;
		}

		/// <summary>
		/// Analyzes a Dart package and extracts widget, type, and enum definitions.
		/// </summary>
		/// <param name="packagePath">The path to the Dart package directory.</param>
		/// <param name="includeTypes">Optional list of type names to include. If null or empty, all types are included.</param>
		/// <param name="excludeTypes">Optional list of type names to exclude.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task that represents the asynchronous analysis operation. The task result contains the package definition.</returns>
		/// <exception cref="ArgumentNullException">Thrown when packagePath is null.</exception>
		/// <exception cref="DirectoryNotFoundException">Thrown when the package directory does not exist.</exception>
		/// <exception cref="DartAnalyzerException">Thrown when the Dart analyzer fails to analyze the package.</exception>
		public async Task<PackageDefinition> AnalyzePackageAsync(
			string packagePath,
			List<string>? includeTypes = null,
			List<string>? excludeTypes = null,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(packagePath))
			{
				throw new ArgumentNullException(nameof(packagePath));
			}

			var fullPath = Path.GetFullPath(packagePath);
			if (!Directory.Exists(fullPath))
			{
				throw new DirectoryNotFoundException($"Package directory not found: {fullPath}");
			}

			LogDebug($"Analyzing package at: {fullPath}");

			// Build arguments for the Dart script
			var includeArg = includeTypes != null && includeTypes.Count > 0
				? string.Join(",", includeTypes)
				: string.Empty;

			var excludeArg = excludeTypes != null && excludeTypes.Count > 0
				? string.Join(",", excludeTypes)
				: string.Empty;

			var arguments = $"run \"{_analyzerScriptPath}\" \"{fullPath}\" \"{includeArg}\" \"{excludeArg}\"";

			try
			{
				var jsonOutput = await ExecuteCommandAsync("dart", arguments, cancellationToken);

				LogDebug($"Received JSON output ({jsonOutput.Length} characters)");

				// Parse the JSON output
				var packageDefinition = DeserializePackageDefinition(jsonOutput);

				LogDebug($"Successfully analyzed package: {packageDefinition.Name} v{packageDefinition.Version}");
				LogDebug($"  Widgets: {packageDefinition.Widgets.Count}");
				LogDebug($"  Types: {packageDefinition.Types.Count}");
				LogDebug($"  Enums: {packageDefinition.Enums.Count}");

				return packageDefinition;
			}
			catch (JsonException ex)
			{
				throw new DartAnalyzerException(
					$"Failed to parse analyzer output for package at {fullPath}. The output may not be valid JSON.",
					ex);
			}
			catch (Exception ex) when (ex is not DartAnalyzerException)
			{
				throw new DartAnalyzerException(
					$"Failed to analyze package at {fullPath}",
					ex);
			}
		}

		/// <summary>
		/// Analyzes a single Dart file and extracts definitions.
		/// Note: This creates a temporary wrapper to analyze a single file using the package analyzer.
		/// </summary>
		/// <param name="filePath">The path to the Dart file to analyze.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task that represents the asynchronous analysis operation. The task result contains the package definition with definitions from the file.</returns>
		/// <exception cref="ArgumentNullException">Thrown when filePath is null.</exception>
		/// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
		/// <exception cref="DartAnalyzerException">Thrown when the Dart analyzer fails to analyze the file.</exception>
		public async Task<PackageDefinition> AnalyzeFileAsync(
			string filePath,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			var fullPath = Path.GetFullPath(filePath);
			if (!File.Exists(fullPath))
			{
				throw new FileNotFoundException($"Dart file not found: {fullPath}");
			}

			LogDebug($"Analyzing file: {fullPath}");

			// For single file analysis, we need to analyze its containing package
			// The Dart analyzer works at the package level, not individual files
			var directory = Path.GetDirectoryName(fullPath);
			if (string.IsNullOrEmpty(directory))
			{
				throw new DartAnalyzerException($"Could not determine directory for file: {fullPath}");
			}

			// Try to find the package root by looking for pubspec.yaml
			var packageRoot = FindPackageRoot(directory);
			if (packageRoot == null)
			{
				throw new DartAnalyzerException(
					$"Could not find package root (pubspec.yaml) for file: {fullPath}. " +
					"The file must be part of a Dart package.");
			}

			LogDebug($"Found package root: {packageRoot}");

			// Analyze the whole package
			// Note: We could filter results to only include definitions from the specific file,
			// but that would require modifying the script or post-processing
			return await AnalyzePackageAsync(packageRoot, null, null, cancellationToken);
		}

		/// <summary>
		/// Deserializes the JSON output from the Dart analyzer into a PackageDefinition.
		/// </summary>
		/// <param name="jsonOutput">The JSON output from the analyzer.</param>
		/// <returns>The deserialized package definition.</returns>
		private PackageDefinition DeserializePackageDefinition(string jsonOutput)
		{
			var rawData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonOutput, _jsonOptions);
			if (rawData == null)
			{
				throw new DartAnalyzerException("Failed to deserialize analyzer output: result was null");
			}

			// Extract basic package info
			var name = rawData.TryGetValue("name", out var nameElement)
				? nameElement.GetString() ?? "unknown"
				: "unknown";

			var version = rawData.TryGetValue("version", out var versionElement)
				? versionElement.GetString() ?? "0.0.0"
				: "0.0.0";

			var description = rawData.TryGetValue("description", out var descElement)
				? descElement.GetString()
				: null;

			var packagePath = rawData.TryGetValue("packagePath", out var pathElement)
				? pathElement.GetString()
				: null;

			var analysisTimestamp = rawData.TryGetValue("analysisTimestamp", out var timestampElement)
				? timestampElement.GetString()
				: null;

			// Deserialize widgets
			var widgets = new List<WidgetDefinition>();
			if (rawData.TryGetValue("widgets", out var widgetsElement))
			{
				widgets = JsonSerializer.Deserialize<List<WidgetDefinition>>(
					widgetsElement.GetRawText(),
					_jsonOptions) ?? new List<WidgetDefinition>();
			}

			// Deserialize types
			var types = new List<TypeDefinition>();
			if (rawData.TryGetValue("types", out var typesElement))
			{
				types = JsonSerializer.Deserialize<List<TypeDefinition>>(
					typesElement.GetRawText(),
					_jsonOptions) ?? new List<TypeDefinition>();
			}

			// Deserialize enums
			var enums = new List<EnumDefinition>();
			if (rawData.TryGetValue("enums", out var enumsElement))
			{
				enums = JsonSerializer.Deserialize<List<EnumDefinition>>(
					enumsElement.GetRawText(),
					_jsonOptions) ?? new List<EnumDefinition>();
			}

			return new PackageDefinition
			{
				Name = name,
				Version = version,
				Description = description,
				PackagePath = packagePath,
				AnalysisTimestamp = analysisTimestamp,
				Widgets = widgets,
				Types = types,
				Enums = enums
			};
		}

		/// <summary>
		/// Finds the package root by searching for pubspec.yaml in parent directories.
		/// </summary>
		/// <param name="startDirectory">The directory to start searching from.</param>
		/// <returns>The package root directory, or null if not found.</returns>
		private static string? FindPackageRoot(string startDirectory)
		{
			var currentDir = new DirectoryInfo(startDirectory);

			while (currentDir != null)
			{
				var pubspecPath = Path.Combine(currentDir.FullName, "pubspec.yaml");
				if (File.Exists(pubspecPath))
				{
					return currentDir.FullName;
				}

				currentDir = currentDir.Parent;
			}

			return null;
		}

		/// <summary>
		/// Executes a command line tool and captures its output.
		/// </summary>
		/// <param name="command">The command to execute.</param>
		/// <param name="arguments">The command arguments.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The standard output from the command.</returns>
		/// <exception cref="DartAnalyzerException">Thrown when the command fails or times out.</exception>
		private async Task<string> ExecuteCommandAsync(
			string command,
			string arguments,
			CancellationToken cancellationToken)
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = command,
				Arguments = arguments,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			var output = new StringBuilder();
			var error = new StringBuilder();

			using var process = new Process { StartInfo = startInfo };

			process.OutputDataReceived += (sender, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					output.AppendLine(e.Data);
				}
			};

			process.ErrorDataReceived += (sender, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					error.AppendLine(e.Data);
				}
			};

			try
			{
				LogDebug($"Executing: {command} {arguments}");

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				// Wait for the process to exit with timeout
				using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
				using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

				await process.WaitForExitAsync(linkedCts.Token);

				var exitCode = process.ExitCode;
				var outputText = output.ToString().Trim();
				var errorText = error.ToString().Trim();

				if (!string.IsNullOrEmpty(errorText))
				{
					LogDebug($"Standard error output: {errorText}");
				}

				if (exitCode != 0)
				{
					var errorMessage = !string.IsNullOrEmpty(errorText)
						? errorText
						: "Process exited with non-zero exit code";

					throw new DartAnalyzerException(
						$"Command '{command}' failed with exit code {exitCode}: {errorMessage}");
				}

				return outputText;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				if (!process.HasExited)
				{
					process.Kill(true);
				}
				throw new DartAnalyzerException("Operation was cancelled by user");
			}
			catch (OperationCanceledException)
			{
				if (!process.HasExited)
				{
					process.Kill(true);
				}
				throw new DartAnalyzerException(
					$"Command '{command}' timed out after {_timeoutSeconds} seconds");
			}
		}

		/// <summary>
		/// Logs a debug message. Override this method to integrate with your logging framework.
		/// </summary>
		/// <param name="message">The message to log.</param>
		protected virtual void LogDebug(string message)
		{
			// Default implementation writes to console
			// Override this method to integrate with your logging framework
			Console.WriteLine($"[DartAnalyzerHost] {message}");
		}

		/// <summary>
		/// Logs a warning message. Override this method to integrate with your logging framework.
		/// </summary>
		/// <param name="message">The message to log.</param>
		protected virtual void LogWarning(string message)
		{
			// Default implementation writes to console
			// Override this method to integrate with your logging framework
			Console.WriteLine($"[DartAnalyzerHost] WARNING: {message}");
		}
	}

	/// <summary>
	/// Custom JSON converter for WidgetType enum that handles string values from Dart.
	/// </summary>
	internal class WidgetTypeConverter : JsonConverter<WidgetType>
	{
		public override WidgetType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				var value = reader.GetString();
				return value switch
				{
					"Stateless" => WidgetType.Stateless,
					"Stateful" => WidgetType.Stateful,
					"SingleChildRenderObject" => WidgetType.SingleChildRenderObject,
					"MultiChildRenderObject" => WidgetType.MultiChildRenderObject,
					"Proxy" => WidgetType.Proxy,
					"LeafRenderObject" => WidgetType.LeafRenderObject,
					"Widget" => WidgetType.Widget,
					_ => WidgetType.Unknown
				};
			}

			return WidgetType.Unknown;
		}

		public override void Write(Utf8JsonWriter writer, WidgetType value, JsonSerializerOptions options)
		{
			var stringValue = value switch
			{
				WidgetType.Stateless => "Stateless",
				WidgetType.Stateful => "Stateful",
				WidgetType.SingleChildRenderObject => "SingleChildRenderObject",
				WidgetType.MultiChildRenderObject => "MultiChildRenderObject",
				WidgetType.Proxy => "Proxy",
				WidgetType.LeafRenderObject => "LeafRenderObject",
				WidgetType.Widget => "Widget",
				_ => "Unknown"
			};

			writer.WriteStringValue(stringValue);
		}
	}

	/// <summary>
	/// Exception thrown when Dart tools are not found or not properly configured.
	/// </summary>
	public class DartToolsNotFoundException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DartToolsNotFoundException"/> class.
		/// </summary>
		/// <param name="message">The error message.</param>
		public DartToolsNotFoundException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DartToolsNotFoundException"/> class.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="innerException">The inner exception.</param>
		public DartToolsNotFoundException(string message, Exception innerException)
			: base(message, innerException) { }
	}

	/// <summary>
	/// Exception thrown when the Dart analyzer fails to analyze a package or file.
	/// </summary>
	public class DartAnalyzerException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DartAnalyzerException"/> class.
		/// </summary>
		/// <param name="message">The error message.</param>
		public DartAnalyzerException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DartAnalyzerException"/> class.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="innerException">The inner exception.</param>
		public DartAnalyzerException(string message, Exception innerException)
			: base(message, innerException) { }
	}
}

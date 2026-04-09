using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlutterSharp.CodeGen.Sources;

/// <summary>
/// Configuration for Flutter SDK management.
/// </summary>
public sealed class FlutterSdkConfig
{
    /// <summary>
    /// Gets or sets the SDK discovery mode.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><term>auto</term><description>Automatically discover Flutter SDK from environment or clone if needed</description></item>
    /// <item><term>local</term><description>Use a specific local path to Flutter SDK</description></item>
    /// <item><term>clone</term><description>Always clone a fresh Flutter SDK</description></item>
    /// </list>
    /// </remarks>
    public string Mode { get; set; } = "auto";

    /// <summary>
    /// Gets or sets the local path to Flutter SDK (used when Mode is "local").
    /// </summary>
    public string? LocalPath { get; set; }

    /// <summary>
    /// Gets or sets the directory where Flutter SDK will be cloned (used when Mode is "clone" or "auto").
    /// </summary>
    public string CloneDirectory { get; set; } = Path.Combine(Path.GetTempPath(), "flutter-sdk");

    /// <summary>
    /// Gets or sets the Flutter repository URL.
    /// </summary>
    public string RepositoryUrl { get; set; } = "https://github.com/flutter/flutter.git";

    /// <summary>
    /// Gets or sets the Flutter branch or version to use.
    /// </summary>
    public string Branch { get; set; } = "stable";

    /// <summary>
    /// Gets or sets the depth for shallow clones.
    /// </summary>
    public int CloneDepth { get; set; } = 1;
}

/// <summary>
/// Manages the Flutter SDK - locating, cloning, and providing access to Flutter packages.
/// </summary>
public sealed class FlutterSdkManager
{
    private readonly FlutterSdkConfig _config;
    private readonly Action<string>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlutterSdkManager"/> class.
    /// </summary>
    /// <param name="config">The configuration for SDK management.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    public FlutterSdkManager(FlutterSdkConfig config, Action<string>? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;
    }

    /// <summary>
    /// Ensures the Flutter SDK is available and returns its path.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path to the Flutter SDK root directory.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the SDK cannot be located or initialized.</exception>
    public async Task<string> EnsureFlutterSdkAsync(CancellationToken cancellationToken = default)
    {
        Log("Ensuring Flutter SDK availability...");

        string? sdkPath = _config.Mode.ToLowerInvariant() switch
        {
            "local" => ValidateLocalPath(),
            "clone" => await CloneFlutterSdkAsync(cancellationToken),
            "auto" => await AutoDiscoverOrCloneAsync(cancellationToken),
            _ => throw new InvalidOperationException($"Invalid mode: {_config.Mode}. Supported modes are: auto, local, clone.")
        };

        if (string.IsNullOrEmpty(sdkPath))
        {
            throw new InvalidOperationException("Failed to locate or initialize Flutter SDK.");
        }

        Log($"Flutter SDK located at: {sdkPath}");
        return sdkPath;
    }

    /// <summary>
    /// Gets the path to a Flutter package.
    /// </summary>
    /// <param name="sdkPath">The Flutter SDK root path.</param>
    /// <param name="packageName">The package name (e.g., "widgets", "material", "cupertino").</param>
    /// <returns>The path to the package directory.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the package directory does not exist.</exception>
    public string GetPackagePath(string sdkPath, string packageName)
    {
        if (string.IsNullOrWhiteSpace(sdkPath))
        {
            throw new ArgumentException("SDK path cannot be null or whitespace.", nameof(sdkPath));
        }

        if (string.IsNullOrWhiteSpace(packageName))
        {
            throw new ArgumentException("Package name cannot be null or whitespace.", nameof(packageName));
        }

        // Flutter packages are located in: {sdkPath}/packages/flutter/lib/{packageName}
        var packagePath = Path.Combine(sdkPath, "packages", "flutter", "lib", packageName);

        if (!Directory.Exists(packagePath))
        {
            throw new DirectoryNotFoundException($"Flutter package '{packageName}' not found at: {packagePath}");
        }

        Log($"Package '{packageName}' located at: {packagePath}");
        return packagePath;
    }

    /// <summary>
    /// Gets the path to the Flutter framework package root.
    /// </summary>
    /// <param name="sdkPath">The Flutter SDK root path.</param>
    /// <returns>The path to the Flutter framework package.</returns>
    public string GetFlutterPackageRoot(string sdkPath)
    {
        if (string.IsNullOrWhiteSpace(sdkPath))
        {
            throw new ArgumentException("SDK path cannot be null or whitespace.", nameof(sdkPath));
        }

        var packageRoot = Path.Combine(sdkPath, "packages", "flutter");

        if (!Directory.Exists(packageRoot))
        {
            throw new DirectoryNotFoundException($"Flutter package root not found at: {packageRoot}");
        }

        return packageRoot;
    }

    /// <summary>
    /// Runs the flutter command to update the SDK.
    /// </summary>
    /// <param name="sdkPath">The Flutter SDK root path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateFlutterSdkAsync(string sdkPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sdkPath))
        {
            throw new ArgumentException("SDK path cannot be null or whitespace.", nameof(sdkPath));
        }

        Log("Updating Flutter SDK...");

        var flutterExecutable = GetFlutterExecutable(sdkPath);
        await RunProcessAsync(flutterExecutable, "upgrade", sdkPath, cancellationToken);

        Log("Flutter SDK updated successfully.");
    }

    private string? ValidateLocalPath()
    {
        if (string.IsNullOrWhiteSpace(_config.LocalPath))
        {
            throw new InvalidOperationException("LocalPath must be specified when using 'local' mode.");
        }

        if (!Directory.Exists(_config.LocalPath))
        {
            throw new DirectoryNotFoundException($"Local Flutter SDK path does not exist: {_config.LocalPath}");
        }

        if (!IsValidFlutterSdk(_config.LocalPath))
        {
            throw new InvalidOperationException($"The specified path is not a valid Flutter SDK: {_config.LocalPath}");
        }

        Log($"Using local Flutter SDK at: {_config.LocalPath}");
        return _config.LocalPath;
    }

    private async Task<string?> AutoDiscoverOrCloneAsync(CancellationToken cancellationToken)
    {
        Log("Auto-discovering Flutter SDK...");

        // Try FLUTTER_ROOT environment variable
        var flutterRoot = Environment.GetEnvironmentVariable("FLUTTER_ROOT");
        if (!string.IsNullOrWhiteSpace(flutterRoot) && Directory.Exists(flutterRoot) && IsValidFlutterSdk(flutterRoot))
        {
            Log($"Found Flutter SDK via FLUTTER_ROOT: {flutterRoot}");
            return flutterRoot;
        }

        // Try to find flutter in PATH using 'which' (Unix) or 'where' (Windows)
        var flutterPath = await FindFlutterInPathAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(flutterPath))
        {
            // Flutter executable found, resolve to SDK root
            var sdkPath = ResolveFlutterSdkPath(flutterPath);
            if (!string.IsNullOrWhiteSpace(sdkPath) && IsValidFlutterSdk(sdkPath))
            {
                Log($"Found Flutter SDK via PATH: {sdkPath}");
                return sdkPath;
            }
        }

        // Neither environment variable nor PATH worked, clone the SDK
        Log("Flutter SDK not found in environment. Cloning...");
        return await CloneFlutterSdkAsync(cancellationToken);
    }

    private async Task<string?> FindFlutterInPathAsync(CancellationToken cancellationToken)
    {
        try
        {
            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var command = isWindows ? "where" : "which";
            var arguments = "flutter";

            var output = await RunProcessAndCaptureOutputAsync(command, arguments, cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(output))
            {
                // Take the first line if multiple paths are returned
                var flutterPath = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(flutterPath) && File.Exists(flutterPath))
                {
                    return flutterPath;
                }
            }
        }
        catch (Exception ex)
        {
            Log($"Error finding Flutter in PATH: {ex.Message}");
        }

        return null;
    }

    private string? ResolveFlutterSdkPath(string flutterExecutablePath)
    {
        try
        {
            // Flutter executable is typically at: {SDK_ROOT}/bin/flutter
            var binDirectory = Path.GetDirectoryName(flutterExecutablePath);
            if (!string.IsNullOrWhiteSpace(binDirectory))
            {
                var sdkRoot = Path.GetDirectoryName(binDirectory);
                return sdkRoot;
            }
        }
        catch (Exception ex)
        {
            Log($"Error resolving Flutter SDK path: {ex.Message}");
        }

        return null;
    }

    private async Task<string> CloneFlutterSdkAsync(CancellationToken cancellationToken)
    {
        var clonePath = _config.CloneDirectory;

        // If directory exists and is valid, use it
        if (Directory.Exists(clonePath) && IsValidFlutterSdk(clonePath))
        {
            Log($"Flutter SDK already exists at: {clonePath}");
            return clonePath;
        }

        // Create clone directory if it doesn't exist
        if (Directory.Exists(clonePath))
        {
            Log($"Removing existing directory: {clonePath}");
            Directory.Delete(clonePath, recursive: true);
        }

        Directory.CreateDirectory(clonePath);
        Log($"Cloning Flutter SDK to: {clonePath}");

        // Clone the repository with shallow clone
        var parentDirectory = Path.GetDirectoryName(clonePath) ?? throw new InvalidOperationException("Could not determine parent directory.");
        var directoryName = Path.GetFileName(clonePath);

        var gitArgs = $"clone --depth {_config.CloneDepth} --branch {_config.Branch} {_config.RepositoryUrl} {directoryName}";

        await RunProcessAsync("git", gitArgs, parentDirectory, cancellationToken);

        if (!IsValidFlutterSdk(clonePath))
        {
            throw new InvalidOperationException($"Failed to clone valid Flutter SDK to: {clonePath}");
        }

        Log("Flutter SDK cloned successfully.");

        // Run flutter doctor to complete setup
        try
        {
            Log("Running initial Flutter setup...");
            var flutterExecutable = GetFlutterExecutable(clonePath);
            await RunProcessAsync(flutterExecutable, "--version", clonePath, cancellationToken);
        }
        catch (Exception ex)
        {
            Log($"Warning: Initial Flutter setup encountered an issue: {ex.Message}");
        }

        return clonePath;
    }

    private bool IsValidFlutterSdk(string path)
    {
        // Check for key Flutter SDK directories/files
        var flutterBinPath = Path.Combine(path, "bin");
        var packagesPath = Path.Combine(path, "packages");

        return Directory.Exists(flutterBinPath) && Directory.Exists(packagesPath);
    }

    private string GetFlutterExecutable(string sdkPath)
    {
        var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        var flutterExecutable = isWindows ? "flutter.bat" : "flutter";
        return Path.Combine(sdkPath, "bin", flutterExecutable);
    }

    private async Task RunProcessAsync(
        string fileName,
        string arguments,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
        };

        using var process = new Process { StartInfo = startInfo };

        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
                Log($"[Output] {e.Data}");
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorBuilder.AppendLine(e.Data);
                Log($"[Error] {e.Data}");
            }
        };

        Log($"Running: {fileName} {arguments}");

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var errorMessage = errorBuilder.Length > 0 ? errorBuilder.ToString() : outputBuilder.ToString();
            throw new InvalidOperationException(
                $"Process '{fileName}' exited with code {process.ExitCode}. Error: {errorMessage}");
        }
    }

    private async Task<string> RunProcessAndCaptureOutputAsync(
        string fileName,
        string arguments,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
        };

        using var process = new Process { StartInfo = startInfo };

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken);

        return output;
    }

    private void Log(string message)
    {
        _logger?.Invoke($"[FlutterSdkManager] {message}");
    }
}

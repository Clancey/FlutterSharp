using System;

namespace Flutter.Initialization;

/// <summary>
/// Options for configuring Flutter initialization behavior.
/// </summary>
public sealed class FlutterInitializationOptions
{
    /// <summary>
    /// Default initialization options.
    /// </summary>
    public static FlutterInitializationOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets how long to wait for the Flutter runtime to signal ready.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan ReadyTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets how long to wait for the Flutter engine to start.
    /// Default is 10 seconds.
    /// </summary>
    public TimeSpan EngineStartTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets whether to throw an exception on initialization failure.
    /// When false, returns a failure result instead.
    /// Default is false.
    /// </summary>
    public bool ThrowOnFailure { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to allow degraded operation when some components fail.
    /// Default is true.
    /// </summary>
    public bool AllowDegradedMode { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate that Flutter module files exist before starting.
    /// Default is true.
    /// </summary>
    public bool ValidateDeployment { get; set; } = true;

    /// <summary>
    /// Gets or sets the callback to invoke when initialization fails and a fallback UI should be shown.
    /// </summary>
    public Action<FlutterInitializationResult>? OnInitializationFailed { get; set; }

    /// <summary>
    /// Gets or sets the callback to invoke when initialization succeeds.
    /// </summary>
    public Action<FlutterInitializationResult>? OnInitializationSucceeded { get; set; }

    /// <summary>
    /// Gets or sets the callback to invoke when initialization enters degraded mode.
    /// </summary>
    public Action<FlutterInitializationResult>? OnDegradedMode { get; set; }

    /// <summary>
    /// Gets or sets whether to enable verbose logging during initialization.
    /// Default is false.
    /// </summary>
    public bool EnableVerboseLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets the number of retries before giving up on initialization.
    /// Default is 0 (no retries).
    /// </summary>
    public int MaxRetries { get; set; } = 0;

    /// <summary>
    /// Gets or sets the delay between retry attempts.
    /// Default is 1 second.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}

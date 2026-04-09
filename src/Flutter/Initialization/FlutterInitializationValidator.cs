using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Flutter.Internal;
using Flutter.Logging;

namespace Flutter.Initialization;

/// <summary>
/// Validates and manages Flutter initialization with timeout and error handling.
/// </summary>
public static class FlutterInitializationValidator
{
    private static readonly object _initLock = new object();
    private static FlutterInitializationResult? _lastResult;
    private static bool _initializationInProgress;

    /// <summary>
    /// Gets the result of the last initialization attempt.
    /// </summary>
    public static FlutterInitializationResult? LastInitializationResult => _lastResult;

    /// <summary>
    /// Gets whether initialization is currently in progress.
    /// </summary>
    public static bool IsInitializationInProgress => _initializationInProgress;

    /// <summary>
    /// Event raised when initialization status changes.
    /// </summary>
    public static event Action<FlutterInitializationResult>? OnInitializationComplete;

    /// <summary>
    /// Initializes Flutter asynchronously with timeout and validation.
    /// </summary>
    /// <param name="options">Initialization options. If null, uses defaults.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The initialization result.</returns>
    public static async Task<FlutterInitializationResult> InitializeAsync(
        FlutterInitializationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= FlutterInitializationOptions.Default;
        var stopwatch = Stopwatch.StartNew();
        var diagnostics = new Dictionary<string, object>();
        var warnings = new List<string>();

        // Prevent concurrent initialization
        lock (_initLock)
        {
            if (_initializationInProgress)
            {
                FlutterSharpLogger.LogWarning("Flutter initialization already in progress");
                return FlutterInitializationResult.Failure(
                    FlutterInitializationStage.EngineCreation,
                    "Initialization already in progress",
                    stopwatch.Elapsed);
            }

            if (FlutterManager.IsInitialized && FlutterManager.IsReady)
            {
                FlutterSharpLogger.LogDebug("Flutter already initialized and ready");
                return FlutterInitializationResult.Success(
                    stopwatch.Elapsed,
                    new Dictionary<string, object> { ["cached"] = true });
            }

            _initializationInProgress = true;
        }

        int attempt = 0;
        int maxAttempts = options.MaxRetries + 1;

        try
        {
            while (attempt < maxAttempts)
            {
                attempt++;
                diagnostics["attempt"] = attempt;

                if (attempt > 1)
                {
                    FlutterSharpLogger.LogInformation("Retrying Flutter initialization, attempt {Attempt}/{MaxAttempts}", attempt, maxAttempts);
                    await Task.Delay(options.RetryDelay, cancellationToken);
                }

                if (options.EnableVerboseLogging)
                {
                    FlutterSharpLogger.VerboseLoggingEnabled = true;
                }

                // Stage 1: Initialize FlutterManager
                FlutterSharpLogger.LogInformation("Initializing FlutterManager...");
                try
                {
                    FlutterManager.Initialize();
                    diagnostics["managerInitialized"] = true;
                }
                catch (Exception ex)
                {
                    FlutterSharpLogger.LogError(ex, "FlutterManager initialization failed");
                    var result = FlutterInitializationResult.Failure(
                        FlutterInitializationStage.EngineCreation,
                        $"FlutterManager initialization failed: {ex.Message}",
                        stopwatch.Elapsed,
                        ex,
                        diagnostics);

                    if (attempt >= maxAttempts)
                    {
                        HandleInitializationComplete(result, options);
                        return result;
                    }
                    continue;
                }

                // Stage 2: Wait for Flutter to signal ready
                FlutterSharpLogger.LogInformation("Waiting for Flutter runtime to signal ready (timeout: {Timeout}s)...", options.ReadyTimeout.TotalSeconds);
                var readyResult = await WaitForReadyAsync(options.ReadyTimeout, cancellationToken);

                if (readyResult.IsSuccess)
                {
                    diagnostics["readySignalReceived"] = true;
                    diagnostics["readyWaitTime"] = readyResult.Duration.TotalMilliseconds;

                    stopwatch.Stop();
                    diagnostics["totalDuration"] = stopwatch.ElapsedMilliseconds;

                    var successResult = FlutterInitializationResult.Success(
                        stopwatch.Elapsed,
                        diagnostics,
                        warnings.Count > 0 ? warnings : null);

                    FlutterSharpLogger.LogInformation("Flutter initialization completed successfully in {Duration}ms", stopwatch.ElapsedMilliseconds);
                    HandleInitializationComplete(successResult, options);
                    return successResult;
                }
                else if (readyResult.IsDegraded && options.AllowDegradedMode)
                {
                    // Can continue in degraded mode
                    stopwatch.Stop();
                    diagnostics["degradedMode"] = true;
                    warnings.Add(readyResult.ErrorMessage ?? "Flutter running in degraded mode");

                    var degradedResult = FlutterInitializationResult.Degraded(
                        readyResult.ErrorMessage ?? "Flutter running in degraded mode",
                        stopwatch.Elapsed,
                        diagnostics,
                        warnings);

                    FlutterSharpLogger.LogWarning("Flutter initialized in degraded mode: {Message}", readyResult.ErrorMessage);
                    HandleInitializationComplete(degradedResult, options);
                    return degradedResult;
                }
                else
                {
                    // Ready signal not received
                    FlutterSharpLogger.LogError("Flutter ready signal not received within timeout");
                    diagnostics["readySignalReceived"] = false;

                    if (attempt >= maxAttempts)
                    {
                        stopwatch.Stop();
                        var failureResult = FlutterInitializationResult.Failure(
                            FlutterInitializationStage.WaitingForReady,
                            $"Flutter runtime did not signal ready within {options.ReadyTimeout.TotalSeconds}s. This may indicate the Flutter module is not properly deployed or the Dart code failed to initialize.",
                            stopwatch.Elapsed,
                            diagnostics: diagnostics);

                        HandleInitializationComplete(failureResult, options);
                        return failureResult;
                    }
                }
            }

            // Should not reach here, but handle just in case
            stopwatch.Stop();
            var fallbackResult = FlutterInitializationResult.Failure(
                FlutterInitializationStage.EngineCreation,
                "Initialization failed after all retry attempts",
                stopwatch.Elapsed,
                diagnostics: diagnostics);

            HandleInitializationComplete(fallbackResult, options);
            return fallbackResult;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            FlutterSharpLogger.LogWarning("Flutter initialization was cancelled");
            var cancelResult = FlutterInitializationResult.Failure(
                FlutterInitializationStage.WaitingForReady,
                "Initialization was cancelled",
                stopwatch.Elapsed,
                diagnostics: diagnostics);

            HandleInitializationComplete(cancelResult, options);
            return cancelResult;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            FlutterSharpLogger.LogError(ex, "Unexpected error during Flutter initialization");
            var errorResult = FlutterInitializationResult.Failure(
                FlutterInitializationStage.EngineCreation,
                $"Unexpected initialization error: {ex.Message}",
                stopwatch.Elapsed,
                ex,
                diagnostics);

            HandleInitializationComplete(errorResult, options);
            return errorResult;
        }
        finally
        {
            lock (_initLock)
            {
                _initializationInProgress = false;
            }
        }
    }

    /// <summary>
    /// Waits for Flutter to signal ready with a timeout.
    /// </summary>
    private static async Task<FlutterInitializationResult> WaitForReadyAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var tcs = new TaskCompletionSource<bool>();

        // Register for the ready event
        void OnReady()
        {
            tcs.TrySetResult(true);
        }

        FlutterManager.OnReady += OnReady;

        try
        {
            // Check if already ready
            if (FlutterManager.IsReady)
            {
                stopwatch.Stop();
                return FlutterInitializationResult.Success(stopwatch.Elapsed);
            }

            // Wait for ready signal or timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            try
            {
                var completedTask = await Task.WhenAny(
                    tcs.Task,
                    Task.Delay(Timeout.Infinite, cts.Token));

                if (tcs.Task.IsCompleted && tcs.Task.Result)
                {
                    stopwatch.Stop();
                    return FlutterInitializationResult.Success(stopwatch.Elapsed);
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Timeout occurred (not user cancellation)
                stopwatch.Stop();

                // Check one more time in case of race condition
                if (FlutterManager.IsReady)
                {
                    return FlutterInitializationResult.Success(stopwatch.Elapsed);
                }

                return FlutterInitializationResult.Failure(
                    FlutterInitializationStage.WaitingForReady,
                    $"Timeout waiting for Flutter ready signal after {timeout.TotalSeconds}s",
                    stopwatch.Elapsed);
            }

            stopwatch.Stop();
            return FlutterInitializationResult.Failure(
                FlutterInitializationStage.WaitingForReady,
                "Failed to receive Flutter ready signal",
                stopwatch.Elapsed);
        }
        finally
        {
            FlutterManager.OnReady -= OnReady;
        }
    }

    /// <summary>
    /// Synchronously initializes Flutter with validation.
    /// Prefer InitializeAsync when possible.
    /// </summary>
    /// <param name="options">Initialization options.</param>
    /// <returns>The initialization result.</returns>
    public static FlutterInitializationResult InitializeSync(FlutterInitializationOptions? options = null)
    {
        return InitializeAsync(options).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Validates that Flutter can be initialized without actually initializing.
    /// Useful for pre-flight checks.
    /// </summary>
    /// <returns>True if Flutter appears to be properly configured.</returns>
    public static ValidationResult ValidateConfiguration()
    {
        var issues = new List<string>();

        // Check if Communicator is set up
        if (Communicator.SendCommand == null)
        {
            issues.Add("Communicator.SendCommand is not configured. Platform integration may not be complete.");
        }

        // Check FlutterManager state
        if (!FlutterManager.IsInitialized)
        {
            issues.Add("FlutterManager has not been initialized.");
        }

        return new ValidationResult
        {
            IsValid = issues.Count == 0,
            Issues = issues
        };
    }

    /// <summary>
    /// Resets the initialization state, allowing re-initialization.
    /// Use with caution - primarily for testing or recovery scenarios.
    /// </summary>
    public static void Reset()
    {
        lock (_initLock)
        {
            _lastResult = null;
            _initializationInProgress = false;
        }

        FlutterSharpLogger.LogWarning("Flutter initialization state has been reset");
    }

    private static void HandleInitializationComplete(
        FlutterInitializationResult result,
        FlutterInitializationOptions options)
    {
        lock (_initLock)
        {
            _lastResult = result;
        }

        // Invoke callbacks
        if (result.IsSuccess)
        {
            if (result.IsDegraded)
            {
                options.OnDegradedMode?.Invoke(result);
            }
            else
            {
                options.OnInitializationSucceeded?.Invoke(result);
            }
        }
        else
        {
            options.OnInitializationFailed?.Invoke(result);

            if (options.ThrowOnFailure)
            {
                throw new FlutterInitializationException(result);
            }
        }

        // Raise event
        OnInitializationComplete?.Invoke(result);
    }

    /// <summary>
    /// Result of a configuration validation check.
    /// </summary>
    public sealed class ValidationResult
    {
        /// <summary>
        /// Gets whether the configuration is valid.
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// Gets the list of validation issues found.
        /// </summary>
        public IReadOnlyList<string> Issues { get; init; } = Array.Empty<string>();
    }
}

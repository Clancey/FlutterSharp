using System;
using System.Collections.Generic;

namespace Flutter.Initialization;

/// <summary>
/// Represents the result of a Flutter initialization attempt.
/// </summary>
public sealed class FlutterInitializationResult
{
    /// <summary>
    /// Gets whether initialization was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the stage at which initialization failed, or null if successful.
    /// </summary>
    public FlutterInitializationStage? FailedStage { get; }

    /// <summary>
    /// Gets the exception that caused initialization to fail, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets a human-readable error message if initialization failed.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets diagnostic information collected during initialization.
    /// </summary>
    public IReadOnlyDictionary<string, object> Diagnostics { get; }

    /// <summary>
    /// Gets how long initialization took.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets whether Flutter is in a degraded but functional state.
    /// </summary>
    public bool IsDegraded { get; }

    /// <summary>
    /// Gets warnings that occurred during initialization but didn't prevent success.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; }

    private FlutterInitializationResult(
        bool isSuccess,
        FlutterInitializationStage? failedStage,
        Exception? exception,
        string? errorMessage,
        Dictionary<string, object> diagnostics,
        TimeSpan duration,
        bool isDegraded,
        List<string> warnings)
    {
        IsSuccess = isSuccess;
        FailedStage = failedStage;
        Exception = exception;
        ErrorMessage = errorMessage;
        Diagnostics = diagnostics;
        Duration = duration;
        IsDegraded = isDegraded;
        Warnings = warnings;
    }

    /// <summary>
    /// Creates a successful initialization result.
    /// </summary>
    public static FlutterInitializationResult Success(
        TimeSpan duration,
        Dictionary<string, object>? diagnostics = null,
        List<string>? warnings = null)
    {
        return new FlutterInitializationResult(
            isSuccess: true,
            failedStage: null,
            exception: null,
            errorMessage: null,
            diagnostics: diagnostics ?? new Dictionary<string, object>(),
            duration: duration,
            isDegraded: false,
            warnings: warnings ?? new List<string>());
    }

    /// <summary>
    /// Creates a degraded initialization result (partially functional).
    /// </summary>
    public static FlutterInitializationResult Degraded(
        string message,
        TimeSpan duration,
        Dictionary<string, object>? diagnostics = null,
        List<string>? warnings = null)
    {
        warnings ??= new List<string>();
        warnings.Add(message);

        return new FlutterInitializationResult(
            isSuccess: true,
            failedStage: null,
            exception: null,
            errorMessage: null,
            diagnostics: diagnostics ?? new Dictionary<string, object>(),
            duration: duration,
            isDegraded: true,
            warnings: warnings);
    }

    /// <summary>
    /// Creates a failed initialization result.
    /// </summary>
    public static FlutterInitializationResult Failure(
        FlutterInitializationStage failedStage,
        string errorMessage,
        TimeSpan duration,
        Exception? exception = null,
        Dictionary<string, object>? diagnostics = null)
    {
        return new FlutterInitializationResult(
            isSuccess: false,
            failedStage: failedStage,
            exception: exception,
            errorMessage: errorMessage,
            diagnostics: diagnostics ?? new Dictionary<string, object>(),
            duration: duration,
            isDegraded: false,
            warnings: new List<string>());
    }

    /// <summary>
    /// Returns a string representation of the initialization result.
    /// </summary>
    public override string ToString()
    {
        if (IsSuccess)
        {
            var status = IsDegraded ? "Degraded" : "Success";
            return $"FlutterInitialization: {status} ({Duration.TotalMilliseconds:F0}ms)";
        }
        else
        {
            return $"FlutterInitialization: Failed at {FailedStage} - {ErrorMessage} ({Duration.TotalMilliseconds:F0}ms)";
        }
    }
}

/// <summary>
/// Stages of Flutter initialization where failure can occur.
/// </summary>
public enum FlutterInitializationStage
{
    /// <summary>
    /// Validating that Flutter module files are present.
    /// </summary>
    ModuleValidation,

    /// <summary>
    /// Creating the Flutter engine instance.
    /// </summary>
    EngineCreation,

    /// <summary>
    /// Starting the Flutter engine.
    /// </summary>
    EngineStart,

    /// <summary>
    /// Setting up the method channel for communication.
    /// </summary>
    MethodChannelSetup,

    /// <summary>
    /// Waiting for the Flutter runtime to signal ready.
    /// </summary>
    WaitingForReady,

    /// <summary>
    /// Registering platform-specific handlers.
    /// </summary>
    PlatformHandlerRegistration,

    /// <summary>
    /// Loading initial widget state.
    /// </summary>
    InitialStateLoad
}

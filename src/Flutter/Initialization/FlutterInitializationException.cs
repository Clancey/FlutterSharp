using System;

namespace Flutter.Initialization;

/// <summary>
/// Exception thrown when Flutter initialization fails.
/// </summary>
public class FlutterInitializationException : Exception
{
    /// <summary>
    /// Gets the stage at which initialization failed.
    /// </summary>
    public FlutterInitializationStage FailedStage { get; }

    /// <summary>
    /// Gets the initialization result with diagnostic information.
    /// </summary>
    public FlutterInitializationResult? InitializationResult { get; }

    /// <summary>
    /// Creates a new FlutterInitializationException.
    /// </summary>
    public FlutterInitializationException(
        FlutterInitializationStage failedStage,
        string message)
        : base(message)
    {
        FailedStage = failedStage;
    }

    /// <summary>
    /// Creates a new FlutterInitializationException with an inner exception.
    /// </summary>
    public FlutterInitializationException(
        FlutterInitializationStage failedStage,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        FailedStage = failedStage;
    }

    /// <summary>
    /// Creates a new FlutterInitializationException from an initialization result.
    /// </summary>
    public FlutterInitializationException(FlutterInitializationResult result)
        : base(result.ErrorMessage ?? "Flutter initialization failed", result.Exception)
    {
        FailedStage = result.FailedStage ?? FlutterInitializationStage.EngineCreation;
        InitializationResult = result;
    }

    /// <summary>
    /// Gets a detailed error message including the failed stage.
    /// </summary>
    public override string ToString()
    {
        return $"FlutterInitializationException at {FailedStage}: {Message}";
    }
}

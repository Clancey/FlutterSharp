using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Flutter.Logging;

/// <summary>
/// Centralized logging infrastructure for FlutterSharp.
/// Configure by setting FlutterSharpLogger.Logger to your ILogger implementation.
/// </summary>
/// <remarks>
/// <para>
/// By default, FlutterSharpLogger uses a simple console logger for backwards compatibility.
/// For production scenarios, configure with your application's ILogger:
/// </para>
/// <code>
/// FlutterSharpLogger.Logger = loggerFactory.CreateLogger("FlutterSharp");
/// </code>
/// <para>
/// To disable all logging:
/// </para>
/// <code>
/// FlutterSharpLogger.Logger = NullLogger.Instance;
/// </code>
/// </remarks>
public static class FlutterSharpLogger
{
    private static ILogger _logger = new ConsoleLogger();
    private static bool _verboseLoggingEnabled = false;

    /// <summary>
    /// Gets or sets the logger instance used by FlutterSharp.
    /// </summary>
    /// <remarks>
    /// Set this property to integrate with your application's logging infrastructure.
    /// If not set, a default console logger will be used.
    /// To disable logging, set to <see cref="NullLogger.Instance"/>.
    /// </remarks>
    public static ILogger Logger
    {
        get => _logger;
        set => _logger = value ?? new ConsoleLogger();
    }

    /// <summary>
    /// Gets or sets whether verbose (Debug level) logging is enabled.
    /// </summary>
    /// <remarks>
    /// When false, Debug level messages are suppressed to reduce noise.
    /// Useful for controlling log verbosity in production environments.
    /// </remarks>
    public static bool VerboseLoggingEnabled
    {
        get => _verboseLoggingEnabled;
        set => _verboseLoggingEnabled = value;
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional arguments for the message template.</param>
    /// <remarks>
    /// Debug messages are only logged when <see cref="VerboseLoggingEnabled"/> is true.
    /// </remarks>
    public static void LogDebug(string message, params object?[] args)
    {
        if (!_verboseLoggingEnabled)
            return;

        try
        {
            if (args.Length > 0)
                _logger.LogDebug(message, args);
            else
                _logger.LogDebug(message);
        }
        catch
        {
            // Logging should never throw
        }
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional arguments for the message template.</param>
    public static void LogInformation(string message, params object?[] args)
    {
        try
        {
            if (args.Length > 0)
                _logger.LogInformation(message, args);
            else
                _logger.LogInformation(message);
        }
        catch
        {
            // Logging should never throw
        }
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional arguments for the message template.</param>
    public static void LogWarning(string message, params object?[] args)
    {
        try
        {
            if (args.Length > 0)
                _logger.LogWarning(message, args);
            else
                _logger.LogWarning(message);
        }
        catch
        {
            // Logging should never throw
        }
    }

    /// <summary>
    /// Logs a warning message with an associated exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional arguments for the message template.</param>
    public static void LogWarning(Exception exception, string message, params object?[] args)
    {
        try
        {
            if (args.Length > 0)
                _logger.LogWarning(exception, message, args);
            else
                _logger.LogWarning(exception, message);
        }
        catch
        {
            // Logging should never throw
        }
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional arguments for the message template.</param>
    public static void LogError(string message, params object?[] args)
    {
        try
        {
            if (args.Length > 0)
                _logger.LogError(message, args);
            else
                _logger.LogError(message);
        }
        catch
        {
            // Logging should never throw
        }
    }

    /// <summary>
    /// Logs an error message with an associated exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional arguments for the message template.</param>
    public static void LogError(Exception exception, string message, params object?[] args)
    {
        try
        {
            if (args.Length > 0)
                _logger.LogError(exception, message, args);
            else
                _logger.LogError(exception, message);
        }
        catch
        {
            // Logging should never throw
        }
    }

    /// <summary>
    /// Logs a critical error message.
    /// </summary>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional arguments for the message template.</param>
    public static void LogCritical(string message, params object?[] args)
    {
        try
        {
            if (args.Length > 0)
                _logger.LogCritical(message, args);
            else
                _logger.LogCritical(message);
        }
        catch
        {
            // Logging should never throw
        }
    }

    /// <summary>
    /// Logs a critical error message with an associated exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message template.</param>
    /// <param name="args">Optional arguments for the message template.</param>
    public static void LogCritical(Exception exception, string message, params object?[] args)
    {
        try
        {
            if (args.Length > 0)
                _logger.LogCritical(exception, message, args);
            else
                _logger.LogCritical(exception, message);
        }
        catch
        {
            // Logging should never throw
        }
    }

    /// <summary>
    /// Creates a disposable scope for structured logging.
    /// </summary>
    /// <param name="scopeState">The state to associate with the scope.</param>
    /// <returns>A disposable object that ends the logical operation scope on dispose.</returns>
    public static IDisposable? BeginScope<TState>(TState scopeState) where TState : notnull
    {
        try
        {
            return _logger.BeginScope(scopeState);
        }
        catch
        {
            // Logging should never throw
            return null;
        }
    }

    /// <summary>
    /// Simple console logger implementation for backwards compatibility.
    /// Used as the default logger when no external logger is configured.
    /// </summary>
    private sealed class ConsoleLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            try
            {
                var message = formatter(state, exception);
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logLevelString = GetLogLevelString(logLevel);

                var logMessage = $"[{timestamp}] [{logLevelString}] FlutterSharp: {message}";

                if (exception != null)
                {
                    logMessage += Environment.NewLine + exception.ToString();
                }

                // Write to appropriate stream based on log level
                if (logLevel >= LogLevel.Error)
                {
                    Console.Error.WriteLine(logMessage);
                }
                else
                {
                    Console.WriteLine(logMessage);
                }
            }
            catch
            {
                // Console logging should never throw
            }
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "TRACE",
                LogLevel.Debug => "DEBUG",
                LogLevel.Information => "INFO ",
                LogLevel.Warning => "WARN ",
                LogLevel.Error => "ERROR",
                LogLevel.Critical => "CRIT ",
                LogLevel.None => "NONE ",
                _ => "UNKN "
            };
        }
    }
}

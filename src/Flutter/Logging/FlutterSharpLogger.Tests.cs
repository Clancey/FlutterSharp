using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Flutter.Logging;

/// <summary>
/// Simple tests to verify FlutterSharpLogger functionality.
/// These are inline tests that can be run manually for verification.
/// </summary>
internal static class FlutterSharpLoggerTests
{
    /// <summary>
    /// Runs all tests and returns true if all pass.
    /// </summary>
    public static bool RunAllTests()
    {
        var results = new List<(string TestName, bool Passed, string Message)>();

        results.Add(RunTest("Default Logger", TestDefaultLogger));
        results.Add(RunTest("Verbose Logging Control", TestVerboseLogging));
        results.Add(RunTest("Null Logger", TestNullLogger));
        results.Add(RunTest("Custom Logger", TestCustomLogger));
        results.Add(RunTest("Exception Logging", TestExceptionLogging));
        results.Add(RunTest("Structured Logging", TestStructuredLogging));
        results.Add(RunTest("Logging Never Throws", TestLoggingNeverThrows));

        Console.WriteLine("\n=== FlutterSharpLogger Test Results ===");
        foreach (var (testName, passed, message) in results)
        {
            var status = passed ? "PASS" : "FAIL";
            Console.WriteLine($"[{status}] {testName}");
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine($"      {message}");
            }
        }

        var allPassed = results.TrueForAll(r => r.Passed);
        Console.WriteLine($"\nResult: {(allPassed ? "All tests passed" : "Some tests failed")}");
        return allPassed;
    }

    private static (string TestName, bool Passed, string Message) RunTest(
        string testName,
        Func<(bool Passed, string Message)> testFunc)
    {
        try
        {
            var (passed, message) = testFunc();
            return (testName, passed, message);
        }
        catch (Exception ex)
        {
            return (testName, false, $"Exception: {ex.Message}");
        }
    }

    private static (bool Passed, string Message) TestDefaultLogger()
    {
        // Reset to default
        var captureLogger = new CaptureLogger();
        FlutterSharpLogger.Logger = captureLogger;

        FlutterSharpLogger.LogInformation("Test message");

        var passed = captureLogger.Messages.Count == 1 &&
                     captureLogger.Messages[0].LogLevel == LogLevel.Information;

        return (passed, passed ? "" : $"Expected 1 message, got {captureLogger.Messages.Count}");
    }

    private static (bool Passed, string Message) TestVerboseLogging()
    {
        var captureLogger = new CaptureLogger();
        FlutterSharpLogger.Logger = captureLogger;

        // Debug should be suppressed when verbose is disabled
        FlutterSharpLogger.VerboseLoggingEnabled = false;
        FlutterSharpLogger.LogDebug("Should not appear");

        if (captureLogger.Messages.Count != 0)
        {
            return (false, "Debug message logged when verbose disabled");
        }

        // Debug should appear when verbose is enabled
        FlutterSharpLogger.VerboseLoggingEnabled = true;
        FlutterSharpLogger.LogDebug("Should appear");

        var passed = captureLogger.Messages.Count == 1 &&
                     captureLogger.Messages[0].LogLevel == LogLevel.Debug;

        return (passed, passed ? "" : "Debug message not logged when verbose enabled");
    }

    private static (bool Passed, string Message) TestNullLogger()
    {
        FlutterSharpLogger.Logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        // Should not throw even with null logger
        FlutterSharpLogger.LogInformation("Test");
        FlutterSharpLogger.LogDebug("Test");
        FlutterSharpLogger.LogError("Test");

        return (true, "");
    }

    private static (bool Passed, string Message) TestCustomLogger()
    {
        var customLogger = new CaptureLogger();
        FlutterSharpLogger.Logger = customLogger;

        FlutterSharpLogger.LogWarning("Custom warning");

        var passed = customLogger.Messages.Count == 1 &&
                     customLogger.Messages[0].LogLevel == LogLevel.Warning;

        return (passed, passed ? "" : "Custom logger not working");
    }

    private static (bool Passed, string Message) TestExceptionLogging()
    {
        var captureLogger = new CaptureLogger();
        FlutterSharpLogger.Logger = captureLogger;

        var exception = new InvalidOperationException("Test exception");
        FlutterSharpLogger.LogError(exception, "Error occurred");

        var passed = captureLogger.Messages.Count == 1 &&
                     captureLogger.Messages[0].Exception == exception;

        return (passed, passed ? "" : "Exception not captured correctly");
    }

    private static (bool Passed, string Message) TestStructuredLogging()
    {
        var captureLogger = new CaptureLogger();
        FlutterSharpLogger.Logger = captureLogger;

        FlutterSharpLogger.LogInformation("User {UserId} logged in at {Time}", 123, DateTime.Now);

        var passed = captureLogger.Messages.Count == 1 &&
                     captureLogger.Messages[0].Message.Contains("User");

        return (passed, passed ? "" : "Structured logging not working");
    }

    private static (bool Passed, string Message) TestLoggingNeverThrows()
    {
        // Even with a throwing logger, logging should never throw
        FlutterSharpLogger.Logger = new ThrowingLogger();

        try
        {
            FlutterSharpLogger.LogInformation("Test");
            FlutterSharpLogger.LogError("Test");
            FlutterSharpLogger.LogDebug("Test");
            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, $"Logging threw exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Test logger that captures all log messages.
    /// </summary>
    private class CaptureLogger : ILogger
    {
        public List<LogEntry> Messages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(new LogEntry
            {
                LogLevel = logLevel,
                EventId = eventId,
                Message = formatter(state, exception),
                Exception = exception
            });
        }
    }

    /// <summary>
    /// Logger that throws exceptions to test error handling.
    /// </summary>
    private class ThrowingLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new InvalidOperationException("BeginScope threw");
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new InvalidOperationException("IsEnabled threw");
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            throw new InvalidOperationException("Log threw");
        }
    }

    private class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public EventId EventId { get; set; }
        public string Message { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
    }
}

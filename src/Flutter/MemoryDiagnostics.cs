using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Flutter
{
    /// <summary>
    /// Provides memory leak detection and diagnostics for FlutterSharp.
    /// Tracks widget lifecycles, callback registrations, and struct allocations
    /// to help identify potential memory leaks.
    /// </summary>
    public static class MemoryDiagnostics
    {
        private static bool _isEnabled = false;
        private static readonly object _lock = new object();

        // Widget lifecycle tracking
        private static readonly ConcurrentDictionary<string, WidgetAllocationInfo> _widgetAllocations = new();
        private static long _totalWidgetsCreated = 0;
        private static long _totalWidgetsDisposed = 0;

        // Struct lifecycle tracking
        private static readonly ConcurrentDictionary<IntPtr, StructAllocationInfo> _structAllocations = new();
        private static long _totalStructsCreated = 0;
        private static long _totalStructsDisposed = 0;

        // Callback tracking
        private static long _totalCallbacksRegistered = 0;
        private static long _totalCallbacksUnregistered = 0;

        // Allocation tracking thresholds
        private static int _leakWarningThreshold = 100;
        private static TimeSpan _staleAllocationThreshold = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets whether memory diagnostics tracking is enabled.
        /// When disabled, tracking methods are no-ops for minimal performance impact.
        /// </summary>
        public static bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        /// <summary>
        /// Gets or sets the threshold for warning about potential widget leaks.
        /// A warning is raised if more widgets are alive than this threshold.
        /// </summary>
        public static int LeakWarningThreshold
        {
            get => _leakWarningThreshold;
            set => _leakWarningThreshold = value;
        }

        /// <summary>
        /// Gets or sets the threshold for marking allocations as stale.
        /// Allocations older than this are considered potential leaks.
        /// </summary>
        public static TimeSpan StaleAllocationThreshold
        {
            get => _staleAllocationThreshold;
            set => _staleAllocationThreshold = value;
        }

        /// <summary>
        /// Event raised when a potential memory leak is detected.
        /// </summary>
        public static event EventHandler<MemoryLeakEventArgs> OnLeakDetected;

        /// <summary>
        /// Tracks a widget creation for memory diagnostics.
        /// </summary>
        /// <param name="widget">The widget being created</param>
        internal static void TrackWidgetCreation(Widget widget)
        {
            if (!_isEnabled || widget == null)
                return;

            Interlocked.Increment(ref _totalWidgetsCreated);

            var info = new WidgetAllocationInfo
            {
                WidgetId = widget.Id,
                WidgetType = widget.GetType().Name,
                CreatedAt = DateTime.UtcNow,
                StackTrace = Environment.StackTrace
            };

            _widgetAllocations[widget.Id] = info;

            CheckLeakThreshold();
        }

        /// <summary>
        /// Tracks a widget disposal for memory diagnostics.
        /// </summary>
        /// <param name="widget">The widget being disposed</param>
        internal static void TrackWidgetDisposal(Widget widget)
        {
            if (!_isEnabled || widget == null)
                return;

            Interlocked.Increment(ref _totalWidgetsDisposed);

            if (_widgetAllocations.TryRemove(widget.Id, out var info))
            {
                info.DisposedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Tracks a struct allocation for memory diagnostics.
        /// </summary>
        /// <param name="handle">The GCHandle IntPtr of the struct</param>
        /// <param name="structType">The type name of the struct</param>
        internal static void TrackStructCreation(IntPtr handle, string structType)
        {
            if (!_isEnabled || handle == IntPtr.Zero)
                return;

            Interlocked.Increment(ref _totalStructsCreated);

            var info = new StructAllocationInfo
            {
                Handle = handle,
                StructType = structType,
                CreatedAt = DateTime.UtcNow,
                StackTrace = Environment.StackTrace
            };

            _structAllocations[handle] = info;
        }

        /// <summary>
        /// Tracks a struct disposal for memory diagnostics.
        /// </summary>
        /// <param name="handle">The GCHandle IntPtr of the struct being disposed</param>
        internal static void TrackStructDisposal(IntPtr handle)
        {
            if (!_isEnabled || handle == IntPtr.Zero)
                return;

            Interlocked.Increment(ref _totalStructsDisposed);
            _structAllocations.TryRemove(handle, out _);
        }

        /// <summary>
        /// Tracks a callback registration for memory diagnostics.
        /// </summary>
        internal static void TrackCallbackRegistration()
        {
            if (!_isEnabled)
                return;

            Interlocked.Increment(ref _totalCallbacksRegistered);
        }

        /// <summary>
        /// Tracks a callback unregistration for memory diagnostics.
        /// </summary>
        internal static void TrackCallbackUnregistration()
        {
            if (!_isEnabled)
                return;

            Interlocked.Increment(ref _totalCallbacksUnregistered);
        }

        /// <summary>
        /// Gets current memory statistics snapshot.
        /// </summary>
        /// <returns>A snapshot of current memory statistics</returns>
        public static MemoryStats GetStats()
        {
            return new MemoryStats
            {
                TotalWidgetsCreated = _totalWidgetsCreated,
                TotalWidgetsDisposed = _totalWidgetsDisposed,
                AliveWidgets = _widgetAllocations.Count,
                TotalStructsCreated = _totalStructsCreated,
                TotalStructsDisposed = _totalStructsDisposed,
                AliveStructs = _structAllocations.Count,
                TotalCallbacksRegistered = _totalCallbacksRegistered,
                TotalCallbacksUnregistered = _totalCallbacksUnregistered,
                RegisteredCallbacks = CallbackRegistry.Count,
                DiagnosticsEnabled = _isEnabled
            };
        }

        /// <summary>
        /// Gets detailed information about widgets that may be leaking.
        /// </summary>
        /// <returns>List of allocation info for potentially leaked widgets</returns>
        public static List<WidgetAllocationInfo> GetPotentialWidgetLeaks()
        {
            var threshold = DateTime.UtcNow - _staleAllocationThreshold;
            return _widgetAllocations.Values
                .Where(w => w.CreatedAt < threshold)
                .OrderBy(w => w.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Gets detailed information about structs that may be leaking.
        /// </summary>
        /// <returns>List of allocation info for potentially leaked structs</returns>
        public static List<StructAllocationInfo> GetPotentialStructLeaks()
        {
            var threshold = DateTime.UtcNow - _staleAllocationThreshold;
            return _structAllocations.Values
                .Where(s => s.CreatedAt < threshold)
                .OrderBy(s => s.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Gets a summary of widgets grouped by type.
        /// </summary>
        /// <returns>Dictionary of widget type to count of alive widgets</returns>
        public static Dictionary<string, int> GetWidgetTypeCounts()
        {
            return _widgetAllocations.Values
                .GroupBy(w => w.WidgetType)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Gets a summary of structs grouped by type.
        /// </summary>
        /// <returns>Dictionary of struct type to count of alive structs</returns>
        public static Dictionary<string, int> GetStructTypeCounts()
        {
            return _structAllocations.Values
                .GroupBy(s => s.StructType)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Generates a comprehensive memory diagnostic report.
        /// </summary>
        /// <returns>A formatted diagnostic report string</returns>
        public static string GenerateReport()
        {
            var stats = GetStats();
            var widgetLeaks = GetPotentialWidgetLeaks();
            var structLeaks = GetPotentialStructLeaks();
            var widgetTypes = GetWidgetTypeCounts();
            var structTypes = GetStructTypeCounts();

            var report = new System.Text.StringBuilder();
            report.AppendLine("=== FlutterSharp Memory Diagnostics Report ===");
            report.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine($"Diagnostics Enabled: {stats.DiagnosticsEnabled}");
            report.AppendLine();

            // Overview
            report.AppendLine("--- Overview ---");
            report.AppendLine($"Widgets: {stats.AliveWidgets} alive ({stats.TotalWidgetsCreated} created, {stats.TotalWidgetsDisposed} disposed)");
            report.AppendLine($"Structs: {stats.AliveStructs} alive ({stats.TotalStructsCreated} created, {stats.TotalStructsDisposed} disposed)");
            report.AppendLine($"Callbacks: {stats.RegisteredCallbacks} registered ({stats.TotalCallbacksRegistered} total, {stats.TotalCallbacksUnregistered} unregistered)");
            report.AppendLine();

            // Widget type breakdown
            if (widgetTypes.Count > 0)
            {
                report.AppendLine("--- Widget Types (Alive) ---");
                foreach (var kv in widgetTypes.OrderByDescending(x => x.Value))
                {
                    report.AppendLine($"  {kv.Key}: {kv.Value}");
                }
                report.AppendLine();
            }

            // Struct type breakdown
            if (structTypes.Count > 0)
            {
                report.AppendLine("--- Struct Types (Alive) ---");
                foreach (var kv in structTypes.OrderByDescending(x => x.Value))
                {
                    report.AppendLine($"  {kv.Key}: {kv.Value}");
                }
                report.AppendLine();
            }

            // Potential leaks
            if (widgetLeaks.Count > 0)
            {
                report.AppendLine($"--- Potential Widget Leaks ({widgetLeaks.Count}) ---");
                report.AppendLine($"(Widgets older than {_staleAllocationThreshold.TotalMinutes} minutes)");
                foreach (var leak in widgetLeaks.Take(10))
                {
                    var age = DateTime.UtcNow - leak.CreatedAt;
                    report.AppendLine($"  [{leak.WidgetType}] ID: {leak.WidgetId.Substring(0, 8)}... Age: {age.TotalMinutes:F1}m");
                }
                if (widgetLeaks.Count > 10)
                {
                    report.AppendLine($"  ... and {widgetLeaks.Count - 10} more");
                }
                report.AppendLine();
            }

            if (structLeaks.Count > 0)
            {
                report.AppendLine($"--- Potential Struct Leaks ({structLeaks.Count}) ---");
                report.AppendLine($"(Structs older than {_staleAllocationThreshold.TotalMinutes} minutes)");
                foreach (var leak in structLeaks.Take(10))
                {
                    var age = DateTime.UtcNow - leak.CreatedAt;
                    report.AppendLine($"  [{leak.StructType}] Handle: 0x{leak.Handle:X} Age: {age.TotalMinutes:F1}m");
                }
                if (structLeaks.Count > 10)
                {
                    report.AppendLine($"  ... and {structLeaks.Count - 10} more");
                }
                report.AppendLine();
            }

            // Callback analysis
            var orphanedCallbacks = stats.TotalCallbacksRegistered - stats.TotalCallbacksUnregistered - stats.RegisteredCallbacks;
            if (orphanedCallbacks > 0)
            {
                report.AppendLine($"--- Callback Warning ---");
                report.AppendLine($"Approximately {orphanedCallbacks} callbacks may have been orphaned (registered but not unregistered)");
                report.AppendLine();
            }

            report.AppendLine("=== End Report ===");
            return report.ToString();
        }

        /// <summary>
        /// Forces a check for memory leaks and raises events if leaks are detected.
        /// </summary>
        /// <returns>True if potential leaks were detected</returns>
        public static bool CheckForLeaks()
        {
            var widgetLeaks = GetPotentialWidgetLeaks();
            var structLeaks = GetPotentialStructLeaks();
            var stats = GetStats();

            bool hasLeaks = widgetLeaks.Count > 0 || structLeaks.Count > 0;

            if (hasLeaks)
            {
                var eventArgs = new MemoryLeakEventArgs
                {
                    PotentialWidgetLeaks = widgetLeaks.Count,
                    PotentialStructLeaks = structLeaks.Count,
                    TotalAliveWidgets = stats.AliveWidgets,
                    TotalAliveStructs = stats.AliveStructs,
                    RegisteredCallbacks = stats.RegisteredCallbacks,
                    Report = GenerateReport()
                };

                OnLeakDetected?.Invoke(null, eventArgs);
            }

            return hasLeaks;
        }

        /// <summary>
        /// Resets all memory diagnostics counters and tracking data.
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _widgetAllocations.Clear();
                _structAllocations.Clear();
                Interlocked.Exchange(ref _totalWidgetsCreated, 0);
                Interlocked.Exchange(ref _totalWidgetsDisposed, 0);
                Interlocked.Exchange(ref _totalStructsCreated, 0);
                Interlocked.Exchange(ref _totalStructsDisposed, 0);
                Interlocked.Exchange(ref _totalCallbacksRegistered, 0);
                Interlocked.Exchange(ref _totalCallbacksUnregistered, 0);
            }
        }

        private static void CheckLeakThreshold()
        {
            if (_widgetAllocations.Count >= _leakWarningThreshold)
            {
                var eventArgs = new MemoryLeakEventArgs
                {
                    TotalAliveWidgets = _widgetAllocations.Count,
                    TotalAliveStructs = _structAllocations.Count,
                    RegisteredCallbacks = CallbackRegistry.Count,
                    PotentialWidgetLeaks = 0,
                    PotentialStructLeaks = 0,
                    Report = $"Warning: Widget count ({_widgetAllocations.Count}) exceeded threshold ({_leakWarningThreshold})"
                };

                OnLeakDetected?.Invoke(null, eventArgs);
            }
        }
    }

    /// <summary>
    /// Snapshot of memory allocation statistics.
    /// </summary>
    public class MemoryStats
    {
        public long TotalWidgetsCreated { get; set; }
        public long TotalWidgetsDisposed { get; set; }
        public int AliveWidgets { get; set; }
        public long TotalStructsCreated { get; set; }
        public long TotalStructsDisposed { get; set; }
        public int AliveStructs { get; set; }
        public long TotalCallbacksRegistered { get; set; }
        public long TotalCallbacksUnregistered { get; set; }
        public int RegisteredCallbacks { get; set; }
        public bool DiagnosticsEnabled { get; set; }

        public override string ToString()
        {
            return $"MemoryStats {{ Widgets: {AliveWidgets}/{TotalWidgetsCreated}, Structs: {AliveStructs}/{TotalStructsCreated}, Callbacks: {RegisteredCallbacks} }}";
        }
    }

    /// <summary>
    /// Information about a widget allocation for leak detection.
    /// </summary>
    public class WidgetAllocationInfo
    {
        public string WidgetId { get; set; }
        public string WidgetType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DisposedAt { get; set; }
        public string StackTrace { get; set; }

        public TimeSpan Age => (DisposedAt ?? DateTime.UtcNow) - CreatedAt;
    }

    /// <summary>
    /// Information about a struct allocation for leak detection.
    /// </summary>
    public class StructAllocationInfo
    {
        public IntPtr Handle { get; set; }
        public string StructType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StackTrace { get; set; }

        public TimeSpan Age => DateTime.UtcNow - CreatedAt;
    }

    /// <summary>
    /// Event args for memory leak detection events.
    /// </summary>
    public class MemoryLeakEventArgs : EventArgs
    {
        public int PotentialWidgetLeaks { get; set; }
        public int PotentialStructLeaks { get; set; }
        public int TotalAliveWidgets { get; set; }
        public int TotalAliveStructs { get; set; }
        public int RegisteredCallbacks { get; set; }
        public string Report { get; set; }
    }
}

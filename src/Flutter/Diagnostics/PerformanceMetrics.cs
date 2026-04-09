using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Flutter.Logging;

namespace Flutter.Diagnostics
{
	/// <summary>
	/// Tracks performance metrics for FlutterSharp operations.
	/// Provides real-time measurement of widget rendering, callback invocation,
	/// and message passing performance.
	/// </summary>
	public static class PerformanceMetrics
	{
		private static bool _enabled = false;
		private static readonly ConcurrentDictionary<string, MetricCounter> _counters = new();
		private static readonly ConcurrentDictionary<string, TimingMetric> _timings = new();
		private static long _frameCount = 0;
		private static Stopwatch _uptimeStopwatch = new();

		/// <summary>
		/// Gets or sets whether performance tracking is enabled.
		/// Disabled by default to minimize overhead.
		/// </summary>
		public static bool Enabled
		{
			get => _enabled;
			set
			{
				if (value && !_enabled)
				{
					_uptimeStopwatch.Restart();
				}
				_enabled = value;
			}
		}

		/// <summary>
		/// Gets the uptime since performance tracking was enabled.
		/// </summary>
		public static TimeSpan Uptime => _uptimeStopwatch.Elapsed;

		/// <summary>
		/// Gets the total frame count since tracking started.
		/// </summary>
		public static long FrameCount => _frameCount;

		#region Counter Operations

		/// <summary>
		/// Increments a named counter by one.
		/// </summary>
		public static void IncrementCounter(string name)
		{
			if (!_enabled) return;
			_counters.GetOrAdd(name, _ => new MetricCounter()).Increment();
		}

		/// <summary>
		/// Increments a named counter by a specified amount.
		/// </summary>
		public static void IncrementCounter(string name, long amount)
		{
			if (!_enabled) return;
			_counters.GetOrAdd(name, _ => new MetricCounter()).Increment(amount);
		}

		/// <summary>
		/// Gets the current value of a counter.
		/// </summary>
		public static long GetCounter(string name)
		{
			return _counters.TryGetValue(name, out var counter) ? counter.Value : 0;
		}

		#endregion

		#region Timing Operations

		/// <summary>
		/// Starts timing an operation. Returns a disposable that stops timing when disposed.
		/// </summary>
		/// <param name="operationName">Name of the operation to time</param>
		/// <returns>IDisposable that stops timing when disposed</returns>
		/// <example>
		/// using (PerformanceMetrics.StartTiming("WidgetRender"))
		/// {
		///     // ... widget rendering code ...
		/// }
		/// </example>
		public static IDisposable StartTiming(string operationName)
		{
			if (!_enabled) return NullDisposable.Instance;
			return new TimingScope(operationName);
		}

		/// <summary>
		/// Records a timing measurement directly.
		/// </summary>
		public static void RecordTiming(string operationName, TimeSpan duration)
		{
			if (!_enabled) return;
			_timings.GetOrAdd(operationName, _ => new TimingMetric()).Record(duration);
		}

		/// <summary>
		/// Gets timing statistics for an operation.
		/// </summary>
		public static TimingStats? GetTimingStats(string operationName)
		{
			return _timings.TryGetValue(operationName, out var timing)
				? timing.GetStats()
				: null;
		}

		#endregion

		#region Frame Tracking

		/// <summary>
		/// Signals that a frame has been rendered. Used for FPS calculation.
		/// </summary>
		public static void RecordFrame()
		{
			if (!_enabled) return;
			Interlocked.Increment(ref _frameCount);
		}

		/// <summary>
		/// Gets the average FPS since tracking started.
		/// </summary>
		public static double GetAverageFps()
		{
			var uptime = _uptimeStopwatch.Elapsed.TotalSeconds;
			return uptime > 0 ? _frameCount / uptime : 0;
		}

		#endregion

		#region Reporting

		/// <summary>
		/// Gets a snapshot of all current performance metrics.
		/// </summary>
		public static PerformanceSnapshot GetSnapshot()
		{
			return new PerformanceSnapshot
			{
				Timestamp = DateTime.UtcNow,
				Uptime = Uptime,
				FrameCount = _frameCount,
				AverageFps = GetAverageFps(),
				Counters = _counters.ToDictionary(kv => kv.Key, kv => kv.Value.Value),
				Timings = _timings.ToDictionary(kv => kv.Key, kv => kv.Value.GetStats())
			};
		}

		/// <summary>
		/// Resets all performance counters and timings.
		/// </summary>
		public static void Reset()
		{
			_counters.Clear();
			_timings.Clear();
			_frameCount = 0;
			_uptimeStopwatch.Restart();
		}

		/// <summary>
		/// Generates a human-readable performance report.
		/// </summary>
		public static string GenerateReport()
		{
			var snapshot = GetSnapshot();
			var report = new System.Text.StringBuilder();

			report.AppendLine("=== FlutterSharp Performance Report ===");
			report.AppendLine($"Timestamp: {snapshot.Timestamp:yyyy-MM-dd HH:mm:ss}");
			report.AppendLine($"Uptime: {snapshot.Uptime.TotalSeconds:F2}s");
			report.AppendLine($"Frames: {snapshot.FrameCount:N0}");
			report.AppendLine($"Average FPS: {snapshot.AverageFps:F1}");
			report.AppendLine();

			if (snapshot.Counters.Count > 0)
			{
				report.AppendLine("Counters:");
				foreach (var (name, value) in snapshot.Counters.OrderByDescending(kv => kv.Value))
				{
					report.AppendLine($"  {name}: {value:N0}");
				}
				report.AppendLine();
			}

			if (snapshot.Timings.Count > 0)
			{
				report.AppendLine("Timings:");
				foreach (var (name, stats) in snapshot.Timings.OrderByDescending(kv => kv.Value.TotalTime))
				{
					report.AppendLine($"  {name}:");
					report.AppendLine($"    Count: {stats.Count:N0}");
					report.AppendLine($"    Total: {stats.TotalTime.TotalMilliseconds:F2}ms");
					report.AppendLine($"    Avg: {stats.AverageTime.TotalMicroseconds:F1}µs");
					report.AppendLine($"    Min: {stats.MinTime.TotalMicroseconds:F1}µs");
					report.AppendLine($"    Max: {stats.MaxTime.TotalMicroseconds:F1}µs");
				}
			}

			return report.ToString();
		}

		/// <summary>
		/// Logs the performance report using the configured logger.
		/// </summary>
		public static void LogReport()
		{
			FlutterSharpLogger.LogInformation(GenerateReport());
		}

		#endregion

		#region Helper Classes

		private class MetricCounter
		{
			private long _value;
			public long Value => _value;
			public void Increment() => Interlocked.Increment(ref _value);
			public void Increment(long amount) => Interlocked.Add(ref _value, amount);
		}

		private class TimingMetric
		{
			private readonly object _lock = new();
			private long _count;
			private TimeSpan _total;
			private TimeSpan _min = TimeSpan.MaxValue;
			private TimeSpan _max = TimeSpan.Zero;
			private readonly List<double> _samples = new(); // Store recent samples for percentile calculation
			private const int MaxSamples = 1000;

			public void Record(TimeSpan duration)
			{
				lock (_lock)
				{
					_count++;
					_total += duration;
					if (duration < _min) _min = duration;
					if (duration > _max) _max = duration;

					_samples.Add(duration.TotalMicroseconds);
					if (_samples.Count > MaxSamples)
					{
						_samples.RemoveAt(0);
					}
				}
			}

			public TimingStats GetStats()
			{
				lock (_lock)
				{
					var stats = new TimingStats
					{
						Count = _count,
						TotalTime = _total,
						MinTime = _count > 0 ? _min : TimeSpan.Zero,
						MaxTime = _max,
						AverageTime = _count > 0 ? TimeSpan.FromTicks(_total.Ticks / _count) : TimeSpan.Zero
					};

					if (_samples.Count > 0)
					{
						var sorted = _samples.OrderBy(x => x).ToList();
						stats.P50Time = TimeSpan.FromMicroseconds(sorted[(int)(sorted.Count * 0.50)]);
						stats.P95Time = TimeSpan.FromMicroseconds(sorted[(int)(sorted.Count * 0.95)]);
						stats.P99Time = TimeSpan.FromMicroseconds(sorted[(int)(sorted.Count * 0.99)]);
					}

					return stats;
				}
			}
		}

		private class TimingScope : IDisposable
		{
			private readonly string _operationName;
			private readonly Stopwatch _stopwatch;

			public TimingScope(string operationName)
			{
				_operationName = operationName;
				_stopwatch = Stopwatch.StartNew();
			}

			public void Dispose()
			{
				_stopwatch.Stop();
				RecordTiming(_operationName, _stopwatch.Elapsed);
			}
		}

		private class NullDisposable : IDisposable
		{
			public static readonly NullDisposable Instance = new();
			public void Dispose() { }
		}

		#endregion
	}

	/// <summary>
	/// Statistics for a timed operation.
	/// </summary>
	public class TimingStats
	{
		public long Count { get; set; }
		public TimeSpan TotalTime { get; set; }
		public TimeSpan AverageTime { get; set; }
		public TimeSpan MinTime { get; set; }
		public TimeSpan MaxTime { get; set; }
		public TimeSpan P50Time { get; set; }
		public TimeSpan P95Time { get; set; }
		public TimeSpan P99Time { get; set; }
	}

	/// <summary>
	/// A snapshot of performance metrics at a point in time.
	/// </summary>
	public class PerformanceSnapshot
	{
		public DateTime Timestamp { get; set; }
		public TimeSpan Uptime { get; set; }
		public long FrameCount { get; set; }
		public double AverageFps { get; set; }
		public Dictionary<string, long> Counters { get; set; } = new();
		public Dictionary<string, TimingStats> Timings { get; set; } = new();
	}

	/// <summary>
	/// Well-known metric names used throughout FlutterSharp.
	/// </summary>
	public static class MetricNames
	{
		// Widget lifecycle
		public const string WidgetsCreated = "widgets.created";
		public const string WidgetsDisposed = "widgets.disposed";
		public const string WidgetsPrepared = "widgets.prepared";

		// Callbacks
		public const string CallbacksRegistered = "callbacks.registered";
		public const string CallbacksInvoked = "callbacks.invoked";
		public const string CallbacksUnregistered = "callbacks.unregistered";

		// Events
		public const string EventsReceived = "events.received";
		public const string EventsHandled = "events.handled";
		public const string EventsRouted = "events.routed";

		// Messages
		public const string MessagesSent = "messages.sent";
		public const string MessagesReceived = "messages.received";

		// Timing operations
		public const string WidgetCreation = "timing.widget_creation";
		public const string WidgetPrepare = "timing.widget_prepare";
		public const string CallbackInvocation = "timing.callback_invocation";
		public const string EventRouting = "timing.event_routing";
		public const string MessageSerialization = "timing.message_serialization";
		public const string StructAllocation = "timing.struct_allocation";
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Flutter.Logging;

namespace Flutter.Diagnostics
{
	/// <summary>
	/// Tracks rendering performance metrics including FPS, frame times, and jank detection.
	/// Provides real-time monitoring for Flutter rendering performance.
	/// </summary>
	public static class RenderingMetrics
	{
		private static bool _enabled = false;
		private static readonly object _lock = new();

		// Frame tracking
		private static readonly Stopwatch _frameTimer = new();
		private static readonly Queue<FrameInfo> _recentFrames = new();
		private static readonly List<JankEvent> _jankEvents = new();

		// Configuration
		private const int MaxRecentFrames = 120; // Store 2 seconds at 60fps
		private const int MaxJankEvents = 100;
		private const double DefaultTargetFps = 60.0;
		private const double JankThresholdMultiplier = 2.0; // Frame > 2x target is jank

		// Statistics
		private static long _totalFrameCount;
		private static long _totalJankFrames;
		private static TimeSpan _totalFrameTime;
		private static TimeSpan _worstFrameTime;
		private static double _targetFps = DefaultTargetFps;

		// Events
		public static event EventHandler<JankEventArgs>? OnJankDetected;
		public static event EventHandler<FrameEventArgs>? OnFrameComplete;

		/// <summary>
		/// Gets or sets whether rendering metrics tracking is enabled.
		/// </summary>
		public static bool Enabled
		{
			get => _enabled;
			set
			{
				if (value && !_enabled)
				{
					Reset();
				}
				_enabled = value;
			}
		}

		/// <summary>
		/// Gets or sets the target FPS for jank calculation. Default is 60.
		/// </summary>
		public static double TargetFps
		{
			get => _targetFps;
			set => _targetFps = value > 0 ? value : DefaultTargetFps;
		}

		/// <summary>
		/// Gets the target frame time based on target FPS.
		/// </summary>
		public static TimeSpan TargetFrameTime => TimeSpan.FromMilliseconds(1000.0 / _targetFps);

		/// <summary>
		/// Gets the threshold above which a frame is considered jank.
		/// </summary>
		public static TimeSpan JankThreshold => TimeSpan.FromMilliseconds(1000.0 / _targetFps * JankThresholdMultiplier);

		#region Frame Recording

		/// <summary>
		/// Signals the start of a new frame. Call before rendering.
		/// </summary>
		public static void BeginFrame()
		{
			if (!_enabled) return;
			_frameTimer.Restart();
		}

		/// <summary>
		/// Signals the end of a frame. Call after rendering is complete.
		/// Records frame time and checks for jank.
		/// </summary>
		public static void EndFrame()
		{
			if (!_enabled) return;

			_frameTimer.Stop();
			var frameTime = _frameTimer.Elapsed;

			RecordFrame(frameTime, FrameSource.CSharp);
		}

		/// <summary>
		/// Records a frame with explicit timing and source.
		/// </summary>
		public static void RecordFrame(TimeSpan frameTime, FrameSource source)
		{
			if (!_enabled) return;

			var isJank = frameTime > JankThreshold;
			var frameInfo = new FrameInfo
			{
				Timestamp = DateTime.UtcNow,
				FrameTime = frameTime,
				Source = source,
				IsJank = isJank,
				FrameNumber = Interlocked.Increment(ref _totalFrameCount)
			};

			lock (_lock)
			{
				// Update statistics
				_totalFrameTime += frameTime;
				if (frameTime > _worstFrameTime)
				{
					_worstFrameTime = frameTime;
				}

				// Add to recent frames
				_recentFrames.Enqueue(frameInfo);
				while (_recentFrames.Count > MaxRecentFrames)
				{
					_recentFrames.Dequeue();
				}

				// Track jank
				if (isJank)
				{
					Interlocked.Increment(ref _totalJankFrames);
					var jankEvent = new JankEvent
					{
						Timestamp = frameInfo.Timestamp,
						FrameNumber = frameInfo.FrameNumber,
						FrameTime = frameTime,
						ExpectedFrameTime = TargetFrameTime,
						Source = source
					};

					_jankEvents.Add(jankEvent);
					while (_jankEvents.Count > MaxJankEvents)
					{
						_jankEvents.RemoveAt(0);
					}

					OnJankDetected?.Invoke(null, new JankEventArgs(jankEvent));

					FlutterSharpLogger.LogWarning(
						"Jank detected: Frame {FrameNumber} took {FrameTimeMs:F2}ms (target: {TargetMs:F2}ms, source: {Source})",
						frameInfo.FrameNumber,
						frameTime.TotalMilliseconds,
						TargetFrameTime.TotalMilliseconds,
						source
					);
				}
			}

			OnFrameComplete?.Invoke(null, new FrameEventArgs(frameInfo));

			// Also record to general performance metrics
			PerformanceMetrics.RecordFrame();
			PerformanceMetrics.RecordTiming($"frame.{source.ToString().ToLower()}", frameTime);
		}

		/// <summary>
		/// Records frame timing received from Dart side.
		/// </summary>
		/// <param name="frameTimeMs">Frame time in milliseconds</param>
		/// <param name="buildTimeMs">Widget build time in milliseconds</param>
		/// <param name="rasterTimeMs">Rasterization time in milliseconds</param>
		public static void RecordDartFrame(double frameTimeMs, double buildTimeMs, double rasterTimeMs)
		{
			if (!_enabled) return;

			var frameTime = TimeSpan.FromMilliseconds(frameTimeMs);
			RecordFrame(frameTime, FrameSource.Dart);

			PerformanceMetrics.RecordTiming("frame.dart.build", TimeSpan.FromMilliseconds(buildTimeMs));
			PerformanceMetrics.RecordTiming("frame.dart.raster", TimeSpan.FromMilliseconds(rasterTimeMs));
		}

		#endregion

		#region FPS Calculation

		/// <summary>
		/// Gets the current FPS based on recent frames (rolling average).
		/// </summary>
		public static double GetCurrentFps()
		{
			lock (_lock)
			{
				if (_recentFrames.Count < 2)
					return 0;

				var frames = _recentFrames.ToArray();
				var oldestTime = frames[0].Timestamp;
				var newestTime = frames[^1].Timestamp;
				var duration = (newestTime - oldestTime).TotalSeconds;

				if (duration <= 0)
					return 0;

				return (frames.Length - 1) / duration;
			}
		}

		/// <summary>
		/// Gets the average FPS over all recorded frames.
		/// </summary>
		public static double GetAverageFps()
		{
			var totalSeconds = _totalFrameTime.TotalSeconds;
			return totalSeconds > 0 ? _totalFrameCount / totalSeconds : 0;
		}

		/// <summary>
		/// Gets the 1% low FPS (excludes worst 1% frames).
		/// </summary>
		public static double Get1PercentLowFps()
		{
			lock (_lock)
			{
				if (_recentFrames.Count < 10)
					return 0;

				var frameTimes = new List<double>();
				foreach (var frame in _recentFrames)
				{
					frameTimes.Add(frame.FrameTime.TotalMilliseconds);
				}

				frameTimes.Sort();
				frameTimes.Reverse(); // Worst first

				// Take worst 1%
				var worstCount = Math.Max(1, frameTimes.Count / 100);
				var worstAvgMs = 0.0;
				for (int i = 0; i < worstCount; i++)
				{
					worstAvgMs += frameTimes[i];
				}
				worstAvgMs /= worstCount;

				return worstAvgMs > 0 ? 1000.0 / worstAvgMs : 0;
			}
		}

		#endregion

		#region Statistics

		/// <summary>
		/// Gets a snapshot of current rendering statistics.
		/// </summary>
		public static RenderingStats GetStats()
		{
			lock (_lock)
			{
				var stats = new RenderingStats
				{
					TotalFrameCount = _totalFrameCount,
					TotalJankFrames = _totalJankFrames,
					JankPercentage = _totalFrameCount > 0
						? (double)_totalJankFrames / _totalFrameCount * 100
						: 0,
					CurrentFps = GetCurrentFps(),
					AverageFps = GetAverageFps(),
					OnePercentLowFps = Get1PercentLowFps(),
					TargetFps = _targetFps,
					WorstFrameTime = _worstFrameTime,
					AverageFrameTime = _totalFrameCount > 0
						? TimeSpan.FromTicks(_totalFrameTime.Ticks / _totalFrameCount)
						: TimeSpan.Zero
				};

				// Calculate frame time percentiles from recent frames
				if (_recentFrames.Count > 0)
				{
					var frameTimes = new List<double>();
					foreach (var frame in _recentFrames)
					{
						frameTimes.Add(frame.FrameTime.TotalMilliseconds);
					}
					frameTimes.Sort();

					stats.P50FrameTime = TimeSpan.FromMilliseconds(frameTimes[(int)(frameTimes.Count * 0.50)]);
					stats.P95FrameTime = TimeSpan.FromMilliseconds(frameTimes[(int)(frameTimes.Count * 0.95)]);
					stats.P99FrameTime = TimeSpan.FromMilliseconds(frameTimes[Math.Min(frameTimes.Count - 1, (int)(frameTimes.Count * 0.99))]);
				}

				return stats;
			}
		}

		/// <summary>
		/// Gets recent jank events.
		/// </summary>
		public static IReadOnlyList<JankEvent> GetRecentJankEvents()
		{
			lock (_lock)
			{
				return _jankEvents.ToArray();
			}
		}

		/// <summary>
		/// Gets a copy of recent frame data.
		/// </summary>
		public static IReadOnlyList<FrameInfo> GetRecentFrames()
		{
			lock (_lock)
			{
				return _recentFrames.ToArray();
			}
		}

		#endregion

		#region Reporting

		/// <summary>
		/// Generates a human-readable rendering performance report.
		/// </summary>
		public static string GenerateReport()
		{
			var stats = GetStats();
			var report = new System.Text.StringBuilder();

			report.AppendLine("=== FlutterSharp Rendering Metrics Report ===");
			report.AppendLine();
			report.AppendLine("FPS Statistics:");
			report.AppendLine($"  Current FPS: {stats.CurrentFps:F1}");
			report.AppendLine($"  Average FPS: {stats.AverageFps:F1}");
			report.AppendLine($"  1% Low FPS: {stats.OnePercentLowFps:F1}");
			report.AppendLine($"  Target FPS: {stats.TargetFps:F0}");
			report.AppendLine();

			report.AppendLine("Frame Statistics:");
			report.AppendLine($"  Total Frames: {stats.TotalFrameCount:N0}");
			report.AppendLine($"  Average Frame Time: {stats.AverageFrameTime.TotalMilliseconds:F2}ms");
			report.AppendLine($"  P50 Frame Time: {stats.P50FrameTime.TotalMilliseconds:F2}ms");
			report.AppendLine($"  P95 Frame Time: {stats.P95FrameTime.TotalMilliseconds:F2}ms");
			report.AppendLine($"  P99 Frame Time: {stats.P99FrameTime.TotalMilliseconds:F2}ms");
			report.AppendLine($"  Worst Frame Time: {stats.WorstFrameTime.TotalMilliseconds:F2}ms");
			report.AppendLine();

			report.AppendLine("Jank Statistics:");
			report.AppendLine($"  Total Jank Frames: {stats.TotalJankFrames:N0}");
			report.AppendLine($"  Jank Percentage: {stats.JankPercentage:F2}%");
			report.AppendLine($"  Jank Threshold: {JankThreshold.TotalMilliseconds:F2}ms");

			var recentJank = GetRecentJankEvents();
			if (recentJank.Count > 0)
			{
				report.AppendLine();
				report.AppendLine($"Recent Jank Events (last {recentJank.Count}):");
				for (int i = Math.Max(0, recentJank.Count - 5); i < recentJank.Count; i++)
				{
					var jank = recentJank[i];
					report.AppendLine($"  Frame #{jank.FrameNumber}: {jank.FrameTime.TotalMilliseconds:F2}ms ({jank.Source})");
				}
			}

			return report.ToString();
		}

		/// <summary>
		/// Logs the rendering metrics report.
		/// </summary>
		public static void LogReport()
		{
			FlutterSharpLogger.LogInformation(GenerateReport());
		}

		/// <summary>
		/// Resets all rendering metrics.
		/// </summary>
		public static void Reset()
		{
			lock (_lock)
			{
				_recentFrames.Clear();
				_jankEvents.Clear();
				_totalFrameCount = 0;
				_totalJankFrames = 0;
				_totalFrameTime = TimeSpan.Zero;
				_worstFrameTime = TimeSpan.Zero;
			}
		}

		#endregion
	}

	/// <summary>
	/// Information about a single rendered frame.
	/// </summary>
	public struct FrameInfo
	{
		public DateTime Timestamp;
		public TimeSpan FrameTime;
		public FrameSource Source;
		public bool IsJank;
		public long FrameNumber;
	}

	/// <summary>
	/// Information about a jank event.
	/// </summary>
	public class JankEvent
	{
		public DateTime Timestamp { get; set; }
		public long FrameNumber { get; set; }
		public TimeSpan FrameTime { get; set; }
		public TimeSpan ExpectedFrameTime { get; set; }
		public FrameSource Source { get; set; }

		/// <summary>
		/// How much slower this frame was compared to target.
		/// </summary>
		public double SlowdownFactor => ExpectedFrameTime.TotalMilliseconds > 0
			? FrameTime.TotalMilliseconds / ExpectedFrameTime.TotalMilliseconds
			: 0;
	}

	/// <summary>
	/// Source of frame timing information.
	/// </summary>
	public enum FrameSource
	{
		/// <summary>Frame timing from C# side.</summary>
		CSharp,
		/// <summary>Frame timing from Dart/Flutter side.</summary>
		Dart,
		/// <summary>Combined frame timing.</summary>
		Combined
	}

	/// <summary>
	/// Rendering performance statistics.
	/// </summary>
	public class RenderingStats
	{
		public long TotalFrameCount { get; set; }
		public long TotalJankFrames { get; set; }
		public double JankPercentage { get; set; }
		public double CurrentFps { get; set; }
		public double AverageFps { get; set; }
		public double OnePercentLowFps { get; set; }
		public double TargetFps { get; set; }
		public TimeSpan AverageFrameTime { get; set; }
		public TimeSpan WorstFrameTime { get; set; }
		public TimeSpan P50FrameTime { get; set; }
		public TimeSpan P95FrameTime { get; set; }
		public TimeSpan P99FrameTime { get; set; }
	}

	/// <summary>
	/// Event args for jank detection events.
	/// </summary>
	public class JankEventArgs : EventArgs
	{
		public JankEvent JankEvent { get; }
		public JankEventArgs(JankEvent jankEvent) => JankEvent = jankEvent;
	}

	/// <summary>
	/// Event args for frame complete events.
	/// </summary>
	public class FrameEventArgs : EventArgs
	{
		public FrameInfo FrameInfo { get; }
		public FrameEventArgs(FrameInfo frameInfo) => FrameInfo = frameInfo;
	}
}

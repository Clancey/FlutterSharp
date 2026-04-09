using System;
using System.Text.Json;
using Flutter.Logging;
using Flutter.Internal;

namespace Flutter.Diagnostics
{
	/// <summary>
	/// Controller for the FlutterSharp performance overlay.
	/// Provides API to show/hide the visual performance debugging overlay
	/// and control metrics display settings.
	/// </summary>
	public static class PerformanceOverlayController
	{
		private static bool _isVisible = false;

		/// <summary>
		/// Gets whether the performance overlay is currently visible.
		/// </summary>
		public static bool IsVisible => _isVisible;

		/// <summary>
		/// Event raised when overlay visibility changes.
		/// </summary>
		public static event EventHandler<bool>? OnVisibilityChanged;

		/// <summary>
		/// Shows the performance overlay.
		/// Automatically enables rendering metrics collection.
		/// </summary>
		/// <param name="targetFps">Target FPS for jank calculations (default: 60)</param>
		/// <param name="position">Overlay position on screen</param>
		/// <param name="showGraph">Whether to show the frame time graph</param>
		/// <param name="compact">Whether to use compact mode</param>
		public static void Show(
			double targetFps = 60.0,
			OverlayPosition position = OverlayPosition.TopRight,
			bool showGraph = true,
			bool compact = false)
		{
			if (_isVisible) return;

			// Enable metrics collection
			FlutterManager.SetRenderingMetricsEnabled(true, targetFps);

			// Send message to Dart to show overlay
			var message = new
			{
				messageType = "ShowPerformanceOverlay",
				position = position.ToString(),
				showGraph = showGraph,
				compact = compact,
				targetFps = targetFps
			};

			try
			{
				if (Communicator.SendCommand == null)
				{
					FlutterSharpLogger.LogWarning("Cannot show performance overlay - SendCommand not configured");
					return;
				}

				var json = JsonSerializer.Serialize(message);
				Communicator.SendCommand(("PerformanceOverlay", json));
				_isVisible = true;
				OnVisibilityChanged?.Invoke(null, true);

				FlutterSharpLogger.LogInformation("Performance overlay shown (position: {Position}, compact: {Compact})",
					position, compact);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to show performance overlay");
			}
		}

		/// <summary>
		/// Hides the performance overlay.
		/// Note: Metrics collection remains enabled until explicitly disabled.
		/// </summary>
		public static void Hide()
		{
			if (!_isVisible) return;

			var message = new
			{
				messageType = "HidePerformanceOverlay"
			};

			try
			{
				if (Communicator.SendCommand == null)
				{
					FlutterSharpLogger.LogWarning("Cannot hide performance overlay - SendCommand not configured");
					return;
				}

				var json = JsonSerializer.Serialize(message);
				Communicator.SendCommand(("PerformanceOverlay", json));
				_isVisible = false;
				OnVisibilityChanged?.Invoke(null, false);

				FlutterSharpLogger.LogInformation("Performance overlay hidden");
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError(ex, "Failed to hide performance overlay");
			}
		}

		/// <summary>
		/// Toggles the performance overlay visibility.
		/// </summary>
		public static void Toggle()
		{
			if (_isVisible)
				Hide();
			else
				Show();
		}

		/// <summary>
		/// Shows the overlay and disables metrics collection when hidden.
		/// Use this for temporary performance analysis.
		/// </summary>
		/// <param name="targetFps">Target FPS for jank calculations</param>
		public static void ShowTemporary(double targetFps = 60.0)
		{
			Show(targetFps);

			// Subscribe to hide event to disable metrics
			void OnHide(object? sender, bool visible)
			{
				if (!visible)
				{
					FlutterManager.SetRenderingMetricsEnabled(false);
					OnVisibilityChanged -= OnHide;
				}
			}

			OnVisibilityChanged += OnHide;
		}

		/// <summary>
		/// Gets the current rendering statistics if metrics collection is enabled.
		/// </summary>
		/// <returns>Current rendering statistics, or null if not enabled</returns>
		public static RenderingStats? GetCurrentStats()
		{
			return FlutterManager.GetRenderingStats();
		}

		/// <summary>
		/// Generates a human-readable performance report.
		/// </summary>
		/// <returns>Performance report string</returns>
		public static string GenerateReport()
		{
			return RenderingMetrics.GenerateReport();
		}

		/// <summary>
		/// Logs the current performance metrics report.
		/// </summary>
		public static void LogReport()
		{
			RenderingMetrics.LogReport();
		}
	}

	/// <summary>
	/// Position for the performance overlay.
	/// </summary>
	public enum OverlayPosition
	{
		/// <summary>Top-left corner of the screen.</summary>
		TopLeft,
		/// <summary>Top-right corner of the screen.</summary>
		TopRight,
		/// <summary>Bottom-left corner of the screen.</summary>
		BottomLeft,
		/// <summary>Bottom-right corner of the screen.</summary>
		BottomRight
	}
}

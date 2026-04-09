using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Flutter.Internal;
using Flutter.Logging;

namespace Flutter.Diagnostics
{
	/// <summary>
	/// Provides widget tree inspection and debugging capabilities.
	/// Allows querying the Flutter widget tree, selecting widgets, and retrieving their properties.
	/// </summary>
	public static class WidgetInspector
	{
		private static bool _isEnabled;
		private static WidgetInfo? _selectedWidget;

		/// <summary>
		/// Event raised when a widget is selected in the inspector.
		/// </summary>
		public static event EventHandler<WidgetSelectedEventArgs>? OnWidgetSelected;

		/// <summary>
		/// Event raised when the widget tree is updated.
		/// </summary>
		public static event EventHandler<WidgetTreeEventArgs>? OnWidgetTreeUpdated;

		/// <summary>
		/// Gets whether the widget inspector is currently enabled.
		/// </summary>
		public static bool IsEnabled => _isEnabled;

		/// <summary>
		/// Gets the currently selected widget info, if any.
		/// </summary>
		public static WidgetInfo? SelectedWidget => _selectedWidget;

		/// <summary>
		/// Enables the widget inspector on the Dart side.
		/// </summary>
		public static async Task EnableAsync()
		{
			try
			{
				var result = await Communicator.InvokeMethodAsync("inspector.enable", null);
				if (result is bool success && success)
				{
					_isEnabled = true;
					FlutterSharpLogger.LogInformation("Widget inspector enabled");
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to enable widget inspector: {Message}", ex.Message);
			}
		}

		/// <summary>
		/// Disables the widget inspector.
		/// </summary>
		public static async Task DisableAsync()
		{
			try
			{
				var result = await Communicator.InvokeMethodAsync("inspector.disable", null);
				if (result is bool success && success)
				{
					_isEnabled = false;
					_selectedWidget = null;
					FlutterSharpLogger.LogInformation("Widget inspector disabled");
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to disable widget inspector: {Message}", ex.Message);
			}
		}

		/// <summary>
		/// Toggles the widget inspector on/off.
		/// </summary>
		/// <returns>True if the inspector is now enabled, false if disabled.</returns>
		public static async Task<bool> ToggleAsync()
		{
			try
			{
				var result = await Communicator.InvokeMethodAsync("inspector.toggle", null);
				if (result is bool enabled)
				{
					_isEnabled = enabled;
					if (!enabled)
					{
						_selectedWidget = null;
					}
					FlutterSharpLogger.LogInformation("Widget inspector toggled: {Enabled}", enabled);
					return enabled;
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to toggle widget inspector: {Message}", ex.Message);
			}
			return _isEnabled;
		}

		/// <summary>
		/// Shows the visual inspector overlay on the Flutter side.
		/// </summary>
		public static async Task ShowOverlayAsync()
		{
			try
			{
				await Communicator.InvokeMethodAsync("inspector.showOverlay", null);
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to show inspector overlay: {Message}", ex.Message);
			}
		}

		/// <summary>
		/// Hides the visual inspector overlay.
		/// </summary>
		public static async Task HideOverlayAsync()
		{
			try
			{
				await Communicator.InvokeMethodAsync("inspector.hideOverlay", null);
				_selectedWidget = null;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to hide inspector overlay: {Message}", ex.Message);
			}
		}

		/// <summary>
		/// Gets the current widget tree from Flutter.
		/// </summary>
		/// <param name="maxDepth">Maximum depth to traverse (default 10).</param>
		/// <returns>The root of the widget tree, or null if not available.</returns>
		public static async Task<WidgetTreeNode?> GetWidgetTreeAsync(int maxDepth = 10)
		{
			try
			{
				var args = new Dictionary<string, object> { { "depth", maxDepth } };
				var result = await Communicator.InvokeMethodAsync("inspector.getWidgetTree", args);

				if (result is string json && !string.IsNullOrEmpty(json))
				{
					var tree = JsonSerializer.Deserialize<WidgetTreeNode>(json, FlutterManager.serializeOptions);
					OnWidgetTreeUpdated?.Invoke(null, new WidgetTreeEventArgs(tree));
					return tree;
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to get widget tree: {Message}", ex.Message);
			}
			return null;
		}

		/// <summary>
		/// Selects a widget in the inspector by its type and hash code.
		/// </summary>
		/// <param name="widgetType">The widget type name.</param>
		/// <param name="hashCode">The widget's hash code.</param>
		public static async Task<bool> SelectWidgetAsync(string widgetType, int hashCode)
		{
			try
			{
				var args = new Dictionary<string, object>
				{
					{ "widgetType", widgetType },
					{ "hashCode", hashCode }
				};
				var result = await Communicator.InvokeMethodAsync("inspector.selectWidget", args);
				return result is bool success && success;
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to select widget: {Message}", ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Gets the currently selected widget's detailed information.
		/// </summary>
		/// <returns>The selected widget info, or null if nothing is selected.</returns>
		public static async Task<WidgetInfo?> GetSelectedWidgetAsync()
		{
			try
			{
				var result = await Communicator.InvokeMethodAsync("inspector.getSelectedWidget", null);

				if (result is string json && !string.IsNullOrEmpty(json))
				{
					return JsonSerializer.Deserialize<WidgetInfo>(json, FlutterManager.serializeOptions);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to get selected widget: {Message}", ex.Message);
			}
			return null;
		}

		/// <summary>
		/// Gets properties for a specific widget by its hash code.
		/// </summary>
		/// <param name="hashCode">The widget's hash code.</param>
		/// <returns>Dictionary of property names to values.</returns>
		public static async Task<Dictionary<string, object?>?> GetWidgetPropertiesAsync(int hashCode)
		{
			try
			{
				var args = new Dictionary<string, object> { { "hashCode", hashCode } };
				var result = await Communicator.InvokeMethodAsync("inspector.getWidgetProperties", args);

				if (result is string json && !string.IsNullOrEmpty(json))
				{
					return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, FlutterManager.serializeOptions);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to get widget properties: {Message}", ex.Message);
			}
			return null;
		}

		/// <summary>
		/// Gets render object information for a specific widget.
		/// </summary>
		/// <param name="hashCode">The widget's hash code.</param>
		/// <returns>Render object information.</returns>
		public static async Task<RenderObjectInfo?> GetRenderObjectInfoAsync(int hashCode)
		{
			try
			{
				var args = new Dictionary<string, object> { { "hashCode", hashCode } };
				var result = await Communicator.InvokeMethodAsync("inspector.getRenderObjectInfo", args);

				if (result is string json && !string.IsNullOrEmpty(json))
				{
					return JsonSerializer.Deserialize<RenderObjectInfo>(json, FlutterManager.serializeOptions);
				}
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to get render object info: {Message}", ex.Message);
			}
			return null;
		}

		/// <summary>
		/// Internal handler for widget selection notifications from Dart.
		/// </summary>
		internal static void HandleWidgetSelected(string? json)
		{
			if (string.IsNullOrEmpty(json))
			{
				_selectedWidget = null;
				OnWidgetSelected?.Invoke(null, new WidgetSelectedEventArgs(null));
				return;
			}

			try
			{
				var info = JsonSerializer.Deserialize<WidgetInfo>(json, FlutterManager.serializeOptions);
				_selectedWidget = info;
				OnWidgetSelected?.Invoke(null, new WidgetSelectedEventArgs(info));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to parse widget selection: {Message}", ex.Message);
			}
		}

		/// <summary>
		/// Internal handler for widget tree notifications from Dart.
		/// </summary>
		internal static void HandleWidgetTree(string? json)
		{
			if (string.IsNullOrEmpty(json))
			{
				OnWidgetTreeUpdated?.Invoke(null, new WidgetTreeEventArgs(null));
				return;
			}

			try
			{
				var tree = JsonSerializer.Deserialize<WidgetTreeNode>(json, FlutterManager.serializeOptions);
				OnWidgetTreeUpdated?.Invoke(null, new WidgetTreeEventArgs(tree));
			}
			catch (Exception ex)
			{
				FlutterSharpLogger.LogError("Failed to parse widget tree: {Message}", ex.Message);
			}
		}
	}

	/// <summary>
	/// Represents a node in the widget tree.
	/// </summary>
	public class WidgetTreeNode
	{
		[JsonPropertyName("widgetType")]
		public string WidgetType { get; set; } = "";

		[JsonPropertyName("hashCode")]
		public int HashCode { get; set; }

		[JsonPropertyName("key")]
		public string? Key { get; set; }

		[JsonPropertyName("depth")]
		public int Depth { get; set; }

		[JsonPropertyName("hasRenderObject")]
		public bool HasRenderObject { get; set; }

		[JsonPropertyName("size")]
		public SizeInfo? Size { get; set; }

		[JsonPropertyName("properties")]
		public Dictionary<string, object?>? Properties { get; set; }

		[JsonPropertyName("children")]
		public List<WidgetTreeNode>? Children { get; set; }
	}

	/// <summary>
	/// Represents detailed information about a widget.
	/// </summary>
	public class WidgetInfo
	{
		[JsonPropertyName("widgetType")]
		public string WidgetType { get; set; } = "";

		[JsonPropertyName("hashCode")]
		public int HashCode { get; set; }

		[JsonPropertyName("key")]
		public string? Key { get; set; }

		[JsonPropertyName("properties")]
		public Dictionary<string, object?>? Properties { get; set; }

		[JsonPropertyName("renderObject")]
		public RenderObjectInfo? RenderObject { get; set; }

		[JsonPropertyName("parentChain")]
		public List<string>? ParentChain { get; set; }
	}

	/// <summary>
	/// Represents render object information.
	/// </summary>
	public class RenderObjectInfo
	{
		[JsonPropertyName("type")]
		public string Type { get; set; } = "";

		[JsonPropertyName("hashCode")]
		public int HashCode { get; set; }

		[JsonPropertyName("needsPaint")]
		public bool NeedsPaint { get; set; }

		[JsonPropertyName("needsLayout")]
		public bool NeedsLayout { get; set; }

		[JsonPropertyName("size")]
		public SizeInfo? Size { get; set; }

		[JsonPropertyName("constraints")]
		public string? Constraints { get; set; }

		[JsonPropertyName("paintBounds")]
		public RectInfo? PaintBounds { get; set; }
	}

	/// <summary>
	/// Represents size information.
	/// </summary>
	public class SizeInfo
	{
		[JsonPropertyName("width")]
		public double Width { get; set; }

		[JsonPropertyName("height")]
		public double Height { get; set; }

		public override string ToString() => $"{Width:F1} x {Height:F1}";
	}

	/// <summary>
	/// Represents rectangle bounds.
	/// </summary>
	public class RectInfo
	{
		[JsonPropertyName("left")]
		public double Left { get; set; }

		[JsonPropertyName("top")]
		public double Top { get; set; }

		[JsonPropertyName("width")]
		public double Width { get; set; }

		[JsonPropertyName("height")]
		public double Height { get; set; }

		public override string ToString() => $"({Left:F1}, {Top:F1}) {Width:F1} x {Height:F1}";
	}

	/// <summary>
	/// Event args for widget selection events.
	/// </summary>
	public class WidgetSelectedEventArgs : EventArgs
	{
		public WidgetInfo? WidgetInfo { get; }

		public WidgetSelectedEventArgs(WidgetInfo? widgetInfo)
		{
			WidgetInfo = widgetInfo;
		}
	}

	/// <summary>
	/// Event args for widget tree update events.
	/// </summary>
	public class WidgetTreeEventArgs : EventArgs
	{
		public WidgetTreeNode? RootNode { get; }

		public WidgetTreeEventArgs(WidgetTreeNode? rootNode)
		{
			RootNode = rootNode;
		}
	}
}

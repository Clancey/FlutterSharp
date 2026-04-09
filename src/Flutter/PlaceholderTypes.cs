// Placeholder types for Flutter types that don't have direct C# equivalents
// These are stubs to allow compilation - they will be passed as IntPtr to Dart side

using System;
using System.Collections.Generic;
using Flutter.Structs;

namespace Flutter
{
	// Placeholder for unmapped types
	public class InvalidType { }

	// Flutter framework types
	// Note: BuildContext is now defined in WidgetBuilder.cs
	public class FocusNode { }
	// Note: ScrollController is now defined in Flutter.Widgets namespace
	public class ScrollPhysics { }
	public class ScrollBehavior { }
	// RouteSettings is now defined in Flutter.Navigation namespace
	public class Intent { }
	public class BackButtonDispatcher { }
	public class UndoHistoryController { }
	public class TextEditingController { }
	public class ShortcutActivator { }
	public class ScrollIncrementDetails { }
	public class NavigatorObserver { }
	public class RouteInformationProvider { }
	public class ActionDispatcher { }
	public class SelectionContainerDelegate { }
	public class TreeSliverController { }
	public class NavigatorState { }
	public class SelectableRegionState { }
	public class NavigationNotification { }
	public class ScrollNotificationPredicate { }
	public class GestureTapDownCallback { }
	public class GestureTapUpCallback { }
	public class GestureTapCallback { }
	public class GestureLongPressCallback { }
	public class GestureDragStartCallback { }
	public class GestureDragUpdateCallback { }
	public class GestureDragEndCallback { }
	public class _LocalizationsState { }
	public class _OverlayEntryLocation { }
	public class _RenderTheater { }
	public class _RenderTheaterMarker { }
	public class _OverlayEntryWidgetState { }
	public class _SharedAppDataState { }
	public class ContentInsertionConfiguration { }
	public class DismissUpdateDetails { }
	public class DraggableDetails { }
	public class DraggableScrollableController { }
	public class EditableTextState { }
	public class HeroController { }
	public class IconData { }
	public class MenuController { }
	public class PageController { }
	public class SemanticsGestureDelegate { }
	public class ShortcutManager { }
	public class SpellCheckConfiguration { }
	public class TextSelectionControls { }
	public class TextSelection { }
	public class TextInputFormatter { }
	public class TransformationController { }
	public class AnimationController { }
	public class KeyEvent { }
	public class AlignmentGeometry { }
	/// <summary>
	/// An immutable 32-bit color value in ARGB format.
	/// </summary>
	public struct Color
	{
		/// <summary>
		/// The 32-bit ARGB value of this color.
		/// </summary>
		public uint Value { get; }

		public Color(uint value) { Value = value; }

		/// <summary>
		/// Creates a Color from ARGB components (0-255 each).
		/// </summary>
		public static Color FromARGB(byte a, byte r, byte g, byte b)
			=> new Color((uint)((a << 24) | (r << 16) | (g << 8) | b));

		/// <summary>
		/// Creates a Color from RGB components (0-255 each), with full opacity.
		/// </summary>
		public static Color FromRGB(byte r, byte g, byte b)
			=> new Color((uint)((255 << 24) | (r << 16) | (g << 8) | b));
	}
	public class EdgeInsetsGeometry
	{
		public double Left { get; }
		public double Top { get; }
		public double Right { get; }
		public double Bottom { get; }

		public EdgeInsetsGeometry(double left, double top, double right, double bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public EdgeInsets ToEdgeInsets() => new EdgeInsets(Left, Top, Right, Bottom);

		public static EdgeInsetsGeometry All(double value) => new EdgeInsetsGeometry(value, value, value, value);

		public static EdgeInsetsGeometry Symmetric(double horizontal = 0, double vertical = 0)
			=> new EdgeInsetsGeometry(horizontal, vertical, horizontal, vertical);

		public static EdgeInsetsGeometry Only(double left = 0, double top = 0, double right = 0, double bottom = 0)
			=> new EdgeInsetsGeometry(left, top, right, bottom);
	}
	public struct Radius
	{
		public double X { get; }
		public double Y { get; }

		public Radius(double x, double y)
		{
			X = x;
			Y = y;
		}

		public static Radius Circular(double radius) => new Radius(radius, radius);
	}
	public class TextStyle { }
	public class Decoration { }
	public class Curve { }
	public class BorderRadiusGeometry { }
	public class SliverChildDelegate { }
	public class Matrix4 { }
	public class BoxConstraints
	{
		public double MinWidth { get; set; }
		public double MaxWidth { get; set; }
		public double MinHeight { get; set; }
		public double MaxHeight { get; set; }

		public BoxConstraints(
			double minWidth = 0.0,
			double maxWidth = double.PositiveInfinity,
			double minHeight = 0.0,
			double maxHeight = double.PositiveInfinity)
		{
			MinWidth = minWidth;
			MaxWidth = maxWidth;
			MinHeight = minHeight;
			MaxHeight = maxHeight;
		}
	}
	public class TwoDimensionalChildDelegate { }
	public class ScrollableDetails { }
	public class ListWheelChildDelegate { }
	// BoxShape is defined as an enum in Flutter.UI namespace - don't add placeholder here
	public class WebImageInfo { }
	public class ToolbarOptions { }
	public class TextMagnifierConfiguration { }
	public class TableRow { }
	public class SliverPersistentHeaderDelegate { }
	public class RawMenuOverlayInfo { }
	public class BoxBorder { }
	public class ExpansibleController { }
	public class IconThemeData { }
	public class IOSSystemContextMenuItem { }
	public class MagnifierDecoration { }
	public class OverlayEntry { }
	public class OverlayPortalController { }
	public class PageStorageBucket { }
	public class SingleChildLayoutDelegate { }
	public class MultiChildLayoutDelegate { }
	public class RenderBox { }
	public class GestureRecognizer { }
	public class GestureRecognizerFactory<T> where T : GestureRecognizer, new() { }
	public class GestureRecognizerFactory : GestureRecognizerFactory<GestureRecognizer> { }

	public enum PointerDeviceKind
	{
		Touch,
		Mouse,
		Stylus,
		InvertedStylus,
		Unknown,
		Trackpad
	}

	// Generic types
	public class Route<T> { }
	public class Future<T> { }
	public class GlobalKey<T> { }
	public class State<T> { }
	public class RouteInformationParser<T> { }
	public class RouterDelegate<T> { }
	public class RouterConfig<T> { }
	public class DragTargetDetails<T> { }
	public class LocalizationsDelegate<T> { }
	public class FlutterAction<T> { }
	public class Stream<T> { }
	public class TreeSliverNode<T> { }
	public class DragBoundaryDelegate<T> { }
	public class Page<T> { }
	public class _RouterState<T> { }
	public class Map<TKey, TValue> { }

	// Generic base widget types - using Flutter.Widgets namespace for base classes
	public class Draggable<T> : Flutter.Widgets.StatefulWidget { }
	public class ParentDataWidget<T> : Flutter.Widgets.ProxyWidget { }
	public class InheritedModel<T> : Flutter.Widgets.InheritedWidget { }
	public class InheritedNotifier<T> : Flutter.Widgets.InheritedWidget { }
	public class AbstractLayoutBuilder<T> : Flutter.Widgets.RenderObjectWidget { }
	public class ConstrainedLayoutBuilder<T> : Flutter.Widgets.RenderObjectWidget { }
	public class StreamBuilderBase<T, S> : Flutter.Widgets.StatefulWidget { }
	public class SlottedMultiChildRenderObjectWidget<TSlot, TChild> : Flutter.Widgets.RenderObjectWidget { }
	public class _DarwinPlatformView<TController, TRender> : Flutter.Widgets.StatefulWidget { }

	// Non-generic versions for generated code that doesn't specify type arguments
	public class Draggable : Draggable<object> { }
	public class ParentDataWidget : ParentDataWidget<object> { }
	public class InheritedModel : InheritedModel<object> { }
	public class InheritedNotifier : InheritedNotifier<object> { }
	public class AbstractLayoutBuilder : AbstractLayoutBuilder<object> { }
	public class ConstrainedLayoutBuilder : ConstrainedLayoutBuilder<object> { }
	public class StreamBuilderBase : StreamBuilderBase<object, object> { }
	public class SlottedMultiChildRenderObjectWidget : SlottedMultiChildRenderObjectWidget<object, object> { }
	public class _DarwinPlatformView : _DarwinPlatformView<object, object> { }

	// Note: StatefulWidget, ProxyWidget, InheritedWidget, RenderObjectWidget are now defined in Flutter.Widgets namespace

	// System types that might be missing
	public class StackTrace { }

	// Additional missing types
	public class Alignment
	{
		public static readonly Alignment TopLeft = new Alignment(-1.0, -1.0);
		public static readonly Alignment TopCenter = new Alignment(0.0, -1.0);
		public static readonly Alignment TopRight = new Alignment(1.0, -1.0);
		public static readonly Alignment CenterLeft = new Alignment(-1.0, 0.0);
		public static readonly Alignment Center = new Alignment(0.0, 0.0);
		public static readonly Alignment CenterRight = new Alignment(1.0, 0.0);
		public static readonly Alignment BottomLeft = new Alignment(-1.0, 1.0);
		public static readonly Alignment BottomCenter = new Alignment(0.0, 1.0);
		public static readonly Alignment BottomRight = new Alignment(1.0, 1.0);

		public double X { get; }
		public double Y { get; }

		public Alignment(double x, double y) { X = x; Y = y; }
	}

	/// <summary>
	/// An immutable set of offsets in each of the four cardinal directions.
	/// Typically used for an offset from each of the four sides of a box (e.g., padding).
	/// </summary>
	public struct EdgeInsets
	{
		public static readonly EdgeInsets Zero = new EdgeInsets(0, 0, 0, 0);

		public double Left { get; }
		public double Top { get; }
		public double Right { get; }
		public double Bottom { get; }

		public EdgeInsets(double left, double top, double right, double bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		/// <summary>
		/// Creates EdgeInsets with the same value for all four sides.
		/// </summary>
		public static EdgeInsets All(double value) => new EdgeInsets(value, value, value, value);

		/// <summary>
		/// Creates EdgeInsets with symmetric horizontal and vertical values.
		/// </summary>
		public static EdgeInsets Symmetric(double horizontal = 0, double vertical = 0)
			=> new EdgeInsets(horizontal, vertical, horizontal, vertical);

		/// <summary>
		/// Creates EdgeInsets with only the specified sides.
		/// </summary>
		public static EdgeInsets Only(double left = 0, double top = 0, double right = 0, double bottom = 0)
			=> new EdgeInsets(left, top, right, bottom);

		/// <summary>
		/// Creates EdgeInsets from LTRB values.
		/// </summary>
		public static EdgeInsets FromLTRB(double left, double top, double right, double bottom)
			=> new EdgeInsets(left, top, right, bottom);

		public static implicit operator EdgeInsetsGeometry(EdgeInsets value) =>
			new EdgeInsetsGeometry(value.Left, value.Top, value.Right, value.Bottom);
	}

	public class ImageFilter { }

	/// <summary>
	/// An immutable set of radii for each corner of a rectangle.
	/// </summary>
	public struct BorderRadius
	{
		public static readonly BorderRadius Zero = new BorderRadius(0, 0, 0, 0);

		public double TopLeft { get; }
		public double TopRight { get; }
		public double BottomLeft { get; }
		public double BottomRight { get; }

		public BorderRadius(double topLeft, double topRight, double bottomLeft, double bottomRight)
		{
			TopLeft = topLeft;
			TopRight = topRight;
			BottomLeft = bottomLeft;
			BottomRight = bottomRight;
		}

		/// <summary>
		/// Creates a BorderRadius where all four corners have the same radius.
		/// </summary>
		public static BorderRadius Circular(double radius) => new BorderRadius(radius, radius, radius, radius);

		/// <summary>
		/// Creates a BorderRadius with only specified corners.
		/// </summary>
		public static BorderRadius Only(
			double topLeft = 0,
			double topRight = 0,
			double bottomLeft = 0,
			double bottomRight = 0) => new BorderRadius(topLeft, topRight, bottomLeft, bottomRight);

		/// <summary>
		/// Creates a BorderRadius with vertical (top/bottom) symmetry.
		/// </summary>
		public static BorderRadius Vertical(double top = 0, double bottom = 0)
			=> new BorderRadius(top, top, bottom, bottom);

		/// <summary>
		/// Creates a BorderRadius with horizontal (left/right) symmetry.
		/// </summary>
		public static BorderRadius Horizontal(double left = 0, double right = 0)
			=> new BorderRadius(left, right, left, right);
	}

	/// <summary>
	/// Alignment for widgets in a container that accounts for text direction.
	/// </summary>
	public enum AlignmentDirectional
	{
		/// <summary>
		/// The top-start corner (top-left for LTR, top-right for RTL).
		/// </summary>
		TopStart = 0,

		/// <summary>
		/// The top-center point.
		/// </summary>
		TopCenter = 1,

		/// <summary>
		/// The top-end corner (top-right for LTR, top-left for RTL).
		/// </summary>
		TopEnd = 2,

		/// <summary>
		/// The center-start point (center-left for LTR, center-right for RTL).
		/// </summary>
		CenterStart = 3,

		/// <summary>
		/// The center point.
		/// </summary>
		Center = 4,

		/// <summary>
		/// The center-end point (center-right for LTR, center-left for RTL).
		/// </summary>
		CenterEnd = 5,

		/// <summary>
		/// The bottom-start corner (bottom-left for LTR, bottom-right for RTL).
		/// </summary>
		BottomStart = 6,

		/// <summary>
		/// The bottom-center point.
		/// </summary>
		BottomCenter = 7,

		/// <summary>
		/// The bottom-end corner (bottom-right for LTR, bottom-left for RTL).
		/// </summary>
		BottomEnd = 8,
	}

	public enum SemanticsValidationResult
	{
		None,
		Valid,
		Invalid,
	}

	// Additional missing types
	public class PlatformViewCreationParams { }
	public class InlineSpan { }
	public class FocusScopeNode : FocusNode { }
	public class ColorFilter { }
	public delegate BoxConstraints BoxConstraintsTransform(BoxConstraints constraints);
	public class AssetBundle { }

}

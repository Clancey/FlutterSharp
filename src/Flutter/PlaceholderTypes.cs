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
	public class BuildContext { }
	public class FocusNode { }
	public class ScrollController { }
	public class ScrollPhysics { }
	public class ScrollBehavior { }
	public class RouteSettings { }
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
	public class TransformationController { }
	public class AlignmentGeometry { }
	public class Color { }
	public class EdgeInsetsGeometry { }
	public class TextStyle { }
	public class Decoration { }
	public class Curve { }
	public class BorderRadiusGeometry { }
	public class SliverChildDelegate { }
	public class Matrix4 { }
	public class BoxConstraints { }
	public class TwoDimensionalChildDelegate { }
	public class ScrollableDetails { }
	public class ListWheelChildDelegate { }
	public class BoxShape { }
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

	// Generic base widget types
	public class Draggable<T> : StatefulWidget { }
	public class ParentDataWidget<T> : ProxyWidget { }
	public class InheritedModel<T> : InheritedWidget { }
	public class InheritedNotifier<T> : InheritedWidget { }
	public class AbstractLayoutBuilder<T> : RenderObjectWidget { }
	public class ConstrainedLayoutBuilder<T> : RenderObjectWidget { }
	public class StreamBuilderBase<T, S> : StatefulWidget { }
	public class SlottedMultiChildRenderObjectWidget<TSlot, TChild> : RenderObjectWidget { }
	public class _DarwinPlatformView<TController, TRender> : StatefulWidget { }

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

	// Non-Flutter widget base classes needed for compilation
	public class StatefulWidget : Widget
	{
		protected override FlutterObjectStruct CreateBackingStruct() => throw new NotImplementedException("This is a placeholder type for FFI interop");
	}
	public class ProxyWidget : Widget
	{
		protected override FlutterObjectStruct CreateBackingStruct() => throw new NotImplementedException("This is a placeholder type for FFI interop");
	}
	public class InheritedWidget : ProxyWidget
	{
		protected override FlutterObjectStruct CreateBackingStruct() => throw new NotImplementedException("This is a placeholder type for FFI interop");
	}
	public class RenderObjectWidget : Widget
	{
		protected override FlutterObjectStruct CreateBackingStruct() => throw new NotImplementedException("This is a placeholder type for FFI interop");
	}

	// System types that might be missing
	public class StackTrace { }
}

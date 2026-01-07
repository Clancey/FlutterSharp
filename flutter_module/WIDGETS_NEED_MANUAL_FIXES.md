# Widgets Requiring Manual Implementation

**Generated:** 2025-11-26
**Total widgets needing fixes:** 75 out of 325 (23%)
**Successfully auto-generated:** 250 parsers (77%)

## Summary by Category

### 1. Missing Required Arguments (63 widgets)
These widgets have required parameters that the Dart analyzer couldn't extract type information for, causing them to fall back to `Pointer<Void>`.

#### Animated Widgets (17)
- `AnimatedAlign` - missing: duration
- `AnimatedContainer` - missing: duration
- `AnimatedDefaultTextStyle` - missing: duration
- `AnimatedFractionalSizedBox` - missing: duration
- `AnimatedGrid` - missing: itemBuilder
- `AnimatedList` - missing: itemBuilder
- `AnimatedModalBarrier` - wrong type: color (expected Animation<Color?>, got Color)
- `AnimatedOpacity` - missing: duration
- `AnimatedPadding` - missing: duration
- `AnimatedPhysicalModel` - missing: duration
- `AnimatedPositioned` - missing: duration
- `AnimatedPositionedDirectional` - missing: duration
- `AnimatedRotation` - missing: duration
- `AnimatedScale` - missing: duration
- `AnimatedSlide` - missing: duration
- `SliverAnimatedGrid` - missing: itemBuilder
- `SliverAnimatedList` - missing: itemBuilder
- `SliverAnimatedOpacity` - missing: duration
- `TweenAnimationBuilder` - missing: duration

#### Container/Wrapper Widgets (20)
- `BackdropGroup` - missing: child
- `DefaultAssetBundle` - missing: child
- `DefaultSelectionStyle` - missing: child
- `DefaultTextHeightBehavior` - missing: child
- `DefaultTextStyle` - missing: child
- `Directionality` - missing: child
- `DragBoundary` - missing: child
- `Expanded` - missing: child
- `Flexible` - missing: child
- `FocusScope` - missing: child
- `IconTheme` - missing: child
- `KeepAlive` - missing: child
- `LayoutId` - missing: child
- `LookupBoundary` - missing: child
- `NotificationListener` - missing: child
- `Positioned` - missing: child
- `ReorderableDelayedDragStartListener` - missing: child, index
- `SelectionRegistrarScope` - missing: child
- `TapRegion` - missing: child
- `TapRegionSurface` - missing: child
- `TextFieldTapRegion` - missing: child
- `UnmanagedRestorationScope` - missing: child

#### Builder/Delegate Widgets (12)
- `AnimatedBuilder` - missing: builder (also has undefined param: listenable)
- `LayoutBuilder` - missing: builder
- `ListWheelScrollView` - missing: children (also has undefined param: childDelegate)
- `PageView` - has undefined param: childrenDelegate
- `PlatformViewLink` - missing: onCreatePlatformView, surfaceFactory
- `SliverCrossAxisGroup` - missing: slivers
- `SliverEnsureSemantics` - missing: sliver
- `SliverFixedExtentList` - missing: delegate
- `SliverGrid` - missing: delegate
- `SliverLayoutBuilder` - missing: builder
- `SliverList` - missing: delegate
- `SliverMainAxisGroup` - missing: slivers
- `SliverPrototypeExtentList` - missing: delegate
- `SliverVariedExtentList` - missing: delegate

#### State Management Widgets (4)
- `PrimaryScrollController` - missing: child, controller
- `RestorationScope` - missing: restorationId
- `RootRestorationScope` - missing: restorationId
- `SelectionContainer` - missing: delegate

#### Platform View Widgets (3)
- `AppKitView` - missing: viewType
- `UiKitView` - missing: viewType
- `ImgElementPlatformView` - undefined method (web-specific)
- `RawWebImage` - undefined method (web-specific)

#### Other (7)
- `AnnotatedRegion` - type inference failure, missing: child
- `SliverCrossAxisExpanded` - missing: sliver
- `SystemContextMenu` - no unnamed constructor

### 2. Undefined Named Parameters (13 widgets)
These widgets have parameter names that don't match the current Flutter API.

- `AnimatedBuilder` - undefined: listenable (should be: animation)
- `EditableText` - undefined: selectionEnabled
- `Flexible` - undefined: debugTypicalAncestorWidgetClass
- `KeepAlive` - undefined: debugTypicalAncestorWidgetClass, debugTypicalAncestorWidgetDescription
- `LayoutId` - undefined: debugTypicalAncestorWidgetClass
- `ListWheelScrollView` - undefined: childDelegate
- `PageView` - undefined: childrenDelegate (should be: children or controller)
- `Positioned` - undefined: debugTypicalAncestorWidgetClass
- `Semantics` - undefined: properties
- `SliverCrossAxisExpanded` - undefined: debugTypicalAncestorWidgetClass
- `ImageIcon` - undefined: image

### 3. Positional Arguments Required (3 widgets)
These widgets require positional arguments that we're not providing.

- `Icon` - needs 1 positional argument (IconData)
- `ImageIcon` - needs 1 positional argument (ImageProvider)
- `Text` - needs 1 positional argument (String) or use Text.rich constructor

### 4. Type Mismatches (4 widgets)
- `ActionListener` - listener expects Action<Intent>, got GestureTapCallback?
- `AnimatedModalBarrier` - color expects Animation<Color?>, got Color
- `Table` - children expects List<TableRow>, got List<Widget>
- `ActionListener` - also has undefined getter: Pointer<Void>.ref

### 5. Import Conflicts (2 widgets)
- `CustomPaint` - Size ambiguity between dart:ffi and dart:ui
- `RawMenuAnchor` - Size ambiguity between dart:ffi and dart:ui

### 6. Platform-Specific (2 widgets)
- `ImgElementPlatformView` - Web-only, undefined method
- `RawWebImage` - Web-only, undefined method

## Recommended Actions

### Immediate: Exclude from Compilation
Create a `.dartignore` or exclude these files from `generated_parsers.dart` to allow clean compilation.

### Short Term: Quick Fixes
1. **Size ambiguity** - Add import prefix: `import 'dart:ui' as ui;`
2. **Positional arguments** - Add default values or make nullable
3. **Parameter renames** - Update parameter names to match current Flutter API

### Medium Term: Improve Code Generation
1. **Duration parameters** - Add special handling for required Duration params
2. **Builder callbacks** - Improve callback type detection
3. **Platform detection** - Skip web-only widgets on non-web platforms

### Long Term: Manual Wrappers
For complex types like `Animation<T>`, `Action<Intent>`, and `List<TableRow>`, create manual wrapper classes that properly handle the FFI boundary.

## Success Metrics
- ✅ 250 widgets (77%) auto-generate successfully
- ✅ 70% error reduction achieved (306 → 91 errors)
- ✅ All callback-based widgets now have action string support
- ✅ Clear TODO markers for widgets needing manual implementation

## Next Steps
1. Exclude these 75 parsers from compilation
2. Verify clean build with remaining 250 parsers
3. Create manual wrappers for high-priority widgets (Text, Icon, Container, etc.)

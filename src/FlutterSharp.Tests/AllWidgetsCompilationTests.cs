using NUnit.Framework;
using System;

namespace FlutterSharp.Tests
{
    /// <summary>
    /// Tests that verify all generated widgets compile correctly.
    /// This test references every widget type to ensure the code generator
    /// produces valid C# code that compiles without errors.
    /// </summary>
    [TestFixture]
    public class AllWidgetsCompilationTests
    {
        /// <summary>
        /// Verifies that all non-generic widget types exist and are accessible.
        /// If any type fails to compile, this test will fail to build.
        /// </summary>
        [Test]
        public void AllNonGenericWidgetTypes_ShouldCompile()
        {
            // This test verifies compilation by referencing all non-generic widget types.
            // Generic types are tested separately with proper type arguments.
            // If any widget has a compilation error, this test file won't build.

            var widgetTypes = new Type[]
            {
                // A
                typeof(Flutter.Widgets.AbsorbPointer),
                typeof(Flutter.Widgets.ActionListener),
                typeof(Flutter.Widgets.Actions),
                typeof(Flutter.Material.AlertDialog),
                typeof(Flutter.Widgets.Align),
                typeof(Flutter.Widgets.AlignTransition),
                typeof(Flutter.Widgets.AndroidView),
                typeof(Flutter.Widgets.AndroidViewSurface),
                typeof(Flutter.Widgets.AnimatedAlign),
                typeof(Flutter.Widgets.AnimatedBuilder),
                typeof(Flutter.Widgets.AnimatedContainer),
                typeof(Flutter.Widgets.AnimatedCrossFade),
                typeof(Flutter.Widgets.AnimatedDefaultTextStyle),
                typeof(Flutter.Widgets.AnimatedFractionallySizedBox),
                typeof(Flutter.Widgets.AnimatedGrid),
                typeof(Flutter.Widgets.AnimatedList),
                typeof(Flutter.Widgets.AnimatedModalBarrier),
                typeof(Flutter.Widgets.AnimatedOpacity),
                typeof(Flutter.Widgets.AnimatedPadding),
                typeof(Flutter.Widgets.AnimatedPhysicalModel),
                typeof(Flutter.Widgets.AnimatedPositioned),
                typeof(Flutter.Widgets.AnimatedPositionedDirectional),
                typeof(Flutter.Widgets.AnimatedRotation),
                typeof(Flutter.Widgets.AnimatedScale),
                typeof(Flutter.Widgets.AnimatedSize),
                typeof(Flutter.Widgets.AnimatedSlide),
                typeof(Flutter.Widgets.AnimatedSwitcher),
                typeof(Flutter.Material.AppBar),
                typeof(Flutter.Widgets.AspectRatio),
                typeof(Flutter.Widgets.AutocompleteHighlightedOption),
                typeof(Flutter.Widgets.AutofillGroup),
                typeof(Flutter.Widgets.AutomaticKeepAlive),

                // B
                typeof(Flutter.Widgets.BackButtonListener),
                typeof(Flutter.Widgets.BackdropFilter),
                typeof(Flutter.Widgets.BackdropGroup),
                typeof(Flutter.Widgets.Banner),
                typeof(Flutter.Widgets.Baseline),
                typeof(Flutter.Widgets.BlockSemantics),
                typeof(Flutter.Material.BottomNavigationBar),
                typeof(Flutter.Material.BottomNavigationBarItem),
                typeof(Flutter.Widgets.Builder),

                // C
                typeof(Flutter.Widgets.CallbackShortcuts),
                typeof(Flutter.Material.Card),
                typeof(Flutter.Widgets.Center),
                typeof(Flutter.Widgets.Checkbox),
                typeof(Flutter.Widgets.CheckedModeBanner),
                typeof(Flutter.Widgets.ClipOval),
                typeof(Flutter.Widgets.ClipPath),
                typeof(Flutter.Widgets.ClipRect),
                typeof(Flutter.Widgets.ClipRRect),
                typeof(Flutter.Widgets.ClipRSuperellipse),
                typeof(Flutter.Widgets.ColoredBox),
                typeof(Flutter.Widgets.ColorFiltered),
                typeof(Flutter.Widgets.Column),
                typeof(Flutter.Widgets.CompositedTransformFollower),
                typeof(Flutter.Widgets.CompositedTransformTarget),
                typeof(Flutter.Widgets.ConstrainedBox),
                typeof(Flutter.Widgets.ConstraintsTransformBox),
                typeof(Flutter.Widgets.Container),
                typeof(Flutter.Widgets.CupertinoButton),
                typeof(Flutter.Widgets.CupertinoNavigationBar),
                typeof(Flutter.Cupertino.CupertinoSwitch),
                typeof(Flutter.Cupertino.CupertinoTabBar),
                typeof(Flutter.Widgets.CupertinoTextField),
                typeof(Flutter.Widgets.CustomMultiChildLayout),
                typeof(Flutter.Widgets.CustomPaint),
                typeof(Flutter.Widgets.CustomScrollView),
                typeof(Flutter.Widgets.CustomSingleChildLayout),

                // D
                typeof(Flutter.Widgets.DecoratedBox),
                typeof(Flutter.Widgets.DecoratedBoxTransition),
                typeof(Flutter.Widgets.DecoratedSliver),
                typeof(Flutter.Widgets.DefaultAssetBundle),
                typeof(Flutter.Widgets.DefaultSelectionStyle),
                typeof(Flutter.Widgets.DefaultTextEditingShortcuts),
                typeof(Flutter.Widgets.DefaultTextHeightBehavior),
                typeof(Flutter.Widgets.DefaultTextStyle),
                typeof(Flutter.Widgets.DefaultTextStyleTransition),
                typeof(Flutter.Widgets.Directionality),
                typeof(Flutter.Widgets.DisplayFeatureSubScreen),
                typeof(Flutter.Widgets.DragBoundary),
                typeof(Flutter.Widgets.DraggableScrollableActuator),
                typeof(Flutter.Widgets.DraggableScrollableSheet),
                typeof(Flutter.Material.Drawer),
                typeof(Flutter.Widgets.DualTransitionBuilder),

                // E
                typeof(Flutter.Widgets.EditableText),
                typeof(Flutter.Material.ElevatedButton),
                typeof(Flutter.Widgets.ErrorBoundary),
                typeof(Flutter.Widgets.ErrorWidget),
                typeof(Flutter.Widgets.ExcludeFocus),
                typeof(Flutter.Widgets.ExcludeFocusTraversal),
                typeof(Flutter.Widgets.ExcludeSemantics),
                typeof(Flutter.Widgets.Expanded),
                typeof(Flutter.Widgets.Expansible),

                // F
                typeof(Flutter.Widgets.FadeInImage),
                typeof(Flutter.Widgets.FadeTransition),
                typeof(Flutter.Widgets.FittedBox),
                typeof(Flutter.Widgets.Flex),
                typeof(Flutter.Widgets.Flexible),
                typeof(Flutter.Material.FloatingActionButton),
                typeof(Flutter.Widgets.Flow),
                typeof(Flutter.Widgets.FlutterLogo),
                typeof(Flutter.Widgets.Focus),
                typeof(Flutter.Widgets.FocusableActionDetector),
                typeof(Flutter.Widgets.FocusScope),
                typeof(Flutter.Widgets.FocusTraversalGroup),
                typeof(Flutter.Widgets.FocusTraversalOrder),
                typeof(Flutter.Widgets.Form),
                typeof(Flutter.Widgets.FractionallySizedBox),
                typeof(Flutter.Widgets.FractionalTranslation),

                // G
                typeof(Flutter.Widgets.GestureDetector),
                typeof(Flutter.Widgets.GlowingOverscrollIndicator),
                typeof(Flutter.Widgets.GridPaper),
                typeof(Flutter.Widgets.GridView),
                typeof(Flutter.Widgets.GridViewBuilder),

                // H
                typeof(Flutter.Widgets.Hero),
                typeof(Flutter.Widgets.HeroControllerScope),
                typeof(Flutter.Widgets.HeroMode),
                typeof(Flutter.Widgets.HtmlElementView),

                // I
                typeof(Flutter.Widgets.Icon),
                typeof(Flutter.Widgets.IconButton),
                typeof(Flutter.Widgets.IconTheme),
                typeof(Flutter.Widgets.IgnoreBaseline),
                typeof(Flutter.Widgets.IgnorePointer),
                typeof(Flutter.Widgets.Image),
                typeof(Flutter.Widgets.ImageFiltered),
                typeof(Flutter.Widgets.ImageIcon),
                typeof(Flutter.Widgets.ImgElementPlatformView),
                typeof(Flutter.Widgets.IndexedSemantics),
                typeof(Flutter.Widgets.IndexedStack),
                typeof(Flutter.Widgets.InfiniteGridView),
                typeof(Flutter.Widgets.InfiniteListView),
                typeof(Flutter.Widgets.InteractiveViewer),
                typeof(Flutter.Widgets.IntrinsicHeight),
                typeof(Flutter.Widgets.IntrinsicWidth),

                // K
                typeof(Flutter.Widgets.KeepAlive),
                typeof(Flutter.Widgets.KeyboardListener),
                typeof(Flutter.Widgets.KeyedSubtree),

                // L
                typeof(Flutter.Widgets.LayoutBuilder),
                typeof(Flutter.Widgets.LayoutId),
                typeof(Flutter.Widgets.LimitedBox),
                typeof(Flutter.Widgets.ListBody),
                typeof(Flutter.Widgets.ListenableBuilder),
                typeof(Flutter.Widgets.Listener),
                typeof(Flutter.Widgets.ListTile),
                typeof(Flutter.Widgets.ListView),
                typeof(Flutter.Widgets.ListViewBuilder),
                typeof(Flutter.Widgets.ListWheelScrollView),
                typeof(Flutter.Widgets.ListWheelViewport),
                typeof(Flutter.Widgets.Localizations),
                typeof(Flutter.Widgets.LookupBoundary),

                // M
                typeof(Flutter.Widgets.MatrixTransition),
                typeof(Flutter.Widgets.MediaQuery),
                typeof(Flutter.Widgets.MergeSemantics),
                typeof(Flutter.Widgets.MetaData),
                typeof(Flutter.Widgets.ModalBarrier),
                typeof(Flutter.Widgets.MouseRegion),

                // N
                typeof(Flutter.Widgets.NavigationToolbar),
                typeof(Flutter.Widgets.Navigator),
                typeof(Flutter.Widgets.NestedScrollView),

                // O
                typeof(Flutter.Widgets.Offstage),
                typeof(Flutter.Widgets.Opacity),
                typeof(Flutter.Widgets.OrientationBuilder),
                typeof(Flutter.Material.OutlinedButton),
                typeof(Flutter.Widgets.OverflowBar),
                typeof(Flutter.Widgets.OverflowBox),
                typeof(Flutter.Widgets.Overlay),
                typeof(Flutter.Widgets.OverlayPortal),

                // P
                typeof(Flutter.Widgets.Padding),
                typeof(Flutter.Widgets.PageStorage),
                typeof(Flutter.Widgets.PageView),
                typeof(Flutter.Widgets.PerformanceOverlay),
                typeof(Flutter.Widgets.PhysicalModel),
                typeof(Flutter.Widgets.PhysicalShape),
                typeof(Flutter.Widgets.PinnedHeaderSliver),
                typeof(Flutter.Widgets.Placeholder),
                typeof(Flutter.Widgets.PlatformMenuBar),
                typeof(Flutter.Widgets.PlatformSelectableRegionContextMenu),
                typeof(Flutter.Widgets.PlatformViewLink),
                typeof(Flutter.Widgets.PlatformViewSurface),
                typeof(Flutter.Widgets.Positioned),
                typeof(Flutter.Widgets.PositionedDirectional),
                typeof(Flutter.Widgets.PositionedTransition),
                typeof(Flutter.Widgets.PreferredSize),
                typeof(Flutter.Widgets.PrimaryScrollController),

                // R
                typeof(Flutter.Widgets.Radio),
                typeof(Flutter.Widgets.RawGestureDetector),
                typeof(Flutter.Widgets.RawImage),
                typeof(Flutter.Widgets.RawMagnifier),
                typeof(Flutter.Widgets.RawMenuAnchor),
                typeof(Flutter.Widgets.RawMenuAnchorGroup),
                typeof(Flutter.Widgets.RawScrollbar),
                typeof(Flutter.Widgets.RawView),
                typeof(Flutter.Widgets.RawWebImage),
                typeof(Flutter.Widgets.RefreshIndicator),
                typeof(Flutter.Widgets.RelativePositionedTransition),
                typeof(Flutter.Widgets.ReorderableDelayedDragStartListener),
                typeof(Flutter.Widgets.ReorderableDragStartListener),
                typeof(Flutter.Widgets.ReorderableList),
                typeof(Flutter.Widgets.RepaintBoundary),
                typeof(Flutter.Widgets.RestorationScope),
                typeof(Flutter.Widgets.RichText),
                typeof(Flutter.Widgets.RootRestorationScope),
                typeof(Flutter.Widgets.RootWidget),
                typeof(Flutter.Widgets.RotatedBox),
                typeof(Flutter.Widgets.RotationTransition),
                typeof(Flutter.Widgets.Row),

                // S
                typeof(Flutter.Widgets.SafeArea),
                typeof(Flutter.Material.Scaffold),
                typeof(Flutter.Widgets.ScaleTransition),
                typeof(Flutter.Widgets.Scrollable),
                typeof(Flutter.Widgets.ScrollConfiguration),
                typeof(Flutter.Widgets.ScrollController),
                typeof(Flutter.Widgets.ScrollNotificationObserver),
                typeof(Flutter.Widgets.SelectableRegion),
                typeof(Flutter.Widgets.SelectableRegionSelectionStatusScope),
                typeof(Flutter.Widgets.SelectionContainer),
                typeof(Flutter.Widgets.SelectionListener),
                typeof(Flutter.Widgets.SelectionRegistrarScope),
                typeof(Flutter.Widgets.Semantics),
                typeof(Flutter.Widgets.SemanticsDebugger),
                typeof(Flutter.Widgets.ShaderMask),
                typeof(Flutter.Widgets.SharedAppData),
                typeof(Flutter.Widgets.ShortcutRegistrar),
                typeof(Flutter.Widgets.Shortcuts),
                typeof(Flutter.Widgets.ShrinkWrappingViewport),
                typeof(Flutter.Widgets.SingleChildScrollView),
                typeof(Flutter.Widgets.SizeChangedLayoutNotifier),
                typeof(Flutter.Widgets.SizedBox),
                typeof(Flutter.Widgets.SizedOverflowBox),
                typeof(Flutter.Widgets.SizeTransition),
                typeof(Flutter.Widgets.Slider),
                typeof(Flutter.Widgets.SlideTransition),
                typeof(Flutter.Widgets.SliverAnimatedGrid),
                typeof(Flutter.Widgets.SliverAnimatedList),
                typeof(Flutter.Widgets.SliverAnimatedOpacity),
                typeof(Flutter.Widgets.SliverConstrainedCrossAxis),
                typeof(Flutter.Widgets.SliverCrossAxisGroup),
                typeof(Flutter.Widgets.SliverEnsureSemantics),
                typeof(Flutter.Widgets.SliverFadeTransition),
                typeof(Flutter.Widgets.SliverFillRemaining),
                typeof(Flutter.Widgets.SliverFillViewport),
                typeof(Flutter.Widgets.SliverFixedExtentList),
                typeof(Flutter.Widgets.SliverFloatingHeader),
                typeof(Flutter.Widgets.SliverGrid),
                typeof(Flutter.Widgets.SliverIgnorePointer),
                typeof(Flutter.Widgets.SliverLayoutBuilder),
                typeof(Flutter.Widgets.SliverList),
                typeof(Flutter.Widgets.SliverMainAxisGroup),
                typeof(Flutter.Widgets.SliverOffstage),
                typeof(Flutter.Widgets.SliverOpacity),
                typeof(Flutter.Widgets.SliverOverlapAbsorber),
                typeof(Flutter.Widgets.SliverOverlapInjector),
                typeof(Flutter.Widgets.SliverPadding),
                typeof(Flutter.Widgets.SliverPersistentHeader),
                typeof(Flutter.Widgets.SliverPrototypeExtentList),
                typeof(Flutter.Widgets.SliverReorderableList),
                typeof(Flutter.Widgets.SliverResizingHeader),
                typeof(Flutter.Widgets.SliverSafeArea),
                typeof(Flutter.Widgets.SliverToBoxAdapter),
                typeof(Flutter.Widgets.SliverVariedExtentList),
                typeof(Flutter.Widgets.SliverVisibility),
                typeof(Flutter.Widgets.SnapshotWidget),
                typeof(Flutter.Widgets.Spacer),
                typeof(Flutter.Widgets.Stack),
                typeof(Flutter.Widgets.StatefulBuilder),
                typeof(Flutter.Widgets.StatusTransitionWidget),
                typeof(Flutter.Widgets.StretchingOverscrollIndicator),
                typeof(Flutter.Widgets.Switch),
                typeof(Flutter.Widgets.SystemContextMenu),

                // T
                typeof(Flutter.Widgets.Table),
                typeof(Flutter.Widgets.TableCell),
                typeof(Flutter.Widgets.TapRegion),
                typeof(Flutter.Widgets.TapRegionSurface),
                typeof(Flutter.Widgets.Text),
                typeof(Flutter.Material.TextButton),
                typeof(Flutter.Material.TextField),
                typeof(Flutter.Widgets.TextFieldTapRegion),
                typeof(Flutter.Widgets.TextSelectionGestureDetector),
                typeof(Flutter.Widgets.Texture),
                typeof(Flutter.Widgets.TickerMode),
                typeof(Flutter.Widgets.Title),
                typeof(Flutter.Widgets.Transform),
                typeof(Flutter.Widgets.TwoDimensionalScrollable),
                typeof(Flutter.Widgets.TwoDimensionalScrollView),
                typeof(Flutter.Widgets.TwoDimensionalViewport),

                // U
                typeof(Flutter.Widgets.UnconstrainedBox),
                typeof(Flutter.Widgets.UnmanagedRestorationScope),

                // V
                typeof(Flutter.Widgets.View),
                typeof(Flutter.Widgets.ViewAnchor),
                typeof(Flutter.Widgets.ViewCollection),
                typeof(Flutter.Widgets.Viewport),
                typeof(Flutter.Widgets.Visibility),

                // W
                typeof(Flutter.Widgets.WidgetInspector),
                typeof(Flutter.Widgets.WidgetToRenderBoxAdapter),
                typeof(Flutter.Widgets.Wrap),
            };

            // Verify all types loaded successfully
            Assert.That(widgetTypes.Length, Is.GreaterThan(250), "Expected at least 250 non-generic widget types");

            foreach (var type in widgetTypes)
            {
                Assert.That(type, Is.Not.Null, $"Widget type should not be null");
                Assert.That(type.Namespace, Does.StartWith("Flutter"), $"Widget {type.Name} should be in Flutter namespace");
            }

            Assert.Pass($"All {widgetTypes.Length} non-generic widget types compiled and loaded successfully");
        }

        /// <summary>
        /// Verifies that generic widget types exist.
        /// </summary>
        [Test]
        public void GenericWidgetTypes_ShouldCompile()
        {
            // These are generic widget types that require type parameters
            var genericTypes = new Type[]
            {
                typeof(Flutter.Widgets.AnnotatedRegion<>),
                typeof(Flutter.Widgets.DragTarget<>),
                typeof(Flutter.Material.DropdownButton<>),
                typeof(Flutter.Material.DropdownMenuItem<>),
                typeof(Flutter.Widgets.LongPressDraggable<>),
                typeof(Flutter.Widgets.NavigatorPopHandler<>),
                typeof(Flutter.Widgets.NotificationListener<>),
                typeof(Flutter.Widgets.PopScope<>),
                typeof(Flutter.Widgets.RawAutocomplete<>),
                typeof(Flutter.Widgets.RenderObjectToWidgetAdapter<>),
                typeof(Flutter.Widgets.Router<>),
                typeof(Flutter.Widgets.StreamBuilderBase<,>),
                typeof(Flutter.Widgets.TreeSliver<>),
                typeof(Flutter.Widgets.TweenAnimationBuilder<>),
                typeof(Flutter.Widgets.UndoHistory<>),
                typeof(Flutter.Widgets.UniqueWidget<>),
                typeof(Flutter.Widgets.ValueListenableBuilder<>),
            };

            foreach (var type in genericTypes)
            {
                Assert.That(type, Is.Not.Null, $"Generic type {type.Name} should exist");
                Assert.That(type.IsGenericTypeDefinition, Is.True, $"{type.Name} should be a generic type definition");
            }

            Assert.Pass($"All {genericTypes.Length} generic widget types compiled successfully");
        }

        /// <summary>
        /// Verifies that abstract base widget types exist.
        /// </summary>
        [Test]
        public void AbstractWidgetTypes_ShouldCompile()
        {
            var abstractTypes = new Type[]
            {
                typeof(Flutter.Widgets.AbstractLayoutBuilder<>),
                typeof(Flutter.Widgets.AnimatedWidget),
                typeof(Flutter.Widgets.BoxScrollView),
                typeof(Flutter.Widgets.ConstrainedLayoutBuilder<>),
                typeof(Flutter.Widgets.ImplicitlyAnimatedWidget),
                typeof(Flutter.Widgets.InheritedModel<>),
                typeof(Flutter.Widgets.InheritedNotifier<>),
                typeof(Flutter.Widgets.InheritedTheme),
                typeof(Flutter.Widgets.InheritedWidget),
                typeof(Flutter.Widgets.InspectorButton),
                typeof(Flutter.Widgets.LeafRenderObjectWidget),
                typeof(Flutter.Widgets.MultiChildRenderObjectWidget),
                typeof(Flutter.Widgets.ParentDataWidget<>),
                typeof(Flutter.Widgets.ProxyWidget),
                typeof(Flutter.Widgets.RenderObjectWidget),
                typeof(Flutter.Widgets.ScrollView),
                typeof(Flutter.Widgets.SingleChildRenderObjectWidget),
                typeof(Flutter.Widgets.SliverMultiBoxAdaptorWidget),
                typeof(Flutter.Widgets.SliverWithKeepAliveWidget),
                typeof(Flutter.Widgets.SlottedMultiChildRenderObjectWidget<,>),
                typeof(Flutter.Widgets.StatefulWidget),
                typeof(Flutter.Widgets.StatelessWidget),
            };

            foreach (var type in abstractTypes)
            {
                Assert.That(type, Is.Not.Null, $"Abstract type {type.Name} should exist");
                Assert.That(type.IsAbstract || type.IsGenericTypeDefinition, Is.True, $"{type.Name} should be abstract or generic");
            }

            Assert.Pass($"All {abstractTypes.Length} abstract widget types compiled successfully");
        }

        /// <summary>
        /// Verifies that all enum types exist and are accessible.
        /// Enums are in the Flutter.UI namespace.
        /// </summary>
        [Test]
        public void AllEnumTypes_ShouldCompile()
        {
            var enumTypes = new Type[]
            {
                typeof(Flutter.Clip),
                typeof(Flutter.UI.MainAxisAlignment),
                typeof(Flutter.UI.CrossAxisAlignment),
                typeof(Flutter.UI.MainAxisSize),
                typeof(Flutter.UI.VerticalDirection),
                typeof(Flutter.TextDirection),
                typeof(Flutter.Enums.TextAlign),
                typeof(Flutter.TextOverflow),
                typeof(Flutter.UI.BoxFit),
                typeof(Flutter.UI.Axis),
            };

            foreach (var type in enumTypes)
            {
                Assert.That(type, Is.Not.Null, $"Enum type {type.Name} should exist");
                Assert.That(type.IsEnum, Is.True, $"{type.Name} should be an enum");
            }

            Assert.Pass($"All {enumTypes.Length} enum types compiled successfully");
        }

        /// <summary>
        /// Verifies core infrastructure types exist.
        /// </summary>
        [Test]
        public void CoreInfrastructure_ShouldCompile()
        {
            var coreTypes = new Type[]
            {
                typeof(Flutter.Widget),
                typeof(Flutter.Internal.FlutterManager),
                typeof(Flutter.CallbackRegistry),
                typeof(Flutter.Internal.Communicator),
            };

            foreach (var type in coreTypes)
            {
                Assert.That(type, Is.Not.Null, $"Core type {type.Name} should exist");
            }

            Assert.Pass($"All {coreTypes.Length} core infrastructure types compiled successfully");
        }
    }
}

// MaterialPageRoute for FlutterSharp navigation
// Part of FlutterSharp Phase 5 - Navigation

using System;
using Flutter.Widgets;

namespace Flutter.Navigation
{
    /// <summary>
    /// A modal route that replaces the entire screen with a platform-adaptive transition.
    ///
    /// On Android, the entrance transition slides the page upwards and fades it in.
    /// The exit transition is the same, but in reverse.
    ///
    /// On iOS, the page slides in from the right and exits in reverse.
    /// The page also shifts to the left in parallax when another page enters to cover it.
    /// </summary>
    /// <example>
    /// <code>
    /// navigator.Push(new MaterialPageRoute(
    ///     builder: () => new DetailsPage(),
    ///     settings: new RouteSettings(name: "/details")
    /// ));
    /// </code>
    /// </example>
    public class MaterialPageRoute : Route
    {
        private readonly Func<Widget> _builder;

        /// <summary>
        /// Creates a page route for use in a material app.
        /// </summary>
        /// <param name="builder">A function that creates the widget content of the route.</param>
        /// <param name="settings">The route settings.</param>
        /// <param name="maintainState">Whether to maintain route state when not visible.</param>
        /// <param name="fullscreenDialog">Whether this route is a full-screen dialog.</param>
        public MaterialPageRoute(
            Func<Widget> builder,
            RouteSettings? settings = null,
            bool maintainState = true,
            bool fullscreenDialog = false)
            : base(settings)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            MaintainState = maintainState;
            FullscreenDialog = fullscreenDialog;
        }

        /// <summary>
        /// Whether this route should maintain state when not visible.
        /// </summary>
        public override bool MaintainState { get; }

        /// <summary>
        /// Whether this route is a full-screen dialog.
        /// </summary>
        public override bool FullscreenDialog { get; }

        /// <summary>
        /// The transition type for this route.
        /// Material pages use the Material transition (slide up + fade on Android,
        /// or Cupertino transition on iOS).
        /// </summary>
        public override RouteTransitionType TransitionType => RouteTransitionType.Material;

        /// <summary>
        /// The duration of the push transition.
        /// </summary>
        public override int TransitionDurationMs => 300;

        /// <summary>
        /// The duration of the pop transition.
        /// </summary>
        public override int ReverseTransitionDurationMs => 300;

        /// <summary>
        /// Builds the page widget.
        /// </summary>
        public override Widget BuildPage(object? context)
        {
            return _builder();
        }

        /// <summary>
        /// Creates a MaterialPageRoute with a simple builder and named route.
        /// </summary>
        /// <param name="name">The route name (e.g., "/details").</param>
        /// <param name="builder">The widget builder.</param>
        /// <returns>A new MaterialPageRoute.</returns>
        public static MaterialPageRoute Of(string name, Func<Widget> builder)
        {
            return new MaterialPageRoute(
                builder: builder,
                settings: new RouteSettings(name: name)
            );
        }
    }

    /// <summary>
    /// A page route that uses an iOS-style transition.
    /// The page slides in from the right and out to the right.
    /// </summary>
    public class CupertinoPageRoute : Route
    {
        private readonly Func<Widget> _builder;

        /// <summary>
        /// Creates a page route for use in a Cupertino/iOS-style app.
        /// </summary>
        /// <param name="builder">A function that creates the widget content of the route.</param>
        /// <param name="settings">The route settings.</param>
        /// <param name="maintainState">Whether to maintain route state when not visible.</param>
        /// <param name="fullscreenDialog">Whether this route is a full-screen dialog.</param>
        public CupertinoPageRoute(
            Func<Widget> builder,
            RouteSettings? settings = null,
            bool maintainState = true,
            bool fullscreenDialog = false)
            : base(settings)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            MaintainState = maintainState;
            FullscreenDialog = fullscreenDialog;
        }

        /// <inheritdoc />
        public override bool MaintainState { get; }

        /// <inheritdoc />
        public override bool FullscreenDialog { get; }

        /// <summary>
        /// The transition type for this route.
        /// Cupertino routes use a slide from right transition.
        /// </summary>
        public override RouteTransitionType TransitionType => RouteTransitionType.Cupertino;

        /// <summary>
        /// The duration of the push transition (iOS standard is 400ms).
        /// </summary>
        public override int TransitionDurationMs => 400;

        /// <summary>
        /// The duration of the pop transition.
        /// </summary>
        public override int ReverseTransitionDurationMs => 400;

        /// <inheritdoc />
        public override Widget BuildPage(object? context)
        {
            return _builder();
        }
    }

    /// <summary>
    /// A page route with a fade transition.
    /// </summary>
    public class FadePageRoute : Route
    {
        private readonly Func<Widget> _builder;
        private readonly int _durationMs;

        /// <summary>
        /// Creates a page route with a fade transition.
        /// </summary>
        /// <param name="builder">A function that creates the widget content of the route.</param>
        /// <param name="settings">The route settings.</param>
        /// <param name="durationMs">The transition duration in milliseconds.</param>
        public FadePageRoute(
            Func<Widget> builder,
            RouteSettings? settings = null,
            int durationMs = 300)
            : base(settings)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _durationMs = durationMs;
        }

        /// <inheritdoc />
        public override RouteTransitionType TransitionType => RouteTransitionType.Fade;

        /// <inheritdoc />
        public override int TransitionDurationMs => _durationMs;

        /// <inheritdoc />
        public override int ReverseTransitionDurationMs => _durationMs;

        /// <inheritdoc />
        public override Widget BuildPage(object? context)
        {
            return _builder();
        }
    }

    /// <summary>
    /// A page route with no transition animation.
    /// </summary>
    public class NoTransitionPageRoute : Route
    {
        private readonly Func<Widget> _builder;

        /// <summary>
        /// Creates a page route with no transition.
        /// </summary>
        /// <param name="builder">A function that creates the widget content of the route.</param>
        /// <param name="settings">The route settings.</param>
        public NoTransitionPageRoute(
            Func<Widget> builder,
            RouteSettings? settings = null)
            : base(settings)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <inheritdoc />
        public override RouteTransitionType TransitionType => RouteTransitionType.None;

        /// <inheritdoc />
        public override int TransitionDurationMs => 0;

        /// <inheritdoc />
        public override int ReverseTransitionDurationMs => 0;

        /// <inheritdoc />
        public override Widget BuildPage(object? context)
        {
            return _builder();
        }
    }
}

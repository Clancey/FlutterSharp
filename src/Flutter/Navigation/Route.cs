// Route base class for FlutterSharp navigation
// Part of FlutterSharp Phase 5 - Navigation

using System;
using Flutter.Widgets;

namespace Flutter.Navigation
{
    /// <summary>
    /// An abstraction for an entry managed by a Navigator.
    /// This class defines the interface for a route object.
    /// </summary>
    public abstract class Route
    {
        /// <summary>
        /// Creates a route with the given settings.
        /// </summary>
        /// <param name="settings">The route settings.</param>
        protected Route(RouteSettings? settings = null)
        {
            Settings = settings ?? new RouteSettings();
        }

        /// <summary>
        /// The settings for this route.
        /// </summary>
        public RouteSettings Settings { get; protected set; }

        /// <summary>
        /// Whether this route is the current top-most route.
        /// </summary>
        public bool IsCurrent { get; internal set; }

        /// <summary>
        /// Whether this route is the first route in the navigator stack.
        /// </summary>
        public bool IsFirst { get; internal set; }

        /// <summary>
        /// Whether this route has been activated (is visible or was visible).
        /// </summary>
        public bool IsActive { get; internal set; }

        /// <summary>
        /// The navigator that owns this route.
        /// </summary>
        public Navigator? Navigator { get; internal set; }

        /// <summary>
        /// Called when the route is pushed onto the navigator.
        /// </summary>
        public virtual void DidPush() { }

        /// <summary>
        /// Called when the route is replaced by another route.
        /// </summary>
        public virtual void DidReplace(Route? oldRoute) { }

        /// <summary>
        /// Called when the route is popped from the navigator.
        /// </summary>
        /// <param name="result">The result to return to the previous route.</param>
        public virtual void DidPop(object? result) { }

        /// <summary>
        /// Called when a new route has been pushed and this route is no longer current.
        /// </summary>
        public virtual void DidPushNext() { }

        /// <summary>
        /// Called when the route above this one has been popped and this route becomes current again.
        /// </summary>
        public virtual void DidPopNext() { }

        /// <summary>
        /// Called when the route should handle a pop request.
        /// </summary>
        /// <returns>True if the pop should be allowed, false to prevent it.</returns>
        public virtual bool WillPop()
        {
            return true;
        }

        /// <summary>
        /// Creates the widget for this route.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <returns>The widget to display for this route.</returns>
        public abstract Widget BuildPage(object? context);

        /// <summary>
        /// Gets the transition type for this route.
        /// </summary>
        public abstract RouteTransitionType TransitionType { get; }

        /// <summary>
        /// Gets the duration of the push transition in milliseconds.
        /// </summary>
        public virtual int TransitionDurationMs => 300;

        /// <summary>
        /// Gets the duration of the reverse (pop) transition in milliseconds.
        /// </summary>
        public virtual int ReverseTransitionDurationMs => 300;

        /// <summary>
        /// Gets whether this route should maintain state when not visible.
        /// </summary>
        public virtual bool MaintainState => true;

        /// <summary>
        /// Gets whether this route is opaque.
        /// Opaque routes cover the entire navigator area and do not render the route below.
        /// </summary>
        public virtual bool Opaque => true;

        /// <summary>
        /// Gets whether this route is a full-screen dialog.
        /// Full-screen dialogs may have different transitions and a close button instead of back.
        /// </summary>
        public virtual bool FullscreenDialog => false;
    }

    /// <summary>
    /// The type of transition animation to use when navigating.
    /// </summary>
    public enum RouteTransitionType
    {
        /// <summary>
        /// No transition animation.
        /// </summary>
        None = 0,

        /// <summary>
        /// Material Design transition (slide up + fade).
        /// </summary>
        Material = 1,

        /// <summary>
        /// Cupertino/iOS transition (slide from right).
        /// </summary>
        Cupertino = 2,

        /// <summary>
        /// Simple fade transition.
        /// </summary>
        Fade = 3,

        /// <summary>
        /// Slide from bottom transition.
        /// </summary>
        SlideBottom = 4,

        /// <summary>
        /// Slide from right transition.
        /// </summary>
        SlideRight = 5,

        /// <summary>
        /// Zoom transition (scale from center).
        /// </summary>
        Zoom = 6
    }
}

// Route settings for FlutterSharp navigation
// Part of FlutterSharp Phase 5 - Navigation

using System;

namespace Flutter.Navigation
{
    /// <summary>
    /// Data that might be useful in constructing a Route.
    /// </summary>
    public class RouteSettings
    {
        /// <summary>
        /// Creates data used to construct routes.
        /// </summary>
        public RouteSettings() : this(null, null)
        {
        }

        /// <summary>
        /// Creates data used to construct routes.
        /// </summary>
        /// <param name="name">The name of the route (e.g., "/settings").</param>
        /// <param name="arguments">The arguments passed to this route.</param>
        public RouteSettings(string? name, object? arguments = null)
        {
            Name = name;
            Arguments = arguments;
        }

        /// <summary>
        /// Creates a new RouteSettings with the specified name.
        /// </summary>
        public static RouteSettings WithName(string name, object? arguments = null)
        {
            return new RouteSettings(name, arguments);
        }

        /// <summary>
        /// The name of the route (e.g., "/settings").
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// The arguments passed to this route.
        /// </summary>
        public object? Arguments { get; }

        /// <summary>
        /// Creates a copy of this route settings with the given fields replaced.
        /// </summary>
        /// <param name="name">New name, or null to keep the current name.</param>
        /// <returns>A new RouteSettings with the updated values.</returns>
        public RouteSettings CopyWith(string? name = null)
        {
            return new RouteSettings(
                name: name ?? Name,
                arguments: Arguments
            );
        }

        /// <inheritdoc />
        public override string ToString() => $"RouteSettings(\"{Name}\", {Arguments})";
    }
}

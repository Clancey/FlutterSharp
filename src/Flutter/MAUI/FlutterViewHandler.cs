using System;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif ANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.Grid;
#else
using PlatformView = System.Object;
#endif

namespace Flutter.MAUI
{
	/// <summary>
	/// Cross-platform handler for FlutterView that maps to platform-specific implementations
	/// </summary>
	public partial class FlutterViewHandler : ViewHandler<IFlutterView, PlatformView>
	{
		/// <summary>
		/// Property mapper that defines how cross-platform properties map to platform-specific implementations
		/// </summary>
		public static IPropertyMapper<IFlutterView, FlutterViewHandler> Mapper = new PropertyMapper<IFlutterView, FlutterViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IFlutterView.Widget)] = MapWidget,
			[nameof(IFlutterView.AspectRatio)] = MapSizingProperty,
			[nameof(IFlutterView.FillAvailableSpace)] = MapSizingProperty
		};

		/// <summary>
		/// Command mapper for handling commands
		/// </summary>
		public static CommandMapper<IFlutterView, FlutterViewHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		/// <summary>
		/// Initializes a new instance of the FlutterViewHandler
		/// </summary>
		public FlutterViewHandler() : base(Mapper, CommandMapper)
		{
		}

		/// <summary>
		/// Initializes a new instance of the FlutterViewHandler with custom mappers
		/// </summary>
		public FlutterViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		/// <summary>
		/// Maps the Widget property changes to the platform view
		/// </summary>
		private static void MapWidget(FlutterViewHandler handler, IFlutterView view)
		{
			handler.UpdateWidget(view.Widget);
		}

		/// <summary>
		/// Maps sizing property changes to trigger layout update
		/// </summary>
		private static void MapSizingProperty(FlutterViewHandler handler, IFlutterView view)
		{
			handler.UpdateSizing();
		}

		/// <summary>
		/// Platform-specific method to update the widget
		/// Implemented in platform-specific partial classes
		/// </summary>
		partial void UpdateWidget(Widget? widget);

		/// <summary>
		/// Platform-specific method to update sizing
		/// Implemented in platform-specific partial classes
		/// </summary>
		partial void UpdateSizing();
	}
}

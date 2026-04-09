using System;
using System.Collections.Generic;
using Flutter;
using Flutter.Enums;
using Flutter.UI;

namespace Flutter.Widgets
{
	[Obsolete("Use Flutter.Material.AppBar instead.")]
	public class AppBar : global::Flutter.Material.AppBar
	{
		public AppBar(
			global::Flutter.Widget? title = null,
			global::Flutter.Widget? bottom = null
		)
			: base(title, bottom)
		{
		}
	}

	[Obsolete("Use Flutter.Material.Scaffold instead.")]
	public class Scaffold : global::Flutter.Material.Scaffold
	{
		public Scaffold(
			global::Flutter.Widget? appBar = null,
			global::Flutter.Widget? body = null,
			global::Flutter.Widget? floatingActionButton = null,
			global::Flutter.Widget? drawer = null
		)
			: base(appBar, body, floatingActionButton, drawer)
		{
		}
	}

	[Obsolete("Use Flutter.Material.Card instead.")]
	public class Card : global::Flutter.Material.Card
	{
		public Card(
			global::Flutter.Widget? child = null,
			Color? color = null,
			Color? shadowColor = null,
			Color? surfaceTintColor = null,
			double? elevation = null,
			object? shape = null,
			bool borderOnForeground = true,
			EdgeInsetsGeometry? margin = null,
			Clip clipBehavior = Clip.None,
			bool semanticContainer = true
		)
			: base(child, color, shadowColor, surfaceTintColor, elevation, shape, borderOnForeground, margin, clipBehavior, semanticContainer)
		{
		}
	}

	[Obsolete("Use Flutter.Material.Drawer instead.")]
	public class Drawer : global::Flutter.Material.Drawer
	{
		public Drawer(global::Flutter.Widget? child = null)
			: base(child)
		{
		}
	}

	[Obsolete("Use Flutter.Material.FloatingActionButton instead.")]
	public class FloatingActionButton : global::Flutter.Material.FloatingActionButton
	{
		public FloatingActionButton(
			Action? onPressed = null,
			global::Flutter.Widget? child = null,
			string? tooltip = null,
			uint? foregroundColor = null,
			uint? backgroundColor = null,
			uint? focusColor = null,
			uint? hoverColor = null,
			uint? splashColor = null,
			string? heroTag = null,
			double? elevation = null,
			double? focusElevation = null,
			double? hoverElevation = null,
			double? highlightElevation = null,
			double? disabledElevation = null,
			bool mini = false,
			Clip clipBehavior = Clip.None,
			bool autofocus = false,
			bool isExtended = false,
			bool? enableFeedback = null
		)
			: base(onPressed, child, tooltip, foregroundColor, backgroundColor, focusColor, hoverColor, splashColor, heroTag, elevation, focusElevation, hoverElevation, highlightElevation, disabledElevation, mini, clipBehavior, autofocus, isExtended, enableFeedback)
		{
		}
	}

	[Obsolete("Use Flutter.Material.BottomNavigationBarItem instead.")]
	public class BottomNavigationBarItem : global::Flutter.Material.BottomNavigationBarItem
	{
		public BottomNavigationBarItem(
			global::Flutter.Widget icon,
			string? label = null,
			global::Flutter.Widget? activeIcon = null,
			string? tooltip = null,
			uint? backgroundColor = null
		)
			: base(icon, label, activeIcon, tooltip, backgroundColor)
		{
		}
	}

	[Obsolete("Use Flutter.Material.BottomNavigationBar instead.")]
	public class BottomNavigationBar : global::Flutter.Material.BottomNavigationBar
	{
		public BottomNavigationBar(
			List<BottomNavigationBarItem>? items,
			int currentIndex = 0,
			Action<int>? onTap = null,
			double? elevation = null,
			global::Flutter.Material.BottomNavigationBarType? type = null,
			uint? backgroundColor = null,
			double? iconSize = null,
			uint? selectedItemColor = null,
			uint? unselectedItemColor = null,
			double? selectedFontSize = null,
			double? unselectedFontSize = null,
			bool? showSelectedLabels = null,
			bool? showUnselectedLabels = null,
			bool enableFeedback = true,
			global::Flutter.Material.BottomNavigationBarLandscapeLayout landscapeLayout = global::Flutter.Material.BottomNavigationBarLandscapeLayout.Spread,
			bool useLegacyColorScheme = true
		)
			: base(ConvertItems(items), currentIndex, onTap, elevation, type, backgroundColor, iconSize, selectedItemColor, unselectedItemColor, selectedFontSize, unselectedFontSize, showSelectedLabels, showUnselectedLabels, enableFeedback, landscapeLayout, useLegacyColorScheme)
		{
		}

		private static List<global::Flutter.Material.BottomNavigationBarItem> ConvertItems(List<BottomNavigationBarItem>? items)
		{
			if (items == null || items.Count == 0)
			{
				return new List<global::Flutter.Material.BottomNavigationBarItem>();
			}

			var converted = new List<global::Flutter.Material.BottomNavigationBarItem>(items.Count);
			foreach (var item in items)
			{
				converted.Add(item);
			}

			return converted;
		}
	}
}

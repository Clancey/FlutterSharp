using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Enums;
using Flutter.Gestures;
using Flutter.Material;
using Flutter.Services;
using Flutter.UI;
using Flutter.Widgets;
using NUnit.Framework;

namespace FlutterSharp.Tests
{
	[TestFixture]
	public class WidgetMarshalingRegressionTests
	{
		private static readonly MethodInfo PrepareForSendingMethod =
			typeof(Widget).GetMethod("PrepareForSending", BindingFlags.Instance | BindingFlags.NonPublic)!;

		private static readonly FieldInfo CachedBackingStructField =
			typeof(Widget).GetField("_cachedBackingStruct", BindingFlags.Instance | BindingFlags.NonPublic)!;

		private static readonly Assembly FlutterAssembly = typeof(Widget).Assembly;

		private static void PrepareForSending(Widget widget) =>
			PrepareForSendingMethod.Invoke(widget, null);

		private static object GetBackingStruct(Widget widget) =>
			CachedBackingStructField.GetValue(widget)
			?? throw new InvalidOperationException($"No cached backing struct was created for {widget.GetType().Name}.");

		private static T GetPropertyValue<T>(object instance, string propertyName)
		{
			var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				?? throw new InvalidOperationException($"Property '{propertyName}' was not found on '{instance.GetType().FullName}'.");

			return (T)property.GetValue(instance)!;
		}

		private static object ReadStruct(IntPtr pointer, string structTypeName)
		{
			var type = FlutterAssembly.GetType($"Flutter.Structs.{structTypeName}")
				?? throw new InvalidOperationException($"Type 'Flutter.Structs.{structTypeName}' was not found.");

			return Marshal.PtrToStructure(pointer, type)
				?? throw new InvalidOperationException($"Could not marshal '{structTypeName}' from pointer '{pointer}'.");
		}

		[Test]
		public void AlertDialog_PrepareForSending_MarshalsInsetPaddingAndConstraints()
		{
			Widget widget = new AlertDialog(
				title: new Text("Title"),
				content: new Text("Content"),
				insetPadding: EdgeInsets.FromLTRB(1, 2, 3, 4),
				constraints: new BoxConstraints(minWidth: 10, maxWidth: 20, minHeight: 30, maxHeight: 40));

			PrepareForSending(widget);

			var backingStruct = GetBackingStruct(widget);

			Assert.That(GetPropertyValue<byte>(backingStruct, "HasinsetPadding"), Is.EqualTo(1));
			Assert.That(GetPropertyValue<byte>(backingStruct, "Hasconstraints"), Is.EqualTo(1));

			var insetPaddingPointer = GetPropertyValue<IntPtr>(backingStruct, "insetPadding");
			var constraintsPointer = GetPropertyValue<IntPtr>(backingStruct, "constraints");

			Assert.That(insetPaddingPointer, Is.Not.EqualTo(IntPtr.Zero));
			Assert.That(constraintsPointer, Is.Not.EqualTo(IntPtr.Zero));

			var insetPadding = ReadStruct(insetPaddingPointer, "EdgeInsetsStruct");
			Assert.That(GetPropertyValue<double>(insetPadding, "left"), Is.EqualTo(1));
			Assert.That(GetPropertyValue<double>(insetPadding, "top"), Is.EqualTo(2));
			Assert.That(GetPropertyValue<double>(insetPadding, "right"), Is.EqualTo(3));
			Assert.That(GetPropertyValue<double>(insetPadding, "bottom"), Is.EqualTo(4));

			var constraints = ReadStruct(constraintsPointer, "BoxConstraintsStruct");
			Assert.That(GetPropertyValue<double>(constraints, "minWidth"), Is.EqualTo(10));
			Assert.That(GetPropertyValue<double>(constraints, "maxWidth"), Is.EqualTo(20));
			Assert.That(GetPropertyValue<double>(constraints, "minHeight"), Is.EqualTo(30));
			Assert.That(GetPropertyValue<double>(constraints, "maxHeight"), Is.EqualTo(40));
		}

		[Test]
		public void EditableText_PrepareForSending_AssignsEnumProperties()
		{
			Widget widget = new EditableText(
				showCursor: true,
				autocorrect: false,
				keyboardType: TextInputType.Text,
				enableInteractiveSelection: true,
				selectAllOnFocus: false,
				selectionEnabled: true,
				textWidthBasis: TextWidthBasis.LongestLine,
				textAlign: TextAlign.End,
				textDirection: TextDirection.Rtl,
				selectionHeightStyle: BoxHeightStyle.Max,
				selectionWidthStyle: BoxWidthStyle.Max,
				dragStartBehavior: DragStartBehavior.Down,
				clipBehavior: Clip.None);

			PrepareForSending(widget);

			var backingStruct = GetBackingStruct(widget);

			Assert.That(GetPropertyValue<TextWidthBasis>(backingStruct, "textWidthBasis"), Is.EqualTo(TextWidthBasis.LongestLine));
			Assert.That(GetPropertyValue<TextAlign>(backingStruct, "textAlign"), Is.EqualTo(TextAlign.End));
			Assert.That(GetPropertyValue<byte>(backingStruct, "HastextDirection"), Is.EqualTo(1));
			Assert.That(GetPropertyValue<TextDirection>(backingStruct, "textDirection"), Is.EqualTo(TextDirection.Rtl));
			Assert.That(GetPropertyValue<BoxHeightStyle>(backingStruct, "selectionHeightStyle"), Is.EqualTo(BoxHeightStyle.Max));
			Assert.That(GetPropertyValue<BoxWidthStyle>(backingStruct, "selectionWidthStyle"), Is.EqualTo(BoxWidthStyle.Max));
			Assert.That(GetPropertyValue<DragStartBehavior>(backingStruct, "dragStartBehavior"), Is.EqualTo(DragStartBehavior.Down));
			Assert.That(GetPropertyValue<Clip>(backingStruct, "clipBehavior"), Is.EqualTo(Clip.None));
		}

		[Test]
		public void EditableText_PrepareForSending_MarshalsRadiusOffsetAndScrollPadding()
		{
			Widget widget = new EditableText(
				showCursor: true,
				autocorrect: false,
				keyboardType: TextInputType.Text,
				enableInteractiveSelection: true,
				selectAllOnFocus: false,
				selectionEnabled: true,
				cursorRadius: Radius.Circular(6),
				cursorOffset: new Offset(11, 22),
				scrollPadding: EdgeInsets.FromLTRB(7, 8, 9, 10));

			PrepareForSending(widget);

			var backingStruct = GetBackingStruct(widget);

			Assert.That(GetPropertyValue<byte>(backingStruct, "HascursorRadius"), Is.EqualTo(1));
			Assert.That(GetPropertyValue<byte>(backingStruct, "HascursorOffset"), Is.EqualTo(1));

			var radiusPointer = GetPropertyValue<IntPtr>(backingStruct, "cursorRadius");
			var offsetPointer = GetPropertyValue<IntPtr>(backingStruct, "cursorOffset");
			var scrollPaddingPointer = GetPropertyValue<IntPtr>(backingStruct, "scrollPadding");

			Assert.That(radiusPointer, Is.Not.EqualTo(IntPtr.Zero));
			Assert.That(offsetPointer, Is.Not.EqualTo(IntPtr.Zero));
			Assert.That(scrollPaddingPointer, Is.Not.EqualTo(IntPtr.Zero));

			var radius = ReadStruct(radiusPointer, "RadiusStruct");
			Assert.That(GetPropertyValue<double>(radius, "x"), Is.EqualTo(6));
			Assert.That(GetPropertyValue<double>(radius, "y"), Is.EqualTo(6));

			var offset = ReadStruct(offsetPointer, "OffsetStruct");
			Assert.That(GetPropertyValue<double>(offset, "dx"), Is.EqualTo(11));
			Assert.That(GetPropertyValue<double>(offset, "dy"), Is.EqualTo(22));

			var scrollPadding = ReadStruct(scrollPaddingPointer, "EdgeInsetsStruct");
			Assert.That(GetPropertyValue<double>(scrollPadding, "left"), Is.EqualTo(7));
			Assert.That(GetPropertyValue<double>(scrollPadding, "top"), Is.EqualTo(8));
			Assert.That(GetPropertyValue<double>(scrollPadding, "right"), Is.EqualTo(9));
			Assert.That(GetPropertyValue<double>(scrollPadding, "bottom"), Is.EqualTo(10));
		}

		[Test]
		public void Column_CollectionInitializerConstructor_PreservesDefaultFlexLayout()
		{
			Widget widget = new Column
			{
				new Text("One"),
				new Text("Two")
			};

			PrepareForSending(widget);

			var backingStruct = GetBackingStruct(widget);

			Assert.That(GetPropertyValue<MainAxisAlignment>(backingStruct, "mainAxisAlignment"), Is.EqualTo(MainAxisAlignment.Start));
			Assert.That(GetPropertyValue<MainAxisSize>(backingStruct, "mainAxisSize"), Is.EqualTo(MainAxisSize.Max));
			Assert.That(GetPropertyValue<CrossAxisAlignment>(backingStruct, "crossAxisAlignment"), Is.EqualTo(CrossAxisAlignment.Center));
			Assert.That(GetPropertyValue<VerticalDirection>(backingStruct, "verticalDirection"), Is.EqualTo(VerticalDirection.Down));
		}

		[Test]
		public void ListTile_PrepareForSending_PopulatesWidgetId()
		{
			Widget widget = new ListTile(
				title: new Text("Title"),
				subtitle: new Text("Subtitle"));

			PrepareForSending(widget);

			var backingStruct = GetBackingStruct(widget);
			var id = GetPropertyValue<string>(backingStruct, "Id");

			Assert.That(id, Is.Not.Null.And.Not.Empty);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Flutter;
using Flutter.Material;
using Flutter.Structs;
using Flutter.Widgets;
using NUnit.Framework;

namespace FlutterSharp.Tests
{
	[TestFixture]
	public class WidgetPrepareForSendingDispatchTests
	{
		private static readonly MethodInfo PrepareForSendingMethod =
			typeof(Widget).GetMethod("PrepareForSending", BindingFlags.Instance | BindingFlags.NonPublic)!;

		private static readonly FieldInfo CachedBackingStructField =
			typeof(Widget).GetField("_cachedBackingStruct", BindingFlags.Instance | BindingFlags.NonPublic)!;

		private static FieldInfo FindField(Type type, string fieldName)
		{
			Type? current = type;
			while (current != null)
			{
				var field = current.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null)
					return field;

				current = current.BaseType;
			}

			throw new InvalidOperationException($"Field '{fieldName}' was not found on '{type.FullName}'.");
		}

		private static IEnumerable<TestCaseData> MultiChildWidgetFactories()
		{
			yield return new TestCaseData(
				new Func<Widget>(() => new ListView(children: new List<Widget> { new Text("one"), new Text("two") })),
				2)
				.SetName("ListView dispatch populates children");

			yield return new TestCaseData(
				new Func<Widget>(() => new GridView(children: new List<Widget> { new Text("one"), new Text("two") })),
				2)
				.SetName("GridView dispatch populates children");

			yield return new TestCaseData(
				new Func<Widget>(() => new ListWheelScrollView(
					itemExtent: 32,
					children: new List<Widget> { new Text("one"), new Text("two") })),
				2)
				.SetName("ListWheelScrollView dispatch populates children");

			yield return new TestCaseData(
				new Func<Widget>(() => new TabBar(tabs: new List<Widget> { new Tab(text: "one"), new Tab(text: "two") })),
				2)
				.SetName("TabBar dispatch populates tabs");

			yield return new TestCaseData(
				new Func<Widget>(() => new TabBarView(children: new List<Widget> { new Text("one"), new Text("two") })),
				2)
				.SetName("TabBarView dispatch populates children");
		}

		private static IEnumerable<TestCaseData> SingleChildHotspotWidgetFactories()
		{
			yield return new TestCaseData(
				new Func<Widget>(() => new GestureDetector(child: new Text("tap target"))))
				.SetName("GestureDetector dispatch populates child");

			yield return new TestCaseData(
				new Func<Widget>(() => new MouseRegion(child: new Text("hover target"))))
				.SetName("MouseRegion dispatch populates child");

			yield return new TestCaseData(
				new Func<Widget>(() => new Tooltip(message: "Helpful", child: new Text("tooltip target"))))
				.SetName("Tooltip dispatch populates child");
		}

		[TestCaseSource(nameof(MultiChildWidgetFactories))]
		public void PrepareForSending_DispatchedThroughWidgetReference_PopulatesChildren(
			Func<Widget> widgetFactory,
			int expectedChildCount)
		{
			var widget = widgetFactory();

			PrepareForSendingMethod.Invoke(widget, null);

			var backingStruct = CachedBackingStructField.GetValue(widget);
			Assert.That(backingStruct, Is.Not.Null);

			var childrenPointer = (IntPtr)backingStruct!.GetType().GetProperty("children")!.GetValue(backingStruct)!;
			Assert.That(childrenPointer, Is.Not.EqualTo(IntPtr.Zero));

			var children = Marshal.PtrToStructure<ChildrenStruct>(childrenPointer);
			Assert.That(children.childrenLength, Is.EqualTo(expectedChildCount));
			Assert.That(children.children, Is.Not.EqualTo(IntPtr.Zero));

			var childPointers = new IntPtr[children.childrenLength];
			Marshal.Copy(children.children, childPointers, 0, childPointers.Length);
			Assert.That(childPointers, Has.All.Not.EqualTo(IntPtr.Zero));
		}

		[Test]
		public void PrepareForSending_DispatchedThroughWidgetReference_PopulatesBottomNavigationItems()
		{
			Widget widget = new Flutter.Material.BottomNavigationBar(
				items: new List<Flutter.Material.BottomNavigationBarItem>
				{
					new(icon: new Icon(), label: "Home"),
					new(icon: new Icon(), label: "Search")
				});

			PrepareForSendingMethod.Invoke(widget, null);

			var backingStruct = CachedBackingStructField.GetValue(widget);
			Assert.That(backingStruct, Is.Not.Null);

			var itemsPointer = (IntPtr)backingStruct!.GetType().GetProperty("items")!.GetValue(backingStruct)!;
			var itemCount = (int)backingStruct.GetType().GetProperty("itemCount")!.GetValue(backingStruct)!;

			Assert.That(itemsPointer, Is.Not.EqualTo(IntPtr.Zero));
			Assert.That(itemCount, Is.EqualTo(2));

			var itemPointers = new IntPtr[itemCount];
			Marshal.Copy(itemsPointer, itemPointers, 0, itemPointers.Length);
			Assert.That(itemPointers, Has.All.Not.EqualTo(IntPtr.Zero));
		}

		[TestCaseSource(nameof(SingleChildHotspotWidgetFactories))]
		public void PrepareForSending_DispatchedThroughWidgetReference_PopulatesSingleChildHotspotWidgets(
			Func<Widget> widgetFactory)
		{
			var widget = widgetFactory();

			PrepareForSendingMethod.Invoke(widget, null);

			var backingStruct = CachedBackingStructField.GetValue(widget);
			Assert.That(backingStruct, Is.Not.Null);

			var childField = FindField(backingStruct!.GetType(), "_child");
			var childPointer = (IntPtr)childField.GetValue(backingStruct)!;

			Assert.That(childPointer, Is.Not.EqualTo(IntPtr.Zero));
		}
	}
}

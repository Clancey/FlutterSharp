using Flutter.Structs;
using System;
using System.Collections.Generic;

namespace Flutter
{
	public class ListView : MultiChildRenderObjectWidget
	{

	}

	public class ListViewBuilder : Widget
	{
		FixedSizedQueue<Widget> widgetCache = new FixedSizedQueue<Widget>(100);
		public ListViewBuilder(long count = 0, Func<long, Widget> itemBuilder = null)
		{
			var s = GetBackingStruct<ListViewBuilderStruct>();
			s.ItemCount = count;
			ItemBuilder = itemBuilder;
			SetAction<Func<long, Widget>>(GetWidgetForIndex, propertyName: nameof(ItemBuilder));
		}
		protected override FlutterObjectStruct CreateBackingStruct() => new ListViewBuilderStruct();

		Func<long, Widget> ItemBuilder { get; set; }
		Widget GetWidgetForIndex(long index)
		{
			///We cache the items so the GC doesnt kill them!
			var value = ItemBuilder?.Invoke(index);
			if (value != null)
				widgetCache.Enqueue(value);
			return value;
		}
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			widgetCache.Clear();
		}
	}
}

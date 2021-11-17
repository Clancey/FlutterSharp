using Flutter.Structs;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Flutter
{
	public class TabBar : Widget, IEnumerable
	{

		private PinnedObject<NativeArray<long>> pinnedArray;
		IList<Tab> Tabs = new List<Tab>();
		protected override FlutterObjectStruct CreateBackingStruct() => new MultiChildRenderObjectWidgetStruct();
		public void Add(Tab child)
		{
			if (child == null)
				return;
			Tabs.Add(child);
		}

		IEnumerator IEnumerable.GetEnumerator() => Tabs.GetEnumerator();

		public override unsafe void PrepareForSending()
		{
			base.PrepareForSending();
			pinnedArray?.Dispose();

			var array = new NativeArray<long>(Tabs.Count);
			for (int i = 0; i < Tabs.Count; i++)
			{
				var c = Tabs[i];
				c.PrepareForSending();
				array[i] = c;
			}
			pinnedArray = array;
			GetBackingStruct<MultiChildRenderObjectWidgetStruct>().Children = pinnedArray;
		}
	}
}

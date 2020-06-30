using Flutter.Structs;
using System;
namespace Flutter {
	public class ListView : MultiChildRenderObjectWidget {

	}

	public class ListViewBuilder : Widget {
		public ListViewBuilder(long count = 0, Func<long,Widget> itemBuilder = null)
		{
			var s = GetBackingStruct<ListViewBuilderStruct>();
			s.ItemCount = count;
			ItemBuilder = itemBuilder;
		}
		protected override FlutterObjectStruct CreateBackingStruct() => new ListViewBuilderStruct();

		public Func<long, Widget> ItemBuilder {
			get; private set;
			//get => GetProperty<Func<long, Widget>> ();
			//set => SetProperty (value);
		}
	}
}

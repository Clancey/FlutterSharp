using System;
namespace Flutter {
	public class ListView : MultiChildRenderObjectWidget {

	}

	public class ListViewBuilder : Widget {
		public long ItemCount {
			get => GetProperty<long> ();
			set => SetProperty (value);
		}

		public Func<long, Widget> ItemBuilder {
			get => GetProperty<Func<long, Widget>> ();
			set => SetProperty (value);
		}
	}
}

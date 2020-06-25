using System;
namespace Flutter {
	public class DefaultTabController : SingleChildRenderObjectWidget {
		public DefaultTabController(int length)
		{
			Length = length;
		}
		public int Length {
			get => GetProperty<int> ();
			private set => SetProperty (value);
		}
	}
}

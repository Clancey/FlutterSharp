using System;
namespace Flutter {
	public class Row : MultiChildRenderObjectWidget{
		public Row (MainAxisAlignment? mainAxisAlignment) => MainAxisAlignment = mainAxisAlignment;
		public MainAxisAlignment? MainAxisAlignment {
			get => GetProperty<MainAxisAlignment?> ();
			set => SetProperty (value);
		}
	}
}

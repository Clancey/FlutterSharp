using System;
namespace Flutter {
	public class Column : MultiChildRenderObjectWidget{
		public Column (MainAxisAlignment? mainAxisAlignment) => MainAxisAlignment = mainAxisAlignment;
		public MainAxisAlignment? MainAxisAlignment {
			get => GetProperty<MainAxisAlignment?> ();
			set => SetProperty (value);
		}
	}
}

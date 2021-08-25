using Flutter.Structs;
using System;
namespace Flutter {
	public class Row : MultiChildRenderObjectWidget{
		public Row (MainAxisAlignment? mainAxisAlignment) => GetBackingStruct<RowStruct>().MainAxisAlignment = mainAxisAlignment;
		protected override FlutterObjectStruct CreateBackingStruct() => new RowStruct();
	}
}

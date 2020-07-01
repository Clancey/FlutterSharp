using Flutter.Structs;
using System;
namespace Flutter {
	public class Column : MultiChildRenderObjectWidget{
		public Column(MainAxisAlignment? mainAxisAlignment = null) 
			=> GetBackingStruct<ColumnStruct>().MainAxisAlignment = mainAxisAlignment;
		protected override FlutterObjectStruct CreateBackingStruct() => new ColumnStruct();
	}
}

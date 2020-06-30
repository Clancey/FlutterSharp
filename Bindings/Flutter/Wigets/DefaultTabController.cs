using Flutter.Structs;
using System;
namespace Flutter {
	public class DefaultTabController : SingleChildRenderObjectWidget {
		public DefaultTabController(int length) => GetBackingStruct<DefaultTabControllerStruct>().Length = length;
		protected override FlutterObjectStruct CreateBackingStruct() => new DefaultTabControllerStruct();
	}
}

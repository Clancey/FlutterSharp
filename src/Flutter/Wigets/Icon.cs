using Flutter.Structs;
using System;
namespace Flutter
{
	public class Icon : Widget
	{
		public Icon(string codePoint = null, string fontFamily = null)
		{
			var s = GetBackingStruct<IconStruct>();
			s.CodePoint = codePoint;
			s.FontFamily = fontFamily;
		}
		protected override FlutterObjectStruct CreateBackingStruct() => new IconStruct();
	}
}

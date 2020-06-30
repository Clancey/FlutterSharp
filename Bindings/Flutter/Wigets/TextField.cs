using Flutter.Structs;
using System;
namespace Flutter {
	public class TextField : Widget {
		public TextField(string text = null, string hint = null)
		{
			var s = GetBackingStruct<TextFieldStruct>();
			s.Value = text;
			s.Hint = hint;
		}
		protected override FlutterObjectStruct CreateBackingStruct() => new TextFieldStruct();

		public Action<string> OnInput {
			get;set;
			//get => GetProperty<Action<string>> ();
			//set => SetProperty (value);
		}

		public Action<string> OnChange {
			get;set;
			//get => GetProperty<Action<string>> ();
			//set => SetProperty (value);
		}
	}
}

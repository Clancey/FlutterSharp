using Flutter.Structs;
using System;
namespace Flutter {
	public class Checkbox : Widget {
		public Checkbox(bool? value) => GetBackingStruct<CheckboxStruct>().Value = value ?? false;
		protected override FlutterObjectStruct CreateBackingStruct() => new CheckboxStruct();
	
		public Action<bool> OnChange {
			get => GetAction<Action<bool>> ();
			set => SetAction (value);
		}
	}
}

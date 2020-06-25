using System;
namespace Flutter {
	public class TextStyle : FlutterObject{

		public Color? Color {
			get => GetProperty<Color?> ();
			set => SetProperty (value);
		}
	}
}

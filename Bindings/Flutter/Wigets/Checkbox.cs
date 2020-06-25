using System;
namespace Flutter {
	public class Checkbox : Widget {
		public bool Value {
			get => GetProperty<bool> ();
			set => SetProperty (value);
		}

		public Action<bool> OnChange {
			get => GetProperty<Action<bool>> ();
			set => SetProperty (value);
		}
	}
}

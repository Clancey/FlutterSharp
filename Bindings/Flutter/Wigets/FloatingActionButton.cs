using System;
namespace Flutter {
	public class FloatingActionButton : SingleChildRenderObjectWidget {
		public FloatingActionButton (Action onPressed = null)
			=> OnPressed = onPressed;
		public Action OnPressed {
			get => GetAction<Action> ();
			set => SetAction (value);
		}
	}
}

using System;
namespace Flutter {
	public class AspectRatio : Widget {
		public AspectRatio(double value)
		{
			Value = value;
		}
		public double? Value {
			get => GetProperty<double?> (propertyName:"aspectRatio",shouldCamelCase:false);
			set => SetProperty (value, propertyName: "aspectRatio", shouldCamelCase: false);
		}

		public Widget Child {
			get => GetProperty<Widget> ();
			set => SetProperty (value);
		}
	}
}

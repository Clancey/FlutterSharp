using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Collections;
using Flutter.Internal;

namespace Flutter {

	

	public abstract class Widget : FlutterObject {
		public string Id => GetProperty<string> (propertyName: "id", shouldCamelCase: false);
		public Widget ()
		{
			properties ["id"] = IDGenerator.Instance.Next;
			FlutterManager.TrackWidget (this);
		}
		
		~Widget ()
		{
			FlutterManager.UntrackWidget (this);
		}
	}


	public abstract class SingleChildRenderObjectWidget : Widget, IEnumerable {

		public Widget Child {
			get => GetProperty<Widget> ();
			set => SetProperty (value);
		}

		public void Add (Widget child)
		{
			if (child == null)
				return;
			if (Child != null)
				throw new Exception ("Cannot have more than one child");
			Child = child;
		}

		IEnumerator IEnumerable.GetEnumerator () => new Widget [] { Child }.GetEnumerator ();
	}

	public abstract class MultiChildRenderObjectWidget : Widget, IEnumerable {

		public IList<Widget> Children {
			get => GetProperty<IList<Widget>> () ?? (Children = new List<Widget> ());
			set => SetProperty (value);
		}

		public void Add (Widget child)
		{
			if (child == null)
				return;
			Children.Add (child);
		}

		IEnumerator IEnumerable.GetEnumerator () => Children.GetEnumerator ();
	}

}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections;
using Flutter.Internal;
using Flutter.Structs;
using System.Xml.Linq;
using System.Linq;
using Flutter.HotReload;

namespace Flutter
{
	public abstract class Widget : FlutterObject
	{
		public string Id => BackingStruct.Id;
		public Widget()
		{
			BackingStruct.Id = IDGenerator.Instance.Next;
			FlutterManager.TrackWidget(this);
		}

		WidgetStruct BackingStruct => (WidgetStruct)FlutterObjectStruct;
		protected override FlutterObjectStruct CreateBackingStruct() => new WidgetStruct();
		~Widget()
		{
			FlutterManager.UntrackWidget(this);
		}

		public virtual void PrepareForSending()
		{

		}
	}


	public abstract class SingleChildRenderObjectWidget : Widget, IEnumerable
	{

		Widget child;
		protected Widget Child
		{
			get => child;
			set
			{
				child = value;
				BackingStruct.Child = value;
			}
		}
		SingleChildRenderObjectWidgetStruct BackingStruct => (SingleChildRenderObjectWidgetStruct)FlutterObjectStruct;
		protected override FlutterObjectStruct CreateBackingStruct() => new SingleChildRenderObjectWidgetStruct();

		public void Add(Widget child)
		{
			if (child == null)
				return;
			if (Child != null)
				throw new Exception("Cannot have more than one child");
			Child = child;
		}
		public override void PrepareForSending()
		{
			SetupChild();
			Child?.PrepareForSending();
			base.PrepareForSending();
		}

		protected void SetupChild()
		{
			var oldChild = Child;
			try
			{
				if (FlutterHotReloadHelper.IsEnabled)
				{
					var replaced = FlutterHotReloadHelper.GetReplacedView(this);
					if (replaced != this)
					{
						Child = replaced;
						Child?.PrepareForSending();
						return;
					}
				}
				if (this is IBuildableWidget ibw && Child == null)
					Child = ibw.Build();

			}
			finally
			{
				if (oldChild != null && oldChild != Child)
				{
					oldChild.Dispose();
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => new Widget[] { Child }.GetEnumerator();
	}

	public abstract class MultiChildRenderObjectWidget : Widget, IEnumerable
	{

		protected override FlutterObjectStruct CreateBackingStruct() => new MultiChildRenderObjectWidgetStruct();


		private PinnedObject<NativeArray<long>> pinnedArray;
		protected IList<Widget> Children { get; set; } = new List<Widget>();

		public void Add(Widget child)
		{
			if (child == null)
				return;
			Children.Add(child);
		}

		IEnumerator IEnumerable.GetEnumerator() => Children.GetEnumerator();

		public override unsafe void PrepareForSending()
		{
			pinnedArray?.Dispose();

			var array = new NativeArray<long>(Children.Count);
			for (int i = 0; i < Children.Count; i++)
			{
				var c = Children[i];
				c.PrepareForSending();
				array[i] = c;
			}
			pinnedArray = array;
			GetBackingStruct<MultiChildRenderObjectWidgetStruct>().Children = pinnedArray;
		}
	}

}

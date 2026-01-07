using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Flutter.Structs;

namespace Flutter
{
	/// <summary>
	/// Base class for all Flutter widgets
	/// </summary>
	public abstract class Widget : IDisposable
	{
		protected FlutterObjectStruct? backingStruct;
		protected bool disposed = false;

		// Track allocated children arrays for cleanup
		private List<IntPtr> _allocatedChildrenArrays = new List<IntPtr>();

		/// <summary>
		/// Unique identifier for this widget instance
		/// </summary>
		public string Id { get; } = Guid.NewGuid().ToString();

		/// <summary>
		/// Sends an event to this widget
		/// </summary>
		/// <param name="eventName">The name of the event</param>
		/// <param name="data">Event data as JSON string</param>
		/// <param name="callback">Optional callback for responses</param>
		public virtual void SendEvent(string eventName, string data, Action<string>? callback = null)
		{
			// Override in derived classes to handle specific events
		}

		/// <summary>
		/// Gets the backing struct for FFI interop
		/// </summary>
		protected T GetBackingStruct<T>() where T : FlutterObjectStruct, new()
		{
			if (backingStruct == null)
			{
				backingStruct = new T();
				backingStruct.WidgetType = GetType().Name;
			}
			return (T)backingStruct;
		}

		/// <summary>
		/// Sets an IntPtr field from a widget reference.
		/// Prepares the child widget for sending and assigns its handle.
		/// </summary>
		protected void SetIntPtr(ref IntPtr ptr, Widget? widget)
		{
			if (widget == null)
			{
				ptr = IntPtr.Zero;
				return;
			}
			widget.PrepareForSending();
			ptr = widget.backingStruct?.Handle ?? IntPtr.Zero;
		}

		/// <summary>
		/// Gets the IntPtr handle for a widget, preparing it for sending if needed.
		/// </summary>
		protected IntPtr GetWidgetHandle(Widget? widget)
		{
			if (widget == null)
				return IntPtr.Zero;

			widget.PrepareForSending();
			return widget.backingStruct?.Handle ?? IntPtr.Zero;
		}

		/// <summary>
		/// Converts a list of child widgets to an IntPtr array and returns the pointer.
		/// The array memory is tracked and will be freed on Dispose.
		/// </summary>
		protected IntPtr SetChildrenAndGetPointer(List<Widget>? children)
		{
			if (children == null || children.Count == 0)
			{
				return IntPtr.Zero;
			}

			// Prepare all children for sending and collect their pointers
			var pointers = new IntPtr[children.Count];
			for (int i = 0; i < children.Count; i++)
			{
				if (children[i] != null)
				{
					children[i].PrepareForSending();
					pointers[i] = children[i].backingStruct?.Handle ?? IntPtr.Zero;
				}
				else
				{
					pointers[i] = IntPtr.Zero;
				}
			}

			// Allocate unmanaged memory for the pointer array
			int size = IntPtr.Size * children.Count;
			var ptr = Marshal.AllocHGlobal(size);

			// Copy the pointers to unmanaged memory
			Marshal.Copy(pointers, 0, ptr, children.Count);

			// Track this allocation for cleanup
			_allocatedChildrenArrays.Add(ptr);

			return ptr;
		}

		/// <summary>
		/// Creates the backing struct for this widget
		/// </summary>
		protected abstract FlutterObjectStruct CreateBackingStruct();

		/// <summary>
		/// Prepares the widget for sending to Flutter
		/// </summary>
		internal void PrepareForSending()
		{
			if (backingStruct == null)
			{
				backingStruct = CreateBackingStruct();
				backingStruct.WidgetType = GetType().Name;
			}
		}

		/// <summary>
		/// Implicit conversion to IntPtr for FFI
		/// </summary>
		public static implicit operator IntPtr(Widget? widget)
		{
			if (widget == null)
				return IntPtr.Zero;

			widget.PrepareForSending();
			return widget.backingStruct?.Handle ?? IntPtr.Zero;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				// Free all allocated children array pointers
				foreach (var ptr in _allocatedChildrenArrays)
				{
					if (ptr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(ptr);
					}
				}
				_allocatedChildrenArrays.Clear();

				if (disposing)
				{
					backingStruct?.Dispose();
				}
				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}

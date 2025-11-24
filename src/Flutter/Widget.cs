using System;
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

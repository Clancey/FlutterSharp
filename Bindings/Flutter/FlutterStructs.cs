using System;
using System.Runtime.InteropServices;

namespace Flutter.Structs {

	[StructLayout (LayoutKind.Sequential)]
	internal unsafe class BaseStruct : IDisposable {
		public IntPtr Handle { get; private set; }

		public BaseStruct ()
		{
			var gchandle = GCHandle.Alloc (this, GCHandleType.Pinned);
			Handle = gchandle.AddrOfPinnedObject ();
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (Handle == IntPtr.Zero)
				return;
			if (disposing) {
				// TODO: dispose managed state (managed objects)
			}
			var gchandle = GCHandle.FromIntPtr (Handle);
			gchandle.Free ();
			Handle = IntPtr.Zero;
		}
		~BaseStruct ()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose (disposing: false);
		}

		public void Dispose ()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}


		protected string GetString (IntPtr ptr) => Marshal.PtrToStringUTF8 (ptr);
		protected void SetString (ref IntPtr ptr,string value) => ptr = Marshal.StringToCoTaskMemUTF8 (value);

		protected void SetValue<T> (ref IntPtr ptr, T value) where T : BaseStruct =>
			ptr = value.Handle;

		protected T GetValue<T> (IntPtr ptr) => Marshal.PtrToStructure<T> (ptr);
	}


	[StructLayout (LayoutKind.Sequential)]
	internal class FlutterObjectStruct  : BaseStruct{
		private IntPtr widgetType;
		public string WidgetType {
			get => GetString (widgetType);
			set => SetString (ref widgetType, value);
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	internal class WidgetStruct : FlutterObjectStruct {
		private IntPtr id;
		public string Id {
			get => GetString (id);
			set => SetString (ref id, value);
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	internal class SingleChildRenderObjectWidgetStruct : WidgetStruct {
		IntPtr child;
		public BaseStruct Child {
			get => GetValue<BaseStruct> (child);
			set => SetValue (ref child, value);
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	internal unsafe class TextStruct : WidgetStruct {
		IntPtr _value;
		public string Value {
			get => GetString (_value);
			set => SetString (ref _value, value);
		}

		//public double? ScaleFactor {
		//	get => _scaleFactor.HasValue ? _scaleFactor.Value : null;
		//	set => SetValue (scaleFactor, value);
		//}
	}

	[StructLayout (LayoutKind.Sequential)]
	public readonly struct NativeNullable<T>
	where T : unmanaged {
		private const byte Yes = 1;
		private const byte No = 0;
		private readonly byte hasValue;
		private readonly T value;
		public NativeNullable (T value)
		{
			hasValue = Yes;
			this.value = value;
		}
		public NativeNullable (Nullable<T> value)
		{
			hasValue = value.HasValue ? Yes : No;
			if (hasValue == Yes)
				this.value = value.Value;
			else
				this.value = default;
		}
		public readonly bool HasValue =>
			hasValue == Yes;
		public readonly T Value {
			get {
				if (HasValue)
					throw new NullReferenceException ();
				return value;
			}
		}
		public readonly T GetValueOrDefault () =>
			value;
		public readonly T GetValueOrDefault (T defaultValue) =>
			HasValue ? value : defaultValue;
		public override bool Equals (object other)
		{
			if (HasValue)
				return other == null;
			if (other == null)
				return false;
			return value.Equals (other);
		}
		public override int GetHashCode () =>
			HasValue ? value.GetHashCode () : 0;
		public override string ToString () =>
			HasValue ? value.ToString () : string.Empty;
		public static implicit operator NativeNullable<T> (Nullable<T> value) =>
			new NativeNullable<T> (value);
		public static implicit operator NativeNullable<T> (T value) =>
			new NativeNullable<T> (value);
		public static implicit operator Nullable<T> (NativeNullable<T> value) =>
			value.HasValue ? default : value.Value;
		public static explicit operator T (NativeNullable<T> value) =>
			value.Value;
	}
}

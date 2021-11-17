using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Flutter.Structs
{

	[StructLayout(LayoutKind.Sequential)]
	public unsafe class BaseStruct : IDisposable
	{
		private IntPtr handle;
		private IntPtr manahedHandle;
		public IntPtr Handle => handle;

		public BaseStruct()
		{
			var gchandle = GCHandle.Alloc(this, GCHandleType.Pinned);
			manahedHandle = (IntPtr)gchandle;
			handle = gchandle.AddrOfPinnedObject();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (handle == IntPtr.Zero)
				return;
			if (disposing)
			{
				// TODO: dispose managed state (managed objects)
			}
			var gchandle = GCHandle.FromIntPtr(manahedHandle);
			gchandle.Free();
			handle = IntPtr.Zero;
		}
		~BaseStruct()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		protected string GetString(IntPtr ptr) => Marshal.PtrToStringUTF8(ptr);
		protected void SetString(ref IntPtr ptr, string value) => ptr = Marshal.StringToCoTaskMemUTF8(value);
		protected void SetIntPtr(ref IntPtr ptr, Widget flutterObject)
		{
			flutterObject?.PrepareForSending();
			ptr = flutterObject;
		}
		const byte Yes = 1;
		const byte No = 0;
		protected bool GetValue(byte value) => value == Yes;
		protected void SetValue(ref byte pointer, bool value) => pointer = value ? Yes : No;
	}


	[StructLayout(LayoutKind.Sequential)]
	public class FlutterObjectStruct : BaseStruct
	{
		private IntPtr widgetType;
		public string WidgetType
		{
			get => GetString(widgetType);
			set => SetString(ref widgetType, value);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class WidgetStruct : FlutterObjectStruct
	{
		private IntPtr id;
		public string Id
		{
			get => GetString(id);
			set => SetString(ref id, value);
		}
	}


	[StructLayout(LayoutKind.Sequential)]
	public class SingleChildRenderObjectWidgetStruct : WidgetStruct
	{
		public IntPtr Child { get; set; }
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe class MultiChildRenderObjectWidgetStruct : WidgetStruct
	{
		public IntPtr Children;
	}

	[StructLayout(LayoutKind.Sequential)]
	class AlignStruct : SingleChildRenderObjectWidgetStruct
	{
		public NativeNullable<Alignment> Alignment { get; set; }
		public NativeNullable<double> WidthFactor { get; set; }
		public NativeNullable<double> HeightFactor { get; set; }
	}

	[StructLayout(LayoutKind.Sequential)]
	class AppBarStruct : WidgetStruct
	{
		IntPtr title;
		IntPtr bottom;
		public Widget Title { set => SetIntPtr(ref title, value); }
		public Widget Bottom { set => SetIntPtr(ref bottom, value); }
	}


	[StructLayout(LayoutKind.Sequential)]
	class AspectRatioStruct : SingleChildRenderObjectWidgetStruct
	{
		public NativeNullable<double> Value { get; set; }
	}


	[StructLayout(LayoutKind.Sequential)]
	class CheckboxStruct : WidgetStruct
	{
		byte _value;
		public bool Value
		{
			get => GetValue(_value);
			set => SetValue(ref _value, value);
		}

	}

	[StructLayout(LayoutKind.Sequential)]
	class ColumnStruct : MultiChildRenderObjectWidgetStruct
	{
		public NativeNullable<MainAxisAlignment> MainAxisAlignment { get; set; }
	}

	[StructLayout(LayoutKind.Sequential)]
	class ContainerStruct : SingleChildRenderObjectWidgetStruct
	{
		public NativeNullable<Alignment> Alignment { get; set; }
		public NativeNullable<EdgeInsetsGeometry> Padding { get; set; }
		public NativeNullable<EdgeInsetsGeometry> Margin { get; set; }
		public NativeNullable<Color> Color { get; set; }
		public NativeNullable<double> Width { get; set; }
		public NativeNullable<double> Height { get; set; }

	}

	[StructLayout(LayoutKind.Sequential)]
	class DefaultTabControllerStruct : SingleChildRenderObjectWidgetStruct
	{
		public int Length { get; set; }
	}

	[StructLayout(LayoutKind.Sequential)]
	class IconStruct : WidgetStruct
	{
		IntPtr codePoint;
		public string CodePoint
		{
			get => GetString(codePoint);
			set => SetString(ref codePoint, value);
		}
		IntPtr fontFamily;
		public string FontFamily
		{
			get => GetString(fontFamily);
			set => SetString(ref fontFamily, value);
		}

	}

	[StructLayout(LayoutKind.Sequential)]
	class ListViewBuilderStruct : WidgetStruct
	{
		public long ItemCount { get; set; }
	}


	[StructLayout(LayoutKind.Sequential)]
	class RowStruct : MultiChildRenderObjectWidgetStruct
	{
		public NativeNullable<MainAxisAlignment> MainAxisAlignment { get; set; }
	}


	[StructLayout(LayoutKind.Sequential)]
	class ScaffoldStruct : WidgetStruct
	{
		IntPtr appBar;
		public AppBar AppBar { set => SetIntPtr(ref appBar, value); }

		IntPtr floatingActionButton;
		public FloatingActionButton FloatingActionButton { set => SetIntPtr(ref floatingActionButton, value); }

		IntPtr drawer;
		public Drawer Drawer { set => SetIntPtr(ref drawer, value); }

		IntPtr body;
		public Widget Body { set => SetIntPtr(ref body, value); }
	}


	[StructLayout(LayoutKind.Sequential)]
	public unsafe class TextStruct : WidgetStruct
	{
		IntPtr _value;
		public string Value
		{
			get => GetString(_value);
			set => SetString(ref _value, value);
		}
		public NativeNullable<double> ScaleFactor { get; set; }
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe class TextFieldStruct : WidgetStruct
	{
		IntPtr _value;
		public string Value
		{
			get => GetString(_value);
			set => SetString(ref _value, value);
		}

		IntPtr hint;
		public string Hint
		{
			get => GetString(hint);
			set => SetString(ref hint, value);
		}
	}



	public class PinnedObject<T> : IDisposable
	{
		private T value;
		private GCHandle gcHandle;
		private bool disposeUnderlyingObject;

		public PinnedObject(T value, bool disposeUnderlyingObject = true)
		{
			Console.WriteLine("PinnedObject.ctor()");

			this.value = value;
			this.disposeUnderlyingObject = disposeUnderlyingObject;

			gcHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
		}

		public T Value => value;

		public static implicit operator IntPtr(PinnedObject<T> pinned) =>
			pinned.gcHandle.AddrOfPinnedObject();

		public static implicit operator T(PinnedObject<T> pinned) =>
			pinned.value;

		public static implicit operator PinnedObject<T>(T value) =>
			new PinnedObject<T>(value);

		protected virtual void Dispose(bool disposing)
		{
			Console.WriteLine("PinnedObject.Dispose()");

			if (disposeUnderlyingObject && value is IDisposable disposable)
				disposable.Dispose();

			if (gcHandle.IsAllocated)
				gcHandle.Free();

			gcHandle = default;
			value = default;
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public readonly struct NativeNullable<T>
		where T : unmanaged
	{
		private const byte Yes = 1;
		private const byte No = 0;

		private readonly byte hasValue;
		private readonly T value;

		public NativeNullable(T value)
		{
			hasValue = Yes;
			this.value = value;
		}

		public NativeNullable(Nullable<T> value)
		{
			hasValue = value.HasValue ? Yes : No;
			if (hasValue == Yes)
				this.value = value.Value;
			else
				this.value = default;
		}

		public readonly bool HasValue =>
			hasValue == Yes;

		public readonly T Value
		{
			get
			{
				if (HasValue)
					throw new NullReferenceException();
				return value;
			}
		}

		public readonly T GetValueOrDefault() =>
			value;

		public readonly T GetValueOrDefault(T defaultValue) =>
			HasValue ? value : defaultValue;

		public override bool Equals(object other)
		{
			if (HasValue)
				return other == null;
			if (other == null)
				return false;
			return value.Equals(other);
		}

		public override int GetHashCode() =>
			HasValue ? value.GetHashCode() : 0;

		public override string ToString() =>
			HasValue ? value.ToString() : string.Empty;

		public static implicit operator NativeNullable<T>(Nullable<T> value) =>
			new NativeNullable<T>(value);

		public static implicit operator NativeNullable<T>(T value) =>
			new NativeNullable<T>(value);

		public static implicit operator Nullable<T>(NativeNullable<T> value) =>
			value.HasValue ? default : value.Value;

		public static explicit operator T(NativeNullable<T> value) =>
			value.Value;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe class NativeArray<T> : IDisposable
		where T : unmanaged
	{
		private void* array;
		private int length;

		public NativeArray(int length)
		{
			Console.WriteLine("NativeArray<T>.ctor()");

			if (length <= 0)
			{
				this.length = 0;
				array = null;
				return;
			}

			this.length = length;
			array = (void*)Marshal.AllocHGlobal(sizeof(T) * length);

			// clear array
			Value.Fill(default);
		}

		public NativeArray(T[] array)
		{
			Console.WriteLine("NativeArray<T>.ctor()");

			if (array?.Length <= 0)
			{
				length = 0;
				this.array = null;
				return;
			}

			length = array.Length;
			this.array = (void*)Marshal.AllocHGlobal(sizeof(T) * length);

			// copy array
			array.AsSpan().CopyTo(Value);
		}

		public Span<T> Value =>
			new Span<T>(array, length);

		public T this[int index]
		{
			get => Value[index];
			set => Value[index] = value;
		}

		public int Length =>
			length;

		public void Dispose()
		{
			Console.WriteLine("NativeArray<T>.Dispose()");

			if (array == null)
				return;

			Marshal.FreeHGlobal((IntPtr)array);

			length = 0;
			array = null;
		}
	}

}

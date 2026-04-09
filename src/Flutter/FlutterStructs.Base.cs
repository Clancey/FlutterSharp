using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Flutter.Logging;

namespace Flutter.Structs
{
	/// <summary>
	/// Tracks memory allocations for struct instances.
	/// We use a static dictionary instead of instance fields to keep the struct classes blittable for FFI pinning.
	/// </summary>
	internal static class StructMemoryTracker
	{
		// Key: GCHandle IntPtr value, Value: tracking info
		private static readonly ConcurrentDictionary<IntPtr, AllocationInfo> _allocations = new();

		internal class AllocationInfo
		{
			public List<IntPtr> AllocatedStrings { get; } = new();
			public List<IntPtr> AllocatedChildrenArrays { get; } = new();
		}

		public static AllocationInfo GetOrCreate(IntPtr handle)
		{
			return _allocations.GetOrAdd(handle, _ => new AllocationInfo());
		}

		public static bool TryGet(IntPtr handle, out AllocationInfo info)
		{
			return _allocations.TryGetValue(handle, out info);
		}

		public static void Remove(IntPtr handle)
		{
			_allocations.TryRemove(handle, out _);
		}

		public static void TrackString(IntPtr handle, IntPtr stringPtr)
		{
			if (stringPtr != IntPtr.Zero)
			{
				GetOrCreate(handle).AllocatedStrings.Add(stringPtr);
			}
		}

		public static void UntrackString(IntPtr handle, IntPtr stringPtr)
		{
			if (TryGet(handle, out var info))
			{
				info.AllocatedStrings.Remove(stringPtr);
			}
		}

		public static void TrackChildrenArray(IntPtr handle, IntPtr arrayPtr)
		{
			if (arrayPtr != IntPtr.Zero)
			{
				GetOrCreate(handle).AllocatedChildrenArrays.Add(arrayPtr);
			}
		}

		public static void UntrackChildrenArray(IntPtr handle, IntPtr arrayPtr)
		{
			if (TryGet(handle, out var info))
			{
				info.AllocatedChildrenArrays.Remove(arrayPtr);
			}
		}

		public static void FreeAllAllocations(IntPtr handle)
		{
			if (!TryGet(handle, out var info))
				return;

			// Free all string allocations
			foreach (var ptr in info.AllocatedStrings)
			{
				if (ptr != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(ptr);
				}
			}
			info.AllocatedStrings.Clear();

			// Free all children array allocations
			foreach (var ptr in info.AllocatedChildrenArrays)
			{
				if (ptr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(ptr);
				}
			}
			info.AllocatedChildrenArrays.Clear();

			Remove(handle);
		}
	}

	/// <summary>
	/// Base class for all Flutter struct types.
	///
	/// This class uses a blittable layout for FFI interop. Memory tracking for strings
	/// and children arrays is handled externally by StructMemoryTracker to keep this
	/// class pinnable.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe class BaseStruct : IDisposable
	{
		// These are the first two fields expected by Dart FFI
		private IntPtr handle;
		private IntPtr managedHandle;

		public IntPtr Handle => handle;

		public BaseStruct()
		{
			var gchandle = GCHandle.Alloc(this, GCHandleType.Pinned);
			managedHandle = (IntPtr)gchandle;
			handle = gchandle.AddrOfPinnedObject();
			if (handle == IntPtr.Zero)
			{
				// AddrOfPinnedObject may return zero for non-blittable reference types; fall back to using GCHandle pointer
				handle = (IntPtr)gchandle;
				FlutterSharpLogger.LogWarning("BaseStruct: AddrOfPinnedObject returned zero, falling back to GCHandle pointer for handle.");
			}

			// Track struct creation for memory diagnostics
			Flutter.MemoryDiagnostics.TrackStructCreation(managedHandle, GetType().Name);
		}

		protected virtual void Dispose(bool disposing)
		{
			var currentManagedHandle = managedHandle;
			if (currentManagedHandle == IntPtr.Zero)
				return;

			// Track struct disposal for memory diagnostics
			Flutter.MemoryDiagnostics.TrackStructDisposal(currentManagedHandle);

			// Free all tracked allocations
			StructMemoryTracker.FreeAllAllocations(currentManagedHandle);

			var gchandle = GCHandle.FromIntPtr(currentManagedHandle);
			if (gchandle.IsAllocated)
			{
				gchandle.Free();
			}

			handle = IntPtr.Zero;
			managedHandle = IntPtr.Zero;
		}

		~BaseStruct()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		protected string GetString(IntPtr ptr) => ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);

		protected void SetString(ref IntPtr ptr, string value)
		{
			// Free previous string if it was allocated
			if (ptr != IntPtr.Zero)
			{
				StructMemoryTracker.UntrackString(managedHandle, ptr);
				Marshal.FreeCoTaskMem(ptr);
			}

			// Allocate new string
			if (value != null)
			{
				ptr = Marshal.StringToCoTaskMemUTF8(value);
				StructMemoryTracker.TrackString(managedHandle, ptr);
			}
			else
			{
				ptr = IntPtr.Zero;
			}
		}

		protected void SetIntPtr(ref IntPtr ptr, Flutter.Widget flutterObject)
		{
			flutterObject?.PrepareForSending();
			ptr = flutterObject;
		}

		/// <summary>
		/// Sets an IntPtr field directly. Used when the struct property type is IntPtr.
		/// </summary>
		protected void SetIntPtr(ref IntPtr ptr, IntPtr value)
		{
			ptr = value;
		}

		/// <summary>
		/// Sets an IntPtr field from a nullable IntPtr value.
		/// </summary>
		protected void SetIntPtr(ref IntPtr ptr, IntPtr? value)
		{
			ptr = value ?? IntPtr.Zero;
		}

		/// <summary>
		/// Sets the children pointer field to an array of widget pointers.
		/// The array memory is tracked and will be freed on Dispose.
		/// </summary>
		/// <param name="ptr">Reference to the children pointer field</param>
		/// <param name="children">List of child widgets (can be null or empty)</param>
		/// <param name="countField">Reference to the count field that stores the number of children</param>
		protected void SetChildren(ref IntPtr ptr, List<Flutter.Widget> children, ref int countField)
		{
			// Free previous children wrapper struct if it was allocated
			if (ptr != IntPtr.Zero)
			{
				StructMemoryTracker.UntrackChildrenArray(managedHandle, ptr);
				var previousChildren = Marshal.PtrToStructure<ChildrenStruct>(ptr);
				if (previousChildren.children != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(previousChildren.children);
				}
				Marshal.FreeHGlobal(ptr);
			}

			// Handle null or empty children list
			if (children == null || children.Count == 0)
			{
				ptr = IntPtr.Zero;
				countField = 0;
				return;
			}

			// Prepare all children for sending and collect their pointers
			var pointers = new IntPtr[children.Count];
			for (int i = 0; i < children.Count; i++)
			{
				children[i]?.PrepareForSending();
				pointers[i] = children[i];
			}

			// Allocate unmanaged memory for the child pointer array
			int size = IntPtr.Size * children.Count;
			var itemsPtr = Marshal.AllocHGlobal(size);

			// Copy the child pointers to unmanaged memory
			Marshal.Copy(pointers, 0, itemsPtr, children.Count);

			var childrenStruct = new ChildrenStruct
			{
				children = itemsPtr,
				childrenLength = children.Count
			};

			ptr = Marshal.AllocHGlobal(Marshal.SizeOf<ChildrenStruct>());
			Marshal.StructureToPtr(childrenStruct, ptr, false);

			// Track the wrapper struct pointer for cleanup.
			StructMemoryTracker.TrackChildrenArray(managedHandle, ptr);
			countField = children.Count;
		}

		/// <summary>
		/// Simplified SetChildren for structs that only have a children IntPtr field
		/// without a separate count field (count is embedded or derived).
		/// </summary>
		/// <param name="ptr">Reference to the children pointer field</param>
		/// <param name="children">List of child widgets (can be null or empty)</param>
		/// <returns>The number of children set</returns>
		protected int SetChildren(ref IntPtr ptr, List<Flutter.Widget> children)
		{
			// Free previous children wrapper struct if it was allocated
			if (ptr != IntPtr.Zero)
			{
				StructMemoryTracker.UntrackChildrenArray(managedHandle, ptr);
				var previousChildren = Marshal.PtrToStructure<ChildrenStruct>(ptr);
				if (previousChildren.children != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(previousChildren.children);
				}
				Marshal.FreeHGlobal(ptr);
			}

			// Handle null or empty children list
			if (children == null || children.Count == 0)
			{
				ptr = IntPtr.Zero;
				return 0;
			}

			// Prepare all children for sending and collect their pointers
			var pointers = new IntPtr[children.Count];
			for (int i = 0; i < children.Count; i++)
			{
				children[i]?.PrepareForSending();
				pointers[i] = children[i];
			}

			// Allocate unmanaged memory for the child pointer array
			int size = IntPtr.Size * children.Count;
			var itemsPtr = Marshal.AllocHGlobal(size);

			// Copy the child pointers to unmanaged memory
			Marshal.Copy(pointers, 0, itemsPtr, children.Count);

			var childrenStruct = new ChildrenStruct
			{
				children = itemsPtr,
				childrenLength = children.Count
			};

			ptr = Marshal.AllocHGlobal(Marshal.SizeOf<ChildrenStruct>());
			Marshal.StructureToPtr(childrenStruct, ptr, false);

			// Track the wrapper struct pointer for cleanup.
			StructMemoryTracker.TrackChildrenArray(managedHandle, ptr);
			return children.Count;
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
		// Note: 'key' field was removed to match Dart FFI struct layout.
		// Widget keys should be handled at the Flutter framework level, not in FFI structs.
	}

	/// <summary>
	/// Base manager class for widget struct memory operations.
	/// All generated struct managers inherit from this class.
	/// </summary>
	public class WidgetStructManager : IDisposable
	{
		private GCHandle _managedHandle;
		private bool _disposed = false;

		public WidgetStructManager()
		{
			_managedHandle = GCHandle.Alloc(this);
		}

		public IntPtr ManagedHandle => (IntPtr)_managedHandle;

		// String helper methods for generated struct managers
		protected string GetString(IntPtr ptr) => ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);

		protected void SetString(ref IntPtr ptr, string value)
		{
			if (ptr != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(ptr);
			}
			ptr = value != null ? Marshal.StringToCoTaskMemUTF8(value) : IntPtr.Zero;
		}

		protected void SetIntPtr(ref IntPtr ptr, Flutter.Widget widget)
		{
			widget?.PrepareForSending();
			ptr = widget;
		}

		protected bool GetValue(byte value) => value == 1;
		protected void SetValue(ref byte pointer, bool value) => pointer = value ? (byte)1 : (byte)0;

		// Methods that work with FlutterObjectStruct
		public void SetWidgetType(ref FlutterObjectStruct structRef, string value)
		{
			structRef.WidgetType = value;
		}

		// Methods that work with WidgetStruct
		public void SetId(ref WidgetStruct structRef, string value)
		{
			structRef.Id = value;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (_managedHandle.IsAllocated)
			{
				_managedHandle.Free();
			}

			_disposed = true;
		}

		~WidgetStructManager()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}



	public class PinnedObject<T> : IDisposable
	{
		private T value;
		private GCHandle gcHandle;
		private bool disposeUnderlyingObject;

		public PinnedObject(T value, bool disposeUnderlyingObject = true)
		{
			FlutterSharpLogger.LogDebug("PinnedObject.ctor()");

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
			FlutterSharpLogger.LogDebug("PinnedObject.Dispose()");

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
				if (!HasValue)
					throw new InvalidOperationException("Nullable value does not have a value.");
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
			value.HasValue ? value.Value : default(T);

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
			FlutterSharpLogger.LogDebug("NativeArray<T>.ctor()");

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
			FlutterSharpLogger.LogDebug("NativeArray<T>.ctor()");

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
			FlutterSharpLogger.LogDebug("NativeArray<T>.Dispose()");

			if (array == null)
				return;

			Marshal.FreeHGlobal((IntPtr)array);

			length = 0;
			array = null;
		}
	}

	/// <summary>
	/// Represents a collection of child widgets for FFI marshalling.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ChildrenStruct
	{
		/// <summary>
		/// Pointer to array of widget pointers.
		/// </summary>
		public IntPtr children;

		/// <summary>
		/// Number of children in the array.
		/// </summary>
		public int childrenLength;
	}

	/// <summary>
	/// Registry for widget type discriminators used in FFI communication.
	/// </summary>
	public static class WidgetTypeRegistry
	{
		private static readonly Dictionary<Type, int> _typeToId = new();
		private static readonly Dictionary<int, Type> _idToType = new();
		private static int _nextId = 1;

		/// <summary>
		/// Registers a widget type and returns its unique ID.
		/// </summary>
		public static int Register<T>() where T : Widget
		{
			var type = typeof(T);
			if (_typeToId.TryGetValue(type, out var id))
				return id;

			id = _nextId++;
			_typeToId[type] = id;
			_idToType[id] = type;
			return id;
		}

		/// <summary>
		/// Gets the type ID for a widget type.
		/// </summary>
		public static int GetTypeId<T>() where T : Widget =>
			_typeToId.GetValueOrDefault(typeof(T), 0);

		/// <summary>
		/// Gets the type for a given ID.
		/// </summary>
		public static Type? GetType(int id) =>
			_idToType.GetValueOrDefault(id);
	}
}

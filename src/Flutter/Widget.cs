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
		protected WidgetStructManager? backingStructManager;
		protected bool disposed = false;

		// Track allocated children wrapper structs and their backing arrays for cleanup
		private List<ChildrenAllocation> _allocatedChildrenArrays = new List<ChildrenAllocation>();

		// Track registered callback IDs for cleanup on disposal
		private List<long> _registeredCallbackIds = new List<long>();

		// Track marshaled auxiliary structs (EdgeInsets, BoxConstraints, Radius, etc.) for cleanup
		private List<IDisposable> _marshaledStructs = new List<IDisposable>();

		// Cache for the backing struct - stored as object to allow any struct type
		private object? _cachedBackingStruct;

		/// <summary>
		/// Unique identifier for this widget instance
		/// </summary>
		public string Id { get; } = Guid.NewGuid().ToString();

		private readonly struct ChildrenAllocation
		{
			public ChildrenAllocation(IntPtr structPointer, IntPtr itemsPointer)
			{
				StructPointer = structPointer;
				ItemsPointer = itemsPointer;
			}

			public IntPtr StructPointer { get; }
			public IntPtr ItemsPointer { get; }
		}

		/// <summary>
		/// Protected constructor that tracks widget creation for memory diagnostics.
		/// </summary>
		protected Widget()
		{
			MemoryDiagnostics.TrackWidgetCreation(this);
		}

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
			ptr = widget.GetHandle();
		}

		/// <summary>
		/// Gets the IntPtr handle for a widget, preparing it for sending if needed.
		/// </summary>
		protected IntPtr GetWidgetHandle(Widget? widget)
		{
			if (widget == null)
				return IntPtr.Zero;

			widget.PrepareForSending();
			return widget.GetHandle();
		}

		/// <summary>
		/// Tracks an auxiliary FFI struct for the lifetime of this widget and returns its handle.
		/// </summary>
		protected IntPtr TrackMarshaledStruct<T>(T marshaledStruct, string widgetType)
			where T : FlutterObjectStruct
		{
			marshaledStruct.WidgetType = widgetType;
			if (marshaledStruct is WidgetStruct widgetStruct && string.IsNullOrEmpty(widgetStruct.Id))
			{
				widgetStruct.Id = $"{Id}:{_marshaledStructs.Count}";
			}

			_marshaledStructs.Add(marshaledStruct);
			return marshaledStruct.Handle;
		}

		/// <summary>
		/// Marshals an <see cref="EdgeInsets"/> value to an FFI struct pointer.
		/// </summary>
		protected IntPtr MarshalEdgeInsets(EdgeInsets value)
		{
			var marshaled = new EdgeInsetsStruct
			{
				left = value.Left,
				top = value.Top,
				right = value.Right,
				bottom = value.Bottom
			};

			return TrackMarshaledStruct(marshaled, nameof(EdgeInsets));
		}

		/// <summary>
		/// Marshals a nullable <see cref="EdgeInsets"/> value to an FFI struct pointer.
		/// </summary>
		protected IntPtr MarshalEdgeInsets(EdgeInsets? value) =>
			value.HasValue ? MarshalEdgeInsets(value.Value) : IntPtr.Zero;

		/// <summary>
		/// Marshals an <see cref="EdgeInsetsGeometry"/> value to an FFI struct pointer.
		/// Currently supports concrete edge values only.
		/// </summary>
		protected IntPtr MarshalEdgeInsetsGeometry(EdgeInsetsGeometry? value)
		{
			if (value == null)
				return IntPtr.Zero;

			return MarshalEdgeInsets(value.ToEdgeInsets());
		}

		/// <summary>
		/// Marshals a <see cref="BoxConstraints"/> value to an FFI struct pointer.
		/// </summary>
		protected IntPtr MarshalBoxConstraints(BoxConstraints? value)
		{
			if (value == null)
				return IntPtr.Zero;

			var marshaled = new BoxConstraintsStruct
			{
				minWidth = value.MinWidth,
				maxWidth = value.MaxWidth,
				minHeight = value.MinHeight,
				maxHeight = value.MaxHeight
			};

			return TrackMarshaledStruct(marshaled, nameof(BoxConstraints));
		}

		/// <summary>
		/// Marshals a <see cref="Radius"/> value to an FFI struct pointer.
		/// </summary>
		protected IntPtr MarshalRadius(Radius value)
		{
			var marshaled = new RadiusStruct
			{
				x = value.X,
				y = value.Y
			};

			return TrackMarshaledStruct(marshaled, nameof(Radius));
		}

		/// <summary>
		/// Marshals a nullable <see cref="Radius"/> value to an FFI struct pointer.
		/// </summary>
		protected IntPtr MarshalRadius(Radius? value) =>
			value.HasValue ? MarshalRadius(value.Value) : IntPtr.Zero;

		/// <summary>
		/// Marshals an <see cref="Offset"/> value to an FFI struct pointer.
		/// </summary>
		protected IntPtr MarshalOffset(Offset value)
		{
			var marshaled = new OffsetStruct
			{
				dx = value.Dx,
				dy = value.Dy
			};

			return TrackMarshaledStruct(marshaled, nameof(Offset));
		}

		/// <summary>
		/// Marshals a nullable <see cref="Offset"/> value to an FFI struct pointer.
		/// </summary>
		protected IntPtr MarshalOffset(Offset? value) =>
			value.HasValue ? MarshalOffset(value.Value) : IntPtr.Zero;

		/// <summary>
		/// Sets a string field in the struct by allocating unmanaged UTF-8 memory.
		/// </summary>
		/// <param name="currentPtr">The current IntPtr value (will be freed if non-zero)</param>
		/// <param name="value">The string value to set</param>
		/// <returns>The new IntPtr pointing to the string</returns>
		protected IntPtr SetString(IntPtr currentPtr, string? value)
		{
			// Free previous string if allocated
			if (currentPtr != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(currentPtr);
			}

			// Allocate new string
			if (value != null)
			{
				return Marshal.StringToCoTaskMemUTF8(value);
			}

			return IntPtr.Zero;
		}

		/// <summary>
		/// Registers a callback with the CallbackRegistry and tracks the ID for cleanup on disposal.
		/// Returns the action ID string (e.g., "action_123") to be stored in the struct.
		/// </summary>
		/// <param name="callback">The callback delegate to register</param>
		/// <returns>The action ID string, or null if callback is null</returns>
		protected string? RegisterCallback(Delegate? callback)
		{
			if (callback == null)
				return null;

			var actionId = CallbackRegistry.Register(callback);
			_registeredCallbackIds.Add(actionId);
			MemoryDiagnostics.TrackCallbackRegistration();
			return $"action_{actionId}";
		}

		/// <summary>
		/// Converts a list of child widgets to a ChildrenStruct wrapper and returns its pointer.
		/// The wrapper and backing array are tracked and freed on Dispose.
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
					pointers[i] = children[i].GetHandle();
				}
				else
				{
					pointers[i] = IntPtr.Zero;
				}
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

			var structPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ChildrenStruct>());
			Marshal.StructureToPtr(childrenStruct, structPtr, false);

			// Track both allocations for cleanup
			_allocatedChildrenArrays.Add(new ChildrenAllocation(structPtr, itemsPtr));

			return structPtr;
		}

		/// <summary>
		/// Prepares the widget for sending to Flutter
		/// </summary>
		internal virtual void PrepareForSending()
		{
			// Override in derived classes to prepare their backing structs
		}

		/// <summary>
		/// Creates the backing struct for this widget type.
		/// Must be overridden in derived classes to return their specific struct type.
		/// </summary>
		protected abstract FlutterObjectStruct CreateBackingStruct();

		/// <summary>
		/// Gets the backing struct for this widget, creating it if necessary.
		/// The struct is cached to allow modifications to persist.
		/// Automatically sets WidgetType and Id when the struct is first created.
		/// </summary>
		/// <typeparam name="T">The specific struct type (e.g., ContainerStruct), must inherit from FlutterObjectStruct</typeparam>
		/// <returns>The backing struct cast to the specified type</returns>
		protected T GetBackingStruct<T>() where T : FlutterObjectStruct
		{
			// Create backing struct if not already created
			if (_cachedBackingStruct == null)
			{
				_cachedBackingStruct = CreateBackingStruct();

				// Automatically set WidgetType and Id on the backing struct
				var flutterStruct = (FlutterObjectStruct)_cachedBackingStruct;
				flutterStruct.WidgetType = GetType().Name;

				// Set Id if this is a WidgetStruct (or derived type)
				if (flutterStruct is WidgetStruct widgetStruct)
				{
					widgetStruct.Id = Id;
				}

				// Create the struct manager if needed
				backingStructManager ??= new WidgetStructManager();
			}

			// Cast the cached struct to the expected type
			return (T)_cachedBackingStruct!;
		}

		/// <summary>
		/// Gets the handle for FFI operations.
		/// Returns the Handle property from the backing struct.
		/// </summary>
		protected virtual IntPtr GetHandle()
		{
			// Get the handle from the backing struct if it exists
			if (_cachedBackingStruct is FlutterObjectStruct flutterStruct)
			{
				return flutterStruct.Handle;
			}
			return IntPtr.Zero;
		}

		/// <summary>
		/// Implicit conversion to IntPtr for FFI
		/// </summary>
		public static implicit operator IntPtr(Widget? widget)
		{
			if (widget == null)
				return IntPtr.Zero;

			widget.PrepareForSending();
			return widget.GetHandle();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				// Track disposal for memory diagnostics
				MemoryDiagnostics.TrackWidgetDisposal(this);

				// Free all allocated children wrapper structs and their backing arrays
				foreach (var allocation in _allocatedChildrenArrays)
				{
					if (allocation.ItemsPointer != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(allocation.ItemsPointer);
					}

					if (allocation.StructPointer != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(allocation.StructPointer);
					}
				}
				_allocatedChildrenArrays.Clear();

				// Unregister all callbacks to prevent memory leaks
				foreach (var callbackId in _registeredCallbackIds)
				{
					CallbackRegistry.Unregister(callbackId);
					MemoryDiagnostics.TrackCallbackUnregistration();
				}
				_registeredCallbackIds.Clear();

				foreach (var marshaledStruct in _marshaledStructs)
				{
					marshaledStruct.Dispose();
				}
				_marshaledStructs.Clear();

				if (disposing)
				{
					backingStructManager?.Dispose();
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

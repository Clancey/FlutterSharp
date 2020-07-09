using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Flutter {
	public static class FlutterPi {
		public static int Init () => FlutterBridge.runMono ();
		public static Task<int> RunApp () => Task.Run (FlutterBridge.runMonoLoop);
	}
	public unsafe class FlutterMethodChannel : IDisposable {
		static Dictionary<string, FlutterMethodChannel> Channels = new Dictionary<string, FlutterMethodChannel> ();
		static FlutterMethodChannel Register (FlutterMethodChannel channel)
		{
			//TODO: Send Subscription to native
			Channels [channel.channel] = channel;
			return channel;
		}

		static void UnRegister (FlutterMethodChannel channel)
		{
			//TODO: Send Removal to native
			Channels.Remove (channel.channel);
		}
		private readonly string channel;



		public delegate void FlutterMessage (string channel, string method, string message, FlutterResult result);
		public static FlutterMethodChannel Create (string channel)
		{
			if (!Channels.TryGetValue (channel, out var value))
				value = Register (new FlutterMethodChannel (channel));
			return value;
		}
		protected FlutterMethodChannel (string channel)
		{
			this.channel = channel;
			Channels [channel] = this;
			Console.WriteLine ($"Creating Chanel: {channel}");
			FlutterBridge.SetReceiver (channel, OnNativeCallback);
		}
		unsafe void OnNativeCallback (string channel, ref PlatchObj obj, IntPtr callbackHandle)
		{
			var completion = new FlutterResult (channel, callbackHandle);
			callback?.Invoke (channel, obj.Method, obj.Args.Value, completion);
			completion?.RunIfNotComplete ();
		}

		public void Dispose ()
		{
			UnRegister (this);
		}
		FlutterMessage callback;
		public void SetMethodCaller (FlutterMessage callback)
		{
			this.callback = callback;
		}

		public void InvokeMethod (string method, string argument)
		{
			Console.WriteLine ($"Sending Invoke: {method} : {argument}");
			var value = new StandardValue { Type = std_value_type.kStdString, Value = argument };
			FlutterBridge.InvokeMethod (channel, method, ref value, IntPtr.Zero, null);
			Console.WriteLine ($"Finished Invoke: {method} : {argument}");
		}
		int OnMessageRecieved (ref PlatchObj obj, IntPtr userdata)
		{
			return 0;
		}

	}

	[StructLayout (LayoutKind.Sequential)]
	struct FlutterMethodCallData {

	}
	public class FlutterMethodCall {

	}
	public class FlutterResult {
		private readonly string channel;
		private readonly IntPtr handle;

		internal FlutterResult (string channel, IntPtr handle)
		{
			this.channel = channel;
			this.handle = handle;
		}

		bool complete = false;
		public void Complete (string result = "")
		{
			complete = true;
			var resp = new StandardValue {
				Type = std_value_type.kStdString,
				Value = result,
			};
			FlutterBridge.SendResponse (handle, ref resp);
		}
		internal void RunIfNotComplete ()
		{
			if (!complete)
				Complete ();
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	internal unsafe struct PlatchObj {
		public platch_codec ValueType;
		//There is a union which causes a buffer
		IntPtr foo;
		public IntPtr method;
		//There is a union which causes a buffer
		IntPtr foo2;
		public StandardValue Args;

		public string Method => Marshal.PtrToStringAuto (method);
	}

	[StructLayout (LayoutKind.Sequential)]
	struct StandardValue {

		public std_value_type Type;
		//There is a union which causes a buffer
		IntPtr foo;
		IntPtr value;
		public string Value {
			get => Marshal.PtrToStringAuto (value);
			set => this.value = Marshal.StringToCoTaskMemUTF8 (value);
		}
	}

	internal unsafe static class FlutterBridge {
		[DllImport (dllImport, EntryPoint = "runMono")]
		public static extern int runMono ();
		[DllImport (dllImport, EntryPoint = "runMonoLoop")]
		public static extern int runMonoLoop ();

		const string dllImport = "Flutter";
		//typedef int (* platch_obj_recv_callback) (char* channel, struct platch_obj * object, FlutterPlatformMessageResponseHandle* responsehandle);
		public delegate void MethodChannelCallback (string channel, ref PlatchObj obj, IntPtr callbackHandle);
		public delegate int MEssageCallback (ref PlatchObj obj, IntPtr userData);
		[DllImport (dllImport, EntryPoint = "plugin_registry_set_receiver", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SetReceiver (string channel, platch_codec codec, MethodChannelCallback callback);
		public static int SetReceiver (string channel, MethodChannelCallback callback)
			=> SetReceiver (channel, platch_codec.kStandardMethodCall, callback);

		[DllImport (dllImport, EntryPoint = "platch_respond_success_std", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SendResponse (IntPtr callbackHandle, ref StandardValue value);
		//platch_send_success_event_std
		[DllImport (dllImport, EntryPoint = "platch_send_success_event_std", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SendMessage (string channel, ref StandardValue value);
		[DllImport (dllImport, EntryPoint = "platch_call_std", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern int InvokeMethod (string channel, string method, ref StandardValue value, IntPtr on_resp, void* userdata);


	}

	enum platch_codec {
		kNotImplemented,
		kStringCodec,
		kBinaryCodec,
		kJSONMessageCodec,
		kStandardMessageCodec,
		kStandardMethodCall,
		kStandardMethodCallResponse,
		kJSONMethodCall,
		kJSONMethodCallResponse
	};
	enum std_value_type {
		kStdNull = 0,
		kStdTrue,
		kStdFalse,
		kStdInt32,
		kStdInt64,
		kStdLargeInt, // treat as kString
		kStdFloat64,
		kStdString,
		kStdUInt8Array,
		kStdInt32Array,
		kStdInt64Array,
		kStdFloat64Array,
		kStdList,
		kStdMap
	};
}

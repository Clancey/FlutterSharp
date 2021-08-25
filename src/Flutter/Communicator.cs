using System;
namespace Flutter.Internal {
	public static class Communicator {
		public static Action<(string Method, string Arguments, Action<string> callback)> OnCommandReceived { get; set; }
		public static Action<(string Method,string Arguments)> SendCommand { get; set; }

		internal static void SendDisposed(string id)
		{

		}
	}
}

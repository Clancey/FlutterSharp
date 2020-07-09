using System;
using System.Threading.Tasks;
using Flutter;
using FlutterTest;

namespace FlutterCore {
	class Program {
		static async Task Main (string [] args)
		{
			Console.WriteLine ("Initializing");
			var app = new FlutterApp ();

			///Pass the widget to app.Run
			await app.Run (new FlutterSample());
		}
	}
}

# FlutterSharp
 
Flutter sharp is an Proof of concept doing a .net -> Dart binding. It is purely experimental.


# How it works
Flutter messaging built into the engine to talk to native frameworks.  We use this by sending messages containing pointers between dart and .net

On the .net side we create objects and pin them to memory. This allows us to load them as structs directly on the dart/flutter side. This allows both runtimes to read/write from shared memory!

# Building

* Make sure you have Flutter and the .NET iOS workload installed.
* Run `./build.sh` to generate the Flutter iOS frameworks used by the samples.
* Build the repo with `Flutter.sln`, or build the samples directly with:
  * `dotnet build Sample/FlutterSample/FlutterSample.csproj -f net10.0-ios -p:RuntimeIdentifier=iossimulator-arm64`
  * `dotnet build Sample/FlutterSample.MAUI/FlutterSample.MAUI.csproj -f net10.0-ios -p:RuntimeIdentifier=iossimulator-arm64`
* To launch the iOS sample on a booted simulator:
  * `xcrun simctl install booted Sample/FlutterSample/bin/Debug/net10.0-ios/iossimulator-arm64/FlutterSample.app`
  * `xcrun simctl launch booted com.fluttersharp.sample`

# Disclaimer

FlutterSharp is a **proof of concept**. There is **no** official support. Use at your own Risk.

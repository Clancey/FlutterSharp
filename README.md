# FlutterSharp
 
Flutter sharp is an Proof of concept doing a .net -> Dart binding. It is purely experimental.


# How it works
Flutter messaging built into the engine to talk to native frameworks.  We use this by sending messages containing pointers between dart and .net

On the .net side we create objects and pin them to memory. This allows us to load them as structs directly on the dart/flutter side. This allows both runtimes to read/write from shared memory!

# Building

* Make sure you have Flutter installed.
* Run the `build.sh`
* Open  `Flutter.sln`
* And build from VS

# Disclaimer

FlutterSharp is a **proof of concept**. There is **no** official support. Use at your own Risk.

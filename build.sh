#!/bin/bash

flutterCommand=`which flutter`
flutterBin=${flutterCommand%/flutter}
engineVersion="$flutterBin/internal/engine.version"
version=`cat $engineVersion`

echo "Using Flutter $version"
flutter_embedding_debug="https://storage.googleapis.com/download.flutter.io/io/flutter/flutter_embedding_debug/1.0.0-$version/flutter_embedding_debug-1.0.0-$version.jar"

cd flutter_sharp

# echo "Cleaning Flutter"
# flutter clean
echo "Building Flutter iOS frameworks"
# For a Flutter plugin, we need to build from the example app
if [ -d "example" ]; then
  cd example
  flutter build ios-framework --output=../build --no-profile
  cd ..
else
  # If no example, try building as aar for the plugin itself
  echo "No example app found, skipping iOS framework build"
fi

echo "Building Flutter Android AAR"
flutter build aar --no-profile --debug 2>&1 || echo "AAR build skipped (not a module)"

echo "Downloading Flutter embedding debug"
curl -o  build/flutter_embedding_debug.jar $flutter_embedding_debug

dotnet build ../src/Flutter.Bindings/Flutter.Bindings.csproj
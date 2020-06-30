#!/bin/bash
cd flutter_module
rm -rf .ios
flutter build ios-framework -v --output=build
flutter build aar

dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate --config /Users/clancey/Projects/SkiaSharp/binding/libSkiaSharp.json --skia /Users/clancey/Projects/FlutterBridge/flutter_sharp/ios  --output flutter..generated.cs  

dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate --config binding/libSkiaSharp.json --skia externals/skia --output binding/Binding/SkiaApi.generated.cs
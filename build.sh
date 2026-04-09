#!/bin/bash

set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$script_dir"

flutter_command="$(command -v flutter || true)"
if [ -z "$flutter_command" ]; then
  echo "flutter is required but was not found on PATH" >&2
  exit 1
fi

dotnet_command="$(command -v dotnet || true)"
if [ -z "$dotnet_command" ] && [ -x "/usr/local/share/dotnet/dotnet" ]; then
  dotnet_command="/usr/local/share/dotnet/dotnet"
fi
if [ -z "$dotnet_command" ]; then
  echo "dotnet is required but was not found on PATH" >&2
  exit 1
fi

flutter_bin="${flutter_command%/flutter}"
engine_version="$flutter_bin/internal/engine.version"
version="$(cat "$engine_version")"

echo "Using Flutter $version"
flutter_embedding_debug="https://storage.googleapis.com/download.flutter.io/io/flutter/flutter_embedding_debug/1.0.0-$version/flutter_embedding_debug-1.0.0-$version.jar"

if [ ! -d "$repo_root/flutter_module" ]; then
  echo "Expected flutter_module to exist before building iOS frameworks" >&2
  exit 1
fi

if [ ! -d "$repo_root/flutter_sharp/example" ]; then
  echo "Expected flutter_sharp/example to exist before building iOS frameworks" >&2
  exit 1
fi

echo "Restoring flutter_module dependencies"
pushd "$repo_root/flutter_module" >/dev/null
flutter pub get

echo "Building flutter_module iOS frameworks"
flutter build ios-framework --output=../flutter_sharp/build/iOS --no-profile < /dev/null
popd >/dev/null

wrapper_output_dir="$repo_root/flutter_sharp/build/wrapper_iOS"
rm -rf "$wrapper_output_dir"

echo "Restoring Flutter example dependencies"
pushd "$repo_root/flutter_sharp/example" >/dev/null
flutter pub get

echo "Building flutter_sharp wrapper frameworks"
flutter build ios-framework --output="$wrapper_output_dir" --no-debug < /dev/null
popd >/dev/null

copy_wrapper_frameworks() {
  local source_configuration="$1"
  local destination_configuration="$2"
  local destination_dir="$repo_root/flutter_sharp/build/iOS/$destination_configuration"
  local source_dir="$wrapper_output_dir/$source_configuration"

  for framework in flutter_sharp; do
    rm -rf "$destination_dir/$framework.xcframework"
    cp -R "$source_dir/$framework.xcframework" "$destination_dir/$framework.xcframework"
  done
}

validate_frameworks() {
  local configuration="$1"
  local framework_dir="$repo_root/flutter_sharp/build/iOS/$configuration"

  for framework in flutter_sharp Flutter App; do
    if [ ! -d "$framework_dir/$framework.xcframework" ]; then
      echo "Missing iOS framework: $framework_dir/$framework.xcframework" >&2
      exit 1
    fi
  done
}

copy_wrapper_frameworks Profile Debug
copy_wrapper_frameworks Release Release
validate_frameworks Debug
validate_frameworks Release

echo "Skipping Android AAR build from flutter_sharp/ (build flutter_module separately if needed)"

echo "Downloading Flutter embedding debug"
mkdir -p "$repo_root/flutter_sharp/build"
curl --fail --location --output "$repo_root/flutter_sharp/build/flutter_embedding_debug.jar" "$flutter_embedding_debug"

"$dotnet_command" build "$repo_root/src/Flutter.Bindings/Flutter.Bindings.csproj"

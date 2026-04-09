#!/bin/bash
# FlutterSharp Code Generator Wrapper Script
# This script ensures the tool is built and runs properly with all dependencies

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_FILE="$SCRIPT_DIR/FlutterSharp.CodeGen.csproj"
OUTPUT_DIR="$SCRIPT_DIR/bin/Debug/net10.0"
DLL_PATH="$OUTPUT_DIR/FlutterSharp.CodeGen.dll"
RUNTIME_CONFIG="$OUTPUT_DIR/FlutterSharp.CodeGen.runtimeconfig.json"

# Function to check if build is needed
needs_build() {
    if [ ! -f "$DLL_PATH" ]; then
        return 0
    fi

    if [ ! -f "$RUNTIME_CONFIG" ]; then
        return 0
    fi

    # Check if any source files are newer than the DLL
    if [ -n "$(find "$SCRIPT_DIR" -name "*.cs" -newer "$DLL_PATH" 2>/dev/null)" ]; then
        return 0
    fi

    return 1
}

# Build if necessary
if needs_build; then
    echo "Building FlutterSharp.CodeGen..."
    dotnet build "$PROJECT_FILE" -c Debug --nologo -v quiet
    echo "Build complete."
fi

# Run the tool
exec dotnet exec "$DLL_PATH" "$@"

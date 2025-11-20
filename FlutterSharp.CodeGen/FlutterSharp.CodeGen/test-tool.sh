#!/bin/bash
# FlutterSharp Code Generator Test & Validation Script
# This script validates the build, runtime configuration, and tool functionality

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="$SCRIPT_DIR/bin/Debug/net10.0"
DLL_PATH="$OUTPUT_DIR/FlutterSharp.CodeGen.dll"
RUNTIME_CONFIG="$OUTPUT_DIR/FlutterSharp.CodeGen.runtimeconfig.json"
DEPS_JSON="$OUTPUT_DIR/FlutterSharp.CodeGen.deps.json"

echo "=================================="
echo "FlutterSharp.CodeGen Test Suite"
echo "=================================="
echo

# Test 1: Check .NET SDK
echo "[1/6] Checking .NET SDK..."
DOTNET_VERSION=$(dotnet --version)
echo "    ✓ .NET SDK version: $DOTNET_VERSION"
echo

# Test 2: Build the project
echo "[2/6] Building project..."
dotnet build "$SCRIPT_DIR/FlutterSharp.CodeGen.csproj" -c Debug --nologo
echo "    ✓ Build successful"
echo

# Test 3: Verify runtime configuration files
echo "[3/6] Verifying runtime configuration..."
if [ ! -f "$RUNTIME_CONFIG" ]; then
    echo "    ✗ ERROR: $RUNTIME_CONFIG not found"
    echo "    Attempting to create from template..."

    TEMPLATE="$SCRIPT_DIR/FlutterSharp.CodeGen.runtimeconfig.template.json"
    if [ -f "$TEMPLATE" ]; then
        cp "$TEMPLATE" "$RUNTIME_CONFIG"
        echo "    ✓ Runtime config created from template"
    else
        echo "    ✗ ERROR: Template not found"
        exit 1
    fi
else
    echo "    ✓ Runtime config exists: $(basename "$RUNTIME_CONFIG")"
fi

if [ ! -f "$DEPS_JSON" ]; then
    echo "    ✗ WARNING: $DEPS_JSON not found"
else
    echo "    ✓ Dependencies file exists: $(basename "$DEPS_JSON")"
fi

if [ ! -f "$DLL_PATH" ]; then
    echo "    ✗ ERROR: $DLL_PATH not found"
    exit 1
else
    echo "    ✓ Main assembly exists: $(basename "$DLL_PATH")"
fi
echo

# Test 4: Verify NuGet packages
echo "[4/6] Verifying NuGet packages..."
REQUIRED_PACKAGES=("Scriban.dll" "System.CommandLine.dll" "YamlDotNet.dll")
for pkg in "${REQUIRED_PACKAGES[@]}"; do
    if [ -f "$OUTPUT_DIR/$pkg" ]; then
        echo "    ✓ $pkg"
    else
        echo "    ✗ ERROR: $pkg not found"
        exit 1
    fi
done
echo

# Test 5: Test basic execution
echo "[5/6] Testing tool execution with --help..."
if dotnet exec "$DLL_PATH" --help > /dev/null 2>&1; then
    echo "    ✓ Tool executes successfully"
else
    echo "    ✗ ERROR: Tool failed to execute"
    exit 1
fi
echo

# Test 6: Run validate command
echo "[6/6] Running 'validate' command..."
echo "----------------------------------------"
dotnet exec "$DLL_PATH" validate
echo "----------------------------------------"
echo

# Summary
echo "=================================="
echo "All Tests Passed! ✓"
echo "=================================="
echo
echo "Tool is ready to use. Available commands:"
echo "  dotnet run -- validate"
echo "  dotnet run -- generate --source sdk"
echo "  dotnet run -- list-widgets"
echo
echo "Or use the wrapper script:"
echo "  ./fluttersharp-codegen.sh validate"
echo "  ./fluttersharp-codegen.sh generate --source sdk"
echo

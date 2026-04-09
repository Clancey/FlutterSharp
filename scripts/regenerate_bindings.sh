#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CODEGEN_PROJ="$ROOT_DIR/FlutterSharp.CodeGen/FlutterSharp.CodeGen/FlutterSharp.CodeGen.csproj"

SOURCE_TYPE="${SOURCE_TYPE:-sdk}"
OUTPUT_CSHARP="${OUTPUT_CSHARP:-$ROOT_DIR/Generated/CSharp}"
OUTPUT_DART="${OUTPUT_DART:-$ROOT_DIR/Generated/Dart}"

printf 'Regenerating FlutterSharp bindings\n'
printf 'Source: %s\n' "$SOURCE_TYPE"
printf 'C# output: %s\n' "$OUTPUT_CSHARP"
printf 'Dart output: %s\n' "$OUTPUT_DART"

# Keep generation deterministic against checked-in dependency versions.
dotnet run --project "$CODEGEN_PROJ" -- generate \
  --source "$SOURCE_TYPE" \
  --output-csharp "$OUTPUT_CSHARP" \
  --output-dart "$OUTPUT_DART"

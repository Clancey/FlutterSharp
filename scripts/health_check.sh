#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CODEGEN_PROJ="$ROOT_DIR/FlutterSharp.CodeGen/FlutterSharp.CodeGen/FlutterSharp.CodeGen.csproj"
FLUTTER_PROJ="$ROOT_DIR/src/Flutter/Flutter.csproj"

OUTPUT_CSHARP="${OUTPUT_CSHARP:-$ROOT_DIR/Generated/CSharp}"
OUTPUT_DART="${OUTPUT_DART:-$ROOT_DIR/Generated/Dart}"
SOURCE_TYPE="${SOURCE_TYPE:-sdk}"
DART_ANALYZE_BASELINE="${DART_ANALYZE_BASELINE:-$ROOT_DIR/flutter_module/dart_analyzer_baseline.json}"
DART_ENFORCE_WARNINGS="${DART_ENFORCE_WARNINGS:-0}"
OWNERSHIP_SCOPE="${OWNERSHIP_SCOPE:-workspace}"

run_step() {
  local label="$1"
  shift
  printf '\n[%s] %s\n' "$(date +"%H:%M:%S")" "$label"
  "$@"
}

printf 'FlutterSharp Health Check\n'
printf 'Root: %s\n' "$ROOT_DIR"
printf 'CodeGen output C#: %s\n' "$OUTPUT_CSHARP"
printf 'CodeGen output Dart: %s\n' "$OUTPUT_DART"

run_step "Build code generator" \
  dotnet build "$CODEGEN_PROJ"

run_step "Validate code generator prerequisites" \
  dotnet run --project "$CODEGEN_PROJ" -- validate

run_step "Regenerate bindings" \
  dotnet run --project "$CODEGEN_PROJ" -- generate \
    --source "$SOURCE_TYPE" \
    --output-csharp "$OUTPUT_CSHARP" \
    --output-dart "$OUTPUT_DART"

run_step "Build Flutter C# project" \
  dotnet build "$FLUTTER_PROJ"

run_step "Run .NET tests" \
  dotnet test "$ROOT_DIR/Flutter.sln" --no-build

run_step "Audit generated ownership thresholds" \
  bash "$ROOT_DIR/scripts/audit_generated_ownership.sh" --enforce --scope="$OWNERSHIP_SCOPE"

run_step "Analyze Dart project (error gate + warning baseline)" \
  bash -lc "
    set -euo pipefail
    OUT_FILE=\$(mktemp)
    cd '$ROOT_DIR/flutter_module'
    set +e
    dart analyze --format machine >\"\$OUT_FILE\" 2>&1
    ANALYZE_EXIT=\$?
    set -e

    read -r ERROR_COUNT WARNING_COUNT INFO_COUNT < <(awk -F'|' '
      \$1 == \"ERROR\" { e++ }
      \$1 == \"WARNING\" { w++ }
      \$1 == \"INFO\" { i++ }
      END { print e + 0, w + 0, i + 0 }
    ' \"\$OUT_FILE\")

    echo \"Dart analyze summary: errors=\$ERROR_COUNT warnings=\$WARNING_COUNT info=\$INFO_COUNT (dart exit=\$ANALYZE_EXIT)\"

    if [[ \"\$ERROR_COUNT\" -gt 0 ]]; then
      echo \"Dart analysis failed: errors detected.\"
      sed -n '1,120p' \"\$OUT_FILE\"
      exit 1
    fi

    if [[ '$DART_ENFORCE_WARNINGS' == '1' && \"\$WARNING_COUNT\" -gt 0 ]]; then
      echo \"Dart analysis failed: warnings present and DART_ENFORCE_WARNINGS=1.\"
      exit 1
    fi

    if [[ -f '$DART_ANALYZE_BASELINE' ]]; then
      WARNING_MAX=\$(rg -o '\"warning_max\"\\s*:\\s*[0-9]+' '$DART_ANALYZE_BASELINE' | rg -o '[0-9]+' | head -n1 || true)
      INFO_MAX=\$(rg -o '\"info_max\"\\s*:\\s*[0-9]+' '$DART_ANALYZE_BASELINE' | rg -o '[0-9]+' | head -n1 || true)

      if [[ -n \"\$WARNING_MAX\" && \"\$WARNING_COUNT\" -gt \"\$WARNING_MAX\" ]]; then
        echo \"Dart analysis failed: warnings regressed above baseline (\$WARNING_COUNT > \$WARNING_MAX).\"
        exit 1
      fi

      if [[ -n \"\$INFO_MAX\" && \"\$INFO_COUNT\" -gt \"\$INFO_MAX\" ]]; then
        echo \"Dart analysis failed: info diagnostics regressed above baseline (\$INFO_COUNT > \$INFO_MAX).\"
        exit 1
      fi
    fi
  "

printf '\nHealth check completed successfully.\n'

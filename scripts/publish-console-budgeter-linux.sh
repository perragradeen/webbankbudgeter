#!/usr/bin/env bash
# Publicerar ConsoleBudgeter som självmantlad linux-x64 (ingen .NET-installation på målmaskinen).
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"
dotnet publish ConsoleBudgeter/ConsoleBudgeter.csproj \
  -c Release \
  -p:PublishProfile=Linux-x64-SelfContained
echo "Körbar fil: $ROOT/artifacts/ConsoleBudgeter/linux-x64/ConsoleBudgeter"

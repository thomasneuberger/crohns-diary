#!/usr/bin/env bash
# scripts/smoke.sh — local smoke-check helper for crohns-diary
# Runs restore/build/test, then starts the app and prints the manual browser checklist.
# Usage: bash scripts/smoke.sh
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

echo "==> dotnet restore"
dotnet restore

echo "==> dotnet build"
dotnet build --no-restore

echo "==> dotnet test"
dotnet test --no-build --verbosity normal

echo ""
echo "==> Starting CrohnsDiary.App (Ctrl+C to stop)"
echo "    Once the app is running, open the URL shown below and verify:"
echo "      1. Home page loads without errors."
echo "      2. Main navigation renders and core pages open."
echo "      3. No obvious startup or browser console errors."
echo ""
dotnet run --project CrohnsDiary.App

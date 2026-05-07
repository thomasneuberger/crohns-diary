# scripts/smoke.ps1 — local smoke-check helper for crohns-diary
# Runs restore/build/test, then starts the app and prints the manual browser checklist.
# Usage: pwsh scripts/smoke.ps1  (or: .\scripts\smoke.ps1 in a PowerShell prompt)
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot

try {
    Write-Host "==> dotnet restore" -ForegroundColor Cyan
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed" }

    Write-Host "==> dotnet build" -ForegroundColor Cyan
    dotnet build --no-restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

    Write-Host "==> dotnet test" -ForegroundColor Cyan
    dotnet test --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) { throw "dotnet test failed" }

    Write-Host ""
    Write-Host "==> Starting CrohnsDiary.App (Ctrl+C to stop)" -ForegroundColor Cyan
    Write-Host "    Once the app is running, open the URL shown below and verify:"
    Write-Host "      1. Home page loads without errors."
    Write-Host "      2. Main navigation renders and core pages open."
    Write-Host "      3. No obvious startup or browser console errors."
    Write-Host ""
    dotnet run --project CrohnsDiary.App
}
finally {
    Pop-Location
}

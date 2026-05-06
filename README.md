# crohns-diary

A Blazor WebAssembly app for tracking Crohn's disease symptoms.

## Local development

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
dotnet run --project CrohnsDiary.App
```

The `scripts/` directory contains helper scripts that run restore/build/test and start the app:

- **`scripts/smoke.sh`** — Linux / macOS (`bash scripts/smoke.sh`)
- **`scripts/smoke.ps1`** — Windows / PowerShell (`pwsh scripts/smoke.ps1`)
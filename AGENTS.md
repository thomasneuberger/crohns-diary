# Crohn's Diary Agent Instructions

## Project Overview
- `CrohnsDiary.sln` contains a .NET 8 solution for a Crohn's disease tracking app.
- `CrohnsDiary.App/` is the main Blazor application.
- `CrohnsDiary.Deploy/` contains Pulumi-based deployment infrastructure.
- UI pages and components are in `CrohnsDiary.App/Pages/` and `CrohnsDiary.App/Layout/`.
- Domain models are in `CrohnsDiary.App/Models/`.
- Local persistence/database abstractions are in `CrohnsDiary.App/Database/`.

## Working Conventions For AI Agents
- Prefer small, focused changes that match existing C# and Razor conventions.
- Preserve localization patterns (`*.resx`, `*.de.resx`) when changing user-facing text.
- Keep changes compatible with .NET 8 and current solution structure.

## Pull Request Requirement
- For every pull request, the application must be built, tested, started, and smoke-checked in the browser before merge.
- Run these commands from the repository root:
  - `dotnet restore`
  - `dotnet build --no-restore`
  - `dotnet test --no-build --verbosity normal`
- Start the application from the repository root:
  - `dotnet run --project CrohnsDiary.App`
- Perform a basic UI check after startup to confirm the app still works:
  - Verify the home page loads without errors.
  - Verify the main navigation renders and core pages open.
  - Verify there are no obvious startup or browser errors blocking normal use.

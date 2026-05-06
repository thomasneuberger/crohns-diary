# Copilot Instructions for crohns-diary

## Project Overview
- `CrohnsDiary.sln` — .NET 8 solution for a Crohn's disease tracking app.
- `CrohnsDiary.App/` — main Blazor WebAssembly application.
- `CrohnsDiary.Deploy/` — Pulumi-based Azure deployment infrastructure.
- UI pages and components: `CrohnsDiary.App/Pages/` and `CrohnsDiary.App/Layout/`.
- Domain models: `CrohnsDiary.App/Models/`.
- Local persistence/database abstractions: `CrohnsDiary.App/Database/`.

---

## No Docker / No Containers
There is **no Dockerfile** in this repository and nothing is planned to run in a container.  
Do **not** add Docker build steps, container instructions, or Docker-related tooling.

---

## Pull Request Requirements
Every PR that touches application code must be built, tested, started, and smoke-checked before merge.

### Required commands (run from the repository root)
```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

### Start the application
```bash
dotnet run --project CrohnsDiary.App
```

### Manual browser smoke check
After starting the app, confirm the following:
- [ ] Home page loads without errors.
- [ ] Main navigation renders and all core pages open.
- [ ] No obvious startup or browser console errors blocking normal use.

---

## Working Conventions

### Code style
- Prefer small, focused changes that match existing C# and Razor conventions.
- Keep changes compatible with .NET 8 and the current solution structure.
- Avoid drive-by refactors or formatting churn unless explicitly requested.

### Localization
- When adding or changing **any user-facing text**, update both the default resource file (`*.resx`) and the German translation (`*.de.resx`).
- Do not hard-code user-facing strings where the area already uses `IStringLocalizer`.

### Dependencies
- New dependencies must be **MIT licensed**.
- Prefer the **newest stable** version of any package.
- For fire-and-forget async patterns, use **AsyncAwaitBestPractices** (MIT) and follow its intended usage.
- Do not introduce packages with non-MIT or unknown licenses.

---

## Pulumi / Deployment
When touching `CrohnsDiary.Deploy/` or `.github/workflows/CD.yml`:
- Summarize which Azure resources are affected in the PR description.
- Ensure config/secrets patterns already in use are not violated.
- Call out any potentially destructive operations (replacements, deletions) explicitly in the PR.
- Verify that stack names, environment variables, and credentials referenced in `CD.yml` remain consistent with the Pulumi program.

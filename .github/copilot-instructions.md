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
Run the app in the background using `nohup` so it persists across separate shell calls:
```bash
nohup dotnet run --project CrohnsDiary.App --urls "http://localhost:5270" > /tmp/app.log 2>&1 &
echo "APP_PID=$!"
```

Then wait for startup and confirm it is listening before proceeding:
```bash
sleep 20 && cat /tmp/app.log && curl -s -o /dev/null -w "HTTP %{http_code}" http://localhost:5270
```
The app is ready when the log shows `Now listening on: http://localhost:5270` and the curl returns `HTTP 200`.

After the smoke check, stop the process to free the port:
```bash
kill $APP_PID 2>/dev/null || true
```

### Manual browser smoke check
After starting the app, confirm the following via `curl` and, if the Playwright MCP browser is available, via interactive browser checks:

**HTTP checks (always required):**
```bash
base="http://localhost:5270"
for path in "/" "/diary" "/settings" "/about"; do
  echo "$path -> $(curl -s -o /dev/null -w '%{http_code}' "$base$path")"
done
for asset in "/_framework/blazor.webassembly.js" "/_framework/blazor.boot.json" \
             "/_content/MudBlazor/MudBlazor.min.css" "/scripts/lib/dexie.min.js" \
             "/manifest.webmanifest" "/favicon.png"; do
  echo "$asset -> $(curl -s -o /dev/null -w '%{http_code}' "$base$asset")"
done
```
All paths and assets must return HTTP 200.

**Interactive browser checks (when Playwright MCP is available):**

> **Note:** The Playwright MCP browser is a shared resource. If you receive an error like _"use --isolated to run multiple instances of the same browser"_, the browser is already in use by another process and you cannot fix this from within the agent. In that case, skip the interactive checks and rely solely on the HTTP checks above — they are sufficient to confirm the app is serving correctly.

When the browser is free:
- [ ] Home page loads and renders without errors (use `browser_navigate` + `browser_snapshot`).
- [ ] Main navigation renders and all core pages open.
- [ ] No obvious startup or browser console errors blocking normal use.
- [ ] Key forms (e.g. diary entry) can be filled out and submitted successfully.

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

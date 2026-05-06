---
name: dotnet-pr-builder
description: >
  Ensures every PR that touches CrohnsDiary.App or solution-level files is built,
  tested, started, and smoke-checked before merge.
---

## Role
You are the **ship-it gate** for the crohns-diary repository.
Your job is to verify that every code change can be built, passes all tests, starts without errors, and survives a basic browser smoke check.

## Trigger
Activate when a PR modifies anything under `CrohnsDiary.App/`, `CrohnsDiary.sln`, or any `.csproj` file.

## Required steps (run from the repo root)

1. **Restore**
   ```bash
   dotnet restore
   ```
2. **Build**
   ```bash
   dotnet build --no-restore
   ```
3. **Test**
   ```bash
   dotnet test --no-build --verbosity normal
   ```
4. **Run**
   ```bash
   dotnet run --project CrohnsDiary.App
   ```
5. **Browser smoke check** — after the app starts, verify:
   - [ ] Home page loads without errors.
   - [ ] Main navigation renders and all core pages open.
   - [ ] No obvious startup or browser console errors.

## PR checklist items to add
Add the following checkboxes to every PR description when this agent is active.  
The Coding Agent **marks steps 1–4** after executing them. Steps 5–7 (browser smoke check) can be automated by the Coding Agent if the Playwright MCP server is configured (see below); otherwise they must be verified by a human reviewer.

```
- [ ] `dotnet restore` — completed without errors        ← Coding Agent
- [ ] `dotnet build --no-restore` — completed without errors  ← Coding Agent
- [ ] `dotnet test --no-build --verbosity normal` — all tests pass  ← Coding Agent
- [ ] `dotnet run --project CrohnsDiary.App` — app starts without errors  ← Coding Agent
- [ ] Home page loads without errors                     ← Coding Agent (Playwright MCP) or Human reviewer
- [ ] Main navigation renders and core pages open        ← Coding Agent (Playwright MCP) or Human reviewer
- [ ] No obvious startup or browser console errors       ← Coding Agent (Playwright MCP) or Human reviewer
```

## Automating the browser smoke check with the Playwright MCP server

When the **Playwright MCP server** (`@playwright/mcp`) is configured for this repository, the Coding Agent can open a real browser, navigate the app, and verify the smoke-check items automatically.

### How to enable it

A repository administrator must add the following JSON to **Settings → Copilot → Cloud agent → MCP configuration**:

```json
{
  "mcpServers": {
    "playwright": {
      "type": "local",
      "command": "npx",
      "args": ["@playwright/mcp@latest"],
      "tools": [
        "browser_navigate",
        "browser_snapshot",
        "browser_click",
        "browser_console_messages",
        "browser_wait_for"
      ]
    }
  }
}
```

### What the Coding Agent should do when Playwright MCP is available

After `dotnet run` starts the app (default port `5000` / `5001`), use the Playwright MCP tools to:

1. Navigate to `http://localhost:5000` and confirm the home page renders without errors.
2. Click through each item in the main navigation and confirm each page opens.
3. Check `browser_console_messages` for any errors blocking normal use.

## Notes
- There is **no Dockerfile** in this repository. Do not add Docker build steps.
- The app is a Blazor WebAssembly application targeting .NET 8.
- Use `scripts/smoke.sh` (Linux/macOS) or `scripts/smoke.ps1` (Windows) for convenience.

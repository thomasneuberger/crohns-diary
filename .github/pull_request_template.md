## Description
<!-- Briefly describe the change and the motivation behind it. -->

## Checklist

### Build & Test _(automated by Coding Agent)_
- [ ] `dotnet restore` — completed without errors
- [ ] `dotnet build --no-restore` — completed without errors
- [ ] `dotnet test --no-build --verbosity normal` — all tests pass

### Run & Smoke Check
- [ ] `dotnet run --project CrohnsDiary.App` — app starts without errors _(automated by Coding Agent)_
- [ ] Home page loads without errors _(Coding Agent with Playwright MCP, or manual — human reviewer)_
- [ ] Main navigation renders and core pages open _(Coding Agent with Playwright MCP, or manual — human reviewer)_
- [ ] No obvious startup or browser console errors _(Coding Agent with Playwright MCP, or manual — human reviewer)_

### Localization _(if user-facing text changed)_
- [ ] Default resource file (`*.resx`) updated
- [ ] German translation file (`*.de.resx`) updated

### Deployment / Infra _(if `CrohnsDiary.Deploy/` or `CD.yml` touched)_
- [ ] Affected Azure resources described above
- [ ] No unintended destructive operations (replacements/deletions)
- [ ] Stack names, env vars, and credentials in `CD.yml` remain consistent

### Dependencies _(if NuGet packages added or updated)_
- [ ] New packages are MIT licensed
- [ ] Using newest stable version

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

## Azure deployment (Static Web App)

Deployment now uses **Azure Static Web Apps**. Pulumi provisions the Static Web App resource and GitHub Actions uploads the published `wwwroot` content.

### Manual migration tasks (custom domain + free TLS)

When migrating an existing environment from Azure Storage + CDN:

1. Deploy once so the new Static Web App exists.
2. In Azure Portal, open the Static Web App and add the existing custom domain.
3. Update DNS to point to the Static Web App default hostname:
   - For subdomains, use a **CNAME** record.
   - For apex/root domains, use the DNS provider’s **ALIAS/ANAME** equivalent if supported.
4. Complete domain validation in the Static Web App.
5. Enable/confirm the **free managed certificate** for that custom domain.
6. After validation and HTTPS are healthy on the Static Web App, remove the old CDN domain binding.

> Tip: lower DNS TTL before cutover to reduce propagation time.

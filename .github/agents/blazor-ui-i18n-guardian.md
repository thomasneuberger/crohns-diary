---
name: blazor-ui-i18n-guardian
description: >
  Prevents AI regressions in Blazor UI components and enforces localization
  rules when user-facing text is added or changed.
---

## Role
You are the **Razor and localization guardian** for the crohns-diary repository.
Your job is to catch common Blazor pitfalls and ensure all user-visible text is properly localized.

## Trigger
Activate when a PR modifies `.razor` files, `.razor.cs` files, files under `CrohnsDiary.App/Pages/`, or `CrohnsDiary.App/Layout/`, or when user-facing strings are added or changed.

## Localization rules

1. **Always update both resource files** when adding or changing user-facing text:
   - Default resource file: `*.resx`
   - German translation: `*.de.resx`
2. **Do not hard-code user-facing strings** in areas that already use `IStringLocalizer`.
3. Ensure new resource keys are added in both files — missing keys cause silent fallback or errors at runtime.

## Blazor / MudBlazor checks

- **Nullable handling:** Use `int?`, `string?`, etc. where inputs can be empty or unset; avoid implicit default-value assumptions.
- **Async event handlers:** Use `async Task` (not `async void`) for Blazor event callbacks.
- **State updates:** Call `StateHasChanged()` when mutating state outside of Blazor's normal rendering cycle.
- **Validation:** Ensure `EditForm` validation is wired correctly; `OnValidSubmit` should not fire if the form is invalid.
- **MudBlazor bindings:** Double-check `@bind-Value` vs `Value` + `ValueChanged` pairs; prefer two-way binding (`@bind-Value`) where the component supports it.

## Fire-and-forget async

When a background task must be started without awaiting it, use **`SafeFireAndForget`** from the **AsyncAwaitBestPractices** package.

Rules:
- **An `onException` handler must always be provided.** Swallowing exceptions silently is not permitted.
- Prefer logging the exception at a minimum: `ex => logger.LogError(ex, "Background task failed")`.
- Do not use `async void` outside of Blazor lifecycle event handlers (e.g., `OnInitializedAsync`) where it is unavoidable.

```csharp
// ✅ Correct — exception handler is always supplied
DoWorkAsync().SafeFireAndForget(onException: ex => logger.LogError(ex, "Background task failed"));

// ❌ Wrong — silent discard, no exception handling
_ = DoWorkAsync();

// ❌ Wrong — no onException handler provided
DoWorkAsync().SafeFireAndForget();
```

## PR checklist item to add
Add this checkbox when any user-facing text changed:

```
- [ ] Default resource file (`*.resx`) updated
- [ ] German translation file (`*.de.resx`) updated
```

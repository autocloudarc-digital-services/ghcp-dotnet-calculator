---
name: refactor-calculator-blazor-app
description: "Refactor the current calculator workspace into a Blazor web app. Use when: refactor calculator to Blazor, build calculator.web, create shared calculator library, add calculator keypad, add Blazor history, add calculator keyboard support."
---

# Refactor Calculator Blazor App

Use this skill for the reusable implementation details behind prompt `3.01.1-refactor-calculator-blazor-app`.

## Scope

Refactor `src/workspace/calculator-xunit-testing/` from a console-focused calculator into a multi-project .NET workspace with shared calculator logic, preserved automated tests, and a Blazor web front end.

Use the active workspace's target framework. Do not rewrite intentional pre-upgrade .NET 8 references in historical prompts or docs while applying this skill to the current post-upgrade workspace.

Read every participating `.csproj` first. The SDK version is not the target
framework. If the intended .NET 10 upgrade was skipped, report that fact rather
than silently upgrading while performing this refactor.

## Project Shape

Target this structure when it is not already present:

```text
src/workspace/calculator-xunit-testing/
├── calculator.slnx
├── calculator/
│   └── calculator.csproj
├── calculator.library/
│   ├── calculator.library.csproj
│   └── CalculatorOperations.cs
├── calculator.tests/
│   ├── calculator.tests.csproj
│   ├── CalculatorTest.cs
│   └── TestCases.csv
└── calculator.web/
    ├── calculator.web.csproj
    ├── Program.cs
    ├── Components/
    ├── Services/
    └── wwwroot/
```

## Implementation Checklist

1. Create or verify `calculator.library` as a class library targeting the workspace's current target framework.
2. Move pure arithmetic operations into `calculator.library` without console I/O dependencies.
3. Update project references so `calculator`, `calculator.tests`, and `calculator.web` depend on `calculator.library`.
4. Create `calculator.web` with the current Blazor Web App template and server interactivity.
5. Register scoped Blazor services in `calculator.web/Program.cs`:
   * `CalculatorService`
   * `HistoryService`
   * `ThemeService`
6. Add `CalculationRecord` for completed calculation display and replay metadata.
7. Implement `CalculatorService` with display state, pending operand/operator state, chained calculations, divide-by-zero handling, and calculation events.
8. Implement `HistoryService` as session-only newest-first storage capped at 50 entries.
9. Implement `ThemeService` as scoped circuit state with light/dark CSS class generation.
10. Add Razor components:
    * `CalculatorKeypad.razor` for a stable 4-column grid
    * `HistoryPanel.razor` for newest-first history and replay
    * `ThemeToggle.razor` for light/dark mode
11. Replace the default home page with the calculator experience. In modern Blazor templates this is usually `Components/Pages/Home.razor` rather than older index-page assumptions.
12. Add JavaScript interop for global keyboard input under `calculator.web/wwwroot/js/`.
13. Replace template layout/navigation with a focused calculator shell and remove unused sample pages when safe.
14. Style the app with responsive CSS, stable keypad dimensions, focus-visible states, high-contrast operator buttons, and mobile layout support.

## Rendering And Circuit Reliability

* Pass current history entries and clear/replay callbacks to `HistoryPanel`.
  An injected mutable service plus an unchanged `EventCallback` does not
  guarantee child rerendering. Check immediate updates and replacement at the
  50-entry cap, not only the backing service's contents.
* Keep neutral numbers, amber operators, red Clear, and green Equals readable
  in both themes and hover states. Preserve accessible names when displaying
  conventional multiplication/division/minus glyphs.
* Distinguish typed calculator input from native Tab/Enter button activation;
  do not let a global keyboard handler activate the focused key twice.
* Catch expected `JSDisconnectedException` during interop disposal and always
  dispose .NET callback references. Do not hide unrelated JavaScript failures.
* On .NET 10, use `MapStaticAssets` and inspect published CSS/JS/framework
  requests. If CSS isolation is removed, move necessary error-banner styling
  into loaded global CSS and remove stale generated-stylesheet links. The
  connection-error banner must be hidden during normal operation.
* Explain that history/theme are circuit-local: transient transport recovery
  can preserve them, but a new circuit, refresh, or worker replacement cannot.

## Keyboard Map

Support these keys through JavaScript interop and component callbacks:

* Digits: `0` through `9`
* Decimal point: `.`
* Operators: `+`, `-`, `*`, `/`, `%`, `^`
* Calculate: `Enter` and `=`
* Clear: `Backspace`, `Delete`, `C`, and `c`

## Validation

Run validation from `src/workspace/calculator-xunit-testing/`:

```pwsh
dotnet build .\calculator.slnx
dotnet test .\calculator.slnx --verbosity minimal
```

For manual verification, start the app on an available local HTTP port:

```pwsh
dotnet run --project .\calculator.web\calculator.web.csproj --urls http://localhost:5297
```

Verify the app responds with HTTP 200 before reporting the URL.

Also use `validate-calculator-with-playwright` for actual interactivity.
HTTP 200 or a prerendered `0` is not proof that the Blazor circuit is attached.
Validate the published Release artifact as well as development execution;
check history, keyboard, themes, separate circuits, and 320-pixel layout.

## Boundaries

* Do not duplicate PostgreSQL seeding steps here. Use `convert-csv-test-data-to-postgresql-container` for persistent local database setup.
* Do not replace Testcontainers in automated tests with a fixed local database dependency.
* Do not hardcode credentials, ports, or expected test counts in implementation reports.
* Do not keep Blazor template sample pages or navigation once the calculator is the only user-facing route.

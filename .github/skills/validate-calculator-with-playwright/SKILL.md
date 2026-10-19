---
name: validate-calculator-with-playwright
description: "Validate local, published Release, or Azure Blazor calculator UI with Playwright, including circuit readiness, history, keyboard, themes, and actual WebSocket recovery. Use when: click 7 + 3 = and confirm 10, test calculator buttons, run calculator UI smoke test, validate published calculator, verify Azure calculator, troubleshoot localhost connection refused."
---

# Validate Calculator With Playwright

Use this skill to run a browser-level calculator smoke test against the Blazor
app. Reuse a running app when available, recover the local server when the URL
is unavailable, interact through accessible Playwright locators, and report the
observed display value.

## When To Use

Use this skill when the user asks to:

* Open the calculator with Playwright and click an expression
* Verify that `7 + 3 =` displays `10`
* Test calculator keypad behavior at `http://localhost:5000`
* Run a calculator UI smoke test
* Recover from `ERR_CONNECTION_REFUSED` before a Playwright calculator test

## Prerequisites

* Exposed Playwright MCP tools, or the bundled standalone runner below
* The .NET SDK required by `calculator.web` is available through `dotnet`
* The calculator web project exists under
  `src/workspace/calculator-xunit-testing/calculator.web/`
* The requested origin is available; port 5000 is only the default local URL

Configured MCP JSON is not proof that tools are exposed in the active session.
Use `configure-mcp-servers` for MCP setup/reload, or the standalone fallback.
Do not fabricate tool names or claim a browser check from HTTP-only results.

## Parameters Reference

| Parameter | Default | Description |
|-----------|---------|-------------|
| URL | `http://localhost:5000` | Calculator URL from the user request |
| Expression | `7 + 3 =` | Ordered keypad actions to perform |
| Expected display | `10` | Exact trimmed text expected in the status element |

Honor values supplied by the user instead of these defaults.

## Required Procedure

1. Try to open or reuse the requested URL with a browser tool.
2. If navigation succeeds, confirm the page title is `Calculator` and continue
   to the browser interaction. If another page occupies the requested port,
   report its title and stop without starting a second server.
3. If navigation fails with `ERR_CONNECTION_REFUSED`, locate the current web
   project only for an intended local origin. A cloud failure must be diagnosed
   against that cloud target, not "recovered" by launching a local substitute.
4. Prefer the project under `src/workspace/calculator-xunit-testing/`. Use a
   project under `src/completed/` only when the file actually exists.
5. Start the project in an asynchronous persistent terminal with the requested
   origin, and retain its terminal ID so the process can be monitored and
   stopped:

   ```powershell
   dotnet run --project src/workspace/calculator-xunit-testing/calculator.web/calculator.web.csproj --urls http://localhost:5000
   ```

6. Wait up to 120 seconds for the server to report
   `Now listening on: http://localhost:5000` before retrying navigation. Do not
   launch a second server while the first is still building. If the process
   exits or the deadline expires, report the captured terminal output and fail.
7. Reuse the active calculator page when possible. Reload or open a new page
   only when necessary.
8. Scope keypad actions to the `Current Calculation` region. Verify the Blazor
   circuit is interactive by clicking `Seven`, waiting for the status to show
   `7`, then clicking the scoped `Clear` button and waiting for status `0`.
   Retry this readiness probe within the same 120-second startup deadline if
   the first click occurs before interactivity attaches.
9. Clear prior input with the scoped `Clear` button, then click these accessible
   buttons in order for the default expression:

   * `Seven`
   * `Add`
   * `Three`
   * `Equals`

10. Read the element with the `status` role. Trim its text and compare it with
    the expected display value.
11. Treat any value other than the exact expected value as a failed test. Report
   the expected and observed values before cleanup.
12. On success, report the URL, expression, and confirmed display value.

## Repeatable Extended Runner

The [runner](./scripts/verify-calculator.cjs) persists the release/browser
regressions learned during the Azure exercise. It never starts a server or
changes Azure configuration. It creates isolated browser contexts, changes only
their circuit-local calculator state, and closes its browsers on failure too.

From the repository root:

```bash
npm ci --include=dev --prefix .github/skills/validate-calculator-with-playwright
npm exec --prefix .github/skills/validate-calculator-with-playwright -- playwright install chromium
npm run --prefix .github/skills/validate-calculator-with-playwright verify -- http://localhost:5000
```

Supply the actual local published-output or HTTPS Azure root URL instead of
the default when appropriate. The runner intentionally tests the standard
`7 + 3 = 10` contract; use the manual flow for custom expressions.
The lock file pins the standalone SDK. The devcontainer preflight prepares its
matching Chromium; on other Linux hosts install browser system dependencies
with the same `playwright install --with-deps chromium` command if needed.
Run `npm test --prefix .github/skills/validate-calculator-with-playwright` for
fast invalid-target/credential-redaction checks without starting a browser.

The extended checks cover immediate history updates, replay/clear, rendering
after the 50-entry cap, keyboard arithmetic versus native Tab/Enter activation,
theme, isolated clients, 320-pixel
layout, normal error-banner visibility, asset failures, and page exceptions.
They close a passthrough WebSocket and require a replacement connection plus
preserved circuit history/theme before claiming reconnect success. A browser
offline toggle alone may leave an established WebSocket alive. Full refresh
must instead create a new circuit with empty history and default theme.

For deployment readiness, publish Release to a separate staging directory,
run that exact output on an available validation port, and test it before
packaging. Keep the pre-existing developer server untouched.

## Playwright Interaction

Prefer role-based locators because the calculator exposes stable accessible
names. The equivalent Playwright flow is:

```javascript
const calculator = page.getByRole('region', { name: 'Current Calculation' });
await calculator.getByRole('button', { name: 'Seven', exact: true }).click();
await expect(page.getByRole('status')).toHaveText('7');
await calculator.getByRole('button', { name: 'Clear', exact: true }).click();
await expect(page.getByRole('status')).toHaveText('0');

await calculator.getByRole('button', { name: 'Clear', exact: true }).click();
await calculator.getByRole('button', { name: 'Seven', exact: true }).click();
await calculator.getByRole('button', { name: 'Add', exact: true }).click();
await calculator.getByRole('button', { name: 'Three', exact: true }).click();
await calculator.getByRole('button', { name: 'Equals', exact: true }).click();

const observedDisplay = (await page.getByRole('status').innerText()).trim();
if (observedDisplay !== '10') {
  throw new Error(`Expected display 10, observed ${observedDisplay}`);
}
```

Use one concise Playwright execution when the browser tool supports it. When
only individual click tools are available, follow the same locator order and
perform the display assertion as a separate final action.

## Validation Result

A successful result must include all of the following evidence:

* URL: the actual tested origin (`http://localhost:5000` by default)
* Expression: `7 + 3 =`
* Expected display: `10`
* Observed display: `10`
* Verdict: passed

Do not report success from the expected arithmetic alone. The observed value
must come from the rendered page after the clicks.

## Troubleshooting

### Connection Refused

Confirm the project path before starting `dotnet run`. The active exercise
project is under `src/workspace/`; a previously documented `src/completed/`
path might not exist.

### Page Opens Before The Build Finishes

Keep the original server process running and wait for its readiness output.
Retry browser navigation only after Kestrel reports that it is listening.

### Clear Selects The History Button

The page contains two buttons named `Clear`. Scope the keypad clear action to
the `Current Calculation` region.

### Display Assertion Cannot Find The Result

Use `getByRole('status')`. The calculator renders its display with an HTML
`output` element, which Playwright exposes with the `status` role.

### Chromium Revision Is Missing

MCP and the standalone SDK may require different Chromium revisions. Install
through the matching pinned package, not an unrelated global Playwright CLI.
Do not hardcode a cached revision path or silently pick the newest cached
executable; an existing cache directory alone is not a compatible installation.

### Published Assets Or History Are Stale

On .NET 10, verify `MapStaticAssets`, framework/CSS/JS requests, and global
connection-error banner styles. Verify history in the rendered child component:
an unchanged callback with an injected mutable service can skip its rerender.
Passing entries as state parameters fixes that boundary; unit tests of the
backing service alone cannot detect it.

## Cleanup

For a one-off validation request, stop only the terminal process started by
this workflow after reporting success or failure. Leave it running only when
the user explicitly requests continued manual access. Never stop a server that
was already running before this workflow began.

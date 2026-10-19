---
title: Agent Guidance for ghcp-dotnet-calculator
description: Repository-wide operating guidance for coding agents working on the calculator labs, customizations, tests, workflows, and Azure assets
---

## Repository Overview

This repository is a GitHub Copilot learning workspace starting with a .NET 8
calculator and upgrading to .NET 10 only at prompt `3.01`. Its later stage
combines a console app, a shared arithmetic library, an
interactive Blazor web app, xUnit tests, workshop exercises, Copilot
customizations, GitHub Actions examples, and Azure deployment assets.

Keep changes focused on the requested exercise or artifact. The repository is
instructional, so preserve deliberate intermediate states and historical .NET
8 references in stage-specific prompts, reports, and lab material unless the
task explicitly updates that stage.

## Repository Structure

* `src/workspace/calculator-xunit-testing/` contains the active .NET solution.
* `src/completed/` contains finished reference material. Do not use it as the
  target for active implementation unless the request explicitly names it.
* `lab-exercises/` contains learner-facing workshop instructions.
* `.github/prompts/`, `.github/agents/`, `.github/instructions/`, and
  `.github/skills/` contain GitHub Copilot customization assets.
* `.github/workflows/` contains numbered instructional workflow examples and
  deployment automation.
* `infra/` contains the Bicep deployment source, generated ARM JSON, and
  exercise-specific Terraform assets.
* `azure.yaml` maps the Blazor project to Azure App Service for Azure Developer
  CLI workflows.
* `docs/`, `artifacts/`, and `inputs/` contain supporting reports, images,
  generated deliverables, and exercise input data.

Ignore `bin/`, `obj/`, logs, and other generated output unless the task is
specifically about those artifacts.

## Prerequisites And Setup

Use the repository root as the working directory. The active solution requires:

* .NET 8 and .NET 10 SDKs, installed side by side in the Dev Container;
  use SDK 8 initially and upgrade through GitHub Copilot upgrade at `3.01`
* PowerShell 7 for repository scripts
* Docker Desktop or another compatible Docker daemon for Testcontainers tests
* Node.js LTS for MCP and `npx`-based exercises
* Git and VS Code for the intended workshop workflow

Confirm the local toolchain before changing the calculator:

The Dev Container provides both SDKs and runtimes. Initial setup creates
`calculator.sln`, `net8.0` projects, and a workspace-local `global.json`
selecting SDK 8. Run commands inside that workspace to apply the pin; the
repository root may select SDK 10. The `.slnx` commands below describe the
post-upgrade stage: before `3.01`, use `calculator.sln` instead. SDK 8 cannot
build `.slnx`. At `3.01`, follow the upgrade prompt to update the workspace
SDK pin to 10, retarget projects through the extension,
and migrate the solution format. Preserve any existing later-stage work;
do not downgrade it or run a reset without explicit approval.

```powershell
dotnet --info
docker info
dotnet restore src/workspace/calculator-xunit-testing/calculator.slnx
```

Do not use the `reset-calculator-lab` skill or run its bundled removal script
unless the user explicitly asks to reset the active exercise. Preview with
`-WhatIf` and preserve the solution under `src/completed` before removal.

## Build And Run

Restore and build the complete active solution from the repository root:

```powershell
dotnet restore src/workspace/calculator-xunit-testing/calculator.slnx
dotnet build src/workspace/calculator-xunit-testing/calculator.slnx --no-restore
```

Run the console app with:

```powershell
dotnet run --project src/workspace/calculator-xunit-testing/calculator/calculator.csproj
```

Run the Blazor app at the local smoke-test URL with:

```powershell
dotnet run --project src/workspace/calculator-xunit-testing/calculator.web/calculator.web.csproj --urls http://localhost:5000
```

The Blazor app exposes a health endpoint at `/health`. When starting a server
for automated validation, wait for Kestrel readiness and stop only the process
created for that validation.

## Testing

Run the full xUnit suite after changes to calculator logic, Blazor services, or
test data:

```powershell
dotnet test src/workspace/calculator-xunit-testing/calculator.slnx --no-build
```

The calculator tests use xUnit, `TestCases.csv`, Npgsql, and a PostgreSQL
Testcontainer. Docker must be running. Keep test credentials randomized at
runtime, seed test data from the copied CSV file, and do not replace isolated
automated containers with a shared local database.

Use test names in the form
`MethodName_Scenario_ExpectedBehavior`. Cover happy paths, zero-divisor errors,
state transitions, formatting, history, and theme behavior as appropriate.
For focused changes, run the narrow affected test first, then run the complete
solution suite before completion.

For browser-facing calculator changes, use the
`validate-calculator-with-playwright` skill. The baseline smoke test opens
`http://localhost:5000`, performs `7 + 3 =` through accessible button names,
and verifies that the rendered status value is exactly `10`.
For release/deployment readiness, exercise the actual published output as well.
Use changing component parameters for history rendering, preserve native
keyboard activation, and distinguish transport reconnect from a new circuit.

## Architecture And Boundaries

* Keep deterministic arithmetic in `calculator.library/CalculatorOperations.cs`.
  It must not depend on console, web, database, or UI concerns.
* Keep console input and output in the `calculator` project.
* Keep Blazor components, scoped UI state, formatting, history, and theme logic
  in `calculator.web`.
* Register Blazor services in `calculator.web/Program.cs` and preserve server
  interactivity, antiforgery, HTTPS redirection, exception handling, and the
  `/health` endpoint.
* Keep assertions and test infrastructure in `calculator.tests`.
* Add shared abstractions only when they reduce meaningful duplication or
  preserve these project boundaries.

Preserve the existing arithmetic contract for addition, subtraction,
multiplication, division, modulo, and exponentiation. Division and modulo by
zero must continue to throw `DivideByZeroException`.

## Coding And Documentation Conventions

Use nullable reference types and implicit usings as configured by the projects.
Follow Microsoft C# naming conventions: PascalCase for types and public members,
camelCase for parameters and local variables. Use descriptive names, focused
methods, early validation, and XML documentation on public members. Prefer
async APIs for I/O and dispose containers, connections, commands, readers, and
other resources deterministically.

All Markdown files require YAML frontmatter. Use one logical title, sequential
ATX headings, language-tagged code fences, ASCII punctuation, and a single
trailing newline. Preserve the repository's numbered naming conventions for
prompts, lab exercises, and workflows.

Before editing a path, apply the more specific instructions in
`.github/instructions/` and the repository-wide guidance in
`.github/copilot-instructions.md`. Reuse an existing prompt, skill, agent, or
helper before creating a parallel implementation.

## Security And Configuration

Never commit credentials, tokens, connection strings, private endpoints, or
real user data. Use environment variables, GitHub secrets, user secrets,
managed identities, or per-run randomized Testcontainers credentials as the
relevant workflow requires. Do not print sensitive values in logs, reports, or
test output.

Validate external input and avoid exposing exception details in user-facing
responses. Preserve antiforgery and production error handling in the Blazor
app. Use parameterized database commands, least-privilege permissions, and
explicit resource cleanup. Treat changes to workflows, hooks, MCP
configuration, and deployment files as security-sensitive changes.

## Automation And Deployment

The numbered files in `.github/workflows/` are teaching examples. Preserve the
concept each workflow demonstrates and do not assume every example is a
required production check. For workflow changes, use lowercase, hyphenated,
descriptive names, pin actions to trusted versions or commit SHAs, declare
minimal permissions, and keep secrets out of commands and artifacts.

`azure.yaml` deploys
`src/workspace/calculator-xunit-testing/calculator.web` to Azure App Service and
uses `infra/main.bicep` as the infrastructure source. Treat `infra/main.json`
as generated output when it corresponds to the Bicep template. Keep Terraform
changes scoped to the exercise directory that owns them. Run the relevant
Bicep, Terraform, or Azure Developer CLI validation before deployment, and do
not deploy or delete cloud resources without explicit user approval.

The default template is web-only; automated PostgreSQL fixtures do not justify
a runtime database or Key Vault. Ask for missing target/authentication choices
and explicit cost approval, then use `deploy-calculator-to-azure`. Keep prompt
`3.02` assessment-only and retain partial status when native tools are missing.
Reconcile CLI deployment errors with OneDeploy, startup diagnostics, HTTPS
health, and browser checks; never hide the error or broaden cloud scope.

## Change Workflow And Completion Criteria

Inspect the current file contents and working tree before editing because the
workspace may contain user changes. Do not revert, overwrite, or reformat
unrelated work. Make the smallest coherent change, preserve public behavior
unless the request changes it, and update tests and documentation when behavior
or commands change.

Before reporting completion:

1. Run the narrowest check that can falsify the change.
2. Run the relevant build, test, lint, or infrastructure validation.
3. Run the full calculator test suite when shared application behavior changed.
4. Review the final diff for secrets, generated noise, stale paths, and
   accidental edits.
5. Report the files changed, validation performed, and any remaining risk or
   unavailable prerequisite.

Do not create commits, branches, pull requests, deployments, or destructive
resets unless the user explicitly requests them.

## Cursor Cloud specific instructions

The Cloud VM snapshot has the .NET 10 SDK and Docker Engine preinstalled, and
the update script runs `dotnet restore` on startup. The notes below capture the
non-obvious caveats for this environment.

* Discover the live solution before choosing paths. Prefer
 `src/workspace/calculator-xunit-testing/` when present. Some snapshots have
 only the completed reference, but use `src/completed/` only when the task
 explicitly permits reference-solution work; otherwise use `calculator-setup`
 for the requested initial stage. Never overwrite an existing later stage.
* Docker startup and cached images vary by snapshot. Check `docker info`;
 do not assume the daemon is absent or start a duplicate. Use the approved
 devcontainer Docker setup/group permissions, not a world-writable Docker
 socket. If access fails on a shared host, ask the environment owner rather
 than changing daemon/socket permissions globally.
* The Blazor web app listens on `http://localhost:5000` with a `/health`
 endpoint; wait for `Healthy` before driving the UI. The baseline smoke test is
 `7 + 3 =`, which must render `10`.

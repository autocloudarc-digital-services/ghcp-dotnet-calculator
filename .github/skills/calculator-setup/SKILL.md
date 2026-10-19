---
name: calculator-setup
description: "Set up .NET 8 calculator solution structure with console app and xUnit test project. Use when: follow prompt 1.12.1 solution setup, create calculator solution, setup calculator, initialize calculator projects, prepare calculator workspace."
---

# Calculator Setup Skill

Use this skill to execute the prompt `1.12.1-solution-setup.prompt.md` as a
modular, repeatable workflow. It creates or verifies the .NET 8 calculator
solution under `src/workspace/calculator-xunit-testing/` with a console app,
an xUnit test project, compatible package versions, project references, and
starter source files.

## When To Use

Use this skill when the user asks to:

* Follow prompt `1.12.1-solution-setup.prompt.md`
* Create the calculator solution structure
* Initialize the calculator console and xUnit projects
* Recreate or verify the `src/workspace/calculator-xunit-testing/` workspace
* Prepare the calculator repo for later implementation prompts

## Bundled Assets

| Asset | Purpose |
|-------|---------|
| [scripts/Set-DotnetCalculatorSolution.ps1](./scripts/Set-DotnetCalculatorSolution.ps1) | Idempotent setup script for the .NET 8 solution and xUnit test project |
| [templates/Calculator.cs](./templates/Calculator.cs) | Starter console source template used when `Calculator.cs` is missing |
| [templates/CalculatorTest.cs](./templates/CalculatorTest.cs) | Starter xUnit test template used when `CalculatorTest.cs` is missing |
| [templates/global.json](./templates/global.json) | Workspace-local SDK 8 selection while SDKs 8 and 10 are installed |

## Prerequisites

* Git repository available so `git rev-parse --show-toplevel` works
* .NET 8 SDK available through `dotnet`; the Dev Container also installs SDK 10
* PowerShell 7 or later available through `pwsh`
* Write access to `src/workspace/`

The Dev Container installs both SDK 8 and SDK 10. Setup creates a workspace-local
`global.json` selecting a serviced SDK 8 feature band and explicitly creates
`net8.0` projects. Keep that baseline through the console, CSV, and PostgreSQL
exercises. Select SDK 10 and use GitHub Copilot upgrade only at prompt `3.01`.
The SDK pin is generated, not repository-wide, so the existing .NET 10
implementation remains usable until separately preserved or removed.
Do not rerun initial
setup over an already upgraded workspace to downgrade it.

## Required Procedure

1. Run the bundled setup script from the repository root:

   ```powershell
   pwsh .github/skills/calculator-setup/scripts/Set-DotnetCalculatorSolution.ps1
   ```

2. Confirm the script reports successful build and test discovery.
3. If the script fails, fix only setup-related issues from prompt 1.12.1.
4. Do not implement calculator operations, data-driven tests, CSV migration, or
   PostgreSQL-backed test execution as part of this skill.

## Script Behavior

The bundled script performs these actions idempotently:

* Detects the repository root with `git rev-parse --show-toplevel`
* Creates `src/workspace/calculator-xunit-testing/`
* Creates a workspace-local `global.json` selecting SDK 8 when missing
* Creates or verifies the .NET 8-compatible `.sln` solution file
* Creates `calculator` as a .NET 8 console project
* Creates `calculator.tests` as a .NET 8 xUnit project
* Pins required test package versions from prompt 1.12.1
* Sets `SuppressTfmSupportBuildErrors` to `true` in the test project
* Renames generated `Program.cs` and `UnitTest1.cs` when present
* Applies bundled source templates only when the target files are missing
* Adds both projects to the solution
* Adds the test-to-console project reference
* Runs build verification and test discovery

## Expected Structure

```text
src/workspace/calculator-xunit-testing/
├── calculator.sln
├── global.json
├── calculator/
│   ├── calculator.csproj
│   └── Calculator.cs
└── calculator.tests/
    ├── calculator.tests.csproj
    └── CalculatorTest.cs
```

## Required Configuration

The setup must preserve these prompt `1.12.1` constraints:

| Setting | Required value |
|---------|----------------|
| Console target framework | `net8.0` |
| Test target framework | `net8.0` |
| `SuppressTfmSupportBuildErrors` | `true` |
| `xunit` | `2.6.2` |
| `xunit.runner.visualstudio` | `2.5.1` |
| `Microsoft.NET.Test.Sdk` | `17.5.0` |
| `coverlet.collector` | `6.0.0` |

## Validation

After running the script, validate from the repository root:

```powershell
dotnet build src/workspace/calculator-xunit-testing/calculator.sln
dotnet test src/workspace/calculator-xunit-testing/calculator.sln --list-tests
```

New initial solutions use `.sln`, which the .NET 8 SDK supports. Existing
solution files are reused. Run `dotnet --version` from inside the generated
workspace to verify SDK 8 selection; the repository root may select SDK 10.
Prompt `3.01` updates the workspace SDK pin to 10 and migrates the solution
to `.slnx` for the later exercises.

## Troubleshooting

### `dotnet` is not found

Install the .NET SDK or run the script from a terminal where `dotnet` is on
`PATH`.

### Test discovery fails

Verify the test project still targets `net8.0`, sets
`SuppressTfmSupportBuildErrors` to `true`, and uses the package versions listed
in this skill.

### Existing source files are not overwritten

This is intentional. The script applies templates only when `Calculator.cs` or
`CalculatorTest.cs` is missing so later prompts can safely build on the setup.

## Next Steps

After this skill succeeds, continue with the implementation and testing prompts
in order, such as `1.12.2-calculator-implementation.prompt.md`,
`1.12.3-refactor-steps.prompt.md`, and
`1.12.4-testing-strategy.prompt.md`.

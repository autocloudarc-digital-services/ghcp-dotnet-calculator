---
name: calculator-xunit-testing
description: "Guidance for the active .NET calculator console, library, Blazor, and xUnit solution"
applyTo: "src/workspace/calculator-xunit-testing/**"
---

# Calculator xUnit Testing Instructions

The Calculator xUnit Testing project is a .NET 10 solution demonstrating shared calculator logic, a console app, a Blazor web app, and xUnit tests.

That inventory describes the post-upgrade stage. Initial setup uses SDK 8,
`net8.0` console/test projects, and `calculator.sln`. Keep that baseline through
CSV and PostgreSQL exercises. Both SDKs are installed in the Dev Container;
initial setup generates a workspace-local `global.json` selecting SDK 8.
At `3.01`, update that SDK pin to 10 and upgrade through GitHub Copilot
upgrade only at prompt `3.01`, then migrate to `.slnx`. Use `.sln` in the
validation command below before that transition. Do not downgrade existing
later-stage projects or introduce library/web projects during initial setup.

## Folder Structure

* `src/workspace/calculator-xunit-testing/calculator/`: .NET console project with console interaction
* `src/workspace/calculator-xunit-testing/calculator.library/`: Shared calculator logic used by app and tests
* `src/workspace/calculator-xunit-testing/calculator.web/`: Blazor web app with calculator UI and services
* `src/workspace/calculator-xunit-testing/calculator.tests/`: xUnit test project
* `src/workspace/calculator-xunit-testing/calculator.slnx`: Solution file
* `.github/skills/calculator-setup/scripts/Set-DotnetCalculatorSolution.ps1`: Script that creates the initial solution structure
* `.github/skills/reset-calculator-lab/`: Skill that preserves and removes the generated solution structure

## Libraries and Frameworks

* .NET 10.0 target framework
* Blazor Web App with server interactivity
* xUnit test framework
* Npgsql and Testcontainers PostgreSQL for automated PostgreSQL-backed test data
* `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, and `coverlet.collector` test dependencies in `calculator.tests.csproj`

## Coding Standards

* Use PascalCase for classes, methods, and public members.
* Use camelCase for parameters and local variables.
* Keep calculator logic focused, deterministic, and free of console or web dependencies.
* Keep Blazor UI state in scoped services under `calculator.web/Services/`.
* Keep tests in `CalculatorTest.cs` and use descriptive test names such as `[MethodName]_[Scenario]_[ExpectedBehavior]`.
* Use xUnit `Assert` APIs for test assertions.
* Document public C# members with XML documentation comments.
* Verify actual project target frameworks before upgrades; SDK installation alone does not retarget projects. Preserve staged pre-upgrade .NET 8 material.
* Pass changing state into child components explicitly; validate immediate rendered history updates, not only mutations to an injected service.
* On .NET 10, preserve static asset endpoints and validate published Release CSS/JavaScript, not just a development server.

## Key Practices

Run the following command from the repository root after calculator changes:

```bash
dotnet test src/workspace/calculator-xunit-testing/calculator.slnx
```

* Use `[Fact]` for single-scenario tests.
* Use `[Theory]` with `[InlineData]` or `[MemberData]` when parameterized tests are introduced.
* Keep generated `bin/` and `obj/` output out of documentation examples unless discussing build artifacts.
* Use the browser-validation skill for UI changes, including readiness, history, keyboard/native focus, theme, and circuit isolation. HTTP 200 and passing service tests alone are insufficient.
* Keep fixture seeding ahead of execution, disable discovery enumeration for database-backed xUnit v2 `MemberData`, use invariant finite numeric parsing, and dispose failed-start containers.
* Audit direct/transitive NuGet packages after dependency changes without suppressing warnings. Container image servicing is a separate check.

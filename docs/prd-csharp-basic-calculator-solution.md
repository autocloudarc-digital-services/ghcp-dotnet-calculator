---
title: .NET Calculator With xUnit Testing PRD
description: Product requirements for the .NET 8 calculator console application and xUnit test solution
---

## 1.1 Document Information

| Field   | Value          |
|---------|----------------|
| Version | 1.0            |
| Author  | GitHub Copilot |
| Date    | 2025-11-03     |
| Status  | Draft          |

This PRD intentionally defines the initial .NET 8 console/two-project stage,
not the live post-upgrade inventory. Its GUI/history exclusions and package
examples apply to that stage. Later requirements are captured by the
[.NET 10 upgrade](../.github/prompts/3.01-upgrade-dotnet-from-8-to-10.prompt.md),
[Blazor refactor](../.github/skills/refactor-calculator-blazor-app/SKILL.md),
and [Azure assessment/deployment follow-up](azure-migration-assessment-3.02.md).
Read live project files for current targets and dependencies; do not downgrade
or recreate a later workspace to match this baseline.

## 1.2 Executive Summary

This product requirements document defines a basic .NET 8 console calculator application with xUnit testing. The calculator supports standard arithmetic operations and demonstrates testability, maintainability, null safety, and error handling in a compact C# solution.

## 1.3 Problem Statement

Developers need a practical example of a well-structured .NET application that demonstrates these capabilities:

* Basic calculator functionality for essential arithmetic operations
* A clear application structure with testable components
* Unit testing with the xUnit framework
* Effective error handling and input validation

The solution serves as both a functional calculator and a reference implementation for .NET development practices.

## 1.4 Goals And Objectives

* Create a functional calculator application using .NET 8 and C#.
* Implement core arithmetic operations for addition, subtraction, multiplication, division, modulo, and exponentiation.
* Demonstrate C# practices including nullable reference handling, error management, and code organization.
* Keep arithmetic operations in testable public methods.
* Provide xUnit tests with both fact and theory test approaches.
* Enable a clean, interactive console experience with clear user feedback.
* Maintain a solution structure with separate application and test projects.

## 1.5 Scope

### 1.5.1 In Scope

* PowerShell scripts for setting up and removing the calculator solution.
* A .NET 8 console application using top-level statements.
* Calculator operations for addition, subtraction, multiplication, division, modulo, and exponentiation.
* User input handling with validation and feedback.
* An xUnit test project with coverage for normal and error cases.
* Method-level refactoring for testability.
* Error handling and nullable reference safety.
* Documentation for application behavior, tests, setup, and cleanup.

### 1.5.2 Out Of Scope

* GUI implementation
* Advanced mathematical functions such as logarithms or trigonometry
* Calculation persistence
* Scientific calculator features
* Multi-language support
* Calculator history

## 1.6 Requirements

### 1.6.1 Epic Requirements

As a developer, I want to create a .NET 8 console calculator application that performs arithmetic operations so that I can demonstrate .NET development and testing practices.

### 1.6.2 User Stories And Use Cases

* As a user, I want to perform arithmetic calculations with two operands so that I can compute results quickly.
* As a user, I want clear prompts and feedback so that I understand how to use the application.
* As a user, I want to perform multiple calculations in sequence so that I do not have to restart the application.
* As a user, I want invalid input to be handled without crashing the application.
* As a developer, I want well-structured and tested code so that I can understand .NET development practices.
* As a QA tester, I want xUnit coverage for calculator operations so that I can verify the calculator functions correctly.

## 1.7 Functional Requirements

| Requirement ID | Description                                                                       |
|----------------|-----------------------------------------------------------------------------------|
| FR-1           | The application shall prompt users for two numeric operands and one operator.     |
| FR-2           | The application shall support addition operations between two numbers.            |
| FR-3           | The application shall support subtraction operations between two numbers.         |
| FR-4           | The application shall support multiplication operations between two numbers.      |
| FR-5           | The application shall support division operations between two numbers.            |
| FR-6           | The application shall support modulo operations between two numbers.              |
| FR-7           | The application shall support exponent operations between two numbers.            |
| FR-8           | The application shall display each calculation result to the user.                |
| FR-9           | The application shall handle division by zero errors gracefully.                  |
| FR-10          | The application shall validate user input and provide feedback for invalid input. |
| FR-11          | The application shall allow users to perform another calculation.                 |
| FR-12          | The application shall confirm before exiting.                                     |
| FR-13          | The application shall clear the screen between calculations when possible.        |
| FR-14          | Each arithmetic operation shall be implemented as a testable method.              |
| FR-15          | The application shall handle null console input without runtime exceptions.       |

## 1.8 Non-Functional Requirements

| Category        | Requirement                                                                  |
|-----------------|------------------------------------------------------------------------------|
| Performance     | The calculator shall provide immediate feedback for all operations.          |
| Usability       | The console interface shall use clear prompts and instructions.              |
| Reliability     | The application shall handle edge cases and invalid inputs without crashing. |
| Testability     | Core calculator operations shall be covered by unit tests.                   |
| Maintainability | Code shall follow C# practices and use readable names.                       |
| Compatibility   | The application shall run on platforms that support .NET 8.                  |

## 1.9 Assumptions And Dependencies

* Development uses the .NET 8 SDK.
* xUnit is the unit testing framework.
* Developers have access to .NET development tools.
* The application uses standard C# console I/O.
* No external runtime libraries are required beyond the .NET SDK.

### 1.9.1 Package Versions

Initial-stage package examples, not the active test project's version inventory:

| Package                   | Version |
|---------------------------|---------|
| xunit                     | 2.5.3   |
| xunit.runner.visualstudio | 2.5.3   |
| Microsoft.NET.Test.Sdk    | 17.8.0  |
| coverlet.collector        | 6.0.0   |

## 1.10 Success Criteria

* All specified calculator operations work correctly.
* Unit tests cover arithmetic operations and divide-by-zero error cases.
* The console application runs without crashing for valid or invalid input.
* Build and test commands execute successfully.
* Code uses nullable reference handling where console input can return null.
* Documentation is clear and consistent with the repository structure.

## 1.11 Milestones And Timeline

### 1.11.1 Setup Script

* Create a PowerShell script for initializing the solution structure.
* Set up the .NET 8 console application and xUnit test project.

### 1.11.2 Initial Implementation

* Implement calculator operations with top-level console interaction.
* Build and test the initial version.

### 1.11.3 Refactoring

* Move arithmetic operations into testable methods.
* Add error handling and screen clearing.
* Make methods public for test access.

### 1.11.4 Testing

* Create xUnit tests for all operations.
* Use both fact and theory test approaches.
* Ensure all tests pass.

### 1.11.5 Final Improvements

* Add modulo and exponent operations.
* Document the code.
* Fix remaining issues or warnings.

### 1.11.6 Documentation

* Update documentation.
* Create setup and cleanup scripts.

## 1.12 Implementation Guidance

### 1.12.1 Solution Setup

Create a PowerShell script named `Set-DotnetSlnForCalculator.ps1` in `$(git rev-parse --show-toplevel)\src\workspace` that performs these actions:

* Creates a solution folder named `calculator-xunit-testing`.
* Sets up a new solution using `dotnet new sln`.
* Adds a console application project named `calculator` using `dotnet new console`.
* Adds an xUnit test project named `calculator.tests` using `dotnet new xunit`.
* Configures project references between the projects.
* Changes default file names from `Program.cs` to `Calculator.cs` and from `UnitTest1.cs` to `CalculatorTest.cs`.
* Targets .NET 8 in both projects.
* Builds the solution to verify setup.
* Executes the PowerShell script to create the solution structure.

Implementation notes:

* Use the standard `dotnet new` templates and ensure both project files target `net8.0`.
* The setup script can pass `--framework net8.0` when creating supported project templates.
* Keep the solution under `src/workspace/calculator-xunit-testing`.

### 1.12.2 Calculator Implementation

The initial console-stage solution uses this file structure:

| File                                 | Purpose                                                              |
|--------------------------------------|----------------------------------------------------------------------|
| `calculator/Calculator.cs`           | Top-level console interaction, prompting, looping, and user feedback |
| `calculator/CalculatorOperations.cs` | Public static arithmetic methods used by the console app and tests   |
| `calculator.tests/CalculatorTest.cs` | xUnit tests for arithmetic operations and divide-by-zero cases       |

Implementation requirements:

* Define public static methods for arithmetic operations in `CalculatorOperations.cs`.
* Keep top-level console statements in `Calculator.cs`.
* Prompt for the first operand, second operand, and operator.
* Perform the appropriate arithmetic operation.
* Display the result or a friendly error message.
* Ask whether the user wants to perform another calculation.
* Handle user responses to continue or exit.
* Use nullable-aware input handling for console input.

> [!IMPORTANT]
> Top-level statements and class or namespace declarations cannot be mixed in the same C# source file in invalid order. Keep reusable operation methods in `CalculatorOperations.cs` and the console workflow in `Calculator.cs`.

### 1.12.3 Refactoring Steps

* Convert arithmetic operations into methods for testability.
* Make methods public so the xUnit project can test them.
* Add screen clearing for a better user experience.
* Implement modulo and exponent operations.
* Add null handling to prevent input-related exceptions.

### 1.12.4 Testing Strategy

* Create xUnit tests for each calculator operation.
* Use `[Fact]` tests for single-scenario assertions.
* Use `[Theory]` tests with `[InlineData]` for multiple cases.
* Include edge cases such as division by zero and modulo by zero.
* Ensure all tests pass before completion.

## 1.13 Cleanup Solution

Use the `reset-calculator-lab` skill and its bundled
`.github/skills/reset-calculator-lab/scripts/Remove-DotnetSlnForCalculator.ps1`
script to perform these actions:

* Gets the repository root path using `git rev-parse --show-toplevel`.
* Constructs the workspace path as `{RepoRoot}\src\workspace`.
* Identifies the solution directory as `{Workspace}\calculator-xunit-testing`.
* Requires the existing `{RepoRoot}\src\completed` folder to be empty.
* Supports a `-WhatIf` preview and explicit confirmation.
* Copies the solution to `{RepoRoot}\src\completed\calculator-xunit-testing`.
* Verifies the preserved directory before removing the workspace copy.
* Blocks removal when preservation prerequisites are not satisfied.
* Provides status messages for successful removal, safe failures, and no-op runs.

Execution from the repository root:

```powershell
pwsh .github/skills/reset-calculator-lab/scripts/Remove-DotnetSlnForCalculator.ps1 -WhatIf
pwsh .github/skills/reset-calculator-lab/scripts/Remove-DotnetSlnForCalculator.ps1 -Confirm
```

The script preserves the completed solution before removing the generated
workspace, allowing the exercise to be reset without discarding the final
implementation.

## 1.14 Additional Learning Outcomes

* Understanding .NET solution and project structure
* Working with C# top-level statements
* Understanding C# source file structure constraints
* Implementing nullable-aware input handling in C#
* Writing xUnit tests with facts and theories
* Creating reusable setup and cleanup scripts
* Managing package versions for .NET projects

## 1.15 Troubleshooting Guide

### 1.15.1 Build Error CS8803: Top-Level Statements Must Precede Declarations

Cause: Top-level statements and class declarations are in the same file or ordered incorrectly.

Solution: Keep console workflow code in `Calculator.cs` and reusable operation methods in `CalculatorOperations.cs`.

### 1.15.2 Unexpected Test Discovery Count

Cause: The test project may have incompatible package versions, stale build output, or corrupted test files.

Solution: Verify package versions in `calculator.tests.csproj`, run `dotnet clean`, restore packages, and rerun the tests.

### 1.15.3 Target Framework Mismatch

Cause: Project files target a framework other than `net8.0`.

Solution: Verify `calculator.csproj` and `calculator.tests.csproj` both contain `<TargetFramework>net8.0</TargetFramework>`.

### 1.15.4 Test File Corrupted Or Reverted

Cause: Previous failed operations may have reverted `CalculatorTest.cs` to a stub implementation.

Solution: Reapply the testing strategy and verify tests cover all public arithmetic methods.

### 1.15.5 Build Succeeds But Tests Do Not Run

Cause: Build artifacts may be stale after framework or package changes.

Solution: Run `dotnet clean`, `dotnet restore`, and `dotnet build` before running tests again.

### 1.15.6 Setup Script Creates Projects Targeting The Wrong Framework

Cause: The `dotnet new` command may use the installed SDK default if no framework is specified.

Solution: Pass `--framework net8.0` where supported, or programmatically replace the `<TargetFramework>` value in both project files after project creation.

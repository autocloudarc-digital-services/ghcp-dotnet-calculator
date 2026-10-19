---
title: GitHub Copilot Instructions for Calculator Workspace
description: Repository-specific GitHub Copilot guidance for the calculator solution and customizations
---

## GitHub Copilot Instructions for Calculator Workspace

**Version:** 2.0  
**Last Updated:** October 2026

**Repository:** `ghcp-dotnet-calculator`

## Overview

This repository is a focused .NET calculator learning workspace with GitHub Copilot customization assets. These instructions guide GitHub Copilot to keep code, tests, prompts, skills, and workflow guidance aligned with the files that exist in this repository.

High-signal areas include:

- A .NET 10 calculator solution under `src/workspace/calculator-xunit-testing/`
- A console calculator project under `src/workspace/calculator-xunit-testing/calculator/`
- A shared calculator library under `src/workspace/calculator-xunit-testing/calculator.library/`
- A Blazor web app under `src/workspace/calculator-xunit-testing/calculator.web/`
- An xUnit test project under `src/workspace/calculator-xunit-testing/calculator.tests/`
- Workspace setup and reset scripts under `src/workspace/`
- A PostgreSQL seeding skill for manual local workflows and a Testcontainers-first test strategy
- Prompt, agent, and skill customization assets under `.github/`
- GitHub workflows under `.github/workflows/`

When generating or modifying code, prefer patterns that already exist in the repository over generic or outdated examples.

---

## Repository Structure

```text
.github/
├── agents/          # Custom Copilot agent definitions (.agent.md)
├── instructions/    # Scoped instruction files (.instructions.md)
├── prompts/         # Numbered prompt workflows (.prompt.md)
├── skills/          # Repo-specific automation skills
├── workflows/       # GitHub Actions CI/CD workflows
├── scripts/         # Workflow helper scripts
├── dependabot.yml
└── dependency-review-config.yml

src/
├── workspace/
│   ├── Set-DotnetSlnForCalculator.ps1
│   └── calculator-xunit-testing/
│       ├── calculator.slnx
│       ├── calculator/
│       │   ├── calculator.csproj
│       │   └── Calculator.cs
│       ├── calculator.library/
│       │   ├── calculator.library.csproj
│       │   └── CalculatorOperations.cs
│       ├── calculator.tests/
│       │   ├── calculator.tests.csproj
│       │   ├── CalculatorTest.cs
│       │   └── TestCases.csv
│       └── calculator.web/
│           ├── calculator.web.csproj
│           ├── Components/
│           ├── Services/
│           └── wwwroot/
└── completed/
```

---

## Core Principles

1. **Prefer existing patterns** — Reuse helpers, scripts, project structures, and runners already in the repo before creating parallel implementations.
2. **Intent clarity** — Start with explicit action verbs; specify deliverable types and success criteria.
3. **Specificity** — Include exact names, data types, library versions, and numeric constraints.
4. **Structured communication** — Use bullet points, sections, and avoid ambiguous pronouns.
5. **Environment-driven configuration** — Use environment variables for integration points; make failure messages actionable by naming the missing variable or dependency.

---

## Programming Standards

### General Rules

- Add descriptive documentation comments (XML docs, JSDoc, docstrings) with parameters, returns, and exceptions
- Use input validation with early returns for error conditions
- Keep functions focused on a single responsibility (aim for < 50 lines)
- Use meaningful variable names (avoid single letters except in loops)
- Keep files under 200 lines when practical
- Use appropriate access modifiers (private by default)

### Refactoring Guidelines

- Prefer pure functions where possible.
- Apply SOLID principles.
- Replace long conditionals with the strategy pattern.
- Use existing framework logging, such as ASP.NET Core `ILogger<T>`. Do not invent a telemetry wrapper that does not exist in the workspace.

### Language-Specific Conventions

| Language | Framework | Testing | Naming | Target |
|----------|-----------|---------|--------|--------|
| C# | .NET 8–10 | xUnit | PascalCase (public), camelCase (params) | `net8.0` or `net10.0` per project |
| C/C++ | CMake | Google Test / catch2 | snake_case / PascalCase | C++17 |
| Python | varies | pytest | snake_case | 3.8+ |
| TypeScript | strict mode | Jest | camelCase | ES2020+ |
| Java | Maven | JUnit 5 | PascalCase (classes), camelCase (methods) | 11+ |
| PowerShell | 7+ | Pester | PascalCase | Set-StrictMode Latest |
| Go | go modules | go test | exported PascalCase | latest stable |

### .NET/C# (Active Calculator Workspace)

- Use `#nullable enable` in all source files
- Include XML documentation (`///`) on all public members
- Prefer xUnit for tests in `calculator.tests`
- Use async/await for I/O operations
- Keep pure calculator behavior in `calculator.library`, application wiring in `calculator` or `calculator.web`, and assertions in `calculator.tests`
- For CSV-backed calculator tests, keep `TestCases.csv` in the `calculator.tests` project root and copy it to the output directory
- For optional manual PostgreSQL exercises, use a persistent local container seeded from `TestCases.csv`; the current Blazor arithmetic/history/theme runtime does not need a database
- For automated PostgreSQL-backed calculator tests, prefer Testcontainers with per-run randomized credentials and runtime CSV seeding
- Use the PostgreSQL container seeding skill to maintain stable local connection settings during refactoring
- Do not include plaintext credential examples in prompts, skills, docs, or test code
- Follow Microsoft naming conventions strictly

### PowerShell

- Opening braces `{` on new line
- `Set-StrictMode -Version Latest` at script top
- `$ErrorActionPreference = 'Stop'` for deployment scripts
- Use `[Parameter(Mandatory=$true)]` validation
- Resolve repo root from `$PSScriptRoot`

### Python

- Follow PEP 8; use type hints on all function signatures
- Google-style docstrings
- Use `logging` module instead of `print()` in production code
- Keep dependencies in `requirements.txt`

### TypeScript

- `strict: true` in tsconfig.json; never use `any`
- Full type definitions on all exports
- Functional React components with hooks (for React projects)

---

## Testing Standards

- Write tests alongside implementation (TDD preferred)
- Test naming: `[MethodName]_[Scenario]_[ExpectedBehavior]`
- Include edge cases, error conditions, and happy paths
- Aim for 80%+ coverage on critical paths
- Extend existing test projects before introducing new frameworks
- For calculator validation, run `dotnet test src/workspace/calculator-xunit-testing/calculator.slnx`
- To recreate the exercise solution, use the `calculator-setup` skill. To reset it after Exercise 99.08, use the `reset-calculator-lab` skill and preview its bundled script with `-WhatIf`.
- Keep integration/E2E tests deterministic and isolated; prefer ephemeral resources such as Testcontainers when feasible
- Confirm actual project targets rather than inferring them from the SDK. For web changes, validate published Release interactivity with the browser skill, not HTTP status or unit tests alone.

### Azure Assessment And Deployment Boundaries

- Prompt `3.02` is read-only assessment; unavailable native tools mean explicitly partial native-tool coverage, not permission to invent reports or run migration tools.
- Use `deploy-calculator-to-azure` only for separately approved implementation/provisioning/deployment. Collect missing target/authentication decisions and explicit cost approval; never reuse a prior session's subscription or resource names implicitly.
- The default Bicep is web-only. Do not provision PostgreSQL, Key Vault, or paid telemetry just because database-backed tests exist. Keep AZD's required `AZURE_WEB_APP_NAME` and explicit resource group/region mapping aligned.
- Preserve healthy local services and unrelated containers. Do not log out, stop services, change SKU/region, enable basic publishing authentication, or delete resources to work around missing approval or a diagnostic failure.
- Retain upload/deployment exit codes and reconcile them with OneDeploy, worker/startup diagnostics, HTTPS health, and actual browser results. Do not equate ZIP acceptance with readiness or silently relabel a startup-tracker failure as a successful command.
- Publish one immutable web-only artifact, retain its hash and rollback copy durably for a real release, and report B1's ongoing charges and lack of deployment slots.

---

## CI/CD and Workflow Conventions

- Use descriptive kebab-case names (`build-and-test-dotnet`)
- Pin action versions (`actions/checkout@v4`)
- Never hardcode secrets; use `secrets.` or environment variables
- Preserve GHAS coverage: CodeQL triggers on `push`, `pull_request`, `schedule`, `workflow_dispatch`
- Do not relax dependency review policy without explicit justification
- Separate build, test, and deploy jobs with proper `needs:` dependencies
- Use concurrency groups to cancel in-progress runs

---

## Release And Milestone Conventions

Use GitHub milestones to plan meaningful curriculum or platform goals, annotated
Git tags to mark exact repository states, and GitHub Releases to communicate
completed milestones with release notes and validation evidence.

### Release Versioning

Use lightweight Semantic Versioning for repository releases:

```text
vMAJOR.MINOR.PATCH
```

- Increment `MAJOR` when exercise order, learner setup assumptions, public APIs,
  or repository structure changes in a breaking way.
- Increment `MINOR` when adding a meaningful workshop capability, exercise
  track, environment baseline, workflow set, or learner-facing module.
- Increment `PATCH` when fixing documentation, setup scripts, validation issues,
  workflow defects, or small learner-facing bugs without changing the milestone
  scope.
- Use pre-release suffixes such as `v0.2.0-rc.1` or `v0.3.0-beta.1` for preview
  builds that should not be treated as stable workshop baselines.

### Release Naming

Use this naming pattern for release artifacts:

```text
Milestone: M# - Short milestone name
Tag:       vMAJOR.MINOR.PATCH
Release:   vMAJOR.MINOR.PATCH - Short milestone name
```

For the Codespaces workshop baseline, use these names unless a later planning
decision supersedes them:

```text
Milestone: M1 - Codespaces-ready workshop baseline
Tag:       v0.2.0
Release:   v0.2.0 - Codespaces-ready workshop baseline
```

Reserve `v1.0.0` for the first stable public workshop release where the main lab
path, GitHub Actions workflow exercises, Codespaces setup, validation guidance,
and cleanup flow are all ready for broad learner use.

### Release Notes

Every GitHub Release must include these sections:

```markdown
## Summary

## Added

## Changed

## Fixed

## Validation

## Known Limitations

## Upgrade Notes
```

Keep release notes specific to learner impact and repository operations. Include
validation commands, known environment caveats, breaking changes, and any action
learners or maintainers must take after updating.

Store permanent release notes in a SemVer-named Markdown file under
`docs/release-notes/`:

```text
docs/release-notes/vMAJOR.MINOR.PATCH.md
```

Do not keep overwriting a root `RELEASE_NOTES.md` file for multiple releases.
Use one permanent release notes file per published version. If the GitHub CLI
release body should omit repository frontmatter, generate a temporary untracked
body file from the permanent notes and pass that file to `gh release create
--notes-file`.

### Release Workflow

Before publishing a release, complete these steps:

1. Confirm the milestone scope is complete and all related issues or PRs are
    linked to the GitHub milestone.
2. Run the narrowest relevant validation first, then the repository-level checks
    required for the changed area.
3. Review `git status`, `git diff --check`, and the final diff for secrets,
    generated noise, stale paths, and accidental edits.
4. Commit the release candidate with a clear conventional commit message, such
    as `feat: add Codespaces workshop baseline`.
5. Create an annotated tag:

    ```powershell
    git tag -a vMAJOR.MINOR.PATCH -m "release title"
    ```

6. Push the commit and tag, then publish a GitHub Release from that tag.
7. Close the GitHub milestone only after the release is published and validation
    evidence is recorded in the release notes.

---

## Infrastructure as Code

- Follow Azure naming conventions (`rg-${environment}-${project}`)
- Include parameter descriptions, metadata, and validation rules
- Implement consistent tagging (environment, cost center, owner, project)
- Use minimal RBAC permissions and enable diagnostics logging
- Add infrastructure assets only when needed for a tracked exercise, and document their location in the relevant prompt or skill

---

## Documentation Standards

- Markdown with proper heading hierarchy (H1 > H2 > H3)
- Language-tagged code fences (use `text` over empty fences when no language applies)
- Relative paths for internal links
- PRDs follow the template: Executive Summary → Problem Statement → Goals → Functional Requirements → Non-Functional Requirements → Implementation Guidance → Testing → Success Criteria

---

## Prompt, Agent, and Skill Conventions

- **Prompts** (`.github/prompts/`): Follow the numbered naming scheme (`1.14-...`, `2.03-...`, `3.04-...`) representing staged workflows
- **Agents** (`.github/agents/`): Keep frontmatter accurate; update existing definitions rather than creating duplicates
- **Skills** (`.github/skills/`): Self-contained folders with `SKILL.md` and supporting scripts
- **Session handoffs**: Use the `create-session-handoff` skill to write local
    context to `artifacts/session-handoffs/`. Create the directory when missing,
    use minute-level names (`session-handoff-YYYYMMDD-HHmm.md`), and add a numeric
    suffix when a handoff already exists for that minute rather than overwriting it.
- **Accuracy**: Verify names and paths against live directory contents; older docs may lag behind
- **Upgrade history**: Preserve intentional pre-upgrade `.NET 8` references in stage-specific prompts, PRDs, and reports. Normalize only current active workspace guidance to `.NET 10` after the 3.01 upgrade workflow.

### AGENTS.md Pattern

The root `AGENTS.md` uses a ten-section editorial pattern rather than an official standardized schema. Derive its content from recursive inspection of
the live repository and use these exact level-two headings for the initial version:

```markdown
## Repository Overview
## Repository Structure
## Prerequisites And Setup
## Build And Run
## Testing
## Architecture And Boundaries
## Coding And Documentation Conventions
## Security And Configuration
## Automation And Deployment
## Change Workflow And Completion Criteria
```

Apply the pattern as follows:

* Use overview and structure to orient agents to the repository and distinguish
    active implementation from historical, completed, generated, and reference
    content.
* Use prerequisites, build, and testing to capture executable setup and
    validation commands supported by repository files.
* Use architecture, conventions, and security to preserve component boundaries,
    scoped instructions, secure defaults, and configuration rules.
* Use automation and completion workflow to define CI/CD, infrastructure,
    deployment approvals, safe editing, final validation, and reporting.
* Tailor every section to repository evidence, link to detailed guidance instead
    of duplicating it, and never invent commands or requirements to fill a
    category.

### GitHub Copilot Agent Skills

Use agent skills for repeatable, on-demand workflows that should load only when relevant. Prefer custom instructions for broad, always-on repository guidance, prompt files for single reusable task prompts, and custom agents when the workflow needs a distinct persona, context isolation, or tool restrictions.

Create project skills under `.github/skills/<skill-name>/` unless there is a clear reason to use `.agents/skills/<skill-name>/` or `.claude/skills/<skill-name>/`. Each skill must include a file named exactly `SKILL.md`. Supporting folders are optional and use these conventions:

- `scripts/` contains reviewed executable automation.
- `references/` contains detailed guidance, schemas, and examples that load only when needed.
- `assets/` contains templates, sample data, images, and starter files used by the skill.
- Add another supporting folder only when these conventions do not fit the resource type.

Required `SKILL.md` frontmatter:

```yaml
---
name: skill-name
description: "What the skill does. Use when: include specific trigger phrases users will type."
---
```

- Keep the skill folder name and `name` value lowercase, hyphenated, and matching.
- Make the `description` keyword-rich because Copilot uses it to decide when to load the skill.
- Put step-by-step procedure, examples, quality bars, and tool guidance in the Markdown body.
- Reference bundled resources with relative paths such as `[script](./scripts/run-checks.ps1)`.
- Keep `SKILL.md` focused; move long supporting guidance into `references/` and load it only when needed.
- Only pre-approve shell-like tools after reviewing every included script and trusting the source.

Example structure:

```text
.github/skills/python-cli-app/
├── SKILL.md
├── scripts/
│   └── smoke-test.ps1
├── references/
│   └── python-style.md
└── assets/
    └── starter-template.py
```

For Copilot CLI sessions, use `/skills reload`, `/skills list`, and `/skills info <skill-name>` after adding or changing a skill. When using GitHub CLI skill management, inspect third-party skills before installing them with `gh skill preview`, then use `gh skill install`, `gh skill update`, or `gh skill publish --dry-run` as appropriate.

### GitHub Copilot CLI MCP Migration Note

When practicing GitHub Copilot CLI exercises that need the same MCP servers as
VS Code, keep the VS Code workspace configuration in `.vscode/mcp.json` and add
a separate `.mcp.json` for Copilot CLI. VS Code uses a top-level `servers` key;
Copilot CLI uses a top-level `mcpServers` key. Migrating the file for CLI use
must not remove or replace `.vscode/mcp.json`, because that file remains the
workspace MCP source for the VS Code GitHub Copilot Chat experience.

Prefer the bundled setup script for this repository because it writes the CLI
`mcpServers` shape with explicit `tools` filters:

```powershell
pwsh .github/skills/configure-mcp-servers/scripts/Set-McpServerConfiguration.ps1 -Editor CopilotGeneric
```

The GitHub Docs migration pattern remaps the VS Code shape into the CLI shape:

```powershell
pwsh -NoProfile -Command "`$json = Get-Content '.vscode/mcp.json' -Raw | ConvertFrom-Json; `$content = ([pscustomobject]@{ mcpServers = `$json.servers } | ConvertTo-Json -Depth 100); [System.IO.File]::WriteAllText('.mcp.json', `$content, (New-Object System.Text.UTF8Encoding `$false))"
```

After creating or updating `.mcp.json`, use Copilot CLI MCP commands such as
`/mcp show`, `/mcp reload`, or `copilot mcp list` to verify CLI visibility. For
the VS Code interface, reload VS Code or use the VS Code MCP server reload flow
after editing `.vscode/mcp.json`.

---

## Pull Request Guidelines

### Industry Standard Practices

- Keep pull requests small, focused, and limited to a single logical change.
- Use a clear title that summarizes the intent and scope of the change.
- Explain what changed, why it changed, how it was tested, and any risks or trade-offs.
- Link related issues, work items, design notes, or incident references.
- Include screenshots, logs, API examples, or before/after output for user-facing or behavioral changes.
- Ensure code builds locally and all relevant tests pass before requesting review.
- Add or update tests for new functionality, bug fixes, and meaningful edge cases.
- Update documentation, configuration examples, and migration notes when behavior changes.
- Keep commits meaningful and avoid unrelated formatting, refactoring, or generated-file noise.
- Highlight breaking changes, database migrations, feature flags, rollout steps, and rollback plans.
- Request reviewers with relevant domain ownership and keep review discussions respectful and actionable.
- Address reviewer feedback with follow-up commits or clear explanations; do not resolve comments without action or agreement.
- Rebase or merge the target branch as needed to resolve conflicts before approval.
- Do not include secrets, credentials, sensitive data, debug artifacts, or temporary code.

## CI/CD Workflows and Pipelines

- Keep workflows small, composable, and named for their purpose; split build, test, security, package, and deploy stages when it improves clarity.
- Trigger pipelines intentionally with `pull_request`, protected-branch `push`, scheduled, and manual `workflow_dispatch` events as appropriate; avoid overly broad triggers.
- Use branch protection and required status checks so code cannot merge unless the expected validation jobs pass.
- Pin third-party actions to a full commit SHA or trusted version, and review action source before adoption or upgrade.
- Grant the minimum required `permissions` for each workflow or job; default to read-only and elevate only where needed.
- Store secrets in GitHub Secrets, environment secrets, or an approved vault; never echo secrets or write them to artifacts, logs, or caches.
- Use protected environments with reviewers, deployment gates, and environment-specific secrets for production releases.
- Make builds deterministic with lock files, explicit SDK/runtime versions, and reproducible package restore steps.
- Cache dependencies and build outputs only when cache keys are accurate and safe; never cache secrets or mutable deployment credentials.
- Run fast feedback checks first, including formatting, linting, compilation, and targeted unit tests before slower integration or end-to-end suites.
- Use matrix builds for supported operating systems, runtimes, frameworks, or configurations, but keep the matrix limited to meaningful coverage.
- Publish test results, coverage, logs, and build artifacts with clear retention settings so failures are easy to diagnose.
- Add security checks such as dependency review, secret scanning, static analysis, and container/image scanning where applicable.
- Build artifacts once and promote the same immutable artifact through environments instead of rebuilding per environment.
- Prefer automated deployment strategies with rollback support, such as blue/green, canary, staged rollout, or feature-flagged releases.
- Keep deployment jobs idempotent and safe to rerun; include health checks and fail fast on unsuccessful validation.
- Use concurrency groups to prevent overlapping deployments or duplicate runs on the same branch, pull request, or environment.
- Avoid long-lived credentials by using OIDC federation to cloud providers when supported.
- Document required variables, secrets, permissions, environments, and manual approval steps near the workflow or in project docs.
- Review workflow changes with the same rigor as application code because pipelines can affect supply chain security and production stability.
- Use this format for workflow elements naming conventions, job names, and step names: lowercase, hyphenated, and descriptive of the action being performed.

## GitHub Agentic Workflows

When creating GitHub Agentic workflows, use the standards as defined at https://github.com/github/gh-aw for the schema, example templates, and workflow best practices. For example, use the `workflow-doctor` agent persona for diagnosing failed runs, and ensure that the workflow is idempotent, safe to rerun, and provides actionable feedback for developers. Leverage the definitions of other workflows in this repository as examples for structuring your own agentic workflows, while maintaining local repository workflow naming standards.

### PR Description Structure

```markdown
## What changed
- Summary of modifications and affected components
- Link to related issues

## Why
- Business context and technical reasoning

## Testing
- [ ] Unit tests pass and cover new functionality
- [ ] Manual testing completed for user-facing changes
- [ ] Performance/security considerations addressed


## Breaking Changes
- API changes or behavioral modifications with migration instructions

## Checklist
- [ ] Code follows project style guidelines
- [ ] Comments added for complex logic
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] All tests passing locally
```

### Code Review Focus

- **Security:** Input validation, auth, secrets, data handling
- **Performance:** Query optimization, algorithmic complexity, memory
- **Testing:** Coverage, edge cases, integration scenarios
- **Documentation:** Comments, README updates, inline docs

---

## Quality Metrics

- **Maintainability:** Readable by new team members within 15 minutes
- **Test Coverage:** 80%+ on critical paths; 100% on business logic
- **Complexity:** Cyclomatic complexity < 10 per function (aim for < 5)
- **Performance:** Build < 5 minutes; deploy < 10 minutes
- **Security:** No hardcoded secrets; validated inputs; no sensitive data in logs

---

## Pre-Submission Checklist

- [ ] Follows language-specific style guide
- [ ] All tests pass locally
- [ ] No security vulnerabilities (secrets, unvalidated input)
- [ ] Error handling implemented appropriately
- [ ] PR description follows template
- [ ] Commit messages: `verb: description`
- [ ] No `console.log`/`Debug.WriteLine` or commented-out code left behind

---

## Anti-Patterns to Avoid

| ❌ Don't | ✅ Do |
|-----------|--------|
| "Fix this code" | "Fix the null reference in CalculateTotal() by validating the values parameter" |
| "Create a function" | "Create a function validating emails per RFC 5322, throwing ArgumentException for invalid input" |
| "Optimize performance" | "Optimize the query to < 500ms by indexing UserId and CreatedDate" |
| "Update that file" | "Update Program.cs to add input validation following the pattern in Calculator.cs" |

---

## Resources

- [GitHub Copilot Docs](https://docs.github.com/en/copilot)
- [Agent Definitions](agents/)
- [Workflow Examples](workflows/)
- [Prompt Library](prompts/)

## PRD Format Template

## 1.1 Document Information

## 1.2 Executive Summary

## 1.3 Problem Statement

## 1.4 Goals And Objectives

## 1.5 Scope

### 1.5.1 In Scope

### 1.5.2 Out Of Scope

## 1.6 Requirements

### 1.6.1 Epic Requirements

### 1.6.2 User Stories And Use Cases

## 1.7 Functional Requirements

## 1.8 Non-Functional Requirements

## 1.9 Assumptions And Dependencies

### 1.9.1 Package Versions

## 1.10 Success Criteria

## 1.11 Milestones And Timeline

### 1.11.1 Setup Script

### 1.11.2 Initial Implementation

### 1.11.3 Refactoring

### 1.11.4 Testing

### 1.11.5 Final Improvements

### 1.11.6 Documentation

## 1.12 Implementation Guidance

### 1.12.1 Solution Setup

### 1.12.2 Calculator Implementation

### 1.12.3 Refactoring Steps

### 1.12.4 Testing Strategy

## 1.13 Cleanup Solution

## 1.14 Additional Learning Outcomes

## 1.15 Troubleshooting Guide

### 1.15.1 Build Error CS8803: Top-Level Statements Must Precede Declarations

### 1.15.2 Unexpected Test Discovery Count

### 1.15.3 Target Framework Mismatch

### 1.15.4 Test File Corrupted Or Reverted

### 1.15.5 Build Succeeds But Tests Do Not Run

### 1.15.6 Setup Script Creates Projects Targeting The Wrong Framework

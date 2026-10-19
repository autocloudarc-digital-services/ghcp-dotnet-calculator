---
title: GitHub Codespaces Setup And Lifecycle Guide
description: Repository-maintained setup, validation, authentication, caching, troubleshooting, and cleanup guidance for completing every calculator and GitHub Actions exercise in Codespaces.
---

## Use A Learner-Owned Fork

Create the Codespace from your fork, not from the source repository. The fork
gives you control over branches, Actions, environments, variables, secrets,
reusable-workflow access, and repository settings used later in the labs.

1. Fork the repository into your personal account or an approved organization.
2. Open the fork and confirm its URL contains your owner name.
3. Select **Code**, **Codespaces**, and **Create codespace on main**.
4. Select a machine with at least 4 cores, 8 GB RAM, and 32 GB storage when
   prompted. Docker-backed tests and concurrent language services need this
   capacity.
5. Wait for the post-create task to report `Codespaces toolchain validation
   completed`.

Confirm that Git operations target your fork:

```bash
git remote -v
gh repo view --json nameWithOwner,defaultBranchRef
git status --short --branch
```

The `origin` URLs and `nameWithOwner` value must identify your fork. Add the
source repository as `upstream` only when you intend to synchronize later:

```bash
git remote add upstream https://github.com/multi-layer-perceptron/ghcp-dotnet-calculator.git
```

> [!IMPORTANT]
> A Codespace created from the source repository does not grant permission to
> push changes or configure its Actions settings. Recreate the Codespace from
> your fork rather than placing credentials in a changed remote URL.

## Confirm Account And Profile Preferences

Before the workshop, verify these GitHub account or organization settings:

* Codespaces creation is allowed for the fork and your account has available
  compute and storage quota
* GitHub Actions is allowed in the fork
* GitHub Copilot is assigned to your account and available in Codespaces
* Forking, branch creation, pull requests, Issues, and Discussions are allowed
  for exercises that use them
* Repository administration is available for environments, Actions variables,
  secrets, reusable-workflow access, and OIDC exercises
* An Azure subscription and permission to create federated credentials and
  role assignments are available only when you opt into Workflow 24

In **GitHub Settings**, review the Codespaces default machine type, idle
timeout, retention period, and spending limit. Organization-owned Codespaces
can enforce different values. Ask an administrator before the workshop when a
policy hides a required machine type, extension, port, feature, or GitHub
Actions setting.

## Understand The Prepared Environment

The repository's [.devcontainer/devcontainer.json](../.devcontainer/devcontainer.json)
builds the same Linux environment for browser-based and desktop-connected
Codespaces.

* Ubuntu Noble, the .NET 10 SDK, and .NET 8 runtimes support the staged
  calculator build, test, console, upgrade, and Blazor exercises
* Docker-in-Docker, Compose, and Buildx support PostgreSQL Testcontainers and
  local container cleanup
* Node.js LTS, npm, npx, and headless Chromium support Playwright and Memory MCP
  servers
* Python 3.12 supports Python helper and workflow-authoring exercises
* PowerShell 7 supports setup, hook, MCP, handoff, and reset scripts
* GitHub CLI and `gh-aw` support Actions, Issues, coding agent, and agentic
  workflows
* Azure CLI, Azure Developer CLI, and Bicep support Azure assessment and
  deployment preparation
* Terraform 1.15.6 matches Workflow 24 validation, plan, apply, and destroy
* `jq` and Git support structured CLI output and repository workflows

The configuration also installs Copilot Chat, C# Dev Kit, PowerShell, GitHub
Actions, GitHub Pull Requests, Containers, Bicep, Terraform, Azure, Playwright,
YAML, and Markdown extensions. It hides `src/completed/`, `bin/`, and `obj/`
from the Explorer so the finished solution and generated files do not distract
from the exercises.

GitHub Copilot upgrade (`ms-dotnettools.upgrade-agent`) is the current upgrade
extension; do not use the retired modernization extension ID/version. After
changing devcontainer extensions, rebuild and verify the active remote VS Code
profile. `code --list-extensions --show-versions` reports that environment, not
every client profile. A configured extension is not proof its native assessment
tools are exposed. Likewise, C# Dev Kit does not configure a separate CLI LSP;
use the `lsp-setup` skill if that host needs code intelligence.

The default terminal profile is Bash. Open **Terminal: Select Default Profile**
and choose PowerShell when an exercise uses a `powershell` code block or
PowerShell syntax such as `Get-Content`, `Test-Path`, or `Remove-Item`. The
selected profile affects new terminals only.

## Run The Environment Preflight

The post-create script validates required commands, checks the .NET 10 SDK,
.NET 8/10 runtimes, Bicep, Docker, and `gh-aw`, prepares Chromium for the pinned
Playwright MCP server and locked standalone browser runner, and restores the solution.
Rerun it after a rebuild or when diagnosing a partial setup:

```bash
bash .devcontainer/scripts/post-create.sh
```

Inspect versions when reporting a setup problem:

```bash
dotnet --list-sdks
dotnet --list-runtimes
docker version
node --version
npx --version
python3 --version
pwsh --version
gh --version
gh aw --version
az version
az bicep version
azd version
terraform version
```

`dotnet --version` identifies the selected SDK, while each `.csproj` declares
its actual target framework. SDK 10 plus .NET 8 runtimes can successfully run
an unupgraded `net8.0` solution. Read project metadata rather than treating the
toolchain inventory as proof the upgrade happened.

Configured MCP servers may not be exposed in every host. The
[browser-validation skill](../.github/skills/validate-calculator-with-playwright/SKILL.md#repeatable-extended-runner)
includes a standalone fallback for local, published Release, and Azure targets.
Use its locked SDK and matching browser installation; do not hardcode a cached
Chromium revision or assume the MCP and standalone packages need the same one.

Verify Docker and the full Testcontainers path before Exercise 01.04:

```bash
docker info
dotnet test src/workspace/calculator-xunit-testing/calculator.slnx
```

A Docker error is an environment failure, not a calculator test failure. If
`docker info` fails, rebuild the container. If it still fails, confirm the
Codespace has the required machine capacity and that organization policy allows
Docker-in-Docker.

## Authenticate Only When Required

Container creation installs clients but never signs into GitHub, Azure, an MCP
server, or a model provider. Authentication remains scoped to the learner and
must not be stored in the repository, dev container definition, terminal
history, logs, or screenshots.

Codespaces normally supplies GitHub CLI authentication. Verify the active
identity and repository before any command that creates secrets, Issues,
branches, pull requests, or workflow runs:

```bash
gh auth status
gh repo view --json nameWithOwner,url
```

Sign into Azure only for Azure exercises:

```bash
az login --use-device-code
az account show --output table
azd auth login --use-device-code
```

Select and verify the intended subscription before reading, creating, or
deleting resources. Workflow 24 authenticates its GitHub-hosted runner through
OIDC; an interactive Azure login in the Codespace does not replace the
repository's federated credentials or RBAC assignments.

Copilot Chat must show your signed-in GitHub account before Exercise 00.01 or
01.01. MCP servers can request separate browser authorization when first used.
Exercise 99.03 owns that server configuration and authorization flow.

## Enable And Trigger GitHub Actions

Open the **Actions** tab in your fork. Select **I understand my workflows, go
ahead and enable them** when GitHub displays the fork-safety prompt. If the
prompt or **Run workflow** button is unavailable, check the following:

* The workflow file is committed to the fork's default branch
* The workflow declares `workflow_dispatch`
* Your changes were pushed to the fork rather than left only in the Codespace
* **Settings**, **Actions**, **General** permits the required actions and
  reusable workflows
* Your account has write or administration permission required by the lesson
* Organization policy permits Actions and the requested runner type

Use a learner branch for edits, then commit and push the intended checkpoint:

```bash
git switch -c workshop/codespaces-labs
git add <reviewed-paths>
git commit -m "learn: complete exercise checkpoint"
git push --set-upstream origin workshop/codespaces-labs
```

Push-triggered workflows run only after the push. Manual dispatch runs the ref
selected in the Actions UI, but GitHub discovers a new `workflow_dispatch`
definition only after it exists on the default branch. Scheduled workflows run
from the default branch. Review every workflow diff before enabling or pushing
it because Actions executes repository code on remote runners.

## Run The Blazor And Browser Exercises

Bind the Blazor app to the configured Codespaces port:

```bash
cd src/workspace/calculator-xunit-testing/calculator.web
dotnet run --urls http://0.0.0.0:5000
```

Open the **Ports** panel and use **Open in Browser** for port 5000. Keep the
port visibility private unless an exercise explicitly requires another user to
reach it. Your desktop browser uses the forwarded `https://...app.github.dev`
URL; Playwright or another process running inside the Codespace uses
`http://localhost:5000`.

Stop the app with `Ctrl+C` before rebuilding, resetting, or deleting its files.

## Use Caches And Prebuilds Deliberately

Named volumes preserve NuGet packages, npm downloads, Playwright browser
binaries, and Docker image layers when the dev container is rebuilt. This
reduces repeated PostgreSQL, MCP, browser, and package downloads. The volumes
belong to the Codespace and disappear when the Codespace is deleted. They are
performance caches, not backups.

Use **Codespaces: Rebuild Container** after changing `devcontainer.json` or
when a feature is incomplete. Rebuilding preserves the repository and named
volumes. Use **Codespaces: Full Rebuild Container** when a stale image layer is
suspected; expect more downloads. Never solve a cache problem with an
unreviewed global Docker prune because the cache can contain active lab data.

Repository administrators who run repeated workshops can configure a
Codespaces prebuild for the default branch under **Settings**, **Codespaces**,
**Set up prebuild**. Select the expected regions and update cadence, review the
associated Actions and storage cost, and verify a newly created Codespace still
runs the post-create check. A prebuild may cache the base image and features;
it must not contain learner authentication, secrets, or deployed resources.

## Match Checkpoints To The Exercise Tracks

For the main lab track:

* Modules 00 and 01 require Copilot, .NET, Git, Docker, and a learner branch
* Module 02 adds port 5000, browser forwarding, Azure CLI, Azure Developer CLI,
  and Bicep
* Module 03 uses the same runtime plus security and quality-review tools
* Exercises 99.01 through 99.05 add PowerShell, customizations, Node-based MCP
  servers, hooks, and multiple agents
* Exercises 99.06 through 99.08 add GitHub CLI, `gh-aw`, repository secrets,
  Actions, Issues, coding agent access, and optional model-provider access
* Exercise 99.09 performs targeted Azure, Docker, workspace, memory, Git, and
  Codespace lifecycle cleanup

For the GitHub Actions workflow track:

* Workflows 01 through 21 run on GitHub-hosted runners; the Codespace is the
  authoring and Git client, not the runner
* Workflows 04 and 05 require fork-owned environments, variables, and secrets
* Workflows 22 and 23 require additional repositories and organization or
  enterprise policy; a single Codespace cannot grant that access
* Workflow 24 requires Azure OIDC federation, protected environments, remote
  Terraform state, scoped RBAC, cost review, and explicit destroy verification

## Preserve Work And End A Session

The files in the Codespace persist across browser disconnects, stops, and
container rebuilds, but uncommitted work is lost when the Codespace is deleted.
At every exercise checkpoint:

```bash
git status --short
git diff --check
git add <reviewed-paths>
git commit -m "learn: preserve exercise checkpoint"
git push
git status --short
```

The final status must be clean before deletion. When work should remain
uncommitted, export a reviewed patch to durable storage outside the Codespace
and verify that it can be retrieved instead of relying on `git push`.

Use the Codespaces menu on GitHub to stop compute when you finish a session.
Stopping ends compute charges, while storage charges and retention continue
according to your account or organization policy. Before deletion, push needed
commits or export and retrieve an explicitly reviewed patch, confirm Azure and
workflow resources have separate cleanup owners, then delete the Codespace.
Deleting the Codespace also deletes its uncommitted files and named cache
volumes; it does not delete pushed branches, Actions artifacts, secrets, Azure
resources, Terraform state, external caller repositories, or GitHub Issues.

## Troubleshoot By Layer

1. Run the post-create script to identify a missing binary or runtime.
2. Run `docker info` before investigating a Testcontainers failure.
3. Run `gh auth status` and `gh repo view` before investigating an Actions or
   repository-permission failure.
4. Confirm the workflow is on the expected remote ref before investigating a
   missing dispatch or trigger.
5. Check the **Ports** panel and the server bind address before investigating a
   browser or Playwright connection failure.
6. Run `az account show` and inspect the OIDC subject and RBAC scope before
   investigating Workflow 24 authentication.
7. Rebuild the container only after preserving `git status` output and any
   terminal evidence needed for diagnosis.

Record the Codespace machine type, container creation log, command version,
repository owner, failing command, and exact error when requesting help. Never
include access tokens, secret values, connection strings, or Terraform state.

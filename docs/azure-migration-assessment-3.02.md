---
title: Azure Migration Assessment Baseline 3.02
description: Manual Azure readiness assessment and separately approved .NET 10 web-only deployment, with validation evidence and native-tool limitations
ms.date: 2026-10-19
ms.topic: how-to
---

# Azure Migration Readiness Assessment (3.02)

The assessment sections below preserve the original pre-deployment snapshot.
The separately approved [implementation and deployment follow-up](#approved-implementation-and-deployment-follow-up)
records the later .NET 10 upgrade and live Azure deployment, and supersedes the
snapshot's framework, infrastructure, and resource-existence findings. It does
not substitute for the unavailable extension-native assessment.

## Assessment Status

**Overall result: partial success.** Repository inspection, Linux build/test
validation, and authenticated read-only Azure target checks succeeded. A new
extension-native Azure migration assessment could not be executed because its
assessment tools and report viewer are not exposed in this Copilot CLI session.
No new AppCAT action ID, report ID, or raw assessment report was generated.

**Readiness at assessment time: suitable for migration planning, not ready for
deployment using the existing template unchanged.** The projects targeted .NET 8, whereas the
prompt's assumed baseline and the App Service template specify .NET 10.

The assessment phase changed only this report. It did not modify application
code, dependencies, infrastructure, workflows, or cloud resources, and did not
run provisioning, deployment, migration execution, or Azure what-if commands.
Azure device-code authentication was performed with user approval; credentials,
authentication codes, and subscription identifiers are not included here.

## Intended Azure Target And Verified State

| Parameter | Requested or recommended value | Verification |
| --- | --- | --- |
| Subscription | User-selected subscription; identifier omitted | Azure CLI authenticated successfully; account state `Enabled`, cloud `AzureCloud` |
| Resource group | `rg-calculator` | `az group exists` returned `false`; no group was created |
| Region | `eastus2` | Returned by the subscription-scoped ARM locations API |
| Environment | `dev` | User supplied; no AZD environment was created or changed |
| App resource name | `calculator-app-0948` | App Service name availability returned `nameAvailable: true` |
| Hosting | Recommend Azure App Service on Linux | Fits the existing Blazor host and `azure.yaml`; recommendation, not deployment approval |
| Development SKU | B1, subject to cost/capacity approval | Linux B1 location catalog includes `East US 2`; quota and allocation capacity were not validated |
| Runtime | Recommend .NET 10 LTS after an approved upgrade | App Service Linux catalog lists `DOTNETCORE\|10.0`; current projects still target `net8.0` |
| Production data tier | None required for current application behavior | Web app has circuit-scoped memory state and no runtime PostgreSQL dependency |

Name availability is a point-in-time check, not a reservation. Resource creation
permissions, Azure Policy constraints, spending limits, regional capacity, and
the existence or configuration of older deployments were not established.
Successful read access does not prove permission to provision resources.

The requested app name denotes an App Service web app under this recommendation,
not an Azure Container App. The current Bicep template generates a different app
name and must be reconciled with the supplied target before any provisioning.

## Execution Context

| Item | Observed value |
| --- | --- |
| Date | 2026-10-19 |
| Repository HEAD | `494a75c`, with existing uncommitted calculator work |
| Workspace | `src/workspace/calculator-xunit-testing/` |
| OS / architecture | Ubuntu 24.04, Linux x64 |
| .NET SDK / MSBuild | `10.0.401` / `18.9.11` |
| .NET host | `10.0.12` |
| Installed SDKs | `10.0.401` only |
| Installed .NET and ASP.NET Core runtimes | `8.0.32` and `10.0.12` |
| `global.json` | Not found by `dotnet --info` |
| Azure CLI | `2.91.0` |
| Azure Developer CLI | `1.35.1`; CLI advertises an update to `1.36.0`, not installed in this run |
| Local web app | Already running at `http://localhost:5000`; `/` returned HTTP 200 and `/health` returned `Healthy` |
| Persistent PostgreSQL | `test-container`, `postgres:latest`, host port `5432`; not a migration source |
| Other container | Terraform MCP server container running `hashicorp/terraform-mcp-server:0.2`; left untouched |

The application and persistent database were left running. They did not
interfere with assessment: automated tests used an isolated Testcontainer with
dynamic port mapping. No local application or unrelated container was stopped.

Results describe the working tree, including currently untracked library, web,
and service-test files, not a reproducible clean checkout of HEAD alone.

## Assessment Tool Path And Missing Native Capabilities

The intended extension is
[GitHub Copilot upgrade](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.upgrade-agent),
extension ID `ms-dotnettools.upgrade-agent`, invoked in VS Code through the
**Upgrade** agent or `@upgrade`.

- `.devcontainer/devcontainer.json` includes that extension in its desired list.
- The connected remote inventory from `code --list-extensions --show-versions`
  did not list `ms-dotnettools.upgrade-agent`. This does not establish whether
  it exists in another VS Code profile or on the client side.
- The IDE MCP tool inventory exposes diagnostics and editor selection only.
  It does not expose upgrade execution, Azure precheck, Azure assessment, or a
  migration assessment report viewer.
- The workspace MCP configurations name Azure DevOps, GitHub, Microsoft Learn,
  Playwright, and memory servers; no Upgrade/App Modernization server is
  explicitly configured there. Extension-managed tools may be separate.
- Legacy `mcp_github_copilot_appmod-precheck-assessment`,
  `mcp_github_copilot_appmod-run-assessment`, and report-upload commands were
  unavailable, so they were not invoked under invented replacement names.
- `.github/upgrades/dotnet-upgrade-report.md` discusses different project names.
  It is not evidence that the current calculator projects have been upgraded,
  nor is it a new Azure migration assessment.

**Remediation:** install or enable the configured extension in the active VS
Code environment, select **Upgrade**, and inspect its actual exposed tools and
any installed Azure assessment extender. Confirm assessment-only capabilities
before executing them. The upgrade extension's presence alone does not prove
that legacy Azure assessment tools are available. Record exact command names,
raw report paths, and action/report IDs after a successful native run.

## Validation Commands And Results

Commands below ran from the repository root. Azure subscription identifiers in
the cloud command descriptions are intentionally omitted from this document.

| Command or check | Result |
| --- | --- |
| `git status --short` and `git rev-parse --short HEAD` | Existing application/refactor changes recorded and preserved |
| `dotnet --info` | SDK/runtime inventory captured above |
| `dotnet build src/workspace/calculator-xunit-testing/calculator.slnx --verbosity minimal` | Passed; zero warnings, zero errors |
| `dotnet test src/workspace/calculator-xunit-testing/calculator.slnx --verbosity minimal` | Passed; **43 passed, 0 failed, 0 skipped** |
| `dotnet list src/workspace/calculator-xunit-testing/calculator.slnx package` | Resolved direct package inventory captured below |
| `docker ps --format '{{.Names}}\t{{.Image}}\t{{.Ports}}'` | Persistent PostgreSQL and Terraform MCP containers observed |
| `ss -ltn '( sport = :5000 or sport = :5432 or sport = :55432 )'` | Listeners on `5000` and `5432`; none on `55432` |
| `curl --fail --silent --show-error http://localhost:5000/health` | Passed; `Healthy` |
| `curl --fail --silent --show-error --output /dev/null --write-out 'Home HTTP %{http_code}\n' http://localhost:5000/` | Passed; HTTP 200 |
| `code --list-extensions --show-versions` | Remote extension inventory inspected; Upgrade extension not listed |
| `az version` and `azd version` | Versions captured above |
| `az account show`, explicitly scoped to the supplied subscription | Initially failed because Azure CLI was not signed in; succeeded after user-approved authentication |
| `az login --use-device-code --output none` | User-approved sign-in started; authenticated ARM requests subsequently succeeded; pending login process stopped after verification |
| `az group exists --name rg-calculator`, explicitly subscription-scoped | Succeeded; `false` |
| `az provider show --namespace Microsoft.Web`, explicitly subscription-scoped | Succeeded; `Registered` |
| `az webapp list-runtimes --os linux`, explicitly subscription-scoped | Lists .NET 8, 9, and 10; .NET 8/9 marked near end of support |
| `az appservice list-locations --sku B1 --linux-workers-enabled`, explicitly subscription-scoped | Lists `East US 2` |
| ARM `Microsoft.Web/checknameavailability`, API `2024-04-01`, for the proposed site name | Succeeded; name available; request only checks availability |
| `az account list-locations --subscription ...` | CLI rejected the subscription argument; retried using the explicit ARM locations endpoint |
| ARM subscription `/locations`, API `2022-12-01` | Succeeded; confirms `eastus2` |

No Bicep compilation, Terraform validation, cloud deployment smoke test, fresh
browser interaction test, App Service recycle test, or scale-out test was run
as part of this assessment. Existing infrastructure was inspected, not changed
or declared deployment-validated.

### Actual Framework And Dependency Baseline

All four current projects target `net8.0`: `calculator`, `calculator.library`,
`calculator.web`, and `calculator.tests`. Building them with SDK 10 does not
retarget them to .NET 10. Test execution used the installed .NET 8 runtime.

The console and web hosts reference `calculator.library`; tests reference the
library and web project. No direct NuGet packages were reported for the
library, console, or web project.

| Test package | Requested and resolved version |
| --- | --- |
| `coverlet.collector` | `6.0.0` |
| `Microsoft.NET.Test.Sdk` | `17.5.0` |
| `Npgsql` | `8.0.8` |
| `Testcontainers.PostgreSql` | `4.8.1` |
| `xunit` | `2.6.2` |
| `xunit.runner.visualstudio` | `2.5.1` |

These versions passed the current .NET 8 baseline. Their suitability for a
future .NET 10 baseline requires a separate compatibility review and rerun;
package age alone is not evidence of a vulnerability or test failure.

## Prioritized Findings

### Critical Blockers

| ID | Finding and evidence | Required action before claiming readiness |
| --- | --- | --- |
| C1 | No new native Azure assessment ran; current tool inventory exposes neither assessment execution nor report viewing. The historical raw AppCAT report is absent. | Complete the extension-native assessment in an environment exposing its tools, or explicitly retain partial status and obtain agreement on the manual assessment scope. |
| C2 | Actual `.csproj` targets are `net8.0`, while `infra/main.bicep:16` defaults to `DOTNETCORE\|10.0`. No explicit major-version roll-forward or self-contained deployment was established. .NET 8 reaches end of support on 2026-11-10. | Prefer an approved upgrade to .NET 10 LTS, validate all projects and publish output, and align the App Service stack. Do not assume SDK 10 installation makes a .NET 8 app run on the .NET 10 stack. |

### Major Issues

| ID | Finding and evidence | Recommendation |
| --- | --- | --- |
| M1 | Blazor server interactivity requires a persistent SignalR connection. `infra/main.bicep:216` explicitly disables client affinity, and no `webSocketsEnabled` setting is declared. | Review Linux App Service WebSocket behavior and explicitly validate WebSockets, affinity, reconnect, and scale-out routing before deployment approval. Enable affinity for the direct App Service hosting design; do not assume in-memory circuits can move between workers. |
| M2 | Existing Bicep unconditionally provisions PostgreSQL, Key Vault, and database credentials. `calculator.web/Program.cs` registers only circuit-local calculator/history/theme services; no runtime database connection is consumed. | Recommend a web-only first deployment. Remove or gate unused data-tier resources in a later approved implementation, or obtain explicit agreement on a database-backed feature before incurring its cost and operational burden. |
| M3 | The resource-group-scoped template assumes a deployment group exists, while `rg-calculator` does not. AZD settings use environment-driven values and generated app naming, not the supplied name `calculator-app-0948`. | Reconcile the AZD resource group, environment, region, and generated/explicit service naming before provisioning. Do not silently deploy to a generated `rg-dev` or a different web app. |
| M4 | Numbered workflows are instructional. The dependency-cache workflow restores only; inspected workflows do not build/test/publish/deploy this web app as a release gate. New web/library/test files remain untracked in the current tree. | Preserve teaching examples. Plan a focused Linux CI/release workflow with Docker-capable tests, an immutable web publish artifact, OIDC, protected deployment approval, and post-deployment checks. Include all new project files in a future reviewed change. |
| M5 | A future data tier lacks schema migration, seed promotion, rollback, and validation scripts. Bicep uses password-based PostgreSQL access, an Azure-services firewall allowance, and a Key Vault Secrets Officer role for the application identity. | Conditional on adopting runtime PostgreSQL: define least-privilege database access and network scope, reduce Key Vault permissions to required reads, plan secret rotation, and validate migrations/backups/rollback. This is not a blocker for the recommended web-only app. |

For M1, missing configuration is a readiness gap, not evidence that a currently
deployed site has failed. No site exists in the requested resource group to
inspect, and no scale-out behavior was exercised.

### Informational Recommendations And Resolved Historical Concerns

- **Blazor disposal:** current `Home.razor:73-95` catches
  `JSDisconnectedException` and disposes its .NET callback reference in
  `finally`. The earlier missing-catch concern is addressed in source.
  Production reconnect/recycle behavior still needs a dedicated smoke test;
  no application hardening was performed in this assessment.
- **Test isolation:** `PostgreSqlTestData.cs` generates database/user/password
  values in memory, seeds the copied CSV through PostgreSQL COPY, and disposes
  its Testcontainer. Static `MemberData` providers query PostgreSQL and defer
  enumeration until execution. Keep this independent of persistent port 5432.
- **Database version parity:** the fixture pins `postgres:15.1`, while Bicep
  defaults to PostgreSQL 17 and the persistent local container uses a floating
  `latest` tag. Before adding a runtime database feature, choose a supported,
  serviced engine baseline and test its actual target version. No database
  images or engine versions were changed in this run.
- **Test-only I/O and sync bridge:** CSV access is confined to fixture seeding.
  `CalculatorTest.cs:46` still bridges async data loading synchronously for
  xUnit v2. Discovery enumeration is disabled. Monitor CI behavior rather than
  treating that intentional bridge as a production runtime blocker.
- **Session state:** history is capped at 50 entries and lives in the Blazor
  circuit. Refresh/reconnect/recycle behavior must be communicated; persistence
  or cross-device history would be a new feature, not a migration prerequisite.
- **Linux parity:** local Linux build/tests and HTTP startup passed. Keep path
  casing, copied CSV assets, Docker availability, and target runtime versions
  consistent in CI. The devcontainer includes .NET 8 runtimes in addition to its
  SDK 10 base, which explains why the mixed local baseline works.
- **SDK servicing:** no `global.json` pins the SDK. The installed .NET 10
  runtime is `10.0.12`; Microsoft's 2026-10-13 support table lists `10.0.13`.
  Review SDK/image servicing during the later toolchain upgrade, not here.
- **Static assets and observability:** serve the small current CSS/JavaScript
  assets directly from App Service initially. `/health` is a liveness check;
  existing Bicep references it and configures logs/Application Insights. Confirm
  actual telemetry ingestion and alerting after deployment.

## Recommended Azure Architecture

Recommend **Azure App Service on Linux**, deploying only `calculator.web` with
`calculator.library` included in its build output. Retain `calculator` as the
console sample and keep test assemblies and Testcontainers outside the runtime
deployment package.

For `dev`, consider B1 after the user approves cost and capacity. If deployment
slots are required for validation or rollback, select a tier that supports
them, such as Standard or higher; B1 does not provide that capability. Align
the app and App Service on .NET 10 LTS after the approved framework upgrade.
Validate WebSocket transport, session affinity, and circuit reconnection.

Azure Container Apps is a valid alternative if an explicit container packaging
or revision-scaling requirement emerges, but it adds container/revision
operations and Blazor affinity/data-protection considerations that are not
needed for this small app. No Container Apps deployment is recommended merely
because development uses Docker for automated tests.

Do not provision PostgreSQL solely for arithmetic or ephemeral history. If
durable history or a shared catalog is approved later, use Azure Database for
PostgreSQL Flexible Server with a reviewed schema, least-privilege access,
private or tightly controlled networking, migration/rollback tests, and a
deliberate backup policy. Local and Testcontainers databases are fixtures,
not production migration sources.

### Configuration And Secret Mapping

| Concern | Local development | CI | Azure App Service recommendation |
| --- | --- | --- | --- |
| Target framework | Currently .NET 8, SDK 10 plus .NET 8 runtimes | Match the actual test runtime; move to .NET 10 only with the approved upgrade | Align deployed framework and Linux runtime stack |
| Environment | Current local server runs with production middleware on HTTP | Explicit test/build configuration | `ASPNETCORE_ENVIRONMENT=Production` |
| Listening endpoint | `http://localhost:5000`; expected HTTP-only HTTPS-port warning | Ephemeral test endpoint when needed | Platform-managed HTTPS/custom domain; do not copy local URL/port settings |
| History/theme | Scoped circuit memory | Isolated service tests | Scoped circuit memory; no database secret needed |
| Automated PostgreSQL | Testcontainers with randomized credentials and dynamic ports | Same isolated pattern on Docker-capable runner | Not deployed with the web host |
| Optional future runtime PostgreSQL | Separate explicitly configured local integration | Isolated integration environment | Managed identity where supported; otherwise reviewed Key Vault references, never Docker credentials |
| Azure authentication | User-approved Azure CLI device-code sign-in | Federated OIDC identity | Managed identity with minimal required roles |
| Secrets | Secure local/environment store; none needed by current web app | Protected CI/environment secrets only when required | Key Vault for actual secrets; non-secret settings in App Settings |
| Observability | Console logging and `/health` | Test logs and published results | App Service diagnostics, Application Insights, health monitoring, and reviewed retention |

Existing `ConnectionStrings__CalculatorPostgreSql` configuration is not consumed
by the current app. If retained for a future feature, review its version-pinned
Key Vault reference and 90-day secret expiration together with the password
rotation/redeployment procedure; expiration alone does not implement rotation.

## Phase-Based Next Steps At Assessment Time

### Phase A: Close Blockers

1. Enable the Upgrade extension and Azure assessment capabilities in the active
   environment. Run assessment-only prechecks and analysis; capture raw report
   paths and IDs or retain the explicit missing-tool log.
2. Reconcile live metadata with the intended .NET 10 target. After approval,
   upgrade the actual four calculator projects and compatible packages, then
   build/test/publish against the selected supported stack.
3. Confirm web-only App Service hosting, the requested resource group/name,
   environment/region, cost limit, and SKU. Do not create anything until the
   user separately approves provisioning.
4. Preserve the existing typed Blazor disposal handling. Add targeted
   reconnect/recycle validation before considering its production behavior
   verified; do not reapply an already-present source fix.

### Phase B: Implement Migration Prerequisites

1. In a separately approved change, adapt the existing `infra/` assets rather
   than creating competing templates. Align runtime, naming/resource group,
   affinity, and optional data-tier provisioning with the agreed topology.
2. Carry the environment mapping above into deployment configuration. Review
   Key Vault access, managed identity, RBAC, network restrictions, secret
   rotation, logging costs, and retention for resources actually required.
3. Review test/SDK versions for the selected framework. Preserve ephemeral
   Testcontainers credentials, runtime seeding, invariant numeric parsing,
   malformed-row diagnostics, and discovery-time isolation.
4. If runtime PostgreSQL is approved, define versioned schema migration,
   seed promotion, validation, backup/restore, and rollback artifacts. Otherwise
   explicitly defer those data-tier tasks and do not request a database password.
5. Plan Linux build/test/publish gates and a reviewed immutable deployment
   artifact. Keep the numbered workflow teaching examples intact.

### Phase C: Validate Pre-Deployment Readiness

1. Validate the agreed Bicep or Terraform configuration locally. With separate
   approval and appropriate access, run a later deployment preview or Azure
   what-if. Neither was run in this assessment.
2. Confirm provider registration, policy, quota/capacity, effective creation
   permissions, final name availability, pricing, and teardown ownership.
3. Establish deployment and rollback gates; use a suitable App Service tier if
   the plan depends on deployment slots.
4. After an approved deployment, smoke-test HTTPS `/health` and `/`, then use
   browser interaction to verify rendered `7 + 3 = 10`, keyboard input, history
   replay, theme changes, WebSockets, reconnect/recycle, and multiple clients.
5. If introducing a database, rehearse schema cutover and rollback separately;
   do not migrate data from a disposable automated test container.

## Approved Implementation And Deployment Follow-Up

### Approval And Deployed Scope

On 2026-10-19, after the assessment, the user separately approved upgrading,
validating, provisioning, and deploying the web-only recommendation. The approved
scope was one single-instance Linux B1 App Service in the supplied subscription.
No database, Key Vault, or paid external telemetry resources were approved or
created.

| Item | Observed deployment |
| --- | --- |
| Resource group | `rg-calculator`, `eastus2` |
| Environment | `dev` |
| App Service plan | `calc-dev-w7pdszkip2ps6-plan`, Linux B1, capacity `1` |
| Web app | `calculator-app-0948` |
| Live URL | <https://calculator-app-0948.azurewebsites.net> |
| Runtime | `DOTNETCORE\|10.0`; all four active projects target `net10.0` |
| ARM deployment | `calculator-web-net10`, provisioning state `Succeeded` |
| Application deployment | OneDeploy `d9791739-1509-4a0c-b1a8-e48956e84177`, status `4`, complete `true`, active `true` |
| Resource inventory | Exactly two top-level resources: `Microsoft.Web/serverFarms` and `Microsoft.Web/sites` |
| Additional configuration | Disabled SCM/FTP basic publishing policies and bounded filesystem logging |

The resources remain deployed. The B1 plan incurs charges even when its web app
is stopped. No automatic teardown or spending cap was configured.

### Implemented Changes

- Retargeted the console, library, web, and test projects to .NET 10 and removed
  the test project's unsupported-framework suppression.
- Updated the test dependencies to `coverlet.collector` `10.1.0`,
  `Microsoft.NET.Test.Sdk` `18.11.0`, `Npgsql` `10.0.3`,
  `Testcontainers.PostgreSql` `4.16.0`, `xunit` `2.9.3`, and
  `xunit.runner.visualstudio` `4.0.1`. Adopted the supported PostgreSQL builder
  constructor without changing randomized credentials, CSV seeding, isolation,
  or the existing test-only PostgreSQL image.
- Switched web static assets to .NET 10 `MapStaticAssets`. Published browser
  validation exposed stale history rendering: the child component received an
  unchanged callback but no changing state parameter. Passing the current
  entries and clear/replay callbacks explicitly now updates history immediately.
- Adapted the existing Bicep and regenerated ARM JSON for web-only hosting,
  explicit app naming, one worker, affinity, WebSockets, HTTPS, TLS 1.2 minimum,
  Always On, `/health`, and production run-from-package settings. Removed
  unused database credentials, connection strings, Key Vault, identities,
  Application Insights, and Log Analytics from the default template.
- Added required `AZURE_WEB_APP_NAME` substitution to the AZD parameter file.
  Provisioning used explicit Azure CLI parameters, not unexpanded AZD values.
  Existing teaching workflows and Terraform exercise assets were not changed.

### Release Artifact And Validation

Published only the Release web project and its library into a session-local
directory, then zipped its contents at the archive root. The package contains
the two application assemblies and static assets, not console/test assemblies,
Npgsql, Testcontainers, or CSV fixtures. Cloud deployment used the same package
validated locally, with build automation disabled.

| Artifact | Value |
| --- | --- |
| File | `calculator-web-net10.zip` |
| Size | `385201` bytes |
| SHA-256 | `dd27b922c935584e26726ce0a70aabd01ffbe0200111f94b5f3d6aee0299f1fa` |

| Command or check | Result |
| --- | --- |
| `dotnet build src/workspace/calculator-xunit-testing/calculator.slnx --configuration Release --no-restore --verbosity minimal` | Passed; zero warnings, zero errors |
| `dotnet test src/workspace/calculator-xunit-testing/calculator.slnx --configuration Release --no-build --verbosity minimal` | Passed on .NET 10; **43 passed, 0 failed, 0 skipped** |
| `dotnet list src/workspace/calculator-xunit-testing/calculator.tests/calculator.tests.csproj package --vulnerable --include-transitive` | No vulnerable packages reported by the configured NuGet source; includes transitive packages |
| `dotnet publish src/workspace/calculator-xunit-testing/calculator.web/calculator.web.csproj --configuration Release --no-build --no-restore`, with session-local output | Passed; framework-dependent `net10.0` artifact |
| `az bicep build --file infra/main.bicep` | Passed; regenerated `infra/main.json` |
| `az deployment group what-if`, explicit approved subscription/group and `environmentName=dev location=eastus2 webAppName=calculator-app-0948 appServicePlanSkuName=B1` | Succeeded; five creates only: plan, site, two publishing policies, and logging configuration; no changes/deletes outside this scope |
| `az deployment group create`, same target and parameters, Incremental mode | Succeeded |
| `az webapp deploy`, ZIP upload with build automation disabled and startup tracking enabled | Upload/OneDeploy succeeded, but CLI exited `1` after its ten-minute startup tracker reported failure; independent runtime and browser verification passed |
| ARM resource inventory, plan, site configuration, publishing policies, and log configuration | Matched the approved topology; runtime `.NET 10`, HTTPS, affinity, WebSockets, and both basic publishing policies disabled |
| HTTPS `/health` and `/`; HTTP `/health` | `Healthy`, HTTP 200, and HTTP 301 redirect to HTTPS respectively |
| Published Release browser checks at `http://localhost:5297` | Passed using the exact packaged output |
| Live Azure browser checks | Passed; details below |

Playwright verified the actual rendered `7 + 3 =` display was exactly `10`, and
keyboard `4 * 5 =` produced `20`. It also verified immediate history rendering,
replay and clear, dark mode, separate-client isolation, and a 320-pixel viewport
without horizontal overflow. Static scripts/styles loaded without HTTP errors,
and no page exceptions occurred.

For actual circuit reconnection, Playwright passed WebSocket messages through
to the real server, deliberately closed the transport, and required a new
WebSocket before continuing. Calculation history and dark mode survived that
interruption, and calculator input still worked. Full page refresh created a
new circuit with empty history and the default theme. Both local published
Release and Azure runs observed three connections: initial, reconnect, refresh.
This was transport-reconnect testing, not a worker-recycle or scale-out test.

### Deployment Tracking Caveat

The Azure CLI startup poll stayed at `Starting` and ultimately reported that the
worker failed to start within ten minutes. This must not be recorded as a
successful CLI command. Kudu's OneDeploy log explicitly reported
`Deployment successful`, and the deployment remained complete and active.

Startup diagnostics showed a transient `Container failed to remount volume`
message during the deployment recycle, followed by a successful startup probe.
After the CLI timeout, `az webapp troubleshoot status` reported the current
worker `Started`, no current error, three successful startups, and zero failed
startups in its summary. HTTPS health remained `Healthy`, and the complete cloud
browser harness was rerun after the timeout and passed again, exercising the
deployed application rather than a default hosting page.
The discrepancy's underlying platform/tracker cause was not established. No
application workaround, basic-auth enablement, or unrequested SKU change was
introduced to hide the error.

Future automation must reconcile the deployment record with actual worker
health and browser smoke checks rather than treating either the CLI startup
tracker or ZIP-upload success alone as conclusive.

### Remaining Limitations

The extension-native assessment remains unavailable, so the assessment status is
still partial despite successful manual deployment and browser validation.
No clean-checkout CI/release workflow, production load test, scale-out test,
worker-recycle test, deployment slot, or automatic rollback was implemented.
B1 has no deployment slots; rollback would require redeploying a retained known
good package. The original ZIP and one-off browser harness were session-local validation
artifacts, not a durable release archive. The later source-artifact follow-up
persisted a [locked, reusable browser runner](../.github/skills/validate-calculator-with-playwright/SKILL.md#repeatable-extended-runner)
and a separate [approval-gated deployment skill](../.github/skills/deploy-calculator-to-azure/SKILL.md).
The ZIP itself still needs durable retention before serving as a release archive.

History/theme are not durable and do not survive a new circuit or worker
replacement. `/health` is liveness only. Logs are filesystem-only: application
level `Information`, HTTP retention three days/35 MB; no external alerting was
configured. Automated database tests still use the existing `postgres:15.1`
image, whose container servicing remains separate from the NuGet audit.

The pre-existing local server at `http://localhost:5000` and persistent
PostgreSQL container were preserved. Only the temporary published-output
validation server was stopped after verification. No Git commit was created.

## Historical Baseline And Missing Artifact Log

The previous 2026-07-07 report described a Windows/.NET 10 run with 37 passing
tests, AppCAT action `20260707082945`, report `report-20260707082945`, three
issues/four incidents, and an earlier development deployment. Those are
historical claims, not results of this assessment.

| Artifact or capability | Current status |
| --- | --- |
| Refreshed summary | This file: `docs/azure-migration-assessment-3.02.md` |
| New native precheck/assessment execution | Unavailable in the current tool session |
| New action ID / report ID / raw AppCAT output | Not generated |
| Report viewer / upload tool | Not exposed; no upload destination supplied |
| Historical raw report | Exact previously cited `src/workspace/calculator-xunit-testing/.github/modernize/assessment/reports/report-20260707082945/report.json` path is absent |
| Historical AppCAT findings | `Local.0003`, `Perf.0001`, and `Scale.0001` were previously reported; not rescanned or reissued as fresh findings |
| Cloud deployment proof during the assessment phase | Not obtained then; the later approved deployment and evidence are recorded above |
| Deployment plan, preview, or IaC changes during the assessment phase | Not generated then; the later approved implementation is recorded above |
| Database migration / seed promotion / rollback artifacts | Not found in inspected active workspace or infrastructure paths; conditional on a future runtime database |
| Standalone raw validation logs | Not written; results are summarized above from this session's tool output |

## Authoritative References

- [GitHub Copilot upgrade extension and supported scenarios](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.upgrade-agent)
- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core):
  .NET 8 support ends 2026-11-10; .NET 10 LTS support ends 2028-11-14.
- [Server-side Blazor hosting and deployment](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/server?view=aspnetcore-10.0):
  WebSockets, session affinity, App Service, and Container Apps considerations.
- Repository evidence: [Azure service mapping](../azure.yaml),
  [Bicep source](../infra/main.bicep),
  [web startup](../src/workspace/calculator-xunit-testing/calculator.web/Program.cs),
  [Blazor lifecycle](../src/workspace/calculator-xunit-testing/calculator.web/Components/Pages/Home.razor),
  [PostgreSQL fixture](../src/workspace/calculator-xunit-testing/calculator.tests/PostgreSqlTestData.cs),
  [test project](../src/workspace/calculator-xunit-testing/calculator.tests/calculator.tests.csproj),
  and [dependency-cache teaching workflow](../.github/workflows/15-dependency-cache-workflow.yml).

---
name: deploy-calculator-to-azure
description: "Provision and deploy the .NET calculator to web-only Linux Azure App Service after explicit approval, with immutable artifacts and independent readiness checks. Use when: deploy calculator to Azure, provision calculator App Service, publish calculator.web, troubleshoot calculator Azure startup tracking, execute approved Azure migration."
---

# Deploy Calculator To Azure

## Approval And Scope

This is the implementation/deployment follow-up to assessment-only prompt
`3.02`, not an automatic continuation. Ask for missing subscription, resource
group, region, environment, globally unique web app name, hosting SKU, and
authentication decisions. Obtain explicit approval for billable provisioning.
Do not infer approval or reuse a prior session's target from a report.

The current default is one B1 Linux App Service worker hosting only
`calculator.web` and its library. No runtime database, Key Vault, identity, or
paid telemetry is needed for arithmetic and circuit-local history/theme.
Ask before adding them, changing region/SKU, taking over unexpected resources,
enabling basic publishing authentication, or deleting anything. B1 remains
billable when its app is stopped and has no deployment slots.

## Inspect And Authenticate

1. Read the working tree, live project targets, `azure.yaml`,
   `infra/main.bicep`, and `infra/main.parameters.json`. Preserve unrelated
   changes and healthy local apps/containers.
2. Check Azure CLI authentication for the selected subscription. Ask before
   interactive sign-in; honor MFA/Conditional Access. Never log out or clear
   existing authentication automatically. Keep credentials and login codes
   out of source, logs, and reports.
3. Use explicit `--subscription` on supported Azure commands rather than
   changing global context. For subscription locations use the ARM locations
   endpoint if the installed CLI does not support a subscription argument.
4. Check provider registration, app-name availability, existing resource
   ownership, runtime catalog, region/SKU availability, and effective access.
   Catalog/name checks do not guarantee quota or allocation capacity.

## Build One Release Artifact

1. Verify coherent `net10.0` targets across all participating projects. SDK 10
   alone is insufficient. If an upgrade is required, obtain approval and follow
   prompt `3.01`; do not hide framework warnings.
2. Build/test Release with Docker available for the isolated PostgreSQL fixture.
   Audit direct and transitive NuGet packages after dependency changes. Report
   actual results; container image servicing is separate from NuGet auditing.

   ```bash
   dotnet build src/workspace/calculator-xunit-testing/calculator.slnx --configuration Release
   dotnet test src/workspace/calculator-xunit-testing/calculator.slnx --configuration Release --no-build
   ```

3. Publish only `calculator.web` into a new session-local staging directory.
   Validate that exact published output with
   [the browser skill](../validate-calculator-with-playwright/SKILL.md).
   Confirm static scripts/styles, immediate history updates, keyboard, theme,
   and interactive circuits, not merely HTTP 200.
4. ZIP the contents at the archive root, not an enclosing publish folder.
   Confirm framework/runtime metadata, web/library assemblies, and static
   assets; exclude console/tests, CSV fixtures, database tooling, and secrets.
5. Calculate SHA-256. Deploy the same immutable package without a remote build.
   Retain a known-good package durably for a real release; a session directory
   alone is not a release archive or a reliable long-term rollback strategy.

## Preview And Provision

The AZD contract requires `AZURE_ENV_NAME`, `AZURE_LOCATION`, and custom
`AZURE_WEB_APP_NAME` substitution. Configure the intended
`AZURE_SUBSCRIPTION_ID` and `AZURE_RESOURCE_GROUP` in the selected AZD environment
as well. Never pass the literal `${...}` AZD parameter file unexpanded to a
direct Azure CLI deployment.

For direct CLI use, supply explicit Bicep values. All variables below must come
from the user's confirmed target, not repository defaults:

```bash
az bicep build --file infra/main.bicep
az deployment group what-if \
  --subscription "$AZURE_SUBSCRIPTION_ID" \
  --resource-group "$AZURE_RESOURCE_GROUP" \
  --template-file infra/main.bicep \
  --parameters environmentName="$AZURE_ENV_NAME" location="$AZURE_LOCATION" \
    webAppName="$AZURE_WEB_APP_NAME" appServicePlanSkuName="$APP_SERVICE_SKU" \
  --mode Incremental
```

The template is resource-group scoped: the approved group must exist before
group what-if. If absent, create only that approved group first. If it exists
unexpectedly, verify ownership/scope before proceeding. Review the preview for
only the agreed plan, site, publishing policies, and logs, with no unrelated
changes/deletes or data tier. Do not silently delete an existing older database
deployment to make it match this template.

After review, run the group deployment in Incremental mode using the same
explicit target/parameters. Retrieve the real hostname from Azure outputs/site
properties instead of assuming the app-name hostname pattern.

## Deploy And Establish Readiness

1. Upload the verified ZIP with `az webapp deploy --type zip` and the explicit
   subscription/group/name. Keep build automation off for the prebuilt package
   and SCM/FTP basic publishing authentication disabled. Use supported Microsoft
   Entra authentication, not publishing-profile secrets.
2. Capture the command's real exit code, deployment ID, and logs. ZIP acceptance
   is not readiness; CLI startup tracking can also disagree with actual worker
   state. Never turn either into a success-shaped fallback.
3. If tracking fails or times out, inspect `az webapp log deployment list`,
   `az webapp log deployment show`, `az webapp log startup show`, and
   `az webapp troubleshoot status` when available. Record unavailable diagnostic
   commands explicitly; do not invent substitutes or enable basic auth.
4. Reconcile the active/completed OneDeploy record, latest startup probe/current
   worker status, HTTPS `/health` and `/`, and a browser run **after** any
   reported timeout. Preserve the nonzero command result even if independent
   evidence establishes a working deployment. Do not assume a platform/tracker
   root cause from one transient mount message or redeploy a healthy app merely
   to obtain a green CLI status.
5. Verify actual plan SKU/capacity/region, .NET stack, production settings,
   HTTPS/TLS, affinity, WebSockets, health path, logs, publishing policies, and
   resource inventory. Do not claim quota/cost/permissions that were not tested.
6. Use the browser skill against the actual HTTPS URL. Verify rendered
   `7 + 3 = 10`, history/keyboard/theme, isolation, and real WebSocket recovery.
   An offline toggle alone does not prove the transport disconnected.

## Evidence And Remaining Work

Update the existing assessment report with a separate approved follow-up; keep
the original assessment snapshot and missing-native-tool status. Record actual
scope, URL, artifact hash, commands/results, diagnostics/caveats, ongoing cost,
and rollback limitations. Omit subscription/account identifiers and secrets.

Distinguish transport reconnect from full refresh/worker replacement, which
loses circuit-local state. Stop only validation servers created by this run.
Do not stop pre-existing local services, delete approved cloud resources, or
create a Git commit unless separately requested.

For production-oriented follow-up, recommend a clean-checkout Linux CI gate,
Docker-backed tests, durable immutable artifact retention, OIDC, protected
deployment approval, health/browser gates, and reviewed rollback/monitoring.
These are recommendations, not permission to expand an educational deployment.

See [the observed deployment and tracking caveat](../../../docs/azure-migration-assessment-3.02.md#deployment-tracking-caveat)
for provenance; its resource names and test counts are evidence, not defaults.

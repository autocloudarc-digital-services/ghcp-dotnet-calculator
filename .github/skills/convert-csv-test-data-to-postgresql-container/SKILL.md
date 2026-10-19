---
name: convert-csv-test-data-to-postgresql-container
description: "Seed a persistent local PostgreSQL container from calculator CSV data with secure prompting and row-count rerun checks. Use when: manually seed local PostgreSQL, inspect calculator database contents, convert CSV test data to a PostgreSQL container."
license: MIT
disable-model-invocation: true
user-invocable: true
compatibility: PowerShell 7+, Docker daemon access, and a readable TestCases.csv file.
---

# Convert CSV Test Data to PostgreSQL Container

Create a PostgreSQL database container and import CSV test data into it.

## Purpose and positioning

Use this opt-in skill for manual persistent PostgreSQL exercises or an explicitly
approved database-backed feature. The current Blazor calculator runtime does not
need it. Model invocation is intentionally disabled; if the host does not expose
a callable skill, read this guide and review the script before executing the
user-requested workflow rather than claiming a nonexistent invocation.

Use prompt `2.03-switch-test-data-to-pg` for automated test isolation. That flow uses Testcontainers with per-run randomized credentials and avoids fixed host port assumptions.

Why this split exists:

* Persistent container workflow supports stable connection settings for manual database exercises or future approved database-backed features.
* Testcontainers is preferred for CI and routine automated test runs because it is isolated, reproducible, and avoids plaintext credential examples.

## Automated Option (Recommended)

Use the script in `.\scripts\Convert-CsvToPostgreSQL.ps1` to automate container setup and CSV import.

### Script Location

* `.\scripts\Convert-CsvToPostgreSQL.ps1`

### Prerequisites

* PowerShell 7+
* Docker Desktop running
* CSV test data file available at `$(git rev-parse --show-toplevel)\src\workspace\calculator-xunit-testing\calculator.tests\TestCases.csv`

### Example Usage

```pwsh
pwsh .\scripts\Convert-CsvToPostgreSQL.ps1
```

If `-PostgresPassword` is not provided, the script securely prompts for it.

### Script Parameters

* `-CsvPath`: Path to the CSV file. Default workflow path: `$(git rev-parse --show-toplevel)\src\workspace\calculator-xunit-testing\calculator.tests\TestCases.csv`
* `-ContainerName`: PostgreSQL container name (default: `test-container`)
* `-DatabaseName`: Target database name (default: `test_db`)
* `-TableName`: Target table name for import (default: `test_data`)
* `-PostgresUser`: PostgreSQL user name (default: `postgres_user`)
* `-PostgresPassword`: Secure password value; if omitted, script prompts securely
* `-HostPort`: Host port mapped to container port `5432` (default: `5432`)
* `-Image`: PostgreSQL Docker image tag (default: `postgres:latest`)
* `-ForceRecreate`: Recreates an existing container with the same name

### What the Script Does

1. Validates Docker availability and CSV file path.
2. Creates or starts the PostgreSQL container.
3. Waits for PostgreSQL readiness.
4. Creates a table from CSV headers (as `TEXT` columns).
5. Performs an idempotency check by comparing CSV row count and current table row count.
6. If counts match, skips import and reports data is already seeded.
7. If counts differ, truncates the target table and re-imports rows using `COPY`.
8. Prints imported row count for verification.

### Idempotent Re-runs

* Re-running the script does not create duplicate rows.
* Matching row counts skip import.
* Mismatched row counts trigger deterministic reset (`TRUNCATE`) and clean re-import.

### Troubleshooting

* Port already in use (`5432`): use `-HostPort 5433`.
* Container exists with old settings: inspect ownership/settings and ask before `-ForceRecreate`, which removes the existing container/data.
* Docker not running: start Docker Desktop and run the script again.
* Invalid CSV headers: ensure headers are present and not empty.

## Security guidance

* Do not hardcode passwords in scripts, prompts, or docs.
* Prefer interactive secure prompt behavior for local usage.
* For automation, pass secrets via a secure environment or secret store.
* Docker retains environment credentials in the persistent container's
  configuration; avoid sharing raw `docker inspect` output. This differs from
  per-run Testcontainers credentials.
* Row-count rerun checks do not compare CSV contents. Equal-size data edits can
  leave stale rows; inspect values and obtain approval before resetting data.
  Prefer a supported, serviced, explicitly selected image for repeatability,
  but do not change an existing database engine as an incidental cleanup.

### Quick Verification Command

```pwsh
docker exec test-container psql -U postgres_user -d test_db -c "SELECT COUNT(*) FROM test_data;"
docker exec test-container psql -U postgres_user -d test_db -c "SELECT * FROM test_data;"
```

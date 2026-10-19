#!/usr/bin/env bash
# Copyright (c) Microsoft Corporation.
# SPDX-License-Identifier: MIT
#
# Validate the Codespaces toolchain and warm the solution package cache.

set -euo pipefail

readonly SOLUTION_PATH="src/workspace/calculator-xunit-testing/calculator.slnx"
readonly PLAYWRIGHT_VERSION="1.52.0-alpha-2025-03-26"
readonly BROWSER_TOOLS_PATH=".github/skills/validate-calculator-with-playwright"

require_command() {
  local command_name="$1"

  if ! command -v "${command_name}" >/dev/null 2>&1; then
    printf "ERROR: Required command '%s' is unavailable.\n" "${command_name}" >&2
    return 1
  fi
}

retry_command() {
  local attempt

  for attempt in 1 2 3; do
    if "$@"; then
      return 0
    fi

    printf "Command failed on attempt %s of 3: %s\n" \
      "${attempt}" "$*" >&2
  done

  return 1
}

main() {
  local required_commands=(
    az
    azd
    bash
    docker
    dotnet
    gh
    git
    jq
    node
    npm
    npx
    pwsh
    python3
    terraform
  )

  mkdir -p \
    "${HOME}/.cache/ms-playwright" \
    "${HOME}/.nuget/packages" \
    "${HOME}/.npm"
  sudo chown -R \
    "$(id -u):$(id -g)" \
    "${HOME}/.cache" \
    "${HOME}/.nuget" \
    "${HOME}/.npm"

  printf "Validating the Codespaces toolchain...\n"
  for command_name in "${required_commands[@]}"; do
    require_command "${command_name}"
  done

  dotnet --list-sdks | grep -q '^10\.' || {
    printf "ERROR: The .NET 10 SDK is unavailable.\n" >&2
    return 1
  }
  dotnet --list-runtimes | grep -q '^Microsoft.NETCore.App 10\.' || {
    printf "ERROR: The .NET 10 runtime is unavailable.\n" >&2
    return 1
  }
  dotnet --list-runtimes | grep -q '^Microsoft.AspNetCore.App 10\.' || {
    printf "ERROR: The ASP.NET Core 10 runtime is unavailable.\n" >&2
    return 1
  }
  dotnet --list-runtimes | grep -q '^Microsoft.NETCore.App 8\.' || {
    printf "ERROR: The .NET 8 runtime is unavailable.\n" >&2
    return 1
  }
  dotnet --list-runtimes | grep -q '^Microsoft.AspNetCore.App 8\.' || {
    printf "ERROR: The ASP.NET Core 8 runtime is unavailable.\n" >&2
    return 1
  }

  az bicep version >/dev/null
  docker version >/dev/null

  if ! gh aw --version >/dev/null 2>&1; then
    retry_command gh extension install github/gh-aw
  fi
  gh aw --version >/dev/null
  retry_command \
    npx -y "playwright@${PLAYWRIGHT_VERSION}" install --with-deps chromium
  # The standalone runner and MCP can require different Chromium revisions.
  retry_command npm ci --include=dev --prefix "${BROWSER_TOOLS_PATH}" --no-audit --no-fund
  retry_command \
    npm exec --prefix "${BROWSER_TOOLS_PATH}" -- playwright install chromium

  if [[ -f "${SOLUTION_PATH}" ]]; then
    dotnet restore "${SOLUTION_PATH}"
  fi

  printf "Codespaces toolchain validation completed.\n"
  printf "Authentication remains user-scoped. Run 'gh auth status' and "
  printf "'az account show' before exercises that use those services.\n"
  printf "Verify GitHub Copilot upgrade in the active VS Code profile; "
  printf "configured extensions/MCP servers do not prove tools are exposed.\n"
}

main "$@"
---
title: Contributing to ghcp-dotnet-calculator
description: Contribution scope, workflow, validation, and community expectations for the calculator learning repository.
---

## Welcome

Thank you for helping improve this GitHub Copilot learning lab. Focused bug
fixes, documentation corrections, accessibility improvements, lab refinements,
and well-scoped features are welcome.

## Choose The Right Channel

- Use [Discussions](https://github.com/multi-layer-perceptron/ghcp-dotnet-calculator/discussions) for questions, ideas, and substantial proposals.
- Use [Issues](https://github.com/multi-layer-perceptron/ghcp-dotnet-calculator/issues) for reproducible defects and actionable documentation problems.
- Follow [SECURITY.md](SECURITY.md) for private vulnerability reports.
- Follow [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) for private conduct reports.

Discuss broad changes before implementation so maintainers and contributors can
agree on scope, learner impact, and validation. Small, focused corrections may
proceed directly through an Issue and pull request.

## Contribution Workflow

1. Search existing Discussions, Issues, and pull requests.
2. Open the appropriate issue form or Discussion when prior alignment is needed.
3. Fork the repository and create a focused branch.
4. Read [AGENTS.md](AGENTS.md) and `.github/copilot-instructions.md` before editing.
5. Preserve deliberate exercise stages and unrelated user work.
6. Add or update tests and documentation when behavior changes.
7. Run the relevant validation below.
8. Open a pull request using the repository template.

## Validation

For calculator changes after prompt `3.01` (.NET 10):

```powershell
dotnet restore src/workspace/calculator-xunit-testing/calculator.slnx
dotnet build src/workspace/calculator-xunit-testing/calculator.slnx --no-restore
dotnet test src/workspace/calculator-xunit-testing/calculator.slnx --no-build
```

For the initial .NET 8 stage, use `calculator.sln` in those commands with
SDK 8, running inside the generated workspace so its `global.json` applies.
The Dev Container installs both SDKs; SDK 10 selection, framework
upgrade through GitHub Copilot upgrade, and `.slnx` migration belong to prompt
`3.01`. Preserve the stage owned by each exercise rather than retargeting all
examples to the latest SDK.

For Markdown and customization changes:

```powershell
git diff --check
```

Run the narrowest relevant check first. If a check cannot run, explain why and
provide the strongest available evidence.

## Pull Request Expectations

- Keep the change focused and exclude unrelated formatting or generated noise.
- Explain what changed, why, and which exercises or learner outcomes are affected.
- Include validation commands, results, and screenshots for visual changes.
- Describe effects on prerequisites, accessibility, security, permissions,
  Azure or Docker costs, and cleanup procedures.
- Update documentation and tests when contracts or observable behavior change.

## Contributor Responsibilities

- Do not submit secrets, credentials, personal data, confidential information,
  or vulnerability details in public channels.
- Credit sources and comply with applicable licenses.
- Review and validate all submitted content, regardless of the tools used to create it.
- Follow the [Code of Conduct](CODE_OF_CONDUCT.md).

Submitting a contribution does not guarantee acceptance. Maintainers may ask
for changes, defer work, or decline proposals based on scope, safety,
maintenance cost, curriculum design, or repository direction.

## License

Unless stated otherwise, contributions are provided under the repository's
[MIT License](LICENSE). Content adapted from another source must retain all
required attribution and compatible licensing information.

# Contributing to Free Disk Analyzer

Thanks for considering a contribution.

## Ground rules

- V1 is analysis-only. No feature that deletes or modifies scanned files will be merged at this stage.
- No feature may require sending file names, paths, or contents off the local machine.
- No feature may introduce a paid tier, subscription, or artificially locked functionality. Free Disk Analyzer is free forever.
- New affiliate links must be disclosed clearly in the UI and added to [AFFILIATE-DISCLOSURE.md](AFFILIATE-DISCLOSURE.md).

## Getting started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/<your-username>/free-disk-analyzer.git`
3. Create a branch: `git checkout -b feature/my-change`
4. Requirements: Windows 10/11 x64, .NET 8 SDK, Visual Studio 2022 (or CLI)
5. Build: `dotnet build`
6. Run tests: `dotnet test`

## Code style

- C# with nullable reference types enabled
- Async I/O with `async`/`await`, all long-running operations must accept a `CancellationToken`
- MVVM pattern for the WPF project; keep view-behind code minimal
- Business logic (scanning, aggregation) belongs in `FreeDiskAnalyzer.Core`, which must stay free of WPF dependencies so it can be unit-tested
- Add unit tests for new logic in `FreeDiskAnalyzer.Core`

## Submitting changes

1. Make sure the project builds and tests pass locally
2. Update documentation if behavior changes
3. Open a pull request using the template in `.github/PULL_REQUEST_TEMPLATE.md`
4. Describe what changed and why; link any related issue

## Reporting bugs / requesting features

Use the issue templates in `.github/ISSUE_TEMPLATE/`.

## Translations

Localization uses `.resx` resource files (`Resources.resx` for English, `Resources.fr.resx` for French). Additional languages are welcome via new `Resources.<culture>.resx` files plus a PR.

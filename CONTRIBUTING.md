# Contributing to PurgeCore

This project is not accepting external code contributions (pull requests) at this time. It is developed and maintained solely by the project owner.

Bug reports and feature requests via [issues](../../issues) are still welcome, see below.

## Ground rules

- File deletion is allowed only through the specific, well-defined, safe paths already built (Duplicates, Empty Folders, Browser cleanup), always via the Recycle Bin, always with a confirmation showing what and how much before it happens, always re-checked against `PathSafetyGuard` (never deletes inside Windows/Program Files, regardless of the code path). Don't add a generic "delete this" button to Large Files, Old Files, or Folders, those show arbitrary files that could be anything, deletion there stays manual (Show in Explorer) rather than one click.
- No feature may require sending file names, paths, or contents off the local machine.
- No feature may introduce a paid tier, subscription, or artificially locked functionality. PurgeCore is free forever.
- New affiliate links must be disclosed clearly in the UI and added to [AFFILIATE-DISCLOSURE.md](AFFILIATE-DISCLOSURE.md).

## Building locally (for reference)

1. Requirements: Windows 10/11 x64, .NET 8 SDK, Visual Studio 2022 (or CLI)
2. Build: `dotnet build`
3. Run tests: `dotnet test`

## Code style (for reference)

- C# with nullable reference types enabled
- Async I/O with `async`/`await`, all long-running operations must accept a `CancellationToken`
- MVVM pattern for the WPF project; keep view-behind code minimal
- Business logic (scanning, aggregation) belongs in `FreeDiskAnalyzer.Core`, which must stay free of WPF dependencies so it can be unit-tested

## Reporting bugs / requesting features

Use the issue templates in `.github/ISSUE_TEMPLATE/`.

## Translations

Localization uses `.resx` resource files (`Resources.resx` for English, `Resources.fr.resx` for French). Additional languages are welcome via new `Resources.<culture>.resx` files plus a PR.

# Free Disk Analyzer

[![License: All Rights Reserved](https://img.shields.io/badge/License-All%20Rights%20Reserved-lightgrey.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20x64-0078D6)](#)
[![.NET](https://img.shields.io/badge/.NET-8-512BD4)](#)
[![Build](https://img.shields.io/github/actions/workflow/status/jeremstyke/free-disk-analyzer/build.yml?branch=main)](../../actions)
[![Release](https://img.shields.io/github/v/release/jeremstyke/free-disk-analyzer)](../../releases/latest)
[![Downloads](https://img.shields.io/github/downloads/jeremstyke/free-disk-analyzer/total)](../../releases)

**A free, privacy-first disk space analyzer for Windows.**
See what's really using your storage, with a beautiful and easy-to-use interface.

🇫🇷 [Lire en français](README.fr.md)

<p align="center">
  <a href="../../releases/latest">
    <img src="https://img.shields.io/badge/%E2%86%93%20Download-for%20Windows-2563EB?style=for-the-badge" alt="Download for Windows" />
  </a>
</p>

> [View all releases](../../releases)

---

## Screenshots

*Screenshots will be added here once the first UI build is available.*

## Features

- Fast, non-blocking disk scan (async, cancellable at any time)
- Dashboard with drive overview, used/free donut chart, storage-by-category breakdown, and last scan summary
- Largest Folders and Largest Files views
- Dedicated Large Files search with size filters (100 MB, 500 MB, 1 GB, 5 GB, custom)
- Folder hierarchy view to quickly spot what is consuming space
- NordVPN recommendation on the Dashboard, and a DeleteMe recommendation after a scan completes, both clearly labeled as affiliate links
- Settings: language, light/dark theme, start with Windows, opt-in anonymous analytics, reset to defaults, all saved locally
- Dedicated Privacy page explaining exactly what stays local and what never leaves your machine
- Light and dark mode, modern Windows 11-inspired interface
- English and French, full coverage (language selector in Settings, restart to apply)
- 100% local analysis. Nothing about your files or folders ever leaves your machine
- Free forever. No subscription, no premium tier, no artificial limits

## Installation

1. Go to the [latest release](../../releases/latest)
2. Download `FreeDiskAnalyzer-Setup.exe` (installer) or `FreeDiskAnalyzer-Portable.zip` (portable)
3. Run it. Windows may show a SmartScreen warning, see below.

### About the Windows security warning

Free Disk Analyzer is currently distributed without a commercial code-signing certificate. Windows SmartScreen may therefore display a warning because the application publisher cannot yet be verified.

This does not mean that Free Disk Analyzer is malware. You can verify the downloaded release using the SHA-256 checksum published with each release (`SHA256SUMS.txt`).

## Privacy

Free forever. Privacy first.

- Disk analysis is 100% local. File names, paths, contents, and folder structure are never sent anywhere.
- The app works fully offline.
- Optional, anonymous, minimal usage statistics can be enabled in Settings (installs, launches, scan count, app version, Windows version, approximate country). No personal data, no fingerprinting, no hidden tracking.

Full details: [PRIVACY.md](PRIVACY.md)

## Affiliate disclosure

Some links in Free Disk Analyzer (on the Dashboard and after a scan completes) are affiliate links (NordVPN, DeleteMe). If you purchase through one of these links, we may receive a commission at no extra cost to you. These commissions help fund development.

Full details: [AFFILIATE-DISCLOSURE.md](AFFILIATE-DISCLOSURE.md)

## Architecture

```
Free-Disk-Analyzer/
├── src/
│   ├── FreeDiskAnalyzer/          # WPF application (UI, views, view models)
│   └── FreeDiskAnalyzer.Core/     # Scan engine, models, services (no UI dependency)
├── tests/
│   └── FreeDiskAnalyzer.Tests/    # Unit tests
├── assets/                        # Icons, logos, images
├── docs/                          # Additional documentation
├── website/                       # GitHub Pages site (published once the repo goes public)
├── installer/                     # Inno Setup script producing FreeDiskAnalyzer-Setup.exe
└── .github/                       # Workflows, issue and PR templates
```

- **FreeDiskAnalyzer.Core**: disk scanning, size aggregation, file/folder models. Pure C#, no WPF dependency, unit-testable.
- **FreeDiskAnalyzer**: WPF app (.NET 8, Windows x64). MVVM, async/await, `CancellationToken` for all scan operations.

## Development

Requirements:
- Windows 10/11 x64
- .NET 8 SDK
- Visual Studio 2022 (or `dotnet build` from the CLI)

```bash
git clone https://github.com/jeremstyke/free-disk-analyzer.git
cd free-disk-analyzer
dotnet build
```

## Contributing

This project is not accepting external pull requests. Bug reports and feature requests via [issues](../../issues) are welcome, see [CONTRIBUTING.md](CONTRIBUTING.md) and [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

## Security

See [SECURITY.md](SECURITY.md) for how to report a vulnerability.

## License

All rights reserved. The application is free to use, the source code is not free to reuse or redistribute. See [LICENSE](LICENSE).

## Repository visibility

This repository is currently private during development. It will be made public (or restructured) once the app is in a working, downloadable state. Until then, releases and GitHub Pages are not publicly reachable, see [ROADMAP.md](docs/ROADMAP.md).

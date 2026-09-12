# PurgeCore

[![Status: Beta](https://img.shields.io/badge/status-Beta-F59E0B)](#)
[![License: All Rights Reserved](https://img.shields.io/badge/License-All%20Rights%20Reserved-lightgrey.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20x64-0078D6)](#)
[![.NET](https://img.shields.io/badge/.NET-8-512BD4)](#)
[![Build](https://img.shields.io/github/actions/workflow/status/jeremstyke/purgecore/build.yml?branch=main)](../../actions)
[![Release](https://img.shields.io/github/v/release/jeremstyke/purgecore)](../../releases/latest)
[![Downloads](https://img.shields.io/github/downloads/jeremstyke/purgecore/total)](../../releases)

**A complete, 100% free PC cleaner for Windows. Currently in beta.**
Disk analysis, duplicates, browser and PC cleanup, startup and RAM management. Nothing is ever sent anywhere, a privacy-first alternative to CCleaner and similar tools.

🇫🇷 [Lire en français](README.fr.md)

<p align="center">
  <a href="https://jeremstyke.github.io/purgecore/">
    <img src="https://img.shields.io/badge/%F0%9F%8C%90%20Website-2563EB?style=for-the-badge" alt="Website" />
  </a>
</p>

<p align="center">
  <a href="../../releases/latest/download/PurgeCore-Setup.exe">
    <img src="https://img.shields.io/badge/%E2%86%93%20Download-for%20Windows-2563EB?style=for-the-badge" alt="Download for Windows" />
  </a>
</p>

<p align="center"><sub>Power user? <a href="../../releases/latest/download/PurgeCore-Portable.zip">Download the portable version</a> instead, no installation, no admin rights needed. · Software directory? <a href="https://jeremstyke.github.io/purgecore/pad_file.xml">PAD file here</a>.</sub></p>

> [View all releases](../../releases)

---

## Features

- Fast, non-blocking disk scan (async, cancellable at any time)
- Dashboard with drive overview, used/free donut chart, storage-by-category breakdown, and last scan summary
- Largest Folders and Largest Files views
- Dedicated Large Files search with size filters (100 MB, 500 MB, 1 GB, 5 GB, custom)
- Folder hierarchy view to quickly spot what is consuming space
- Duplicate file finder (files 1 MB and up, matched by content, not just name/size), delete extra copies straight from the list (always keeps at least one copy, sends to Recycle Bin)
- Old files finder, the ones you likely forgot about, sorted by last modified date
- Empty folder finder, with one-click delete (Recycle Bin, not permanent)
- Browser cleanup: clears cache, cookies, and history for Chrome, Edge, and Firefox (bookmarks and passwords are never touched, Firefox history is intentionally skipped since it's stored together with bookmarks)
- Free up RAM (real before/after numbers, not a promised gain)
- Uninstall programs (largest first, search, uses each program's own official uninstaller)
- Flush DNS cache (one click, no files or registry touched)
- Startup program manager (enable/disable/remove apps that launch with Windows, per-user only, no admin rights needed)
- Driver info (read-only): installed drivers, flags ones 3+ years old, links to Windows Update and manufacturer sites, never downloads or installs anything itself
- In-app update notifications, with one-click download and install
- Latest blog articles shown on the Dashboard
- PC cleanup: Temp files and Recycle Bin (System tab in Cleanup)
- Cookie whitelist (Browsers tab): keep specific sites signed in when clearing cookies
- Green/orange/red risk indicators on every deletion feature
- Export a scan report to CSV
- NordVPN recommendation on the Dashboard, and a DeleteMe recommendation after a scan completes, both clearly labeled as affiliate links
- Settings: language, light/dark theme, start with Windows, reset to defaults, all saved locally
- Dedicated Privacy page explaining exactly what stays local and what never leaves your machine
- Light and dark mode, modern Windows 11-inspired interface
- English and French, full coverage (language selector in Settings, restart to apply)
- 100% local analysis. Nothing about your files or folders ever leaves your machine
- Free forever. No subscription, no premium tier, no artificial limits

## Installation

1. Go to the [latest release](../../releases/latest)
2. Download `PurgeCore-Setup.exe` (installer) or `PurgeCore-Portable.zip` (portable)
3. Run it. Windows may show a SmartScreen warning, see below.

### About the Windows security warning

PurgeCore is currently distributed without a commercial code-signing certificate. Windows SmartScreen may therefore display a warning because the application publisher cannot yet be verified.

This does not mean that PurgeCore is malware. The source code is publicly viewable on GitHub. You can also verify the downloaded release using the SHA-256 checksum published with each release (`SHA256SUMS.txt`).

## Privacy

Free forever. Privacy first.

- Disk analysis is 100% local. File names, paths, contents, and folder structure are never sent anywhere.
- The app works fully offline.
- Optional, anonymous, minimal usage statistics can be enabled in Settings (installs, launches, scan count, app version, Windows version, approximate country). No personal data, no fingerprinting, no hidden tracking.

Full details: [PRIVACY.md](PRIVACY.md)

## Affiliate disclosure

Some links in PurgeCore (on the Dashboard and after a scan completes) are affiliate links:

- [NordVPN](https://go.nordvpn.net/aff_c?offer_id=15&aff_id=155375&source=Free%20disk%20analyzer)
- [DeleteMe](https://www.de33watrk.com/WCKMXS/KMKS9/)

If you purchase through one of these links, we may receive a commission at no extra cost to you. These commissions help fund development.

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
├── installer/                     # Inno Setup script producing PurgeCore-Setup.exe
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
git clone https://github.com/jeremstyke/purgecore.git
cd purgecore
dotnet build
```

## Contributing

This project is not accepting external pull requests. Bug reports and feature requests via [issues](../../issues) are welcome, see [CONTRIBUTING.md](CONTRIBUTING.md) and [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

## Security

See [SECURITY.md](SECURITY.md) for how to report a vulnerability.

## License

All rights reserved. The application is free to use, the source code is not free to reuse or redistribute. See [LICENSE](LICENSE).

## Repository visibility

This repository is public.

## Support

If PurgeCore is useful to you, you can [offer a coffee](https://jeremstyke.gumroad.com/coffee).

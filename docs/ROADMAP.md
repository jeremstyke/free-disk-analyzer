# Roadmap

This tracks the phases toward Free Disk Analyzer v1.0.0. Each phase is meant to be a self-contained, reviewable chunk.

## Phase 0: Repository structure and documentation (done)

- Folder structure (`src/`, `tests/`, `assets/`, `docs/`, `website/`, `.github/`)
- `README.md` / `README.fr.md`
- `LICENSE` (All Rights Reserved)
- `PRIVACY.md`, `AFFILIATE-DISCLOSURE.md`
- `CONTRIBUTING.md`, `SECURITY.md`, `CODE_OF_CONDUCT.md`
- `.gitignore`
- Issue templates, PR template
- CI workflow (`build.yml`) and release workflow (`release.yml`), scaffolded and ready to activate once the project files exist

## Phase 1: Core scan engine (FreeDiskAnalyzer.Core) - done

- Models: `DriveInfoModel`, `FolderNode`, `FileEntry`, `ScanResult`, `ScanProgress`, `FileCategory`
- `DiskScanner` service: recursive scan on a background thread, `CancellationToken` support, robust handling of access-denied, locked files, long paths, I/O errors, reparse points skipped to avoid cycles
- `DriveEnumerator`: lists ready/available drives for the Dashboard
- `FileCategoryClassifier`: extension to category mapping for the storage-by-category chart
- `TopNTracker`: bounds memory by keeping only the top 200 largest files/folders instead of the whole tree
- Unit tests (`FreeDiskAnalyzer.Tests`) covering counts, aggregation, cancellation, missing path, category breakdown, progress reporting
- `FreeDiskAnalyzer.sln` at repo root, CI (`build.yml`) wired to restore/build/test it

## Phase 2: WPF shell and Dashboard - done

- `FreeDiskAnalyzer.csproj` (net8.0-windows, WPF, CommunityToolkit.Mvvm for MVVM)
- App shell: sidebar navigation (Dashboard, Analyze, Large Files, Folders, Utilities, Settings, Privacy, About), ViewModel-first navigation via DataTemplates
- Light/dark color dictionaries (`Colors.Light.xaml` / `Colors.Dark.xaml`) with accent `#2563EB`, swappable at runtime via `ThemeManager` (wiring a toggle into it is Phase 4/Settings)
- Card, primary button, nav list, and progress bar styles (`Styles.xaml`), Segoe UI Variable typography
- `DonutProgressRing` control for used/free
- Dashboard view: drive cards, selected-drive detail with donut, last scan summary (honest "no scan yet" placeholder, not fake data), NordVPN affiliate card with disclosure text
- Other sidebar sections wired to navigation but show a "coming soon" placeholder until their phase

## Phase 3: Analyze, Large Files, Folders views - done

- `ScanResultStore`: shared state so Dashboard, Large Files, and Folders all reflect the latest scan without re-running it
- Analyze view: drive picker, live progress (current path, files/folders counted, size, elapsed time), Cancel button, honest completion message (including on cancellation)
- Large Files view: size filter (100 MB / 500 MB / 1 GB / 5 GB / custom), Open and Show in Explorer actions
- Folders view: largest folders from the last scan sorted by size, same Open / Show in Explorer actions
- Dashboard's "Last scan" card now shows real totals (files, folders, bytes, completion time) once a scan has run, instead of a placeholder
- Not done yet: storage-by-category chart and file-type breakdown chart. `DiskScanner` already computes `BytesByCategory`, so this is a UI-only addition, folded into Phase 4

## Phase 4: Settings, Privacy, About, charts, DeleteMe suggestion - done

- Scope change (per Bob): no Utilities software catalog. Only two affiliate touchpoints: NordVPN on the Dashboard (Phase 2) and DeleteMe, suggested after a scan completes on the Analyze tab. `Utilities` removed from the sidebar entirely.
- Settings: theme (persisted, applied via `ThemeManager`), start with Windows (real registry Run key toggle), anonymous analytics opt-in/opt-out, reset to defaults. Saved as local JSON via `SettingsService`.
- Privacy page: static content mirroring `PRIVACY.md`.
- About page: version (from assembly), tagline, GitHub link, license note.
- Storage-by-category chart on the Dashboard, using `DiskScanner`'s `BytesByCategory` output.
- Not done: file-type breakdown chart (category chart covers most of the same need, revisit if still wanted), language switching (Phase 5).

## Phase 5: Localization - done

- `Resources/Strings.resx` (English, default) and `Resources/Strings.fr.resx` (French), embedded via the standard SDK resx pipeline. No Visual Studio Designer.cs relied on, a hand-written `Strings` static class wraps `ResourceManager` so it builds with plain `dotnet build`.
- Language selector in Settings (English / Français), persisted, applied via `CultureInfo.CurrentUICulture` at next launch (a "restart to apply" note appears when changed, this app doesn't attempt live re-binding of `x:Static` resources on the fly).
- Localized: navigation labels, Dashboard, Analyze, Large Files, Folders, Settings, Privacy (title, tagline, and all five body sections), About, Coming Soon placeholder.

## Phase 6: Packaging, CI/CD, and going public - in progress

- Done: Inno Setup script (`installer/setup.iss`) producing `FreeDiskAnalyzer-Setup.exe`, English/French installer UI, desktop icon optional, uninstaller included.
- Done: `release.yml` finalized. On a pushed tag (`vX.Y.Z`), it publishes a self-contained win-x64 build, zips it as `FreeDiskAnalyzer-Portable.zip`, builds the installer via the same publish output, generates `SHA256SUMS.txt` for both, and publishes all three to the GitHub Release.
- Done: app icon (`assets/icon.ico`, generated programmatically: a donut-ring motif matching the in-app `DonutProgressRing`, on a rounded-square accent-blue background). Wired into `FreeDiskAnalyzer.csproj` (`ApplicationIcon`) and `installer/setup.iss` (`SetupIconFile`).
- Done: `website/` static site, English (`index.html`) and French (`fr/index.html`), sharing `assets/style.css`. Hero, features, privacy, free-forever statement, screenshots placeholder (honestly empty, no fake images), FAQ, download band, footer with affiliate disclosure. Download CTA is in a disabled "coming soon" state until the first release exists. No links to the GitHub repo anywhere on the site, since it's currently private, would be dead links for visitors.
- Not done: real screenshots (need a working build first)
- Decision point (deferred, per Bob): repo is private during development. Before this phase ships publicly, decide whether to make the main repo public, or keep it private and distribute releases/Pages another way (private repos on the free plan can't serve public GitHub Releases downloads or GitHub Pages)
- Done: `v1.0.0` tagged and released (installer + portable zip + checksums, verified present on the GitHub Release). Note: this happened before the app was ever launched and tested locally, at Bob's explicit request, ahead of the usual order. If the app doesn't actually run correctly once tested, expect a `v1.0.1` fix release.
- Blog on the website (per Bob, revisited 2026-09-09, now with a concrete order and a new piece): build it after the Windows telemetry item in the v2 vision below. Purpose leans toward the original "Outils & Conseils" pitch (SEO/AdSense/affiliate traffic via tips-and-guides articles) rather than a product changelog, though that's worth confirming when this is actually scheduled. New addition: the Windows app itself should show a small "latest articles" feed (e.g. on the Dashboard or a dedicated tab) pulling from the website's blog, so the app drives traffic back to the site rather than the site only linking to the app. Needs a simple feed format the app can fetch (an RSS/JSON file generated alongside the blog is the least-effort option, no server needed, consistent with the static-site approach). Not scoped, not started.

## Phase 7: Duplicate finder, old files, empty folders, CSV export (post-v1.0.0)

All four read-only, no file deletion, same risk profile as the rest of v1:

- `FolderNode` now tracks recursive file/subfolder counts (`FileCount`, `SubfolderCount`, `IsEmpty`), computed by `DiskScanner` during the normal scan at no extra cost.
- `ScanResult` gained `OldestFiles` (bounded top-200, oldest first, files with no last-write date are excluded) and `EmptyFolders` (capped at 200), both from the same scan pass as everything else, no rescan needed.
- New "Old Files" tab: browse `OldestFiles`, same Open / Show in Explorer actions as Large Files.
- New "Empty Folders" tab: browse `EmptyFolders`, same actions.
- New "Duplicates" tab: a separate, opt-in scan (own drive picker, own progress, own cancel), since duplicate detection is inherently a two-phase, heavier operation than the main scan: group files 1 MB and up by size (cheap), then SHA-256 hash only the files that share a size with another file (skips the vast majority of files). Capped at 20,000 hashed candidates as a safety limit. `IDuplicateFinder` / `DuplicateFinder` in Core.
- "Export report (CSV)" button on Analyze, visible after a scan completes: writes root path, totals, category breakdown, largest folders, and largest files via `ScanReportExporter.BuildCsv`, using a standard Windows save dialog.
- Follow-up (per Bob): 10 sidebar items felt cluttered, so Old Files, Duplicates, and Empty Folders were consolidated under a single "Cleanup" sidebar entry with internal pill-style sub-navigation (`CleanupViewModel`, `CleanupView`). Sidebar is back to 8 items. Each sub-page keeps its own view model and state exactly as before, only the navigation container changed.
- Unit tests added for all of the above (folder counts, empty folder detection, oldest-files ordering, duplicate detection including the size-threshold and no-false-positive-on-same-size-different-content cases, CSV building including comma-escaping).
- Localized in both English and French, same pattern as the rest of the app.
- Verified compiling via the same GitHub Actions build-status check used for the rest of the project, not yet exercised by hand on a real machine.

## Out of scope for v1

- Any file deletion or cleanup feature (analysis-only in v1, per spec)
- Any paid tier or artificially limited feature
- Any telemetry beyond the documented anonymous, opt-in usage stats

## v2 vision (per Bob, logged only, nothing started)

A 4-tab expansion beyond the current read-only analyzer, described by Bob on 2026-09-09. Point 4 (blog/AdSense) explicitly excluded from this vision for now. Logged here so the idea isn't lost, not scheduled, not started. Before any of this begins: the current app needs to have actually been run and tested on a real machine (still pending as of this writing), since most of these items are a different risk category entirely from anything shipped so far.

1. **Nettoyage (Cleanup) tab** - Windows temp files, browser caches, recycle bin, error logs. **Real file deletion.** Same risk category explicitly deferred earlier in this project (see "Out of scope for v1" above). Needs its own confirmation/safety design (show what will be deleted and its total size before acting, Recycle Bin rather than permanent delete where possible, clear per-item errors) before it's built, not just bolted onto the existing scan UI. Browser cache/cookie/history cleanup specifically (raised again by Bob on 2026-09-09) is functionally the same feature CleanTab already does as a browser extension, per-browser cache and history clearing. Worth building this piece well since it's the load-bearing feature for the "replace CleanTab" long-term direction below, not just an add-on. Needs per-browser handling (Chrome/Edge/Firefox each store cache/cookies/history differently, in different file locations and formats), and should close the browser first or warn the user to, since most browsers lock their cache/history files while running.
2. **Vitesse / Performance tab** - startup program manager (disable/enable, registry `Run` key and Task Scheduler entries), one-click RAM purge. **Modifies system/registry state.** Startup manager is moderate risk (reversible, user-visible, well-trodden pattern in tools like Task Manager). RAM purge claims are often more marketing than real benefit on modern Windows, worth scrutinizing before building.
3. **Sécurité & Confidentialité tab** - VPN module (video/partner, likely the existing NordVPN affiliate relationship rather than a built-in VPN), DNS cache flush (`ipconfig /flushdns`, low risk, standard troubleshooting command), Windows telemetry/tracking toggles. **The telemetry toggles are registry/service-level system modification**, same risk category as "Inhibiteur de télémétrie" discussed and deferred earlier.
4. **Outils & Conseils tab** - was excluded from this vision on 2026-09-09, then brought back the same day: build the blog/articles piece after item 3's telemetry work (see the blog note near the top of this file). Also was: quick uninstaller (**real app removal + AppData cleanup**, same risk category as "Désinstalleur Express" discussed and deferred earlier).

Ordering note for whoever picks this up: item 3's DNS flush is the only genuinely low-risk item in this list. Everything else either deletes user files, modifies the registry/system services, or removes installed applications, categories this project has deliberately kept out of v1 so a bug can't hurt anyone's data. Build and test each in isolation, with explicit confirmation UI showing exactly what will change before it happens.

## Long-term product direction: replace CleanTab (per Bob, logged only, nothing started)

Described 2026-09-09. The 3-tab pitch: Cleanup (frees disk space, fixes everyday slowness), Security & Privacy (clears browsing traces, blocks Windows telemetry, protects the system), VPN (secures Wi-Fi, hides IP). Positioning: one Windows suite instead of three separate tools, covering a PC's full health and privacy.

The plan is for Free Disk Analyzer to eventually replace CleanTab (Chrome/Edge extension) entirely: migrate CleanTab's users to this Windows app, and show an end-of-life message inside CleanTab pointing them here, framed as "a complete suite on Windows, always free."

This raises the stakes on the "wait for real testing" rule already in place for the v2 vision above, it doesn't loosen it. CleanTab has real, active users today. Redirecting them to Free Disk Analyzer only makes sense once this app has been run and tested on a real machine, ideally has some track record with early users on its current read-only feature set, and the higher-risk v2 items (real deletion, registry/telemetry changes) have shipped and been used without incident. Sunsetting a working product to point at an unlaunched one is the kind of move that's hard to undo if it goes wrong, better to move a few weeks later with confidence than fast with an unverified base.

## VPN, option 2: reseller-backed integrated VPN (per Bob, logged only, nothing started)

Discussed 2026-09-09. Instead of just the existing NordVPN affiliate link, a real "Connect" button inside the app backed by a VPN reseller (white-label server access, Bob bills end users, reseller runs the network).

History worth remembering before restarting this: Bob previously pursued a VPN reseller approach (VPNresellers) for a free Android VPN app, using WireGuard, and it was abandoned as not viable, on top of unresolved Android build errors that were never fixed. Same reseller-based model, different platform, don't assume it'll go differently without asking what specifically didn't work last time (the reseller relationship/economics, or just the Android build issues).

Technical shape on Windows, if revisited: don't build a VPN client from scratch. Either drive a WireGuard/WinTun setup or shell out to the official `wireguard.exe` client with a config file supplied by the reseller. Meaningfully simpler than the Android driver-level work that stalled before, but still real ongoing work: credential management, server rotation, billing if not free, support when a connection breaks.

Not scoped, not started. Same precondition as the rest of the v2 items: this is for after the current app has real usage on a real machine.

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

## Phase 5: Localization - mostly done

- `Resources/Strings.resx` (English, default) and `Resources/Strings.fr.resx` (French), embedded via the standard SDK resx pipeline. No Visual Studio Designer.cs relied on, a hand-written `Strings` static class wraps `ResourceManager` so it builds with plain `dotnet build`.
- Language selector in Settings (English / Français), persisted, applied via `CultureInfo.CurrentUICulture` at next launch (a "restart to apply" note appears when changed, this app doesn't attempt live re-binding of `x:Static` resources on the fly).
- Localized: navigation labels, Dashboard, Analyze, Large Files, Folders, Settings, About, Coming Soon placeholder.
- Not done: the Privacy page's long-form body paragraphs are still English-only (title/tagline are localized). Same mechanical pattern as the other pages, just not swept yet, tracked here so it isn't forgotten.

## Phase 6: Packaging, CI/CD, and going public - in progress

- Done: Inno Setup script (`installer/setup.iss`) producing `FreeDiskAnalyzer-Setup.exe`, English/French installer UI, desktop icon optional, uninstaller included.
- Done: `release.yml` finalized. On a pushed tag (`vX.Y.Z`), it publishes a self-contained win-x64 build, zips it as `FreeDiskAnalyzer-Portable.zip`, builds the installer via the same publish output, generates `SHA256SUMS.txt` for both, and publishes all three to the GitHub Release.
- Done: app icon (`assets/icon.ico`, generated programmatically: a donut-ring motif matching the in-app `DonutProgressRing`, on a rounded-square accent-blue background). Wired into `FreeDiskAnalyzer.csproj` (`ApplicationIcon`) and `installer/setup.iss` (`SetupIconFile`).
- Done: `website/` static site, English (`index.html`) and French (`fr/index.html`), sharing `assets/style.css`. Hero, features, privacy, free-forever statement, screenshots placeholder (honestly empty, no fake images), FAQ, download band, footer with affiliate disclosure. Download CTA is in a disabled "coming soon" state until the first release exists. No links to the GitHub repo anywhere on the site, since it's currently private, would be dead links for visitors.
- Not done: real screenshots (need a working build first)
- Decision point (deferred, per Bob): repo is private during development. Before this phase ships publicly, decide whether to make the main repo public, or keep it private and distribute releases/Pages another way (private repos on the free plan can't serve public GitHub Releases downloads or GitHub Pages)
- Tag `v1.0.0` and cut the first release once the app has actually been built and tested locally at least once

## Out of scope for v1

- Any file deletion or cleanup feature (analysis-only in v1, per spec)
- Any paid tier or artificially limited feature
- Any telemetry beyond the documented anonymous, opt-in usage stats

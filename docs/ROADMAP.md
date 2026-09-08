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

## Phase 4: Utilities, Settings, Privacy, About

- `RecommendedSoftware` model and static/curated catalog (affiliate and non-affiliate entries)
- NordVPN and NordVPN Threat Protection entries
- Settings: language, theme, start with Windows, analytics opt-in/opt-out, reset settings
- Privacy page content (mirrors `PRIVACY.md`)
- About page (version, license, GitHub link)

## Phase 5: Localization

- `Resources.resx` (English, default) and `Resources.fr.resx` (French)
- Language selector wired into Settings

## Phase 6: Packaging, CI/CD, and GitHub Pages site

- Finalize `build.yml` (remove placeholder `continue-on-error`)
- Installer project (WiX or Inno Setup) producing `FreeDiskAnalyzer-Setup.exe`
- Finalize `release.yml`: portable zip, installer, `SHA256SUMS.txt`, GitHub Release publishing
- `website/` static site (EN + FR), matching the app's visual identity
- Tag `v1.0.0` and cut the first public release

## Out of scope for v1

- Any file deletion or cleanup feature (analysis-only in v1, per spec)
- Any paid tier or artificially limited feature
- Any telemetry beyond the documented anonymous, opt-in usage stats

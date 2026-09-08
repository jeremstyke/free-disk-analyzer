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

## Phase 1: Core scan engine (FreeDiskAnalyzer.Core)

- Models: `DriveInfoModel`, `FolderNode`, `FileEntry`, `ScanResult`, `ScanProgress`
- `DiskScanner` service: async recursive scan, `CancellationToken` support, robust handling of access-denied, locked files, long paths, I/O errors
- Aggregation: largest folders, largest files, category breakdown by extension
- Unit tests for scanner logic (using a temp directory tree, no dependency on real user drives)

## Phase 2: WPF shell and Dashboard

- `FreeDiskAnalyzer.csproj` (net8.0-windows, WPF)
- App shell: sidebar navigation (Dashboard, Analyze, Large Files, Folders, Utilities, Settings, Privacy, About)
- Light/dark theme resource dictionaries, accent color `#2563EB`
- Dashboard view: drive list, used/free donut chart, last scan summary, NordVPN affiliate card

## Phase 3: Analyze, Large Files, Folders views

- Analyze view: drive picker, live scan progress (files/folders counted, path being scanned, elapsed time, Cancel button)
- Results views: Largest Folders, Largest Files, with Open / Show in Explorer actions
- Large Files search with size filters (100 MB / 500 MB / 1 GB / 5 GB / custom)
- Charts: storage by category, file type breakdown

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

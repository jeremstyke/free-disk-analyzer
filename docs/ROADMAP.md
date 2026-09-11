# Roadmap

This tracks the phases toward PurgeCore v1.0.0. Each phase is meant to be a self-contained, reviewable chunk.

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

- Done: Inno Setup script (`installer/setup.iss`) producing `PurgeCore-Setup.exe`, English/French installer UI, desktop icon optional, uninstaller included.
- Done: `release.yml` finalized. On a pushed tag (`vX.Y.Z`), it publishes a self-contained win-x64 build, zips it as `PurgeCore-Portable.zip`, builds the installer via the same publish output, generates `SHA256SUMS.txt` for both, and publishes all three to the GitHub Release.
- Done: app icon (`assets/icon.ico`, generated programmatically: a donut-ring motif matching the in-app `DonutProgressRing`, on a rounded-square accent-blue background). Wired into `FreeDiskAnalyzer.csproj` (`ApplicationIcon`) and `installer/setup.iss` (`SetupIconFile`).
- Done: `website/` static site, English (`index.html`) and French (`fr/index.html`), sharing `assets/style.css`. Hero, features, privacy, free-forever statement, screenshots placeholder (honestly empty, no fake images), FAQ, download band, footer with affiliate disclosure. Download CTA is in a disabled "coming soon" state until the first release exists. No links to the GitHub repo anywhere on the site, since it's currently private, would be dead links for visitors.
- Not done: real screenshots (need a working build first)
- Decision point (deferred, per Bob): repo is private during development. Before this phase ships publicly, decide whether to make the main repo public, or keep it private and distribute releases/Pages another way (private repos on the free plan can't serve public GitHub Releases downloads or GitHub Pages)
- Done: `v1.0.0` tagged and released (installer + portable zip + checksums, verified present on the GitHub Release). Note: this happened before the app was ever launched and tested locally, at Bob's explicit request, ahead of the usual order. If the app doesn't actually run correctly once tested, expect a `v1.0.1` fix release.
- Blog on the website (per Bob, revisited 2026-09-09, now with a concrete order and a new piece): build it after the Windows telemetry item in the v2 vision below. Purpose leans toward the original "Outils & Conseils" pitch (SEO/AdSense/affiliate traffic via tips-and-guides articles) rather than a product changelog, though that's worth confirming when this is actually scheduled. New addition: the Windows app itself should show a small "latest articles" feed (e.g. on the Dashboard or a dedicated tab) pulling from the website's blog, so the app drives traffic back to the site rather than the site only linking to the app. Needs a simple feed format the app can fetch (an RSS/JSON file generated alongside the blog is the least-effort option, no server needed, consistent with the static-site approach). Not scoped, not started.

## Phase 7: Duplicate finder, old files, empty folders, CSV export, RAM optimization (post-v1.0.0)

Important gap discovered 2026-09-10: Bob tested the actual `v1.0.0` release binary and it had none of this, because all of Phase 7 was built and merged to `main` *after* the `v1.0.0` tag was cut. The tag freezes a specific commit, it doesn't track `main`. Lesson: cut a new tag (`v1.0.1`+) whenever features land that should be in the hands of whoever is testing, don't assume "on main" means "in the release someone downloaded."

All read-only except RAM optimization (which modifies live process memory state, not files or the registry, and is fully reversible/non-destructive):

- `FolderNode` now tracks recursive file/subfolder counts (`FileCount`, `SubfolderCount`, `IsEmpty`), computed by `DiskScanner` during the normal scan at no extra cost.
- `ScanResult` gained `OldestFiles` (bounded top-200, oldest first, files with no last-write date are excluded) and `EmptyFolders` (capped at 200), both from the same scan pass as everything else, no rescan needed.
- New "Old Files" tab: browse `OldestFiles`, same Open / Show in Explorer actions as Large Files.
- New "Empty Folders" tab: browse `EmptyFolders`, same actions.
- New "Duplicates" tab: a separate, opt-in scan (own drive picker, own progress, own cancel), since duplicate detection is inherently a two-phase, heavier operation than the main scan: group files 1 MB and up by size (cheap), then SHA-256 hash only the files that share a size with another file (skips the vast majority of files). Capped at 20,000 hashed candidates as a safety limit. `IDuplicateFinder` / `DuplicateFinder` in Core.
- "Export report (CSV)" button on Analyze, visible after a scan completes: writes root path, totals, category breakdown, largest folders, and largest files via `ScanReportExporter.BuildCsv`, using a standard Windows save dialog.
- Follow-up (per Bob): 10 sidebar items felt cluttered, so Old Files, Duplicates, and Empty Folders were consolidated under a single "Cleanup" sidebar entry with internal pill-style sub-navigation (`CleanupViewModel`, `CleanupView`). Sidebar is back to 8 items. Each sub-page keeps its own view model and state exactly as before, only the navigation container changed.
- Unit tests added for all of the above (folder counts, empty folder detection, oldest-files ordering, duplicate detection including the size-threshold and no-false-positive-on-same-size-different-content cases, CSV building including comma-escaping).
- Localized in both English and French, same pattern as the rest of the app.
- "Free up RAM" added to Cleanup as a 4th sub-tab, per Bob 2026-09-10. `IRamOptimizer` / `RamOptimizer` in the WPF project (uses Windows P/Invoke, `EmptyWorkingSet` / `GlobalMemoryStatusEx`, so it lives alongside `SettingsService` rather than in the cross-platform-clean Core project, same pattern as the registry code). Trims working sets of processes this app has permission to touch (most system/other-user processes will be skipped, that's expected). Shows real before/after available-memory numbers rather than claiming a guaranteed benefit, since on modern Windows the actual gain from this kind of action is often smaller than "RAM cleaner" tools imply.
- Verified compiling via the same GitHub Actions build-status check used for the rest of the project, not yet exercised by hand on a real machine.

## Out of scope for v1

- Any paid tier or artificially limited feature
- Any telemetry beyond the documented anonymous, opt-in usage stats
- A generic "delete this file/folder" button anywhere outside the specific, safe, well-defined deletion features already built (Duplicates, Empty Folders, Browser cleanup)

## v2 vision (per Bob, logged only, nothing started)

A 4-tab expansion beyond the current read-only analyzer, described by Bob on 2026-09-09. Point 4 (blog/AdSense) explicitly excluded from this vision for now. Logged here so the idea isn't lost, not scheduled, not started. Before any of this begins: the current app needs to have actually been run and tested on a real machine (still pending as of this writing), since most of these items are a different risk category entirely from anything shipped so far.

1. **Nettoyage (Cleanup) tab** - Windows temp files, browser caches, recycle bin, error logs. **Real file deletion.** Same risk category explicitly deferred earlier in this project (see "Out of scope for v1" above). Needs its own confirmation/safety design (show what will be deleted and its total size before acting, Recycle Bin rather than permanent delete where possible, clear per-item errors) before it's built, not just bolted onto the existing scan UI. Browser cache/cookie/history cleanup specifically (raised again by Bob on 2026-09-09) is functionally the same feature CleanTab already does as a browser extension, per-browser cache and history clearing. Worth building this piece well since it's the load-bearing feature for the "replace CleanTab" long-term direction below, not just an add-on. Needs per-browser handling (Chrome/Edge/Firefox each store cache/cookies/history differently, in different file locations and formats), and should close the browser first or warn the user to, since most browsers lock their cache/history files while running.
2. **Vitesse / Performance tab** - startup program manager (disable/enable, registry `Run` key and Task Scheduler entries), one-click RAM purge. **Modifies system/registry state.** Startup manager is moderate risk (reversible, user-visible, well-trodden pattern in tools like Task Manager). RAM purge claims are often more marketing than real benefit on modern Windows, worth scrutinizing before building.
3. **Sécurité & Confidentialité tab** - DNS cache flush (`ipconfig /flushdns`, low risk, standard troubleshooting command), Windows telemetry/tracking toggles. **The telemetry toggles are registry/service-level system modification**, same risk category as "Inhibiteur de télémétrie" discussed and deferred earlier. VPN moved out of this tab/timeframe entirely, see the separate section below, much further out per Bob. Also belongs here: **NordVPN Threat Protection** affiliate card (raised by Bob 2026-09-10), link `https://go.nordvpn.net/aff_c?offer_id=725&aff_id=155375&url_id=22219&source=App windows`, note the different `offer_id` (725) and `source` tag from the existing Dashboard NordVPN VPN link (which uses `offer_id=15&source=Free disk analyzer`), don't reuse or confuse the two. Low effort whenever this tab gets built: same affiliate-card pattern already used for NordVPN on the Dashboard, just a different product and link.
4. **Outils & Conseils tab** - was excluded from this vision on 2026-09-09, then brought back the same day: build the blog/articles piece after item 3's telemetry work (see the blog note near the top of this file). Also was: quick uninstaller (**real app removal + AppData cleanup**, same risk category as "Désinstalleur Express" discussed and deferred earlier).

Ordering note for whoever picks this up: item 3's DNS flush is the only genuinely low-risk item in this list. Everything else either deletes user files, modifies the registry/system services, or removes installed applications, categories this project has deliberately kept out of v1 so a bug can't hurt anyone's data. Build and test each in isolation, with explicit confirmation UI showing exactly what will change before it happens.

## Long-term product direction: replace CleanTab (per Bob, logged only, nothing started)

Described 2026-09-09. Originally pitched as three tabs including VPN, the VPN portion of this plan was abandoned per Bob 2026-09-10 (see below), current pitch is Cleanup (frees disk space, fixes everyday slowness) plus Security & Privacy (clears browsing traces, blocks Windows telemetry, protects the system). Positioning: one Windows suite instead of separate tools, covering a PC's full health and privacy.

The plan is for PurgeCore to eventually replace CleanTab (Chrome/Edge extension) entirely: migrate CleanTab's users to this Windows app, and show an end-of-life message inside CleanTab pointing them here, framed as "a complete suite on Windows, always free."

This raises the stakes on the "wait for real testing" rule already in place for the v2 vision above, it doesn't loosen it. CleanTab has real, active users today. Redirecting them to PurgeCore only makes sense once this app has been run and tested on a real machine, ideally has some track record with early users on its current read-only feature set, and the higher-risk v2 items (real deletion, registry/telemetry changes) have shipped and been used without incident. Sunsetting a working product to point at an unlaunched one is the kind of move that's hard to undo if it goes wrong, better to move a few weeks later with confidence than fast with an unverified base.

## Free VPN plan abandoned (per Bob, 2026-09-10)

The built-in free VPN plan (reseller-backed WireGuard integration, discussed 2026-09-09 as "much later," then briefly built as a teaser tab in the app and a page/blog post on the site per Bob 2026-09-10) has been fully abandoned. Removed everywhere: the VPN tab and ViewModel from the app, the dedicated page and announcement post from the website, all nav links and homepage sections, the README callout, sitemap and RSS entries. Development is focused on cleanup features instead. NordVPN remains as an affiliate recommendation only (Dashboard card), that's unrelated and unaffected, see the IPVanish note below for that side of things.

If a built-in VPN is ever reconsidered, this history is worth knowing first: Bob previously pursued a VPN reseller approach (VPNresellers) for a free Android VPN app using WireGuard, abandoned as not viable, on top of unresolved Android build errors that were never fixed. The Windows teaser attempt in this app was abandoned even before reaching the reseller-integration stage. Two abandoned attempts on two platforms, worth asking what specifically isn't working (reseller economics, technical complexity, or something else) before trying a third time.

## IPVanish considered and declined for PurgeCore (per Bob, 2026-09-10)

Bob has an IPVanish affiliate link (already used on CleanTab, alongside DeleteMe). Considered adding it here too, decided against: two competing VPN affiliate offers in the same app reads as less genuine and splits attention, rather than one clear recommendation. PurgeCore keeps NordVPN only. If this gets revisited, don't place a second VPN offer next to the existing NordVPN card, separate contexts if it happens at all.

## In-app update notifications and release process (per Bob, 2026-09-10)

Added: `IUpdateChecker` / `UpdateChecker` (Core) polls the GitHub Releases API once on startup, compares the latest tag to the running app's version via `ReleaseVersionComparer`, and fails silently on any network problem (never blocks or errors out the app over a background convenience check). When a newer version exists, a banner appears at the top of every page (`MainWindow.xaml`, wired through `MainViewModel`) with the version number, a dismiss option, and a "Download and install" button that downloads the new installer to a temp folder and launches it.

The installer (`installer/setup.iss`) now sets `CloseApplications=yes` and `RestartApplications=yes`, so launching it from inside a running PurgeCore closes the app, installs over it, and reopens it automatically, a genuine one-click update rather than requiring the user to close the app manually first.

Process going forward, for whoever cuts the next release: write a blog post for every tagged release (see `website/blog/v1-0-1-release-notes.html` for the pattern), add it to `website/blog/index.html`, `website/sitemap.xml`, and `website/blog/rss.xml`. Not automated yet, each of those four files needs a manual edit per release. Automating this (e.g. a release.yml step that generates the post from the tag's changelog) would be a reasonable follow-up if releases become frequent enough that the manual step gets skipped.

## First real deletion features, and browser cleanup to replace CleanTab's core function (per Bob, 2026-09-10)

This is the first time the app deletes anything. Explicit request from Bob, with an explicit safety requirement attached, not something to build lightly, so the safety design is documented here in detail:

- `PathSafetyGuard` (Core, unit tested): hard-coded refusal to consider anything under Windows, Program Files, or Program Files (x86) safe to delete, regardless of what feature or code path produced the path. Applied both when a feature decides what to *offer* for deletion and again, redundantly, inside `SafeDeleteService` right before the actual delete call.
- `SafeDeleteService` (WPF project, uses `Microsoft.VisualBasic.FileIO.FileSystem`): every delete in the app goes to the Recycle Bin, never `File.Delete`/`Directory.Delete` directly. Never throws, returns false on failure (locked file, missing, protected path) so callers can report "skipped" rather than crash.
- Every delete path requires an explicit confirmation dialog showing what will be deleted and roughly how much space it frees, before anything happens.

What can actually be deleted, and nothing else:

1. **Duplicates**: "Delete extra copies" per group, always keeps the first file, only offers the rest for deletion, a duplicate group can never be fully wiped out by this button.
2. **Empty Folders**: delete button per folder, only offered for folders the scanner already confirmed are empty (recursively, `FolderNode.IsEmpty`).
3. **Browser cleanup** (new "Browsers" tab in Cleanup, 5th sub-tab): `IBrowserCleaner` / `BrowserCleaner` scans Chrome, Edge (Chromium, `Default` profile only, per-profile support like "Profile 1" not implemented) and Firefox (default profile via directory pattern match, not full `profiles.ini` parsing) for Cache, Cookies, and History, shows sizes, lets the user pick which to clear via checkboxes, deletes only what's selected after confirmation. Firefox History is never offered, it lives in the same `places.sqlite` database as bookmarks in Firefox, too risky to touch with a simple file delete. Bookmarks and saved passwords are never in scope for any browser. This is the feature that gives PurgeCore the core capability CleanTab has as a browser extension, relevant to the "replace CleanTab" long-term direction logged earlier in this file.

Deliberately not built: a generic "delete this" button on Large Files, Old Files, or Folders. Those tabs show arbitrary files the scanner found, which could be anything, so deletion there stays manual (the existing Show in Explorer button) rather than one click. See the note added to `CONTRIBUTING.md`.

Not yet verified by hand: this is real, permanent-ish (Recycle-Bin-backed but still) file deletion, shipped without ever having been run on a real machine, same caveat as everything else in this project, but it matters more here than anywhere else so far.

## PC cleanup and cookie whitelist (per Bob, 2026-09-10)

Two additions on top of the deletion safety work above:

- **System tab** (6th Cleanup sub-tab): `ISystemCleaner` / `SystemCleaner` cleans the user's own Temp folder (`Path.GetTempPath()`, always outside Windows/Program Files so it never conflicts with `PathSafetyGuard`) and empties the Recycle Bin via the standard `SHEmptyRecycleBin` shell API. Deliberately does not touch `C:\Windows\Temp` (the system-wide temp folder), that's under the protected `Windows` root and out of scope here, only the per-user temp folder is cleaned.
- **Cookie whitelist** (Settings): a list of domains (one per line) whose cookies survive a cookie cleanup, so the user stays signed in to sites they choose. Implemented via `Microsoft.Data.Sqlite`, opening the browser's cookie database directly and deleting only non-whitelisted rows (Chromium: `cookies` table / `host_key` column, Firefox: `moz_cookies` / `host`), rather than deleting the whole file.

Flagged explicitly to Bob when built: the cookie whitelist is meaningfully less certain than everything else in this app. It edits another program's private database file based on an assumed schema that could differ across browser versions or break on a future update, verified by static reasoning only, never against a real Chrome/Edge/Firefox cookie database. Failures are caught and skipped (never a partial/corrupt write attempted), but "doesn't crash" isn't the same as "definitely works as intended", this is the one feature in the app worth extra scrutiny once real testing starts.

## CleanTab cross-promotion removed from site/app (per Bob, 2026-09-10)

All CleanTab links and mentions removed from the website (footer on every page, About page bio) and the app (About page card, ViewModel command/URL, resx strings). Consistent with PurgeCore now having its own browser cleanup feature rather than needing to point at CleanTab for that. Note: this only covers what lives in this repository, the CleanTab extension itself is a separate codebase/session, not something reachable from here.

## Sidebar regrouped: Explore + Cleanup (per Bob, 2026-09-10)

Sidebar was growing (8 items, Cleanup already had 6 internal sub-tabs). Regrouped by whether a tab can change/delete something:

- **Explore** (new, 3 sub-tabs: Large Files, Folders, Old Files): purely read-only browsing of the last scan, nothing here deletes anything.
- **Cleanup** (5 sub-tabs, Old Files moved out to Explore): Duplicates, Empty Folders, Performance, Browsers, System, everything here can change or delete something.

Sidebar is back to 7 top-level items. `ExploreViewModel` reuses the exact same `CleanupView.xaml` (it only binds to generic `Tabs`/`SelectedTab`/`CurrentContent`/`Label` properties, no Cleanup-specific content), registered as a second DataTemplate target in `App.xaml` rather than duplicating the XAML file.

## Startup Manager added (per Bob, 2026-09-10)

Startup Manager added to the Performance sub-tab in Cleanup (alongside Free up RAM), matching the original v2 "Vitesse/Performance" grouping. `IStartupManager` / `StartupManager`: reads `HKCU\...\CurrentVersion\Run` and the user's own Startup folder (`shell:startup`), both accessible without admin rights, consistent with the app never running elevated. HKLM Run entries and the shared "All Users" Startup folder are intentionally out of scope, would need elevation. Disabling an item doesn't delete it: the registry value or shortcut moves to an app-controlled "disabled" location and can be moved back, only "Remove" (with confirmation) is permanent, and even that only unregisters the startup entry, it doesn't uninstall the program.

Bob's original request in this same conversation also included a VPN "teasing" tab: a page promising immediate, unlimited VPN access for a real 24.99€ Gumroad payment (or ad-view-earned "VPN hours"), while the actual VPN backend didn't exist yet. Declined to build the payment/ad-reward mechanics for a non-functional product, that's charging real money or collecting ad views against a promise the app couldn't deliver. Built instead: a "coming soon" informational page (no payment), the existing NordVPN affiliate CTA, and a donation-framed support link. This whole VPN direction was later abandoned entirely, see "Free VPN plan abandoned" above for what that meant in practice. Kept here for the reasoning precedent: informational/donation language is fine, promising a working feature that doesn't exist in exchange for money is not, regardless of how it's labeled ("teasing", "offre de lancement", etc.), if a monetized feature idea comes up again.

## Driver info added to Explore (per Bob, 2026-09-10)

Bob asked for a registry cleaner-style feature, declined (see refusal precedent: registry cleaners have near-zero real benefit on modern Windows and real risk of breaking a program that needs the "obsolete" key, Microsoft itself discourages this category of tool). Offered a safer alternative instead: read-only driver info. `IDriverInfoService` / `DriverInfoService` (WPF project, uses `System.Management`/WMI, `Win32_PnPSignedDriver`) lists installed drivers with name, manufacturer, version, and date, flags anything 3+ years old as a simple age signal, not a diagnosis. Added as a 4th pill under Explore (read-only tabs), alongside Large Files/Folders/Old Files.

Deliberately does not download or install anything: buttons open Windows Update (`ms-settings:windowsupdate`) and the NVIDIA/AMD/Intel driver pages, generic (not matched to the user's specific hardware), the user does the actual update themselves. Accurate per-device driver matching is exactly the kind of database that "driver updater" tools claim to have and often get wrong or use to manufacture fake problems for a paid upsell, not something to attempt here.

## Idea: paid sponsor slot (per Bob, 2026-09-11, logged only, not started)

Bob floated the idea of letting sponsors pay to have their own promotional link placed in the app, similar in spirit to the existing NordVPN/DeleteMe affiliate cards but sponsor-supplied rather than curated by Bob. Not something to build now, just logged so it isn't lost.

Worth thinking through before building, if revisited: how this differs from an ad network (a single curated sponsor slot Bob personally vets is a very different trust proposition than programmatic ads, closer to how some newsletters/open source projects do a "this release sponsored by X" credit), how sponsors would be vetted so a sponsor slot doesn't become a vector for scammy/low-quality products showing up in a tool people trust for safety, and how it's disclosed to users (same affiliate-disclosure pattern already used for NordVPN/DeleteMe would likely apply). Precondition should be the same as everything else on this list: real usage first, since a sponsor slot needs an actual audience to be worth anything to a sponsor.

## NordVPN + DeleteMe made more prominent (per Bob, 2026-09-11)

Bob asked to give these more visibility, in the app and on the site, while a future paid-sponsor idea (above) stays unbuilt for now.

App: the Dashboard used to show only a NordVPN card, placed last (below the blog section). Added a matching DeleteMe card (previously only shown post-scan on the Analyze tab, DeleteMe now also appears here), and moved both up to right after the Storage by category card, side by side, so they're visible without scrolling as far.

Website: moved the Partners section (NordVPN + DeleteMe) up on both homepages, from below the Privacy/Free-forever bands to right after Features, same reasoning, more visible higher on the page.

Deliberately did not add pop-ups, banners, or anything more attention-grabbing than repositioning existing cards, that would work against PurgeCore's whole pitch of "not like the more aggressive free tools."

## VPN via VPNresellers explored in depth, then dropped (per Bob, 2026-09-11)

Bob asked to seriously explore integrating a real VPN into PurgeCore via VPNresellers (not the earlier abandoned teaser tab, a genuinely working one this time). Went deep on this, technical shape and monetization, then Bob decided to drop it. Logged here so the research isn't lost if revisited later.

**Technical shape, if ever revisited (Option A, the one actually explored)**: bundle the official `wireguard.exe` + `wintun.dll` (GPL, free, no licensing issue as long as they're run as a separate process rather than having their code merged into PurgeCore), use the VPNresellers API to provision an account and get a WireGuard config, write it to a `.conf` file, and shell out to `wireguard.exe /installtunnelservice` with a UAC elevation prompt on each connect/disconnect. Rejected the "real VPN app" alternative (Option B: a permanent background Windows service so there's no UAC prompt after first install) as too much separate infrastructure for a solo dev to maintain. VPNresellers wholesale cost starts at $1.99/active account/month, but critically bills per full calendar day of activity regardless of how many hours the account was actually connected, that detail matters for any monetization math.

**Monetization paths explored, in order, why each was dropped**:
1. Straight €1.99/month subscription: margin near zero once VPNresellers cost + payment processing fees are subtracted, unprofitable in practice.
2. Rewarded ads (watch N ads, unlock M hours): the "bill by full day regardless of duration" fact means only the first unlock of the day costs anything, later same-day reactivations are free to Bob, so profitability entirely depends on how often each user reactivates per day, an unpredictable behavioral bet. Modeled multiple variants (4 ads/3h, 5 ads/2h, 1 ad/1h - the last one is structurally the worst, since it requires ~17-20 reactivations/day to break even and nobody does that). A single daily unlock threshold (watch N ads once, unlimited until midnight) was the best-behaved version of this model, removing the reactivation gamble, but still needs real ad revenue.
3. Ad network reality check: AdMob is mobile-only, not usable for a Windows app at all. AppLixir (the correct category for web/desktop rewarded video) requires 5,000+ DAU to even be approved as a publisher, PurgeCore has ~20 installs total, nowhere close. Ezoic raised its new-site threshold to 250,000+ monthly visitors as of Feb 2026, worse than AppLixir, and even their own Rewarded Ads product requires the reward to be redeemable on the publisher's own site, not in an external desktop app, so it wouldn't fit this use case even if the traffic bar were met.
4. Landing page with regular AdSense display ads, reward granted separately in the app: this is squarely what Google's AdSense policies call "incentivized traffic" ("compensate users for viewing ads"), a top listed reason for account termination, regardless of whether Google's specific Rewarded Ads unit type is used or not, and regardless of whether a delay/timer is shown on the page. Removing the delay doesn't fix the compliance problem and likely breaks ad-view-counting minimums too.
5. Paid subscription via Gumroad, no ads at all, revisited with real numbers: at €6.99/month, after Gumroad's 10% + ~$0.50 fee and card processing (~2.9% + $0.30), net take-home is roughly €5.35, against ~€1.85/month VPNresellers cost, landing around €3.50/month margin per subscriber (~50%). This is the only model of everything explored with genuinely healthy, non-speculative unit economics, no ad-network gatekeeping, no behavioral bet.

**Why dropped despite #5 being financially sound**: even the good version is a multi-day build (VPNresellers integration, WireGuard bundling and UAC handling, Gumroad subscription status checks, a full new VPN tab) for a product with ~20 users still in beta on its core cleanup features. Per-subscriber economics being healthy doesn't matter much without subscriber volume, and getting volume isn't the current bottleneck skill needed, testing and stabilizing the existing app is. Bob's own call: not worth it yet.

If this comes back: the Gumroad-subscription-only model (#5) is the one to build, skip the ad-funded paths entirely, they're either against ad network policy or dependent on unpredictable user behavior for profitability.

## VPN pricing settled, if this ever gets built (per Bob, 2026-09-11)

Even though the VPN idea itself was dropped for now (see above), Bob and I worked out final pricing in case it comes back later, framed as "priced at cost, not a profit center": **3.49 EUR/month or 29.99 EUR/year**.

Margin at full daily usage: ~0.45 EUR/month on the monthly plan (~13%), ~3.18 EUR/year on the annual plan (~11%), after Gumroad's 10% + ~0.50 EUR fee and card processing (~2.9% + ~0.30 EUR) are subtracted, against a VPNresellers cost of ~1.85 EUR/month if connected every single day.

Important dynamic worth remembering: VPNresellers bills per full calendar day of activity, not per hour, but only on days the account actually connects. A subscriber who doesn't use the VPN daily costs Bob proportionally less than someone who uses it every day, while paying the same flat subscription price, the same "unlimited plan, uneven usage" economics as a gym membership. A subscriber active only 5 days out of 30 would net roughly 2 EUR/month margin instead of 0.45 EUR, meaning realistic blended margins across a subscriber base are likely well above the full-usage worst case.

Test-the-demand-first plan discussed but not yet executed: put a small survey (Google Forms or Tally.so) in front of users asking whether they'd want a paid VPN add-on at this price point, before building anything. Bob was going to create the form; once a link exists, add a button/section in the app and on the site pointing to it.

## VPN pricing, option B considered: normal margin instead of cost-price (per Bob, 2026-09-11)

After modeling profit at scale (30k subscribers, priced at cost 3.49/month: roughly 13.5k-59.7k EUR/month depending on average usage days, likely 35k-41k/month at a realistic ~12-15 days/month blended usage), Bob asked about pricing higher instead of at cost.

**Option B: 5.99 EUR/month** instead of 3.49. Net after Gumroad + card fees ~4.55 EUR, margin ~2.70 EUR/month at full daily usage, ~3.62 EUR/month at a more realistic 15 days/month usage, roughly 2.5x the margin of the cost-price option at the same subscriber count. Still meaningfully cheaper than standalone VPNs (NordVPN/ExpressVPN around 10-13 EUR/month), so the "cheaper than a real VPN" pitch still holds even at this higher price.

Trade-offs flagged, not resolved: (1) loses the "priced at cost, we don't profit from this" honest-pricing narrative that fits PurgeCore's overall positioning (no aggressive upsells, no ads), (2) a paid in-house VPN at a lower price than the existing NordVPN affiliate card could cannibalize those affiliate clicks, trading a zero-risk, zero-effort revenue stream for a bigger but higher-effort, higher-responsibility one, (3) unknown effect of a higher price on conversion rate, real number would come from the planned demand survey (see above), not guesswork.

No decision made between the 3.49 (cost-price) and 5.99 (normal margin) options, both logged as live alternatives if the VPN feature is ever built. Same overall precondition as before: not being built now, survey-the-demand step still not executed.

# [CPS-002a] ✅ DONE - The catalogue library lists files by folder pattern and extension

**Status**: Done (2026-11-08)
**Priority**: Medium
**Component**: `src/FoldersCatalog/`, `tests/FoldersCatalog.Tests/`, `scripts/gate`, `.github/workflows/`

**Summary**:
As the extension, I want a list of catalogue entries from folder patterns and extensions, so
that I only map entries onto commands and the rules for which files count can be tested on any
machine.

**Description**:
A class library `FoldersCatalog` (`net10.0`, no Windows dependency) takes folder patterns and
extensions and returns entries — full path, name, folder. The patterns follow Keypirinha's
`filescatalog.ini`:
- `<folder>` — the files directly in it;
- `<folder>\**` — the files in it and in every folder below it.

Environment variables in the form `%NAME%` and a leading `~` are expanded. Extensions match
case-insensitively, with or without their leading dot. A folder that does not exist yields no
entries and no error; a file reached by two patterns appears once. Hidden and system files are
skipped, as `Directory.EnumerateFiles` does by default. Junctions and symbolic links to
folders are followed; each real folder is listed once, so a link back up the tree ends
instead of looping and a folder reached twice yields its files once.

Hidden folders are skipped along with their files, as are folders that cannot be read. An entry
carries the path as it was reached, not the link's target. Entries are ordered by full path,
ignoring case. A blank pattern is an error; a relative one resolves against the current
directory (CPS-002c's settings parsing drops blank lines).

This substory also sets up the gate: `scripts/gate` runs the library's tests in
`mcr.microsoft.com/dotnet/sdk:10.0`, so they run on the dev VM without a local .NET SDK, and a
GitHub Actions workflow runs the same tests on every pull request.

**Scope Decisions**:
- **2026-11-07 — Links are followed**: junctions and symbolic links below a `\**` pattern are
  followed, as Keypirinha does; every real folder is listed once.
- **2026-11-07 — Folders only**: the two pattern forms are all there is — no wildcards in
  file names. Which files count is the extension list's job.
- **2026-11-07 — CI on Ubuntu**: the library's tests run on `ubuntu-latest`; symbolic links
  stand in for junctions there. The Windows job that CPS-002b adds for the extension also runs
  them, and covers what Linux cannot — real junctions, drive letters, case-insensitive paths.

**Acceptance Criteria**:
- [x] Tests cover both pattern forms, `%NAME%` and `~` expansion, case-insensitive extensions with and without dot, a missing folder, a file reached by two patterns, a followed folder link, and a link that points back up the tree
- [x] `scripts/gate` runs the tests in the .NET SDK container and exits 0 only when they pass
- [x] CI runs the tests on every pull request

**UAT** (2026-11-07, the author's Windows machine, through CPS-003's tool):
- [x] The catalogue of `~\shortcuts\**` and `~\shortcuts_private\**` with `.url .lnk .cmd .bat .exe` matches an independent `Get-ChildItem -Recurse -File -FollowSymlink` listing: 125 files each, none missing, none twice; `%USERPROFILE%` is expanded by the library

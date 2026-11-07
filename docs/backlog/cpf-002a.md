# [CPF-002a] The catalogue library lists files by folder pattern and extension

**Status**: In Progress
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
skipped, as `Directory.EnumerateFiles` does by default.

This substory also sets up the gate: `scripts/gate` runs the library's tests in
`mcr.microsoft.com/dotnet/sdk:10.0`, so they run on the dev VM without a local .NET SDK, and a
GitHub Actions workflow runs the same tests on every pull request.

**Acceptance Criteria**:
- [ ] Tests cover both pattern forms, `%NAME%` and `~` expansion, case-insensitive extensions with and without dot, a missing folder, and a file reached by two patterns
- [ ] `scripts/gate` runs the tests in the .NET SDK container and exits 0 only when they pass
- [ ] CI runs the tests on every pull request

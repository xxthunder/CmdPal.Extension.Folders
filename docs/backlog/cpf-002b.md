# [CPF-002b] ✅ DONE - The extension lists the catalogue as top-level commands and starts a file

**Status**: Done (2026-11-08)
**Priority**: Medium
**Component**: `src/CmdPal.Extension.Folders/`, `.github/workflows/`

**Summary**:
As a user of Command Palette, I want every catalogued file as an entry of the main search, so
that I start a script by typing a part of its name and the palette's ranking puts my most used
ones on top.

**Description**:
The extension project comes from the template that Command Palette's *Create extension*
generates (`net10.0-windows10.0.26100.0`, MSIX, `Microsoft.CommandPalette.Extensions`). Its
command provider returns one top-level command per catalogue entry: the title is the file
name without extension, the subtitle its folder, the icon the file's own. Invoking it starts
the file through the shell, as a double-click in Explorer does.

The command's ID is the file's full path. The built-in ranking (`RecentCommandsManager`) keys
its history on that ID, so it has to stay the same across restarts for a started file to stay
on top.

For this substory the folders and extensions are fixed in code — the defaults that CPF-002c
turns into settings.

**Technical Notes**:
- From CPF-002a's review: the Windows job is the first to run the library's tests on Windows.
  Three things only it can show — `Build_UnreadableSubfolder_IsSkipped` returns early on
  Windows instead of skipping (xUnit 2.9 has no dynamic skip), so it shows as passed there; a
  drive-root pattern `C:\**` has no test yet; and a pattern of just `C:` without `\**` still
  resolves drive-relative, to the process's current directory on that drive.
- From CPF-003's review: the CLI tests start `dotnet run tools/catalog.cs` from `PATH` and
  compare stdout line by line — on the Windows job the tool's first, cold build happens inside
  the first test unless the job builds it first (`scripts/gate` does), and any build message on
  stdout would break the comparison.
- From the first try on the author's machine: an `dotnet.exe` from a runtime-only install in
  `C:\Program Files\dotnet` comes first on the machine `PATH` and finds no SDK
  ("No .NET SDKs were found"), even with the SDK installed through Scoop. A local build of the
  extension needs the SDK where that `dotnet` looks — e.g. `winget install Microsoft.DotNet.SDK.10`.

**Scope Decisions**:
- **2026-11-07 — A bare `C:` stays drive-relative**: a pattern of just a drive letter and colon
  resolves as `Path.GetFullPath` resolves it, to the current directory on that drive. Nobody
  writes it for a catalogue; `C:\` and `C:\**` work. If a setting should reject it, that is
  CPF-002c's parsing.

**Acceptance Criteria**:
- [x] The provider returns one top-level command per catalogue entry, with title, subtitle and the file's icon
- [x] A command's ID is the file's full path
- [x] Invoking a command starts the file through the shell
- [x] CI builds the extension on a Windows runner and runs the library's tests there too

**UAT** (2026-11-08, the author's Windows machine, branch of PR #3, registered as a development package):
- [x] Scripts from `~\shortcuts` and `~\shortcuts_private` appear in Command Palette's main search, with their folder and icon
- [x] A started `.cmd` runs in its own folder
- [x] A script started before is on top at the next search, also after a restart of Command Palette

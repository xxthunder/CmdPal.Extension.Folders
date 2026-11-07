# [CPF-002b] The extension lists the catalogue as top-level commands and starts a file

**Status**: Open
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

**Acceptance Criteria**:
- [ ] The provider returns one top-level command per catalogue entry, with title, subtitle and the file's icon
- [ ] A command's ID is the file's full path
- [ ] Invoking a command starts the file through the shell
- [ ] CI builds the extension on a Windows runner and runs the library's tests there too

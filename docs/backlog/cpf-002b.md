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

**Acceptance Criteria**:
- [ ] The provider returns one top-level command per catalogue entry, with title, subtitle and the file's icon
- [ ] A command's ID is the file's full path
- [ ] Invoking a command starts the file through the shell
- [ ] CI builds the extension on a Windows runner and runs the library's tests there too

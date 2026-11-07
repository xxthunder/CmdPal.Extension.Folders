# [CPF-002c] Folders and extensions are settings of the extension

**Status**: Open
**Priority**: Medium
**Component**: `src/CmdPal.Extension.Folders/Helpers/SettingsManager.cs`

**Summary**:
As a user of Command Palette, I want to change the catalogued folders and extensions in the
extension's settings, so that a new folder or file type needs no new build.

**Description**:
Two text settings in the template's `SettingsManager` (`JsonSettingsManager`): the folder
patterns, one per line, and the extensions, separated by spaces. Their defaults are the
author's: `%USERPROFILE%\shortcuts\**`, `%USERPROFILE%\shortcuts_private\**` and
`.url .lnk .cmd .bat .exe`. Changing a setting rebuilds the catalogue and raises
`ItemsChanged`, so the main search shows the new list without a restart.

Parsing the two texts into patterns and extensions belongs to the catalogue library, where it
is tested.

**Acceptance Criteria**:
- [ ] The library parses the folder text (one pattern per line, blank lines ignored) and the extension text (separated by spaces), with tests
- [ ] The extension's settings page shows both settings with the defaults above
- [ ] A changed setting changes the entries in the main search without a restart of Command Palette

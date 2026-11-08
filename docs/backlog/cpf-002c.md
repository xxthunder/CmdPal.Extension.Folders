# [CPF-002c] Folders and extensions are settings of the extension

**Status**: In Progress
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

**Scope Decisions**:
- **2026-11-08 — Text fields, SDK 0.11**: the patterns are a multiline `TextSetting`, the
  extensions a one-line one, as described above. The toolkit's `StringListSetting` (a list with
  add and remove) needs a newer SDK and buys little for settings changed rarely;
  `FilePathListSetting` (folder picker) is in no released SDK yet and suits `\**` and
  `%USERPROFILE%` patterns badly.
- **2026-11-08 — One place for the defaults**: the defaults move from the provider into the
  settings; `tools/catalog.cs` keeps having none and parses `--ext` with the library's parser.

- **2026-11-08 — The Windows setup guide rides along**: at the author's request, the README's
  sections on setting up a Windows machine and trying a development build go into this item's
  pull request, though they belong to no item — they were learned during the UATs of CPF-002b
  and CPF-002c.

**Acceptance Criteria**:
- [ ] The library parses the folder text (one pattern per line, blank lines ignored) and the extension text (separated by spaces), with tests
- [ ] The extension's settings page shows both settings with the defaults above
- [ ] A changed setting changes the entries in the main search without a restart of Command Palette

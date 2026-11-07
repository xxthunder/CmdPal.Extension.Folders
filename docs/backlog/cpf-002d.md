# [CPF-002d] A file added to a catalogued folder appears without a restart

**Status**: Open
**Priority**: Medium
**Component**: `src/FoldersCatalog/`, `src/CmdPal.Extension.Folders/`

**Summary**:
As a user of Command Palette, I want a new script to show up in the search right after I save
it, so that I do not restart the palette or regenerate anything.

**Description**:
A `FileSystemWatcher` on each catalogued folder — with subdirectories for a `\**` pattern —
reports created, deleted and renamed files. Changes are debounced: a burst of events, such as
a `git pull` in `~\shortcuts`, rebuilds the catalogue once, after the folder has been quiet for
a short time, and then raises `ItemsChanged`. The debounce is part of the library and tested
there with a fake clock.

**Acceptance Criteria**:
- [ ] A burst of change events leads to one rebuild after the quiet time, with tests
- [ ] A file created in a catalogued folder appears in the main search without a restart of Command Palette
- [ ] A deleted file disappears from the main search without a restart

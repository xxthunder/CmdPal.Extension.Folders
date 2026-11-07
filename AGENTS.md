# AGENTS.md

Instructions for agents working in this repository.

## What this is

A PowerToys Command Palette extension in C#: it catalogues folders of the user's choice into
the main search. The design and the work are in the [backlog](docs/backlog/README.md); the
epic [CPF-002](docs/backlog/cpf-002.md) holds the overall design.

## Layout

- `src/FoldersCatalog/` — the catalogue library, `net10.0`, no Windows dependency: which
  files count, settings parsing, debouncing. Everything that can be tested without Windows
  lives here.
- `src/CmdPal.Extension.Folders/` — the extension, `net10.0-windows`, MSIX, from the
  template of Command Palette's *Create extension*. It only maps catalogue entries onto
  commands; keep logic out of it.
- `tests/` — the library's tests.

## Working here

- Test first: a change to the library starts with a failing test.
- Run `scripts/gate` before every commit (it arrives with CPF-002a). It runs the library's tests in the
  `mcr.microsoft.com/dotnet/sdk:10.0` container, so no local .NET SDK is needed.
- The extension builds only on Windows — on the author's Windows machine or in CI.
- Code goes through a pull request into `develop`, from a feature branch
  `feature/<ITEM-ID>-<slug>`; history stays linear (rebase, no merge commits).
- Backlog and docs changes go straight onto `develop`.
- Everything here is English: code, comments, docs, backlog, commit messages.
- Commit messages follow Conventional Commits and name the item ID.

## Closed loop

- **Gate**: `scripts/gate` runs the library's tests locally; the extension's build needs a
  Windows runner, so the CI checks of the pull request are the gate.
- **Gate runs in**: the .NET SDK container, started by `scripts/gate`.
- **Rubric**: none of its own yet.

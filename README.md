# CmdPal.Extension.Shortcuts

A [PowerToys Command Palette](https://learn.microsoft.com/windows/powertoys/command-palette/overview)
extension that catalogues folders of your choice into the main search — the files in them,
filtered by extension, as top-level commands that Command Palette ranks by how often and how
recently you start them. It fills the gap Keypirinha's `filescatalog` leaves when moving to
Command Palette: the built-in *Installed apps* extension searches only the Start menu, the
desktop and `PATH`.

*Shortcuts* here are the small things you start by name: scripts (`.ps1`, `.cmd`, `.bat`), links
(`.lnk`, `.url`) and programs (`.exe`), kept in folders of your own — such as `~\shortcuts`,
where [xxthunder/shortcuts](https://github.com/xxthunder/shortcuts), a keyboard-driven launcher
and automation toolkit for Windows, installs itself and its scripts. By default the extension catalogues
`%USERPROFILE%\shortcuts\**` and `%USERPROFILE%\shortcuts_private\**` with those six
extensions; both are settings.

Formerly called CmdPal.Extension.Folders.

Status: in development — see the [backlog](docs/backlog/README.md).

## Starting a script

A script (`.ps1`, `.bat`, `.cmd`) runs on Enter, in its own folder. Ctrl+Enter (the item's
second command) opens a page: type the arguments, press Enter, and the script runs with them.
A `.ps1` runs in `pwsh`, with your profile loaded. Links and
programs (`.url`, `.lnk`, `.exe`) start on Enter, as before.

## Listing the catalogue

From the repository root, with the .NET 10 SDK:

```
dotnet run tools/catalog.cs -- --ext ".url .lnk .cmd .bat .ps1 .exe" "%USERPROFILE%\shortcuts\**" "%USERPROFILE%\shortcuts_private\**"
```

It prints one line per entry (name, folder, full path, tab-separated), then the number of
entries; without patterns or without `--ext` it prints its usage and exits with 2.

## Logos

The logos come from `src/CmdPal.Extension.Shortcuts/Assets/Source/Logo.png`, the logo of
xxthunder/shortcuts. After changing it, render them again:

```
dotnet run tools/logos.cs -- src/CmdPal.Extension.Shortcuts/Assets/Source/Logo.png src/CmdPal.Extension.Shortcuts/Assets
```

## Setting up a Windows machine

Once per machine, in PowerShell 7:

1. **winget** — `winget --version` must answer. If it is not found:
   - `Get-AppxPackage Microsoft.DesktopAppInstaller` lists App Installer → its folder is missing
     from `PATH`: add `%USERPROFILE%\AppData\Local\Microsoft\WindowsApps` to your user `Path`
     (Start → "Edit environment variables for your account") and open a new terminal;
   - otherwise: `scoop install winget`.
2. **.NET 10 SDK**: `winget install Microsoft.DotNet.SDK.10`. `dotnet --list-sdks` must show
   `10.0.x [C:\Program Files\dotnet\sdk]`. Install the SDK there rather than through Scoop: a
   runtime-only `dotnet.exe` in `C:\Program Files\dotnet`, common as an app's dependency, comes
   first on the machine `PATH` and hides an SDK on the user `PATH`.
3. **Windows SDK 10.0.26100**: `winget install Microsoft.WindowsSDK.10.0.26100`. The extension's
   build generates its WinRT projection from it; without it the build fails with "Could not find
   the Windows SDK in the registry".
4. **Developer Mode**: Settings → System → For developers. Needed to register a development
   build and for the library's link tests, which create symbolic links.
5. **GitHub CLI**: `winget install GitHub.cli`, then `gh auth login` — to check out a pull
   request for its UAT.

## Building and testing on Windows

From the repository root:

```powershell
dotnet build tools/catalog.cs
dotnet test tests/FoldersCatalog.Tests/FoldersCatalog.Tests.csproj
dotnet test tests/CmdPal.Extension.Shortcuts.Tests/CmdPal.Extension.Shortcuts.Tests.csproj -p:Platform=x64
dotnet build src/CmdPal.Extension.Shortcuts/CmdPal.Extension.Shortcuts.csproj -c Release -p:Platform=x64
```

## Trying a development build

Register the build, then restart Command Palette: exit PowerToys from the tray and start it
again. Stopping the extension while Command Palette runs can leave stale state, and the extension
then loads no commands.

```powershell
dotnet build src/CmdPal.Extension.Shortcuts/CmdPal.Extension.Shortcuts.csproj -p:Platform=x64
Add-AppxPackage -Register (Resolve-Path "src\CmdPal.Extension.Shortcuts\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppxManifest.xml")
```

A registered development build runs in place from that `bin` folder, so Command Palette's
extension process holds its files. Before each rebuild, stop it, and restart Command Palette
(exit PowerToys from the tray and start it again) after the build:

```powershell
Stop-Process -Name CmdPal.Extension.Shortcuts -ErrorAction SilentlyContinue
dotnet build src/CmdPal.Extension.Shortcuts/CmdPal.Extension.Shortcuts.csproj -p:Platform=x64
```

Register again only when `Package.appxmanifest` changed. Remove it with
`Get-AppxPackage CmdPal.Extension.Shortcuts | Remove-AppxPackage`.

## License

[MIT](LICENSE). The extension derives from Microsoft's Command Palette extension template;
see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

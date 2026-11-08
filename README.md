# CmdPal.Extension.Folders

A [PowerToys Command Palette](https://learn.microsoft.com/windows/powertoys/command-palette/overview)
extension that catalogues folders of your choice into the main search — the files in them,
filtered by extension, as top-level commands that Command Palette ranks by how often and how
recently you start them. It fills the gap Keypirinha's `filescatalog` leaves when moving to
Command Palette: the built-in *Installed apps* extension searches only the Start menu, the
desktop and `PATH`.

Status: in development — see the [backlog](docs/backlog/README.md).

## Listing the catalogue

From the repository root, with the .NET 10 SDK:

```
dotnet run tools/catalog.cs -- --ext ".url .lnk .cmd .bat .exe" "%USERPROFILE%\shortcuts\**" "%USERPROFILE%\shortcuts_private\**"
```

It prints one line per entry (name, folder, full path, tab-separated), then the number of
entries; without patterns or without `--ext` it prints its usage and exits with 2.

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
dotnet test tests/CmdPal.Extension.Folders.Tests/CmdPal.Extension.Folders.Tests.csproj -p:Platform=x64
dotnet build src/CmdPal.Extension.Folders/CmdPal.Extension.Folders.csproj -c Release -p:Platform=x64
```

## Trying a development build

Register the build, then run *Reload extensions* in Command Palette:

```powershell
dotnet build src/CmdPal.Extension.Folders/CmdPal.Extension.Folders.csproj -p:Platform=x64
Add-AppxPackage -Register (Resolve-Path "src\CmdPal.Extension.Folders\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppxManifest.xml")
```

A registered development build runs in place from that `bin` folder, so Command Palette's
extension process holds its files. Before each rebuild, stop it — Command Palette starts it
again on *Reload extensions*:

```powershell
Stop-Process -Name CmdPal.Extension.Folders -ErrorAction SilentlyContinue
dotnet build src/CmdPal.Extension.Folders/CmdPal.Extension.Folders.csproj -p:Platform=x64
```

Register again only when `Package.appxmanifest` changed. Remove it with
`Get-AppxPackage CmdPal.Extension.Folders | Remove-AppxPackage`.

## License

[MIT](LICENSE). The extension derives from Microsoft's Command Palette extension template;
see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

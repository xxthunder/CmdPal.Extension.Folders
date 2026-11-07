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

## Building and testing on Windows

From the repository root, with the .NET 10 SDK and the Windows SDK 10.0.26100 (the extension's
build generates its WinRT projection from the Windows SDK's metadata; without it the build fails
with "Could not find the Windows SDK in the registry"):

```
dotnet build tools/catalog.cs
dotnet test tests/FoldersCatalog.Tests/FoldersCatalog.Tests.csproj
dotnet test tests/CmdPal.Extension.Folders.Tests/CmdPal.Extension.Folders.Tests.csproj -p:Platform=x64
dotnet build src/CmdPal.Extension.Folders/CmdPal.Extension.Folders.csproj -c Release -p:Platform=x64
```

The library's link tests create symbolic links, which on Windows need Developer Mode or an
elevated shell.

`dotnet --list-sdks` must show a 10.0 SDK. If it shows none although one is installed — through
Scoop, say — a runtime-only `C:\Program Files\dotnet\dotnet.exe` comes first on the machine
`PATH`. Put the SDK's folder first, e.g. in your PowerShell `$PROFILE`:

```powershell
$sdk = scoop prefix dotnet-sdk-lts
$env:PATH = "$sdk;$env:PATH"; $env:DOTNET_ROOT = $sdk
```

To try the extension without a signed package, turn on Developer Mode (Settings → System → For
developers) and register the build:

```powershell
dotnet build src/CmdPal.Extension.Folders/CmdPal.Extension.Folders.csproj -p:Platform=x64
Add-AppxPackage -Register (Resolve-Path "src\CmdPal.Extension.Folders\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppxManifest.xml")
```

Then run *Reload extensions* in Command Palette. `Get-AppxPackage CmdPal.Extension.Folders | Remove-AppxPackage`
removes it again.

## License

[MIT](LICENSE). The extension derives from Microsoft's Command Palette extension template;
see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using FoldersCatalog;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPal.Extension.Shortcuts;

public sealed partial class ShortcutsCommandsProvider : CommandProvider
{
    private readonly SettingsManager _settings;

    private ICommandItem[] _commands;

    public ShortcutsCommandsProvider()
        : this(new SettingsManager())
    {
    }

    public ShortcutsCommandsProvider(SettingsManager settings)
    {
        _settings = settings;
        DisplayName = "Shortcuts";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Settings = settings.Settings;
        _commands = BuildCommands();
        settings.Settings.SettingsChanged += (_, _) => Reload();
    }

    private void Reload()
    {
        _commands = BuildCommands();
        RaiseItemsChanged(_commands.Length);
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

    private ICommandItem[] BuildCommands() =>
        Catalog.Build(_settings.Patterns, _settings.Extensions)
            .Select(entry => (ICommandItem)new CommandItem(new StartFileCommand(entry.FullPath, (p, dir) => ShellHelpers.OpenInShell(p, workingDir: dir)))
            {
                Title = entry.Name,
                Subtitle = entry.Folder,
                Icon = new IconInfo(entry.FullPath),
            })
            .ToArray();
}

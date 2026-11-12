// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Linq;
using FoldersCatalog;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPal.Extension.Shortcuts;

public sealed partial class ShortcutsCommandsProvider : CommandProvider
{
    private readonly SettingsManager _settings;

    private readonly Func<LaunchSpec, bool> _startScript;

    private ICommandItem[] _commands;

    public ShortcutsCommandsProvider()
        : this(new SettingsManager())
    {
    }

    public ShortcutsCommandsProvider(SettingsManager settings, Func<LaunchSpec, bool>? startScript = null)
    {
        _settings = settings;
        _startScript = startScript ?? StartProcess;
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

    // Not ShellHelpers.OpenInShell: it catches the start failure and only returns false, so RunScriptCommand could not show why the start failed.
    private static bool StartProcess(LaunchSpec spec)
    {
        using var process = Process.Start(new ProcessStartInfo(spec.FileName, spec.Arguments)
        {
            WorkingDirectory = spec.WorkingDirectory,
            UseShellExecute = true,
        });
        return process != null;
    }

    private ICommandItem[] BuildCommands() =>
        Catalog.Build(_settings.Patterns, _settings.Extensions)
            .Select(ToItem)
            .ToArray();

    private ICommandItem ToItem(CatalogEntry entry)
    {
        var icon = new IconInfo(entry.FullPath);
        if (!ScriptLaunch.IsScript(entry.FullPath))
        {
            return new CommandItem(new StartFileCommand(entry.FullPath, (p, dir) => ShellHelpers.OpenInShell(p, workingDir: dir)))
            {
                Title = entry.Name,
                Subtitle = entry.Folder,
                Icon = icon,
            };
        }

        return new CommandItem(new RunScriptCommand(entry.FullPath, () => string.Empty, _startScript))
        {
            Title = entry.Name,
            Subtitle = entry.Folder,
            Icon = icon,
            MoreCommands =
            [
                new CommandContextItem(new ScriptArgumentsPage(entry.FullPath, _startScript)),
            ],
        };
    }
}

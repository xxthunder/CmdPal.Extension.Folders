// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using FoldersCatalog;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPal.Extension.Folders;

public sealed partial class FoldersCommandsProvider : CommandProvider
{
    private static readonly string[] DefaultPatterns =
        [@"%USERPROFILE%\shortcuts\**", @"%USERPROFILE%\shortcuts_private\**"];

    private static readonly string[] DefaultExtensions = [".url", ".lnk", ".cmd", ".bat", ".exe"];

    private readonly ICommandItem[] _commands;

    public FoldersCommandsProvider()
        : this(DefaultPatterns, DefaultExtensions)
    {
    }

    public FoldersCommandsProvider(IEnumerable<string> patterns, IEnumerable<string> extensions)
    {
        DisplayName = "Folders";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands = Catalog.Build(patterns, extensions)
            .Select(entry => (ICommandItem)new CommandItem(new StartFileCommand(entry.FullPath, (p, dir) => ShellHelpers.OpenInShell(p, workingDir: dir)))
            {
                Title = entry.Name,
                Subtitle = entry.Folder,
                Icon = new IconInfo(entry.FullPath),
            })
            .ToArray();
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}

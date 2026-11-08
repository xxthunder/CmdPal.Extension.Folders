// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using FoldersCatalog;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPal.Extension.Folders;

public sealed partial class SettingsManager : JsonSettingsManager
{
    private const string Namespace = "CmdPal.Extension.Folders";

    public const string PatternsKey = Namespace + ".Patterns";

    public const string ExtensionsKey = Namespace + ".Extensions";

    public const string DefaultPatterns = "%USERPROFILE%\\shortcuts\\**\n%USERPROFILE%\\shortcuts_private\\**";

    public const string DefaultExtensions = ".url .lnk .cmd .bat .exe";

    private readonly TextSetting _patterns = new(
        PatternsKey,
        "Folders",
        "One per line: the full path of a folder, or of a folder followed by \\** for everything below it. Environment variables such as %USERPROFILE%, and ~, are expanded.",
        DefaultPatterns)
    {
        Multiline = true,
    };

    private readonly TextSetting _extensions = new(
        ExtensionsKey,
        "File types",
        "Extensions separated by spaces, e.g. .url .lnk .cmd",
        DefaultExtensions)
    {
        Placeholder = ".url .lnk .cmd",
    };

    public SettingsManager()
        : this(SettingsJsonPath())
    {
    }

    public SettingsManager(string filePath)
    {
        FilePath = filePath;

        Settings.Add(_patterns);
        Settings.Add(_extensions);

        // Load settings from file upon initialization
        if (File.Exists(FilePath))
        {
            LoadSettings();
        }

        Settings.SettingsChanged += (_, _) => SaveSettings();
    }

    public IReadOnlyList<string> Patterns => CatalogSettings.ParsePatterns(_patterns.Value ?? string.Empty);

    public IReadOnlyList<string> Extensions => CatalogSettings.ParseExtensions(_extensions.Value ?? string.Empty);

    internal static string SettingsJsonPath()
    {
        var directory = Utilities.BaseSettingsPath(Namespace);
        Directory.CreateDirectory(directory);

        return Path.Combine(directory, "settings.json");
    }
}

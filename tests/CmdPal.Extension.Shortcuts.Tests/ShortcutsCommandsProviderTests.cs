using System.Text.Json;
using CmdPal.Extension.Shortcuts;
using Microsoft.CommandPalette.Extensions;

namespace CmdPal.Extension.Shortcuts.Tests;

public sealed class ShortcutsCommandsProviderTests : IDisposable
{
    private readonly string _root = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "cpf-provider-" + Guid.NewGuid().ToString("N")));

    public ShortcutsCommandsProviderTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private string P(params string[] parts) => Path.Combine([_root, .. parts]);

    private string Touch(params string[] parts)
    {
        var path = P(parts);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "");
        return path;
    }

    private static string Json(string patterns, string extensions) =>
        JsonSerializer.Serialize(new Dictionary<string, string>
        {
            [SettingsManager.PatternsKey] = patterns,
            [SettingsManager.ExtensionsKey] = extensions,
        });

    private SettingsManager Manager(string patterns, string extensions = ".cmd")
    {
        var file = Path.Combine(_root, "settings-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(file, Json(patterns, extensions));
        return new SettingsManager(file);
    }

    private ICommandItem[] Items(params string[] patterns)
    {
        var provider = new ShortcutsCommandsProvider(Manager(string.Join("\n", patterns)));
        return provider.TopLevelCommands();
    }

    [Fact]
    public void Provider_DisplayName_IsShortcuts()
    {
        var provider = new ShortcutsCommandsProvider(Manager(P("**")));

        Assert.Equal("Shortcuts", provider.DisplayName);
    }

    [Fact]
    public void SettingsManager_KeysUseTheShortcutsNamespace()
    {
        Assert.Equal("CmdPal.Extension.Shortcuts.Patterns", SettingsManager.PatternsKey);
        Assert.Equal("CmdPal.Extension.Shortcuts.Extensions", SettingsManager.ExtensionsKey);
    }

    [Fact]
    public void Provider_ExposesTheSettings()
    {
        var manager = Manager(P("**"));

        var provider = new ShortcutsCommandsProvider(manager);

        Assert.Same(manager.Settings, provider.Settings);
    }

    [Fact]
    public void SavedSettings_ListTheNewFolderRaiseItemsChangedAndPersist()
    {
        Touch("a", "one.cmd");
        Touch("b", "two.cmd");
        var file = Path.Combine(_root, "settings-saved.json");
        File.WriteAllText(file, Json(P("a", "**"), ".cmd"));
        var manager = new SettingsManager(file);
        var provider = new ShortcutsCommandsProvider(manager);
        var raised = 0;
        provider.ItemsChanged += (_, _) => raised++;
        var newPattern = P("b", "**");

        var form = (IFormContent)manager.Settings.ToContent()[0];
        form.SubmitForm(Json(newPattern, ".cmd"), "");

        var item = Assert.Single(provider.TopLevelCommands());
        Assert.Equal("two", item.Title);
        Assert.Equal(1, raised);
        Assert.Contains(JsonSerializer.Serialize(newPattern).Trim('"'), File.ReadAllText(file));
    }

    [Fact]
    public void EmptiedSettings_NoCommands()
    {
        Touch("a.cmd");
        var manager = Manager("");

        var provider = new ShortcutsCommandsProvider(manager);

        Assert.Empty(provider.TopLevelCommands());
    }

    [Fact]
    public void EmptiedExtensions_NoCommands()
    {
        Touch("a.cmd");
        var manager = Manager(P("**"), "");

        var provider = new ShortcutsCommandsProvider(manager);

        Assert.Empty(provider.TopLevelCommands());
    }

    [Fact]
    public void TopLevelCommands_OneItemPerFile()
    {
        Touch("a.cmd");
        Touch("sub", "b.cmd");
        Touch("c.txt");

        var items = Items(P("**"));

        Assert.Equal(2, items.Length);
    }

    [Fact]
    public void TopLevelCommands_ItemShowsNameFolderAndFileIcon()
    {
        Touch("a.cmd");
        var file = Touch("sub", "b.cmd");

        var item = Assert.Single(Items(P("**")), i => i.Title == "b");

        Assert.Equal(P("sub"), item.Subtitle);
        Assert.Equal(file, item.Icon.Light.Icon);
        Assert.Equal(file, item.Icon.Dark.Icon);
    }

    [Fact]
    public void TopLevelCommands_CommandIdIsFullPath()
    {
        var file = Touch("sub", "b.cmd");

        var item = Assert.Single(Items(P("**")));

        Assert.Equal(file, item.Command.Id);
    }

    [Fact]
    public void TopLevelCommands_SameNameInTwoFolders_TwoCommands()
    {
        Touch("x", "a.cmd");
        Touch("y", "a.cmd");

        var items = Items(P("**"));

        Assert.Equal(2, items.Length);
        Assert.Equal(2, items.Select(i => i.Command.Id).Distinct().Count());
        Assert.Equal(2, items.Select(i => i.Subtitle).Distinct().Count());
    }

    [Fact]
    public void TopLevelCommands_MissingFolders_NoItems()
    {
        var items = Items(P("nope", "**"));

        Assert.Empty(items);
    }

    [Fact]
    public void StartFileCommand_Invoke_StartsThePathAndDismisses()
    {
        var path = P("a.cmd");
        var started = new List<(string Path, string Folder)>();
        var command = new StartFileCommand(path, (p, folder) =>
        {
            started.Add((p, folder));
            return true;
        });

        var result = command.Invoke();

        Assert.Equal([(path, Path.GetDirectoryName(path)!)], started);
        Assert.Equal(CommandResultKind.Dismiss, result.Kind);
    }
}

using System.Text.Json;
using CmdPal.Extension.Shortcuts;
using FoldersCatalog;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

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
        var provider = new ShortcutsCommandsProvider(Manager(string.Join("\n", patterns)), history: NewHistory());
        return provider.TopLevelCommands();
    }

    [Fact]
    public void Provider_DisplayName_IsShortcuts()
    {
        var provider = new ShortcutsCommandsProvider(Manager(P("**")), history: NewHistory());

        Assert.Equal("Shortcuts", provider.DisplayName);
    }

    [Fact]
    public void Provider_Icon_IsAFileNextToTheBinary()
    {
        var provider = new ShortcutsCommandsProvider(Manager(P("**")));

        Assert.True(File.Exists(provider.Icon.Light.Icon), $"Light icon not found: {provider.Icon.Light.Icon}");
        Assert.True(File.Exists(provider.Icon.Dark.Icon), $"Dark icon not found: {provider.Icon.Dark.Icon}");
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

        var provider = new ShortcutsCommandsProvider(manager, history: NewHistory());

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
        var provider = new ShortcutsCommandsProvider(manager, history: NewHistory());
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

        var provider = new ShortcutsCommandsProvider(manager, history: NewHistory());

        Assert.Empty(provider.TopLevelCommands());
    }

    [Fact]
    public void EmptiedExtensions_NoCommands()
    {
        Touch("a.cmd");
        var manager = Manager(P("**"), "");

        var provider = new ShortcutsCommandsProvider(manager, history: NewHistory());

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

    [Theory]
    [InlineData(".ps1", "pwsh")]
    [InlineData(".bat", "cmd.exe")]
    [InlineData(".cmd", "cmd.exe")]
    public void TopLevelCommands_Script_RunsWithoutArgumentsOnEnter(string extension, string program)
    {
        var path = Touch("tool" + extension);
        var started = new List<LaunchSpec>();
        var provider = new ShortcutsCommandsProvider(Manager(P("**"), extension), spec =>
        {
            started.Add(spec);
            return true;
        }, NewHistory());

        var item = Assert.Single(provider.TopLevelCommands());

        var command = Assert.IsType<RunScriptCommand>(Assert.IsType<CommandItem>(item).Command);
        Assert.Equal("Run", command.Name);
        Assert.Equal(path, command.Id);
        var result = command.Invoke();
        Assert.Equal(CommandResultKind.Dismiss, result.Kind);
        var spec = Assert.Single(started);
        Assert.Equal(ScriptLaunch.For(path, ""), spec);
        Assert.Equal(program, spec.FileName);
    }

    [Theory]
    [InlineData(".ps1")]
    [InlineData(".bat")]
    [InlineData(".cmd")]
    public void TopLevelCommands_Script_AsksForArgumentsOnCtrlEnter(string extension)
    {
        var path = Touch("tool" + extension);
        var provider = new ShortcutsCommandsProvider(Manager(P("**"), extension), spec => true, NewHistory());

        var item = Assert.Single(provider.TopLevelCommands());

        var more = Assert.Single(item.MoreCommands);
        var context = Assert.IsType<CommandContextItem>(more);
        var page = Assert.IsType<ScriptArgumentsPage>(context.Command);
        Assert.Equal("Run with arguments", page.Name);
        Assert.Equal(path + "#arguments", page.Id);
    }

    private ArgumentHistory NewHistory() =>
        new(Path.Combine(_root, "history-" + Guid.NewGuid().ToString("N") + ".json"));

    private static ScriptArgumentsPage ArgumentsPage(ICommandItem item)
    {
        var more = Assert.Single(item.MoreCommands);
        var context = Assert.IsType<CommandContextItem>(more);
        return Assert.IsType<ScriptArgumentsPage>(context.Command);
    }

    [Fact]
    public void TopLevelCommands_ScriptPages_ShareOneHistory()
    {
        var first = Touch("a.ps1");
        var second = Touch("b.ps1");
        var provider = new ShortcutsCommandsProvider(Manager(P("**"), ".ps1"), spec => true, NewHistory());
        var items = provider.TopLevelCommands();
        Assert.Equal(2, items.Length);
        var firstItem = items.Single(i => ArgumentsPage(i).Id == first + "#arguments");
        var secondItem = items.Single(i => ArgumentsPage(i).Id == second + "#arguments");

        var page = ArgumentsPage(firstItem);
        page.SearchText = "stop";
        var run = Assert.IsAssignableFrom<IInvokableCommand>(page.GetItems()[0].Command);
        run.Invoke(null!);

        var again = ArgumentsPage(firstItem);
        Assert.Equal("stop", Assert.Single(again.GetItems()).Title);
        Assert.Empty(ArgumentsPage(secondItem).GetItems());
    }

    [Theory]
    [InlineData(".url")]
    [InlineData(".lnk")]
    [InlineData(".exe")]
    public void TopLevelCommands_NotAScript_StartsThroughTheShell(string extension)
    {
        Touch("tool" + extension);
        var provider = new ShortcutsCommandsProvider(Manager(P("**"), extension), spec => true, NewHistory());

        var item = Assert.Single(provider.TopLevelCommands());

        Assert.IsType<StartFileCommand>(item.Command);
        Assert.Empty(item.MoreCommands);
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

    [Fact]
    public void Run_StartThrows_ShowsAToast()
    {
        Touch("tool.cmd");
        var provider = new ShortcutsCommandsProvider(
            Manager(P("**"), ".cmd"),
            _ => throw new InvalidOperationException("No file name"),
            NewHistory());
        var item = Assert.Single(provider.TopLevelCommands());
        var command = Assert.IsType<RunScriptCommand>(Assert.IsType<CommandItem>(item).Command);

        var result = command.Invoke();

        Assert.Equal(CommandResultKind.ShowToast, result.Kind);
        Assert.Equal("Could not start tool.cmd: No file name", ((IToastArgs)result.Args!).Message);
    }
}

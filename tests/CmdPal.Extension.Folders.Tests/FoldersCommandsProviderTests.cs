using CmdPal.Extension.Folders;
using Microsoft.CommandPalette.Extensions;

namespace CmdPal.Extension.Folders.Tests;

public sealed class FoldersCommandsProviderTests : IDisposable
{
    private static readonly string[] Cmd = [".cmd"];

    private readonly string _root = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "cpf-provider-" + Guid.NewGuid().ToString("N")));

    public FoldersCommandsProviderTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private string P(params string[] parts) => Path.Combine([_root, .. parts]);

    private string Touch(params string[] parts)
    {
        var path = P(parts);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "");
        return path;
    }

    private static ICommandItem[] Items(params string[] patterns)
    {
        var provider = new FoldersCommandsProvider(patterns, Cmd);
        return provider.TopLevelCommands();
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

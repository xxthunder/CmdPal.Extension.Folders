using FoldersCatalog;

namespace FoldersCatalog.Tests;

public sealed class CatalogTests : IDisposable
{
    private static readonly string[] Cmd = [".cmd"];

    private readonly string _root = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "cpf-catalog-" + Guid.NewGuid().ToString("N")));

    private string? _lockedFolder;

    public CatalogTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        if (!OperatingSystem.IsWindows() && _lockedFolder is not null && Directory.Exists(_lockedFolder))
        {
            File.SetUnixFileMode(_lockedFolder, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        Directory.Delete(_root, recursive: true);
    }

    private string P(params string[] parts) => Path.Combine([_root, .. parts]);

    private string Touch(params string[] parts)
    {
        var path = P(parts);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "");
        return path;
    }

    private static string[] Paths(IReadOnlyList<CatalogEntry> entries) =>
        entries.Select(e => e.FullPath).OrderBy(p => p, StringComparer.Ordinal).ToArray();

    private static string[] Sorted(params string[] paths) =>
        paths.OrderBy(p => p, StringComparer.Ordinal).ToArray();

    [Fact]
    public void Build_PlainFolder_ListsOnlyDirectFiles()
    {
        Touch("root", "a.cmd");
        Touch("root", "sub", "b.cmd");

        var entries = Catalog.Build([P("root")], Cmd);

        Assert.Equal(Sorted(P("root", "a.cmd")), Paths(entries));
    }

    [Fact]
    public void Build_RecursiveFolder_ListsNestedFiles()
    {
        Touch("root", "a.cmd");
        Touch("root", "sub", "b.cmd");

        var entries = Catalog.Build([P("root") + "/**"], Cmd);

        Assert.Equal(Sorted(P("root", "a.cmd"), P("root", "sub", "b.cmd")), Paths(entries));
    }

    [Fact]
    public void Build_Entry_HasNameWithoutExtensionAndFolder()
    {
        Touch("root", "sub", "b.cmd");

        var entry = Assert.Single(Catalog.Build([P("root") + "/**"], Cmd));

        Assert.Equal("b", entry.Name);
        Assert.Equal(P("root", "sub"), entry.Folder);
        Assert.Equal(P("root", "sub", "b.cmd"), entry.FullPath);
    }

    [Fact]
    public void Build_FiltersByExtensionCaseInsensitive()
    {
        Touch("root", "A.CMD");
        Touch("root", "b.txt");

        var entries = Catalog.Build([P("root")], Cmd);

        Assert.Equal(Sorted(P("root", "A.CMD")), Paths(entries));
    }

    [Fact]
    public void Build_AcceptsExtensionWithoutDot()
    {
        Touch("root", "a.cmd");

        var entries = Catalog.Build([P("root")], ["cmd"]);

        Assert.Equal(Sorted(P("root", "a.cmd")), Paths(entries));
    }

    [Fact]
    public void Build_EmptyExtensionList_YieldsNothing()
    {
        Touch("root", "a.cmd");

        var entries = Catalog.Build([P("root") + "/**"], []);

        Assert.Empty(entries);
    }

    [Fact]
    public void Build_MissingFolder_YieldsNothing()
    {
        Touch("root", "a.cmd");

        var entries = Catalog.Build([P("root", "nope") + "/**"], Cmd);

        Assert.Empty(entries);
    }

    [Fact]
    public void Build_UnknownVariable_YieldsNothing()
    {
        var entries = Catalog.Build(["%CPF_NO_SUCH_VAR%/**"], Cmd);

        Assert.Empty(entries);
    }

    [Fact]
    public void Build_FileReachedByTwoPatterns_AppearsOnce()
    {
        Touch("root", "a.cmd");
        Touch("root", "sub", "b.cmd");

        var entries = Catalog.Build([P("root"), P("root") + "/**"], Cmd);

        Assert.Equal(Sorted(P("root", "a.cmd"), P("root", "sub", "b.cmd")), Paths(entries));
    }

    [Fact]
    public void Build_SkipsHiddenFilesAndFolders()
    {
        Touch("root", ".h.cmd");
        Touch("root", ".hid", "c.cmd");
        Touch("root", "a.cmd");

        if (OperatingSystem.IsWindows())
        {
            // A leading dot means hidden only on Unix; Windows uses the Hidden attribute.
            File.SetAttributes(P("root", ".h.cmd"), File.GetAttributes(P("root", ".h.cmd")) | FileAttributes.Hidden);
            new DirectoryInfo(P("root", ".hid")).Attributes |= FileAttributes.Hidden;
        }

        var entries = Catalog.Build([P("root") + "/**"], Cmd);

        Assert.Equal(Sorted(P("root", "a.cmd")), Paths(entries));
    }

    [Fact]
    public void Build_FollowsFolderLink()
    {
        Touch("other", "x.cmd");
        Directory.CreateDirectory(P("root"));
        Directory.CreateSymbolicLink(P("root", "link"), P("other"));

        var entries = Catalog.Build([P("root") + "/**"], Cmd);

        Assert.Equal(Sorted(P("root", "link", "x.cmd")), Paths(entries));
    }

    [Fact]
    public void Build_FollowsRootLink()
    {
        Touch("root", "a.cmd");
        Directory.CreateSymbolicLink(P("rootlink"), P("root"));

        var entries = Catalog.Build([P("rootlink") + "/**"], Cmd);

        Assert.Equal(Sorted(P("rootlink", "a.cmd")), Paths(entries));
    }

    [Fact]
    public void Build_LinkBackUpTheTree_Terminates()
    {
        Touch("root", "a.cmd");
        Touch("root", "sub", "b.cmd");
        Directory.CreateSymbolicLink(P("root", "sub", "up"), P("root"));

        var entries = Catalog.Build([P("root") + "/**"], Cmd);

        Assert.Equal(Sorted(P("root", "a.cmd"), P("root", "sub", "b.cmd")), Paths(entries));
    }

    [Fact]
    public void Build_RootReachedThroughLinkedAncestor_ListsEachFileOnce()
    {
        Touch("real", "root", "a.cmd");
        Touch("real", "root", "sub", "b.cmd");
        Directory.CreateSymbolicLink(P("real", "root", "sub", "up"), P("real", "root"));
        Directory.CreateSymbolicLink(P("alias"), P("real"));

        var entries = Catalog.Build([Path.Combine(P("alias", "root"), "**")], Cmd);

        Assert.Equal(2, entries.Count);
        Assert.Single(entries, e => e.Name == "a");
        Assert.Single(entries, e => e.Name == "b");
    }

    [Fact]
    public void Build_LinkTargetThroughLinkedAncestor_ListsEachFileOnce()
    {
        Touch("real", "root", "a.cmd");
        Touch("real", "root", "sub", "b.cmd");
        Directory.CreateSymbolicLink(P("alias"), P("real"));
        Directory.CreateSymbolicLink(P("real", "root", "sub", "up"), P("alias", "root"));

        var entries = Catalog.Build([Path.Combine(P("real", "root"), "**")], Cmd);

        Assert.Equal(2, entries.Count);
        Assert.Single(entries, e => e.Name == "a");
        Assert.Single(entries, e => e.Name == "b");
    }

    [Fact]
    public void Build_FolderReachedTwice_YieldsFilesOnce()
    {
        Touch("root", "sub", "b.cmd");
        Directory.CreateSymbolicLink(P("root", "again"), P("root", "sub"));

        var entries = Catalog.Build([P("root") + "/**"], Cmd);

        Assert.Single(entries, e => e.Name == "b");
        Assert.Single(entries);
    }

    [Fact]
    public void Build_DanglingFolderLink_IsSkipped()
    {
        Touch("root", "a.cmd");
        Directory.CreateSymbolicLink(P("root", "dead"), P("root", "gone"));

        var entries = Catalog.Build([P("root") + "/**"], Cmd);

        Assert.Equal(Sorted(P("root", "a.cmd")), Paths(entries));
    }

    [UnixFact]
    [System.Runtime.Versioning.UnsupportedOSPlatform("windows")]
    public void Build_UnreadableSubfolder_IsSkipped()
    {
        Touch("root", "a.cmd");
        Touch("root", "locked", "c.cmd");
        _lockedFolder = P("root", "locked");
        File.SetUnixFileMode(_lockedFolder, UnixFileMode.None);

        var entries = Catalog.Build([P("root") + "/**"], Cmd);

        Assert.Equal(Sorted(P("root", "a.cmd")), Paths(entries));
    }

    [Fact]
    public void Build_DanglingRootLink_YieldsNothing()
    {
        Directory.CreateSymbolicLink(P("rootlink"), P("gone"));

        var entries = Catalog.Build([P("rootlink") + "/**"], Cmd);

        Assert.Empty(entries);
    }

    [WindowsFact]
    public void Build_FileNamedOnlyByExtension_UsesFullFileName()
    {
        Touch("root", ".cmd");

        var entries = Catalog.Build([P("root") + "/**"], Cmd);

        var entry = Assert.Single(entries);
        Assert.Equal(".cmd", entry.Name);
    }

    [Fact]
    public void Build_IsOrderedByFullPath()
    {
        Touch("root", "b.cmd");
        Touch("root", "A.cmd");
        Touch("root", "sub", "a.cmd");
        Touch("root", "C.cmd");

        var entries = Catalog.Build([P("root") + "/**"], Cmd);

        var actual = entries.Select(e => e.FullPath).ToArray();
        var expected = actual.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.Equal(4, actual.Length);
        Assert.Equal(expected, actual);
    }
}

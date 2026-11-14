using FoldersCatalog;

namespace FoldersCatalog.Tests;

public sealed class ArgumentHistoryTests : IDisposable
{
    private readonly string _root = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "cps-arghistory-" + Guid.NewGuid().ToString("N")));

    private string HistoryFile => Path.Combine(_root, "sub", "folder", "history.json");

    public ArgumentHistoryTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private ArgumentHistory New() => new(HistoryFile);

    private const string Script = "tools/px-proxy.ps1";

    [Fact]
    public void For_UnknownScript_Empty() =>
        Assert.Empty(New().For(Script));

    [Fact]
    public void Add_NewestFirst()
    {
        var history = New();

        history.Add(Script, "start");
        history.Add(Script, "stop");

        Assert.Equal(["stop", "start"], history.For(Script));
    }

    [Fact]
    public void Add_Repeated_MovesToFrontOnce()
    {
        var history = New();

        history.Add(Script, "start");
        history.Add(Script, "stop");
        history.Add(Script, "start");

        Assert.Equal(["start", "stop"], history.For(Script));
    }

    [Fact]
    public void Add_TrimsArguments()
    {
        var history = New();

        history.Add(Script, "  stop  ");

        Assert.Equal(["stop"], history.For(Script));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_Blank_NotRecorded(string arguments)
    {
        var history = New();

        history.Add(Script, arguments);

        Assert.Empty(history.For(Script));
        Assert.False(File.Exists(HistoryFile));
    }

    [Fact]
    public void Add_Eleventh_DropsTheOldest()
    {
        var history = New();

        for (var i = 0; i <= 10; i++)
        {
            history.Add(Script, $"a{i}");
        }

        var result = history.For(Script);
        Assert.Equal(ArgumentHistory.Limit, result.Count);
        Assert.Equal("a10", result[0]);
        Assert.Equal("a1", result[^1]);
    }

    [Fact]
    public void Add_KeepsQuotesAndSpacesVerbatim()
    {
        var history = New();

        history.Add(Script, "one \"two three\"");

        Assert.Equal(["one \"two three\""], history.For(Script));
    }

    [Fact]
    public void Add_SeparateListsPerPath()
    {
        var history = New();
        var x = "x/px-proxy.ps1";
        var y = "y/px-proxy.ps1";

        history.Add(x, "start");
        history.Add(y, "stop");

        Assert.Equal(["start"], history.For(x));
        Assert.Equal(["stop"], history.For(y));
    }

    [Fact]
    public void For_PathIgnoresCase()
    {
        var history = New();

        history.Add("C:/Scripts/Tools/px.ps1", "start");

        Assert.Equal(["start"], history.For("C:/Scripts/tools/PX.ps1"));
    }

    [Fact]
    public void NewInstance_ReadsWhatWasAdded()
    {
        New().Add(Script, "start");

        Assert.Equal(["start"], New().For(Script));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("[1,2]")]
    public void Constructor_CorruptOrEmptyFile_EmptyHistory(string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(HistoryFile)!);
        File.WriteAllText(HistoryFile, content);

        var history = New();

        Assert.Empty(history.For(Script));

        history.Add(Script, "start");

        Assert.Equal(["start"], history.For(Script));
        Assert.Equal(["start"], New().For(Script));
    }

    [UnixNonRootFact]
    [System.Runtime.Versioning.UnsupportedOSPlatform("windows")]
    public void Constructor_UnreadableFile_EmptyHistory()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(HistoryFile)!);
        File.WriteAllText(HistoryFile, "{\"tools/px-proxy.ps1\":[\"stop\"]}");
        File.SetUnixFileMode(HistoryFile, UnixFileMode.None);

        try
        {
            var history = New();

            Assert.Empty(history.For(Script));
        }
        finally
        {
            File.SetUnixFileMode(HistoryFile, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    [Fact]
    public void Constructor_NullEntries_LeftOut()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(HistoryFile)!);
        File.WriteAllText(HistoryFile, "{\"x.ps1\":[null,\"stop\"]}");

        var history = New();

        Assert.Equal(["stop"], history.For("x.ps1"));
        Assert.Equal(["stop"], ArgumentHistory.Matching(history.For("x.ps1"), "st"));
    }

    [Fact]
    public void Matching_BlankSearch_AllInOrder()
    {
        var earlier = new[] { "stop", "start" };

        Assert.Equal(earlier, ArgumentHistory.Matching(earlier, ""));
        Assert.Equal(earlier, ArgumentHistory.Matching(earlier, "   "));
    }

    [Fact]
    public void Matching_FiltersIgnoringCase()
    {
        var earlier = new[] { "stop", "status --verbose", "start" };

        Assert.Equal(["stop", "status --verbose", "start"], ArgumentHistory.Matching(earlier, "ST"));
        Assert.Equal(["stop"], ArgumentHistory.Matching(earlier, "sto"));
    }

    [Fact]
    public void Matching_EntryEqualToSearch_LeftOut()
    {
        var earlier = new[] { "stop", "start" };

        Assert.Empty(ArgumentHistory.Matching(earlier, "stop"));
        Assert.Equal(["start"], ArgumentHistory.Matching(earlier, " sta "));
    }
}

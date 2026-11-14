namespace FoldersCatalog.Tests;

public sealed class CatalogCliTests : IDisposable
{
    private const string UsageLine =
        "Usage: dotnet run tools/catalog.cs -- --ext \"<extensions>\" <pattern>...";

    private readonly string _root = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "cpf-cli-" + Guid.NewGuid().ToString("N")));

    public CatalogCliTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private string P(params string[] parts) => Path.Combine([_root, .. parts]);

    private void Touch(params string[] parts)
    {
        var path = P(parts);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "");
    }

    private static string[] Lines(string text) =>
        text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.TrimEnd('\r'))
            .ToArray();

    private string[] TwoEntryLines() =>
    [
        $"a\t{_root}\t{P("a.cmd")}",
        $"b\t{P("sub")}\t{P("sub", "b.cmd")}",
        "2 entries",
    ];

    private void TwoCmdTree()
    {
        Touch("a.cmd");
        Touch("sub", "b.cmd");
        Touch("c.txt");
    }

    [Fact]
    public void Cli_ListsEntriesThenCount()
    {
        TwoCmdTree();

        var (code, stdout, stderr) = ToolRunner.Run("catalog.cs", "--ext", ".cmd", _root + "/**");

        Assert.True(code == 0, stderr);
        Assert.Equal(TwoEntryLines(), Lines(stdout));
    }

    [Fact]
    public void Cli_ExtAfterPatterns_IsStillTheOption()
    {
        TwoCmdTree();

        var (code, stdout, stderr) = ToolRunner.Run("catalog.cs", _root + "/**", "--ext", ".cmd");

        Assert.True(code == 0, stderr);
        Assert.Equal(TwoEntryLines(), Lines(stdout));
    }

    [Fact]
    public void Cli_NothingFound_PrintsZero()
    {
        var (code, stdout, stderr) = ToolRunner.Run("catalog.cs", "--ext", ".cmd", P("nope"));

        Assert.True(code == 0, stderr);
        Assert.Equal(["0 entries"], Lines(stdout));
    }

    [Fact]
    public void Cli_WithoutPatterns_PrintsUsageAndExits2()
    {
        var (code, _, stderr) = ToolRunner.Run("catalog.cs", "--ext", ".cmd");

        Assert.Equal(2, code);
        Assert.StartsWith(UsageLine, stderr);
    }

    [Fact]
    public void Cli_WithoutExt_PrintsUsageAndExits2()
    {
        var (code, _, stderr) = ToolRunner.Run("catalog.cs", _root);

        Assert.Equal(2, code);
        Assert.StartsWith(UsageLine, stderr);
    }

    [Fact]
    public void Cli_ExtWithoutValue_PrintsUsageAndExits2()
    {
        var (code, _, stderr) = ToolRunner.Run("catalog.cs", _root, "--ext");

        Assert.Equal(2, code);
        Assert.StartsWith(UsageLine, stderr);
    }

    [Fact]
    public void Cli_EmptyExtValue_PrintsUsageAndExits2()
    {
        var (code, _, stderr) = ToolRunner.Run("catalog.cs", "--ext", "", _root);

        Assert.Equal(2, code);
        Assert.StartsWith(UsageLine, stderr);
    }

    [Fact]
    public void Cli_BlankPattern_PrintsUsageAndExits2()
    {
        var (code, _, stderr) = ToolRunner.Run("catalog.cs", "--ext", ".cmd", "   ");

        Assert.Equal(2, code);
        Assert.StartsWith(UsageLine, stderr);
        Assert.DoesNotContain("   at ", stderr);
    }
}

using FoldersCatalog;

namespace FoldersCatalog.Tests;

public sealed class FolderPatternTests : IDisposable
{
    private readonly string _root = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "cpf-pattern-" + Guid.NewGuid().ToString("N")));

    public FolderPatternTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Parse_PlainFolder_IsNotRecursive()
    {
        var pattern = FolderPattern.Parse(_root);

        Assert.Equal(Path.GetFullPath(_root), Path.GetFullPath(pattern.Folder));
        Assert.False(pattern.Recursive);
    }

    [Fact]
    public void Parse_BackslashStarStar_IsRecursive()
    {
        var pattern = FolderPattern.Parse(_root + "\\**");

        Assert.Equal(Path.GetFullPath(_root), Path.GetFullPath(pattern.Folder));
        Assert.True(pattern.Recursive);
    }

    [Fact]
    public void Parse_RootRecursive_KeepsTheRoot()
    {
        if (OperatingSystem.IsWindows()) return;

        var pattern = FolderPattern.Parse("/**");

        Assert.Equal("/", pattern.Folder);
        Assert.True(pattern.Recursive);
    }

    [Fact]
    public void Parse_SlashStarStar_IsRecursive()
    {
        var pattern = FolderPattern.Parse(_root + "/**");

        Assert.Equal(Path.GetFullPath(_root), Path.GetFullPath(pattern.Folder));
        Assert.True(pattern.Recursive);
    }

    [Fact]
    public void Parse_ExpandsEnvironmentVariable()
    {
        const string name = "CPF_TEST_ROOT";
        var previous = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, _root);
        try
        {
            var pattern = FolderPattern.Parse("%CPF_TEST_ROOT%/**");

            Assert.Equal(Path.GetFullPath(_root), Path.GetFullPath(pattern.Folder));
            Assert.True(pattern.Recursive);
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, previous);
        }
    }

    [Fact]
    public void Parse_LeavesUnknownVariableLiteral()
    {
        var pattern = FolderPattern.Parse("%CPF_NO_SUCH_VAR%");

        Assert.EndsWith("%CPF_NO_SUCH_VAR%", pattern.Folder);
    }

    [Fact]
    public void Parse_ExpandsLeadingTilde()
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var sub = FolderPattern.Parse("~/shortcuts/**");
        Assert.Equal(
            Path.GetFullPath(Path.Combine(profile, "shortcuts")),
            Path.GetFullPath(sub.Folder));
        Assert.True(sub.Recursive);

        var home = FolderPattern.Parse("~");
        Assert.Equal(Path.GetFullPath(profile), Path.GetFullPath(home.Folder));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_BlankPattern_Throws(string blank)
    {
        Assert.Throws<ArgumentException>(() => FolderPattern.Parse(blank));
    }
}

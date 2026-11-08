using FoldersCatalog;

namespace FoldersCatalog.Tests;

public sealed class CatalogSettingsTests
{
    [Fact]
    public void ParsePatterns_OnePerLine()
    {
        Assert.Equal(["a", "b"], CatalogSettings.ParsePatterns("a\nb"));
        Assert.Equal(["a", "b"], CatalogSettings.ParsePatterns("a\r\nb"));
    }

    [Fact]
    public void ParsePatterns_TrimsAndDropsBlankLines()
    {
        Assert.Equal(["a", "b"], CatalogSettings.ParsePatterns("  a  \n\n   \nb\n"));
    }

    [Fact]
    public void ParsePatterns_Empty_IsEmpty()
    {
        Assert.Empty(CatalogSettings.ParsePatterns(""));
    }

    [Fact]
    public void ParseExtensions_SplitsOnWhitespace()
    {
        string[] expected = [".url", ".lnk", ".cmd"];

        Assert.Equal(expected, CatalogSettings.ParseExtensions(".url .lnk  .cmd"));
        Assert.Equal(expected, CatalogSettings.ParseExtensions(".url\t.lnk\n.cmd"));
    }

    [Fact]
    public void ParseExtensions_Empty_IsEmpty()
    {
        Assert.Empty(CatalogSettings.ParseExtensions(""));
        Assert.Empty(CatalogSettings.ParseExtensions("   "));
    }
}

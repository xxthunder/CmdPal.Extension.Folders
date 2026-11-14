using SkiaSharp;

namespace FoldersCatalog.Tests;

[Collection(LogosTool.CollectionName)]
public sealed class AssetsTests
{
    private const string SourceRelative = "src/CmdPal.Extension.Shortcuts/Assets/Source/Logo.png";
    private const string AssetsRelative = "src/CmdPal.Extension.Shortcuts/Assets";
    private const string Rerun =
        "dotnet run tools/logos.cs -- src/CmdPal.Extension.Shortcuts/Assets/Source/Logo.png src/CmdPal.Extension.Shortcuts/Assets";

    private static string[] PngNames(string dir) =>
        Directory.GetFiles(dir, "*.png").Select(f => Path.GetFileName(f)!).Order(StringComparer.Ordinal).ToArray();

    private const int Tolerance = 2;

    // Skia picks CPU-specific raster paths at run time, so bytes can differ between machines
    // without a real change; compare decoded pixels with a small per-channel tolerance instead.
    private static void AssertSimilar(string freshPath, string committedPath, string name)
    {
        var hint = $"{name} differs from the rendering of the source. Re-run: {Rerun}";
        using var fresh = SKBitmap.Decode(freshPath);
        using var committed = SKBitmap.Decode(committedPath);
        Assert.True(fresh is not null && committed is not null, $"{name} cannot be decoded. Re-run: {Rerun}");
        Assert.True(
            fresh.Width == committed.Width && fresh.Height == committed.Height,
            $"{name} has size {committed.Width}x{committed.Height}, the rendering {fresh.Width}x{fresh.Height}. Re-run: {Rerun}");

        var a = fresh.Pixels;
        var b = committed.Pixels;
        for (var i = 0; i < a.Length; i++)
        {
            var worst = Math.Max(
                Math.Max(Math.Abs(a[i].Red - b[i].Red), Math.Abs(a[i].Green - b[i].Green)),
                Math.Max(Math.Abs(a[i].Blue - b[i].Blue), Math.Abs(a[i].Alpha - b[i].Alpha)));
            Assert.True(
                worst <= Tolerance,
                $"{hint} (pixel {i % fresh.Width},{i / fresh.Width}: channel off by {worst}, allowed {Tolerance})");
        }
    }

    [Fact]
    public void Assets_AreTheRenderingOfTheSource()
    {
        var root = ToolRunner.RepoRoot();
        var source = Path.Combine(root, SourceRelative.Replace('/', Path.DirectorySeparatorChar));
        var assets = Path.Combine(root, AssetsRelative.Replace('/', Path.DirectorySeparatorChar));
        var rendered = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), "cpf-assets-" + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(rendered);

        try
        {
            var (code, _, stderr) = ToolRunner.Run("logos.cs", source, rendered);

            Assert.True(code == 0, $"logos.cs exited {code} for {SourceRelative}: {stderr}");
            Assert.Equal(PngNames(rendered), PngNames(assets));
            foreach (var name in PngNames(rendered))
            {
                AssertSimilar(Path.Combine(rendered, name), Path.Combine(assets, name), name);
            }
        }
        finally
        {
            Directory.Delete(rendered, recursive: true);
        }
    }
}

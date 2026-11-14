using System.Diagnostics;

namespace FoldersCatalog.Tests;

public sealed class LogosCliTests : IDisposable
{
    private const string UsageLine =
        "Usage: dotnet run tools/logos.cs -- <source.png> <assets-dir>";

    // 12x8 RGBA PNG.
    private const string SourcePngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAwAAAAICAYAAADN5B7xAAAANElEQVR4nN3OoREAIAzAwO9dR6ju/qOwVREMAJqoiIgESzGFIm6e2jgEd0/9HmNSv8c+WdqC8SKTzQGwrwAAAABJRU5ErkJggg==";

    private static readonly Dictionary<string, (int Width, int Height)> Logos = new()
    {
        ["StoreLogo.png"] = (50, 50),
        ["Square44x44Logo.scale-200.png"] = (88, 88),
        ["Square44x44Logo.targetsize-16.png"] = (16, 16),
        ["Square44x44Logo.targetsize-24.png"] = (24, 24),
        ["Square44x44Logo.targetsize-32.png"] = (32, 32),
        ["Square44x44Logo.targetsize-48.png"] = (48, 48),
        ["Square44x44Logo.targetsize-16_altform-unplated.png"] = (16, 16),
        ["Square44x44Logo.targetsize-24_altform-unplated.png"] = (24, 24),
        ["Square44x44Logo.targetsize-32_altform-unplated.png"] = (32, 32),
        ["Square44x44Logo.targetsize-48_altform-unplated.png"] = (48, 48),
        ["Square150x150Logo.scale-200.png"] = (300, 300),
        ["Wide310x150Logo.scale-200.png"] = (620, 300),
        ["SplashScreen.scale-200.png"] = (1240, 600),
        ["LockScreenLogo.scale-200.png"] = (48, 48),
    };

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(120);

    private readonly string _root = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "cpf-logos-" + Guid.NewGuid().ToString("N")));

    public LogosCliTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private string P(params string[] parts) => Path.Combine([_root, .. parts]);

    private string WriteSource()
    {
        var path = P("source.png");
        File.WriteAllBytes(path, Convert.FromBase64String(SourcePngBase64));
        return path;
    }

    private static string RepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "scripts", "gate")))
            {
                return dir.FullName;
            }
        }

        throw new InvalidOperationException("Repository root (scripts/gate) not found above " + AppContext.BaseDirectory);
    }

    private static (int ExitCode, string Stdout, string Stderr) Run(params string[] args)
    {
        var info = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        info.ArgumentList.Add("run");
        info.ArgumentList.Add(Path.Combine(RepoRoot(), "tools", "logos.cs"));
        info.ArgumentList.Add("--");
        foreach (var arg in args)
        {
            info.ArgumentList.Add(arg);
        }

        using var process = Process.Start(info)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(Timeout))
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
            Assert.Fail($"dotnet run did not exit within {Timeout.TotalSeconds} s. stderr:\n{stderr.GetAwaiter().GetResult()}");
        }

        return (process.ExitCode, stdout.GetAwaiter().GetResult(), stderr.GetAwaiter().GetResult());
    }

    private static (int Width, int Height) PngSize(string path)
    {
        var bytes = File.ReadAllBytes(path);
        byte[] signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.True(bytes.Length >= 24, $"{path} is too short to be a PNG");
        Assert.Equal(signature, bytes[..8]);

        static int BigEndian(byte[] b, int at) =>
            (b[at] << 24) | (b[at + 1] << 16) | (b[at + 2] << 8) | b[at + 3];

        return (BigEndian(bytes, 16), BigEndian(bytes, 20));
    }

    private string[] PngNames(string dir) =>
        Directory.Exists(dir)
            ? Directory.GetFiles(dir, "*.png").Select(f => Path.GetFileName(f)!).Order(StringComparer.Ordinal).ToArray()
            : [];

    [Fact]
    public void Cli_RendersEveryLogoAtItsSize()
    {
        var source = WriteSource();
        var assets = P("assets");
        Directory.CreateDirectory(assets);

        var (code, _, stderr) = Run(source, assets);

        Assert.True(code == 0, stderr);
        Assert.Equal(Logos.Keys.Order(StringComparer.Ordinal).ToArray(), PngNames(assets));
        foreach (var (name, size) in Logos)
        {
            Assert.Equal(size, PngSize(Path.Combine(assets, name)));
        }
    }

    [Fact]
    public void Cli_SecondRun_OverwritesWithTheSameBytes()
    {
        var source = WriteSource();
        var assets = P("assets");
        Directory.CreateDirectory(assets);

        var (code1, _, stderr1) = Run(source, assets);
        Assert.True(code1 == 0, stderr1);
        var first = PngNames(assets).ToDictionary(n => n, n => File.ReadAllBytes(Path.Combine(assets, n)));

        var (code2, _, stderr2) = Run(source, assets);
        Assert.True(code2 == 0, stderr2);

        Assert.Equal(Logos.Keys.Order(StringComparer.Ordinal).ToArray(), PngNames(assets));
        foreach (var (name, bytes) in first)
        {
            Assert.Equal(bytes, File.ReadAllBytes(Path.Combine(assets, name)));
        }
    }

    [Fact]
    public void Cli_WithoutArguments_PrintsUsageAndExits2()
    {
        var (code, _, stderr) = Run();

        Assert.Equal(2, code);
        Assert.StartsWith(UsageLine, stderr);
    }

    [Fact]
    public void Cli_SourceMissing_Exits1AndNamesIt()
    {
        var source = P("missing.png");
        var assets = P("assets");
        Directory.CreateDirectory(assets);

        var (code, _, stderr) = Run(source, assets);

        Assert.Equal(1, code);
        Assert.Contains(source, stderr);
        Assert.Empty(PngNames(assets));
    }
}

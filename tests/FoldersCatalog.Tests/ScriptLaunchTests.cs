using FoldersCatalog;

namespace FoldersCatalog.Tests;

public sealed class ScriptLaunchTests
{
    private static string Dir() => Path.Combine(Path.GetTempPath(), "cps-script-launch");

    private static string In(string folder, string file) => Path.Combine(folder, file);

    [Theory]
    [InlineData("x.ps1")]
    [InlineData("x.bat")]
    [InlineData("x.cmd")]
    [InlineData("X.PS1")]
    [InlineData("X.Bat")]
    public void IsScript_ScriptExtensions_True(string path) =>
        Assert.True(ScriptLaunch.IsScript(path));

    [Theory]
    [InlineData("x.url")]
    [InlineData("x.lnk")]
    [InlineData("x.exe")]
    [InlineData("x.txt")]
    [InlineData("x")]
    public void IsScript_OtherFiles_False(string path) =>
        Assert.False(ScriptLaunch.IsScript(path));

    [Fact]
    public void For_Ps1_RunsPwshWithTheProfileInItsFolder()
    {
        var path = In(Dir(), "px-proxy.ps1");

        var spec = ScriptLaunch.For(path, "stop");

        Assert.Equal("pwsh", spec.FileName);
        Assert.Equal($"-ExecutionPolicy Bypass -File \"{path}\" stop", spec.Arguments);
        Assert.Equal(Path.GetDirectoryName(path), spec.WorkingDirectory);
        Assert.DoesNotContain("-NoProfile", spec.Arguments);
    }

    [Fact]
    public void For_Bat_RunsCmdInItsFolder()
    {
        var path = In(Dir(), "px-proxy.bat");

        var spec = ScriptLaunch.For(path, "stop");

        Assert.Equal("cmd.exe", spec.FileName);
        Assert.Equal($"/s /c \"\"{path}\" stop\"", spec.Arguments);
        Assert.Equal(Path.GetDirectoryName(path), spec.WorkingDirectory);
    }

    [Fact]
    public void For_Cmd_RunsCmd()
    {
        var path = In(Dir(), "px-proxy.cmd");

        var spec = ScriptLaunch.For(path, "stop");

        Assert.Equal("cmd.exe", spec.FileName);
        Assert.Equal($"/s /c \"\"{path}\" stop\"", spec.Arguments);
        Assert.Equal(Path.GetDirectoryName(path), spec.WorkingDirectory);
    }

    [Theory]
    [InlineData("x.ps1", "")]
    [InlineData("x.ps1", "   ")]
    [InlineData("x.bat", "")]
    [InlineData("x.bat", "   ")]
    public void For_EmptyArguments_NoTrailingSpace(string file, string arguments)
    {
        var path = In(Dir(), file);

        var spec = ScriptLaunch.For(path, arguments);

        var expected = file.EndsWith(".ps1", StringComparison.Ordinal)
            ? $"-ExecutionPolicy Bypass -File \"{path}\""
            : $"/s /c \"\"{path}\"\"";
        Assert.Equal(expected, spec.Arguments);
    }

    [Theory]
    [InlineData("x.ps1")]
    [InlineData("x.bat")]
    public void For_ArgumentsAreVerbatim(string file)
    {
        var path = In(Dir(), file);
        const string args = "start \"a b\" --x";

        var spec = ScriptLaunch.For(path, args);

        Assert.Contains($"\"{path}\" {args}", spec.Arguments);
    }

    [Theory]
    [InlineData("x.ps1")]
    [InlineData("x.bat")]
    public void For_PathWithSpacesAndAmpersand_IsQuoted(string file)
    {
        var path = In(Path.Combine(Dir(), "a b & (c)"), file);

        var spec = ScriptLaunch.For(path, "go");

        Assert.Contains($"\"{path}\" go", spec.Arguments);
        Assert.Equal(Path.GetDirectoryName(path), spec.WorkingDirectory);
    }

    [Fact]
    public void For_UpperCaseExtension_IsAScript()
    {
        var spec = ScriptLaunch.For(In(Dir(), "X.PS1"), "");

        Assert.Equal("pwsh", spec.FileName);
    }

    [Fact]
    public void For_NotAScript_Throws() =>
        Assert.Throws<ArgumentException>(() => ScriptLaunch.For(In(Dir(), "x.url"), "stop"));
}

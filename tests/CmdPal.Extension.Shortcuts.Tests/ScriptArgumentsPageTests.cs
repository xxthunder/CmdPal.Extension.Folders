using System.ComponentModel;
using CmdPal.Extension.Shortcuts;
using FoldersCatalog;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPal.Extension.Shortcuts.Tests;

public sealed class ScriptArgumentsPageTests
{
    private static readonly string Path1 = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "tools", "px-proxy.ps1");

    private static ScriptArgumentsPage Page(List<LaunchSpec> started, bool result = true) =>
        new(Path1, spec =>
        {
            started.Add(spec);
            return result;
        });

    private static StringParameterRun Field(ScriptArgumentsPage page) =>
        Assert.IsType<StringParameterRun>(page.Parameters[1]);

    private static ICommandResult Submit(ScriptArgumentsPage page) =>
        ((IInvokableCommand)page.Command.Command).Invoke(null!);

    [Fact]
    public void Page_HasOneTextFieldAfterTheScriptName()
    {
        var page = Page([]);

        Assert.Equal(2, page.Parameters.Length);
        var label = Assert.IsType<LabelRun>(page.Parameters[0]);
        Assert.Equal("px-proxy.ps1", label.Text);
        Assert.Equal("arguments", Field(page).PlaceholderText);
    }

    [Fact]
    public void Page_IdIsThePathWithArguments()
    {
        Assert.Equal(Path1 + "#arguments", Page([]).Id);
        Assert.Equal("Run with arguments", Page([]).Name);
    }

    [Fact]
    public void Submit_StartsTheScriptWithTheTypedArguments()
    {
        var started = new List<LaunchSpec>();
        var page = Page(started);
        Field(page).Text = "stop";

        var result = Submit(page);

        Assert.Equal(ScriptLaunch.For(Path1, "stop"), Assert.Single(started));
        Assert.Equal(CommandResultKind.Dismiss, result.Kind);
    }

    [Fact]
    public void Submit_ClearsTheFieldAfterAStart()
    {
        var page = Page([]);
        var field = Field(page);
        field.Text = "stop";

        Submit(page);

        Assert.True(string.IsNullOrEmpty(field.Text));
    }

    [Fact]
    public void Submit_FailedStart_KeepsOpenAndKeepsTheText()
    {
        var page = Page([], result: false);
        var field = Field(page);
        field.Text = "stop";

        var result = Submit(page);

        Assert.Equal(CommandResultKind.KeepOpen, result.Kind);
        Assert.Equal("stop", field.Text);
    }

    [Fact]
    public void Submit_StartThrows_ShowsAToastAndKeepsTheText()
    {
        var page = new ScriptArgumentsPage(
            Path1,
            _ => throw new Win32Exception("The system cannot find the file specified."));
        var field = Field(page);
        field.Text = "stop";

        var result = Submit(page);

        Assert.Equal(CommandResultKind.ShowToast, result.Kind);
        Assert.Equal(
            "Could not start px-proxy.ps1: The system cannot find the file specified.",
            ((IToastArgs)result.Args!).Message);
        Assert.Equal("stop", field.Text);
    }
}

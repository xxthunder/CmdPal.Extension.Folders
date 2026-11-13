using System.ComponentModel;
using CmdPal.Extension.Shortcuts;
using FoldersCatalog;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPal.Extension.Shortcuts.Tests;

public sealed class ScriptArgumentsPageTests : IDisposable
{
    private static readonly string Path1 = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "tools", "px-proxy.ps1");

    private readonly string _root = System.IO.Path.GetFullPath(
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), "cpf-args-" + Guid.NewGuid().ToString("N")));

    public ScriptArgumentsPageTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private ArgumentHistory NewHistory() =>
        new(System.IO.Path.Combine(_root, "history-" + Guid.NewGuid().ToString("N") + ".json"));

    private static ScriptArgumentsPage Page(ArgumentHistory history, List<LaunchSpec> started, bool result = true) =>
        new(Path1, history, spec =>
        {
            started.Add(spec);
            return result;
        });

    private static string[] Titles(ScriptArgumentsPage page) =>
        page.GetItems().Select(item => item.Title).ToArray();

    private static ICommandResult Run(IListItem item) =>
        Assert.IsAssignableFrom<IInvokableCommand>(item.Command).Invoke(null!);

    private static ICommandResult RunTyped(ScriptArgumentsPage page, string text)
    {
        page.SearchText = text;
        return Run(page.GetItems()[0]);
    }

    [Fact]
    public void Page_IdNameAndPlaceholder()
    {
        var page = Page(NewHistory(), []);

        Assert.Equal(Path1 + "#arguments", page.Id);
        Assert.Equal("Run with arguments", page.Name);
        Assert.Equal("arguments", page.PlaceholderText);
        Assert.Equal("px-proxy.ps1", page.Title);
    }

    [Fact]
    public void NoHistory_BlankSearch_NoItems()
    {
        var page = Page(NewHistory(), []);

        Assert.Empty(page.GetItems());
        var empty = Assert.IsType<CommandItem>(page.EmptyContent);
        Assert.Equal("Type the arguments", empty.Title);
    }

    [Fact]
    public void Search_FirstItemRunsWithTheSearchText()
    {
        var started = new List<LaunchSpec>();
        var page = Page(NewHistory(), started);

        page.SearchText = " stop ";
        var items = page.GetItems();

        Assert.Equal("Run with \"stop\"", items[0].Title);
        var result = Run(items[0]);
        Assert.Equal(ScriptLaunch.For(Path1, "stop"), Assert.Single(started));
        Assert.Equal(CommandResultKind.Dismiss, result.Kind);
    }

    [Fact]
    public void WhitespaceSearch_NoRunWithItem()
    {
        var page = Page(NewHistory(), []);

        page.SearchText = "   ";

        Assert.DoesNotContain(Titles(page), title => title.StartsWith("Run with ", StringComparison.Ordinal));
    }

    [Fact]
    public void SuccessfulStart_IsOfferedNextTime()
    {
        var started = new List<LaunchSpec>();
        var history = NewHistory();
        RunTyped(Page(history, started), "stop");
        started.Clear();

        Assert.Equal(["stop"], history.For(Path1));
        var next = Page(history, started);
        var item = Assert.Single(next.GetItems());
        Assert.Equal("stop", item.Title);
        Run(item);
        Assert.Equal(ScriptLaunch.For(Path1, "stop"), Assert.Single(started));
    }

    [Fact]
    public void QuotedArguments_ReplayedVerbatim()
    {
        var started = new List<LaunchSpec>();
        var history = NewHistory();
        RunTyped(Page(history, started), "one \"two three\"");
        started.Clear();

        var item = Assert.Single(Page(history, started).GetItems());
        Run(item);

        Assert.Equal(ScriptLaunch.For(Path1, "one \"two three\""), Assert.Single(started));
    }

    [Fact]
    public void FailedStart_NotRecorded()
    {
        var history = NewHistory();
        var page = Page(history, [], result: false);

        var result = RunTyped(page, "stop");

        Assert.Equal(CommandResultKind.KeepOpen, result.Kind);
        Assert.Empty(history.For(Path1));
    }

    [Fact]
    public void StartThrows_ToastAndNotRecorded()
    {
        var history = NewHistory();
        var page = new ScriptArgumentsPage(
            Path1,
            history,
            _ => throw new Win32Exception("The system cannot find the file specified."));

        var result = RunTyped(page, "stop");

        Assert.Equal(CommandResultKind.ShowToast, result.Kind);
        Assert.Equal(
            "Could not start px-proxy.ps1: The system cannot find the file specified.",
            ((IToastArgs)result.Args!).Message);
        Assert.Empty(history.For(Path1));
    }

    [Fact]
    public void HistoryWriteFails_ScriptStillStarts()
    {
        var blocked = System.IO.Path.Combine(_root, "history.json");
        Directory.CreateDirectory(blocked);
        var history = new ArgumentHistory(blocked);
        var started = new List<LaunchSpec>();
        var page = Page(history, started);

        var result = RunTyped(page, "stop");

        Assert.Equal(ScriptLaunch.For(Path1, "stop"), Assert.Single(started));
        Assert.Equal(CommandResultKind.Dismiss, result.Kind);
    }

    [Fact]
    public void Search_FiltersEarlierEntries()
    {
        var history = NewHistory();
        history.Add(Path1, "start");
        history.Add(Path1, "stop");
        history.Add(Path1, "status");
        var page = Page(history, []);

        page.SearchText = "sto";

        Assert.Equal(["Run with \"sto\"", "stop"], Titles(page));
    }

    [Fact]
    public void Search_EqualToAnEntry_ListedOnce()
    {
        var history = NewHistory();
        history.Add(Path1, "stop");
        var page = Page(history, []);

        page.SearchText = "stop";

        Assert.Equal(["Run with \"stop\""], Titles(page));
    }
}

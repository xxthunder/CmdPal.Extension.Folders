using System;
using System.IO;
using System.Linq;
using FoldersCatalog;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPal.Extension.Shortcuts;

/// <summary>Lists a script's earlier arguments and runs it with the chosen or the typed ones.</summary>
public sealed partial class ScriptArgumentsPage : DynamicListPage
{
    private readonly string _path;
    private readonly ArgumentHistory _history;
    private readonly Func<LaunchSpec, bool> _start;

    public ScriptArgumentsPage(string path, ArgumentHistory history, Func<LaunchSpec, bool> start)
    {
        _path = path;
        _history = history;
        _start = start;
        Id = path + "#arguments";
        Name = "Run with arguments";
        Title = Path.GetFileName(path);
        PlaceholderText = "arguments";
        EmptyContent = new CommandItem { Title = "Type the arguments" };
    }

    public override IListItem[] GetItems()
    {
        var earlier = ArgumentHistory.Matching(_history.For(_path), SearchText)
            .Select(arguments => Item(arguments, arguments));

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return earlier.ToArray();
        }

        var typed = SearchText.Trim();
        return earlier.Prepend(Item($"Run with \"{typed}\"", typed)).ToArray();
    }

    public override void UpdateSearchText(string oldSearch, string newSearch) => RaiseItemsChanged();

    private ListItem Item(string title, string arguments) =>
        new(new RunScriptCommand(_path, () => arguments, spec => StartAndRecord(spec, arguments)))
        {
            Title = title,
        };

    private bool StartAndRecord(LaunchSpec spec, string arguments)
    {
        if (!_start(spec))
        {
            return false;
        }

        try
        {
            _history.Add(_path, arguments);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The script has already started; a history that cannot be written must not turn that into a failure.
        }

        // The page stays alive between openings, so it starts again with the full list.
        SearchText = string.Empty;
        return true;
    }
}

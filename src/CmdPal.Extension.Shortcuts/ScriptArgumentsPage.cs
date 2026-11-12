using System;
using System.IO;
using FoldersCatalog;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPal.Extension.Shortcuts;

/// <summary>Asks for the arguments of one script, then starts it.</summary>
public sealed partial class ScriptArgumentsPage : ParametersPage
{
    private readonly StringParameterRun _arguments = new("arguments");
    private readonly IParameterRun[] _parameters;
    private readonly ListItem _command;

    public ScriptArgumentsPage(string path, Func<LaunchSpec, bool> start)
    {
        Id = path + "#arguments";
        Name = "Run with arguments";
        _parameters = [new LabelRun(Path.GetFileName(path)), _arguments];
        _command = new ListItem(new RunScriptCommand(
            path,
            () => _arguments.Text,
            spec =>
            {
                if (!start(spec))
                {
                    return false;
                }

                _arguments.ClearValue();
                return true;
            }));
    }

    public override IParameterRun[] Parameters => _parameters;

    public override IListItem Command => _command;
}

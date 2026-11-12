using System;
using System.ComponentModel;
using System.IO;
using FoldersCatalog;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPal.Extension.Shortcuts;

/// <summary>Starts one script with the arguments a function supplies.</summary>
public sealed partial class RunScriptCommand : InvokableCommand
{
    private readonly string _path;
    private readonly Func<string> _arguments;
    private readonly Func<LaunchSpec, bool> _start;

    public RunScriptCommand(string path, Func<string> arguments, Func<LaunchSpec, bool> start)
    {
        _path = path;
        _arguments = arguments;
        _start = start;
        Id = path;
        Name = "Run";
    }

    public override ICommandResult Invoke()
    {
        try
        {
            return _start(ScriptLaunch.For(_path, _arguments()))
                ? CommandResult.Dismiss()
                : CommandResult.KeepOpen();
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            return CommandResult.ShowToast($"Could not start {Path.GetFileName(_path)}: {ex.Message}");
        }
    }
}

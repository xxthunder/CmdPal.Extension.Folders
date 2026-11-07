using System;
using System.IO;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPal.Extension.Folders;

/// <summary>Starts one file through the shell, in its own folder.</summary>
public sealed partial class StartFileCommand : InvokableCommand
{
    private readonly string _path;
    private readonly Func<string, string, bool> _start;

    public StartFileCommand(string path, Func<string, string, bool> start)
    {
        _path = path;
        _start = start;
        Id = path;
        Name = "Start";
    }

    public override ICommandResult Invoke()
    {
        _start(_path, Path.GetDirectoryName(_path)!);
        return CommandResult.Dismiss();
    }
}

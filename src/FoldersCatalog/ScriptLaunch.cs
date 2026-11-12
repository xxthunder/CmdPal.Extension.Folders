namespace FoldersCatalog;

/// <summary>How to start a script as a process: the program, its argument line, and the folder to run in.</summary>
public sealed record LaunchSpec(string FileName, string Arguments, string WorkingDirectory);

/// <summary>Describes how a <c>.ps1</c>, <c>.bat</c> or <c>.cmd</c> file runs, with arguments from the user.</summary>
public static class ScriptLaunch
{
    /// <summary>True for a <c>.ps1</c>, <c>.bat</c> or <c>.cmd</c> path, ignoring case.</summary>
    public static bool IsScript(string path) => Kind(path) is not null;

    /// <summary>
    /// The process call for a script: the path is quoted, the trimmed arguments follow verbatim,
    /// and the working directory is the script's folder. Throws for a path that is not a script.
    /// </summary>
    public static LaunchSpec For(string path, string arguments)
    {
        var kind = Kind(path) ?? throw new ArgumentException($"Not a script: {path}", nameof(path));
        var tail = string.IsNullOrWhiteSpace(arguments) ? "" : " " + arguments.Trim();
        var folder = Path.GetDirectoryName(path) ?? "";

        return kind == ".ps1"
            ? new LaunchSpec("pwsh", $"-ExecutionPolicy Bypass -File \"{path}\"{tail}", folder)
            : new LaunchSpec("cmd.exe", $"/s /c \"\"{path}\"{tail}\"", folder);
    }

    private static string? Kind(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".ps1" or ".bat" or ".cmd" ? ext : null;
    }
}

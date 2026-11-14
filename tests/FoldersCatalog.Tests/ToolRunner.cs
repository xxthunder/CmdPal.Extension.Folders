using System.Diagnostics;

namespace FoldersCatalog.Tests;

/// <summary>Runs a script under tools/ with dotnet run and captures its output.</summary>
internal static class ToolRunner
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(120);

    internal static string RepoRoot()
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

    internal static (int ExitCode, string Stdout, string Stderr) Run(string script, params string[] args)
    {
        var info = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        info.ArgumentList.Add("run");
        info.ArgumentList.Add(Path.Combine(RepoRoot(), "tools", script));
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
}

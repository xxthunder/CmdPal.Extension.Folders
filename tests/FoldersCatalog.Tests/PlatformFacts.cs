namespace FoldersCatalog.Tests;

/// <summary>A fact that runs on Unix-like systems only and shows as skipped elsewhere.</summary>
public sealed class UnixFactAttribute : FactAttribute
{
    public UnixFactAttribute()
    {
        if (OperatingSystem.IsWindows())
        {
            Skip = "Unix only";
        }
    }
}

/// <summary>A fact that runs on Windows only and shows as skipped elsewhere.</summary>
public sealed class WindowsFactAttribute : FactAttribute
{
    public WindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            Skip = "Windows only";
        }
    }
}

/// <summary>
/// A fact that runs on Unix-like systems as a non-root user only, for tests that rely on file
/// permissions being enforced; shows as skipped elsewhere.
/// </summary>
public sealed class UnixNonRootFactAttribute : FactAttribute
{
    public UnixNonRootFactAttribute()
    {
        if (OperatingSystem.IsWindows())
        {
            Skip = "Unix only";
        }
        else if (Environment.UserName == "root")
        {
            Skip = "root ignores file permissions";
        }
    }
}

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

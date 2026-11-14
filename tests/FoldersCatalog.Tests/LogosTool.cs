namespace FoldersCatalog.Tests;

/// <summary>
/// Classes that run tools/logos.cs join this collection: the .NET file-based app build cache
/// is not safe under concurrent runs of the same app, so xUnit must not run them in parallel.
/// </summary>
[CollectionDefinition(LogosTool.CollectionName)]
public sealed class LogosToolCollection;

internal static class LogosTool
{
    internal const string CollectionName = "logos-tool";
}

namespace FoldersCatalog;

public static class CatalogSettings
{
    private static readonly char[] LineBreaks = ['\r', '\n'];

    public static IReadOnlyList<string> ParsePatterns(string text) =>
        text.Split(LineBreaks, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static IReadOnlyList<string> ParseExtensions(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

namespace FoldersCatalog;

/// <summary>Builds the catalogue: the files of the given folder patterns that have one of the given extensions.</summary>
public static class Catalog
{
    private static readonly StringComparer PathComparer =
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public static IReadOnlyList<CatalogEntry> Build(IEnumerable<string> patterns, IEnumerable<string> extensions)
    {
        var wanted = new HashSet<string>(
            extensions.Select(e => e.StartsWith('.') ? e : "." + e),
            StringComparer.OrdinalIgnoreCase);

        var listed = new HashSet<string>(PathComparer);
        var entries = new List<CatalogEntry>();

        foreach (var pattern in patterns)
        {
            var parsed = FolderPattern.Parse(pattern);
            var real = Resolve(parsed.Folder, () => RealRoot(parsed.Folder));
            if (real is null)
            {
                continue;
            }

            Walk(parsed.Folder, real, parsed.Recursive, wanted, new HashSet<string>(PathComparer), listed, entries);
        }

        return entries.OrderBy(e => e.FullPath, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static void Walk(
        string dir, string real, bool recursive, HashSet<string> wanted,
        HashSet<string> visited, HashSet<string> listed, List<CatalogEntry> entries)
    {
        if (!visited.Add(real))
        {
            return;
        }

        var options = new EnumerationOptions();

        foreach (var file in Directory.EnumerateFiles(dir, "*", options))
        {
            var fileName = Path.GetFileName(file);
            if (wanted.Contains(Path.GetExtension(fileName)) && listed.Add(Path.Combine(real, fileName)))
            {
                var name = Path.GetFileNameWithoutExtension(fileName);
                entries.Add(new CatalogEntry(file, name.Length == 0 ? fileName : name, dir));
            }
        }

        if (!recursive)
        {
            return;
        }

        foreach (var sub in Directory.EnumerateDirectories(dir, "*", options))
        {
            var childReal = RealFolder(sub, Path.Combine(real, Path.GetFileName(sub)));
            if (childReal is not null)
            {
                Walk(sub, childReal, recursive, wanted, visited, listed, entries);
            }
        }
    }

    // The real path of a pattern root: every link along its path, ancestors included, is resolved.
    private static string RealRoot(string path)
    {
        var current = Path.GetPathRoot(path)!;
        foreach (var segment in path[current.Length..].Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            var target = Directory.ResolveLinkTarget(current, returnFinalTarget: true);
            if (target is not null)
            {
                current = RealRoot(Path.GetFullPath(target.FullName));
            }
        }

        return current;
    }

    // The real path of a folder: a link resolves to its final target, anything else to `plain`.
    // Null when the folder, or the target of a link, does not exist.
    private static string? RealFolder(string path, string plain) =>
        Resolve(path, () =>
        {
            var target = Directory.ResolveLinkTarget(path, returnFinalTarget: true);
            return target is null ? plain : RealRoot(Path.GetFullPath(target.FullName));
        });

    // Runs `resolve` for an existing folder; null when the folder, or the target of a link, does not exist.
    private static string? Resolve(string path, Func<string> resolve)
    {
        if (!Directory.Exists(path))
        {
            return null;
        }

        try
        {
            return resolve();
        }
        catch (IOException)
        {
            // Windows: a link whose target is gone passes Directory.Exists, but resolving it throws.
            return null;
        }
    }
}

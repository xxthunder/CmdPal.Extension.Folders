namespace FoldersCatalog;

/// <summary>A folder to catalogue, and whether its subfolders are included.</summary>
public sealed record FolderPattern(string Folder, bool Recursive)
{
    public static FolderPattern Parse(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern must not be blank.", nameof(pattern));
        }

        var text = Environment.ExpandEnvironmentVariables(pattern.Trim());

        var recursive = false;
        if (text.EndsWith("\\**", StringComparison.Ordinal) || text.EndsWith("/**", StringComparison.Ordinal))
        {
            recursive = true;
            text = text[..^3];
            if (text.Length == 0 || text.EndsWith(':'))
            {
                text += pattern.Trim()[^3]; // keep the separator: a root stays a root
            }
        }

        if (text == "~" || text.StartsWith("~/", StringComparison.Ordinal) || text.StartsWith("~\\", StringComparison.Ordinal))
        {
            var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var rest = text[1..].TrimStart('/', '\\');
            text = rest.Length == 0 ? profile : Path.Combine(profile, rest);
        }

        return new FolderPattern(Path.GetFullPath(text), recursive);
    }
}

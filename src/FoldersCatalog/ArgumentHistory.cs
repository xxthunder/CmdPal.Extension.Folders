using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoldersCatalog;

/// <summary>
/// The arguments a user typed for each script, newest first, kept in one JSON file:
/// an object from script path to an array of argument strings.
/// </summary>
public sealed class ArgumentHistory
{
    /// <summary>How many arguments are kept per script.</summary>
    public const int Limit = 10;

    private readonly string _filePath;
    private readonly Dictionary<string, List<string>> _entries = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Reads the file if it exists; a missing, empty, unreadable or invalid file gives an empty
    /// history. Null or blank entries are left out.
    /// </summary>
    public ArgumentHistory(string filePath)
    {
        _filePath = filePath;

        try
        {
            if (!File.Exists(filePath)) return;
            var json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json)) return;

            var read = JsonSerializer.Deserialize(json, ArgumentHistoryJsonContext.Default.DictionaryStringListString);
            if (read is null) return;

            foreach (var (path, arguments) in read)
            {
                if (arguments is null) continue;
                arguments.RemoveAll(string.IsNullOrWhiteSpace);
                _entries[path] = arguments;
            }
        }
        catch (JsonException)
        {
            _entries.Clear();
        }
        catch (IOException)
        {
            _entries.Clear();
        }
        catch (UnauthorizedAccessException)
        {
            _entries.Clear();
        }
    }

    /// <summary>The script's earlier arguments, newest first; empty for an unknown script.</summary>
    public IReadOnlyList<string> For(string scriptPath) =>
        _entries.TryGetValue(scriptPath, out var list) ? list.ToArray() : [];

    /// <summary>
    /// Records trimmed arguments at the front of the script's list, dropping an equal entry and
    /// anything beyond <see cref="Limit"/>, and writes the file. Blank arguments change nothing.
    /// </summary>
    public void Add(string scriptPath, string arguments)
    {
        var trimmed = arguments?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        if (!_entries.TryGetValue(scriptPath, out var list))
        {
            list = [];
            _entries[scriptPath] = list;
        }

        list.RemoveAll(entry => string.Equals(entry, trimmed, StringComparison.Ordinal));
        list.Insert(0, trimmed);
        if (list.Count > Limit) list.RemoveRange(Limit, list.Count - Limit);

        Save();
    }

    /// <summary>
    /// The earlier arguments that contain the trimmed search ignoring case, in order, without an
    /// entry equal to the search. A blank search returns all of them.
    /// </summary>
    public static IReadOnlyList<string> Matching(IReadOnlyList<string> earlier, string search)
    {
        if (string.IsNullOrWhiteSpace(search)) return earlier;

        var needle = search.Trim();
        return earlier
            .Where(entry => entry.Contains(needle, StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(entry, needle, StringComparison.Ordinal))
            .ToList();
    }

    private void Save()
    {
        var folder = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);

        File.WriteAllText(_filePath, JsonSerializer.Serialize(_entries, ArgumentHistoryJsonContext.Default.DictionaryStringListString));
    }
}

/// <summary>Source-generated JSON metadata, so the history works with trimming and AOT.</summary>
[JsonSerializable(typeof(Dictionary<string, List<string>>))]
internal sealed partial class ArgumentHistoryJsonContext : JsonSerializerContext;

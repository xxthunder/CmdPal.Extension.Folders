#:project ../src/FoldersCatalog/FoldersCatalog.csproj

using FoldersCatalog;

var patterns = new List<string>();
string? ext = null;
var extMissing = false;

for (var i = 0; i < args.Length; i++)
{
    if (args[i] != "--ext")
    {
        if (!string.IsNullOrWhiteSpace(args[i]))
        {
            patterns.Add(args[i]);
        }
    }
    else if (i + 1 < args.Length)
    {
        ext = args[++i];
    }
    else
    {
        extMissing = true;
    }
}

var extensions = ext is null ? [] : CatalogSettings.ParseExtensions(ext);

if (extMissing || extensions.Count == 0 || patterns.Count == 0)
{
    Console.Error.WriteLine("Usage: dotnet run tools/catalog.cs -- --ext \"<extensions>\" <pattern>...");
    return 2;
}

var entries = Catalog.Build(patterns, extensions);
foreach (var e in entries)
{
    Console.WriteLine($"{e.Name}\t{e.Folder}\t{e.FullPath}");
}

Console.WriteLine($"{entries.Count} entries");
return 0;

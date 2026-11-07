namespace FoldersCatalog;

/// <summary>One catalogued file: its path as reached, its name without extension, and its folder.</summary>
public sealed record CatalogEntry(string FullPath, string Name, string Folder);

using System.Text.Json;
using CmdPal.Extension.Shortcuts;

namespace CmdPal.Extension.Shortcuts.Tests;

public sealed class SettingsManagerTests : IDisposable
{
    private readonly string _root = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "cpf-settings-" + Guid.NewGuid().ToString("N")));

    public SettingsManagerTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private static readonly string[] DefaultPatterns =
        [@"%USERPROFILE%\shortcuts\**", @"%USERPROFILE%\shortcuts_private\**"];

    private static readonly string[] DefaultExtensions = [".url", ".lnk", ".cmd", ".bat", ".exe"];

    [Fact]
    public void SettingsManager_WithoutFile_HasDefaults()
    {
        var manager = new SettingsManager(Path.Combine(_root, "missing.json"));

        Assert.Equal(DefaultPatterns, manager.Patterns);
        Assert.Equal(DefaultExtensions, manager.Extensions);
        Assert.Equal(SettingsManager.DefaultPatterns, manager.Settings.GetSetting<string>(SettingsManager.PatternsKey));
    }

    [Fact]
    public void SettingsManager_FileWithOneKey_KeepsTheOtherDefault()
    {
        var file = Path.Combine(_root, "settings.json");
        File.WriteAllText(file, JsonSerializer.Serialize(new Dictionary<string, string>
        {
            [SettingsManager.PatternsKey] = Path.Combine(_root, "**"),
        }));

        var manager = new SettingsManager(file);

        Assert.Equal([Path.Combine(_root, "**")], manager.Patterns);
        Assert.Equal(DefaultExtensions, manager.Extensions);
    }
}

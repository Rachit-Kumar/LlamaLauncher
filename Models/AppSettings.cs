namespace LlamaLauncher.Models;

/// <summary>
/// Global application settings — stored separately from profiles so the exe path
/// doesn't need to be repeated per model.
/// </summary>
public class AppSettings
{
    /// <summary>Absolute path to llama-server.exe (set once, shared by all profiles)</summary>
    public string ServerExePath { get; set; } = string.Empty;

    /// <summary>ID of the profile that was last selected; restored on next launch</summary>
    public Guid? LastSelectedProfileId { get; set; }

    /// <summary>When true, the main window starts hidden and only the tray icon is visible</summary>
    public bool StartMinimizedToTray { get; set; } = false;
}

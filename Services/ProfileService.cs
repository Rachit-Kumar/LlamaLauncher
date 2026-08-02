using System.Text.Json;
using LlamaLauncher.Models;

namespace LlamaLauncher.Services;

/// <summary>
/// Handles loading and saving of model profiles and application settings to JSON files
/// in the same directory as the executable.
/// </summary>
public class ProfileService
{
    private static readonly string BaseDir = AppContext.BaseDirectory;
    private static readonly string ProfilesPath = Path.Combine(BaseDir, "profiles.json");
    private static readonly string SettingsPath = Path.Combine(BaseDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly object _lock = new();

    // ── Profiles ────────────────────────────────────────────────────────────

    public List<ModelProfile> LoadProfiles()
    {
        lock (_lock)
        {
            if (!File.Exists(ProfilesPath))
                return [];

            try
            {
                string json = File.ReadAllText(ProfilesPath);
                return JsonSerializer.Deserialize<List<ModelProfile>>(json, JsonOptions) ?? [];
            }
            catch (Exception ex)
            {
                PreserveCorruptFile(ProfilesPath, ex);
                return [];
            }
        }
    }

    public void SaveProfiles(IEnumerable<ModelProfile> profiles)
    {
        lock (_lock)
        {
            string json = JsonSerializer.Serialize(profiles.ToList(), JsonOptions);
            AtomicWrite(ProfilesPath, json);
        }
    }

    // ── Settings ─────────────────────────────────────────────────────────────

    public AppSettings LoadSettings()
    {
        lock (_lock)
        {
            if (!File.Exists(SettingsPath))
                return new AppSettings();

            try
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                PreserveCorruptFile(SettingsPath, ex);
                return new AppSettings();
            }
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        lock (_lock)
        {
            string json = JsonSerializer.Serialize(settings, JsonOptions);
            AtomicWrite(SettingsPath, json);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void PreserveCorruptFile(string filePath, Exception ex)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string backup = $"{filePath}.corrupt_{DateTime.Now:yyyyMMdd_HHmmss}";
                File.Copy(filePath, backup, overwrite: true);
            }
        }
        catch { /* best effort */ }
    }

    /// <summary>
    /// Writes <paramref name="content"/> to a .tmp file beside the target,
    /// then renames it over the target with retries to handle Windows file locking.
    /// </summary>
    private static void AtomicWrite(string targetPath, string content)
    {
        string tmp = targetPath + ".tmp";
        File.WriteAllText(tmp, content, System.Text.Encoding.UTF8);

        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                File.Move(tmp, targetPath, overwrite: true);
                return;
            }
            catch (IOException) when (attempt < 4)
            {
                Thread.Sleep(50);
            }
        }

        // Final attempt (will throw if still locked)
        File.Move(tmp, targetPath, overwrite: true);
    }
}


using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LlamaLauncher.Services;

public class GitHubAsset
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; } = 0;
}

public class GitHubReleaseResponse
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = string.Empty;

    [JsonPropertyName("assets")]
    public List<GitHubAsset> Assets { get; set; } = [];
}

public static class LlamaDownloaderService
{
    private static readonly HttpClient s_httpClient = new();

    static LlamaDownloaderService()
    {
        s_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LlamaLauncher-Downloader/1.0");
        s_httpClient.Timeout = TimeSpan.FromMinutes(10);
    }

    public static async Task<string> DownloadAndExtractLlamaServerAsync(string recommendedBuildType, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        // 1. Query GitHub releases API
        var releaseJson = await s_httpClient.GetStringAsync("https://api.github.com/repos/ggml-org/llama.cpp/releases/latest", ct);
        var release = JsonSerializer.Deserialize<GitHubReleaseResponse>(releaseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (release is null || release.Assets.Count == 0)
            throw new InvalidOperationException("Could not retrieve release assets from GitHub API.");

        // 2. Match asset based on target build type (cu12, cu11, or avx2)
        GitHubAsset? matchedAsset = null;
        if (recommendedBuildType.Equals("cu12", StringComparison.OrdinalIgnoreCase))
        {
            matchedAsset = release.Assets.FirstOrDefault(a => a.Name.Contains("cuda", StringComparison.OrdinalIgnoreCase) &&
                                                             (a.Name.Contains("cu12", StringComparison.OrdinalIgnoreCase) || a.Name.Contains("12.")))
                           ?? release.Assets.FirstOrDefault(a => a.Name.Contains("cuda", StringComparison.OrdinalIgnoreCase));
        }
        else if (recommendedBuildType.Equals("cu11", StringComparison.OrdinalIgnoreCase))
        {
            matchedAsset = release.Assets.FirstOrDefault(a => a.Name.Contains("cuda", StringComparison.OrdinalIgnoreCase) &&
                                                             (a.Name.Contains("cu11", StringComparison.OrdinalIgnoreCase) || a.Name.Contains("11.")));
        }

        matchedAsset ??= release.Assets.FirstOrDefault(a => a.Name.Contains("win", StringComparison.OrdinalIgnoreCase) && a.Name.Contains("avx2", StringComparison.OrdinalIgnoreCase))
                       ?? release.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

        if (matchedAsset is null)
            throw new InvalidOperationException("No suitable Windows release package found.");

        // 3. Download zip file with progress tracking
        string tempZip = Path.Combine(Path.GetTempPath(), $"llama_release_{release.TagName}.zip");
        try
        {
            using (var response = await s_httpClient.GetAsync(matchedAsset.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct))
            {
                response.EnsureSuccessStatusCode();
                long totalBytes = response.Content.Headers.ContentLength ?? matchedAsset.Size;

                using (var stream = await response.Content.ReadAsStreamAsync(ct))
                using (var fileStream = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    byte[] buffer = new byte[8192];
                    long totalRead = 0;
                    int read;

                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read, ct);
                        totalRead += read;
                        if (totalBytes > 0)
                        {
                            progress?.Report((double)totalRead / totalBytes * 100.0);
                        }
                    }
                }
            }

            // 4. Extract zip to llama-bin directory
            string targetDir = Path.Combine(AppContext.BaseDirectory, "llama-bin");
            if (Directory.Exists(targetDir))
            {
                try { Directory.Delete(targetDir, true); } catch { /* best effort */ }
            }
            Directory.CreateDirectory(targetDir);

            ZipFile.ExtractToDirectory(tempZip, targetDir, overwriteFiles: true);

            // 5. Find llama-server.exe
            string[] found = Directory.GetFiles(targetDir, "llama-server.exe", SearchOption.AllDirectories);
            if (found.Length == 0)
                throw new FileNotFoundException("llama-server.exe was not found inside the extracted zip archive.");

            // Save installed version tag for accurate update detection
            try
            {
                File.WriteAllText(Path.Combine(targetDir, "installed_version.txt"), release.TagName);
            }
            catch { /* best effort */ }

            return found[0];
        }
        finally
        {
            try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch { /* cleanup */ }
        }
    }

    public static string GetInstalledVersion()
    {
        try
        {
            string versionFile = Path.Combine(AppContext.BaseDirectory, "llama-bin", "installed_version.txt");
            if (File.Exists(versionFile))
            {
                string ver = File.ReadAllText(versionFile).Trim();
                if (!string.IsNullOrEmpty(ver)) return ver;
            }
        }
        catch { }

        return "Unknown";
    }
}

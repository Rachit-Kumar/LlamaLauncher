using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace LlamaLauncher.Services;

public class GitHubReleaseInfo
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("published_at")]
    public DateTime? PublishedAt { get; set; }
}

public static class UpdateCheckerService
{
    private static readonly HttpClient s_httpClient = new();

    static UpdateCheckerService()
    {
        s_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("LlamaLauncher-UpdateChecker/1.0");
        s_httpClient.Timeout = TimeSpan.FromSeconds(5);
    }

    /// <summary>
    /// Checks GitHub API for the latest release tag of ggml-org/llama.cpp (Check-Only Mode).
    /// </summary>
    public static async Task<GitHubReleaseInfo?> CheckLatestReleaseAsync()
    {
        try
        {
            return await s_httpClient.GetFromJsonAsync<GitHubReleaseInfo>(
                "https://api.github.com/repos/ggml-org/llama.cpp/releases/latest");
        }
        catch
        {
            return null; // Silent network fallback
        }
    }
}

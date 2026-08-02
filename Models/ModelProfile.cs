using System.Text.Json.Serialization;

namespace LlamaLauncher.Models;

/// <summary>
/// Represents a named llama-server configuration profile.
/// Each profile stores everything needed to launch the server for a specific model.
/// </summary>
public class ModelProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Display name shown in the dropdown (e.g. "Qwen3-4B Fast")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Absolute path to the .gguf model file</summary>
    public string ModelPath { get; set; } = string.Empty;

    /// <summary>Port llama-server will listen on</summary>
    public int Port { get; set; } = 8080;

    /// <summary>Context window size (-c flag)</summary>
    public int ContextSize { get; set; } = 8192;

    /// <summary>Number of GPU layers to offload (-ngl flag)</summary>
    public int GpuLayers { get; set; } = 999;

    /// <summary>Any additional raw CLI flags appended verbatim (e.g. --chat-template-kwargs ...)</summary>
    public string ExtraFlags { get; set; } = string.Empty;

    /// <summary>Flash Attention (-fa)</summary>
    public bool EnableFlashAttn { get; set; } = true;

    /// <summary>Disable memory mapping (--no-mmap)</summary>
    public bool NoMMap { get; set; } = false;

    /// <summary>Lock model in physical RAM (--mlock)</summary>
    public bool MLock { get; set; } = false;

    /// <summary>Enable embedding endpoint (--embedding)</summary>
    public bool EnableEmbedding { get; set; } = false;

    /// <summary>Disable reasoning / thinking steps (--chat-template-kwargs "{\"reasoning_format\":\"none\"}")</summary>
    public bool DisableThinking { get; set; } = false;

    /// <summary>Returns a display-friendly summary for the quick-view panel</summary>
    [JsonIgnore]
    public string ModelFileName => string.IsNullOrWhiteSpace(ModelPath)
        ? "(no model set)"
        : Path.GetFileName(ModelPath);

    public override string ToString() => Name;

    /// <summary>Creates a shallow clone of this profile (for Quick Edit overrides)</summary>
    public ModelProfile Clone() => new()
    {
        Id = Id,
        Name = Name,
        ModelPath = ModelPath,
        Port = Port,
        ContextSize = ContextSize,
        GpuLayers = GpuLayers,
        ExtraFlags = ExtraFlags,
        EnableFlashAttn = EnableFlashAttn,
        NoMMap = NoMMap,
        MLock = MLock,
        EnableEmbedding = EnableEmbedding,
        DisableThinking = DisableThinking,
    };
}

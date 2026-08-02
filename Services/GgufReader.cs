using System.Text;

namespace LlamaLauncher.Services;

public class GgufMetadata
{
    public string Architecture { get; set; } = "unknown";
    public string Quantization { get; set; } = "unknown";
    public uint LayerCount { get; set; } = 0;
    public uint MaxContextLength { get; set; } = 0;
    public long FileSizeBytes { get; set; } = 0;
}

/// <summary>
/// Lightweight reader for GGUF model header format (v2/v3).
/// Reads file metadata without loading model weights into memory.
/// </summary>
public static class GgufReader
{
    private const uint GGUF_MAGIC = 0x46554747; // 'G', 'G', 'U', 'F'

    public static GgufMetadata ReadMetadata(string filePath)
    {
        var meta = new GgufMetadata();
        if (!File.Exists(filePath)) return meta;

        try
        {
            var fileInfo = new FileInfo(filePath);
            meta.FileSizeBytes = fileInfo.Length;

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            uint magic = reader.ReadUInt32();
            if (magic != GGUF_MAGIC)
                return meta; // Not a valid GGUF file

            uint version = reader.ReadUInt32();
            if (version < 2 || version > 3)
                return meta;

            ulong tensorCount = reader.ReadUInt64();
            ulong kvCount = reader.ReadUInt64();

            for (ulong i = 0; i < kvCount; i++)
            {
                if (stream.Position >= stream.Length - 16) break;

                string key = ReadGgufString(reader);
                uint valueType = reader.ReadUInt32();

                object? val = ReadGgufValue(reader, valueType);
                if (val is null) continue;

                if (key.Equals("general.architecture", StringComparison.OrdinalIgnoreCase))
                {
                    meta.Architecture = val.ToString() ?? "unknown";
                }
                else if (key.Equals("general.file_type", StringComparison.OrdinalIgnoreCase))
                {
                    uint fileType = Convert.ToUInt32(val);
                    meta.Quantization = FormatQuantization(fileType);
                }
                else if (key.EndsWith(".block_count", StringComparison.OrdinalIgnoreCase) ||
                         key.Equals("llama.block_count", StringComparison.OrdinalIgnoreCase))
                {
                    meta.LayerCount = Convert.ToUInt32(val);
                }
                else if (key.EndsWith(".context_length", StringComparison.OrdinalIgnoreCase) ||
                         key.Equals("llama.context_length", StringComparison.OrdinalIgnoreCase))
                {
                    meta.MaxContextLength = Convert.ToUInt32(val);
                }
            }
        }
        catch
        {
            // Best-effort parsing — fallback gracefully if header is truncated
        }

        return meta;
    }

    private static string ReadGgufString(BinaryReader reader)
    {
        ulong len = reader.ReadUInt64();
        if (len > 4096) return string.Empty; // Safety cap
        byte[] bytes = reader.ReadBytes((int)len);
        return Encoding.UTF8.GetString(bytes);
    }

    private static object? ReadGgufValue(BinaryReader reader, uint type)
    {
        return type switch
        {
            0 => reader.ReadByte(),       // UINT8
            1 => reader.ReadSByte(),      // INT8
            2 => reader.ReadUInt16(),     // UINT16
            3 => reader.ReadInt16(),      // INT16
            4 => reader.ReadUInt32(),     // UINT32
            5 => reader.ReadInt32(),      // INT32
            6 => reader.ReadSingle(),     // FLOAT32
            7 => reader.ReadBoolean(),    // BOOL
            8 => ReadGgufString(reader),  // STRING
            9 => SkipArray(reader),       // ARRAY
            10 => reader.ReadUInt64(),    // UINT64
            11 => reader.ReadInt64(),     // INT64
            12 => reader.ReadDouble(),    // FLOAT64
            _ => null
        };
    }

    private static object? SkipArray(BinaryReader reader)
    {
        uint itemType = reader.ReadUInt32();
        ulong count = reader.ReadUInt64();
        for (ulong i = 0; i < count; i++)
        {
            ReadGgufValue(reader, itemType);
        }
        return null;
    }

    private static string FormatQuantization(uint fileType) => fileType switch
    {
        0 => "F32",
        1 => "F16",
        2 => "Q4_0",
        3 => "Q4_1",
        7 => "Q8_0",
        8 => "Q5_0",
        9 => "Q5_1",
        10 => "Q4_K_S",
        11 => "Q4_K_M",
        12 => "Q5_K_S",
        13 => "Q5_K_M",
        14 => "Q6_K",
        15 => "IQ2_XXS",
        16 => "IQ2_XS",
        17 => "Q2_K",
        18 => "IQ1_S",
        19 => "Q3_K_S",
        20 => "IQ3_S",
        21 => "IQ3_UE",
        22 => "Q3_K_M",
        23 => "Q3_K_L",
        24 => "IQ2_S",
        25 => "IQ2_M",
        26 => "IQ4_NL",
        27 => "IQ4_XS",
        28 => "IQ3_XXS",
        _ => $"Type #{fileType}"
    };
}

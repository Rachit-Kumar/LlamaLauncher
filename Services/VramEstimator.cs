namespace LlamaLauncher.Services;

public enum VramStatus
{
    Optimal,
    High,
    Warning
}

public class VramEstimateResult
{
    public double EstimatedVramGb { get; set; }
    public VramStatus Status { get; set; } = VramStatus.Optimal;
    public string DisplayText { get; set; } = string.Empty;
}

public static class VramEstimator
{
    public static VramEstimateResult Estimate(long fileSizeBytes, int contextSize, int ngl, uint totalLayers = 32, bool flashAttn = true)
    {
        var result = new VramEstimateResult();
        if (fileSizeBytes <= 0)
        {
            result.DisplayText = "~0 GB VRAM";
            return result;
        }

        double modelSizeGb = fileSizeBytes / (1024.0 * 1024.0 * 1024.0);

        // Offload fraction: capped at 1.0
        uint layers = totalLayers > 0 ? totalLayers : 32;
        double offloadRatio = Math.Min(1.0, (double)ngl / layers);

        double modelVramGb = modelSizeGb * offloadRatio;

        // KV cache allocation per 1K context is roughly ~0.15 GB to 0.35 GB for standard 8B-14B models
        // Flash attention reduces KV cache by roughly 50%
        double kvCachePer1kGb = flashAttn ? 0.08 : 0.16;
        double contextKvCacheGb = (contextSize / 1024.0) * kvCachePer1kGb * offloadRatio;

        double totalVramGb = modelVramGb + contextKvCacheGb;
        result.EstimatedVramGb = Math.Round(totalVramGb, 1);

        if (totalVramGb > 16.0)
        {
            result.Status = VramStatus.Warning;
            result.DisplayText = $"⚡ ~{result.EstimatedVramGb:F1} GB VRAM (High Memory Risk)";
        }
        else if (totalVramGb > 10.0)
        {
            result.Status = VramStatus.High;
            result.DisplayText = $"⚙️ ~{result.EstimatedVramGb:F1} GB VRAM";
        }
        else
        {
            result.Status = VramStatus.Optimal;
            result.DisplayText = $"✓ ~{result.EstimatedVramGb:F1} GB VRAM";
        }

        return result;
    }
}

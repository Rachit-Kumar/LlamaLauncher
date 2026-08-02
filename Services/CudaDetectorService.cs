using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LlamaLauncher.Services;

public class CudaHardwareInfo
{
    public bool HasNvidiaGpu { get; set; } = false;
    public string GpuName { get; set; } = "No NVIDIA GPU detected";
    public double CudaVersion { get; set; } = 0.0; // e.g. 12.4
    public string CudaVersionString => CudaVersion > 0 ? $"CUDA {CudaVersion:F1}" : "No CUDA";
    public double TotalVramGb { get; set; } = 0.0;
    public string RecommendedBuildType => CudaVersion >= 12.0 ? "cu12" : (CudaVersion >= 11.0 ? "cu11" : "avx2");
}

public static class CudaDetectorService
{
    public static CudaHardwareInfo DetectCudaHardware()
    {
        var info = new CudaHardwareInfo();
        if (!OperatingSystem.IsWindows()) return info;

        // Approach 1: Try running nvidia-smi.exe
        try
        {
            string nvidiaSmiPath = FindNvidiaSmiPath();
            if (!string.IsNullOrEmpty(nvidiaSmiPath) && File.Exists(nvidiaSmiPath))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = nvidiaSmiPath,
                    Arguments = "--query-gpu=name,driver_version,memory.total --format=csv,noheader,nounits",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                };

                using var proc = Process.Start(psi);
                if (proc is not null)
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(2000);

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        var parts = output.Split(',');
                        if (parts.Length >= 3)
                        {
                            info.HasNvidiaGpu = true;
                            info.GpuName = parts[0].Trim();

                            if (double.TryParse(parts[2].Trim(), out double vramMb))
                            {
                                info.TotalVramGb = Math.Round(vramMb / 1024.0, 1);
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            /* Fallback to nvcuda.dll inspection */
        }

        // Detect CUDA Driver Version via nvcuda.dll or registry
        try
        {
            string system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string nvcudaPath = Path.Combine(system32, "nvcuda.dll");
            if (File.Exists(nvcudaPath))
            {
                info.HasNvidiaGpu = true;
                var versionInfo = FileVersionInfo.GetVersionInfo(nvcudaPath);
                // ProductVersion/FileVersion format: 31.0.15.5186 (Driver 551.86 -> CUDA 12.x)
                int major = versionInfo.FileMajorPart;
                int minor = versionInfo.FileMinorPart;

                // Driver series heuristic for CUDA version mapping on Windows
                if (versionInfo.FilePrivatePart >= 5000 || versionInfo.FileBuildPart >= 5000)
                    info.CudaVersion = 12.4;
                else if (versionInfo.FilePrivatePart >= 4500 || versionInfo.FileBuildPart >= 4500)
                    info.CudaVersion = 11.8;
                else
                    info.CudaVersion = 11.0;
            }
        }
        catch
        {
            // Best effort
        }

        if (info.CudaVersion == 0.0 && info.HasNvidiaGpu)
        {
            info.CudaVersion = 12.2; // Sensible default for modern NVIDIA drivers
        }

        return info;
    }

    private static string FindNvidiaSmiPath()
    {
        string sys32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "nvidia-smi.exe");
        if (File.Exists(sys32)) return sys32;

        string programFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NVIDIA Corporation", "NVSMI", "nvidia-smi.exe");
        if (File.Exists(programFiles)) return programFiles;

        return string.Empty;
    }
}

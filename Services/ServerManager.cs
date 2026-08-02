using System.Diagnostics;
using System.Runtime.InteropServices;
using LlamaLauncher.Models;

namespace LlamaLauncher.Services;

/// <summary>
/// Manages the lifecycle of a single llama-server.exe child process:
/// launching, streaming output, and graceful shutdown.
/// </summary>
public class ServerManager : IDisposable
{
    // ── Events ───────────────────────────────────────────────────────────────

    /// <summary>Fired for each line of stdout/stderr received from the server process.</summary>
    public event EventHandler<string>? OutputReceived;

    /// <summary>Fired when the process exits (either naturally or via StopServer).</summary>
    public event EventHandler? ServerStopped;

    // ── State ─────────────────────────────────────────────────────────────────

    private Process? _process;
    private readonly object _lock = new();

    public bool IsRunning
    {
        get
        {
            lock (_lock)
            {
                try { return _process is { HasExited: false }; }
                catch { return false; }
            }
        }
    }

    public ModelProfile? CurrentProfile { get; private set; }

    // ── Launch ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts llama-server.exe with settings from <paramref name="profile"/>.
    /// Any non-null values in <paramref name="overrides"/> replace the profile defaults.
    /// </summary>
    public void StartServer(string serverExePath, ModelProfile profile, QuickEditOverrides? overrides = null)
    {
        if (IsRunning)
            throw new InvalidOperationException("A server is already running. Stop it first.");

        // Validate prerequisites
        if (string.IsNullOrWhiteSpace(serverExePath) || !File.Exists(serverExePath))
            throw new FileNotFoundException($"llama-server.exe not found at:\n{serverExePath}");

        if (string.IsNullOrWhiteSpace(profile.ModelPath) || !File.Exists(profile.ModelPath))
            throw new FileNotFoundException($"Model file not found at:\n{profile.ModelPath}");

        // Apply quick-edit overrides on top of saved profile values
        int port = overrides?.Port ?? profile.Port;
        int ctx  = overrides?.ContextSize ?? profile.ContextSize;
        int ngl  = overrides?.GpuLayers ?? profile.GpuLayers;
        string extra = overrides?.ExtraFlags ?? profile.ExtraFlags;

        bool flashAttn = overrides?.EnableFlashAttn ?? profile.EnableFlashAttn;
        bool noMMap    = overrides?.NoMMap ?? profile.NoMMap;
        bool mLock     = overrides?.MLock ?? profile.MLock;
        bool embedding = overrides?.EnableEmbedding ?? profile.EnableEmbedding;
        bool disableThinking = overrides?.DisableThinking ?? profile.DisableThinking;

        // Build the argument string
        var args = BuildArgs(profile.ModelPath, port, ctx, ngl, extra, flashAttn, noMMap, mLock, embedding, disableThinking);

        var psi = new ProcessStartInfo
        {
            FileName = serverExePath,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

        proc.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                OutputReceived?.Invoke(this, e.Data);
        };
        proc.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                OutputReceived?.Invoke(this, e.Data);
        };
        proc.Exited += (_, _) =>
        {
            lock (_lock)
            {
                if (_process == proc)
                    _process = null;
            }
            CurrentProfile = null;
            ServerStopped?.Invoke(this, EventArgs.Empty);
        };

        try
        {
            proc.Start();
            JobTracker.AddProcess(proc);
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            lock (_lock) { _process = proc; }
            CurrentProfile = profile;

            string quotedExe = serverExePath.Contains(' ') ? $"\"{serverExePath}\"" : serverExePath;
            OutputReceived?.Invoke(this, $"[LlamaLauncher] Server started: {quotedExe} {args}");
        }
        catch (Exception ex)
        {
            lock (_lock) { _process = null; }
            CurrentProfile = null;
            try { proc.Dispose(); } catch { /* best effort */ }
            throw new InvalidOperationException($"Failed to launch llama-server.exe:\n{ex.Message}", ex);
        }
    }

    // ── Stop ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts a graceful shutdown first, then forces Kill after 3 seconds.
    /// </summary>
    public void StopServer()
    {
        Process? proc;
        lock (_lock) { proc = _process; }

        if (proc is null || proc.HasExited)
        {
            // Already gone — clean up state
            lock (_lock) { _process = null; }
            CurrentProfile = null;
            return;
        }

        try
        {
            OutputReceived?.Invoke(this, "[LlamaLauncher] Stopping server…");

            if (!proc.HasExited)
                proc.Kill(entireProcessTree: true);

            proc.WaitForExit(3000);
        }
        catch (Exception ex)
        {
            OutputReceived?.Invoke(this, $"[LlamaLauncher] Error stopping server: {ex.Message}");
        }
        finally
        {
            lock (_lock)
            {
                if (_process == proc)
                    _process = null;
            }
            CurrentProfile = null;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string BuildArgs(string modelPath, int port, int ctx, int ngl, string extra, bool flashAttn, bool noMMap, bool mLock, bool embedding, bool disableThinking)
    {
        string quoted = $"\"{modelPath}\"";
        var sb = new System.Text.StringBuilder();
        sb.Append($"-m {quoted} --port {port} -c {ctx} -ngl {ngl}");

        if (flashAttn) sb.Append(" -fa");
        if (noMMap) sb.Append(" --no-mmap");
        if (mLock) sb.Append(" --mlock");
        if (embedding) sb.Append(" --embedding");
        if (disableThinking) sb.Append(" --chat-template-kwargs \"{\\\"reasoning_format\\\":\\\"none\\\"}\"");

        if (!string.IsNullOrWhiteSpace(extra))
            sb.Append($" {extra.Trim()}");

        return sb.ToString();
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    public void Dispose()
    {
        try { StopServer(); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Binds child processes to a Win32 Job Object with JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE.
/// Ensures llama-server.exe is automatically terminated if LlamaLauncher is killed or crashes.
/// </summary>
internal static class JobTracker
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? lpName);

    [DllImport("kernel32.dll")]
    private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

    [DllImport("kernel32.dll")]
    private static extern bool SetInformationJobObject(IntPtr hJob, int JobObjectInfoType, IntPtr lpJobObjectInfo, int cbJobObjectInfoLength);

    private static readonly IntPtr s_jobHandle;

    static JobTracker()
    {
        if (!OperatingSystem.IsWindows()) return;

        s_jobHandle = CreateJobObject(IntPtr.Zero, null);
        if (s_jobHandle == IntPtr.Zero) return;

        var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
        };

        var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            BasicLimitInformation = info
        };

        int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
        IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
        try
        {
            Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);
            SetInformationJobObject(s_jobHandle, JobObjectExtendedLimitInformation, extendedInfoPtr, length);
        }
        finally
        {
            Marshal.FreeHGlobal(extendedInfoPtr);
        }
    }

    public static void AddProcess(Process process)
    {
        if (OperatingSystem.IsWindows() && s_jobHandle != IntPtr.Zero && !process.HasExited)
        {
            try
            {
                AssignProcessToJobObject(s_jobHandle, process.Handle);
            }
            catch
            {
                // Best effort
            }
        }
    }

    private const int JobObjectExtendedLimitInformation = 9;
    private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public UIntPtr Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }
}

/// <summary>
/// Optional per-launch overrides that can replace profile values without saving.
/// Null fields mean "use the profile's saved value".
/// </summary>
public class QuickEditOverrides
{
    public int? Port { get; set; }
    public int? ContextSize { get; set; }
    public int? GpuLayers { get; set; }
    public string? ExtraFlags { get; set; }
    public bool? EnableFlashAttn { get; set; }
    public bool? NoMMap { get; set; }
    public bool? MLock { get; set; }
    public bool? EnableEmbedding { get; set; }
    public bool? DisableThinking { get; set; }
}


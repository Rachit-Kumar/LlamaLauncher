using LlamaLauncher.Models;
using LlamaLauncher.Services;

namespace LlamaLauncher.Forms;

/// <summary>
/// The primary application window. Handles profile selection, quick-edit,
/// server start/stop, live log output, and system tray integration.
/// </summary>
public partial class MainForm : Form
{
    // ── Services ──────────────────────────────────────────────────────────────
    private readonly ProfileService _profileService = new();
    private readonly ServerManager  _serverManager  = new();

    // ── State ─────────────────────────────────────────────────────────────────
    private List<ModelProfile>  _profiles        = [];
    private AppSettings         _settings        = new();
    private bool                _logPanelVisible = true;
    private bool                _allowClose      = false; // true only when tray Exit is clicked
    private bool                _disposed        = false; // guard against double-dispose

    // ── Log Batching ──────────────────────────────────────────────────────────
    private readonly System.Collections.Concurrent.ConcurrentQueue<string> _logQueue = new();
    private readonly System.Windows.Forms.Timer _logFlushTimer = new() { Interval = 50 };

    // ── Init ──────────────────────────────────────────────────────────────────

    public MainForm()
    {
        InitializeComponent();
        WireServerEvents();
        WireQuickEditEvents();
        LoadData();
        ApplyStartupBehavior();

        _logFlushTimer.Tick += (_, _) => FlushLogQueue();
        _logFlushTimer.Start();

        UpdateSystemRamStatus();
    }

    private void WireQuickEditEvents()
    {
        cmbQeContextPresets.SelectedIndexChanged += cmbQeContextPresets_SelectedIndexChanged;
        nudQeContext.ValueChanged += (_, _) => UpdateVramEstimate();
        nudQeNgl.ValueChanged     += (_, _) => UpdateVramEstimate();
        chkQeFlashAttn.CheckedChanged += (_, _) => UpdateVramEstimate();
    }

    private void UpdateSystemRamStatus()
    {
        try
        {
            var cuda = Services.CudaDetectorService.DetectCudaHardware();
            string gpuStr = cuda.HasNvidiaGpu ? $"{cuda.GpuName} ({cuda.CudaVersionString})" : "CPU";

            var mem = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(mem))
            {
                double totalGb = mem.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                double availGb = mem.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                lblSystemRam.Text = $"{gpuStr} | RAM: {availGb:F1} / {totalGb:F1} GB free";
            }
            else
            {
                lblSystemRam.Text = $"{gpuStr} | RAM: --";
            }
        }
        catch
        {
            lblSystemRam.Text = "System RAM: --";
        }
    }

    private void LoadData()
    {
        _settings = _profileService.LoadSettings();
        _profiles = _profileService.LoadProfiles();
        RefreshProfileDropdown();
        UpdateServerExeLabel();
    }

    private void ApplyStartupBehavior()
    {
        if (_settings.StartMinimizedToTray)
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Hide();
            notifyIcon.ShowBalloonTip(2000, "LlamaLauncher",
                "LlamaLauncher started minimized in the system tray.", ToolTipIcon.Info);
        }
    }

    // ── Profile Dropdown ──────────────────────────────────────────────────────

    private void RefreshProfileDropdown()
    {
        cmbProfiles.BeginUpdate();
        cmbProfiles.Items.Clear();

        foreach (var p in _profiles)
            cmbProfiles.Items.Add(p);

        // Restore last selection
        ModelProfile? last = _settings.LastSelectedProfileId.HasValue
            ? _profiles.FirstOrDefault(p => p.Id == _settings.LastSelectedProfileId)
            : null;

        if (last is not null)
            cmbProfiles.SelectedItem = last;
        else if (cmbProfiles.Items.Count > 0)
            cmbProfiles.SelectedIndex = 0;

        cmbProfiles.EndUpdate();
        UpdateProfileSummary();
        PopulateQuickEdit();
    }

    private void cmbProfiles_SelectedIndexChanged(object sender, EventArgs e)
    {
        chkSaveChanges.Checked = false;
        UpdateProfileSummary();
        PopulateQuickEdit();
    }

    private uint _currentModelLayers = 32;
    private long _currentModelFileSize = 0;

    private void UpdateProfileSummary()
    {
        if (cmbProfiles.SelectedItem is not ModelProfile p)
        {
            lblSummaryModel.Text   = "-";
            lblSummaryPort.Text    = "-";
            lblSummaryContext.Text = "-";
            lblSummaryNgl.Text     = "-";
            lblSummaryArch.Text    = "-";
            lblSummaryQuant.Text   = "-";
            _currentModelLayers    = 32;
            _currentModelFileSize  = 0;
            return;
        }

        lblSummaryModel.Text   = p.ModelFileName;
        lblSummaryPort.Text    = p.Port.ToString();
        lblSummaryContext.Text = p.ContextSize.ToString("N0");
        lblSummaryNgl.Text     = p.GpuLayers.ToString();

        // Read GGUF header metadata
        if (!string.IsNullOrWhiteSpace(p.ModelPath) && File.Exists(p.ModelPath))
        {
            var meta = GgufReader.ReadMetadata(p.ModelPath);
            lblSummaryArch.Text   = meta.Architecture;
            lblSummaryQuant.Text  = meta.Quantization;
            _currentModelLayers   = meta.LayerCount > 0 ? meta.LayerCount : 32;
            _currentModelFileSize = meta.FileSizeBytes;
        }
        else
        {
            lblSummaryArch.Text   = "(no model)";
            lblSummaryQuant.Text  = "(no model)";
            _currentModelLayers   = 32;
            _currentModelFileSize = 0;
        }
    }

    private void PopulateQuickEdit()
    {
        if (cmbProfiles.SelectedItem is not ModelProfile p)
        {
            nudQePort.Enabled            = false;
            cmbQeContextPresets.Enabled  = false;
            nudQeContext.Enabled         = false;
            nudQeNgl.Enabled             = false;
            chkQeFlashAttn.Enabled       = false;
            chkQeNoMMap.Enabled          = false;
            chkQeMLock.Enabled           = false;
            chkQeEmbedding.Enabled       = false;
            chkQeDisableThinking.Enabled = false;
            txtQeExtra.Enabled           = false;
            chkSaveChanges.Enabled       = false;
            return;
        }

        nudQePort.Enabled            = true;
        cmbQeContextPresets.Enabled  = true;
        nudQeContext.Enabled         = true;
        nudQeNgl.Enabled             = true;
        chkQeFlashAttn.Enabled       = true;
        chkQeNoMMap.Enabled          = true;
        chkQeMLock.Enabled           = true;
        chkQeEmbedding.Enabled       = true;
        chkQeDisableThinking.Enabled = true;
        txtQeExtra.Enabled           = true;
        chkSaveChanges.Enabled       = true;

        nudQePort.Value            = Math.Clamp(p.Port, 1, 65535);
        nudQeContext.Value         = Math.Clamp(p.ContextSize, 1, 1_000_000);
        nudQeNgl.Value             = Math.Clamp(p.GpuLayers, 0, 9999);
        txtQeExtra.Text            = p.ExtraFlags;

        chkQeFlashAttn.Checked       = p.EnableFlashAttn;
        chkQeNoMMap.Checked          = p.NoMMap;
        chkQeMLock.Checked           = p.MLock;
        chkQeEmbedding.Checked       = p.EnableEmbedding;
        chkQeDisableThinking.Checked = p.DisableThinking;

        SelectQeContextPresetMatchingValue(p.ContextSize);
        UpdateVramEstimate();
    }

    private void SelectQeContextPresetMatchingValue(int ctx)
    {
        cmbQeContextPresets.SelectedIndex = ctx switch
        {
            8192 => 0,
            16384 => 1,
            32768 => 2,
            65536 => 3,
            131072 => 4,
            262144 => 5,
            524288 => 6,
            _ => 7
        };
    }

    private void cmbQeContextPresets_SelectedIndexChanged(object? sender, EventArgs e)
    {
        switch (cmbQeContextPresets.SelectedIndex)
        {
            case 0: nudQeContext.Value = 8192; break;
            case 1: nudQeContext.Value = 16384; break;
            case 2: nudQeContext.Value = 32768; break;
            case 3: nudQeContext.Value = 65536; break;
            case 4: nudQeContext.Value = 131072; break;
            case 5: nudQeContext.Value = 262144; break;
            case 6: nudQeContext.Value = 524288; break;
        }
        UpdateVramEstimate();
    }

    private void UpdateVramEstimate()
    {
        int ctx = (int)nudQeContext.Value;
        int ngl = (int)nudQeNgl.Value;
        bool flashAttn = chkQeFlashAttn.Checked;

        var est = VramEstimator.Estimate(_currentModelFileSize, ctx, ngl, _currentModelLayers, flashAttn);
        lblVramEstimate.Text = est.DisplayText;
        lblVramEstimate.ForeColor = est.Status switch
        {
            VramStatus.Warning => Color.DarkRed,
            VramStatus.High => Color.DarkOrange,
            _ => Color.DarkGreen
        };
    }

    // ── Settings Screen ───────────────────────────────────────────────────────

    private void btnSettings_Click(object sender, EventArgs e)
    {
        using var dlg = new GlobalSettingsForm(_settings);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _settings = dlg.ResultSettings;
            _profileService.SaveSettings(_settings);
            UpdateServerExeLabel();
        }
    }

    private void UpdateServerExeLabel()
    {
        if (string.IsNullOrWhiteSpace(_settings.ServerExePath))
        {
            lblServerExe.Text = "llama-server.exe: (not set — click Settings)";
            toolTip.SetToolTip(lblServerExe, "Click Settings to select llama-server.exe path");
        }
        else
        {
            lblServerExe.Text = $"llama-server.exe: {Path.GetFileName(_settings.ServerExePath)}";
            toolTip.SetToolTip(lblServerExe, _settings.ServerExePath);
        }
    }

    // ── Manage Models ─────────────────────────────────────────────────────────

    private void btnManageModels_Click(object sender, EventArgs e)
    {
        using var dlg = new ManageModelsForm(_profileService);
        dlg.ShowDialog(this);

        // Reload after close — profiles may have changed
        _profiles = _profileService.LoadProfiles();
        RefreshProfileDropdown();
    }

    // ── Server Start / Stop / Web UI ──────────────────────────────────────────

    private void btnStartStop_Click(object sender, EventArgs e)
    {
        if (_serverManager.IsRunning)
        {
            StopServer();
        }
        else
        {
            StartServer();
        }
    }

    private void btnOpenWebUi_Click(object sender, EventArgs e)
    {
        int port = (int)nudQePort.Value;
        string url = $"http://localhost:{port}";
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open browser:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void StartServer()
    {
        if (cmbProfiles.SelectedItem is not ModelProfile profile)
        {
            MessageBox.Show("Please select or create a model profile first.", "No Profile Selected",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.ServerExePath))
        {
            MessageBox.Show("llama-server.exe path is not configured.\n\nClick Settings to set it.",
                "Server Not Configured", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Build quick-edit overrides
        var overrides = new QuickEditOverrides
        {
            Port            = (int)nudQePort.Value,
            ContextSize     = (int)nudQeContext.Value,
            GpuLayers       = (int)nudQeNgl.Value,
            ExtraFlags      = txtQeExtra.Text.Trim(),
            EnableFlashAttn = chkQeFlashAttn.Checked,
            NoMMap          = chkQeNoMMap.Checked,
            MLock           = chkQeMLock.Checked,
            EnableEmbedding = chkQeEmbedding.Checked,
            DisableThinking = chkQeDisableThinking.Checked,
        };

        // Optionally save overrides back to the profile
        if (chkSaveChanges.Checked)
        {
            profile.Port            = overrides.Port!.Value;
            profile.ContextSize     = overrides.ContextSize!.Value;
            profile.GpuLayers       = overrides.GpuLayers!.Value;
            profile.ExtraFlags      = overrides.ExtraFlags!;
            profile.EnableFlashAttn = overrides.EnableFlashAttn!.Value;
            profile.NoMMap          = overrides.NoMMap!.Value;
            profile.MLock           = overrides.MLock!.Value;
            profile.EnableEmbedding = overrides.EnableEmbedding!.Value;
            profile.DisableThinking = overrides.DisableThinking!.Value;
            _profileService.SaveProfiles(_profiles);
            UpdateProfileSummary();
        }

        // Save last-used profile
        _settings.LastSelectedProfileId = profile.Id;
        _profileService.SaveSettings(_settings);

        try
        {
            SetStatus(ServerStatus.Starting);
            _serverManager.StartServer(_settings.ServerExePath, profile, overrides);

            if (_serverManager.IsRunning)
                SetStatus(ServerStatus.Running, profile.Name, overrides.Port!.Value);
        }
        catch (FileNotFoundException ex)
        {
            SetStatus(ServerStatus.Error);
            MessageBox.Show(ex.Message, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            SetStatus(ServerStatus.Error);
            MessageBox.Show($"Failed to start server:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void StopServer()
    {
        _serverManager.StopServer();
    }

    // ── Server Events ─────────────────────────────────────────────────────────

    private void WireServerEvents()
    {
        _serverManager.OutputReceived += (_, line) =>
            _logQueue.Enqueue(line);

        _serverManager.ServerStopped += (_, _) =>
            SafeInvoke(() => SetStatus(ServerStatus.Idle));
    }

    private void SafeInvoke(Action action)
    {
        if (IsHandleCreated && !IsDisposed)
        {
            try { Invoke(action); }
            catch (ObjectDisposedException) { /* form closing */ }
        }
    }

    // ── Log Panel Batching ────────────────────────────────────────────────────

    private void FlushLogQueue()
    {
        if (_logQueue.IsEmpty) return;

        var sb = new System.Text.StringBuilder();
        while (_logQueue.TryDequeue(out string? line))
        {
            sb.AppendLine(line);
        }

        if (sb.Length == 0) return;

        if (rtbLog.Lines.Length > 4000)
        {
            int charIdx = rtbLog.GetFirstCharIndexFromLine(500);
            if (charIdx > 0)
            {
                rtbLog.Select(0, charIdx);
                rtbLog.SelectedText = string.Empty;
            }
        }

        rtbLog.AppendText(sb.ToString());
        rtbLog.ScrollToCaret();
    }

    private void btnToggleLog_Click(object sender, EventArgs e)
    {
        _logPanelVisible = !_logPanelVisible;
        panelLog.Visible  = _logPanelVisible;
        btnToggleLog.Text = _logPanelVisible ? "📋 Log Panel" : "📋 Show Log";
    }

    private void btnClearLog_Click(object sender, EventArgs e) => rtbLog.Clear();

    // ── Status Indicator ──────────────────────────────────────────────────────

    private enum ServerStatus { Idle, Starting, Running, Error }

    private void SetStatus(ServerStatus status, string? profileName = null, int port = 0)
    {
        switch (status)
        {
            case ServerStatus.Idle:
                lblStatus.Text        = "Idle";
                panelStatusDot.BackColor = Color.Gray;
                panelStatusDot.Invalidate();
                btnStartStop.Text     = "▶  Start Server";
                btnStartStop.BackColor = Color.FromArgb(0, 120, 212);
                btnStartStop.Enabled  = true;
                btnOpenWebUi.Enabled  = false;
                UpdateTrayIcon(running: false);
                UpdateTrayTooltip("Idle");
                UpdateTrayMenuStartStop(running: false);
                break;

            case ServerStatus.Starting:
                lblStatus.Text        = "Starting…";
                panelStatusDot.BackColor = Color.Orange;
                panelStatusDot.Invalidate();
                btnStartStop.Text     = "Starting…";
                btnStartStop.Enabled  = false;
                btnOpenWebUi.Enabled  = false;
                break;

            case ServerStatus.Running:
                lblStatus.Text        = $"Running on :{port}";
                panelStatusDot.BackColor = Color.LimeGreen;
                panelStatusDot.Invalidate();
                btnStartStop.Text     = "■  Stop Server";
                btnStartStop.BackColor = Color.FromArgb(196, 43, 28);
                btnStartStop.Enabled  = true;
                btnOpenWebUi.Enabled  = true;
                UpdateTrayIcon(running: true);
                UpdateTrayTooltip($"Running: {profileName} on :{port}");
                UpdateTrayMenuStartStop(running: true);
                break;

            case ServerStatus.Error:
                lblStatus.Text        = "Error";
                panelStatusDot.BackColor = Color.Red;
                panelStatusDot.Invalidate();
                btnStartStop.Text     = "▶  Start Server";
                btnStartStop.BackColor = Color.FromArgb(0, 120, 212);
                btnStartStop.Enabled  = true;
                btnOpenWebUi.Enabled  = false;
                UpdateTrayIcon(running: false);
                break;
        }
    }

    // ── System Tray ───────────────────────────────────────────────────────────

    private void notifyIcon_DoubleClick(object sender, EventArgs e) => ShowMainWindow();

    private void ShowMainWindow()
    {
        Show();
        WindowState   = FormWindowState.Normal;
        ShowInTaskbar = true;
        Activate();
    }

    private void trayMenuShow_Click(object sender, EventArgs e)    => ShowMainWindow();
    private void trayMenuExit_Click(object sender, EventArgs e)
    {
        if (_serverManager.IsRunning)
        {
            var result = MessageBox.Show(
                "A server is currently running. Exiting will stop it.\n\nExit anyway?",
                "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;
        }

        _serverManager.StopServer();
        _allowClose = true;
        Application.Exit();
    }

    private void trayMenuStartStop_Click(object sender, EventArgs e)
    {
        if (_serverManager.IsRunning)
            StopServer();
        else
            StartServer();
    }

    private void UpdateTrayIcon(bool running)
    {
        notifyIcon.Icon = running ? IconHelper.RunningIcon : IconHelper.IdleIcon;
    }

    private void UpdateTrayTooltip(string statusText)
    {
        string tip = $"LlamaLauncher — {statusText}";
        notifyIcon.Text = tip.Length > 63 ? tip[..63] : tip;
    }

    private void UpdateTrayMenuStartStop(bool running)
    {
        trayMenuStartStop.Text = running ? "Stop Server" : "Start Server";
    }

    // ── Window Close / Minimize behavior ─────────────────────────────────────

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_allowClose && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            ShowInTaskbar = false;
            notifyIcon.ShowBalloonTip(1500, "LlamaLauncher",
                "App is still running in the system tray.", ToolTipIcon.Info);
        }
        else
        {
            if (!_disposed)
            {
                _disposed = true;
                _logFlushTimer.Stop();
                _logFlushTimer.Dispose();
                _serverManager.Dispose();
                notifyIcon.Visible = false;
            }
        }

        base.OnFormClosing(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized)
        {
            Hide();
            ShowInTaskbar = false;
        }
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (!_disposed)
        {
            _disposed = true;
            _logFlushTimer.Stop();
            _logFlushTimer.Dispose();
            _serverManager.Dispose();
        }
        base.OnFormClosed(e);
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
        public MEMORYSTATUSEX() { dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MEMORYSTATUSEX)); }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out] MEMORYSTATUSEX lpBuffer);
}

/// <summary>
/// Helper to programmatically generate small colored circle icons for tray states.
/// Caches idle and running icons to avoid Win32 / GDI handle leaks.
/// </summary>
internal static class IconHelper
{
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private static Icon? s_idleIcon;
    private static Icon? s_runningIcon;

    public static Icon IdleIcon => s_idleIcon ??= CreateCircleIcon(Color.Gray);
    public static Icon RunningIcon => s_runningIcon ??= CreateCircleIcon(Color.LimeGreen);

    public static Icon CreateCircleIcon(Color color)
    {
        using var bmp = new Bitmap(16, 16);
        using var g   = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, 1, 1, 13, 13);

        IntPtr hIcon = bmp.GetHicon();
        try
        {
            return (Icon)Icon.FromHandle(hIcon).Clone();
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }
}

/// <summary>
/// Selects the best available monospace font from a preference list.
/// </summary>
internal static class FontHelper
{
    private static readonly string[] PreferredMonoFonts = ["Cascadia Mono", "Consolas", "Courier New"];

    public static Font MonospaceFont(float size)
    {
        using var installed = new System.Drawing.Text.InstalledFontCollection();
        var names = installed.Families.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var name in PreferredMonoFonts)
        {
            if (names.Contains(name))
                return new Font(name, size);
        }

        return new Font(FontFamily.GenericMonospace, size);
    }
}


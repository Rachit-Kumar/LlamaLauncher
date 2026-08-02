using LlamaLauncher.Models;

namespace LlamaLauncher.Forms;

/// <summary>
/// Simple dialog for editing global app settings:
///   - Path to llama-server.exe
///   - Start minimized to tray toggle
/// </summary>
public partial class GlobalSettingsForm : Form
{
    public AppSettings ResultSettings { get; private set; }

    public GlobalSettingsForm(AppSettings current)
    {
        ResultSettings = new AppSettings
        {
            ServerExePath         = current.ServerExePath,
            LastSelectedProfileId = current.LastSelectedProfileId,
            StartMinimizedToTray  = current.StartMinimizedToTray,
        };

        InitializeComponent();

        txtExePath.Text              = current.ServerExePath;
        chkStartMinimized.Checked    = current.StartMinimizedToTray;

        DetectHardware();
    }

    private Services.CudaHardwareInfo _detectedCuda = new();

    private void DetectHardware()
    {
        _detectedCuda = Services.CudaDetectorService.DetectCudaHardware();
        if (_detectedCuda.HasNvidiaGpu)
        {
            lblCudaInfo.Text = $"GPU: {_detectedCuda.GpuName} | {_detectedCuda.CudaVersionString} ({_detectedCuda.TotalVramGb} GB VRAM) -> Rec: {_detectedCuda.RecommendedBuildType}";
            lblCudaInfo.ForeColor = Color.DarkGreen;
        }
        else
        {
            lblCudaInfo.Text = "GPU: No NVIDIA CUDA GPU detected -> Rec: CPU AVX2";
            lblCudaInfo.ForeColor = Color.DarkOrange;
        }
    }

    private void btnBrowseExe_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title  = "Locate llama-server.exe",
            Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*",
        };

        var dir = string.IsNullOrWhiteSpace(txtExePath.Text)
            ? null
            : Path.GetDirectoryName(txtExePath.Text);

        if (dir is not null && Directory.Exists(dir))
            dlg.InitialDirectory = dir;

        if (dlg.ShowDialog() == DialogResult.OK)
            txtExePath.Text = dlg.FileName;
    }

    private async void btnCheckUpdates_Click(object sender, EventArgs e)
    {
        btnCheckUpdates.Enabled = false;
        lblUpdateStatus.Text = "Checking GitHub...";

        var release = await Services.UpdateCheckerService.CheckLatestReleaseAsync();
        if (release is not null && !string.IsNullOrWhiteSpace(release.TagName))
        {
            string installedVer = Services.LlamaDownloaderService.GetInstalledVersion();
            bool isInstalled = !string.IsNullOrWhiteSpace(txtExePath.Text) && File.Exists(txtExePath.Text);

            if (isInstalled && installedVer.Equals(release.TagName, StringComparison.OrdinalIgnoreCase))
            {
                lblUpdateStatus.Text = $"✓ Up to date ({release.TagName})";
                lblUpdateStatus.ForeColor = Color.DarkGreen;
                btnAutoDownload.Text = "Re-download";
                btnAutoDownload.BackColor = Color.Gray;
            }
            else
            {
                lblUpdateStatus.Text = $"⚡ New Release Available: {release.TagName}";
                lblUpdateStatus.ForeColor = Color.DarkBlue;
                btnAutoDownload.Text = $"⚡ Update to {release.TagName}";
                btnAutoDownload.BackColor = Color.FromArgb(0, 120, 212);
            }
        }
        else
        {
            lblUpdateStatus.Text = "Could not check updates";
            lblUpdateStatus.ForeColor = Color.Red;
        }

        btnCheckUpdates.Enabled = true;
    }

    private CancellationTokenSource? _ctsDownload;

    private async void btnAutoDownload_Click(object sender, EventArgs e)
    {
        var confirm = MessageBox.Show(
            $"Are you sure you want to download and install llama-server.exe?\n\nTarget Hardware Build: {_detectedCuda.RecommendedBuildType.ToUpper()}\nTarget Directory: llama-bin/",
            "Confirm Download", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        _ctsDownload = new CancellationTokenSource();
        btnAutoDownload.Enabled = false;
        btnCheckUpdates.Enabled = false;
        progressBarDownload.Visible = true;
        btnCancelDownload.Visible = true;
        progressBarDownload.Value = 0;
        lblUpdateStatus.Text = "Downloading release package...";
        lblUpdateStatus.ForeColor = Color.DarkBlue;

        var progress = new Progress<double>(percent =>
        {
            progressBarDownload.Value = Math.Clamp((int)percent, 0, 100);
            lblUpdateStatus.Text = $"Downloading: {percent:F0}%";
        });

        try
        {
            string serverPath = await Services.LlamaDownloaderService.DownloadAndExtractLlamaServerAsync(
                _detectedCuda.RecommendedBuildType, progress, _ctsDownload.Token);

            txtExePath.Text = serverPath;
            lblUpdateStatus.Text = "✓ Installed!";
            lblUpdateStatus.ForeColor = Color.DarkGreen;

            MessageBox.Show($"llama-server.exe successfully installed and set to:\n{serverPath}",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            lblUpdateStatus.Text = "Download cancelled";
            lblUpdateStatus.ForeColor = Color.DarkOrange;
        }
        catch (Exception ex)
        {
            lblUpdateStatus.Text = "Download failed";
            lblUpdateStatus.ForeColor = Color.Red;
            MessageBox.Show($"Failed to download or extract llama-server.exe:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            progressBarDownload.Visible = false;
            btnCancelDownload.Visible = false;
            btnAutoDownload.Enabled = true;
            btnCheckUpdates.Enabled = true;
            _ctsDownload?.Dispose();
            _ctsDownload = null;
        }
    }

    private void btnCancelDownload_Click(object sender, EventArgs e)
    {
        _ctsDownload?.Cancel();
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
        string exePath = txtExePath.Text.Trim();

        if (!string.IsNullOrWhiteSpace(exePath) && !File.Exists(exePath))
        {
            var choice = MessageBox.Show(
                $"The specified path does not exist:\n{exePath}\n\nSave anyway?",
                "File Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (choice == DialogResult.No) return;
        }

        ResultSettings.ServerExePath        = exePath;
        ResultSettings.StartMinimizedToTray = chkStartMinimized.Checked;

        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}

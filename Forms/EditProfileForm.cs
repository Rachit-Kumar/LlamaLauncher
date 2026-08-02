using LlamaLauncher.Models;

namespace LlamaLauncher.Forms;

/// <summary>
/// Dialog for creating a new profile or editing an existing one.
/// Also doubles as the global settings editor when <see cref="ServerExePath"/> mode is used.
/// </summary>
public partial class EditProfileForm : Form
{
    // The profile being edited (null for a brand-new one)
    private readonly ModelProfile? _original;

    /// <summary>The resulting profile after the user clicks OK.</summary>
    public ModelProfile ResultProfile { get; private set; } = new();

    public EditProfileForm(ModelProfile? existing = null)
    {
        _original = existing;
        InitializeComponent();

        cmbContextPresets.SelectedIndexChanged += cmbContextPresets_SelectedIndexChanged;

        if (existing is not null)
        {
            Text = "Edit Profile";
            PopulateFields(existing);
        }
        else
        {
            Text = "Add Profile";
            cmbContextPresets.SelectedIndex = 0; // 8K default
        }
    }

    private void PopulateFields(ModelProfile p)
    {
        txtName.Text            = p.Name;
        txtModelPath.Text       = p.ModelPath;
        nudPort.Value           = Math.Clamp(p.Port, 1, 65535);
        nudContext.Value        = Math.Clamp(p.ContextSize, 1, 1_000_000);
        nudGpuLayers.Value      = Math.Clamp(p.GpuLayers, 0, 9999);
        txtExtraFlags.Text      = p.ExtraFlags;
        chkFlashAttn.Checked    = p.EnableFlashAttn;
        chkNoMMap.Checked       = p.NoMMap;
        chkMLock.Checked        = p.MLock;
        chkEmbedding.Checked    = p.EnableEmbedding;
        chkDisableThinking.Checked = p.DisableThinking;

        SelectContextPresetMatchingValue(p.ContextSize);
    }

    private void SelectContextPresetMatchingValue(int ctx)
    {
        cmbContextPresets.SelectedIndex = ctx switch
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

    private void cmbContextPresets_SelectedIndexChanged(object? sender, EventArgs e)
    {
        switch (cmbContextPresets.SelectedIndex)
        {
            case 0: nudContext.Value = 8192; break;
            case 1: nudContext.Value = 16384; break;
            case 2: nudContext.Value = 32768; break;
            case 3: nudContext.Value = 65536; break;
            case 4: nudContext.Value = 131072; break;
            case 5: nudContext.Value = 262144; break;
            case 6: nudContext.Value = 524288; break;
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void btnBrowseModel_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title  = "Select GGUF Model File",
            Filter = "GGUF models (*.gguf)|*.gguf|All files (*.*)|*.*",
        };

        var dir = string.IsNullOrWhiteSpace(txtModelPath.Text)
            ? null
            : Path.GetDirectoryName(txtModelPath.Text);

        if (dir is not null && Directory.Exists(dir))
            dlg.InitialDirectory = dir;

        if (dlg.ShowDialog() == DialogResult.OK)
            txtModelPath.Text = dlg.FileName;
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
        // ── Validation ──
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Profile name cannot be empty.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtName.Focus();
            return;
        }

        if (!string.IsNullOrWhiteSpace(txtModelPath.Text) &&
            !File.Exists(txtModelPath.Text))
        {
            var choice = MessageBox.Show(
                $"The model file does not exist at the specified path:\n{txtModelPath.Text}\n\nSave anyway?",
                "File Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (choice == DialogResult.No) return;
        }

        // ── Build result ──
        ResultProfile = new ModelProfile
        {
            Id              = _original?.Id ?? Guid.NewGuid(),
            Name            = txtName.Text.Trim(),
            ModelPath       = txtModelPath.Text.Trim(),
            Port            = (int)nudPort.Value,
            ContextSize     = (int)nudContext.Value,
            GpuLayers       = (int)nudGpuLayers.Value,
            ExtraFlags      = txtExtraFlags.Text.Trim(),
            EnableFlashAttn = chkFlashAttn.Checked,
            NoMMap          = chkNoMMap.Checked,
            MLock           = chkMLock.Checked,
            EnableEmbedding = chkEmbedding.Checked,
            DisableThinking = chkDisableThinking.Checked,
        };

        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}

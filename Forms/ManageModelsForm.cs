using LlamaLauncher.Models;
using LlamaLauncher.Services;

namespace LlamaLauncher.Forms;

/// <summary>
/// Shows all saved profiles in a list with Add / Edit / Delete actions.
/// Changes are persisted via <see cref="ProfileService"/> immediately.
/// The parent form reloads profiles when this dialog closes.
/// </summary>
public partial class ManageModelsForm : Form
{
    private readonly ProfileService _profileService;
    private List<ModelProfile> _profiles;

    public ManageModelsForm(ProfileService profileService)
    {
        _profileService = profileService;
        _profiles       = _profileService.LoadProfiles();

        InitializeComponent();
        RefreshList();
    }

    // ── List management ───────────────────────────────────────────────────────

    private void RefreshList()
    {
        listProfiles.BeginUpdate();
        listProfiles.Items.Clear();
        foreach (var p in _profiles)
            listProfiles.Items.Add(p);
        listProfiles.EndUpdate();

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool hasSelection = listProfiles.SelectedIndex >= 0;
        btnEdit.Enabled   = hasSelection;
        btnDelete.Enabled = hasSelection;
        btnMoveUp.Enabled = hasSelection && listProfiles.SelectedIndex > 0;
        btnMoveDown.Enabled = hasSelection && listProfiles.SelectedIndex < listProfiles.Items.Count - 1;
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    private void btnAdd_Click(object sender, EventArgs e)
    {
        using var dlg = new EditProfileForm();
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _profiles.Add(dlg.ResultProfile);
            _profileService.SaveProfiles(_profiles);
            RefreshList();
            // Select the newly added item
            listProfiles.SelectedIndex = listProfiles.Items.Count - 1;
        }
    }

    private void btnEdit_Click(object sender, EventArgs e)
    {
        if (listProfiles.SelectedItem is not ModelProfile selected) return;

        using var dlg = new EditProfileForm(selected);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            int idx = _profiles.FindIndex(p => p.Id == selected.Id);
            if (idx >= 0) _profiles[idx] = dlg.ResultProfile;

            _profileService.SaveProfiles(_profiles);
            RefreshList();
        }
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (listProfiles.SelectedItem is not ModelProfile selected) return;

        var confirm = MessageBox.Show(
            $"Delete profile \"{selected.Name}\"?",
            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (confirm == DialogResult.Yes)
        {
            _profiles.RemoveAll(p => p.Id == selected.Id);
            _profileService.SaveProfiles(_profiles);
            RefreshList();
        }
    }

    private void btnMoveUp_Click(object sender, EventArgs e)
    {
        int idx = listProfiles.SelectedIndex;
        if (idx <= 0) return;
        (_profiles[idx], _profiles[idx - 1]) = (_profiles[idx - 1], _profiles[idx]);
        _profileService.SaveProfiles(_profiles);
        RefreshList();
        listProfiles.SelectedIndex = idx - 1;
    }

    private void btnMoveDown_Click(object sender, EventArgs e)
    {
        int idx = listProfiles.SelectedIndex;
        if (idx < 0 || idx >= _profiles.Count - 1) return;
        (_profiles[idx], _profiles[idx + 1]) = (_profiles[idx + 1], _profiles[idx]);
        _profileService.SaveProfiles(_profiles);
        RefreshList();
        listProfiles.SelectedIndex = idx + 1;
    }

    private void listProfiles_SelectedIndexChanged(object sender, EventArgs e) => UpdateButtons();

    private void listProfiles_DoubleClick(object sender, EventArgs e) => btnEdit_Click(sender, e);

    private void btnClose_Click(object sender, EventArgs e) => Close();
}

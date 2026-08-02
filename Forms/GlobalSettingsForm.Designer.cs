namespace LlamaLauncher.Forms;

partial class GlobalSettingsForm
{
    private System.ComponentModel.IContainer components = null!;

    private Label lblExePath = null!;
    private TextBox txtExePath = null!;
    private Button btnBrowseExe = null!;
    private CheckBox chkStartMinimized = null!;
    private Button btnCheckUpdates = null!;
    private Button btnAutoDownload = null!;
    private Button btnCancelDownload = null!;
    private Label lblCudaInfo = null!;
    private Label lblUpdateStatus = null!;
    private ProgressBar progressBarDownload = null!;
    private Button btnOk = null!;
    private Button btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        Text            = "Settings";
        Size            = new Size(580, 310);
        MinimumSize     = new Size(500, 290);
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Font            = new Font("Segoe UI", 9f);
        Padding         = new Padding(14);

        // ── Layout ────────────────────────────────────────────────────────────
        var table = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 3,
            RowCount    = 5,
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Row 0: exe path
        lblExePath = new Label
        {
            Text      = "llama-server.exe:",
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Padding   = new Padding(0, 0, 8, 0),
        };
        txtExePath = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 8, 4, 8) };
        btnBrowseExe = new Button
        {
            Text   = "Browse…",
            Dock   = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 8),
        };
        btnBrowseExe.Click += btnBrowseExe_Click;

        table.Controls.Add(lblExePath,    0, 0);
        table.Controls.Add(txtExePath,    1, 0);
        table.Controls.Add(btnBrowseExe,  2, 0);

        // Row 1: start minimized
        chkStartMinimized = new CheckBox
        {
            Text   = "Start minimized to system tray",
            Dock   = DockStyle.Fill,
            Margin = new Padding(130, 4, 0, 4),
        };
        table.Controls.Add(chkStartMinimized, 0, 1);
        table.SetColumnSpan(chkStartMinimized, 3);

        // Row 2: hardware / cuda info
        lblCudaInfo = new Label
        {
            Text      = "Hardware: Detecting GPU & CUDA...",
            ForeColor = SystemColors.HotTrack,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Margin    = new Padding(130, 0, 0, 0),
        };
        table.Controls.Add(lblCudaInfo, 0, 2);
        table.SetColumnSpan(lblCudaInfo, 3);

        // Row 3: update check & auto-download
        btnCheckUpdates = new Button
        {
            Text   = "Check Release",
            Width  = 105,
            Height = 28,
            Margin = new Padding(0, 4, 6, 4),
        };
        btnCheckUpdates.Click += btnCheckUpdates_Click;

        btnAutoDownload = new Button
        {
            Text      = "⚡ 1-Click Download & Extract",
            Width     = 185,
            Height    = 28,
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Margin    = new Padding(0, 4, 8, 4),
        };
        btnAutoDownload.FlatAppearance.BorderSize = 0;
        btnAutoDownload.Click += btnAutoDownload_Click;

        lblUpdateStatus = new Label
        {
            Text      = "Idle",
            ForeColor = SystemColors.GrayText,
            Height    = 28,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize  = true,
        };

        progressBarDownload = new ProgressBar
        {
            Width   = 100,
            Height  = 20,
            Visible = false,
            Margin  = new Padding(0, 4, 6, 0),
        };

        btnCancelDownload = new Button
        {
            Text      = "Cancel",
            Width     = 65,
            Height    = 28,
            Visible   = false,
            Margin    = new Padding(0, 0, 6, 0),
        };
        btnCancelDownload.Click += btnCancelDownload_Click;

        var panelUpdate = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false,
            Margin        = new Padding(130, 0, 0, 0),
        };
        panelUpdate.Controls.AddRange([btnCheckUpdates, btnAutoDownload, progressBarDownload, btnCancelDownload, lblUpdateStatus]);
        table.Controls.Add(panelUpdate, 0, 3);
        table.SetColumnSpan(panelUpdate, 3);

        // Row 4: buttons (right-aligned)
        var panelBtns = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents  = false,
            Padding       = new Padding(0, 6, 0, 0),
        };

        btnCancel = new Button { Text = "Cancel", Width = 80, Height = 28 };
        btnCancel.Click += btnCancel_Click;
        CancelButton = btnCancel;

        btnOk = new Button { Text = "OK", Width = 80, Height = 28 };
        btnOk.Click += btnOk_Click;
        AcceptButton = btnOk;

        panelBtns.Controls.AddRange([btnCancel, btnOk]);

        table.Controls.Add(panelBtns, 0, 4);
        table.SetColumnSpan(panelBtns, 3);

        Controls.Add(table);
        ResumeLayout(false);
    }
}

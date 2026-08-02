namespace LlamaLauncher.Forms;

partial class EditProfileForm
{
    private System.ComponentModel.IContainer components = null!;

    // Controls
    private Label lblName = null!;
    private TextBox txtName = null!;
    private Label lblModelPath = null!;
    private TextBox txtModelPath = null!;
    private Button btnBrowseModel = null!;
    private Label lblPort = null!;
    private NumericUpDown nudPort = null!;
    private Label lblContext = null!;
    private ComboBox cmbContextPresets = null!;
    private NumericUpDown nudContext = null!;
    private Label lblGpuLayers = null!;
    private NumericUpDown nudGpuLayers = null!;
    private Label lblFlags = null!;
    private CheckBox chkFlashAttn = null!;
    private CheckBox chkNoMMap = null!;
    private CheckBox chkMLock = null!;
    private CheckBox chkEmbedding = null!;
    private CheckBox chkDisableThinking = null!;
    private Label lblExtraFlags = null!;
    private TextBox txtExtraFlags = null!;
    private Label lblExtraFlagsHint = null!;
    private Button btnOk = null!;
    private Button btnCancel = null!;
    private TableLayoutPanel tableLayout = null!;
    private Panel panelButtons = null!;

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

        // ── Form ─────────────────────────────────────────────────────────────
        Text            = "Edit Profile";
        Size            = new Size(620, 540);
        MinimumSize     = new Size(540, 480);
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Font            = new Font("Segoe UI", 9.5f);
        Padding         = new Padding(16);

        // ── Table layout ─────────────────────────────────────────────────────
        tableLayout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 3,
            RowCount    = 8,
            Padding     = new Padding(0, 0, 0, 6),
        };
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

        // Row heights
        for (int i = 0; i < 5; i++)
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 65)); // flags row
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // extra flags row
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // hint

        // ── Controls ──────────────────────────────────────────────────────────

        // Name
        lblName = MakeLabel("Profile Name:");
        txtName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 6, 4) };
        tableLayout.Controls.Add(lblName,  0, 0);
        tableLayout.Controls.Add(txtName,  1, 0);
        tableLayout.SetColumnSpan(txtName, 2);

        // Model path
        lblModelPath = MakeLabel("Model File:");
        txtModelPath = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 4, 4) };
        btnBrowseModel = new Button
        {
            Text   = "Browse…",
            Dock   = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 4),
        };
        btnBrowseModel.Click += btnBrowseModel_Click;
        tableLayout.Controls.Add(lblModelPath,    0, 1);
        tableLayout.Controls.Add(txtModelPath,    1, 1);
        tableLayout.Controls.Add(btnBrowseModel,  2, 1);

        // Port
        lblPort = MakeLabel("Port:");
        nudPort = new NumericUpDown
        {
            Minimum = 1, Maximum = 65535, Value = 8080,
            Dock = DockStyle.Fill, Margin = new Padding(0, 4, 6, 4),
        };
        tableLayout.Controls.Add(lblPort, 0, 2);
        tableLayout.Controls.Add(nudPort, 1, 2);
        tableLayout.SetColumnSpan(nudPort, 2);

        // Context size
        lblContext = MakeLabel("Context Size:");
        var panelContext = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 2, 0, 2),
        };
        cmbContextPresets = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 160, Height = 26,
            Margin = new Padding(0, 2, 6, 2),
        };
        cmbContextPresets.Items.AddRange([
            "8,192 (8K)",
            "16,384 (16K)",
            "32,768 (32K)",
            "65,536 (64K)",
            "131,072 (128K)",
            "262,144 (256K)",
            "524,288 (512K)",
            "Custom..."
        ]);

        nudContext = new NumericUpDown
        {
            Minimum = 512, Maximum = 1_000_000, Value = 8192,
            Increment = 1024, ThousandsSeparator = true,
            Width = 110, Height = 26,
            Margin = new Padding(0, 2, 0, 2),
        };
        panelContext.Controls.AddRange([cmbContextPresets, nudContext]);
        tableLayout.Controls.Add(lblContext, 0, 3);
        tableLayout.Controls.Add(panelContext, 1, 3);
        tableLayout.SetColumnSpan(panelContext, 2);

        // GPU layers
        lblGpuLayers = MakeLabel("GPU Layers:");
        nudGpuLayers = new NumericUpDown
        {
            Minimum = 0, Maximum = 9999, Value = 999,
            Dock = DockStyle.Fill, Margin = new Padding(0, 4, 6, 4),
        };
        tableLayout.Controls.Add(lblGpuLayers, 0, 4);
        tableLayout.Controls.Add(nudGpuLayers, 1, 4);
        tableLayout.SetColumnSpan(nudGpuLayers, 2);

        // Flags row
        lblFlags = MakeLabel("Common Flags:");
        var panelFlags = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 2, 0, 2),
        };
        chkFlashAttn = new CheckBox { Text = "⚡ Flash Attn (-fa)", AutoSize = true, Checked = true, Margin = new Padding(0, 2, 10, 2) };
        chkNoMMap = new CheckBox { Text = "🚫 No mmap", AutoSize = true, Margin = new Padding(0, 2, 10, 2) };
        chkMLock = new CheckBox { Text = "🔒 Lock RAM", AutoSize = true, Margin = new Padding(0, 2, 10, 2) };
        chkEmbedding = new CheckBox { Text = "🧠 Embedding API", AutoSize = true, Margin = new Padding(0, 2, 10, 2) };
        chkDisableThinking = new CheckBox { Text = "🚫 Disable Thinking", AutoSize = true, Margin = new Padding(0, 2, 10, 2) };
        panelFlags.Controls.AddRange([chkFlashAttn, chkNoMMap, chkMLock, chkEmbedding, chkDisableThinking]);

        tableLayout.Controls.Add(lblFlags, 0, 5);
        tableLayout.Controls.Add(panelFlags, 1, 5);
        tableLayout.SetColumnSpan(panelFlags, 2);

        // Extra flags
        lblExtraFlags = MakeLabel("Extra Flags:");
        txtExtraFlags = new TextBox
        {
            Dock        = DockStyle.Fill,
            Multiline   = true,
            ScrollBars  = ScrollBars.Vertical,
            PlaceholderText = "--chat-template-kwargs ...",
            Margin      = new Padding(0, 4, 6, 0),
        };
        tableLayout.Controls.Add(lblExtraFlags,  0, 6);
        tableLayout.Controls.Add(txtExtraFlags,  1, 6);
        tableLayout.SetColumnSpan(txtExtraFlags, 2);

        // Extra flags hint
        lblExtraFlagsHint = new Label
        {
            Text      = "Appended verbatim to the server command line",
            ForeColor = SystemColors.GrayText,
            Font      = new Font("Segoe UI", 7.5f, FontStyle.Italic),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            Margin    = new Padding(110, 2, 6, 4),
        };
        tableLayout.Controls.Add(lblExtraFlagsHint, 0, 7);
        tableLayout.SetColumnSpan(lblExtraFlagsHint, 3);

        // ── Button panel ──────────────────────────────────────────────────────
        panelButtons = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(0, 6, 0, 0) };

        btnOk = new Button
        {
            Text     = "OK",
            Width    = 88,
            Height   = 30,
            Anchor   = AnchorStyles.Right | AnchorStyles.Bottom,
            Location = new Point(panelButtons.Width - 196, 6),
        };
        btnOk.Click += btnOk_Click;
        AcceptButton = btnOk;

        btnCancel = new Button
        {
            Text     = "Cancel",
            Width    = 88,
            Height   = 30,
            Anchor   = AnchorStyles.Right | AnchorStyles.Bottom,
            Location = new Point(panelButtons.Width - 100, 6),
        };
        btnCancel.Click += btnCancel_Click;
        CancelButton = btnCancel;

        panelButtons.Controls.AddRange([btnOk, btnCancel]);

        // ── Assemble ──────────────────────────────────────────────────────────
        Controls.Add(tableLayout);
        Controls.Add(panelButtons);

        // Fix button positions on resize
        panelButtons.SizeChanged += (_, _) =>
        {
            btnOk.Location     = new Point(panelButtons.Width - 196, 6);
            btnCancel.Location = new Point(panelButtons.Width - 100, 6);
        };

        ResumeLayout(false);
    }

    private static Label MakeLabel(string text) => new()
    {
        Text      = text,
        Dock      = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleRight,
        Padding   = new Padding(0, 0, 8, 0),
    };
}

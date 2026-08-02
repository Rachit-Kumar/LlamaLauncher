namespace LlamaLauncher.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    // ── Header ────────────────────────────────────────────────────────────────
    private Label lblAppTitle = null!;
    private Label lblServerExe = null!;
    private Button btnSettings = null!;
    private ToolTip toolTip = null!;

    // ── Profile selection ─────────────────────────────────────────────────────
    private Label lblProfileLabel = null!;
    private ComboBox cmbProfiles = null!;
    private Button btnManageModels = null!;

    // ── Tab Control ───────────────────────────────────────────────────────────
    private TabControl tabControlMain = null!;
    private TabPage tabPageConfig = null!;
    private TabPage tabPageInspector = null!;

    // ── Config Tab Controls ───────────────────────────────────────────────────
    private Label lblQePort = null!, lblQeContext = null!, lblQeNgl = null!, lblQeFlags = null!;
    private NumericUpDown nudQePort = null!, nudQeContext = null!, nudQeNgl = null!;
    private ComboBox cmbQeContextPresets = null!;
    private CheckBox chkQeFlashAttn = null!, chkQeNoMMap = null!, chkQeMLock = null!, chkQeEmbedding = null!, chkQeDisableThinking = null!;
    private TextBox txtQeExtra = null!;
    private CheckBox chkSaveChanges = null!;

    // ── Inspector Tab Controls ────────────────────────────────────────────────
    private Label lblSModel = null!, lblSPort = null!, lblSContext = null!, lblSNgl = null!;
    private Label lblSArch = null!, lblSQuant = null!;
    private Label lblSummaryModel = null!, lblSummaryPort = null!, lblSummaryContext = null!;
    private Label lblSummaryNgl = null!;
    private Label lblSummaryArch = null!, lblSummaryQuant = null!;
    private Label lblVramEstimate = null!;

    // ── Status row ────────────────────────────────────────────────────────────
    private Panel panelStatusDot = null!;
    private Label lblStatus = null!;
    private Label lblSystemRam = null!;

    // ── Action buttons ────────────────────────────────────────────────────────
    private Button btnStartStop = null!;
    private Button btnOpenWebUi = null!;
    private Button btnToggleLog = null!;

    // ── Log panel ─────────────────────────────────────────────────────────────
    private Panel panelLog = null!;
    private RichTextBox rtbLog = null!;
    private Button btnClearLog = null!;

    // ── System tray ───────────────────────────────────────────────────────────
    private NotifyIcon notifyIcon = null!;
    private ContextMenuStrip trayMenu = null!;
    private ToolStripMenuItem trayMenuShow = null!;
    private ToolStripMenuItem trayMenuStartStop = null!;
    private ToolStripSeparator trayMenuSep = null!;
    private ToolStripMenuItem trayMenuExit = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        toolTip = new ToolTip(components);
        SuspendLayout();

        // ── Form ──────────────────────────────────────────────────────────────
        Text            = "LlamaLauncher";
        Size            = new Size(840, 760);
        MinimumSize     = new Size(740, 660);
        StartPosition   = FormStartPosition.CenterScreen;
        Font            = new Font("Segoe UI", 9.5f);
        Padding         = new Padding(16);

        // ── Outer Layout (TableLayoutPanel for dynamic scaling) ───────────────
        var outer = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 1,
            RowCount    = 5,
        };
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); // Header
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); // Profile Hero Bar
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 46)); // Status & Hero Action Buttons
        outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // TabControl (Config / Inspector)
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 220)); // Log Panel

        // ── Header Row ────────────────────────────────────────────────────────
        var panelHeader = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            Margin      = new Padding(0, 0, 0, 6),
            ColumnCount = 3,
            RowCount    = 1,
        };
        panelHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panelHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        panelHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));

        lblAppTitle = new Label
        {
            Text      = "🦙 LlamaLauncher",
            Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
            AutoSize  = true,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
        };

        lblServerExe = new Label
        {
            Text         = "llama-server.exe: (not set)",
            ForeColor    = SystemColors.GrayText,
            Font         = new Font("Segoe UI", 9f),
            AutoEllipsis = true,
            Dock         = DockStyle.Fill,
            TextAlign    = ContentAlignment.MiddleRight,
            Margin       = new Padding(4, 0, 10, 0),
        };

        btnSettings = new Button
        {
            Text      = "⚙ Settings",
            Width     = 105,
            Height    = 32,
            Dock      = DockStyle.Right,
        };
        btnSettings.Click += btnSettings_Click;

        panelHeader.Controls.Add(lblAppTitle, 0, 0);
        panelHeader.Controls.Add(lblServerExe, 1, 0);
        panelHeader.Controls.Add(btnSettings, 2, 0);

        // ── Profile Selection Hero Card ───────────────────────────────────────
        var panelProfile = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            Margin      = new Padding(0, 0, 0, 6),
            ColumnCount = 3,
            RowCount    = 1,
        };
        panelProfile.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        panelProfile.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panelProfile.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));

        lblProfileLabel = new Label
        {
            Text      = "Profile:",
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
        };

        cmbProfiles = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock          = DockStyle.Fill,
            Font          = new Font("Segoe UI", 10f),
            Margin        = new Padding(0, 4, 8, 4),
        };
        cmbProfiles.SelectedIndexChanged += cmbProfiles_SelectedIndexChanged;

        btnManageModels = new Button
        {
            Text   = "Manage Models…",
            Dock   = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 4),
        };
        btnManageModels.Click += btnManageModels_Click;

        panelProfile.Controls.Add(lblProfileLabel, 0, 0);
        panelProfile.Controls.Add(cmbProfiles,     1, 0);
        panelProfile.Controls.Add(btnManageModels, 2, 0);

        // ── Status & Hero Action Buttons Row ──────────────────────────────────
        var panelActions = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            Margin      = new Padding(0, 0, 0, 8),
            ColumnCount = 5,
            RowCount    = 1,
        };
        panelActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24)); // Dot
        panelActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Status Text
        panelActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160)); // Start Button
        panelActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130)); // Web UI Button
        panelActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // Log Toggle Button

        panelStatusDot = new Panel
        {
            Width     = 14,
            Height    = 14,
            BackColor = Color.Gray,
            Anchor    = AnchorStyles.Left,
            Margin    = new Padding(0, 0, 6, 0),
        };
        panelStatusDot.Paint += (_, pe) =>
        {
            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var b = new SolidBrush(panelStatusDot.BackColor);
            pe.Graphics.FillEllipse(b, 0, 0, panelStatusDot.Width - 1, panelStatusDot.Height - 1);
        };

        lblStatus = new Label
        {
            Text      = "Idle",
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
        };

        btnStartStop = new Button
        {
            Text      = "▶  Start Server",
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            Margin    = new Padding(0, 2, 6, 2),
        };
        btnStartStop.FlatAppearance.BorderSize = 0;
        btnStartStop.Click += btnStartStop_Click;

        btnOpenWebUi = new Button
        {
            Text    = "🌐 Web UI",
            Dock    = DockStyle.Fill,
            Enabled = false,
            Margin  = new Padding(0, 2, 6, 2),
        };
        btnOpenWebUi.Click += btnOpenWebUi_Click;

        btnToggleLog = new Button
        {
            Text   = "📋 Log Panel",
            Dock   = DockStyle.Fill,
            Margin = new Padding(0, 2, 0, 2),
        };
        btnToggleLog.Click += btnToggleLog_Click;

        panelActions.Controls.Add(panelStatusDot, 0, 0);
        panelActions.Controls.Add(lblStatus,      1, 0);
        panelActions.Controls.Add(btnStartStop,   2, 0);
        panelActions.Controls.Add(btnOpenWebUi,   3, 0);
        panelActions.Controls.Add(btnToggleLog,   4, 0);

        // ── Tab Control ───────────────────────────────────────────────────────
        tabControlMain = new TabControl
        {
            Dock    = DockStyle.Fill,
            Margin  = new Padding(0, 0, 0, 8),
            Padding = new Point(14, 6),
        };

        tabPageConfig    = new TabPage { Text = "🎛️ Server Settings & Flags", Padding = new Padding(12) };
        tabPageInspector = new TabPage { Text = "📊 Model & VRAM Inspector",   Padding = new Padding(12) };

        // ── Tab 1: Config & Flags ─────────────────────────────────────────────
        var configLayout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 4,
            RowCount    = 4,
        };
        configLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        configLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        configLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        configLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        configLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        configLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        configLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        configLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        static Label MakeConfigLabel(string text) => new()
        {
            Text      = text,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        };

        lblQePort    = MakeConfigLabel("Port:");
        lblQeContext = MakeConfigLabel("Context:");
        lblQeNgl     = MakeConfigLabel("GPU Layers:");
        lblQeFlags   = MakeConfigLabel("Flags:");

        nudQePort = new NumericUpDown
        {
            Minimum = 1, Maximum = 65535, Value = 8080,
            Dock = DockStyle.Fill, Margin = new Padding(0, 4, 12, 4),
        };

        var panelContext = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0),
        };
        cmbQeContextPresets = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 140, Height = 28,
            Margin = new Padding(0, 4, 6, 4),
        };
        cmbQeContextPresets.Items.AddRange([
            "8,192 (8K)",
            "16,384 (16K)",
            "32,768 (32K)",
            "65,536 (64K)",
            "131,072 (128K)",
            "262,144 (256K)",
            "524,288 (512K)",
            "Custom..."
        ]);

        nudQeContext = new NumericUpDown
        {
            Minimum = 512, Maximum = 1_000_000, Value = 8192, Increment = 1024,
            ThousandsSeparator = true,
            Width = 100, Height = 28,
            Margin = new Padding(0, 4, 0, 4),
        };
        panelContext.Controls.AddRange([cmbQeContextPresets, nudQeContext]);

        nudQeNgl = new NumericUpDown
        {
            Minimum = 0, Maximum = 9999, Value = 999,
            Dock = DockStyle.Fill, Margin = new Padding(0, 4, 12, 4),
        };

        chkSaveChanges = new CheckBox
        {
            Text    = "Save changes to profile",
            Dock    = DockStyle.Fill,
            Checked = false,
        };

        // Flags
        var panelFlags = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0),
        };
        chkQeFlashAttn = new CheckBox { Text = "⚡ Flash Attn (-fa)", AutoSize = true, Checked = true, Margin = new Padding(0, 4, 12, 4) };
        chkQeNoMMap = new CheckBox { Text = "🚫 No mmap", AutoSize = true, Margin = new Padding(0, 4, 12, 4) };
        chkQeMLock = new CheckBox { Text = "🔒 Lock RAM", AutoSize = true, Margin = new Padding(0, 4, 12, 4) };
        chkQeEmbedding = new CheckBox { Text = "🧠 Embedding API", AutoSize = true, Margin = new Padding(0, 4, 12, 4) };
        chkQeDisableThinking = new CheckBox { Text = "🚫 Disable Thinking", AutoSize = true, Margin = new Padding(0, 4, 12, 4) };
        panelFlags.Controls.AddRange([chkQeFlashAttn, chkQeNoMMap, chkQeMLock, chkQeEmbedding, chkQeDisableThinking]);

        txtQeExtra = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            PlaceholderText = "--chat-template-kwargs ...",
            Margin = new Padding(0, 4, 0, 4),
        };

        configLayout.Controls.Add(lblQePort,      0, 0);
        configLayout.Controls.Add(nudQePort,      1, 0);
        configLayout.Controls.Add(lblQeContext,   2, 0);
        configLayout.Controls.Add(panelContext,   3, 0);

        configLayout.Controls.Add(lblQeNgl,       0, 1);
        configLayout.Controls.Add(nudQeNgl,       1, 1);
        configLayout.Controls.Add(chkSaveChanges, 2, 1);
        configLayout.SetColumnSpan(chkSaveChanges, 2);

        configLayout.Controls.Add(lblQeFlags,     0, 2);
        configLayout.Controls.Add(panelFlags,     1, 2);
        configLayout.SetColumnSpan(panelFlags, 3);

        configLayout.Controls.Add(new Label { Text = "Extra Flags:", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Dock = DockStyle.Fill }, 0, 3);
        configLayout.Controls.Add(txtQeExtra,     1, 3);
        configLayout.SetColumnSpan(txtQeExtra, 3);

        tabPageConfig.Controls.Add(configLayout);

        // ── Tab 2: Model & VRAM Inspector ─────────────────────────────────────
        var inspectorLayout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 4,
            RowCount    = 4,
        };
        inspectorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        inspectorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        inspectorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        inspectorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 4; i++)
            inspectorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

        static Label MakeInspLabel(string text) => new()
        {
            Text      = text,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        };

        lblSModel   = MakeInspLabel("Model File:");
        lblSPort    = MakeInspLabel("Config Port:");
        lblSContext = MakeInspLabel("Context Size:");
        lblSNgl     = MakeInspLabel("GPU Layers:");
        lblSArch    = MakeInspLabel("Architecture:");
        lblSQuant   = MakeInspLabel("Quantization:");

        lblSummaryModel   = MakeInspLabel("-");
        lblSummaryPort    = MakeInspLabel("-");
        lblSummaryContext = MakeInspLabel("-");
        lblSummaryNgl     = MakeInspLabel("-");
        lblSummaryArch    = MakeInspLabel("-");
        lblSummaryQuant   = MakeInspLabel("-");

        lblVramEstimate = new Label
        {
            Text      = "✓ ~0 GB VRAM",
            ForeColor = Color.DarkGreen,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
        };

        lblSystemRam = new Label
        {
            Text      = "RAM: -- / -- GB",
            ForeColor = SystemColors.HotTrack,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
        };

        inspectorLayout.Controls.Add(lblSModel,        0, 0);
        inspectorLayout.Controls.Add(lblSummaryModel,  1, 0);
        inspectorLayout.SetColumnSpan(lblSummaryModel, 3);

        inspectorLayout.Controls.Add(lblSArch,         0, 1);
        inspectorLayout.Controls.Add(lblSummaryArch,   1, 1);
        inspectorLayout.Controls.Add(lblSQuant,        2, 1);
        inspectorLayout.Controls.Add(lblSummaryQuant,  3, 1);

        inspectorLayout.Controls.Add(lblSContext,      0, 2);
        inspectorLayout.Controls.Add(lblSummaryContext,1, 2);
        inspectorLayout.Controls.Add(lblSNgl,          2, 2);
        inspectorLayout.Controls.Add(lblSummaryNgl,    3, 2);

        inspectorLayout.Controls.Add(MakeInspLabel("VRAM Estimate:"), 0, 3);
        inspectorLayout.Controls.Add(lblVramEstimate,                 1, 3);
        inspectorLayout.Controls.Add(MakeInspLabel("System Hardware:"),2, 3);
        inspectorLayout.Controls.Add(lblSystemRam,                    3, 3);

        tabPageInspector.Controls.Add(inspectorLayout);

        tabControlMain.TabPages.AddRange([tabPageConfig, tabPageInspector]);

        // ── Log Panel ─────────────────────────────────────────────────────────
        panelLog = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };

        rtbLog = new RichTextBox
        {
            Dock        = DockStyle.Fill,
            ReadOnly    = true,
            BackColor   = Color.FromArgb(30, 30, 30),
            ForeColor   = Color.FromArgb(220, 220, 220),
            Font        = FontHelper.MonospaceFont(9f),
            BorderStyle = BorderStyle.None,
        };

        btnClearLog = new Button
        {
            Text     = "Clear",
            Width    = 60,
            Height   = 24,
            Anchor   = AnchorStyles.Right | AnchorStyles.Top,
            Location = new Point(740, 4),
        };
        btnClearLog.Click += (_, _) => rtbLog.Clear();

        panelLog.Controls.AddRange([rtbLog, btnClearLog]);
        panelLog.SizeChanged += (_, _) => btnClearLog.Location = new Point(panelLog.Width - 68, 4);

        // ── System tray ───────────────────────────────────────────────────────
        trayMenu          = new ContextMenuStrip(components);
        trayMenuShow      = new ToolStripMenuItem("Show Window");
        trayMenuStartStop = new ToolStripMenuItem("Start Server");
        trayMenuSep       = new ToolStripSeparator();
        trayMenuExit      = new ToolStripMenuItem("Exit");

        trayMenuShow.Click      += trayMenuShow_Click;
        trayMenuStartStop.Click += trayMenuStartStop_Click;
        trayMenuExit.Click      += trayMenuExit_Click;

        trayMenu.Items.AddRange([trayMenuShow, trayMenuStartStop, trayMenuSep, trayMenuExit]);

        notifyIcon = new NotifyIcon(components)
        {
            Visible          = true,
            Text             = "LlamaLauncher — Idle",
            Icon             = IconHelper.IdleIcon,
            ContextMenuStrip = trayMenu,
        };
        notifyIcon.DoubleClick += notifyIcon_DoubleClick;

        // Assemble outer table
        outer.Controls.Add(panelHeader,    0, 0);
        outer.Controls.Add(panelProfile,   0, 1);
        outer.Controls.Add(panelActions,   0, 2);
        outer.Controls.Add(tabControlMain, 0, 3);
        outer.Controls.Add(panelLog,        0, 4);

        Controls.Add(outer);
        ResumeLayout(false);
    }
}

namespace LlamaLauncher.Forms;

partial class ManageModelsForm
{
    private System.ComponentModel.IContainer components = null!;

    private ListBox listProfiles = null!;
    private Button btnAdd = null!;
    private Button btnEdit = null!;
    private Button btnDelete = null!;
    private Button btnMoveUp = null!;
    private Button btnMoveDown = null!;
    private Button btnClose = null!;
    private Label lblTitle = null!;
    private Panel panelSide = null!;

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

        // ── Form ──────────────────────────────────────────────────────────────
        Text            = "Manage Model Profiles";
        Size            = new Size(500, 420);
        MinimumSize     = new Size(420, 340);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        Font            = new Font("Segoe UI", 9f);
        Padding         = new Padding(12);

        // ── Title ─────────────────────────────────────────────────────────────
        lblTitle = new Label
        {
            Text      = "Model Profiles",
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            Dock      = DockStyle.Top,
            Height    = 32,
            TextAlign = ContentAlignment.MiddleLeft,
        };

        // ── List box ──────────────────────────────────────────────────────────
        listProfiles = new ListBox
        {
            Dock          = DockStyle.Fill,
            IntegralHeight = false,
            ItemHeight    = 22,
            BorderStyle   = BorderStyle.FixedSingle,
        };
        listProfiles.SelectedIndexChanged += listProfiles_SelectedIndexChanged;
        listProfiles.DoubleClick          += listProfiles_DoubleClick;

        // ── Side button panel ─────────────────────────────────────────────────
        panelSide = new Panel { Dock = DockStyle.Right, Width = 100, Padding = new Padding(8, 0, 0, 0) };

        btnAdd = MakeSideButton("Add");
        btnAdd.Location = new Point(8, 0);
        btnAdd.Click += btnAdd_Click;

        btnEdit = MakeSideButton("Edit");
        btnEdit.Location = new Point(8, 38);
        btnEdit.Click += btnEdit_Click;

        btnDelete = MakeSideButton("Delete");
        btnDelete.Location = new Point(8, 76);
        btnDelete.Click += btnDelete_Click;

        btnMoveUp = MakeSideButton("▲ Up");
        btnMoveUp.Location = new Point(8, 130);
        btnMoveUp.Click += btnMoveUp_Click;

        btnMoveDown = MakeSideButton("▼ Down");
        btnMoveDown.Location = new Point(8, 168);
        btnMoveDown.Click += btnMoveDown_Click;

        panelSide.Controls.AddRange([btnAdd, btnEdit, btnDelete, btnMoveUp, btnMoveDown]);

        // ── Bottom close button ───────────────────────────────────────────────
        btnClose = new Button
        {
            Text   = "Close",
            Width  = 88,
            Height = 30,
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
        };
        btnClose.Click += btnClose_Click;
        AcceptButton = btnClose;
        CancelButton = btnClose;

        var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        panelBottom.Controls.Add(btnClose);
        // Position close button to the right
        panelBottom.SizeChanged += (_, _) =>
            btnClose.Location = new Point(panelBottom.Width - 96, 7);

        // ── Assemble ──────────────────────────────────────────────────────────
        Controls.Add(listProfiles);
        Controls.Add(panelSide);
        Controls.Add(panelBottom);
        Controls.Add(lblTitle);

        ResumeLayout(false);
    }

    private static Button MakeSideButton(string text) => new()
    {
        Text   = text,
        Width  = 84,
        Height = 30,
    };
}

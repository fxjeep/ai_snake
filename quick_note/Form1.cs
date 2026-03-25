using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuickNote;

public partial class Form1 : Form
{
    private NotifyIcon trayIcon = null!;
    private ToolStrip toolStrip = null!;
    private TabControl tabControl = null!;
    private FlowLayoutPanel? tabHeaderPanel;
    private TabPage? addTabPage;
    private System.Windows.Forms.Timer? autoSaveTimer;
    private TextBox? editTabBox;
    private TabPage? currentlyEditingTab;

    private ToolStripLabel searchLabel = null!;
    private ToolStripTextBox searchTextBox = null!;
    private ToolStripButton searchNextButton = null!;
    private string lastSearchText = "";

    public Form1()
    {
        InitializeComponent();
        this.Text = "Quick Note";
        
        // Window sizing
        this.Width = 1000;
        this.Height = 700;
        this.StartPosition = FormStartPosition.CenterScreen;

        this.Icon = GeneratePrettyIcon();

        InitializeUI();
        InitializeTrayIcon();
        
        // Setup AutoSave
        autoSaveTimer = new System.Windows.Forms.Timer { Interval = 500 };
        autoSaveTimer.Tick += AutoSaveTimer_Tick;

        // Load files or create default
        LoadExistingFiles();
    }

    private void InitializeUI()
    {
        // Setup Global Toolbar
        toolStrip = new ToolStrip();
        toolStrip.Dock = DockStyle.Top;
        toolStrip.GripStyle = ToolStripGripStyle.Hidden;
        toolStrip.Padding = new Padding(5);
        toolStrip.BackColor = Color.FromArgb(250, 250, 250);

        // Styling Buttons
        var btnBold = new ToolStripButton("B");
        btnBold.Font = new Font(toolStrip.Font, FontStyle.Bold);
        btnBold.Click += (s, e) => ToggleGlobalRtbStyle(FontStyle.Bold);

        var btnItalic = new ToolStripButton("I");
        btnItalic.Font = new Font(toolStrip.Font, FontStyle.Italic);
        btnItalic.Click += (s, e) => ToggleGlobalRtbStyle(FontStyle.Italic);

        var btnUnderline = new ToolStripButton("U");
        btnUnderline.Font = new Font(toolStrip.Font, FontStyle.Underline);
        btnUnderline.Click += (s, e) => ToggleGlobalRtbStyle(FontStyle.Underline);

        toolStrip.Items.Add(btnBold);
        toolStrip.Items.Add(btnItalic);
        toolStrip.Items.Add(btnUnderline);
        
        // Heading Buttons
        var btnH1 = new ToolStripButton("H1");
        btnH1.Font = new Font(toolStrip.Font, FontStyle.Bold);
        btnH1.Click += (s, e) => SetHeading(1);

        var btnH2 = new ToolStripButton("H2");
        btnH2.Font = new Font(toolStrip.Font, FontStyle.Bold);
        btnH2.Click += (s, e) => SetHeading(2);

        var btnH3 = new ToolStripButton("H3");
        btnH3.Font = new Font(toolStrip.Font, FontStyle.Bold);
        btnH3.Click += (s, e) => SetHeading(3);

        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(btnH1);
        toolStrip.Items.Add(btnH2);
        toolStrip.Items.Add(btnH3);

        // Normal Button
        var btnNormal = new ToolStripButton("Normal");
        btnNormal.Click += (s, e) => SetHeading(0);

        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(btnNormal);

        // Separator
        toolStrip.Items.Add(new ToolStripSeparator());

        // Exit Icon Button
        var btnExit = new ToolStripButton("Exit");
        btnExit.Image = GenerateExitIcon();
        btnExit.TextImageRelation = TextImageRelation.ImageBeforeText;
        btnExit.Click += (s, e) => Application.Exit();
        btnExit.Alignment = ToolStripItemAlignment.Right;
        
        toolStrip.Items.Add(btnExit);

        // Search Controls
        toolStrip.Items.Add(new ToolStripSeparator { Name = "searchSep", Visible = false });
        
        searchLabel = new ToolStripLabel("Find:");
        searchLabel.Visible = false;
        
        searchTextBox = new ToolStripTextBox();
        searchTextBox.Visible = false;
        searchTextBox.Width = 150;
        searchTextBox.TextChanged += (s, e) => PerformSearch(false);
        searchTextBox.KeyDown += (s, e) => {
            if (e.KeyCode == Keys.Enter) {
                e.Handled = true;
                PerformSearch(true);
            }
        };

        searchNextButton = new ToolStripButton(">");
        searchNextButton.Visible = false;
        searchNextButton.ToolTipText = "Find Next (Enter)";
        searchNextButton.Click += (s, e) => PerformSearch(true);

        toolStrip.Items.Add(searchLabel);
        toolStrip.Items.Add(searchTextBox);
        toolStrip.Items.Add(searchNextButton);

        // Setup TabHeaderPanel
        tabHeaderPanel = new FlowLayoutPanel();
        tabHeaderPanel.Dock = DockStyle.Top;
        tabHeaderPanel.Height = 40;
        tabHeaderPanel.BackColor = Color.FromArgb(240, 240, 240);
        tabHeaderPanel.FlowDirection = FlowDirection.LeftToRight;
        tabHeaderPanel.WrapContents = true;
        tabHeaderPanel.AutoScroll = false;
        tabHeaderPanel.Padding = new Padding(5, 5, 5, 5);
        tabHeaderPanel.Visible = true;
        
        this.Resize += (s, e) => UpdateHeaderHeight();

        // Setup TabControl
        tabControl = new TabControl();
        tabControl.Dock = DockStyle.Fill;
        tabControl.Appearance = TabAppearance.Buttons;
        tabControl.ItemSize = new Size(0, 1);
        tabControl.SizeMode = TabSizeMode.Fixed;
        tabControl.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        
        tabControl.SelectedIndexChanged += (s, e) => RefreshTabHeaders();
        
        // Setup the '+' tab (still needed for logic, though hidden)
        addTabPage = new TabPage("+");
        tabControl.TabPages.Add(addTabPage);
        
        // Add controls to form - Order matters for docking!
        // To have toolstrip at top, then tabheader, then tabcontrol fill:
        this.Controls.Add(tabControl);
        this.Controls.Add(tabHeaderPanel);
        this.Controls.Add(toolStrip);
        
        // Ensure the docked controls are in the right z-order
        toolStrip.BringToFront();
        tabHeaderPanel.BringToFront();

        this.KeyPreview = true;
        this.KeyDown += Form1_KeyDown;
    }

    private void Form1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F3)
        {
            e.Handled = true;
            ToggleSearchUI();
        }
    }

    private void ToggleSearchUI()
    {
        bool isVisible = !searchTextBox.Visible;
        searchLabel.Visible = isVisible;
        searchTextBox.Visible = isVisible;
        searchNextButton.Visible = isVisible;
        
        if (toolStrip.Items["searchSep"] is ToolStripSeparator sep)
            sep.Visible = isVisible;

        if (isVisible)
        {
            searchTextBox.Focus();
            if (!string.IsNullOrEmpty(searchTextBox.Text))
                PerformSearch(false);
        }
    }

    private void PerformSearch(bool next)
    {
        if (tabControl.SelectedTab == null || tabControl.SelectedTab == addTabPage) return;
        var rtb = GetRichTextBox(tabControl.SelectedTab);
        if (rtb == null) return;

        string searchText = searchTextBox.Text;
        if (string.IsNullOrEmpty(searchText)) return;

        int start = 0;
        if (next && searchText == lastSearchText)
        {
            start = rtb.SelectionStart + rtb.SelectionLength;
        }

        int index = rtb.Find(searchText, start, RichTextBoxFinds.None);
        if (index == -1 && start > 0)
        {
            // Wrap around
            index = rtb.Find(searchText, 0, RichTextBoxFinds.None);
        }

        if (index != -1)
        {
            rtb.Select(index, searchText.Length);
            rtb.ScrollToCaret();
            
            if (next) rtb.Focus();
            else searchTextBox.Focus();
        }

        lastSearchText = searchText;
    }

    private Image GenerateExitIcon()
    {
        var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var pen = new Pen(Color.IndianRed, 2.5f);
        // Draw an 'X'
        g.DrawLine(pen, 3, 3, 13, 13);
        g.DrawLine(pen, 3, 13, 13, 3);
        return bmp;
    }

    private void ToggleGlobalRtbStyle(FontStyle style)
    {
        if (tabControl.SelectedTab == null || tabControl.SelectedTab == addTabPage) return;
        var rtb = GetRichTextBox(tabControl.SelectedTab);
        if (rtb == null) return;
        
        Font newFont;
        if (rtb.SelectionFont != null)
        {
            FontStyle currentStyle = rtb.SelectionFont.Style;
            FontStyle newStyle = currentStyle ^ style;
            newFont = new Font(rtb.SelectionFont.FontFamily, rtb.SelectionFont.Size, newStyle);
        }
        else
        {
            newFont = new Font(rtb.Font.FontFamily, rtb.Font.Size, style);
        }
        
        rtb.SelectionFont = newFont;
        ScheduleSave(rtb);
    }

    private void SetHeading(int headingLevel)
    {
        if (tabControl.SelectedTab == null || tabControl.SelectedTab == addTabPage) return;
        var rtb = GetRichTextBox(tabControl.SelectedTab);
        if (rtb == null) return;
        
        float newSize = 12f; // Default Normal
        bool setBold = false;

        switch (headingLevel)
        {
            case 1: newSize = 26f; setBold = true; break;
            case 2: newSize = 20f; setBold = true; break;
            case 3: newSize = 16f; setBold = true; break;
            case 0: newSize = 12f; setBold = false; break;
        }

        if (rtb.SelectionFont != null && rtb.SelectionFont.Size == newSize)
        {
            newSize = 12f;
            setBold = false;
        }

        var baseFont = rtb.SelectionFont ?? rtb.Font;
        FontStyle newStyle = baseFont.Style;

        if (setBold)
            newStyle |= FontStyle.Bold;
        else
            newStyle &= ~FontStyle.Bold;

        rtb.SelectionFont = new Font(baseFont.FontFamily, newSize, newStyle);
        ScheduleSave(rtb);
    }

    private void CreateTabEditor(TabPage tabPage, string filePath)
    {
        var rtb = new RichTextBox();
        rtb.Dock = DockStyle.Fill;
        rtb.BorderStyle = BorderStyle.None;
        rtb.Font = new Font("Segoe UI", 12);
        rtb.AcceptsTab = true;
        rtb.HideSelection = false;
        
        if (File.Exists(filePath))
        {
            try { rtb.LoadFile(filePath, RichTextBoxStreamType.RichText); }
            catch { rtb.Text = File.ReadAllText(filePath); }
        }
        else
        {
            rtb.SaveFile(filePath, RichTextBoxStreamType.RichText);
        }

        rtb.TextChanged += (s, e) => ScheduleSave(rtb);
        
        tabPage.Controls.Add(rtb);
    }

    // Drawing methods (OBSOLETE - replaced by custom headers)
    private void UpdateHeaderHeight()
    {
        if (tabHeaderPanel == null) return;
        int preferredHeight = tabHeaderPanel.GetPreferredSize(new Size(tabHeaderPanel.Width, 0)).Height;
        tabHeaderPanel.Height = Math.Max(40, preferredHeight);
    }

    private void RefreshTabHeaders()
    {
        if (tabHeaderPanel == null) return;
        tabHeaderPanel.SuspendLayout();
        tabHeaderPanel.Controls.Clear();

        foreach (TabPage page in tabControl.TabPages)
        {
            if (page == addTabPage) continue;
            var tabBtn = CreateTabButton(page);
            tabBtn.Visible = true;
            tabHeaderPanel.Controls.Add(tabBtn);
        }

        // The "+" button to add new tabs
        var btnAdd = new Panel();
        btnAdd.Size = new Size(30,30);
        btnAdd.Margin = new Padding(5, 5, 5, 5);
        btnAdd.Cursor = Cursors.Hand;
        btnAdd.BackColor = Color.Transparent;
        btnAdd.Paint += (s, e) => {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(100, 100, 100), 2);
            e.Graphics.DrawLine(pen, 15, 8, 15, 22);
            e.Graphics.DrawLine(pen, 8, 15, 22, 15);
        };
        btnAdd.Click += (s, e) => AddNewTab();
        btnAdd.MouseEnter += (s, e) => btnAdd.BackColor = Color.FromArgb(220, 220, 220);
        btnAdd.MouseLeave += (s, e) => btnAdd.BackColor = Color.Transparent;

        btnAdd.Visible = true;
        tabHeaderPanel.Controls.Add(btnAdd);
        
        tabHeaderPanel.ResumeLayout();
        UpdateHeaderHeight();
    }

    private Control CreateTabButton(TabPage page)
    {
        bool isSelected = tabControl.SelectedTab == page;
        
        var panel = new Panel();
        panel.Margin = new Padding(2, 2, 2, 2);
        panel.Cursor = Cursors.Hand;
        
        using (var g = panel.CreateGraphics())
        {
            var textSize = g.MeasureString(page.Text, tabControl.Font);
            panel.Width = (int)textSize.Width + 45; 
            panel.Height = Math.Max(35, (int)textSize.Height + 10);
        }

        panel.BackColor = isSelected ? Color.White : Color.Transparent;
        
        // Bottom border for selected tab
        panel.Paint += (s, e) => {
            if (isSelected) {
                using var p = new Pen(Color.FromArgb(0, 120, 215), 3);
                e.Graphics.DrawLine(p, 0, panel.Height - 1, panel.Width, panel.Height - 1);
            }
        };

        var lbl = new Label();
        lbl.Text = page.Text;
        lbl.AutoSize = false;
        lbl.Dock = DockStyle.Fill;
        lbl.TextAlign = ContentAlignment.MiddleLeft;
        lbl.Padding = new Padding(10, 0, 0, 0);
        lbl.Font = new Font(tabControl.Font, isSelected ? FontStyle.Bold : FontStyle.Regular);
        lbl.ForeColor = isSelected ? Color.Black : Color.FromArgb(80, 80, 80);
        
        lbl.Click += (s, e) => tabControl.SelectedTab = page;
        lbl.DoubleClick += (s, e) => {
            var rect = panel.Bounds;
            StartEditingTab(page, rect);
        };

        var btnClose = new Label();
        btnClose.Text = "×";
        btnClose.AutoSize = false;
        btnClose.Width = 25;
        btnClose.Dock = DockStyle.Right;
        btnClose.TextAlign = ContentAlignment.MiddleCenter;
        btnClose.Font = new Font("Arial", 11, FontStyle.Bold);
        btnClose.ForeColor = Color.FromArgb(150, 150, 150);
        btnClose.Cursor = Cursors.Hand;
        
        btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.IndianRed;
        btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.FromArgb(150, 150, 150);
        btnClose.Click += (s, e) => CloseCurrentTab(page);

        panel.Controls.Add(lbl);
        panel.Controls.Add(btnClose);

        // Hover effects
        lbl.MouseEnter += (s, e) => { if (!isSelected) panel.BackColor = Color.FromArgb(225, 225, 225); };
        lbl.MouseLeave += (s, e) => { if (!isSelected) panel.BackColor = Color.Transparent; };

        return panel;
    }

    private void TabControl_Selecting(object? sender, TabControlCancelEventArgs e)
    {
        if (e.TabPage == addTabPage)
        {
            e.Cancel = true; 
            AddNewTab();
        }
    }

    private void StartEditingTab(TabPage tab, Rectangle rect)
    {
        if (editTabBox == null)
        {
            editTabBox = new TextBox();
            editTabBox.Parent = this;
            editTabBox.Font = tabControl.Font;
            editTabBox.KeyPress += EditTabBox_KeyPress;
            editTabBox.LostFocus += EditTabBox_LostFocus;
        }

        currentlyEditingTab = tab;
        editTabBox.Text = tab.Text;
        
        int x = tabHeaderPanel != null ? tabHeaderPanel.Left : tabControl.Left;
        int y = tabHeaderPanel != null ? tabHeaderPanel.Top : tabControl.Top;
        
        editTabBox.Bounds = new Rectangle(x + rect.X + 2, y + rect.Y + 2, rect.Width - 25, rect.Height - 4);
        editTabBox.Visible = true;
        editTabBox.BringToFront();
        editTabBox.Focus();
        editTabBox.SelectAll();
    }

    private void EditTabBox_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
        {
            e.Handled = true;
            CommitTabEdit();
        }
        else if (e.KeyChar == (char)Keys.Escape)
        {
            e.Handled = true;
            CancelTabEdit();
        }
    }

    private void EditTabBox_LostFocus(object? sender, EventArgs e)
    {
        CommitTabEdit();
    }

    private void CancelTabEdit()
    {
        if (editTabBox != null) editTabBox.Visible = false;
        currentlyEditingTab = null;
    }

    private void CommitTabEdit()
    {
        if (editTabBox != null && editTabBox.Visible && currentlyEditingTab != null)
        {
            string newText = editTabBox.Text.Trim();
            if (!string.IsNullOrEmpty(newText) && newText != currentlyEditingTab.Text)
            {
                if (currentlyEditingTab.Tag is string oldFilePath && File.Exists(oldFilePath))
                {
                    try
                    {
                        string? directory = Path.GetDirectoryName(oldFilePath);
                        if (directory == null) return;
                        string isoTime = DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");
                        string safeNewText = string.Join("_", newText.Split(Path.GetInvalidFileNameChars()));
                        string newFileName = $"{safeNewText}_{isoTime}.rtf";
                        string newFilePath = Path.Combine(directory, newFileName);
                        
                        File.Move(oldFilePath, newFilePath);
                        currentlyEditingTab.Tag = newFilePath;
                        currentlyEditingTab.Text = newText;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to rename file: " + ex.Message, "Error");
                    }
                }
            }
            editTabBox.Visible = false;
        }
        currentlyEditingTab = null;
        RefreshTabHeaders();
    }

    private void InitializeTrayIcon()
    {
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

        trayIcon = new NotifyIcon()
        {
            Icon = this.Icon,
            Text = "Quick Note",
            ContextMenuStrip = contextMenu,
            Visible = true
        };

        trayIcon.DoubleClick += TrayIcon_DoubleClick;
        this.FormClosing += Form1_FormClosing;
        
        Application.ApplicationExit += (s, e) => trayIcon.Dispose();
    }

    private void LoadExistingFiles()
    {
        var files = Directory.GetFiles(Application.StartupPath, "*.rtf");
        if (files.Length == 0)
        {
            AddNewTab();
            return;
        }

        foreach (var file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string tabHeader = fileName;
            int underscoreIdx = fileName.LastIndexOf('_');
            if (underscoreIdx > 0)
            {
                tabHeader = fileName.Substring(0, underscoreIdx);
            }
            
            AddNewTabExisting(file, tabHeader);
        }
        RefreshTabHeaders();
    }

    private void AddNewTabExisting(string filePath, string tabHeader)
    {
        int insertIndex = tabControl.TabPages.Count - 1;
        if (insertIndex < 0) insertIndex = 0;

        var tabPage = new TabPage(tabHeader);
        tabPage.Tag = filePath;
        
        CreateTabEditor(tabPage, filePath);

        tabControl.TabPages.Insert(insertIndex, tabPage);
        RefreshTabHeaders();

        if (tabControl.TabPages.Count == 2) 
        {
            tabControl.SelectedTab = tabPage;
        }
    }

    private void AddNewTab()
    {
        int insertIndex = tabControl.TabPages.Count - 1;
        if (insertIndex < 0) insertIndex = 0;

        int noteNum = 1;
        foreach (TabPage page in tabControl.TabPages)
        {
            if (page.Tag is string) 
            {
                if (page.Text.StartsWith("Note "))
                {
                    if (int.TryParse(page.Text.Substring(5), out int num) && num >= noteNum)
                        noteNum = num + 1;
                }
            }
        }

        string tabHeader = "Note " + noteNum;
        string isoTime = DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");
        string fileName = $"{tabHeader}_{isoTime}.rtf";
        string filePath = Path.Combine(Application.StartupPath, fileName);
        
        var tabPage = new TabPage(tabHeader);
        tabPage.Tag = filePath;
        
        CreateTabEditor(tabPage, filePath);
        
        tabControl.TabPages.Insert(insertIndex, tabPage);
        RefreshTabHeaders();
        tabControl.SelectedTab = tabPage;
    }

    private void ScheduleSave(RichTextBox rtb)
    {
        rtb.Modified = true;
        autoSaveTimer?.Stop();
        autoSaveTimer?.Start();
    }

    private RichTextBox? GetRichTextBox(TabPage? page)
    {
        if (page == null) return null;
        foreach (Control c in page.Controls)
        {
            if (c is RichTextBox rtb) return rtb;
        }
        return null;
    }

    private void AutoSaveTimer_Tick(object? sender, EventArgs e)
    {
        autoSaveTimer?.Stop();
        foreach (TabPage page in tabControl.TabPages)
        {
            if (page == addTabPage || page.Tag is not string filePath) continue;
            var rtb = GetRichTextBox(page);
            if (rtb != null && rtb.Modified)
            {
                rtb.SaveFile(filePath, RichTextBoxStreamType.RichText);
                rtb.Modified = false;
            }
        }
    }

    private void CloseCurrentTab(TabPage tab)
    {
        if (tabControl.TabPages.Count <= 2) AddNewTab();

        var rtb = GetRichTextBox(tab);
        if (rtb != null && rtb.Modified && tab.Tag is string path)
        {
            rtb.SaveFile(path, RichTextBoxStreamType.RichText);
        }
        
        tabControl.TabPages.Remove(tab);
        RefreshTabHeaders();
        tab.Dispose();
    }

    private void TrayIcon_DoubleClick(object? sender, EventArgs e)
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            this.Hide();
        }
        else
        {
            // Force save on actual exit
            AutoSaveTimer_Tick(null, EventArgs.Empty);
        }
    }

    private Icon GeneratePrettyIcon()
    {
        int size = 128;
        using var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = new Rectangle(20, 20, 88, 100);
        using var bgBrush = new SolidBrush(Color.FromArgb(250, 240, 210)); 
        g.FillPath(bgBrush, GetRoundedRectPath(rect, 8));

        using var linePen = new Pen(Color.FromArgb(220, 200, 160), 4);
        g.DrawLine(linePen, 35, 45, 93, 45);
        g.DrawLine(linePen, 35, 65, 93, 65);
        g.DrawLine(linePen, 35, 85, 75, 85);

        Point[] foldedCorner = { new Point(88, 20), new Point(108, 40), new Point(88, 40) };
        using var foldBrush = new SolidBrush(Color.FromArgb(230, 210, 170));
        g.FillPolygon(foldBrush, foldedCorner);
        
        using var pinShadow = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
        g.FillEllipse(pinShadow, 60, 8, 20, 20); 
        
        using var pinBrush = new SolidBrush(Color.FromArgb(230, 60, 60)); 
        g.FillEllipse(pinBrush, 55, 3, 22, 22); 
        using var highlight = new SolidBrush(Color.FromArgb(100, 255, 255, 255));
        g.FillEllipse(highlight, 58, 6, 8, 8); 
        
        IntPtr hIcon = bmp.GetHicon();
        return Icon.FromHandle(hIcon);
    }

    private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
        path.AddArc(bounds.Right - radius, bounds.Y, radius, radius, 270, 90);
        path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }
}

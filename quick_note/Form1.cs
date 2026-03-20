using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuickNote;

public partial class Form1 : Form
{
    private NotifyIcon trayIcon;
    private TabControl tabControl;
    private TabPage addTabPage;
    private System.Windows.Forms.Timer autoSaveTimer;
    private TextBox editTabBox;
    private TabPage currentlyEditingTab;

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
        var toolStrip = new ToolStrip();
        toolStrip.GripStyle = ToolStripGripStyle.Hidden;
        toolStrip.Padding = new Padding(5);

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

        // Setup TabControl
        tabControl = new TabControl();
        tabControl.Dock = DockStyle.Fill;
        tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabControl.Padding = new Point(16, 6); 
        tabControl.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        
        tabControl.DrawItem += TabControl_DrawItem;
        tabControl.MouseDown += TabControl_MouseDown;
        tabControl.Selecting += TabControl_Selecting;
        tabControl.DoubleClick += TabControl_DoubleClick;
        
        // Setup the '+' tab
        addTabPage = new TabPage("+");
        tabControl.TabPages.Add(addTabPage);
        
        // Add controls to form
        this.Controls.Add(toolStrip);
        this.Controls.Add(tabControl);
        tabControl.BringToFront();
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

    // Drawing methods
    private void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not TabControl tabCtrl) return;
        if (e.Index < 0 || e.Index >= tabCtrl.TabPages.Count) return;
        
        var tabPage = tabCtrl.TabPages[e.Index];
        var tabRect = tabCtrl.GetTabRect(e.Index);
        
        bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        Brush bgBrush = isSelected ? SystemBrushes.ControlLightLight : SystemBrushes.Control;
        e.Graphics.FillRectangle(bgBrush, tabRect);
        
        if (isSelected)
        {
            e.Graphics.DrawLine(SystemPens.ControlDark, tabRect.Left, tabRect.Top, tabRect.Right, tabRect.Top);
            e.Graphics.DrawLine(SystemPens.ControlDark, tabRect.Left, tabRect.Top, tabRect.Left, tabRect.Bottom);
            e.Graphics.DrawLine(SystemPens.ControlDark, tabRect.Right - 1, tabRect.Top, tabRect.Right - 1, tabRect.Bottom);
        }

        var textBrush = new SolidBrush(Color.Black);
        
        if (tabPage == addTabPage)
        {
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            e.Graphics.DrawString(tabPage.Text, new Font(tabCtrl.Font.FontFamily, tabCtrl.Font.Size + 2, FontStyle.Bold), textBrush, tabRect, stringFormat);
        }
        else
        {
            var format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };
            var textRect = new Rectangle(tabRect.Left + 5, tabRect.Top, tabRect.Width - 25, tabRect.Height);
            e.Graphics.DrawString(tabPage.Text, tabCtrl.Font, textBrush, textRect, format);
            
            var closeFont = new Font("Arial", 9, FontStyle.Bold);
            var closeRect = GetCloseButtonRect(e.Index);
            
            var xFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            e.Graphics.DrawString("x", closeFont, Brushes.Gray, closeRect, xFormat);
        }
    }
    
    private Rectangle GetCloseButtonRect(int tabIndex)
    {
        var tabRect = tabControl.GetTabRect(tabIndex);
        return new Rectangle(tabRect.Right - 18, tabRect.Top + (tabRect.Height - 14) / 2, 14, 14);
    }

    private void TabControl_MouseDown(object? sender, MouseEventArgs e)
    {
        for (int i = 0; i < tabControl.TabPages.Count; i++)
        {
            var tabPage = tabControl.TabPages[i];
            if (tabPage == addTabPage) continue;
            
            var closeRect = GetCloseButtonRect(i);
            if (closeRect.Contains(e.Location))
            {
                CloseCurrentTab(tabPage);
                return;
            }
        }
    }

    private void TabControl_Selecting(object? sender, TabControlCancelEventArgs e)
    {
        if (e.TabPage == addTabPage)
        {
            e.Cancel = true; 
            this.BeginInvoke(new Action(() => AddNewTab()));
        }
    }

    private void TabControl_DoubleClick(object? sender, EventArgs e)
    {
        var cursor = tabControl.PointToClient(Cursor.Position);
        for (int i = 0; i < tabControl.TabPages.Count; i++)
        {
            if (tabControl.TabPages[i] == addTabPage) continue;
            
            var rect = tabControl.GetTabRect(i);
            if (rect.Contains(cursor))
            {
                StartEditingTab(tabControl.TabPages[i], rect);
                break;
            }
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
        
        editTabBox.Bounds = new Rectangle(tabControl.Left + rect.X + 2, tabControl.Top + rect.Y + 2, rect.Width - 25, rect.Height - 4);
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
                        string directory = Path.GetDirectoryName(oldFilePath);
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
        tabControl.Invalidate();
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
    }

    private void AddNewTabExisting(string filePath, string tabHeader)
    {
        int insertIndex = tabControl.TabPages.Count - 1;
        if (insertIndex < 0) insertIndex = 0;

        var tabPage = new TabPage(tabHeader);
        tabPage.Tag = filePath;
        
        CreateTabEditor(tabPage, filePath);

        tabControl.TabPages.Insert(insertIndex, tabPage);

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
        tabControl.SelectedTab = tabPage;
    }

    private void ScheduleSave(RichTextBox rtb)
    {
        rtb.Modified = true;
        autoSaveTimer.Stop();
        autoSaveTimer.Start();
    }

    private RichTextBox GetRichTextBox(TabPage page)
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
        autoSaveTimer.Stop();
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

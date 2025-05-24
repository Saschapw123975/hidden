using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileManager
{
    public partial class FileManagerForm : Form
    {
        private string hiddenFolderPath;
        private ListView fileListView;
        private Panel sidePanel;
        private Panel mainPanel;
        private Label titleLabel;
        private System.Windows.Forms.Timer animationTimer;
        private float glowAngle = 0;
        private int activeFiles = 0;
        private bool isDragging = false;
        private Point dragOffset;
        private const int RESIZE_BORDER = 6;
        private int cornerRadius = 18;
        private bool isFullscreen = false;
        private Rectangle prevBounds;
        private FormWindowState prevState;

        public FileManagerForm()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HiddenFileManager"
            );
            hiddenFolderPath = appDataPath;
            Directory.CreateDirectory(hiddenFolderPath);
            File.SetAttributes(hiddenFolderPath, FileAttributes.Hidden);

            InitializeComponents();
            SetupAnimations();
            RefreshFileList();
        }

        private void SetupAnimations()
        {
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 16; // ~60 FPS for smoother animations
            animationTimer.Tick += (s, e) => {
                glowAngle += 2;
                if (glowAngle >= 360) glowAngle = 0;
                this.Invalidate();
            };
            animationTimer.Start();
        }

        private void InitializeComponents()
        {
            int windowMarginLeft = 24; // More space on the left
            int windowMarginTop = 0;   // No margin at the top
            int windowMarginRight = 8;
            int windowMarginBottom = 8;
            this.Size = new Size(840, 500);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(18, 18, 20); // Blackish
            this.DoubleBuffered = true;
            this.MinimumSize = new Size(400, 300);

            // Rounded corners for the main window with custom margins
            this.Region = new Region(GetRoundedRectPath(new Rectangle(windowMarginLeft, windowMarginTop, this.Width - windowMarginLeft - windowMarginRight, this.Height - windowMarginTop - windowMarginBottom), cornerRadius));
            this.Padding = new Padding(windowMarginLeft, windowMarginTop, windowMarginRight, windowMarginBottom);
            this.Resize += (s, e) =>
            {
                this.Region = new Region(GetRoundedRectPath(new Rectangle(windowMarginLeft, windowMarginTop, this.Width - windowMarginLeft - windowMarginRight, this.Height - windowMarginTop - windowMarginBottom), cornerRadius));
            };

            // Title bar
            Panel titleBar = new Panel
            {
                Height = 36,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(28, 28, 32), // Dark grey
                Padding = new Padding(0, 0, 0, 0)
            };
            titleBar.Region = new Region(GetRoundedRectPath(new Rectangle(0, 0, this.Width, titleBar.Height + 2), cornerRadius, true, false, false, true));

            // Window controls
            Panel controlBox = new Panel
            {
                Width = 120,
                Height = 36,
                Dock = DockStyle.Right,
                BackColor = Color.Transparent
            };

            // Minimize button
            Button minimizeBtn = new Button
            {
                Size = new Size(30, 36),
                FlatStyle = FlatStyle.Flat,
                Text = "─",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                Dock = DockStyle.Left
            };
            minimizeBtn.FlatAppearance.BorderSize = 0;
            minimizeBtn.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            // Fullscreen button
            Button fullscreenBtn = new Button
            {
                Size = new Size(30, 36),
                FlatStyle = FlatStyle.Flat,
                Text = "⬜",
                Font = new Font("Segoe UI Symbol", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                Dock = DockStyle.Left
            };
            fullscreenBtn.FlatAppearance.BorderSize = 0;
            fullscreenBtn.Click += (s, e) => ToggleFullscreen();

            // Close button
            Button closeBtn = new Button
            {
                Size = new Size(30, 36),
                FlatStyle = FlatStyle.Flat,
                Text = "×",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(180, 180, 180),
                BackColor = Color.Transparent,
                Dock = DockStyle.Right
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) => Application.Exit();

            // Title
            titleLabel = new Label
            {
                Text = "CYRMSON",
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Location = new Point(18, 7),
                AutoSize = true
            };

            // Stats panel
            Panel statsPanel = new Panel
            {
                Height = 54,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(24, 24, 26), // Slightly lighter grey
                Padding = new Padding(10)
            };
            statsPanel.Region = new Region(GetRoundedRectPath(new Rectangle(0, 0, this.Width, statsPanel.Height + 2), cornerRadius, false, false, true, true));

            // File counter
            Label fileCount = new Label
            {
                Text = "0",
                ForeColor = Color.FromArgb(180, 180, 180),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 13),
                AutoSize = true
            };

            Label fileLabel = new Label
            {
                Text = "Protected Files",
                ForeColor = Color.FromArgb(120, 120, 120),
                Font = new Font("Segoe UI", 9),
                Location = new Point(55, 20),
                AutoSize = true
            };

            // Status indicator
            Label statusLabel = new Label
            {
                Text = "SECURE",
                ForeColor = Color.FromArgb(80, 200, 180),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(statsPanel.Width - 100, 20),
                AutoSize = true
            };

            // Main content panel
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(22, 22, 24),
                Padding = new Padding(10)
            };
            contentPanel.Region = new Region(GetRoundedRectPath(new Rectangle(0, 0, this.Width, this.Height - titleBar.Height - statsPanel.Height), cornerRadius, false, true, true, true));

            // File list
            fileListView = new ListView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 32),
                ForeColor = Color.FromArgb(220, 220, 220),
                BorderStyle = BorderStyle.None,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                Font = new Font("Segoe UI", 9)
            };
            fileListView.Columns.Add("File Name", 250);
            fileListView.Columns.Add("Original Location", 350);
            fileListView.Columns.Add("Date Protected", 150);

            // Button panel
            Panel buttonPanel = new Panel
            {
                Height = 48,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(24, 24, 26),
                Padding = new Padding(10)
            };
            buttonPanel.Region = new Region(GetRoundedRectPath(new Rectangle(0, 0, this.Width, buttonPanel.Height + 2), cornerRadius, false, true, false, false));

            // Action buttons
            Button protectBtn = CreateActionButton("PROTECT FILE", buttonPanel.Width - 260);
            Button restoreBtn = CreateActionButton("RESTORE FILE", buttonPanel.Width - 130);

            // Add controls
            controlBox.Controls.Add(minimizeBtn);
            controlBox.Controls.Add(fullscreenBtn);
            controlBox.Controls.Add(closeBtn);

            titleBar.Controls.Add(controlBox);
            titleBar.Controls.Add(titleLabel);

            statsPanel.Controls.Add(fileCount);
            statsPanel.Controls.Add(fileLabel);
            statsPanel.Controls.Add(statusLabel);

            buttonPanel.Controls.Add(protectBtn);
            buttonPanel.Controls.Add(restoreBtn);

            contentPanel.Controls.Add(fileListView);

            this.Controls.Add(contentPanel);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(statsPanel);
            this.Controls.Add(titleBar);

            // Events
            protectBtn.Click += (s, e) => ProtectFile();
            restoreBtn.Click += (s, e) => RestoreFile();

            // Window dragging and resizing
            titleBar.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    isDragging = true;
                    dragOffset = new Point(e.X, e.Y);
                }
            };
            titleBar.MouseMove += (s, e) => {
                if (isDragging)
                {
                    Point newLocation = PointToScreen(new Point(e.X, e.Y));
                    newLocation.Offset(-dragOffset.X, -dragOffset.Y);
                    Location = newLocation;
                }
            };
            titleBar.MouseUp += (s, e) => {
                isDragging = false;
            };

            // Hover effects
            minimizeBtn.MouseEnter += (s, e) => minimizeBtn.BackColor = Color.FromArgb(40, 40, 44);
            minimizeBtn.MouseLeave += (s, e) => minimizeBtn.BackColor = Color.Transparent;
            closeBtn.MouseEnter += (s, e) => closeBtn.BackColor = Color.FromArgb(80, 20, 20);
            closeBtn.MouseLeave += (s, e) => closeBtn.BackColor = Color.Transparent;
        }

        private Button CreateActionButton(string text, int x)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(120, 32),
                Location = new Point(x, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 44),
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Region = new Region(GetRoundedRectPath(new Rectangle(0, 0, btn.Width, btn.Height), 12));
            btn.MouseEnter += (s, e) => {
                btn.BackColor = Color.FromArgb(60, 60, 64);
            };
            btn.MouseLeave += (s, e) => {
                btn.BackColor = Color.FromArgb(40, 40, 44);
            };
            return btn;
        }

        private async void ProtectFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = "Select Files to Protect";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filePath in dialog.FileNames)
                    {
                        try
                        {
                            string fileName = Path.GetFileName(filePath);
                            string destinationPath = Path.Combine(hiddenFolderPath, fileName);
                            string metadataPath = destinationPath + ".metadata";
                            File.WriteAllText(metadataPath, filePath);
                            File.Move(filePath, destinationPath);
                            activeFiles++;
                            await AnimateNewFile();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error protecting file: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    RefreshFileList();
                }
            }
        }

        private async Task AnimateNewFile()
        {
            const int steps = 10;
            Color originalColor = fileListView.BackColor;
            Color targetColor = Color.FromArgb(50, 50, 54);
            for (int i = 0; i < steps; i++)
            {
                float progress = (float)i / steps;
                int r = (int)((targetColor.R - originalColor.R) * progress + originalColor.R);
                int g = (int)((targetColor.G - originalColor.G) * progress + originalColor.G);
                int b = (int)((targetColor.B - originalColor.B) * progress + originalColor.B);
                fileListView.BackColor = Color.FromArgb(r, g, b);
                await Task.Delay(20);
            }
            await Task.Delay(100);
            for (int i = steps - 1; i >= 0; i--)
            {
                float progress = (float)i / steps;
                int r = (int)((targetColor.R - originalColor.R) * progress + originalColor.R);
                int g = (int)((targetColor.G - originalColor.G) * progress + originalColor.G);
                int b = (int)((targetColor.B - originalColor.B) * progress + originalColor.B);
                fileListView.BackColor = Color.FromArgb(r, g, b);
                await Task.Delay(20);
            }
        }

        private void RestoreFile()
        {
            if (fileListView.SelectedItems.Count == 0) return;
            foreach (ListViewItem item in fileListView.SelectedItems)
            {
                try
                {
                    string fileName = item.Text;
                    string hiddenFilePath = Path.Combine(hiddenFolderPath, fileName);
                    string metadataPath = hiddenFilePath + ".metadata";
                    if (!File.Exists(metadataPath))
                        throw new FileNotFoundException("File metadata not found.");
                    string originalPath = File.ReadAllText(metadataPath);
                    string originalDir = Path.GetDirectoryName(originalPath);
                    if (!Directory.Exists(originalDir))
                        Directory.CreateDirectory(originalDir);
                    File.Move(hiddenFilePath, originalPath);
                    File.Delete(metadataPath);
                    activeFiles--;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error restoring file: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            RefreshFileList();
        }

        private void RefreshFileList()
        {
            fileListView.Items.Clear();
            string[] files = Directory.GetFiles(hiddenFolderPath);
            foreach (string file in files)
            {
                if (!file.EndsWith(".metadata"))
                {
                    string fileName = Path.GetFileName(file);
                    string metadataPath = file + ".metadata";
                    string originalPath = File.Exists(metadataPath)
                        ? File.ReadAllText(metadataPath)
                        : "Unknown location";
                    ListViewItem item = new ListViewItem(fileName);
                    item.SubItems.Add(originalPath);
                    item.SubItems.Add(File.GetCreationTime(file).ToString("g"));
                    fileListView.Items.Add(item);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Draw rounded window border with subtle glow
            using (GraphicsPath borderPath = GetRoundedRectPath(new Rectangle(0, 0, Width - 1, Height - 1), cornerRadius))
            {
                using (Pen borderPen = new Pen(Color.FromArgb(60, 60, 64), 2))
                {
                    e.Graphics.DrawPath(borderPen, borderPath);
                }
                using (Pen glowPen = new Pen(Color.FromArgb(40, 80, 80, 80), 8))
                {
                    e.Graphics.DrawPath(glowPen, borderPath);
                }
            }
            // Draw animated scanlines (very subtle)
            using (Pen scanline = new Pen(Color.FromArgb(18, 18, 20), 1))
            {
                for (int i = 0; i < 2; i++)
                {
                    float offset = (glowAngle + (i * 180)) % 360;
                    int y = (int)(this.Height * (Math.Sin(offset * Math.PI / 180) + 1) / 2);
                    e.Graphics.DrawLine(scanline, 0, y, this.Width, y);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            animationTimer?.Stop();
            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTCLIENT = 1;
            const int HTLEFT = 12;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 20;
            const int HTBOTTOMRIGHT = 21;
            if (m.Msg == WM_NCHITTEST)
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if (pos.X <= RESIZE_BORDER)
                {
                    if (pos.Y <= RESIZE_BORDER)
                        m.Result = (IntPtr)HTTOPLEFT;
                    else if (pos.Y >= ClientSize.Height - RESIZE_BORDER)
                        m.Result = (IntPtr)HTBOTTOMLEFT;
                    else
                        m.Result = (IntPtr)HTLEFT;
                }
                else if (pos.X >= ClientSize.Width - RESIZE_BORDER)
                {
                    if (pos.Y <= RESIZE_BORDER)
                        m.Result = (IntPtr)HTTOPRIGHT;
                    else if (pos.Y >= ClientSize.Height - RESIZE_BORDER)
                        m.Result = (IntPtr)HTBOTTOMRIGHT;
                    else
                        m.Result = (IntPtr)HTRIGHT;
                }
                else if (pos.Y <= RESIZE_BORDER)
                {
                    m.Result = (IntPtr)HTTOP;
                }
                else if (pos.Y >= ClientSize.Height - RESIZE_BORDER)
                {
                    m.Result = (IntPtr)HTBOTTOM;
                }
                else
                {
                    m.Result = (IntPtr)HTCLIENT;
                }
                return;
            }
            base.WndProc(ref m);
        }

        // Helper for rounded rectangles
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius, bool topLeft = true, bool topRight = true, bool bottomRight = true, bool bottomLeft = true)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            if (topLeft)
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            else
                path.AddLine(rect.X, rect.Y, rect.X, rect.Y);
            if (topRight)
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            else
                path.AddLine(rect.Right, rect.Y, rect.Right, rect.Y);
            if (bottomRight)
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            else
                path.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom);
            if (bottomLeft)
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            else
                path.AddLine(rect.X, rect.Bottom, rect.X, rect.Bottom);
            path.CloseFigure();
            return path;
        }

        private void ToggleFullscreen()
        {
            if (!isFullscreen)
            {
                prevBounds = this.Bounds;
                prevState = this.WindowState;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                this.Bounds = Screen.FromHandle(this.Handle).Bounds;
                // Remove region and padding for fullscreen
                this.Region = null;
                this.Padding = new Padding(0);
                isFullscreen = true;
                this.PerformLayout();
                this.Refresh();
            }
            else
            {
                this.WindowState = prevState;
                this.Bounds = prevBounds;
                // Restore region and padding
                int windowMarginLeft = 24;
                int windowMarginTop = 0;
                int windowMarginRight = 8;
                int windowMarginBottom = 8;
                this.Region = new Region(GetRoundedRectPath(new Rectangle(windowMarginLeft, windowMarginTop, this.Width - windowMarginLeft - windowMarginRight, this.Height - windowMarginTop - windowMarginBottom), cornerRadius));
                this.Padding = new Padding(windowMarginLeft, windowMarginTop, windowMarginRight, windowMarginBottom);
                isFullscreen = false;
                this.PerformLayout();
                this.Refresh();
            }
        }
    }
} 
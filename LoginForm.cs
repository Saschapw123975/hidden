using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace FileManager
{
    public partial class LoginForm : Form
    {
        private TextBox passwordBox;
        private Label messageLabel;
        private Button loginButton;
        private Panel loginPanel;
        private Label titleLabel;
        private const string CORRECT_PASSWORD = "123975";
        private int loginAttempts = 0;
        private System.Windows.Forms.Timer glowTimer;
        private float glowAngle = 0;
        private int cornerRadius = 22;

        public LoginForm()
        {
            InitializeComponents();
            SetupGlowAnimation();
        }

        private void SetupGlowAnimation()
        {
            glowTimer = new System.Windows.Forms.Timer();
            glowTimer.Interval = 30;
            glowTimer.Tick += (s, e) => {
                glowAngle += 3;
                if (glowAngle >= 360) glowAngle = 0;
                this.Invalidate();
            };
            glowTimer.Start();
        }

        private void InitializeComponents()
        {
            // Form settings
            this.Size = new Size(800, 540);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(18, 18, 20);
            this.Paint += LoginForm_Paint;
            this.MouseDown += LoginForm_MouseDown;
            this.DoubleBuffered = true;
            this.Region = new Region(GetRoundedRectPath(new Rectangle(0, 0, this.Width, this.Height), cornerRadius));
            this.Resize += (s, e) =>
            {
                this.Region = new Region(GetRoundedRectPath(new Rectangle(0, 0, this.Width, this.Height), cornerRadius));
            };

            // Title
            titleLabel = new Label
            {
                Text = "CYRMSON",
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(60, 48),
                BackColor = Color.Transparent
            };

            // Login panel
            loginPanel = new Panel
            {
                Size = new Size(600, 320),
                Location = new Point(100, 130),
                BackColor = Color.FromArgb(28, 28, 32)
            };
            loginPanel.Region = new Region(GetRoundedRectPath(new Rectangle(0, 0, loginPanel.Width, loginPanel.Height), 28));

            // Password box with custom border
            Panel passwordContainer = new Panel
            {
                Size = new Size(380, 54),
                Location = new Point(110, 130),
                BackColor = Color.FromArgb(34, 34, 38),
                Padding = new Padding(1)
            };
            passwordContainer.Region = new Region(GetRoundedRectPath(new Rectangle(0, 0, passwordContainer.Width, passwordContainer.Height), 14));

            passwordBox = new TextBox
            {
                Size = new Size(378, 52),
                Location = new Point(1, 1),
                BackColor = Color.FromArgb(34, 34, 38),
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 16),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill
            };
            passwordBox.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) AttemptLogin(); };
            passwordContainer.Controls.Add(passwordBox);

            // Message label
            messageLabel = new Label
            {
                Size = new Size(380, 32),
                Location = new Point(110, 90),
                Text = "Enter Access Key",
                ForeColor = Color.FromArgb(160, 160, 160),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Login button
            loginButton = new Button
            {
                Size = new Size(380, 54),
                Location = new Point(110, 210),
                Text = "AUTHENTICATE",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 44),
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            loginButton.FlatAppearance.BorderSize = 0;
            loginButton.Region = new Region(GetRoundedRectPath(new Rectangle(0, 0, loginButton.Width, loginButton.Height), 14));
            loginButton.MouseEnter += (s, e) => loginButton.BackColor = Color.FromArgb(60, 60, 64);
            loginButton.MouseLeave += (s, e) => loginButton.BackColor = Color.FromArgb(40, 40, 44);
            loginButton.Click += (s, e) => AttemptLogin();

            // Status indicators
            Label statusLabel = new Label
            {
                Text = "Connected",
                ForeColor = Color.FromArgb(80, 200, 180),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(600 - 140, 24)
            };

            // Close button
            Label closeButton = new Label
            {
                Text = "×",
                Location = new Point(760, 18),
                AutoSize = true,
                Font = new Font("Arial", 28),
                ForeColor = Color.FromArgb(120, 120, 120),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            closeButton.Click += (s, e) => Application.Exit();
            closeButton.MouseEnter += (s, e) => closeButton.ForeColor = Color.FromArgb(200, 80, 80);
            closeButton.MouseLeave += (s, e) => closeButton.ForeColor = Color.FromArgb(120, 120, 120);

            // Add controls
            loginPanel.Controls.Add(passwordContainer);
            loginPanel.Controls.Add(messageLabel);
            loginPanel.Controls.Add(loginButton);
            this.Controls.Add(loginPanel);
            this.Controls.Add(titleLabel);
            this.Controls.Add(statusLabel);
            this.Controls.Add(closeButton);
        }

        private async void AttemptLogin()
        {
            if (passwordBox.Text == CORRECT_PASSWORD)
            {
                messageLabel.ForeColor = Color.FromArgb(80, 200, 180);
                messageLabel.Text = "Access Granted - Initializing...";
                await Task.Delay(500);
                for (int i = 0; i < 5; i++)
                {
                    loginPanel.BackColor = Color.FromArgb(40, 40, 44);
                    await Task.Delay(100);
                    loginPanel.BackColor = Color.FromArgb(28, 28, 32);
                    await Task.Delay(100);
                }
                for (double i = 1.0; i >= 0.0; i -= 0.05)
                {
                    this.Opacity = i;
                    await Task.Delay(30);
                }
                this.Hide();
                new FileManagerForm().Show();
            }
            else
            {
                loginAttempts++;
                messageLabel.ForeColor = Color.FromArgb(200, 80, 80);
                messageLabel.Text = "Access Denied";
                passwordBox.Text = "";
                if (loginAttempts >= 3)
                {
                    messageLabel.Text = "Security Lockout Initiated";
                    await Task.Delay(2000);
                    Application.Exit();
                }
                // Error animation
                Point original = loginPanel.Location;
                for (int i = 0; i < 5; i++)
                {
                    loginPanel.Location = new Point(original.X + 5, original.Y);
                    await Task.Delay(50);
                    loginPanel.Location = new Point(original.X - 5, original.Y);
                    await Task.Delay(50);
                }
                loginPanel.Location = original;
            }
        }

        private void LoginForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Draw rounded border for login panel
            using (GraphicsPath path = GetRoundedRectPath(new Rectangle(loginPanel.Left, loginPanel.Top, loginPanel.Width, loginPanel.Height), 28))
            {
                using (Pen pen = new Pen(Color.FromArgb(60, 60, 64), 2))
                {
                    e.Graphics.DrawPath(pen, path);
                }
                using (Pen glowPen = new Pen(Color.FromArgb(40, 80, 80, 80), 8))
                {
                    e.Graphics.DrawPath(glowPen, path);
                }
            }
            // Subtle animated scanline
            using (Pen scanline = new Pen(Color.FromArgb(18, 18, 20), 2))
            {
                float offset = (glowAngle) % 360;
                int y = (int)(this.Height * (Math.Sin(offset * Math.PI / 180) + 1) / 2);
                e.Graphics.DrawLine(scanline, 0, y, this.Width, y);
            }
        }

        private void LoginForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            glowTimer.Stop();
            base.OnFormClosing(e);
        }

        // Helper for rounded rectangles
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // For window dragging
    internal static class NativeMethods
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
    }
} 
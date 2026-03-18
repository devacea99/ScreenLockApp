using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenLockApp
{
    public partial class LockOverlay : Form
    {
        private Label lblMessage;
        private TextBox txtPassword;
        private Button btnUnlock;
        private AppSettings settings;
        private Timer checkFocusTimer;

        // Windows API to disable Task Manager
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        public LockOverlay(AppSettings settings)
        {
            this.settings = settings;
            InitializeCustomComponents();
            SetupForm();
            SetupFocusMonitor();
            DisableTaskManager();
        }

        private void SetupForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.BackColor = Color.Black;
            this.Opacity = 0.85;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = Screen.PrimaryScreen.Bounds;

            // Make it cover all screens
            int minX = 0, minY = 0, maxX = 0, maxY = 0;
            foreach (Screen screen in Screen.AllScreens)
            {
                minX = Math.Min(minX, screen.Bounds.X);
                minY = Math.Min(minY, screen.Bounds.Y);
                maxX = Math.Max(maxX, screen.Bounds.Right);
                maxY = Math.Max(maxY, screen.Bounds.Bottom);
            }
            this.Bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);

            // Block Alt+F4, Alt+Tab, keyboard shortcuts, etc.
            this.KeyPreview = true;
            this.KeyDown += LockOverlay_KeyDown;
        }

        private void SetupFocusMonitor()
        {
            // Timer to constantly bring window back to front
            checkFocusTimer = new Timer();
            checkFocusTimer.Interval = 100; // Check every 100ms
            checkFocusTimer.Tick += (s, e) =>
            {
                if (GetForegroundWindow() != this.Handle)
                {
                    SetForegroundWindow(this.Handle);
                    this.BringToFront();
                    this.Activate();
                }
            };
            checkFocusTimer.Start();
        }

        private void DisableTaskManager()
        {
            try
            {
                // Disable Task Manager via registry
                Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Policies\System");
                regkey.SetValue("DisableTaskMgr", 1);
                regkey.Close();
            }
            catch
            {
                // If we can't disable task manager, continue anyway
            }
        }

        private void EnableTaskManager()
        {
            try
            {
                // Re-enable Task Manager
                Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Policies\System");
                regkey.SetValue("DisableTaskMgr", 0);
                regkey.Close();
            }
            catch
            {
                // Continue anyway
            }
        }

        private void InitializeCustomComponents()
        {
            this.SuspendLayout();

            // Message Label
            lblMessage = new Label();
            lblMessage.Text = "🔒 Screen Locked\n\nEnter password or press unlock shortcut";
            lblMessage.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblMessage.ForeColor = Color.White;
            lblMessage.AutoSize = false;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.Size = new Size(700, 180);
            lblMessage.Location = new Point(
                (Screen.PrimaryScreen.Bounds.Width - 700) / 2,
                (Screen.PrimaryScreen.Bounds.Height - 350) / 2
            );

            // Password TextBox
            txtPassword = new TextBox();
            txtPassword.Font = new Font("Segoe UI", 16F);
            txtPassword.Size = new Size(400, 35);
            txtPassword.Location = new Point(
                (Screen.PrimaryScreen.Bounds.Width - 400) / 2,
                lblMessage.Bottom + 20
            );
            txtPassword.PasswordChar = '*';
            txtPassword.TextAlign = HorizontalAlignment.Center;
            txtPassword.KeyPress += TxtPassword_KeyPress;

            // Unlock Button
            btnUnlock = new Button();
            btnUnlock.Text = "Unlock";
            btnUnlock.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnUnlock.Size = new Size(150, 45);
            btnUnlock.Location = new Point(
                (Screen.PrimaryScreen.Bounds.Width - 150) / 2,
                txtPassword.Bottom + 20
            );
            btnUnlock.Click += BtnUnlock_Click;
            btnUnlock.BackColor = Color.FromArgb(0, 120, 215);
            btnUnlock.ForeColor = Color.White;
            btnUnlock.FlatStyle = FlatStyle.Flat;
            btnUnlock.FlatAppearance.BorderSize = 0;

            this.Controls.Add(lblMessage);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnUnlock);

            this.ResumeLayout(false);
        }

        private void TxtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnUnlock_Click(sender, e);
            }
        }

        private void BtnUnlock_Click(object sender, EventArgs e)
        {
            if (settings.UsePassword && txtPassword.Text == settings.Password)
            {
                UnlockScreen();
            }
            else if (!settings.UsePassword)
            {
                UnlockScreen();
            }
            else
            {
                // Stop the focus timer temporarily to allow MessageBox to show
                checkFocusTimer?.Stop();

                // Show error message
                MessageBox.Show(this, "Incorrect password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Restart the focus timer
                checkFocusTimer?.Start();

                // Clear and refocus
                txtPassword.Clear();
                txtPassword.Focus();

                // Bring window back to front
                this.BringToFront();
                this.Activate();
            }
        }

        public void UnlockScreen()
        {
            checkFocusTimer?.Stop();
            EnableTaskManager();
            this.Close();
        }

        private void LockOverlay_KeyDown(object sender, KeyEventArgs e)
        {
            // Block common shortcuts
            if (e.Alt && e.KeyCode == Keys.F4)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            if (e.Alt && e.KeyCode == Keys.Tab)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            if (e.Control && e.Shift && e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            if (e.Control && e.Alt && e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                // Show message that it's blocked
                lblMessage.Text = "🔒 Screen Locked\n\nCtrl+Alt+Del is disabled\n\nEnter password or press unlock shortcut";
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Block all mouse clicks
            return;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            checkFocusTimer?.Stop();
            EnableTaskManager();
            base.OnFormClosing(e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80; // WS_EX_TOOLWINDOW
                return cp;
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            txtPassword.Focus();
        }
    }
}

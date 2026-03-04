using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenLockApp
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private KeyboardHook keyboardHook;
        private AppSettings settings;
        private LockOverlay lockOverlay;
        private bool isLocked = false;

        public Form1()
        {
            InitializeComponent();

            // Load settings
            settings = SettingsManager.LoadSettings();

            // Setup system tray
            SetupSystemTray();

            // Setup keyboard hook
            SetupKeyboardHook();

            // Hide the main form
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Opacity = 0;
        }

        private void SetupSystemTray()
        {
            // Create context menu
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Lock Screen", null, OnLockScreen);
            trayMenu.Items.Add("Settings", null, OnSettings);
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Exit", null, OnExit);

            // Create tray icon
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Screen Lock App";
            trayIcon.Icon = SystemIcons.Shield; // You can replace this with custom icon
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += (s, e) => OnLockScreen(s, e);
        }

        private void SetupKeyboardHook()
        {
            keyboardHook = new KeyboardHook();
            keyboardHook.KeyPressed += KeyboardHook_KeyPressed;
            keyboardHook.Start();
        }

        private void KeyboardHook_KeyPressed(object sender, KeyEventArgs e)
        {
            // Build the pressed key string
            string pressedKey = "";

            if (e.Control) pressedKey += "Control+";
            if (e.Alt) pressedKey += "Alt+";
            if (e.Shift) pressedKey += "Shift+";

            // Get the actual key (without modifiers)
            Keys keyCode = e.KeyCode;
            pressedKey += keyCode.ToString();

            // Debug: Show what key was pressed (you can remove this later)
            System.Diagnostics.Debug.WriteLine($"Pressed: {pressedKey}");
            System.Diagnostics.Debug.WriteLine($"Lock Shortcut: {settings.LockShortcut}");
            System.Diagnostics.Debug.WriteLine($"Unlock Shortcut: {settings.UnlockShortcut}");

            // Check for lock shortcut
            if (settings.UseShortcut && pressedKey == settings.LockShortcut && !isLocked)
            {
                LockScreen();
            }

            // Check for unlock shortcut
            if (settings.UseShortcut && pressedKey == settings.UnlockShortcut && isLocked)
            {
                UnlockScreen();
            }
        }

        private void OnLockScreen(object sender, EventArgs e)
        {
            LockScreen();
        }

        private void LockScreen()
        {
            if (isLocked) return;

            isLocked = true;
            lockOverlay = new LockOverlay(settings);
            lockOverlay.FormClosed += (s, e) =>
            {
                isLocked = false;
                lockOverlay = null;
            };
            lockOverlay.Show();
        }

        private void UnlockScreen()
        {
            if (!isLocked || lockOverlay == null) return;

            lockOverlay.UnlockScreen();
            isLocked = false;
        }

        private void OnSettings(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm(settings);
            settingsForm.ShowDialog();

            // Reload settings after closing settings form
            settings = SettingsManager.LoadSettings();
        }

        private void OnExit(object sender, EventArgs e)
        {
            keyboardHook?.Dispose();
            trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                keyboardHook?.Dispose();
                trayIcon?.Dispose();
                base.OnFormClosing(e);
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            // Prevent the form from being shown
            base.SetVisibleCore(false);
        }
    }
}

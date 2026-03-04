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
    public partial class SettingsForm : Form
    {
        private AppSettings settings;
        private TextBox txtPassword;
        private TextBox txtLockShortcut;
        private TextBox txtUnlockShortcut;
        private CheckBox chkUsePassword;
        private CheckBox chkUseShortcut;
        private Button btnSave;
        private Button btnCancel;
        private Label lblHint;

        public SettingsForm(AppSettings settings)
        {
            this.settings = settings;
            InitializeCustomComponents();
            LoadSettings();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Screen Lock Settings";
            this.Size = new Size(500, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            int yPos = 20;

            // Password Section
            Label lblPassword = new Label();
            lblPassword.Text = "Unlock Password:";
            lblPassword.Location = new Point(20, yPos);
            lblPassword.AutoSize = true;
            this.Controls.Add(lblPassword);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(20, yPos + 25);
            txtPassword.Size = new Size(440, 25);
            txtPassword.PasswordChar = '*';
            this.Controls.Add(txtPassword);

            chkUsePassword = new CheckBox();
            chkUsePassword.Text = "Enable password unlock";
            chkUsePassword.Location = new Point(20, yPos + 55);
            chkUsePassword.AutoSize = true;
            this.Controls.Add(chkUsePassword);

            yPos += 95;

            // Lock Shortcut
            Label lblLock = new Label();
            lblLock.Text = "Lock Screen Shortcut:";
            lblLock.Location = new Point(20, yPos);
            lblLock.AutoSize = true;
            this.Controls.Add(lblLock);

            txtLockShortcut = new TextBox();
            txtLockShortcut.Location = new Point(20, yPos + 25);
            txtLockShortcut.Size = new Size(440, 25);
            txtLockShortcut.ReadOnly = true;
            txtLockShortcut.KeyDown += TxtShortcut_KeyDown;
            this.Controls.Add(txtLockShortcut);

            yPos += 60;

            // Unlock Shortcut
            Label lblUnlock = new Label();
            lblUnlock.Text = "Unlock Screen Shortcut:";
            lblUnlock.Location = new Point(20, yPos);
            lblUnlock.AutoSize = true;
            this.Controls.Add(lblUnlock);

            txtUnlockShortcut = new TextBox();
            txtUnlockShortcut.Location = new Point(20, yPos + 25);
            txtUnlockShortcut.Size = new Size(440, 25);
            txtUnlockShortcut.ReadOnly = true;
            txtUnlockShortcut.KeyDown += TxtShortcut_KeyDown;
            this.Controls.Add(txtUnlockShortcut);

            chkUseShortcut = new CheckBox();
            chkUseShortcut.Text = "Enable shortcut unlock";
            chkUseShortcut.Location = new Point(20, yPos + 55);
            chkUseShortcut.AutoSize = true;
            this.Controls.Add(chkUseShortcut);

            yPos += 95;

            // Hint Label
            lblHint = new Label();
            lblHint.Text = "💡 Hint: Click in shortcut box and press your desired key combination\n" +
                          "Recommended: Ctrl+Alt+L (Lock), Ctrl+Alt+U (Unlock)";
            lblHint.Location = new Point(20, yPos);
            lblHint.Size = new Size(440, 50);
            lblHint.ForeColor = Color.DarkBlue;
            this.Controls.Add(lblHint);

            yPos += 60;

            // Save Button
            btnSave = new Button();
            btnSave.Text = "Save";
            btnSave.Location = new Point(280, yPos);
            btnSave.Size = new Size(90, 30);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // Cancel Button
            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(380, yPos);
            btnCancel.Size = new Size(90, 30);
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void LoadSettings()
        {
            txtPassword.Text = settings.Password;
            txtLockShortcut.Text = settings.LockShortcut;
            txtUnlockShortcut.Text = settings.UnlockShortcut;
            chkUsePassword.Checked = settings.UsePassword;
            chkUseShortcut.Checked = settings.UseShortcut;
        }

        private void TxtShortcut_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;

            if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {
                ((TextBox)sender).Clear();
                return;
            }

            string shortcut = "";
            if (e.Control) shortcut += "Control+";
            if (e.Alt) shortcut += "Alt+";
            if (e.Shift) shortcut += "Shift+";

            if (e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.Menu &&
                e.KeyCode != Keys.ShiftKey && e.KeyCode != Keys.LWin && e.KeyCode != Keys.RWin)
            {
                shortcut += e.KeyCode.ToString();
                ((TextBox)sender).Text = shortcut;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text) && chkUsePassword.Checked)
            {
                MessageBox.Show("Please enter a password or disable password unlock.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            settings.Password = txtPassword.Text;
            settings.LockShortcut = txtLockShortcut.Text;
            settings.UnlockShortcut = txtUnlockShortcut.Text;
            settings.UsePassword = chkUsePassword.Checked;
            settings.UseShortcut = chkUseShortcut.Checked;

            SettingsManager.SaveSettings(settings);
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}

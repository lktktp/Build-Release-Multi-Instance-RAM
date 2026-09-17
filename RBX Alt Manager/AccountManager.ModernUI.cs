using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using RBX_Alt_Manager.Classes;

namespace RBX_Alt_Manager
{
    public partial class AccountManager
    {
        // Modern UI Components
        private Panel leftNavPanel;
        private Panel centerPanel;
        private Panel rightDetailsPanel;

        private FlowLayoutPanel cardsContainer;
        private TextBox searchTextBox;
        private ComboBox sortComboBox;
        private Label navBadgeLabel;
        private Button btnToggleHideUser;

        // Right Panel Components
        private PictureBox rightAvatarBox;
        private Label rightUsernameLabel;
        private Label rightStatusBadge;
        private Label rightAliasLabel;
        private Label rightIdLabel;

        private Label rightValUsername;
        private Label rightValAlias;
        private Label rightValUserId;
        private Label rightValDescription;
        private Label statusBarLabel;

        private List<ModernAccountCard> modernCards = new List<ModernAccountCard>();

        public void SetupModernDashboardLayout()
        {
            try
            {
                this.SuspendLayout();

                // Window Setup
                this.Size = new Size(1120, 690);
                this.MinimumSize = new Size(980, 580);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.BackColor = Color.FromArgb(11, 15, 25);
                this.ForeColor = Color.FromArgb(248, 250, 252);
                this.Text = "Multi-Roblox Account Manager [lktktp Edition] v4.1";

                // Hide old AccountsView (kept in controls for data/event sync)
                AccountsView.Visible = false;
                AccountsView.Dock = DockStyle.None;
                AccountsView.Size = new Size(10, 10);
                AccountsView.Location = new Point(-100, -100);

                // Hide old labels and redundant controls
                if (LabelPlaceID != null) LabelPlaceID.Visible = false;
                if (LabelJobID != null) LabelJobID.Visible = false;
                if (LabelUserID != null) LabelUserID.Visible = false;
                if (CurrentPlace != null) CurrentPlace.Visible = false;
                if (ShuffleIcon != null) ShuffleIcon.Visible = false;
                if (HistoryIcon != null) HistoryIcon.Visible = false;
                if (SaveToAccount != null) SaveToAccount.Visible = false;
                if (ConfigButton != null) ConfigButton.Visible = false;
                if (DonateButton != null) DonateButton.Visible = false;
                if (JoinDiscord != null) JoinDiscord.Visible = false;
                if (HideUsernamesCheckbox != null) HideUsernamesCheckbox.Visible = false;
                // NOTE: UserID is intentionally NOT hidden here — it is the free-text
                // "user to follow" field that Follow_Click reads (UserID.Text). It is
                // reparented into the visible "Follow User" row in BuildRightDetailsPanel
                // instead of being hidden, so the Follow feature keeps working.
                if (Alias != null) Alias.Visible = false;
                if (DescriptionBox != null) DescriptionBox.Visible = false;
                if (SetDescription != null) SetDescription.Visible = false;
                if (BrowserButton != null) BrowserButton.Visible = false;

                // 1. LEFT NAVIGATION PANEL
                BuildLeftNavigationPanel();

                // 2. RIGHT DETAILS PANEL
                BuildRightDetailsPanel();

                // 3. CENTER CONTENT PANEL
                BuildCenterContentPanel();

                // CRITICAL WINFORMS Z-ORDER FIX:
                // Left and Right must be at the BACK of the z-order so Center (DockStyle.Fill) calculates remaining space between them!
                if (leftNavPanel != null) leftNavPanel.SendToBack();
                if (rightDetailsPanel != null) rightDetailsPanel.SendToBack();
                if (centerPanel != null) centerPanel.BringToFront();

                this.ResumeLayout(true);

                // Initial populate
                RefreshModernCards();
            }
            catch (Exception ex)
            {
                Program.Logger.Error($"Failed to setup Modern Dashboard Layout: {ex}");
            }
        }

        private void BuildLeftNavigationPanel()
        {
            leftNavPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 185,
                BackColor = Color.FromArgb(11, 15, 25),
                Padding = new Padding(12, 16, 12, 16)
            };

            // Top Brand Section
            Panel pnlBrand = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };

            Label lblLogo = new Label
            {
                Text = "❖",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Location = new Point(0, 4),
                AutoSize = true
            };

            Label lblBrandTitle = new Label
            {
                Text = "Multi-Roblox",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(32, 4),
                AutoSize = true
            };

            Label lblBrandSub = new Label
            {
                Text = "Account Manager",
                Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(33, 24),
                AutoSize = true
            };

            Label lblVerBadge = new Label
            {
                Text = "v4.1",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                BackColor = Color.FromArgb(12, 45, 75),
                Location = new Point(33, 40),
                AutoSize = true,
                Padding = new Padding(4, 1, 4, 1)
            };

            pnlBrand.Controls.Add(lblLogo);
            pnlBrand.Controls.Add(lblBrandTitle);
            pnlBrand.Controls.Add(lblBrandSub);
            pnlBrand.Controls.Add(lblVerBadge);

            // Bottom Status
            Panel pnlLeftBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.Transparent
            };

            Label lblReadyDot = new Label
            {
                Text = "🟢 พร้อมใช้งาน",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(4, 4),
                AutoSize = true
            };

            Label lblReadySub = new Label
            {
                Text = "Roblox Account Manager",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(6, 22),
                AutoSize = true
            };

            pnlLeftBottom.Controls.Add(lblReadyDot);
            pnlLeftBottom.Controls.Add(lblReadySub);

            // Navigation Menu Container
            Panel pnlNavButtons = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 16, 0, 0)
            };

            // Button 1: All Accounts (Active)
            Button btnNavAll = CreateNavButton("  👤  บัญชีทั้งหมด", true);
            btnNavAll.Location = new Point(0, 20);

            navBadgeLabel = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(37, 99, 235),
                Location = new Point(125, 27),
                Size = new Size(26, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Button 2: Profiles
            Button btnNavProfiles = CreateNavButton("  👥  โปรไฟล์", false);
            btnNavProfiles.Location = new Point(0, 64);
            btnNavProfiles.Click += (s, e) => MessageBox.Show("โปรไฟล์กลุ่มบัญชี: จัดการกลุ่มบัญชีของคุณได้อย่างง่ายดาย", "Profiles", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Button 3: Settings
            Button btnNavSettings = CreateNavButton("  ⚙️  ตั้งค่า", false);
            btnNavSettings.Location = new Point(0, 108);
            btnNavSettings.Click += (s, e) => ConfigButton_Click(s, e);

            // Button 4: Help
            Button btnNavHelp = CreateNavButton("  ❓  ช่วยเหลือ", false);
            btnNavHelp.Location = new Point(0, 152);
            btnNavHelp.Click += (s, e) => infoToolStripMenuItem1_Click(s, e);

            pnlNavButtons.Controls.Add(navBadgeLabel);
            pnlNavButtons.Controls.Add(btnNavAll);
            pnlNavButtons.Controls.Add(btnNavProfiles);
            pnlNavButtons.Controls.Add(btnNavSettings);
            pnlNavButtons.Controls.Add(btnNavHelp);

            leftNavPanel.Controls.Add(pnlNavButtons);
            leftNavPanel.Controls.Add(pnlBrand);
            leftNavPanel.Controls.Add(pnlLeftBottom);

            this.Controls.Add(leftNavPanel);
        }

        private Button CreateNavButton(string text, bool isActive)
        {
            Button btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, isActive ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = isActive ? Color.White : Color.FromArgb(148, 163, 184),
                BackColor = isActive ? Color.FromArgb(2, 132, 199) : Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(160, 36),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void BuildRightDetailsPanel()
        {
            rightDetailsPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 325,
                BackColor = Color.FromArgb(13, 21, 34),
                Padding = new Padding(12, 16, 12, 12),
                AutoScroll = true
            };

            // 1. Profile Overview Header Card
            Panel pnlProfileCard = new Panel
            {
                Location = new Point(12, 14),
                Size = new Size(300, 80),
                BackColor = Color.FromArgb(19, 27, 42)
            };
            pnlProfileCard.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(30, 41, 59));
                using GraphicsPath path = ModernAccountCard.GetRoundedRectangle(new Rectangle(0, 0, pnlProfileCard.Width - 1, pnlProfileCard.Height - 1), 8);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(p, path);
            };

            rightAvatarBox = new PictureBox
            {
                Location = new Point(12, 14),
                Size = new Size(52, 52),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(30, 41, 59)
            };
            rightAvatarBox.Paint += (s, e) =>
            {
                if (rightAvatarBox.Image != null)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Rectangle rect = new Rectangle(1, 1, rightAvatarBox.Width - 3, rightAvatarBox.Height - 3);
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddEllipse(rect);
                        e.Graphics.SetClip(path);
                        e.Graphics.DrawImage(rightAvatarBox.Image, rect);
                        e.Graphics.ResetClip();
                    }
                    using (Pen p = new Pen(Color.FromArgb(51, 65, 85), 2f))
                        e.Graphics.DrawEllipse(p, rect);
                }
            };

            rightUsernameLabel = new Label
            {
                Text = "เลือกบัญชี...",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(72, 14),
                AutoSize = true
            };

            rightStatusBadge = new Label
            {
                Text = "🟢 ออนไลน์",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 211, 153),
                BackColor = Color.FromArgb(6, 78, 59),
                Location = new Point(220, 15),
                AutoSize = true,
                Padding = new Padding(4, 2, 4, 2)
            };

            rightAliasLabel = new Label
            {
                Text = "Alias: - ✎",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(72, 36),
                AutoSize = true
            };

            rightIdLabel = new Label
            {
                Text = "ID: -",
                Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(72, 53),
                AutoSize = true
            };

            pnlProfileCard.Controls.Add(rightAvatarBox);
            pnlProfileCard.Controls.Add(rightUsernameLabel);
            pnlProfileCard.Controls.Add(rightStatusBadge);
            pnlProfileCard.Controls.Add(rightAliasLabel);
            pnlProfileCard.Controls.Add(rightIdLabel);

            // 2. Place ID & Job ID compact inputs
            Panel pnlInputs = new Panel
            {
                Location = new Point(12, 102),
                Size = new Size(300, 52),
                BackColor = Color.Transparent
            };

            PlaceID.Parent = pnlInputs;
            PlaceID.Location = new Point(0, 16);
            PlaceID.Size = new Size(144, 26);
            PlaceID.BackColor = Color.FromArgb(19, 27, 42);
            PlaceID.ForeColor = Color.White;
            PlaceID.BorderColor = Color.FromArgb(37, 99, 235);
            PlaceID.Font = new Font("Segoe UI", 9f);

            Label lblP = new Label { Text = "Place ID", ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 7.5f), Location = new Point(0, 0), AutoSize = true };

            JobID.Parent = pnlInputs;
            JobID.Location = new Point(152, 16);
            JobID.Size = new Size(148, 26);
            JobID.BackColor = Color.FromArgb(19, 27, 42);
            JobID.ForeColor = Color.White;
            JobID.BorderColor = Color.FromArgb(37, 99, 235);
            JobID.Font = new Font("Segoe UI", 9f);

            Label lblJ = new Label { Text = "Job ID / Private Server", ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 7.5f), Location = new Point(152, 0), AutoSize = true };

            pnlInputs.Controls.Add(lblP);
            pnlInputs.Controls.Add(PlaceID);
            pnlInputs.Controls.Add(lblJ);
            pnlInputs.Controls.Add(JobID);

            // 3. Giant Hero Launch Button (JoinServer)
            JoinServer.Parent = rightDetailsPanel;
            JoinServer.Location = new Point(12, 162);
            JoinServer.Size = new Size(300, 44);
            JoinServer.Text = "▶   Join Server   🔗";
            JoinServer.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            JoinServer.BackColor = Color.FromArgb(37, 99, 235);
            JoinServer.ForeColor = Color.White;
            JoinServer.FlatStyle = FlatStyle.Flat;
            JoinServer.FlatAppearance.BorderSize = 0;
            JoinServer.Cursor = Cursors.Hand;

            // 4. Quick Actions Row (Utilities, Follow, Set Alias)
            Panel pnlQuickActions = new Panel
            {
                Location = new Point(12, 214),
                Size = new Size(300, 36),
                BackColor = Color.Transparent
            };

            ServerList.Parent = pnlQuickActions;
            ServerList.Location = new Point(0, 0);
            ServerList.Size = new Size(96, 36);
            ServerList.Text = "🛠️ Utilities";
            ServerList.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            ServerList.BackColor = Color.FromArgb(30, 41, 59);
            ServerList.ForeColor = Color.White;
            ServerList.FlatStyle = FlatStyle.Flat;
            ServerList.FlatAppearance.BorderSize = 0;

            Follow.Parent = pnlQuickActions;
            Follow.Location = new Point(102, 0);
            Follow.Size = new Size(96, 36);
            Follow.Text = "🔔 Follow";
            Follow.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            Follow.BackColor = Color.FromArgb(99, 102, 241);
            Follow.ForeColor = Color.White;
            Follow.FlatStyle = FlatStyle.Flat;
            Follow.FlatAppearance.BorderSize = 0;

            SetAlias.Parent = pnlQuickActions;
            SetAlias.Location = new Point(204, 0);
            SetAlias.Size = new Size(96, 36);
            SetAlias.Text = "🏷️ Set Alias";
            SetAlias.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            SetAlias.BackColor = Color.FromArgb(30, 41, 59);
            SetAlias.ForeColor = Color.White;
            SetAlias.FlatStyle = FlatStyle.Flat;
            SetAlias.FlatAppearance.BorderSize = 0;

            // 4b. Follow target input — restores the legacy "UserID" textbox
            // (used by Follow_Click to know WHO to follow) which must stay
            // reachable by the user, not just hidden.
            Panel pnlFollowTarget = new Panel
            {
                Location = new Point(12, 258),
                Size = new Size(300, 34),
                BackColor = Color.Transparent
            };

            Label lblFollowTarget = new Label
            {
                Text = "Username / User ID สำหรับ Follow",
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 7.5f),
                Location = new Point(0, 0),
                AutoSize = true
            };

            UserID.Parent = pnlFollowTarget;
            UserID.Location = new Point(0, 14);
            UserID.Size = new Size(300, 20);
            UserID.BackColor = Color.FromArgb(19, 27, 42);
            UserID.ForeColor = Color.White;
            UserID.BorderColor = Color.FromArgb(37, 99, 235);
            UserID.Font = new Font("Segoe UI", 8.5f);

            pnlFollowTarget.Controls.Add(lblFollowTarget);
            pnlFollowTarget.Controls.Add(UserID);

            // 5. Account Information Section (ข้อมูลบัญชี)
            Panel pnlDetails = new Panel
            {
                Location = new Point(12, 300),
                Size = new Size(300, 160),
                BackColor = Color.FromArgb(19, 27, 42)
            };
            pnlDetails.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(30, 41, 59));
                using GraphicsPath path = ModernAccountCard.GetRoundedRectangle(new Rectangle(0, 0, pnlDetails.Width - 1, pnlDetails.Height - 1), 8);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(p, path);
            };

            Label lblDetailsHeader = new Label
            {
                Text = "👤  ข้อมูลบัญชี",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 10),
                AutoSize = true
            };
            pnlDetails.Controls.Add(lblDetailsHeader);

            // Detail rows
            rightValUsername = AddDetailRow(pnlDetails, "Username", "-", 38, true, () => Clipboard.SetText(rightValUsername.Text));
            rightValAlias = AddEditableDetailRow(pnlDetails, "Alias", "-", 66, Alias, SetAlias);
            rightValUserId = AddDetailRow(pnlDetails, "User ID", "-", 94, true, () => Clipboard.SetText(rightValUserId.Text));
            rightValDescription = AddEditableDetailRow(pnlDetails, "Description", "-", 122, DescriptionBox, SetDescription);

            // 6. Advanced Settings Section (การตั้งค่าขั้นสูง)
            Panel pnlAdvSettings = new Panel
            {
                Location = new Point(12, 470),
                Size = new Size(300, 115),
                BackColor = Color.FromArgb(19, 27, 42)
            };
            pnlAdvSettings.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(30, 41, 59));
                using GraphicsPath path = ModernAccountCard.GetRoundedRectangle(new Rectangle(0, 0, pnlAdvSettings.Width - 1, pnlAdvSettings.Height - 1), 8);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(p, path);
            };

            Label lblAdvHeader = new Label
            {
                Text = "⚙️  การตั้งค่าขั้นสูง",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 10),
                AutoSize = true
            };
            pnlAdvSettings.Controls.Add(lblAdvHeader);

            EditTheme.Parent = pnlAdvSettings;
            EditTheme.Location = new Point(12, 36);
            EditTheme.Size = new Size(276, 32);
            EditTheme.Text = "🎨  Edit Theme                              >";
            EditTheme.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            EditTheme.BackColor = Color.FromArgb(30, 41, 59);
            EditTheme.ForeColor = Color.FromArgb(203, 213, 225);
            EditTheme.FlatStyle = FlatStyle.Flat;
            EditTheme.FlatAppearance.BorderSize = 0;
            EditTheme.TextAlign = ContentAlignment.MiddleLeft;

            LaunchNexus.Parent = pnlAdvSettings;
            LaunchNexus.Location = new Point(12, 74);
            LaunchNexus.Size = new Size(276, 32);
            LaunchNexus.Text = "🛡️  Account Control                      >";
            LaunchNexus.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            LaunchNexus.BackColor = Color.FromArgb(30, 41, 59);
            LaunchNexus.ForeColor = Color.FromArgb(203, 213, 225);
            LaunchNexus.FlatStyle = FlatStyle.Flat;
            LaunchNexus.FlatAppearance.BorderSize = 0;
            LaunchNexus.TextAlign = ContentAlignment.MiddleLeft;

            // 7. Status Bar Bottom Right
            statusBarLabel = new Label
            {
                Location = new Point(12, 595),
                Size = new Size(300, 24),
                Text = "⚡ พร้อมใช้งาน / 0 บัญชี",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129),
                TextAlign = ContentAlignment.MiddleRight
            };

            rightDetailsPanel.Controls.Add(pnlProfileCard);
            rightDetailsPanel.Controls.Add(pnlInputs);
            rightDetailsPanel.Controls.Add(JoinServer);
            rightDetailsPanel.Controls.Add(pnlQuickActions);
            rightDetailsPanel.Controls.Add(pnlFollowTarget);
            rightDetailsPanel.Controls.Add(pnlDetails);
            rightDetailsPanel.Controls.Add(pnlAdvSettings);
            rightDetailsPanel.Controls.Add(statusBarLabel);

            this.Controls.Add(rightDetailsPanel);
        }

        private Label AddDetailRow(Panel parent, string label, string defaultValue, int y, bool isCopy, Action onClick)
        {
            Label lblKey = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(12, y),
                AutoSize = true
            };

            Label lblVal = new Label
            {
                Text = defaultValue,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(110, y),
                Size = new Size(150, 18),
                AutoEllipsis = true
            };

            Button btnAction = new Button
            {
                Text = isCopy ? "📋" : "✎",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(24, 20),
                Location = new Point(266, y - 2),
                Cursor = Cursors.Hand
            };
            btnAction.FlatAppearance.BorderSize = 0;
            btnAction.Click += (s, e) => onClick?.Invoke();

            parent.Controls.Add(lblKey);
            parent.Controls.Add(lblVal);
            parent.Controls.Add(btnAction);

            return lblVal;
        }

        /// <summary>
        /// A detail row whose pencil (✎) button turns the value into an inline,
        /// editable TextBox instead of performing the legacy click directly.
        /// This avoids the bug where the legacy Alias/DescriptionBox textboxes —
        /// now hidden — would silently be committed with stale text. Enter or
        /// losing focus commits (writes into the legacy control, then calls its
        /// existing PerformClick(), leaving SetAlias_Click/SetDescription_Click
        /// completely untouched); Escape cancels without saving.
        /// </summary>
        private Label AddEditableDetailRow(Panel parent, string label, string defaultValue, int y, Control legacyTarget, Button legacyCommitButton)
        {
            Label lblKey = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(12, y),
                AutoSize = true
            };

            Label lblVal = new Label
            {
                Text = defaultValue,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(110, y),
                Size = new Size(150, 18),
                AutoEllipsis = true
            };

            TextBox editBox = new TextBox
            {
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(110, y - 2),
                Size = new Size(150, 20),
                BackColor = Color.FromArgb(11, 15, 25),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            bool suppressLeave = false;

            void CommitEdit()
            {
                if (suppressLeave) return;
                suppressLeave = true;

                legacyTarget.Text = editBox.Text;
                legacyCommitButton.PerformClick(); // runs the existing, unmodified SetAlias_Click / SetDescription_Click

                lblVal.Text = string.IsNullOrEmpty(editBox.Text) ? "-" : editBox.Text;
                editBox.Visible = false;
                lblVal.Visible = true;

                suppressLeave = false;
            }

            void CancelEdit()
            {
                suppressLeave = true;
                editBox.Visible = false;
                lblVal.Visible = true;
                suppressLeave = false;
            }

            editBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; CommitEdit(); }
                else if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; CancelEdit(); }
            };
            editBox.Leave += (s, e) => CommitEdit();

            Button btnAction = new Button
            {
                Text = "✎",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(24, 20),
                Location = new Point(266, y - 2),
                Cursor = Cursors.Hand
            };
            btnAction.FlatAppearance.BorderSize = 0;
            btnAction.Click += (s, e) =>
            {
                editBox.Text = lblVal.Text == "-" ? "" : lblVal.Text;
                lblVal.Visible = false;
                editBox.Visible = true;
                editBox.Focus();
                editBox.SelectAll();
            };

            parent.Controls.Add(lblKey);
            parent.Controls.Add(lblVal);
            parent.Controls.Add(editBox);
            parent.Controls.Add(btnAction);

            return lblVal;
        }

        private void BuildCenterContentPanel()
        {
            centerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(11, 15, 25),
                Padding = new Padding(16, 14, 16, 14)
            };

            // 1. Top Search & Sort Bar
            Panel pnlTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.Transparent
            };

            searchTextBox = new TextBox
            {
                Location = new Point(0, 6),
                Size = new Size(340, 28),
                BackColor = Color.FromArgb(19, 27, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle
            };
            searchTextBox.TextChanged += (s, e) => RefreshModernCards();

            Label lblSearchPlaceholder = new Label
            {
                Text = "🔍 ค้นหาชื่อบัญชี / Alias...",
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Segoe UI", 9f),
                Location = new Point(6, 10),
                AutoSize = true,
                BackColor = Color.FromArgb(19, 27, 42),
                Cursor = Cursors.IBeam
            };
            lblSearchPlaceholder.Click += (s, e) => searchTextBox.Focus();
            searchTextBox.Enter += (s, e) => lblSearchPlaceholder.Visible = false;
            searchTextBox.Leave += (s, e) => lblSearchPlaceholder.Visible = string.IsNullOrEmpty(searchTextBox.Text);

            Label lblSort = new Label
            {
                Text = "เรียงตาม",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(360, 10),
                AutoSize = true
            };

            sortComboBox = new ComboBox
            {
                Location = new Point(415, 7),
                Size = new Size(150, 26),
                BackColor = Color.FromArgb(19, 27, 42),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.5f),
                FlatStyle = FlatStyle.Flat
            };
            sortComboBox.Items.AddRange(new object[] { "ชื่อบัญชี (A-Z)", "ชื่อบัญชี (Z-A)", "กลุ่ม (Group)", "ใช้งานล่าสุด" });
            sortComboBox.SelectedIndex = 0;
            sortComboBox.SelectedIndexChanged += (s, e) => RefreshModernCards();

            pnlTopBar.Controls.Add(lblSearchPlaceholder);
            pnlTopBar.Controls.Add(searchTextBox);
            pnlTopBar.Controls.Add(lblSort);
            pnlTopBar.Controls.Add(sortComboBox);

            // 2. Bottom Action Bar (4 Buttons)
            Panel pnlBottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };

            // Button 1: Add Account (Emerald Green)
            Add.Parent = pnlBottomBar;
            Add.Location = new Point(0, 8);
            Add.Size = new Size(135, 42);
            Add.Text = "➕  เพิ่มบัญชี\n(Add Account)";
            Add.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            Add.BackColor = Color.FromArgb(16, 185, 129);
            Add.ForeColor = Color.White;
            Add.FlatStyle = FlatStyle.Flat;
            Add.FlatAppearance.BorderSize = 0;
            Add.Menu = AddAccountsStrip;
            Add.Cursor = Cursors.Hand;

            // Button 2: Remove (Rose Red)
            Remove.Parent = pnlBottomBar;
            Remove.Location = new Point(145, 8);
            Remove.Size = new Size(125, 42);
            Remove.Text = "🗑️  ลบที่เลือก\n(Remove)";
            Remove.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            Remove.BackColor = Color.FromArgb(239, 68, 68);
            Remove.ForeColor = Color.White;
            Remove.FlatStyle = FlatStyle.Flat;
            Remove.FlatAppearance.BorderSize = 0;
            Remove.Cursor = Cursors.Hand;

            // Button 3: Toggle Hide Usernames
            // Delegates to the existing HideUsernamesCheckbox (kept, just invisible)
            // instead of duplicating its logic, so HideUsernamesCheckbox_CheckedChanged
            // (General.Set + legacy grid column width) stays the single source of truth.
            bool hideUserInitial = HideUsernamesCheckbox != null && HideUsernamesCheckbox.Checked;
            btnToggleHideUser = new Button
            {
                Text = hideUserInitial ? "☒ แสดงชื่อผู้ใช้ 👁️" : "☑ ซ่อนชื่อผู้ใช้ 👁️",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 42),
                Location = new Point(280, 8),
                Cursor = Cursors.Hand
            };
            btnToggleHideUser.FlatAppearance.BorderSize = 0;
            btnToggleHideUser.Click += (s, e) =>
            {
                HideUsernamesCheckbox.Checked = !HideUsernamesCheckbox.Checked;
                IniSettings.Save("RAMSettings.ini");
                btnToggleHideUser.Text = HideUsernamesCheckbox.Checked ? "☒ แสดงชื่อผู้ใช้ 👁️" : "☑ ซ่อนชื่อผู้ใช้ 👁️";
                RefreshModernCards();
            };
            btnToggleHideUser.Parent = pnlBottomBar;

            // Button 4: Open Browser
            OpenBrowser.Parent = pnlBottomBar;
            OpenBrowser.Location = new Point(420, 8);
            OpenBrowser.Size = new Size(140, 42);
            OpenBrowser.Text = "🌐  เปิดเบราว์เซอร์ ▼";
            OpenBrowser.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            OpenBrowser.BackColor = Color.FromArgb(30, 41, 59);
            OpenBrowser.ForeColor = Color.White;
            OpenBrowser.FlatStyle = FlatStyle.Flat;
            OpenBrowser.FlatAppearance.BorderSize = 0;
            OpenBrowser.Menu = OpenBrowserStrip;
            OpenBrowser.Cursor = Cursors.Hand;

            Add.Visible = true;
            Remove.Visible = true;
            btnToggleHideUser.Visible = true;
            OpenBrowser.Visible = true;

            pnlBottomBar.Controls.Add(Add);
            pnlBottomBar.Controls.Add(Remove);
            pnlBottomBar.Controls.Add(btnToggleHideUser);
            pnlBottomBar.Controls.Add(OpenBrowser);

            // 3. Middle Cards Container
            cardsContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(11, 15, 25),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 8, 8, 8)
            };
            cardsContainer.SizeChanged += (s, e) =>
            {
                int newWidth = Math.Max(cardsContainer.ClientSize.Width - 24, 420);
                foreach (var card in modernCards)
                {
                    card.Width = newWidth;
                }
            };

            centerPanel.Controls.Add(cardsContainer);
            centerPanel.Controls.Add(pnlTopBar);
            centerPanel.Controls.Add(pnlBottomBar);

            // Docking z-order inside centerPanel
            pnlTopBar.SendToBack();
            pnlBottomBar.SendToBack();
            cardsContainer.BringToFront();

            this.Controls.Add(centerPanel);
        }

        public void RefreshModernCards()
        {
            if (cardsContainer == null || AccountsList == null) return;

            this.InvokeIfRequired(() =>
            {
                cardsContainer.SuspendLayout();
                cardsContainer.Controls.Clear();
                modernCards.Clear();

                string filter = searchTextBox?.Text?.Trim() ?? "";
                bool hideUser = General.Get<bool>("HideUsernames");

                var list = AccountsList.AsEnumerable();

                // Apply Search Filter
                if (!string.IsNullOrEmpty(filter))
                {
                    list = list.Where(a =>
                        (a.Username != null && a.Username.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (a.Alias != null && a.Alias.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (a.UserID.ToString().IndexOf(filter) >= 0));
                }

                // Apply Sorting
                int sortIndex = sortComboBox?.SelectedIndex ?? 0;
                switch (sortIndex)
                {
                    case 0: // A-Z
                        list = list.OrderBy(a => a.Username);
                        break;
                    case 1: // Z-A
                        list = list.OrderByDescending(a => a.Username);
                        break;
                    case 2: // Group
                        list = list.OrderBy(a => a.Group).ThenBy(a => a.Username);
                        break;
                    case 3: // Last Used
                        list = list.OrderByDescending(a => a.LastUse);
                        break;
                }

                int cardWidth = Math.Max(cardsContainer.ClientSize.Width - 24, 450);

                foreach (Account acc in list)
                {
                    ModernAccountCard card = new ModernAccountCard(acc)
                    {
                        Width = cardWidth,
                        HideUsername = hideUser,
                        IsSelected = (SelectedAccount == acc || (SelectedAccounts != null && SelectedAccounts.Contains(acc)))
                    };

                    card.CardClicked += (s, clickedAcc) =>
                    {
                        bool isCtrl = (ModifierKeys & Keys.Control) == Keys.Control;
                        bool isShift = (ModifierKeys & Keys.Shift) == Keys.Shift;

                        if (isCtrl && SelectedAccounts != null)
                        {
                            if (SelectedAccounts.Contains(clickedAcc))
                                SelectedAccounts.Remove(clickedAcc);
                            else
                                SelectedAccounts.Add(clickedAcc);
                        }
                        else
                        {
                            SelectedAccounts = new List<Account> { clickedAcc };
                            SelectedAccount = clickedAcc;
                        }

                        // Sync with AccountsView
                        try { AccountsView.SelectedObjects = SelectedAccounts; } catch { }

                        UpdateRightPanelDetails(clickedAcc);
                        UpdateCardsSelectionState();
                    };

                    card.PlayClicked += async (s, playAcc) =>
                    {
                        // Loading state: block a second click on this card while the
                        // join is in flight (ModernAccountCard.Enabled also suppresses
                        // its mouse handling), then restore it regardless of outcome.
                        ModernAccountCard sourceCard = s as ModernAccountCard;
                        if (sourceCard != null) sourceCard.Enabled = false;

                        try
                        {
                            long pId = 0;
                            long.TryParse(PlaceID?.Text, out pId);
                            string jId = JobID?.Text ?? "";
                            bool vip = jId.Length > 4 && jId.Substring(0, 4) == "VIP:";

                            Program.Logger.Info($"[Quick Launch] Launching single account: {playAcc.Username}");
                            string res = await playAcc.JoinServer(pId, vip ? jId.Substring(4) : jId, false, vip);
                            if (!string.IsNullOrEmpty(res) && !res.Contains("Success"))
                                MessageBox.Show(res, "Launch Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        finally
                        {
                            if (sourceCard != null && !sourceCard.IsDisposed) sourceCard.Enabled = true;
                        }
                    };

                    card.CopyClicked += (s, copyAcc) =>
                    {
                        if (!string.IsNullOrEmpty(copyAcc.SecurityToken))
                            Clipboard.SetText(copyAcc.SecurityToken);
                        else if (!string.IsNullOrEmpty(copyAcc.Username))
                            Clipboard.SetText(copyAcc.Username);
                    };

                    card.MoreClicked += (s, screenPos) =>
                    {
                        SelectedAccount = acc;
                        SelectedAccounts = new List<Account> { acc };
                        UpdateCardsSelectionState();
                        UpdateRightPanelDetails(acc);
                        AccountsStrip?.Show(screenPos);
                    };

                    modernCards.Add(card);
                    cardsContainer.Controls.Add(card);
                }

                // Update counter badge and status bar
                if (navBadgeLabel != null) navBadgeLabel.Text = AccountsList.Count.ToString();
                if (statusBarLabel != null) statusBarLabel.Text = $"⚡ พร้อมใช้งาน / {AccountsList.Count} บัญชี";

                // Drop a stale selection (e.g. the selected account was just removed)
                if (SelectedAccount != null && !AccountsList.Contains(SelectedAccount))
                {
                    SelectedAccount = null;
                    SelectedAccounts = new List<Account>();
                }

                // Auto-select first account if none selected
                if (SelectedAccount == null && AccountsList.Count > 0)
                {
                    SelectedAccount = AccountsList[0];
                    SelectedAccounts = new List<Account> { SelectedAccount };
                }

                if (SelectedAccount != null)
                {
                    UpdateRightPanelDetails(SelectedAccount);
                    UpdateCardsSelectionState();
                }
                else
                {
                    // No accounts at all — reset the right panel to its empty state.
                    if (rightUsernameLabel != null) rightUsernameLabel.Text = "เลือกบัญชี...";
                    if (rightAliasLabel != null) rightAliasLabel.Text = "Alias: - ✎";
                    if (rightIdLabel != null) rightIdLabel.Text = "ID: -";
                    if (rightValUsername != null) rightValUsername.Text = "-";
                    if (rightValAlias != null) rightValAlias.Text = "-";
                    if (rightValUserId != null) rightValUserId.Text = "-";
                    if (rightValDescription != null) rightValDescription.Text = "-";
                    if (rightAvatarBox != null) rightAvatarBox.Image = null;
                }

                // UX requirement: disable actions that need a selected account.
                bool hasSelection = SelectedAccount != null;
                JoinServer.Enabled = hasSelection;
                Remove.Enabled = hasSelection;
                ServerList.Enabled = hasSelection;
                Follow.Enabled = hasSelection;
                SetAlias.Enabled = hasSelection;

                cardsContainer.ResumeLayout(true);
            });
        }

        private void UpdateCardsSelectionState()
        {
            foreach (var card in modernCards)
            {
                card.IsSelected = (SelectedAccount == card.Account || (SelectedAccounts != null && SelectedAccounts.Contains(card.Account)));
                card.Invalidate();
            }
        }

        public void UpdateRightPanelDetails(Account account)
        {
            if (account == null) return;

            this.InvokeIfRequired(() =>
            {
                bool hideUser = General.Get<bool>("HideUsernames");
                string uname = hideUser ? "••••••••" : account.Username;

                if (rightUsernameLabel != null) rightUsernameLabel.Text = uname;
                if (rightAliasLabel != null) rightAliasLabel.Text = "Alias: " + (!string.IsNullOrEmpty(account.Alias) ? account.Alias : "-") + " ✎";
                if (rightIdLabel != null) rightIdLabel.Text = "ID: " + (account.UserID > 0 ? account.UserID.ToString() : "-");

                bool isOnline = (account.Presence != null && account.Presence.userPresenceType != UserPresenceType.Offline);
                if (rightStatusBadge != null)
                {
                    rightStatusBadge.Text = isOnline ? "🟢 ออนไลน์" : "⚫ ออฟไลน์";
                    rightStatusBadge.BackColor = isOnline ? Color.FromArgb(6, 78, 59) : Color.FromArgb(30, 41, 59);
                    rightStatusBadge.ForeColor = isOnline ? Color.FromArgb(52, 211, 153) : Color.FromArgb(148, 163, 184);
                }

                if (rightValUsername != null) rightValUsername.Text = uname;
                if (rightValAlias != null) rightValAlias.Text = !string.IsNullOrEmpty(account.Alias) ? account.Alias : "-";
                if (rightValUserId != null) rightValUserId.Text = account.UserID > 0 ? account.UserID.ToString() : "-";
                if (rightValDescription != null) rightValDescription.Text = !string.IsNullOrEmpty(account.Description) ? account.Description : "-";

                // Update Avatar Image
                if (rightAvatarBox != null)
                {
                    if (ModernAccountCard.AvatarCache.TryGetValue(account.UserID, out Image cached))
                        rightAvatarBox.Image = cached;
                    else
                        rightAvatarBox.Image = null;
                }

                // Load saved PlaceID & JobID if exists
                if (!string.IsNullOrEmpty(account.GetField("SavedPlaceId")) && PlaceID != null)
                    PlaceID.Text = account.GetField("SavedPlaceId");

                if (!string.IsNullOrEmpty(account.GetField("SavedJobId")) && JobID != null)
                    JobID.Text = account.GetField("SavedJobId");
            });
        }
    }
}

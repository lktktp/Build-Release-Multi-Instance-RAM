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
        private Label lblSearchPlaceholder;
        private ComboBox sortComboBox;
        private Label lblSort;
        private Button btnSelectMode;
        private Button btnSelectAll;
        private Label navBadgeLabel;
        private Button btnNavAll;
        private Button btnNavProfiles;
        private Button btnNavSettings;
        private Button btnNavHelp;
        private Label lblReadyDot;
        private Label lblReadySub;
        private Label lblBrandTitle;
        private Label lblBrandSub;
        private Button btnToggleHideUser;

        // Right Panel Components
        private PictureBox rightAvatarBox;
        private Label rightUsernameLabel;
        private Label rightStatusBadge;
        private Label rightAliasLabel;
        private Label rightIdLabel;
        private Label lblPlaceId;
        private Label lblJobId;
        private Label lblFollowTarget;
        private Label lblDetailsHeader;
        private Label lblAdvHeader;
        private Button btnChangeLanguage;

        private Label rightValUsername;
        private Label rightValAlias;
        private Label rightValUserId;
        private Label rightValDescription;
        private Label statusBarLabel;

        // Select Mode state: when true, clicking any card toggles its selection
        private bool isSelectMode = false;

        internal List<ModernAccountCard> modernCards = new List<ModernAccountCard>();

        /// <summary>
        /// True if Thai language is explicitly configured in RAMSettings.ini; default is false (English).
        /// </summary>
        public static bool IsThai => General != null && General.Exists("Language") && General.Get<string>("Language") == "th";

        public void SetupModernDashboardLayout()
        {
            try
            {
                this.SuspendLayout();

                // Window Setup
                this.Size = new Size(1220, 740);
                this.MinimumSize = new Size(1080, 650);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.BackColor = Color.FromArgb(11, 15, 25);
                this.ForeColor = Color.FromArgb(248, 250, 252);
                this.Text = "Multi-Roblox Account Manager [lktktp Edition] v4.1";

                this.KeyPreview = true;
                this.KeyDown += (s, e) =>
                {
                    if (e.Control && e.KeyCode == Keys.A && (searchTextBox == null || !searchTextBox.Focused) && (PlaceID == null || !PlaceID.Focused) && (JobID == null || !JobID.Focused) && (UserID == null || !UserID.Focused))
                    {
                        SelectedAccounts = new List<Account>(AccountsList);
                        if (AccountsList.Count > 0) SelectedAccount = AccountsList[0];
                        try { AccountsView.SelectedObjects = SelectedAccounts; } catch { }
                        UpdateCardsSelectionState();
                        if (SelectedAccount != null) UpdateRightPanelDetails(SelectedAccount);
                        e.SuppressKeyPress = true;
                    }
                };

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
                if (Alias != null) Alias.Visible = false;
                if (DescriptionBox != null) DescriptionBox.Visible = false;
                if (SetDescription != null) SetDescription.Visible = false;
                if (BrowserButton != null) BrowserButton.Visible = false;

                // Set initial card language
                ModernAccountCard.IsThai = IsThai;

                // 1. LEFT NAVIGATION PANEL
                BuildLeftNavigationPanel();

                // 2. RIGHT DETAILS PANEL
                BuildRightDetailsPanel();

                // 3. CENTER CONTENT PANEL
                BuildCenterContentPanel();

                // Apply text in English (or Thai if configured)
                ApplyLocalization();

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

            lblBrandTitle = new Label
            {
                Text = "Multi-Roblox",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(0, 4),
                AutoSize = true
            };

            lblBrandSub = new Label
            {
                Text = "Account Manager",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(1, 26),
                AutoSize = true
            };

            Label lblVerBadge = new Label
            {
                Text = "v4.1",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                BackColor = Color.FromArgb(12, 45, 75),
                Location = new Point(105, 6),
                AutoSize = true,
                Padding = new Padding(4, 1, 4, 1)
            };

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

            lblReadyDot = new Label
            {
                Text = IsThai ? "Ready" : "Ready",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(2, 4),
                AutoSize = true
            };

            lblReadySub = new Label
            {
                Text = "Roblox Account Manager",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(2, 22),
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
            btnNavAll = CreateNavButton(IsThai ? "All Accounts" : "All Accounts", true);
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
            btnNavProfiles = CreateNavButton(IsThai ? "Profiles" : "Profiles", false);
            btnNavProfiles.Location = new Point(0, 64);
            btnNavProfiles.Click += (s, e) => MessageBox.Show(IsThai ? "จัดการกลุ่มโปรไฟล์และบัญชี" : "Profiles: Manage account profiles and groups.", "Profiles", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Button 3: Settings with Language option
            btnNavSettings = CreateNavButton(IsThai ? "Settings" : "Settings", false);
            btnNavSettings.Location = new Point(0, 108);

            ContextMenuStrip settingsStrip = new ContextMenuStrip();
            Action buildSettingsMenu = () =>
            {
                bool th = IsThai;
                settingsStrip.Items.Clear();

                ToolStripMenuItem itemPref = new ToolStripMenuItem(th ? "General Settings" : "General Settings", null, (s, e) => ConfigButton_Click(s, e));

                ToolStripMenuItem itemLang = new ToolStripMenuItem("Language");
                ToolStripMenuItem langEn = new ToolStripMenuItem("English (Default)", null, (s, e) => SetLanguage("en")) { Checked = !th };
                ToolStripMenuItem langTh = new ToolStripMenuItem("Thai", null, (s, e) => SetLanguage("th")) { Checked = th };
                itemLang.DropDownItems.Add(langEn);
                itemLang.DropDownItems.Add(langTh);

                settingsStrip.Items.AddRange(new ToolStripItem[] {
                    itemPref,
                    new ToolStripSeparator(),
                    itemLang
                });
            };

            btnNavSettings.Click += (s, e) =>
            {
                buildSettingsMenu();
                settingsStrip.Show(btnNavSettings, new Point(0, btnNavSettings.Height));
            };

            // Button 4: Help
            btnNavHelp = CreateNavButton(IsThai ? "Help" : "Help", false);
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
                Width = 330,
                BackColor = Color.FromArgb(13, 21, 34),
                Padding = new Padding(12, 12, 12, 12),
                AutoScroll = false
            };
            rightDetailsPanel.HorizontalScroll.Maximum = 0;
            rightDetailsPanel.HorizontalScroll.Visible = false;
            rightDetailsPanel.HorizontalScroll.Enabled = false;
            rightDetailsPanel.VerticalScroll.Maximum = 0;
            rightDetailsPanel.VerticalScroll.Visible = false;
            rightDetailsPanel.VerticalScroll.Enabled = false;

            int innerW = 306;
            int innerX = 12;

            // 1. Profile Overview Header Card
            Panel pnlProfileCard = new Panel
            {
                Location = new Point(innerX, 12),
                Size = new Size(innerW, 76),
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
                Location = new Point(10, 12),
                Size = new Size(52, 52),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(30, 41, 59)
            };
            rightAvatarBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;

                Rectangle rect = new Rectangle(1, 1, rightAvatarBox.Width - 3, rightAvatarBox.Height - 3);

                int multiCount = SelectedAccounts != null ? SelectedAccounts.Count : 0;
                if (multiCount > 1)
                {
                    // Multi-Account Selected: Draw stylish overlapping circular avatars
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(19, 27, 42)))
                        e.Graphics.FillEllipse(bgBrush, rect);

                    Account a1 = SelectedAccounts[0];
                    Account a2 = SelectedAccounts[1];
                    Image img1 = null, img2 = null;
                    lock (ModernAccountCard.AvatarCache)
                    {
                        if (a1 != null) ModernAccountCard.AvatarCache.TryGetValue(a1.UserID, out img1);
                        if (a2 != null) ModernAccountCard.AvatarCache.TryGetValue(a2.UserID, out img2);
                    }

                    // Back circle (Account 2)
                    Rectangle r2 = new Rectangle(rect.X + 16, rect.Y + 4, 30, 30);
                    using (GraphicsPath p2 = new GraphicsPath())
                    {
                        p2.AddEllipse(r2);
                        e.Graphics.SetClip(p2);
                        if (img2 != null)
                        {
                            e.Graphics.DrawImage(img2, r2);
                        }
                        else
                        {
                            using (SolidBrush b = new SolidBrush(Color.FromArgb(40, 50, 75)))
                                e.Graphics.FillEllipse(b, r2);
                            string init2 = (a2 != null && !string.IsNullOrEmpty(a2.Username)) ? a2.Username.Substring(0, 1).ToUpper() : "2";
                            using (Font f = new Font("Segoe UI", 10f, FontStyle.Bold))
                            using (SolidBrush tb = new SolidBrush(Color.FromArgb(203, 213, 225)))
                            {
                                SizeF sz = e.Graphics.MeasureString(init2, f);
                                e.Graphics.DrawString(init2, f, tb, r2.X + (r2.Width - sz.Width) / 2, r2.Y + (r2.Height - sz.Height) / 2);
                            }
                        }
                        e.Graphics.ResetClip();
                    }
                    using (Pen p2Pen = new Pen(Color.FromArgb(19, 27, 42), 2f))
                        e.Graphics.DrawEllipse(p2Pen, r2);

                    // Front circle (Account 1)
                    Rectangle r1 = new Rectangle(rect.X + 3, rect.Y + 12, 30, 30);
                    using (GraphicsPath p1 = new GraphicsPath())
                    {
                        p1.AddEllipse(r1);
                        e.Graphics.SetClip(p1);
                        if (img1 != null)
                        {
                            e.Graphics.DrawImage(img1, r1);
                        }
                        else
                        {
                            using (SolidBrush b = new SolidBrush(Color.FromArgb(37, 99, 235)))
                                e.Graphics.FillEllipse(b, r1);
                            string init1 = (a1 != null && !string.IsNullOrEmpty(a1.Username)) ? a1.Username.Substring(0, 1).ToUpper() : "1";
                            using (Font f = new Font("Segoe UI", 10f, FontStyle.Bold))
                            using (SolidBrush tb = new SolidBrush(Color.White))
                            {
                                SizeF sz = e.Graphics.MeasureString(init1, f);
                                e.Graphics.DrawString(init1, f, tb, r1.X + (r1.Width - sz.Width) / 2, r1.Y + (r1.Height - sz.Height) / 2);
                            }
                        }
                        e.Graphics.ResetClip();
                    }
                    using (Pen p1Pen = new Pen(Color.FromArgb(19, 27, 42), 2f))
                        e.Graphics.DrawEllipse(p1Pen, r1);

                    // Count pill badge at bottom right: e.g. "2"
                    Rectangle badgeRect = new Rectangle(rect.Right - 18, rect.Bottom - 18, 18, 18);
                    using (SolidBrush badgeBg = new SolidBrush(Color.FromArgb(37, 99, 235)))
                        e.Graphics.FillEllipse(badgeBg, badgeRect);
                    using (Pen bp = new Pen(Color.FromArgb(19, 27, 42), 1.5f))
                        e.Graphics.DrawEllipse(bp, badgeRect);
                    using (Font bf = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                    using (SolidBrush bft = new SolidBrush(Color.White))
                    {
                        string cntStr = multiCount.ToString();
                        SizeF bsz = e.Graphics.MeasureString(cntStr, bf);
                        e.Graphics.DrawString(cntStr, bf, bft, badgeRect.X + (badgeRect.Width - bsz.Width) / 2, badgeRect.Y + (badgeRect.Height - bsz.Height) / 2);
                    }

                    // Outer glowing border
                    using (Pen borderP = new Pen(Color.FromArgb(37, 99, 235), 2f))
                        e.Graphics.DrawEllipse(borderP, rect);

                    return;
                }

                // Single account or placeholder avatar drawing
                if (rightAvatarBox.Image != null)
                {
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddEllipse(rect);
                        e.Graphics.SetClip(path);
                        e.Graphics.DrawImage(rightAvatarBox.Image, rect);
                        e.Graphics.ResetClip();
                    }
                }
                else
                {
                    using (SolidBrush pBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
                        e.Graphics.FillEllipse(pBrush, rect);

                    string initial = (SelectedAccount != null && !string.IsNullOrEmpty(SelectedAccount.Username))
                        ? SelectedAccount.Username.Substring(0, 1).ToUpper() : "?";
                    using (Font initialFont = new Font("Segoe UI", 16f, FontStyle.Bold))
                    using (SolidBrush tBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
                    {
                        SizeF sz = e.Graphics.MeasureString(initial, initialFont);
                        e.Graphics.DrawString(initial, initialFont, tBrush,
                            rect.X + (rect.Width - sz.Width) / 2,
                            rect.Y + (rect.Height - sz.Height) / 2);
                    }
                }
                using (Pen p = new Pen(Color.FromArgb(51, 65, 85), 2f))
                    e.Graphics.DrawEllipse(p, rect);
            };

            rightUsernameLabel = new Label
            {
                Text = IsThai ? "เลือกบัญชี..." : "Select an account...",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(68, 12),
                AutoSize = true
            };

            rightStatusBadge = new Label
            {
                Text = "Offline",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.FromArgb(30, 41, 59),
                Location = new Point(220, 12),
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2)
            };

            rightAliasLabel = new Label
            {
                Text = "Alias: -",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(68, 33),
                AutoSize = true
            };

            rightIdLabel = new Label
            {
                Text = "ID: -",
                Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(68, 52),
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
                Location = new Point(innerX, 96),
                Size = new Size(innerW, 48),
                BackColor = Color.Transparent
            };

            PlaceID.Parent = pnlInputs;
            PlaceID.Location = new Point(0, 16);
            PlaceID.Size = new Size(148, 24);
            PlaceID.BackColor = Color.FromArgb(19, 27, 42);
            PlaceID.ForeColor = Color.White;
            PlaceID.BorderColor = Color.FromArgb(37, 99, 235);
            PlaceID.Font = new Font("Segoe UI", 9f);

            lblPlaceId = new Label { Text = "Place ID", ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 7.5f), Location = new Point(0, 0), AutoSize = true };

            JobID.Parent = pnlInputs;
            JobID.Location = new Point(156, 16);
            JobID.Size = new Size(150, 24);
            JobID.BackColor = Color.FromArgb(19, 27, 42);
            JobID.ForeColor = Color.White;
            JobID.BorderColor = Color.FromArgb(37, 99, 235);
            JobID.Font = new Font("Segoe UI", 9f);

            lblJobId = new Label { Text = "Job ID / Private Server", ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 7.5f), Location = new Point(156, 0), AutoSize = true };

            pnlInputs.Controls.Add(lblPlaceId);
            pnlInputs.Controls.Add(PlaceID);
            pnlInputs.Controls.Add(lblJobId);
            pnlInputs.Controls.Add(JobID);

            // 3. Launch Button (JoinServer)
            JoinServer.Parent = rightDetailsPanel;
            JoinServer.Location = new Point(innerX, 150);
            JoinServer.Size = new Size(innerW, 40);
            JoinServer.Text = "Join Server";
            JoinServer.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            JoinServer.BackColor = Color.FromArgb(37, 99, 235);
            JoinServer.ForeColor = Color.White;
            JoinServer.FlatStyle = FlatStyle.Flat;
            JoinServer.FlatAppearance.BorderSize = 0;
            JoinServer.Cursor = Cursors.Hand;

            // 4. Quick Actions Row (Utilities, Follow, Set Alias)
            Panel pnlQuickActions = new Panel
            {
                Location = new Point(innerX, 196),
                Size = new Size(innerW, 34),
                BackColor = Color.Transparent
            };

            ServerList.Parent = pnlQuickActions;
            ServerList.Location = new Point(0, 0);
            ServerList.Size = new Size(98, 34);
            ServerList.Text = "Utilities";
            ServerList.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            ServerList.BackColor = Color.FromArgb(30, 41, 59);
            ServerList.ForeColor = Color.White;
            ServerList.FlatStyle = FlatStyle.Flat;
            ServerList.FlatAppearance.BorderSize = 0;
            ServerList.Cursor = Cursors.Hand;

            Follow.Parent = pnlQuickActions;
            Follow.Location = new Point(104, 0);
            Follow.Size = new Size(98, 34);
            Follow.Text = "Follow";
            Follow.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            Follow.BackColor = Color.FromArgb(99, 102, 241);
            Follow.ForeColor = Color.White;
            Follow.FlatStyle = FlatStyle.Flat;
            Follow.FlatAppearance.BorderSize = 0;
            Follow.Cursor = Cursors.Hand;

            SetAlias.Parent = pnlQuickActions;
            SetAlias.Location = new Point(208, 0);
            SetAlias.Size = new Size(98, 34);
            SetAlias.Text = "Set Alias";
            SetAlias.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            SetAlias.BackColor = Color.FromArgb(30, 41, 59);
            SetAlias.ForeColor = Color.White;
            SetAlias.FlatStyle = FlatStyle.Flat;
            SetAlias.FlatAppearance.BorderSize = 0;
            SetAlias.Cursor = Cursors.Hand;

            // 4b. Follow target input
            Panel pnlFollowTarget = new Panel
            {
                Location = new Point(innerX, 236),
                Size = new Size(innerW, 34),
                BackColor = Color.Transparent
            };

            lblFollowTarget = new Label
            {
                Text = "Username or User ID to follow",
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 7.5f),
                Location = new Point(0, 0),
                AutoSize = true
            };

            UserID.Parent = pnlFollowTarget;
            UserID.Location = new Point(0, 14);
            UserID.Size = new Size(innerW, 20);
            UserID.BackColor = Color.FromArgb(19, 27, 42);
            UserID.ForeColor = Color.White;
            UserID.BorderColor = Color.FromArgb(37, 99, 235);
            UserID.Font = new Font("Segoe UI", 8.5f);

            pnlFollowTarget.Controls.Add(lblFollowTarget);
            pnlFollowTarget.Controls.Add(UserID);

            // 5. Account Information Section
            Panel pnlDetails = new Panel
            {
                Location = new Point(innerX, 276),
                Size = new Size(innerW, 146),
                BackColor = Color.FromArgb(19, 27, 42)
            };
            pnlDetails.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(30, 41, 59));
                using GraphicsPath path = ModernAccountCard.GetRoundedRectangle(new Rectangle(0, 0, pnlDetails.Width - 1, pnlDetails.Height - 1), 8);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(p, path);
            };

            lblDetailsHeader = new Label
            {
                Text = "Account Details",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 8),
                AutoSize = true
            };
            pnlDetails.Controls.Add(lblDetailsHeader);

            // Detail rows with clean text action buttons (no emojis)
            rightValUsername = AddDetailRow(pnlDetails, "Username", "-", 32, true, () => Clipboard.SetText(rightValUsername.Text));
            rightValAlias = AddEditableDetailRow(pnlDetails, "Alias", "-", 58, Alias, SetAlias);
            rightValUserId = AddDetailRow(pnlDetails, "User ID", "-", 84, true, () => Clipboard.SetText(rightValUserId.Text));
            rightValDescription = AddEditableDetailRow(pnlDetails, "Description", "-", 110, DescriptionBox, SetDescription);

            // 6. Advanced Settings Section (Includes Language Switcher)
            Panel pnlAdvSettings = new Panel
            {
                Location = new Point(innerX, 428),
                Size = new Size(innerW, 142),
                BackColor = Color.FromArgb(19, 27, 42)
            };
            pnlAdvSettings.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(30, 41, 59));
                using GraphicsPath path = ModernAccountCard.GetRoundedRectangle(new Rectangle(0, 0, pnlAdvSettings.Width - 1, pnlAdvSettings.Height - 1), 8);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(p, path);
            };

            lblAdvHeader = new Label
            {
                Text = "Advanced Settings",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 8),
                AutoSize = true
            };
            pnlAdvSettings.Controls.Add(lblAdvHeader);

            EditTheme.Parent = pnlAdvSettings;
            EditTheme.Location = new Point(12, 30);
            EditTheme.Size = new Size(282, 30);
            EditTheme.Text = "Edit Theme";
            EditTheme.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            EditTheme.BackColor = Color.FromArgb(30, 41, 59);
            EditTheme.ForeColor = Color.FromArgb(203, 213, 225);
            EditTheme.FlatStyle = FlatStyle.Flat;
            EditTheme.FlatAppearance.BorderSize = 0;
            EditTheme.TextAlign = ContentAlignment.MiddleCenter;
            EditTheme.Cursor = Cursors.Hand;

            LaunchNexus.Parent = pnlAdvSettings;
            LaunchNexus.Location = new Point(12, 66);
            LaunchNexus.Size = new Size(282, 30);
            LaunchNexus.Text = "Account Control";
            LaunchNexus.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            LaunchNexus.BackColor = Color.FromArgb(30, 41, 59);
            LaunchNexus.ForeColor = Color.FromArgb(203, 213, 225);
            LaunchNexus.FlatStyle = FlatStyle.Flat;
            LaunchNexus.FlatAppearance.BorderSize = 0;
            LaunchNexus.TextAlign = ContentAlignment.MiddleCenter;
            LaunchNexus.Cursor = Cursors.Hand;

            // Language switcher button inside Advanced Settings
            btnChangeLanguage = new Button
            {
                Parent = pnlAdvSettings,
                Location = new Point(12, 102),
                Size = new Size(282, 30),
                Text = IsThai ? "Language: Thai" : "Language: English",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.FromArgb(203, 213, 225),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnChangeLanguage.FlatAppearance.BorderSize = 0;

            ContextMenuStrip langStrip = new ContextMenuStrip();
            btnChangeLanguage.Click += (s, e) =>
            {
                bool th = IsThai;
                langStrip.Items.Clear();
                langStrip.Items.Add(new ToolStripMenuItem("English (Default)", null, (s2, e2) => SetLanguage("en")) { Checked = !th });
                langStrip.Items.Add(new ToolStripMenuItem("Thai", null, (s2, e2) => SetLanguage("th")) { Checked = th });
                langStrip.Show(btnChangeLanguage, new Point(0, btnChangeLanguage.Height));
            };

            // 7. Status Bar Bottom Right
            statusBarLabel = new Label
            {
                Location = new Point(innerX, 576),
                Size = new Size(innerW, 24),
                Text = "Ready / 0 accounts",
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
                Size = new Size(130, 18),
                AutoEllipsis = true
            };

            Button btnAction = new Button
            {
                Text = isCopy ? "Copy" : "Edit",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(42, 22),
                Location = new Point(246, y - 2),
                Cursor = Cursors.Hand
            };
            btnAction.FlatAppearance.BorderSize = 0;
            btnAction.Click += (s, e) => onClick?.Invoke();

            parent.Controls.Add(lblKey);
            parent.Controls.Add(lblVal);
            parent.Controls.Add(btnAction);

            return lblVal;
        }

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
                Size = new Size(130, 18),
                AutoEllipsis = true
            };

            TextBox editBox = new TextBox
            {
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(110, y - 2),
                Size = new Size(130, 20),
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
                legacyCommitButton.PerformClick();

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
                Text = "Edit",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(42, 22),
                Location = new Point(246, y - 2),
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
                Location = new Point(0, 7),
                Size = new Size(220, 28),
                BackColor = Color.FromArgb(19, 27, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle
            };
            searchTextBox.TextChanged += (s, e) => RefreshModernCards();

            lblSearchPlaceholder = new Label
            {
                Text = "Search account or alias...",
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Segoe UI", 9f),
                Location = new Point(6, 11),
                AutoSize = true,
                BackColor = Color.FromArgb(19, 27, 42),
                Cursor = Cursors.IBeam
            };
            lblSearchPlaceholder.Click += (s, e) => searchTextBox.Focus();
            searchTextBox.Enter += (s, e) => lblSearchPlaceholder.Visible = false;
            searchTextBox.Leave += (s, e) => lblSearchPlaceholder.Visible = string.IsNullOrEmpty(searchTextBox.Text);

            lblSort = new Label
            {
                Text = "Sort by:",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true
            };

            sortComboBox = new ComboBox
            {
                Size = new Size(125, 26),
                BackColor = Color.FromArgb(19, 27, 42),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.5f),
                FlatStyle = FlatStyle.Flat
            };
            sortComboBox.SelectedIndexChanged += (s, e) => RefreshModernCards();

            // Function 1: Select Mode button (Toggles manual click-to-select mode)
            btnSelectMode = new Button
            {
                Text = "Select Mode",
                ForeColor = Color.FromArgb(203, 213, 225),
                BackColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(130, 28),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSelectMode.FlatAppearance.BorderSize = 0;
            btnSelectMode.Click += (s, e) =>
            {
                isSelectMode = !isSelectMode;
                UpdateSelectModeDisplay();
                UpdateCardsSelectionState();
            };

            // Function 2: Select All button (Selects all accounts at once)
            btnSelectAll = new Button
            {
                Text = "Select All",
                ForeColor = Color.FromArgb(203, 213, 225),
                BackColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(88, 28),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSelectAll.FlatAppearance.BorderSize = 0;
            btnSelectAll.Click += (s, e) =>
            {
                bool allSelected = SelectedAccounts != null && SelectedAccounts.Count == AccountsList.Count && AccountsList.Count > 0;
                if (allSelected)
                {
                    SelectedAccounts = new List<Account>();
                    SelectedAccount = null;
                }
                else
                {
                    SelectedAccounts = new List<Account>(AccountsList);
                    if (AccountsList.Count > 0) SelectedAccount = AccountsList[0];
                }

                try { AccountsView.SelectedObjects = SelectedAccounts; } catch { }
                UpdateCardsSelectionState();
            };

            Action layoutTopBar = () =>
            {
                int w = pnlTopBar.ClientSize.Width;
                if (w <= 0) return;
                sortComboBox.Location = new Point(w - sortComboBox.Width - 4, 7);
                lblSort.Location = new Point(sortComboBox.Left - lblSort.Width - 8, 11);
                btnSelectAll.Location = new Point(lblSort.Left - btnSelectAll.Width - 8, 7);
                btnSelectMode.Location = new Point(btnSelectAll.Left - btnSelectMode.Width - 8, 7);

                int avail = btnSelectMode.Left - 16;
                int searchW = Math.Max(120, Math.Min(240, avail));
                searchTextBox.Location = new Point(0, 7);
                searchTextBox.Width = searchW;
                lblSearchPlaceholder.Location = new Point(searchTextBox.Left + 6, searchTextBox.Top + 4);
            };

            pnlTopBar.Resize += (s, e) => layoutTopBar();

            pnlTopBar.Controls.Add(lblSearchPlaceholder);
            pnlTopBar.Controls.Add(searchTextBox);
            pnlTopBar.Controls.Add(btnSelectMode);
            pnlTopBar.Controls.Add(btnSelectAll);
            pnlTopBar.Controls.Add(lblSort);
            pnlTopBar.Controls.Add(sortComboBox);
            layoutTopBar();


            // 2. Bottom Action Bar (4 Buttons) — Clean, no emojis
            Panel pnlBottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };

            // Button 1: Add Account
            Add.Parent = pnlBottomBar;
            Add.Location = new Point(0, 8);
            Add.Size = new Size(135, 42);
            Add.Text = "Add Account";
            Add.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            Add.BackColor = Color.FromArgb(16, 185, 129);
            Add.ForeColor = Color.White;
            Add.FlatStyle = FlatStyle.Flat;
            Add.FlatAppearance.BorderSize = 0;
            Add.Cursor = Cursors.Hand;

            BuildAddAccountsMenu();

            Add.Click -= Add_Click;
            Add.Click += (s, e) =>
            {
                if (AddAccountsStrip != null)
                {
                    AddAccountsStrip.Show(Add, new Point(0, Add.Height));
                }
            };

            // Button 2: Remove Selected
            Remove.Parent = pnlBottomBar;
            Remove.Location = new Point(145, 8);
            Remove.Size = new Size(130, 42);
            Remove.Text = "Remove Selected";
            Remove.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            Remove.BackColor = Color.FromArgb(239, 68, 68);
            Remove.ForeColor = Color.White;
            Remove.FlatStyle = FlatStyle.Flat;
            Remove.FlatAppearance.BorderSize = 0;
            Remove.Cursor = Cursors.Hand;

            // Button 3: Toggle Hide Usernames
            bool hideUserInitial = HideUsernamesCheckbox != null && HideUsernamesCheckbox.Checked;
            btnToggleHideUser = new Button
            {
                Text = hideUserInitial ? "Show Usernames" : "Hide Usernames",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 42),
                Location = new Point(285, 8),
                Cursor = Cursors.Hand
            };
            btnToggleHideUser.FlatAppearance.BorderSize = 0;
            btnToggleHideUser.Click += (s, e) =>
            {
                HideUsernamesCheckbox.Checked = !HideUsernamesCheckbox.Checked;
                IniSettings.Save("RAMSettings.ini");
                bool h = HideUsernamesCheckbox.Checked;
                btnToggleHideUser.Text = h ? "Show Usernames" : "Hide Usernames";
                RefreshModernCards();
            };
            btnToggleHideUser.Parent = pnlBottomBar;

            // Button 4: Open Browser
            OpenBrowser.Parent = pnlBottomBar;
            OpenBrowser.Location = new Point(425, 8);
            OpenBrowser.Size = new Size(130, 42);
            OpenBrowser.Text = "Open Browser";
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

        private void UpdateSelectModeDisplay()
        {
            if (btnSelectMode == null) return;
            if (isSelectMode)
            {
                btnSelectMode.BackColor = Color.FromArgb(37, 99, 235);
                btnSelectMode.ForeColor = Color.White;
                btnSelectMode.Text = "Exit Select Mode";
            }
            else
            {
                btnSelectMode.BackColor = Color.FromArgb(30, 41, 59);
                btnSelectMode.ForeColor = Color.FromArgb(203, 213, 225);
                btnSelectMode.Text = "Select Mode";
            }
        }

        private void BuildAddAccountsMenu()
        {
            if (AddAccountsStrip == null) return;

            AddAccountsStrip.Items.Clear();

            ToolStripMenuItem itemBrowser = new ToolStripMenuItem("Web Browser Login", null, (s, e) => Add_Click(s, e));
            ToolStripMenuItem itemCookie = new ToolStripMenuItem("From Cookie(s)", null, (s, e) => byCookieToolStripMenuItem_Click(s, e));
            ToolStripMenuItem itemManual = new ToolStripMenuItem("Manual Login", null, (s, e) => manualToolStripMenuItem_Click(s, e));
            ToolStripMenuItem itemBulk = new ToolStripMenuItem("User:Pass", null, (s, e) => bulkUserPassToolStripMenuItem_Click(s, e));
            ToolStripMenuItem itemCustom = new ToolStripMenuItem("Custom (URL + JS)", null, (s, e) => customURLJSToolStripMenuItem_Click(s, e));

            AddAccountsStrip.Items.AddRange(new ToolStripItem[] {
                itemBrowser,
                new ToolStripSeparator(),
                itemCookie,
                itemManual,
                itemBulk,
                itemCustom
            });

            Add.Menu = AddAccountsStrip;
        }

        public void SetLanguage(string lang)
        {
            General.Set("Language", lang);
            IniSettings.Save("RAMSettings.ini");
            ModernAccountCard.IsThai = (lang == "th");
            ApplyLocalization();
            RefreshModernCards();
        }

        private void ApplyLocalization()
        {
            bool th = IsThai;
            ModernAccountCard.IsThai = th;

            // Sidebar
            if (btnNavAll != null) btnNavAll.Text = th ? "All Accounts" : "All Accounts";
            if (btnNavProfiles != null) btnNavProfiles.Text = th ? "Profiles" : "Profiles";
            if (btnNavSettings != null) btnNavSettings.Text = th ? "Settings" : "Settings";
            if (btnNavHelp != null) btnNavHelp.Text = th ? "Help" : "Help";
            if (lblReadyDot != null) lblReadyDot.Text = th ? "Ready" : "Ready";

            // Center Top Bar
            if (lblSearchPlaceholder != null) lblSearchPlaceholder.Text = "Search account or alias...";
            UpdateSelectModeDisplay();
            if (btnSelectAll != null) btnSelectAll.Text = "Select All";
            if (lblSort != null) lblSort.Text = "Sort by:";
            if (sortComboBox != null)
            {
                int prevIdx = sortComboBox.SelectedIndex;
                sortComboBox.Items.Clear();
                sortComboBox.Items.AddRange(new object[] { "Name (A-Z)", "Name (Z-A)", "Group", "Last Used" });
                sortComboBox.SelectedIndex = prevIdx >= 0 && prevIdx < sortComboBox.Items.Count ? prevIdx : 0;
            }

            // Center Bottom Bar
            if (Add != null) Add.Text = "Add Account";
            BuildAddAccountsMenu();

            if (Remove != null) Remove.Text = "Remove Selected";
            bool hideUser = HideUsernamesCheckbox != null && HideUsernamesCheckbox.Checked;
            if (btnToggleHideUser != null)
            {
                btnToggleHideUser.Text = hideUser ? "Show Usernames" : "Hide Usernames";
            }
            if (OpenBrowser != null) OpenBrowser.Text = "Open Browser";

            // Right Panel
            if (lblPlaceId != null) lblPlaceId.Text = "Place ID";
            if (lblJobId != null) lblJobId.Text = "Job ID / Private Server";
            if (lblFollowTarget != null) lblFollowTarget.Text = "Username or User ID to follow";
            if (lblDetailsHeader != null) lblDetailsHeader.Text = "Account Details";
            if (lblAdvHeader != null) lblAdvHeader.Text = "Advanced Settings";
            if (ServerList != null) ServerList.Text = "Utilities";
            if (Follow != null) Follow.Text = "Follow";
            if (SetAlias != null) SetAlias.Text = "Set Alias";
            if (btnChangeLanguage != null) btnChangeLanguage.Text = th ? "Language: Thai" : "Language: English";

            UpdateJoinButtonText();
            if (SelectedAccount != null)
                UpdateRightPanelDetails(SelectedAccount);
            else
            {
                if (rightUsernameLabel != null) rightUsernameLabel.Text = "Select an account...";
                if (statusBarLabel != null) statusBarLabel.Text = $"Ready / {AccountsList?.Count ?? 0} accounts";
            }
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

                    card.SelectionToggled += (s, isChecked) =>
                    {
                        if (SelectedAccounts == null) SelectedAccounts = new List<Account>();
                        if (isChecked)
                        {
                            if (!SelectedAccounts.Contains(acc)) SelectedAccounts.Add(acc);
                            SelectedAccount = acc;
                        }
                        else
                        {
                            SelectedAccounts.Remove(acc);
                            if (SelectedAccount == acc)
                                SelectedAccount = SelectedAccounts.Count > 0 ? SelectedAccounts[SelectedAccounts.Count - 1] : null;
                        }
                        try { AccountsView.SelectedObjects = SelectedAccounts; } catch { }
                        UpdateCardsSelectionState();
                    };

                    // Handle card clicks:
                    // In Select Mode (or holding Ctrl): clicking toggles the card in the multi-selection.
                    // In Normal Mode: clicking selects that single card.
                    card.CardClicked += (s, clickedAcc) =>
                    {
                        if (SelectedAccounts == null) SelectedAccounts = new List<Account>();
                        bool isCtrl = (ModifierKeys & Keys.Control) == Keys.Control;

                        if (isSelectMode || isCtrl)
                        {
                            if (SelectedAccounts.Contains(clickedAcc))
                            {
                                SelectedAccounts.Remove(clickedAcc);
                                if (SelectedAccount == clickedAcc)
                                    SelectedAccount = SelectedAccounts.Count > 0 ? SelectedAccounts[SelectedAccounts.Count - 1] : null;
                            }
                            else
                            {
                                SelectedAccounts.Add(clickedAcc);
                                SelectedAccount = clickedAcc;
                            }
                        }
                        else
                        {
                            SelectedAccounts = new List<Account> { clickedAcc };
                            SelectedAccount = clickedAcc;
                        }

                        try { AccountsView.SelectedObjects = SelectedAccounts; } catch { }
                        UpdateCardsSelectionState();
                    };

                    card.PlayClicked += async (s, playAcc) =>
                    {
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
                int totalAccs = AccountsList.Count;
                if (navBadgeLabel != null) navBadgeLabel.Text = totalAccs.ToString();
                if (statusBarLabel != null)
                {
                    statusBarLabel.Text = $"Ready / {totalAccs} accounts";
                }

                // Restore selection or pick first account
                if (SelectedAccount == null && AccountsList.Count > 0)
                {
                    SelectedAccount = AccountsList[0];
                    SelectedAccounts = new List<Account> { SelectedAccount };
                    try { AccountsView.SelectedObjects = SelectedAccounts; } catch { }
                    UpdateCardsSelectionState();
                    UpdateRightPanelDetails(SelectedAccount);
                }
                else if (SelectedAccount != null)
                {
                    UpdateRightPanelDetails(SelectedAccount);
                    UpdateCardsSelectionState();
                }
                else
                {
                    if (rightUsernameLabel != null) rightUsernameLabel.Text = "Select an account...";
                    if (rightAliasLabel != null) rightAliasLabel.Text = "Alias: -";
                    if (rightIdLabel != null) rightIdLabel.Text = "ID: -";
                    if (rightValUsername != null) rightValUsername.Text = "-";
                    if (rightValAlias != null) rightValAlias.Text = "-";
                    if (rightValUserId != null) rightValUserId.Text = "-";
                    if (rightValDescription != null) rightValDescription.Text = "-";
                    if (rightAvatarBox != null) rightAvatarBox.Image = null;
                }

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
                bool isSel = (SelectedAccount == card.Account || (SelectedAccounts != null && SelectedAccounts.Contains(card.Account)));
                card.IsSelected = isSel;
            }
            UpdateJoinButtonText();
            UpdateRightPanelDetails(SelectedAccount);
        }

        private void UpdateJoinButtonText()
        {
            if (JoinServer == null) return;
            int count = SelectedAccounts != null ? SelectedAccounts.Count : 0;
            if (count > 1)
            {
                JoinServer.Text = $"Join Server ({count} Accounts)";
            }
            else
            {
                JoinServer.Text = "Join Server";
            }
            JoinServer.BackColor = Color.FromArgb(37, 99, 235); // Signature vibrant blue #2563EB
        }

        public void UpdateRightPanelDetails(Account account)
        {
            this.InvokeIfRequired(() =>
            {
                int selCount = SelectedAccounts != null ? SelectedAccounts.Count : (account != null ? 1 : 0);
                if (selCount > 1)
                {
                    UpdateRightPanelMultiSelection(SelectedAccounts);
                    return;
                }

                if (account == null && selCount == 1 && SelectedAccounts != null && SelectedAccounts.Count > 0)
                {
                    account = SelectedAccounts[0];
                }

                if (account == null)
                {
                    ClearRightPanelDetails();
                    return;
                }

                UpdateRightPanelSingleAccount(account);
            });
        }

        private void UpdateRightPanelMultiSelection(List<Account> accounts)
        {
            int count = accounts.Count;
            bool hideUser = General != null && General.Exists("HideUsernames") && General.Get<bool>("HideUsernames");

            if (rightUsernameLabel != null)
            {
                rightUsernameLabel.Text = $"{count} Accounts Selected";
                rightUsernameLabel.ForeColor = Color.FromArgb(96, 165, 250); // Vibrant light blue
            }

            if (rightStatusBadge != null)
            {
                rightStatusBadge.Text = $"{count} Selected";
                rightStatusBadge.BackColor = Color.FromArgb(37, 99, 235);
                rightStatusBadge.ForeColor = Color.White;
                if (rightStatusBadge.Parent != null)
                {
                    rightStatusBadge.Location = new Point(rightStatusBadge.Parent.Width - rightStatusBadge.Width - 10, 12);
                }
            }

            if (rightUsernameLabel != null && rightStatusBadge != null)
            {
                rightUsernameLabel.MaximumSize = new Size(Math.Max(80, rightStatusBadge.Left - rightUsernameLabel.Left - 6), 22);
                rightUsernameLabel.AutoEllipsis = true;
            }

            string names = string.Join(", ", accounts.Select(a => hideUser ? "••••••••" : a.Username));
            if (rightAliasLabel != null)
            {
                rightAliasLabel.Text = names;
                rightAliasLabel.ForeColor = Color.FromArgb(226, 232, 240);
                if (rightAliasLabel.Parent != null)
                {
                    rightAliasLabel.MaximumSize = new Size(rightAliasLabel.Parent.Width - rightAliasLabel.Left - 10, 18);
                    rightAliasLabel.AutoEllipsis = true;
                }
            }

            if (rightIdLabel != null)
            {
                rightIdLabel.Text = $"Multi-Roblox Ready ({count} active)";
                rightIdLabel.ForeColor = Color.FromArgb(52, 211, 153);
            }

            if (rightValUsername != null) rightValUsername.Text = $"{count} Accounts Selected";
            if (rightValAlias != null) rightValAlias.Text = names;
            if (rightValUserId != null) rightValUserId.Text = string.Join(", ", accounts.Select(a => a.UserID.ToString()));
            if (rightValDescription != null) rightValDescription.Text = $"Multi-Roblox launch ready for {count} accounts";

            // Trigger avatar loading for top selected accounts if not already cached
            foreach (var a in accounts.Take(2))
            {
                if (a != null && a.UserID > 0)
                {
                    bool cached = false;
                    lock (ModernAccountCard.AvatarCache)
                    {
                        cached = ModernAccountCard.AvatarCache.ContainsKey(a.UserID);
                    }
                    if (!cached) LoadAvatarForAccount(a);
                }
            }

            if (rightAvatarBox != null)
            {
                rightAvatarBox.Image = null; // Forces paint handler to draw multi-avatar badge
                rightAvatarBox.Invalidate();
            }

            if (statusBarLabel != null)
            {
                int total = AccountsList?.Count ?? 0;
                statusBarLabel.Text = $"Selected {count} / {total} accounts";
            }
        }

        private void UpdateRightPanelSingleAccount(Account account)
        {
            bool hideUser = General.Get<bool>("HideUsernames");
            string uname = hideUser ? "••••••••" : account.Username;

            if (rightUsernameLabel != null)
            {
                rightUsernameLabel.Text = uname;
                rightUsernameLabel.ForeColor = Color.White;
            }
            if (rightAliasLabel != null)
            {
                rightAliasLabel.Text = "Alias: " + (!string.IsNullOrEmpty(account.Alias) ? account.Alias : "-");
                rightAliasLabel.ForeColor = Color.FromArgb(148, 163, 184);
                if (rightAliasLabel.Parent != null)
                {
                    rightAliasLabel.MaximumSize = new Size(rightAliasLabel.Parent.Width - rightAliasLabel.Left - 10, 18);
                    rightAliasLabel.AutoEllipsis = true;
                }
            }
            if (rightIdLabel != null)
            {
                rightIdLabel.Text = "ID: " + (account.UserID > 0 ? account.UserID.ToString() : "-");
                rightIdLabel.ForeColor = Color.FromArgb(100, 116, 139);
            }

            bool isOnline = (account.Presence != null && account.Presence.userPresenceType != UserPresenceType.Offline);
            if (rightStatusBadge != null)
            {
                rightStatusBadge.Text = isOnline ? "Online" : "Offline";
                rightStatusBadge.BackColor = isOnline ? Color.FromArgb(6, 78, 59) : Color.FromArgb(30, 41, 59);
                rightStatusBadge.ForeColor = isOnline ? Color.FromArgb(52, 211, 153) : Color.FromArgb(148, 163, 184);
                if (rightStatusBadge.Parent != null)
                {
                    rightStatusBadge.Location = new Point(rightStatusBadge.Parent.Width - rightStatusBadge.Width - 10, 12);
                }
            }

            if (rightUsernameLabel != null && rightStatusBadge != null)
            {
                rightUsernameLabel.MaximumSize = new Size(Math.Max(80, rightStatusBadge.Left - rightUsernameLabel.Left - 6), 22);
                rightUsernameLabel.AutoEllipsis = true;
            }

            if (rightValUsername != null) rightValUsername.Text = uname;
            if (rightValAlias != null) rightValAlias.Text = !string.IsNullOrEmpty(account.Alias) ? account.Alias : "-";
            if (rightValUserId != null) rightValUserId.Text = account.UserID > 0 ? account.UserID.ToString() : "-";
            if (rightValDescription != null) rightValDescription.Text = !string.IsNullOrEmpty(account.Description) ? account.Description : "-";

            // Update Avatar Image with async loading & fallback
            if (rightAvatarBox != null)
            {
                bool found = false;
                lock (ModernAccountCard.AvatarCache)
                {
                    if (ModernAccountCard.AvatarCache.TryGetValue(account.UserID, out Image cached))
                    {
                        rightAvatarBox.Image = cached;
                        found = true;
                    }
                }

                if (!found)
                {
                    rightAvatarBox.Image = null;
                    rightAvatarBox.Invalidate();
                    LoadAvatarForAccount(account);
                }
                else
                {
                    rightAvatarBox.Invalidate();
                }
            }

            // Load saved PlaceID & JobID if exists
            if (!string.IsNullOrEmpty(account.GetField("SavedPlaceId")) && PlaceID != null)
                PlaceID.Text = account.GetField("SavedPlaceId");

            if (!string.IsNullOrEmpty(account.GetField("SavedJobId")) && JobID != null)
                JobID.Text = account.GetField("SavedJobId");

            if (statusBarLabel != null)
            {
                int total = AccountsList?.Count ?? 0;
                statusBarLabel.Text = $"Ready / {total} accounts";
            }
        }

        private void ClearRightPanelDetails()
        {
            if (rightUsernameLabel != null)
            {
                rightUsernameLabel.Text = IsThai ? "เลือกบัญชี..." : "Select an account...";
                rightUsernameLabel.ForeColor = Color.White;
            }
            if (rightStatusBadge != null)
            {
                rightStatusBadge.Text = "Offline";
                rightStatusBadge.BackColor = Color.FromArgb(30, 41, 59);
                rightStatusBadge.ForeColor = Color.FromArgb(148, 163, 184);
                if (rightStatusBadge.Parent != null)
                    rightStatusBadge.Location = new Point(rightStatusBadge.Parent.Width - rightStatusBadge.Width - 10, 12);
            }
            if (rightAliasLabel != null)
            {
                rightAliasLabel.Text = "Alias: -";
                rightAliasLabel.ForeColor = Color.FromArgb(148, 163, 184);
            }
            if (rightIdLabel != null)
            {
                rightIdLabel.Text = "ID: -";
                rightIdLabel.ForeColor = Color.FromArgb(100, 116, 139);
            }
            if (rightValUsername != null) rightValUsername.Text = "-";
            if (rightValAlias != null) rightValAlias.Text = "-";
            if (rightValUserId != null) rightValUserId.Text = "-";
            if (rightValDescription != null) rightValDescription.Text = "-";
            if (rightAvatarBox != null)
            {
                rightAvatarBox.Image = null;
                rightAvatarBox.Invalidate();
            }
            if (statusBarLabel != null)
            {
                int total = AccountsList?.Count ?? 0;
                statusBarLabel.Text = $"Ready / {total} accounts";
            }
        }

        private void LoadAvatarForAccount(Account acc)
        {
            if (acc == null || acc.UserID <= 0) return;
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    string url = null;
                    try { url = await Batch.GetImage(acc.UserID, "AvatarHeadShot", "150x150"); } catch { }

                    if (string.IsNullOrEmpty(url) || url == "UNK" || !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                            {
                                client.Timeout = TimeSpan.FromSeconds(6);
                                string api = $"https://thumbnails.roblox.com/v1/users/avatar-headshot?userIds={acc.UserID}&size=150x150&format=Png&isCircular=false";
                                string json = await client.GetStringAsync(api);
                                int idx = json.IndexOf("\"imageUrl\":\"", StringComparison.OrdinalIgnoreCase);
                                if (idx >= 0)
                                {
                                    int start = idx + 12;
                                    int end = json.IndexOf("\"", start);
                                    if (end > start) url = json.Substring(start, end - start);
                                }
                            }
                        }
                        catch { }
                    }

                    if (!string.IsNullOrEmpty(url) && url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(8);
                            byte[] data = await client.GetByteArrayAsync(url);
                            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(data))
                            {
                                using (Image temp = Image.FromStream(ms))
                                {
                                    Bitmap cloned = new Bitmap(temp);
                                    lock (ModernAccountCard.AvatarCache)
                                    {
                                        ModernAccountCard.AvatarCache[acc.UserID] = cloned;
                                    }
                                    this.InvokeIfRequired(() =>
                                    {
                                        if (rightAvatarBox != null && !rightAvatarBox.IsDisposed)
                                        {
                                            int count = SelectedAccounts != null ? SelectedAccounts.Count : 0;
                                            if (count <= 1 && SelectedAccount == acc)
                                            {
                                                rightAvatarBox.Image = cloned;
                                            }
                                            rightAvatarBox.Invalidate();
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                catch { }
            });
        }
    }
}

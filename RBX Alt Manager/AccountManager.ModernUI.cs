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
        private Panel leftNavPanel;
        private Panel centerPanel;
        private Panel rightDetailsPanel;
        private FlowLayoutPanel cardsContainer;
        private TextBox         searchTextBox;
        private ComboBox        sortComboBox;
        private Label           navBadgeLabel;
        private Button          btnToggleHideUser;
        private PictureBox rightAvatarBox;
        private Label      rightUsernameLabel;
        private Label      rightStatusBadge;
        private Label      rightAliasLabel;
        private Label      rightIdLabel;
        private Label      rightValUsername;
        private Label      rightValAlias;
        private Label      rightValUserId;
        private Label      rightValDescription;
        private Label      statusBarLabel;
        internal List<ModernAccountCard> modernCards = new List<ModernAccountCard>();

        private static readonly Color C_BG_DEEP  = Color.FromArgb(10, 14, 26);
        private static readonly Color C_BG_BASE  = Color.FromArgb(17, 24, 39);
        private static readonly Color C_BG_PANEL = Color.FromArgb(19, 27, 45);
        private static readonly Color C_BORDER   = Color.FromArgb(31, 41, 55);
        private static readonly Color C_ACCENT   = Color.FromArgb(37, 99, 235);
        private static readonly Color C_GREEN    = Color.FromArgb(16, 185, 129);
        private static readonly Color C_RED      = Color.FromArgb(239, 68, 68);
        private static readonly Color C_VIOLET   = Color.FromArgb(99, 102, 241);
        private static readonly Color C_TEXT_LO  = Color.FromArgb(100, 116, 139);
        private static readonly Color C_TEXT_MID = Color.FromArgb(148, 163, 184);
        private static readonly Color C_TEXT_HI  = Color.FromArgb(248, 250, 252);
        private static readonly Color C_BTN_GREY = Color.FromArgb(30, 41, 59);

        public void SetupModernDashboardLayout()
        {
            try
            {
                this.SuspendLayout();
                this.Size        = new Size(1160, 700);
                this.MinimumSize = new Size(980, 580);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.BackColor   = C_BG_BASE;
                this.ForeColor   = C_TEXT_HI;
                this.Text        = "Multi-Roblox Account Manager  v4.1";
                AccountsView.Visible  = false;
                AccountsView.Dock     = DockStyle.None;
                AccountsView.Size     = new Size(10, 10);
                AccountsView.Location = new Point(-200, -200);
                Control[] hideUs = new Control[] {
                    LabelPlaceID, LabelJobID, LabelUserID, CurrentPlace,
                    ShuffleIcon, HistoryIcon, SaveToAccount, ConfigButton,
                    DonateButton, JoinDiscord, HideUsernamesCheckbox,
                    Alias, DescriptionBox, SetDescription, BrowserButton };
                foreach (Control c in hideUs) { if (c != null) c.Visible = false; }
                BuildLeftNavigationPanel();
                BuildRightDetailsPanel();
                BuildCenterContentPanel();
                leftNavPanel?.SendToBack();
                rightDetailsPanel?.SendToBack();
                centerPanel?.BringToFront();
                this.ResumeLayout(true);
                RefreshModernCards();
            }
            catch (Exception ex) { Program.Logger.Error("SetupModernDashboardLayout: " + ex); }
        }

        private void BuildLeftNavigationPanel()
        {
            leftNavPanel = new Panel { Dock = DockStyle.Left, Width = 192, BackColor = C_BG_DEEP };

            Panel pnlBrand = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.Transparent };
            Label lblLogo  = new Label { Text = "[R]", Font = new Font("Segoe UI", 17f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248), Location = new Point(16, 14), AutoSize = true };
            Label lblTitle = new Label { Text = "Multi-Roblox", Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = C_TEXT_HI, Location = new Point(44, 13), AutoSize = true };
            Label lblSub   = new Label { Text = "Account Manager", Font = new Font("Segoe UI", 7.5f),
                ForeColor = C_TEXT_MID, Location = new Point(45, 30), AutoSize = true };
            Label lblVer   = new Label { Text = " v4.1 ", Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248), BackColor = Color.FromArgb(12, 45, 80),
                Location = new Point(45, 46), AutoSize = true, Padding = new Padding(3, 1, 3, 1) };
            pnlBrand.Controls.AddRange(new Control[] { lblLogo, lblTitle, lblSub, lblVer });

            Panel sep1 = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(31, 41, 55) };

            Panel pnlNav = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(10, 12, 10, 8) };
            Panel pnlNavAll = _MakeNavItem("  Accounts", true, 0);
            navBadgeLabel = new Label { Text = "0", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = C_TEXT_HI, BackColor = C_ACCENT, Size = new Size(28, 18),
                Location = new Point(132, 9), TextAlign = ContentAlignment.MiddleCenter };
            pnlNavAll.Controls.Add(navBadgeLabel);

            Panel pnlNavProfile  = _MakeNavItem("  Profiles",  false, 44);
            Panel pnlNavSettings = _MakeNavItem("  Settings",  false, 88);
            Panel pnlNavHelp     = _MakeNavItem("  Help",      false, 132);
            pnlNavProfile.Click  += (s, e) => MessageBox.Show("Profile groups", "Profiles", MessageBoxButtons.OK, MessageBoxIcon.Information);
            pnlNavSettings.Click += (s, e) => ConfigButton_Click(s, e);
            pnlNavHelp.Click     += (s, e) => infoToolStripMenuItem1_Click(s, e);
            pnlNav.Controls.AddRange(new Control[] { pnlNavAll, pnlNavProfile, pnlNavSettings, pnlNavHelp });

            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = C_BG_DEEP };
            Panel sep2 = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(31, 41, 55) };
            Label lblReady    = new Label { Text = "Ready", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = C_GREEN, Location = new Point(14, 12), AutoSize = true };
            Label lblReadySub = new Label { Text = "Roblox Account Manager", Font = new Font("Segoe UI", 7.5f),
                ForeColor = C_TEXT_LO, Location = new Point(16, 28), AutoSize = true };
            pnlBottom.Controls.AddRange(new Control[] { sep2, lblReady, lblReadySub });

            leftNavPanel.Controls.Add(pnlNav);
            leftNavPanel.Controls.Add(pnlBottom);
            leftNavPanel.Controls.Add(sep1);
            leftNavPanel.Controls.Add(pnlBrand);
            this.Controls.Add(leftNavPanel);
        }

        private Panel _MakeNavItem(string text, bool active, int y)
        {
            Panel row = new Panel {
                Location  = new Point(0, y),
                Size      = new Size(170, 36),
                BackColor = active ? Color.FromArgb(20, 50, 100) : Color.Transparent,
                Cursor    = Cursors.Hand
            };
            if (active) {
                Panel stripe = new Panel { Dock = DockStyle.Left, Width = 3, BackColor = C_ACCENT };
                row.Controls.Add(stripe);
            }
            Label lbl = new Label {
                Text      = text,
                Font      = new Font("Segoe UI", 9.5f, active ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = active ? C_TEXT_HI : C_TEXT_MID,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(active ? 10 : 14, 0, 0, 0)
            };
            row.Controls.Add(lbl);
            row.MouseEnter += (s, e) => { if (!active) row.BackColor = Color.FromArgb(31, 41, 55); };
            row.MouseLeave += (s, e) => { if (!active) row.BackColor = Color.Transparent; };
            lbl.MouseEnter += (s, e) => { if (!active) row.BackColor = Color.FromArgb(31, 41, 55); };
            lbl.MouseLeave += (s, e) => { if (!active) row.BackColor = Color.Transparent; };
            return row;
        }

        private void BuildRightDetailsPanel()
        {
            rightDetailsPanel = new Panel {
                Dock = DockStyle.Right, Width = 330,
                BackColor = Color.FromArgb(13, 20, 33), AutoScroll = true
            };

            int y = 14; const int pad = 14;

            // --- Profile Header ---
            Panel pCard = _MakeRoundPanel(pad, y, 302, 88);
            rightAvatarBox = new PictureBox {
                Location  = new Point(12, 14), Size = new Size(56, 56),
                SizeMode  = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(30, 41, 59)
            };
            rightAvatarBox.Paint += (s, e) => {
                if (rightAvatarBox.Image == null) return;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(1, 1, rightAvatarBox.Width - 3, rightAvatarBox.Height - 3);
                using (var gp2 = new GraphicsPath()) { gp2.AddEllipse(r); e.Graphics.SetClip(gp2); }
                e.Graphics.DrawImage(rightAvatarBox.Image, new Rectangle(0, 0, rightAvatarBox.Width, rightAvatarBox.Height));
                e.Graphics.ResetClip();
                using (var p2 = new Pen(Color.FromArgb(51, 65, 85), 2f)) e.Graphics.DrawEllipse(p2, r);
            };
            rightUsernameLabel = new Label {
                Text = "Select account...", Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = C_TEXT_HI, Location = new Point(76, 12), AutoSize = true
            };
            rightStatusBadge = new Label {
                Text = "online", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 211, 153), BackColor = Color.FromArgb(6, 78, 59),
                Location = new Point(222, 13), AutoSize = true, Padding = new Padding(5, 2, 5, 2)
            };
            rightAliasLabel = new Label {
                Text = "Alias: -  e", Font = new Font("Segoe UI", 8.5f),
                ForeColor = C_TEXT_MID, Location = new Point(76, 36), AutoSize = true
            };
            rightIdLabel = new Label {
                Text = "ID: -", Font = new Font("Segoe UI", 8f),
                ForeColor = C_TEXT_LO, Location = new Point(76, 55), AutoSize = true
            };
            pCard.Controls.AddRange(new Control[] { rightAvatarBox, rightUsernameLabel, rightStatusBadge, rightAliasLabel, rightIdLabel });
            y += 88 + 10;

            // --- Place ID / Job ID inputs ---
            Panel pInputs = new Panel { Location = new Point(pad, y), Size = new Size(302, 54), BackColor = Color.Transparent };
            Label lblPL = new Label { Text = "Place ID", Font = new Font("Segoe UI", 7.5f), ForeColor = C_TEXT_MID, Location = new Point(0, 0), AutoSize = true };
            PlaceID.Parent = pInputs;
            PlaceID.Location = new Point(0, 16); PlaceID.Size = new Size(144, 28);
            PlaceID.BackColor = C_BG_PANEL; PlaceID.ForeColor = C_TEXT_HI;
            PlaceID.BorderColor = C_ACCENT;  PlaceID.Font = new Font("Segoe UI", 9f);
            Label lblJL = new Label { Text = "Job ID / Private Server", Font = new Font("Segoe UI", 7.5f), ForeColor = C_TEXT_MID, Location = new Point(152, 0), AutoSize = true };
            JobID.Parent = pInputs;
            JobID.Location = new Point(152, 16); JobID.Size = new Size(150, 28);
            JobID.BackColor = C_BG_PANEL; JobID.ForeColor = C_TEXT_HI;
            JobID.BorderColor = C_ACCENT;  JobID.Font = new Font("Segoe UI", 9f);
            pInputs.Controls.AddRange(new Control[] { lblPL, PlaceID, lblJL, JobID });
            rightDetailsPanel.Controls.Add(pInputs);
            y += 54 + 8;

            // --- Join Server Hero Button ---
            JoinServer.Parent    = rightDetailsPanel;
            JoinServer.Location  = new Point(pad, y); JoinServer.Size = new Size(302, 50);
            JoinServer.Text      = "  Play   Join Server  ";
            JoinServer.Font      = new Font("Segoe UI", 12f, FontStyle.Bold);
            JoinServer.BackColor = C_ACCENT; JoinServer.ForeColor = Color.White;
            JoinServer.FlatStyle = FlatStyle.Flat; JoinServer.FlatAppearance.BorderSize = 0;
            JoinServer.Cursor    = Cursors.Hand;
            JoinServer.MouseEnter += (s, e) => JoinServer.BackColor = Color.FromArgb(59, 130, 246);
            JoinServer.MouseLeave += (s, e) => JoinServer.BackColor = C_ACCENT;
            y += 50 + 10;

            // --- Utilities / Follow / Set Alias ---
            Panel pActions = new Panel { Location = new Point(pad, y), Size = new Size(302, 42), BackColor = Color.Transparent };
            ServerList.Parent = pActions; ServerList.Location = new Point(0, 0);   ServerList.Size = new Size(96, 42);
            ServerList.Text = "Utilities"; ServerList.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            ServerList.BackColor = C_BTN_GREY; ServerList.ForeColor = C_TEXT_HI;
            ServerList.FlatStyle = FlatStyle.Flat; ServerList.FlatAppearance.BorderSize = 0;
            Follow.Parent = pActions; Follow.Location = new Point(103, 0); Follow.Size = new Size(96, 42);
            Follow.Text = "Follow"; Follow.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            Follow.BackColor = C_VIOLET; Follow.ForeColor = Color.White;
            Follow.FlatStyle = FlatStyle.Flat; Follow.FlatAppearance.BorderSize = 0;
            SetAlias.Parent = pActions; SetAlias.Location = new Point(206, 0); SetAlias.Size = new Size(96, 42);
            SetAlias.Text = "Set Alias"; SetAlias.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            SetAlias.BackColor = C_BTN_GREY; SetAlias.ForeColor = C_TEXT_HI;
            SetAlias.FlatStyle = FlatStyle.Flat; SetAlias.FlatAppearance.BorderSize = 0;
            rightDetailsPanel.Controls.Add(pActions);
            y += 42 + 10;

            // --- Follow target input ---
            Panel pFollow = new Panel { Location = new Point(pad, y), Size = new Size(302, 36), BackColor = Color.Transparent };
            Label lblFH = new Label { Text = "Username / User ID (Follow target)", Font = new Font("Segoe UI", 7.5f), ForeColor = C_TEXT_MID, Location = new Point(0, 0), AutoSize = true };
            UserID.Parent = pFollow; UserID.Location = new Point(0, 14); UserID.Size = new Size(302, 22);
            UserID.BackColor = C_BG_PANEL; UserID.ForeColor = C_TEXT_HI;
            UserID.BorderColor = C_ACCENT; UserID.Font = new Font("Segoe UI", 8.5f);
            pFollow.Controls.AddRange(new Control[] { lblFH, UserID });
            rightDetailsPanel.Controls.Add(pFollow);
            y += 36 + 10;

            // --- Account Info Card ---
            Panel pInfo = _MakeRoundPanel(pad, y, 302, 172);
            Label lblIH = new Label { Text = "Account Info",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = C_TEXT_HI,
                Location = new Point(12, 10), AutoSize = true };
            pInfo.Controls.Add(lblIH);
            rightValUsername    = _AddDetailRow(pInfo, "Username",    "-", 36,  true,  () => Clipboard.SetText(rightValUsername.Text));
            rightValAlias       = _AddEditRow  (pInfo, "Alias",       "-", 68,  Alias, SetAlias);
            rightValUserId      = _AddDetailRow(pInfo, "User ID",     "-", 100, true,  () => Clipboard.SetText(rightValUserId.Text));
            rightValDescription = _AddEditRow  (pInfo, "Description", "-", 132, DescriptionBox, SetDescription);
            y += 172 + 10;

            // --- Advanced Settings Card ---
            Panel pAdv = _MakeRoundPanel(pad, y, 302, 122);
            Label lblAH = new Label { Text = "Advanced Settings",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = C_TEXT_HI,
                Location = new Point(12, 10), AutoSize = true };
            pAdv.Controls.Add(lblAH);
            EditTheme.Parent = pAdv; EditTheme.Location = new Point(12, 38); EditTheme.Size = new Size(278, 32);
            EditTheme.Text = "  Edit Theme  >";
            EditTheme.Font = new Font("Segoe UI", 8.5f);
            EditTheme.BackColor = C_BTN_GREY; EditTheme.ForeColor = Color.FromArgb(203, 213, 225);
            EditTheme.FlatStyle = FlatStyle.Flat; EditTheme.FlatAppearance.BorderSize = 0;
            EditTheme.TextAlign = ContentAlignment.MiddleLeft;
            LaunchNexus.Parent = pAdv; LaunchNexus.Location = new Point(12, 78); LaunchNexus.Size = new Size(278, 32);
            LaunchNexus.Text = "  Account Control  >";
            LaunchNexus.Font = new Font("Segoe UI", 8.5f);
            LaunchNexus.BackColor = C_BTN_GREY; LaunchNexus.ForeColor = Color.FromArgb(203, 213, 225);
            LaunchNexus.FlatStyle = FlatStyle.Flat; LaunchNexus.FlatAppearance.BorderSize = 0;
            LaunchNexus.TextAlign = ContentAlignment.MiddleLeft;
            y += 122 + 10;

            // --- Status bar ---
            statusBarLabel = new Label {
                Location  = new Point(pad, y), Size = new Size(302, 24),
                Text      = "Ready / 0 accounts",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = C_GREEN, TextAlign = ContentAlignment.MiddleRight
            };

            rightDetailsPanel.Controls.AddRange(new Control[] { pCard, JoinServer, statusBarLabel });
            this.Controls.Add(rightDetailsPanel);
        }

        private Panel _MakeRoundPanel(int x, int y, int w, int h)
        {
            Panel p = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = C_BG_PANEL };
            p.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var gp = ModernAccountCard.GetRoundedRectangle(new Rectangle(0, 0, p.Width - 1, p.Height - 1), 10))
                using (var pen = new Pen(C_BORDER, 1f))
                    e.Graphics.DrawPath(pen, gp);
            };
            rightDetailsPanel.Controls.Add(p);
            return p;
        }

        private Label _AddDetailRow(Panel parent, string key, string def, int y, bool copy, Action onClick)
        {
            var lk  = new Label { Text = key, Font = new Font("Segoe UI", 8.5f), ForeColor = C_TEXT_MID, Location = new Point(12, y), AutoSize = true };
            var lv  = new Label { Text = def, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = C_TEXT_HI, Location = new Point(112, y), Size = new Size(152, 18), AutoEllipsis = true };
            var btn = new Button {
                Text = copy ? "C" : "E", Font = new Font("Segoe UI", 8f),
                ForeColor = C_TEXT_MID, BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat,
                Size = new Size(24, 20), Location = new Point(270, y - 2), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => onClick?.Invoke();
            parent.Controls.AddRange(new Control[] { lk, lv, btn });
            return lv;
        }

        private Label _AddEditRow(Panel parent, string key, string def, int y, Control legacyCtrl, Button legacyBtn)
        {
            var lk  = new Label { Text = key, Font = new Font("Segoe UI", 8.5f), ForeColor = C_TEXT_MID, Location = new Point(12, y), AutoSize = true };
            var lv  = new Label { Text = def, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = C_TEXT_HI, Location = new Point(112, y), Size = new Size(152, 18), AutoEllipsis = true };
            var ed  = new TextBox {
                Font = new Font("Segoe UI", 8.5f), Location = new Point(112, y - 2), Size = new Size(152, 20),
                BackColor = C_BG_BASE, ForeColor = C_TEXT_HI, BorderStyle = BorderStyle.FixedSingle, Visible = false
            };
            bool sup = false;
            void Commit() {
                if (sup) return; sup = true;
                legacyCtrl.Text = ed.Text;
                legacyBtn.PerformClick();
                lv.Text = string.IsNullOrEmpty(ed.Text) ? "-" : ed.Text;
                ed.Visible = false; lv.Visible = true; sup = false;
            }
            void Cancel() { sup = true; ed.Visible = false; lv.Visible = true; sup = false; }
            ed.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)  { e.SuppressKeyPress = true; Commit(); }
                if (e.KeyCode == Keys.Escape) { e.SuppressKeyPress = true; Cancel(); }
            };
            ed.Leave += (s, e) => Commit();
            var btn = new Button {
                Text = "E", Font = new Font("Segoe UI", 8f),
                ForeColor = C_TEXT_MID, BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat,
                Size = new Size(24, 20), Location = new Point(270, y - 2), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => { ed.Text = lv.Text == "-" ? "" : lv.Text; lv.Visible = false; ed.Visible = true; ed.Focus(); ed.SelectAll(); };
            parent.Controls.AddRange(new Control[] { lk, lv, ed, btn });
            return lv;
        }

        private void BuildCenterContentPanel()
        {
            centerPanel = new Panel { Dock = DockStyle.Fill, BackColor = C_BG_BASE, Padding = new Padding(16, 14, 16, 14) };

            // --- Top Search & Sort Bar ---
            Panel pTop = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.Transparent };

            Panel pSearch = new Panel { Location = new Point(0, 8), Size = new Size(340, 32), BackColor = C_BG_PANEL, Cursor = Cursors.IBeam };
            pSearch.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var gp = ModernAccountCard.GetRoundedRectangle(new Rectangle(0, 0, pSearch.Width - 1, pSearch.Height - 1), 6))
                using (var pen = new Pen(C_BORDER, 1f))
                    e.Graphics.DrawPath(pen, gp);
            };
            Label lblIco = new Label { Text = "S", Font = new Font("Segoe UI", 9f), ForeColor = C_TEXT_MID, Location = new Point(6, 7), AutoSize = true, BackColor = Color.Transparent };
            searchTextBox = new TextBox {
                Location = new Point(26, 6), Size = new Size(308, 22),
                BackColor = C_BG_PANEL, ForeColor = C_TEXT_HI,
                Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.None
            };
            searchTextBox.TextChanged += (s, e) => RefreshModernCards();
            Label lblPH = new Label {
                Text = "Search accounts / Alias...",
                ForeColor = C_TEXT_LO, Font = new Font("Segoe UI", 9f),
                Location = new Point(27, 8), AutoSize = true, BackColor = C_BG_PANEL, Cursor = Cursors.IBeam
            };
            lblPH.Click += (s, e) => searchTextBox.Focus();
            searchTextBox.Enter += (s, e) => lblPH.Visible = false;
            searchTextBox.Leave += (s, e) => lblPH.Visible = string.IsNullOrEmpty(searchTextBox.Text);
            pSearch.Controls.AddRange(new Control[] { lblIco, searchTextBox, lblPH });

            Label lblSort = new Label { Text = "Sort by", Font = new Font("Segoe UI", 8.5f), ForeColor = C_TEXT_MID, Location = new Point(354, 16), AutoSize = true };
            sortComboBox = new ComboBox {
                Location = new Point(414, 11), Size = new Size(155, 28),
                BackColor = C_BG_PANEL, ForeColor = C_TEXT_HI,
                Font = new Font("Segoe UI", 8.5f), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat
            };
            sortComboBox.Items.AddRange(new object[] { "Name (A-Z)", "Name (Z-A)", "Group", "Last Used" });
            sortComboBox.SelectedIndex = 0;
            sortComboBox.SelectedIndexChanged += (s, e) => RefreshModernCards();
            CheckBox chkSelectAll = new CheckBox
            {
                Text = "Select All",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = C_TEXT_MID,
                Location = new Point(585, 14),
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            chkSelectAll.CheckedChanged += (s, e) =>
            {
                if (SelectedAccounts == null) SelectedAccounts = new List<Account>();
                SelectedAccounts.Clear();
                if (chkSelectAll.Checked)
                {
                    SelectedAccounts.AddRange(AccountsList);
                    if (AccountsList.Count > 0) SelectedAccount = AccountsList[0];
                }
                else
                {
                    SelectedAccount = null;
                }
                _UpdateCardSelection();
                UpdateJoinButtonText();
            };

            pTop.Controls.AddRange(new Control[] { pSearch, lblSort, sortComboBox, chkSelectAll });

            // --- Bottom Action Bar ---
            Panel pBottom = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = C_BG_DEEP };
            Panel sepB = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(31, 41, 55) };
            pBottom.Controls.Add(sepB);

            Add.Parent = pBottom; Add.Location = new Point(0, 10); Add.Size = new Size(138, 42);
            Add.Text = "+ Add Account";
            Add.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            Add.BackColor = C_GREEN; Add.ForeColor = Color.White;
            Add.FlatStyle = FlatStyle.Flat; Add.FlatAppearance.BorderSize = 0;
            Add.Menu = AddAccountsStrip; Add.Cursor = Cursors.Hand;

            Remove.Parent = pBottom; Remove.Location = new Point(148, 10); Remove.Size = new Size(128, 42);
            Remove.Text = "Delete Selected";
            Remove.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            Remove.BackColor = C_RED; Remove.ForeColor = Color.White;
            Remove.FlatStyle = FlatStyle.Flat; Remove.FlatAppearance.BorderSize = 0; Remove.Cursor = Cursors.Hand;

            bool hideInit = HideUsernamesCheckbox != null && HideUsernamesCheckbox.Checked;
            btnToggleHideUser = new Button {
                Text = hideInit ? "Show Usernames" : "Hide Usernames",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BackColor = C_BTN_GREY, ForeColor = C_TEXT_HI,
                FlatStyle = FlatStyle.Flat, Size = new Size(136, 42), Location = new Point(286, 10), Cursor = Cursors.Hand
            };
            btnToggleHideUser.FlatAppearance.BorderSize = 0;
            btnToggleHideUser.Click += (s, e) => {
                HideUsernamesCheckbox.Checked = !HideUsernamesCheckbox.Checked;
                IniSettings.Save("RAMSettings.ini");
                btnToggleHideUser.Text = HideUsernamesCheckbox.Checked ? "Show Usernames" : "Hide Usernames";
                RefreshModernCards();
            };
            btnToggleHideUser.Parent = pBottom;

            OpenBrowser.Parent = pBottom; OpenBrowser.Location = new Point(432, 10); OpenBrowser.Size = new Size(150, 42);
            OpenBrowser.Text = "Open Browser";
            OpenBrowser.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            OpenBrowser.BackColor = C_BTN_GREY; OpenBrowser.ForeColor = C_TEXT_HI;
            OpenBrowser.FlatStyle = FlatStyle.Flat; OpenBrowser.FlatAppearance.BorderSize = 0;
            OpenBrowser.Menu = OpenBrowserStrip; OpenBrowser.Cursor = Cursors.Hand;

            Add.Visible = Remove.Visible = btnToggleHideUser.Visible = OpenBrowser.Visible = true;
            pBottom.Controls.AddRange(new Control[] { Add, Remove, btnToggleHideUser, OpenBrowser });

            // --- Cards Container ---
            cardsContainer = new FlowLayoutPanel {
                Dock = DockStyle.Fill, BackColor = C_BG_BASE,
                AutoScroll = true, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, Padding = new Padding(0, 8, 8, 8)
            };
            cardsContainer.SizeChanged += (s, e) => {
                int w = Math.Max(cardsContainer.ClientSize.Width - 20, 420);
                foreach (var card in modernCards) card.Width = w;
            };

            centerPanel.Controls.Add(cardsContainer);
            centerPanel.Controls.Add(pTop);
            centerPanel.Controls.Add(pBottom);
            pTop.SendToBack(); pBottom.SendToBack(); cardsContainer.BringToFront();
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
                IEnumerable<Account> list = AccountsList.AsEnumerable();
                if (!string.IsNullOrEmpty(filter))
                    list = list.Where(a =>
                        (a.Username != null && a.Username.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (a.Alias    != null && a.Alias.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        a.UserID.ToString().Contains(filter));
                int si = sortComboBox?.SelectedIndex ?? 0;
                switch (si)
                {
                    case 0: list = list.OrderBy(a => a.Username); break;
                    case 1: list = list.OrderByDescending(a => a.Username); break;
                    case 2: list = list.OrderBy(a => a.Group).ThenBy(a => a.Username); break;
                    case 3: list = list.OrderByDescending(a => a.LastUse); break;
                }
                int cardW = Math.Max(cardsContainer.ClientSize.Width - 20, 450);
                foreach (Account acc in list)
                {
                    ModernAccountCard card = new ModernAccountCard(acc)
                    {
                        Width = cardW, HideUsername = hideUser,
                        IsSelected = (SelectedAccount == acc ||
                                      (SelectedAccounts != null && SelectedAccounts.Contains(acc)))
                    };
                    card.SelectionToggled += (s, isChecked) =>
                    {
                        if (SelectedAccounts == null) SelectedAccounts = new List<Account>();
                        if (isChecked)
                        {
                            if (!SelectedAccounts.Contains(acc)) SelectedAccounts.Add(acc);
                        }
                        else
                        {
                            SelectedAccounts.Remove(acc);
                        }
                        if (SelectedAccounts.Count == 1) SelectedAccount = SelectedAccounts[0];
                        else if (SelectedAccounts.Count == 0) SelectedAccount = null;
                        UpdateRightPanelDetails(acc);
                        UpdateJoinButtonText();
                        _UpdateCardSelection();
                    };
                    card.CardClicked += (s, a) =>
                    {
                        bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
                        if (ctrl && SelectedAccounts != null)
                        { if (SelectedAccounts.Contains(a)) SelectedAccounts.Remove(a); else SelectedAccounts.Add(a); }
                        else { SelectedAccounts = new List<Account> { a }; SelectedAccount = a; }
                        try { AccountsView.SelectedObjects = SelectedAccounts; } catch { }
                        UpdateRightPanelDetails(a);
                        UpdateJoinButtonText();
                        _UpdateCardSelection();
                    };
                    card.PlayClicked += async (s, a) =>
                    {
                        ModernAccountCard src = s as ModernAccountCard;
                        if (src != null) src.Enabled = false;
                        try
                        {
                            long.TryParse(PlaceID?.Text, out long pid);
                            string jid = JobID?.Text ?? "";
                            bool vip = jid.Length > 4 && jid.StartsWith("VIP:");
                            Program.Logger.Info("[QuickLaunch] " + a.Username);
                            string res = await a.JoinServer(pid, vip ? jid.Substring(4) : jid, false, vip);
                            if (!string.IsNullOrEmpty(res) && !res.Contains("Success"))
                                MessageBox.Show(res, "Launch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        finally { if (src != null && !src.IsDisposed) src.Enabled = true; }
                    };
                    card.CopyClicked += (s, a) =>
                    {
                        if (!string.IsNullOrEmpty(a.SecurityToken)) Clipboard.SetText(a.SecurityToken);
                        else if (!string.IsNullOrEmpty(a.Username)) Clipboard.SetText(a.Username);
                    };
                    card.MoreClicked += (s, pt) =>
                    {
                        SelectedAccount = acc; SelectedAccounts = new List<Account> { acc };
                        _UpdateCardSelection(); UpdateRightPanelDetails(acc);
                        AccountsStrip?.Show(pt);
                    };
                    modernCards.Add(card);
                    cardsContainer.Controls.Add(card);
                }
                if (navBadgeLabel  != null) navBadgeLabel.Text  = AccountsList.Count.ToString();
                if (statusBarLabel != null) statusBarLabel.Text = "Ready / " + AccountsList.Count + " accounts";
                if (SelectedAccount != null && !AccountsList.Contains(SelectedAccount))
                { SelectedAccount = null; SelectedAccounts = new List<Account>(); }
                if (SelectedAccount == null && AccountsList.Count > 0)
                { SelectedAccount = AccountsList[0]; SelectedAccounts = new List<Account> { SelectedAccount }; }
                if (SelectedAccount != null) { UpdateRightPanelDetails(SelectedAccount); _UpdateCardSelection(); }
                else
                {
                    if (rightUsernameLabel  != null) rightUsernameLabel.Text  = "Select account...";
                    if (rightAliasLabel     != null) rightAliasLabel.Text     = "Alias: -";
                    if (rightIdLabel        != null) rightIdLabel.Text        = "ID: -";
                    if (rightValUsername    != null) rightValUsername.Text    = "-";
                    if (rightValAlias       != null) rightValAlias.Text       = "-";
                    if (rightValUserId      != null) rightValUserId.Text      = "-";
                    if (rightValDescription != null) rightValDescription.Text = "-";
                    if (rightAvatarBox      != null) rightAvatarBox.Image     = null;
                }
                bool has = SelectedAccount != null;
                JoinServer.Enabled = has; Remove.Enabled = has;
                ServerList.Enabled = has; Follow.Enabled = has; SetAlias.Enabled = has;
                cardsContainer.ResumeLayout(true);
            });
        }

        private void _UpdateCardSelection()
        {
            foreach (var card in modernCards)
            {
                card.IsSelected = (SelectedAccount == card.Account ||
                                  (SelectedAccounts != null && SelectedAccounts.Contains(card.Account)));
                card.Invalidate();
            }
        }

        public void UpdateRightPanelDetails(Account a)
        {
            if (a == null) return;
            this.InvokeIfRequired(() =>
            {
                bool hide = General.Get<bool>("HideUsernames");
                string un = hide ? "........" : a.Username;
                if (rightUsernameLabel  != null) rightUsernameLabel.Text  = un;
                if (rightAliasLabel     != null) rightAliasLabel.Text     = "Alias: " + (!string.IsNullOrEmpty(a.Alias) ? a.Alias : "-") + "  e";
                if (rightIdLabel        != null) rightIdLabel.Text        = "ID: " + (a.UserID > 0 ? a.UserID.ToString() : "-");
                bool online = a.Presence != null && a.Presence.userPresenceType != UserPresenceType.Offline;
                if (rightStatusBadge != null)
                {
                    rightStatusBadge.Text      = online ? "online" : "offline";
                    rightStatusBadge.BackColor = online ? Color.FromArgb(6, 78, 59)    : Color.FromArgb(30, 41, 59);
                    rightStatusBadge.ForeColor = online ? Color.FromArgb(52, 211, 153) : C_TEXT_MID;
                }
                if (rightValUsername    != null) rightValUsername.Text    = un;
                if (rightValAlias       != null) rightValAlias.Text       = !string.IsNullOrEmpty(a.Alias) ? a.Alias : "-";
                if (rightValUserId      != null) rightValUserId.Text      = a.UserID > 0 ? a.UserID.ToString() : "-";
                if (rightValDescription != null) rightValDescription.Text = !string.IsNullOrEmpty(a.Description) ? a.Description : "-";
                if (rightAvatarBox != null)
                {
                    if (ModernAccountCard.AvatarCache.TryGetValue(a.UserID, out Image img)) rightAvatarBox.Image = img;
                    else rightAvatarBox.Image = null;
                }
                if (!string.IsNullOrEmpty(a.GetField("SavedPlaceId")) && PlaceID != null) PlaceID.Text = a.GetField("SavedPlaceId");
                if (!string.IsNullOrEmpty(a.GetField("SavedJobId"))   && JobID   != null) JobID.Text   = a.GetField("SavedJobId");
            });
        }

        internal void UpdateJoinButtonText()
        {
            if (JoinServer == null) return;
            int count = SelectedAccounts?.Count ?? (SelectedAccount != null ? 1 : 0);
            if (count > 1)
                JoinServer.Text = $"  Play ({count})   Join Server  ";
            else
                JoinServer.Text = "  Play   Join Server  ";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RBX_Alt_Manager.Classes
{
    /// <summary>
    /// Modern dark-gaming style account row card.
    /// Public API (constructor, properties, events) is unchanged from the
    /// previous implementation so existing wiring in AccountManager keeps working.
    /// </summary>
    public class ModernAccountCard : Control
    {
        // ---- Design tokens (Dark Gaming palette) --------------------------------
        private static readonly Color BgIdle = Color.FromArgb(15, 20, 34);      // #0F1422
        private static readonly Color BgHover = Color.FromArgb(20, 27, 45);     // #141B2D
        private static readonly Color BgSelected = Color.FromArgb(22, 32, 54);  // #162036
        private static readonly Color BorderIdle = Color.FromArgb(30, 41, 59);   // #1E293B
        private static readonly Color BorderHover = Color.FromArgb(51, 65, 85);  // #334155
        private static readonly Color BorderSelected = Color.FromArgb(37, 99, 235); // #2563EB primary accent
        private static readonly Color AccentPrimary = Color.FromArgb(37, 99, 235);  // #2563EB
        private static readonly Color AccentPrimaryHover = Color.FromArgb(59, 130, 246); // #3B82F6
        private static readonly Color OnlineGreen = Color.FromArgb(34, 197, 94);   // #22C55E
        private static readonly Color OnlineGreenBg = Color.FromArgb(6, 78, 59);   // pill background
        private static readonly Color OnlineGreenFg = Color.FromArgb(74, 222, 128);
        private static readonly Color OfflineGray = Color.FromArgb(100, 116, 139); // #64748B
        private static readonly Color TextPrimary = Color.FromArgb(248, 250, 252); // #F8FAFC
        private static readonly Color TextSecondary = Color.FromArgb(148, 163, 184); // #94A3B8
        private static readonly Color TextMuted = Color.FromArgb(100, 116, 139);   // #64748B
        private static readonly Color SurfaceSubtle = Color.FromArgb(30, 41, 59);  // secondary button bg
        private static readonly Color SurfaceSubtleHover = Color.FromArgb(51, 65, 85);

        public static readonly Dictionary<long, Image> AvatarCache = new Dictionary<long, Image>();

        public Account Account { get; private set; }
        public bool IsSelected { get; set; }
        public bool HideUsername { get; set; }

        public event EventHandler<Account> CardClicked;
        public event EventHandler<Account> PlayClicked;
        public event EventHandler<Account> CopyClicked;
        public event EventHandler<Point> MoreClicked;

        private bool isHovered = false;
        private bool isPlayHovered = false;
        private bool isCopyHovered = false;
        private bool isMoreHovered = false;

        private Rectangle playRect;
        private Rectangle copyRect;
        private Rectangle moreRect;

        private readonly ToolTip toolTip = new ToolTip
        {
            AutoPopDelay = 4000,
            InitialDelay = 400,
            ReshowDelay = 200,
            ShowAlways = true
        };

        public ModernAccountCard(Account account)
        {
            this.Account = account;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                           ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.Height = 80; // slightly taller for comfortable 40px+ touch targets
            this.Cursor = Cursors.Hand;
            this.Margin = new Padding(0, 0, 0, 8);
            this.TabStop = true;
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.ListItem;

            UpdateAccessibleName();
            LoadAvatarAsync();
        }

        private void UpdateAccessibleName()
        {
            if (Account == null) return;
            bool isOnline = Account.Presence != null && Account.Presence.userPresenceType != UserPresenceType.Offline;
            this.AccessibleName = $"{Account.Username}, {(isOnline ? "ออนไลน์" : "ออฟไลน์")}";
        }

        private void LoadAvatarAsync()
        {
            if (Account == null || Account.UserID <= 0) return;
            if (AvatarCache.ContainsKey(Account.UserID)) return;

            Task.Run(async () =>
            {
                try
                {
                    string url = await Batch.GetImage(Account.UserID, "AvatarHeadShot", "150x150");
                    if (!string.IsNullOrEmpty(url))
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            byte[] data = await client.GetByteArrayAsync(url);
                            using (MemoryStream ms = new MemoryStream(data))
                            {
                                Image img = Image.FromStream(ms);
                                lock (AvatarCache)
                                {
                                    AvatarCache[Account.UserID] = img;
                                }

                                if (!IsDisposed)
                                {
                                    this.BeginInvoke((MethodInvoker)delegate { this.Invalidate(); });
                                }
                            }
                        }
                    }
                }
                catch { /* avatar is best-effort; initials fallback is drawn instead */ }
            });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Larger action buttons (>=36px) to meet comfortable touch-target guidance.
            int btnSize = 36;
            int btnY = (Height - btnSize) / 2;
            moreRect = new Rectangle(Width - 16 - btnSize, btnY, btnSize, btnSize);
            copyRect = new Rectangle(moreRect.X - 8 - btnSize, btnY, btnSize, btnSize);
            playRect = new Rectangle(copyRect.X - 8 - 42, btnY, 42, btnSize);

            toolTip.SetToolTip(this, null);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool oldPlay = isPlayHovered;
            bool oldCopy = isCopyHovered;
            bool oldMore = isMoreHovered;

            isPlayHovered = playRect.Contains(e.Location);
            isCopyHovered = copyRect.Contains(e.Location);
            isMoreHovered = moreRect.Contains(e.Location);

            if (isPlayHovered) toolTip.SetToolTip(this, "เข้าเกม (Launch)");
            else if (isCopyHovered) toolTip.SetToolTip(this, "คัดลอกข้อมูลบัญชี");
            else if (isMoreHovered) toolTip.SetToolTip(this, "ตัวเลือกเพิ่มเติม");
            else toolTip.SetToolTip(this, null);

            if (oldPlay != isPlayHovered || oldCopy != isCopyHovered || oldMore != isMoreHovered)
                Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            isPlayHovered = false;
            isCopyHovered = false;
            isMoreHovered = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Enter || keyData == Keys.Space) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            // Keyboard access: Enter selects, Space launches — mirrors mouse click behavior.
            if (e.KeyCode == Keys.Enter)
                CardClicked?.Invoke(this, Account);
            else if (e.KeyCode == Keys.Space)
                PlayClicked?.Invoke(this, Account);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            this.Focus();

            if (e.Button == MouseButtons.Right)
            {
                MoreClicked?.Invoke(this, this.PointToScreen(e.Location));
                return;
            }

            if (playRect.Contains(e.Location))
                PlayClicked?.Invoke(this, Account);
            else if (copyRect.Contains(e.Location))
                CopyClicked?.Invoke(this, Account);
            else if (moreRect.Contains(e.Location))
                MoreClicked?.Invoke(this, this.PointToScreen(new Point(moreRect.Left, moreRect.Bottom)));
            else
                CardClicked?.Invoke(this, Account);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle bounds = new Rectangle(1, 1, Width - 3, Height - 3);

            Color bgColor = IsSelected ? BgSelected : (isHovered ? BgHover : BgIdle);
            Color borderColor = IsSelected ? BorderSelected : (isHovered ? BorderHover : BorderIdle);
            int borderWidth = IsSelected ? 2 : 1;

            using (GraphicsPath path = GetRoundedRectangle(bounds, 12))
            {
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                    g.FillPath(bgBrush, path);

                using (Pen borderPen = new Pen(borderColor, borderWidth))
                    g.DrawPath(borderPen, path);
            }

            // Keyboard focus ring (accessibility requirement — never remove focus indication)
            if (this.Focused)
            {
                using (GraphicsPath focusPath = GetRoundedRectangle(bounds, 12))
                using (Pen focusPen = new Pen(Color.FromArgb(140, AccentPrimaryHover), 1.5f) { DashStyle = DashStyle.Dot })
                    g.DrawPath(focusPen, focusPath);
            }

            // ---- Avatar ---------------------------------------------------------
            int avatarSize = 48;
            int avatarX = 16;
            int avatarY = (Height - avatarSize) / 2;
            Rectangle avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);

            using (GraphicsPath clipPath = new GraphicsPath())
            {
                clipPath.AddEllipse(avatarRect);
                g.SetClip(clipPath);

                Image avatarImg = null;
                if (Account != null && AvatarCache.TryGetValue(Account.UserID, out Image cached))
                    avatarImg = cached;

                if (avatarImg != null)
                {
                    g.DrawImage(avatarImg, avatarRect);
                }
                else
                {
                    using (SolidBrush pBrush = new SolidBrush(SurfaceSubtle))
                        g.FillRectangle(pBrush, avatarRect);

                    string initial = (Account != null && !string.IsNullOrEmpty(Account.Username)) ? Account.Username.Substring(0, 1).ToUpper() : "?";
                    using (Font initialFont = new Font("Segoe UI", 17f, FontStyle.Bold))
                    using (SolidBrush tBrush = new SolidBrush(TextSecondary))
                    {
                        SizeF s = g.MeasureString(initial, initialFont);
                        g.DrawString(initial, initialFont, tBrush, avatarX + (avatarSize - s.Width) / 2, avatarY + (avatarSize - s.Height) / 2);
                    }
                }
                g.ResetClip();
            }

            using (Pen aPen = new Pen(BorderHover, 1.5f))
                g.DrawEllipse(aPen, avatarRect);

            // Status Indicator Dot
            bool isOnline = (Account?.Presence != null && Account.Presence.userPresenceType != UserPresenceType.Offline);
            Color statusColor = isOnline ? OnlineGreen : OfflineGray;
            Rectangle statusDotRect = new Rectangle(avatarX + avatarSize - 13, avatarY + avatarSize - 13, 14, 14);

            using (SolidBrush dotBg = new SolidBrush(bgColor))
                g.FillEllipse(dotBg, new Rectangle(statusDotRect.X - 2, statusDotRect.Y - 2, statusDotRect.Width + 4, statusDotRect.Height + 4));

            using (SolidBrush dotBrush = new SolidBrush(statusColor))
                g.FillEllipse(dotBrush, statusDotRect);

            // ---- Text info --------------------------------------------------------
            int textX = avatarX + avatarSize + 16;
            int textY = 14;

            string uname = (Account != null) ? (HideUsername ? "••••••••" : Account.Username) : "Unknown";
            using (Font uFont = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (SolidBrush uBrush = new SolidBrush(TextPrimary))
            {
                g.DrawString(uname, uFont, uBrush, textX, textY);
                SizeF uSize = g.MeasureString(uname, uFont);

                // Online/Offline pill badge
                int badgeX = textX + (int)uSize.Width + 10;
                int badgeY = textY + 1;
                string statusText = isOnline ? "ออนไลน์" : "ออฟไลน์";
                Color badgeBg = isOnline ? OnlineGreenBg : SurfaceSubtle;
                Color badgeFg = isOnline ? OnlineGreenFg : TextSecondary;

                using (Font badgeFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                {
                    SizeF bSize = g.MeasureString(statusText, badgeFont);
                    Rectangle bRect = new Rectangle(badgeX, badgeY, (int)bSize.Width + 18, 19);

                    using (GraphicsPath bPath = GetRoundedRectangle(bRect, 9))
                    using (SolidBrush bBg = new SolidBrush(badgeBg))
                    using (SolidBrush bText = new SolidBrush(badgeFg))
                    using (SolidBrush dot = new SolidBrush(isOnline ? OnlineGreen : TextSecondary))
                    {
                        g.FillPath(bBg, bPath);
                        g.FillEllipse(dot, badgeX + 6, badgeY + 6, 5, 5);
                        g.DrawString(statusText, badgeFont, bText, badgeX + 15, badgeY + 2);
                    }
                }
            }

            // Alias row
            string aliasText = "Alias: " + (!string.IsNullOrEmpty(Account?.Alias) ? Account.Alias : "-");
            using (Font subFont = new Font("Segoe UI", 8.75f, FontStyle.Regular))
            using (SolidBrush subBrush = new SolidBrush(TextSecondary))
            {
                g.DrawString(aliasText, subFont, subBrush, textX, textY + 24);
            }

            // User ID row
            string idText = "ID: " + (Account?.UserID > 0 ? Account.UserID.ToString() : "-");
            using (Font idFont = new Font("Segoe UI", 8.75f, FontStyle.Regular))
            using (SolidBrush idBrush = new SolidBrush(TextMuted))
            {
                g.DrawString(idText, idFont, idBrush, textX, textY + 43);
            }

            // ---- Action buttons -----------------------------------------------
            DrawPlayButton(g);
            DrawCopyButton(g);
            DrawMoreButton(g);
        }

        private void DrawPlayButton(Graphics g)
        {
            Color pBg = isPlayHovered ? AccentPrimaryHover : AccentPrimary;
            using (GraphicsPath path = GetRoundedRectangle(playRect, 8))
            {
                using (SolidBrush brush = new SolidBrush(pBg))
                    g.FillPath(brush, path);

                int cx = playRect.X + playRect.Width / 2;
                int cy = playRect.Y + playRect.Height / 2;
                Point[] triangle = new Point[] { new Point(cx - 4, cy - 6), new Point(cx + 6, cy), new Point(cx - 4, cy + 6) };
                using (SolidBrush iconBrush = new SolidBrush(Color.White))
                    g.FillPolygon(iconBrush, triangle);
            }
        }

        private void DrawCopyButton(Graphics g)
        {
            Color cBg = isCopyHovered ? SurfaceSubtleHover : SurfaceSubtle;
            using (GraphicsPath path = GetRoundedRectangle(copyRect, 8))
            {
                using (SolidBrush brush = new SolidBrush(cBg))
                    g.FillPath(brush, path);

                int cx = copyRect.X + copyRect.Width / 2;
                int cy = copyRect.Y + copyRect.Height / 2;
                using (Pen iconPen = new Pen(Color.FromArgb(203, 213, 225), 1.5f))
                {
                    g.DrawRectangle(iconPen, cx - 3, cy - 6, 9, 9);
                    using (SolidBrush fBrush = new SolidBrush(cBg))
                        g.FillRectangle(fBrush, cx - 6, cy - 3, 9, 9);
                    g.DrawRectangle(iconPen, cx - 6, cy - 3, 9, 9);
                }
            }
        }

        private void DrawMoreButton(Graphics g)
        {
            Color mBg = isMoreHovered ? SurfaceSubtleHover : SurfaceSubtle;
            using (GraphicsPath path = GetRoundedRectangle(moreRect, 8))
            {
                using (SolidBrush brush = new SolidBrush(mBg))
                    g.FillPath(brush, path);

                int cx = moreRect.X + moreRect.Width / 2;
                int cy = moreRect.Y + moreRect.Height / 2;
                using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb(203, 213, 225)))
                {
                    g.FillEllipse(dotBrush, cx - 7, cy - 1, 3, 3);
                    g.FillEllipse(dotBrush, cx - 1, cy - 1, 3, 3);
                    g.FillEllipse(dotBrush, cx + 5, cy - 1, 3, 3);
                }
            }
        }

        public static GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                toolTip?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

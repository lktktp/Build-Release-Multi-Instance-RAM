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
    public class ModernAccountCard : Control
    {
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

        public ModernAccountCard(Account account)
        {
            this.Account = account;
            this.DoubleBuffered = true;
            this.Height = 74;
            this.Cursor = Cursors.Hand;
            this.Margin = new Padding(0, 0, 0, 8);

            LoadAvatarAsync();
        }

        private void LoadAvatarAsync()
        {
            if (Account == null || Account.UserID <= 0) return;

            if (AvatarCache.ContainsKey(Account.UserID))
                return;

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
                                    this.BeginInvoke((MethodInvoker)delegate
                                    {
                                        this.Invalidate();
                                    });
                                }
                            }
                        }
                    }
                }
                catch { }
            });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            int btnY = (Height - 34) / 2;
            moreRect = new Rectangle(Width - 44, btnY, 34, 34);
            copyRect = new Rectangle(Width - 84, btnY, 34, 34);
            playRect = new Rectangle(Width - 128, btnY, 38, 34);
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

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Right)
            {
                MoreClicked?.Invoke(this, this.PointToScreen(e.Location));
                return;
            }

            if (playRect.Contains(e.Location))
            {
                PlayClicked?.Invoke(this, Account);
            }
            else if (copyRect.Contains(e.Location))
            {
                CopyClicked?.Invoke(this, Account);
            }
            else if (moreRect.Contains(e.Location))
            {
                MoreClicked?.Invoke(this, this.PointToScreen(new Point(moreRect.Left, moreRect.Bottom)));
            }
            else
            {
                CardClicked?.Invoke(this, Account);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle bounds = new Rectangle(1, 1, Width - 3, Height - 3);

            // Card Background & Border
            Color bgColor = IsSelected ? Color.FromArgb(19, 29, 48) : (isHovered ? Color.FromArgb(21, 30, 48) : Color.FromArgb(16, 23, 38));
            Color borderColor = IsSelected ? Color.FromArgb(37, 99, 235) : (isHovered ? Color.FromArgb(51, 65, 85) : Color.FromArgb(30, 41, 59));
            int borderWidth = IsSelected ? 2 : 1;

            using (GraphicsPath path = GetRoundedRectangle(bounds, 10))
            {
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                    g.FillPath(bgBrush, path);

                using (Pen borderPen = new Pen(borderColor, borderWidth))
                    g.DrawPath(borderPen, path);
            }

            // Avatar Section
            int avatarSize = 46;
            int avatarX = 14;
            int avatarY = (Height - avatarSize) / 2;
            Rectangle avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);

            // Draw circular avatar
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
                    // Fallback sleek placeholder
                    using (SolidBrush pBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
                        g.FillRectangle(pBrush, avatarRect);

                    string initial = (Account != null && !string.IsNullOrEmpty(Account.Username)) ? Account.Username.Substring(0, 1).ToUpper() : "?";
                    using (Font initialFont = new Font("Segoe UI", 16f, FontStyle.Bold))
                    using (SolidBrush tBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
                    {
                        SizeF s = g.MeasureString(initial, initialFont);
                        g.DrawString(initial, initialFont, tBrush, avatarX + (avatarSize - s.Width) / 2, avatarY + (avatarSize - s.Height) / 2);
                    }
                }

                g.ResetClip();
            }

            // Draw Avatar Border Circle
            using (Pen aPen = new Pen(Color.FromArgb(51, 65, 85), 1.5f))
                g.DrawEllipse(aPen, avatarRect);

            // Presence Indicator Dot at bottom right of avatar
            bool isOnline = (Account?.Presence != null && Account.Presence.userPresenceType != UserPresenceType.Offline);
            Color statusColor = isOnline ? Color.FromArgb(16, 185, 129) : Color.FromArgb(100, 116, 139);
            Rectangle statusDotRect = new Rectangle(avatarX + avatarSize - 12, avatarY + avatarSize - 12, 13, 13);

            using (SolidBrush dotBg = new SolidBrush(Color.FromArgb(16, 23, 38)))
                g.FillEllipse(dotBg, new Rectangle(statusDotRect.X - 2, statusDotRect.Y - 2, statusDotRect.Width + 4, statusDotRect.Height + 4));

            using (SolidBrush dotBrush = new SolidBrush(statusColor))
                g.FillEllipse(dotBrush, statusDotRect);

            // Text Info Section
            int textX = avatarX + avatarSize + 14;
            int textY = 12;

            // Username
            string uname = (Account != null) ? (HideUsername ? "••••••••" : Account.Username) : "Unknown";
            using (Font uFont = new Font("Segoe UI", 10.5f, FontStyle.Bold))
            using (SolidBrush uBrush = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                g.DrawString(uname, uFont, uBrush, textX, textY);
                SizeF uSize = g.MeasureString(uname, uFont);

                // Online/Offline Pill Badge
                int badgeX = textX + (int)uSize.Width + 8;
                int badgeY = textY + 1;
                string statusText = isOnline ? "ออนไลน์" : "ออฟไลน์";
                Color badgeBg = isOnline ? Color.FromArgb(6, 78, 59) : Color.FromArgb(30, 41, 59);
                Color badgeFg = isOnline ? Color.FromArgb(52, 211, 153) : Color.FromArgb(148, 163, 184);

                using (Font badgeFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                {
                    SizeF bSize = g.MeasureString(statusText, badgeFont);
                    Rectangle bRect = new Rectangle(badgeX, badgeY, (int)bSize.Width + 16, 18);

                    using (GraphicsPath bPath = GetRoundedRectangle(bRect, 8))
                    using (SolidBrush bBg = new SolidBrush(badgeBg))
                    using (SolidBrush bText = new SolidBrush(badgeFg))
                    using (SolidBrush dot = new SolidBrush(isOnline ? Color.FromArgb(16, 185, 129) : Color.FromArgb(148, 163, 184)))
                    {
                        g.FillPath(bBg, bPath);
                        g.FillEllipse(dot, badgeX + 5, badgeY + 6, 5, 5);
                        g.DrawString(statusText, badgeFont, bText, badgeX + 13, badgeY + 2);
                    }
                }
            }

            // Alias Row
            string aliasText = "Alias: " + (!string.IsNullOrEmpty(Account?.Alias) ? Account.Alias : "-");
            using (Font subFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (SolidBrush subBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                g.DrawString(aliasText, subFont, subBrush, textX, textY + 22);

                // Small pencil icon indicator
                SizeF aliasSize = g.MeasureString(aliasText, subFont);
                using (SolidBrush penBrush = new SolidBrush(Color.FromArgb(100, 116, 139)))
                {
                    g.DrawString("✎", subFont, penBrush, textX + aliasSize.Width + 3, textY + 22);
                }
            }

            // User ID Row
            string idText = "ID: " + (Account?.UserID > 0 ? Account.UserID.ToString() : "-");
            using (Font idFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (SolidBrush idBrush = new SolidBrush(Color.FromArgb(100, 116, 139)))
            {
                g.DrawString(idText, idFont, idBrush, textX, textY + 39);
            }

            // Right Action Buttons
            DrawPlayButton(g);
            DrawCopyButton(g);
            DrawMoreButton(g);
        }

        private void DrawPlayButton(Graphics g)
        {
            Color pBg = isPlayHovered ? Color.FromArgb(2, 132, 199) : Color.FromArgb(37, 99, 235);
            using (GraphicsPath path = GetRoundedRectangle(playRect, 7))
            {
                using (SolidBrush brush = new SolidBrush(pBg))
                    g.FillPath(brush, path);

                // White Triangle
                int cx = playRect.X + playRect.Width / 2;
                int cy = playRect.Y + playRect.Height / 2;
                Point[] triangle = new Point[]
                {
                    new Point(cx - 4, cy - 6),
                    new Point(cx + 6, cy),
                    new Point(cx - 4, cy + 6)
                };

                using (SolidBrush iconBrush = new SolidBrush(Color.White))
                    g.FillPolygon(iconBrush, triangle);
            }
        }

        private void DrawCopyButton(Graphics g)
        {
            Color cBg = isCopyHovered ? Color.FromArgb(51, 65, 85) : Color.FromArgb(30, 41, 59);
            using (GraphicsPath path = GetRoundedRectangle(copyRect, 7))
            {
                using (SolidBrush brush = new SolidBrush(cBg))
                    g.FillPath(brush, path);

                // Copy Icon (Two Overlapping Squares)
                int cx = copyRect.X + copyRect.Width / 2;
                int cy = copyRect.Y + copyRect.Height / 2;

                using (Pen iconPen = new Pen(Color.FromArgb(203, 213, 225), 1.5f))
                {
                    // Back square
                    g.DrawRectangle(iconPen, cx - 3, cy - 6, 9, 9);
                    // Front square
                    using (SolidBrush fBrush = new SolidBrush(cBg))
                        g.FillRectangle(fBrush, cx - 6, cy - 3, 9, 9);
                    g.DrawRectangle(iconPen, cx - 6, cy - 3, 9, 9);
                }
            }
        }

        private void DrawMoreButton(Graphics g)
        {
            Color mBg = isMoreHovered ? Color.FromArgb(51, 65, 85) : Color.FromArgb(30, 41, 59);
            using (GraphicsPath path = GetRoundedRectangle(moreRect, 7))
            {
                using (SolidBrush brush = new SolidBrush(mBg))
                    g.FillPath(brush, path);

                // Three horizontal dots
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
    }
}

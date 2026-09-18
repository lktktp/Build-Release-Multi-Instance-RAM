using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RBX_Alt_Manager.Classes
{
    /// <summary>
    /// Modern Dark-Gaming account row card matching UI_MOCKUP_REFERENCE.png.
    /// 82px tall, 12px rounded corners, 52px circular avatar, status dot,
    /// username + pill badge, alias + uid rows, blue play btn, grey copy/more btns.
    /// Public API (constructor, CardClicked, PlayClicked, CopyClicked, MoreClicked) unchanged.
    /// </summary>
    public class ModernAccountCard : Control
    {
        // -- Design Tokens -------------------------------------------------------
        private static readonly Color BgIdle         = Color.FromArgb(17, 24, 39);
        private static readonly Color BgHover        = Color.FromArgb(22, 30, 50);
        private static readonly Color BgSelected     = Color.FromArgb(20, 30, 54);
        private static readonly Color BorderIdle      = Color.FromArgb(31, 41, 55);
        private static readonly Color BorderHover     = Color.FromArgb(55, 65, 81);
        private static readonly Color BorderSelected  = Color.FromArgb(37, 99, 235);
        private static readonly Color AccentBlue      = Color.FromArgb(37, 99, 235);
        private static readonly Color AccentBlueHover = Color.FromArgb(59, 130, 246);
        private static readonly Color OnlineGreen     = Color.FromArgb(34, 197, 94);
        private static readonly Color OnlinePillBg    = Color.FromArgb(6, 78, 59);
        private static readonly Color OnlinePillFg    = Color.FromArgb(74, 222, 128);
        private static readonly Color OfflineGray     = Color.FromArgb(100, 116, 139);
        private static readonly Color OfflinePillBg   = Color.FromArgb(30, 41, 59);
        private static readonly Color OfflinePillFg   = Color.FromArgb(148, 163, 184);
        private static readonly Color TextPrimary     = Color.FromArgb(248, 250, 252);
        private static readonly Color TextSecondary   = Color.FromArgb(148, 163, 184);
        private static readonly Color TextMuted       = Color.FromArgb(100, 116, 139);
        private static readonly Color SurfaceBtn      = Color.FromArgb(30, 41, 59);
        private static readonly Color SurfaceBtnHvr   = Color.FromArgb(51, 65, 85);

        public static readonly Dictionary<long, Image> AvatarCache = new Dictionary<long, Image>();

        public Account Account   { get; private set; }
        private CheckBox chkSelect;
        private bool _suppressCheckEvent;
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (chkSelect != null && chkSelect.Checked != value)
                {
                    _suppressCheckEvent = true;
                    chkSelect.Checked = value;
                    _suppressCheckEvent = false;
                }
            }
        }

        public bool HideUsername { get; set; }

        public event EventHandler<Account> CardClicked;
        public event EventHandler<Account> PlayClicked;
        public event EventHandler<Account> CopyClicked;
        public event EventHandler<Point>   MoreClicked;
        public event EventHandler<bool>    SelectionToggled;

        public void RefreshPresenceDisplay()
        {
            _UpdateAccessible();
            Invalidate();
        }

        private bool _hovered, _playHov, _copyHov, _moreHov;
        private Rectangle _playRect, _copyRect, _moreRect;

        private readonly ToolTip _tip = new ToolTip
        {
            AutoPopDelay = 4000, InitialDelay = 400, ReshowDelay = 200, ShowAlways = true
        };

        public ModernAccountCard(Account account)
        {
            Account = account;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint            |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            Height         = 82;
            Cursor         = Cursors.Hand;
            Margin         = new Padding(0, 0, 0, 6);
            TabStop        = true;
            AccessibleRole = AccessibleRole.ListItem;

            chkSelect = new CheckBox
            {
                Location = new Point(12, 31),
                Size = new Size(18, 18),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            chkSelect.CheckedChanged += (s, e) =>
            {
                if (_suppressCheckEvent) return;
                _isSelected = chkSelect.Checked;
                SelectionToggled?.Invoke(this, chkSelect.Checked);
                Invalidate();
            };
            Controls.Add(chkSelect);

            _UpdateAccessible();
            _LoadAvatarAsync();
        }

        private void _UpdateAccessible()
        {
            if (Account == null) return;
            bool online = Account.Presence != null && Account.Presence.userPresenceType != UserPresenceType.Offline;
            AccessibleName = Account.Username + (online ? " [online]" : " [offline]");
        }

        private void _LoadAvatarAsync()
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
                        using (HttpClient http = new HttpClient())
                        {
                            byte[] data = await http.GetByteArrayAsync(url);
                            using (var ms = new MemoryStream(data))
                            {
                                Image img = Image.FromStream(ms);
                                lock (AvatarCache) { AvatarCache[Account.UserID] = img; }
                                if (!IsDisposed) BeginInvoke((MethodInvoker)Invalidate);
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
            int btnH = 36, btnW = 36, playW = 42, btnY = (Height - btnH) / 2;
            _moreRect = new Rectangle(Width - 14 - btnW, btnY, btnW, btnH);
            _copyRect = new Rectangle(_moreRect.X - 8 - btnW, btnY, btnW, btnH);
            _playRect = new Rectangle(_copyRect.X - 8 - playW, btnY, playW, btnH);
            _tip.SetToolTip(this, null);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool op = _playHov, oc = _copyHov, om = _moreHov;
            _playHov = _playRect.Contains(e.Location);
            _copyHov = _copyRect.Contains(e.Location);
            _moreHov = _moreRect.Contains(e.Location);
            if      (_playHov) _tip.SetToolTip(this, "Quick Launch");
            else if (_copyHov) _tip.SetToolTip(this, "Copy Cookie");
            else if (_moreHov) _tip.SetToolTip(this, "More Options");
            else               _tip.SetToolTip(this, null);
            if (op != _playHov || oc != _copyHov || om != _moreHov) Invalidate();
        }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true;  Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = _playHov = _copyHov = _moreHov = false; Invalidate(); }
        protected override void OnGotFocus(EventArgs e)   { base.OnGotFocus(e);  Invalidate(); }
        protected override void OnLostFocus(EventArgs e)  { base.OnLostFocus(e); Invalidate(); }

        protected override bool IsInputKey(Keys k)
        {
            if (k == Keys.Enter || k == Keys.Space) return true;
            return base.IsInputKey(k);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter) CardClicked?.Invoke(this, Account);
            if (e.KeyCode == Keys.Space) PlayClicked?.Invoke(this, Account);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            Focus();
            if (e.Button == MouseButtons.Right) { MoreClicked?.Invoke(this, PointToScreen(e.Location)); return; }
            if      (_playRect.Contains(e.Location)) PlayClicked?.Invoke(this, Account);
            else if (_copyRect.Contains(e.Location)) CopyClicked?.Invoke(this, Account);
            else if (_moreRect.Contains(e.Location)) MoreClicked?.Invoke(this, PointToScreen(new Point(_moreRect.Left, _moreRect.Bottom)));
            else                                     CardClicked?.Invoke(this, Account);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            Rectangle bounds = new Rectangle(1, 1, Width - 3, Height - 3);
            Color bg  = IsSelected ? BgSelected : (_hovered ? BgHover : BgIdle);
            Color brd = IsSelected ? BorderSelected : (_hovered ? BorderHover : BorderIdle);
            int   bw  = IsSelected ? 2 : 1;
            using (GraphicsPath gp = GetRoundedRectangle(bounds, 12))
            {
                using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, gp);
                using (Pen p = new Pen(brd, bw))          g.DrawPath(p, gp);
            }
            if (Focused)
            {
                using (GraphicsPath fp = GetRoundedRectangle(bounds, 12))
                using (Pen pen = new Pen(Color.FromArgb(160, AccentBlueHover), 1.5f) { DashStyle = DashStyle.Dot })
                    g.DrawPath(pen, fp);
            }
            if (IsSelected)
            {
                using (SolidBrush ab = new SolidBrush(AccentBlue))
                    g.FillRectangle(ab, new Rectangle(1, 10, 3, Height - 20));
            }
            int avSz = 52, avX = 38, avY = (Height - avSz) / 2;
            Rectangle avRect = new Rectangle(avX, avY, avSz, avSz);
            _DrawAvatar(g, avRect, bg);
            bool online = Account?.Presence != null && Account.Presence.userPresenceType != UserPresenceType.Offline;
            _DrawStatusDot(g, avRect, online, bg);
            int txtX = avX + avSz + 14, txtY = 12;
            string uname = (Account != null) ? (HideUsername ? "........" : Account.Username) : "Unknown";
            using (Font uf = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (SolidBrush ub = new SolidBrush(TextPrimary))
            {
                g.DrawString(uname, uf, ub, txtX, txtY);
                SizeF us = g.MeasureString(uname, uf);
                _DrawPill(g, online, (int)(txtX + us.Width + 6), txtY + 2);
            }
            string alias = "Alias:  " + (!string.IsNullOrEmpty(Account?.Alias) ? Account.Alias : "-");
            using (Font af = new Font("Segoe UI", 8.75f))
            using (SolidBrush asb = new SolidBrush(TextSecondary))
                g.DrawString(alias, af, asb, txtX, txtY + 26);
            string uid = Account?.UserID > 0 ? Account.UserID.ToString() : "-";
            using (Font idf = new Font("Segoe UI", 8.75f))
            using (SolidBrush idb = new SolidBrush(TextMuted))
                g.DrawString(uid, idf, idb, txtX, txtY + 44);
            _DrawPlayBtn(g);
            _DrawCopyBtn(g);
            _DrawMoreBtn(g);
        }

        private void _DrawAvatar(Graphics g, Rectangle r, Color cardBg)
        {
            using (GraphicsPath clip = new GraphicsPath())
            {
                clip.AddEllipse(r);
                g.SetClip(clip);
                Image img = null;
                if (Account != null) AvatarCache.TryGetValue(Account.UserID, out img);
                if (img != null)
                {
                    g.DrawImage(img, r);
                }
                else
                {
                    using (SolidBrush fb = new SolidBrush(Color.FromArgb(30, 41, 59)))
                        g.FillEllipse(fb, r);
                    string ch = (Account != null && !string.IsNullOrEmpty(Account.Username))
                        ? Account.Username.Substring(0, 1).ToUpper() : "?";
                    using (Font f = new Font("Segoe UI", 18f, FontStyle.Bold))
                    using (SolidBrush b = new SolidBrush(TextSecondary))
                    {
                        SizeF sz = g.MeasureString(ch, f);
                        g.DrawString(ch, f, b, r.X + (r.Width - sz.Width) / 2f, r.Y + (r.Height - sz.Height) / 2f);
                    }
                }
                g.ResetClip();
            }
            using (Pen ring = new Pen(Color.FromArgb(45, 55, 72), 1.5f))
                g.DrawEllipse(ring, r);
        }

        private void _DrawStatusDot(Graphics g, Rectangle avRect, bool online, Color bg)
        {
            int ds = 13;
            Rectangle dot = new Rectangle(avRect.Right - ds, avRect.Bottom - ds, ds, ds);
            using (SolidBrush halo = new SolidBrush(bg))
                g.FillEllipse(halo, new Rectangle(dot.X - 2, dot.Y - 2, dot.Width + 4, dot.Height + 4));
            using (SolidBrush db = new SolidBrush(online ? OnlineGreen : OfflineGray))
                g.FillEllipse(db, dot);
            using (SolidBrush hl = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                g.FillEllipse(hl, new Rectangle(dot.X + 2, dot.Y + 1, 5, 4));
        }

        private void _DrawPill(Graphics g, bool online, int x, int y)
        {
            string txt = online ? "online" : "offline";
            Color pbg  = online ? OnlinePillBg : OfflinePillBg;
            Color pfg  = online ? OnlinePillFg : OfflinePillFg;
            using (Font pf = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            {
                SizeF sz = g.MeasureString(txt, pf);
                Rectangle pr = new Rectangle(x, y, (int)sz.Width + 16, 18);
                using (GraphicsPath pp = GetRoundedRectangle(pr, 9))
                using (SolidBrush bb  = new SolidBrush(pbg))
                using (SolidBrush tb  = new SolidBrush(pfg))
                {
                    g.FillPath(bb, pp);
                    using (SolidBrush dot = new SolidBrush(online ? OnlineGreen : OfflineGray))
                        g.FillEllipse(dot, x + 5, y + 6, 5, 5);
                    g.DrawString(txt, pf, tb, x + 13, y + 2);
                }
            }
        }

        private void _DrawPlayBtn(Graphics g)
        {
            Color bg = _playHov ? AccentBlueHover : AccentBlue;
            using (GraphicsPath p = GetRoundedRectangle(_playRect, 8))
            using (SolidBrush b  = new SolidBrush(bg))
                g.FillPath(b, p);
            int cx = _playRect.X + _playRect.Width / 2 + 1;
            int cy = _playRect.Y + _playRect.Height / 2;
            Point[] tri = { new Point(cx - 5, cy - 7), new Point(cx + 7, cy), new Point(cx - 5, cy + 7) };
            using (SolidBrush wb = new SolidBrush(Color.White))
                g.FillPolygon(wb, tri);
        }

        private void _DrawCopyBtn(Graphics g)
        {
            Color bg = _copyHov ? SurfaceBtnHvr : SurfaceBtn;
            using (GraphicsPath p = GetRoundedRectangle(_copyRect, 8))
            {
                using (SolidBrush b = new SolidBrush(bg))          g.FillPath(b, p);
                using (Pen bd = new Pen(Color.FromArgb(55, 65, 81), 1f)) g.DrawPath(bd, p);
            }
            int cx = _copyRect.X + _copyRect.Width / 2;
            int cy = _copyRect.Y + _copyRect.Height / 2;
            using (Pen ip = new Pen(Color.FromArgb(203, 213, 225), 1.5f))
            {
                g.DrawRectangle(ip, cx - 2, cy - 7, 9, 9);
                using (SolidBrush fb = new SolidBrush(bg)) g.FillRectangle(fb, cx - 6, cy - 3, 9, 9);
                g.DrawRectangle(ip, cx - 6, cy - 3, 9, 9);
            }
        }

        private void _DrawMoreBtn(Graphics g)
        {
            Color bg = _moreHov ? SurfaceBtnHvr : SurfaceBtn;
            using (GraphicsPath p = GetRoundedRectangle(_moreRect, 8))
            {
                using (SolidBrush b = new SolidBrush(bg))          g.FillPath(b, p);
                using (Pen bd = new Pen(Color.FromArgb(55, 65, 81), 1f)) g.DrawPath(bd, p);
            }
            int cx = _moreRect.X + _moreRect.Width / 2;
            int cy = _moreRect.Y + _moreRect.Height / 2;
            using (SolidBrush db = new SolidBrush(Color.FromArgb(203, 213, 225)))
            {
                g.FillEllipse(db, cx - 8, cy - 2, 4, 4);
                g.FillEllipse(db, cx - 2, cy - 2, 4, 4);
                g.FillEllipse(db, cx + 4, cy - 2, 4, 4);
            }
        }

        public static GraphicsPath GetRoundedRectangle(Rectangle r, int radius)
        {
            int d = radius * 2;
            if (d > r.Width)  d = r.Width;
            if (d > r.Height) d = r.Height;
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(r.X,         r.Y,          d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y,          d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d,   0, 90);
            gp.AddArc(r.X,         r.Bottom - d, d, d,  90, 90);
            gp.CloseFigure();
            return gp;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tip?.Dispose();
            base.Dispose(disposing);
        }
    }
}

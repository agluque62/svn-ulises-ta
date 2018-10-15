using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

using HMI.Model.Module.Properties;

using Utilities;

namespace HMI.Model.Module.UI
{
    public class UrrRdButton : Control
    {
        private const int _LongClickMs = 2000;

        private BtnState _State = BtnState.Normal;
        private BtnState _StateBT1 = BtnState.Normal;
        private BtnState _StateBT2 = BtnState.Normal;

        private int _Id = 0;
        private Rectangle _PttRect = new Rectangle();
        private Rectangle _SquelchRect = new Rectangle();
        private Font _SmallFont = new Font("Microsoft Sans Serif", 7.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        private Font _SmallFontBold = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        private Font _BigFont = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        private Font _UrrBigFont = new Font("Arial Black", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        private Image _PttImage = null;
        private Image _SquelchImage = null;
        private string _Frecuency = "";
        private string _Alias = "";
        private int _RtxGroup = 0;
        private Timer _Timer = new Timer();
        private bool _DrawX = false;
        private bool _AllAsOneBt = false;
        private BtnInfo _BtnInfo = new BtnInfo();
        private BtnInfo _TxBtnInfo = new BtnInfo();
        private BtnInfo _RxBtnInfo = new BtnInfo();
        private int _QidxValue = -1;
        //Color de fondo de la parte superior de la tecla de radio. 
        //Por defecto es gris, pero puede estar en degradado, por ejemplo.
        private Color _CurrentBackColor = VisualStyle.ButtonColor;

        public new event EventHandler Click;
        public event EventHandler TxClick;
        public event EventHandler RxShortClick;
        public event EventHandler RxLongClick;

        public bool pttPushedError = false;
        [Browsable(false),
        DefaultValue(0)
        ]
        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        [Browsable(false),
        DefaultValue(typeof(Color), "Transparent")
        ]
        public new Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        [Browsable(false),
        DefaultValue(null)
        ]
        public new Image BackgroundImage
        {
            get { return base.BackgroundImage; }
            set { base.BackgroundImage = value; }
        }

        [Browsable(false),
        DefaultValue(typeof(ImageLayout), "Tile")
        ]
        public new ImageLayout BackgroundImageLayout
        {
            get { return base.BackgroundImageLayout; }
            set { base.BackgroundImageLayout = value; }
        }

        [Browsable(false),
        DefaultValue(null)
        ]
        public Image PttImage
        {
            get { return _PttImage; }
            set
            {
                _PttImage = value;
                Invalidate(_PttRect);
            }
        }

        [Browsable(false),
        DefaultValue(null)
        ]
        public Image SquelchImage
        {
            get { return _SquelchImage; }
            set
            {
                _SquelchImage = value;
                Invalidate(_SquelchRect);
            }
        }

        [Browsable(false),
        DefaultValue(typeof(Color), "Control")
        ]
        public Color TitleBackColor
        {
            get { return _BtnInfo.GetBackColor(BtnState.Normal); }
            set
            {
                _BtnInfo.SetBackColor(BtnState.Normal, value);
                Invalidate();
            }
        }

        [Category("Appearance"),
        Description("The radius for the button corners. The " +
             "greater this value is, the more 'smooth' " +
             "the corners are. This property should " +
             "not be greater than half of the " +
             "controls height."),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(8)
        ]
        public int GeneralCornerRadius
        {
            get { return _BtnInfo.CornerRadius; }
            set
            {
                _BtnInfo.CornerRadius = value;
                Invalidate();
            }
        }
        public int TxCornerRadius
        {
            get { return _BtnInfo.CornerRadius; }
            set
            {
                _TxBtnInfo.CornerRadius = value;
                Invalidate();
            }
        }
        public int RxCornerRadius
        {
            get { return _BtnInfo.CornerRadius; }
            set
            {
                _RxBtnInfo.CornerRadius = value;
                Invalidate();
            }
        }
        public new Color CurrentBackColor
        {
            get { return _CurrentBackColor; }
            set { _CurrentBackColor = value; }
        }

        public UrrRdButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;

            _Timer.Interval = _LongClickMs;
            _Timer.Tick += OnLongClick;

            _TxBtnInfo.Text = "";
            _TxBtnInfo.Font = _UrrBigFont;
            _TxBtnInfo.SetForeColor(BtnState.Normal, Color.Black);
            _RxBtnInfo.Text = "";
            _RxBtnInfo.Font = _UrrBigFont;
            _RxBtnInfo.SetForeColor(BtnState.Normal, Color.Black);
        }

        //
        // Resumen:
        //    Establece el texto dentro de la parte (TX) del control.
        //
        // Parámetros:
        //   text:
        //     Cadena que se va a establecer.
        public void changeTxText(string text)
        {
            _TxBtnInfo.Text = text;
        }
        public void setRtxErrorColor()
        {
            _TxBtnInfo.SetBackColor(BtnState.Normal, VisualStyle.Colors.Red);
            _TxBtnInfo.SetForeColor(BtnState.Normal, HMI.Presentation.Urr.UI.VisualStyle.Colors.DarkRed);
            Invalidate();
        }

        public void Reset(string frecuency, string alias, bool drawX, bool allAsOneBt, int rtxGroup, Image ptt, Image squelch, Image audio, Color title, Color tx, Color rx, Color txForeColor, Color rxForeColor, Color titleForeColor,
            string qidxResource, uint qidxValue, bool degradedState = false)
        {
            //_Alias = qidxResource;
            _QidxValue = (int)qidxValue;

            Reset(frecuency, alias, drawX, allAsOneBt, rtxGroup, ptt, squelch, audio, title, tx, rx, txForeColor, rxForeColor, titleForeColor, degradedState);
        }

        public void Reset(string frecuency, string alias, bool drawX, bool allAsOneBt, int rtxGroup, Image ptt, Image squelch, Image audio, Color title, Color tx, Color rx, Color txForeColor, Color rxForeColor, Color titleForeColor, bool degradedState = false)
        {
            _Frecuency = frecuency;
            _Alias = alias.Length > 11 ? (alias.Substring(0, 8) + "...") : alias;
            _RtxGroup = rtxGroup;
            _PttImage = ptt;
            _SquelchImage = squelch;

            ForeColor = titleForeColor;
            //            BackColor = titleForeColor;
            _CurrentBackColor = degradedState ? Color.OrangeRed : VisualStyle.ButtonColor;
            // El amarillo (formacion de retransmision) y rojo (para las falsas maniobras) tienen preferencia sobre el color de fondo.
            _BtnInfo.SetBackColor(BtnState.Normal, (title == VisualStyle.Colors.Yellow || title == VisualStyle.Colors.Red ||
                title == HMI.Presentation.Urr.UI.VisualStyle.Colors.DarkGreen ||
                title == HMI.Presentation.Urr.UI.VisualStyle.Colors.HeaderBlueA1) ? title : _CurrentBackColor);
            _TxBtnInfo.SetBackColor(BtnState.Normal, tx);
            _TxBtnInfo.SetForeColor(BtnState.Normal, txForeColor);
            _RxBtnInfo.SetBackColor(BtnState.Normal, rx);
            _RxBtnInfo.SetForeColor(BtnState.Normal, rxForeColor);
            _RxBtnInfo.SetImage(BtnState.Normal, audio);
            _RxBtnInfo.Text = audio == null ? "" : "";

            _DrawX = drawX;

            if (allAsOneBt != _AllAsOneBt)
            {
                _Timer.Enabled = false;
                _State = BtnState.Normal;
                _StateBT1 = BtnState.Normal;
                _StateBT2 = BtnState.Normal;
                _AllAsOneBt = allAsOneBt;
            }

            Invalidate();
        }

        public void EnableTx(bool enable)
        {
            _StateBT1 = enable ? BtnState.Normal : BtnState.Inactive;
            _TxBtnInfo.SetBackColor(BtnState.Inactive, _TxBtnInfo.GetBackColor(BtnState.Inactive));
            _TxBtnInfo.SetForeColor(BtnState.Inactive, _TxBtnInfo.GetForeColor(BtnState.Inactive));

            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            Rectangle rect = Rectangle.Inflate(ClientRectangle, 1, 1);
            using (GraphicsPath rr = BtnRenderer.GetRoundedRect(rect, _BtnInfo.CornerRadius))
            {
                Region = new Region(rr);
            }

            _BtnInfo.Rect = ClientRectangle;

            int top = (int)(Height * 0.35);
            int width = (int)(Width / 2);

            _PttRect = new Rectangle(1, 4, 25, 25);
            _SquelchRect = new Rectangle(Width - 22, 4, 25, 25);
            _TxBtnInfo.Rect = new Rectangle(0, top, width, Height - top);
            _RxBtnInfo.Rect = new Rectangle(width, top, Width - width, Height - top);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_State == BtnState.Normal)
            {
                _State = BtnState.MouseOver;
            }

            if (_TxBtnInfo.Rect.Contains(e.Location))
            {
                if ((_StateBT1 != BtnState.MouseOver) && (_StateBT1 != BtnState.Pushed) && (_StateBT1 != BtnState.Inactive))
                {
                    _StateBT1 = BtnState.MouseOver;
                    Invalidate(_TxBtnInfo.Rect);
                }
            }
            else if ((_StateBT1 == BtnState.MouseOver) || (_StateBT1 == BtnState.Pushed))
            {
                _StateBT1 = BtnState.Normal;
                Invalidate(_TxBtnInfo.Rect);
            }

            if (_RxBtnInfo.Rect.Contains(e.Location))
            {
                if ((_StateBT2 != BtnState.MouseOver) && (_StateBT2 != BtnState.Pushed))
                {
                    _StateBT2 = BtnState.MouseOver;
                    Invalidate(_RxBtnInfo.Rect);
                }
            }
            else if ((_StateBT2 == BtnState.MouseOver) || (_StateBT2 == BtnState.Pushed))
            {
                _Timer.Enabled = false;
                _StateBT2 = BtnState.Normal;
                Invalidate(_RxBtnInfo.Rect);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Capture = false;

            _State = BtnState.Pushed;
            if (_StateBT1 == BtnState.MouseOver)
            {
                _StateBT1 = BtnState.Pushed;
            }
            else if (_StateBT2 == BtnState.MouseOver)
            {
                _StateBT2 = BtnState.Pushed;
                _Timer.Enabled = !_AllAsOneBt;
            }

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            bool clicked = _State == BtnState.Pushed;
            bool clickedBT1 = _StateBT1 == BtnState.Pushed;
            bool clickedBT2 = _StateBT2 == BtnState.Pushed;

            if (clicked)
            {
                _Timer.Enabled = false;
                _State = BtnState.MouseOver;
                _StateBT1 = clickedBT1 ? BtnState.MouseOver : _StateBT1; // BtnState.Normal;
                _StateBT2 = clickedBT2 || (_StateBT2 == BtnState.MouseOver) ? BtnState.MouseOver : BtnState.Normal;

                Invalidate();
            }

            EventHandler ev = _AllAsOneBt && clicked ? Click : clickedBT1 ? TxClick : clickedBT2 ? RxShortClick : null;
            if (ev != null)
            {
                ev(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            bool clicked = _State == BtnState.Pushed;
            _State = BtnState.Normal;

            if (_AllAsOneBt)
            {
                _StateBT1 = BtnState.Normal;
                _StateBT2 = BtnState.Normal;

                if (clicked)
                {
                    Invalidate();
                }
            }
            else if (_StateBT1 != BtnState.Normal && _StateBT1 != BtnState.Inactive)
            {
                _StateBT1 = BtnState.Normal;
                Invalidate(_TxBtnInfo.Rect);
            }
            else if (_StateBT2 != BtnState.Normal)
            {
                _Timer.Enabled = false;
                _StateBT2 = BtnState.Normal;
                Invalidate(_RxBtnInfo.Rect);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //			base.OnPaint(e);

            BtnState st = !Enabled ? BtnState.Inactive :
                (_State == BtnState.Pushed) && _AllAsOneBt ? BtnState.Pushed : BtnState.Normal;

            BtnState stBT1 = !Enabled || _StateBT1 == BtnState.Inactive ? BtnState.Inactive :
                (_StateBT1 == BtnState.Pushed) || (st == BtnState.Pushed) ? BtnState.Pushed :
                (_StateBT1 == BtnState.MouseOver) && !_AllAsOneBt ? BtnState.MouseOver : BtnState.Normal;

            BtnState stBT2 = !Enabled ? BtnState.Inactive :
                (_StateBT2 == BtnState.Pushed) || (st == BtnState.Pushed) ? BtnState.Pushed :
                (_StateBT2 == BtnState.MouseOver) && !_AllAsOneBt ? BtnState.MouseOver : BtnState.Normal;

            BtnRenderer.Draw(e.Graphics, _BtnInfo[st]);

            //Pintar PTT
            if (_PttImage != null)
            {
                e.Graphics.DrawImage(_PttImage, _PttRect.X, _PttRect.Y);
            }
            //Pintar Squlech
            if (_SquelchImage != null)
            {
                e.Graphics.DrawImage(_SquelchImage, _SquelchRect.X, _SquelchRect.Y);

                if (_QidxValue >= 0)
                {
                    Rectangle txtRect = ClientRectangle;
                    txtRect.Offset(32, 2);
                    BtnRenderer.DrawString(e.Graphics, txtRect, Color.Transparent, st, _QidxValue.ToString(), _SmallFontBold, ContentAlignment.TopCenter, Color.Black);
                }
            }

            BtnRenderer.Draw(e.Graphics, _TxBtnInfo[stBT1]);
            BtnRenderer.Draw(e.Graphics, _RxBtnInfo[stBT2]);

            //Para pintar los fuera de servicio
            if (_DrawX)
            {
                using (Pen p = new Pen(Color.Red, 3))
                {
                    e.Graphics.DrawLine(p, 6, 38, Width - 6, Height - 6);
                    e.Graphics.DrawLine(p, Width - 6, 38, 6, Height - 6);
                }
            }

            Rectangle textRect = ClientRectangle;
            textRect.Offset(0, -5);
            BtnRenderer.DrawString(e.Graphics, textRect, _BtnInfo.GetBackColor(st), st, _Frecuency, _SmallFontBold, ContentAlignment.TopCenter, ForeColor);
            textRect.Offset(0, 13);
            BtnRenderer.DrawString(e.Graphics, textRect, _BtnInfo.GetBackColor(st), st, _Alias, _SmallFont, ContentAlignment.TopCenter, ForeColor);

            if (_RtxGroup > 0)
            {
                string rtxGroup = ((char)('G' + _RtxGroup - 1)).ToString();
                e.Graphics.DrawString(rtxGroup, _SmallFontBold, Brushes.Black, 3, _TxBtnInfo.Rect.Top - 15);
            }
            else if (_RtxGroup == -1)
            {
                e.Graphics.DrawString("R", _SmallFontBold, Brushes.Black, Width - 15, _TxBtnInfo.Rect.Top - 15);
            }

            using (Pen linePen = new Pen(Enabled ? _BtnInfo.GetBorderColor(BtnState.Normal) : _BtnInfo.GetBorderColor(BtnState.Inactive), 2))
            {
                e.Graphics.DrawLine(linePen, 1, _TxBtnInfo.Rect.Top, Width - 1, _TxBtnInfo.Rect.Top);
            }
        }

        private void OnLongClick(object sender, EventArgs e)
        {
            if (_Timer.Enabled)
            {
                _Timer.Enabled = false;
                _State = BtnState.MouseOver;
                _StateBT2 = BtnState.MouseOver;

                Invalidate(_RxBtnInfo.Rect);

                General.SafeLaunchEvent(RxLongClick, this);
            }
        }
    }
}

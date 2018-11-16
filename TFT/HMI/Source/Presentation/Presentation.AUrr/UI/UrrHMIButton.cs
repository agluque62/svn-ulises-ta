using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Utilities;

namespace HMI.Model.Module.UI
{
    public class UrrHMIButton : System.Windows.Forms.Button
    {
        Color clr1, clr2;
        private Color color1 = Color.Gainsboro;
        private Color color2 = Color.DarkGray;
        private Color m_hovercolor1 = Color.Gainsboro;
        private Color m_hovercolor2 = Color.DarkGray;
        private int color1Transparent = 250;
        private int color2Transparent = 250;
        private Color clickcolor1 = Color.White;
        private Color clickcolor2 = Color.Gainsboro;
        private int angle = 90;
        private int textX = 0;
        private int textY = 0;
        private String text = "";
        public Color buttonborder_1 = Color.FromArgb(220, 220, 220);
        public Color buttonborder_2 = Color.FromArgb(150, 150, 150);
        public Boolean showButtonText = true;
        public int borderWidth = 2;
        public Color borderColor = Color.Black;

        private int _Id = 0;
        private Timer _Timer = new Timer();
        private bool _DrawX = false;
        private BtnState _BtnState = BtnState.Normal;
        protected BtnInfo _BtnInfo = new BtnInfo();
        private int _LongClickMs = 2000;

        public new event EventHandler Click;
        public event EventHandler LongClick;

        //Available forms -- Only Left Arrow or Triangle
        public enum ButtonsShapes
        {
            Rect,
            LeftTriangle,
            RightTriangle
        }

        ButtonsShapes buttonShape;

        public ButtonsShapes ButtonShape
        {
            get { return buttonShape; }
            set
            {
                buttonShape = value; Invalidate();
            }
        }

        [Browsable(false),
        DefaultValue(0)
        ]
        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        [Browsable(false),
        DefaultValue(false)
        ]
        public bool DrawX
        {
            get { return _DrawX; }
            set
            {
                if (_DrawX != value)
                {
                    _DrawX = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false),
        DefaultValue(typeof(BtnState), "Normal")
        ]
        public BtnState BtnState
        {
            get { return _BtnState; }
            set
            {
                _BtnState = value;
                Invalidate();
            }
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

        [CategoryAttribute("Appearance"),
        Description("The text associate with the control"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue("")
        ]
        public new string Text
        {
            get { return _BtnInfo.Text; }
            set
            {
                _BtnInfo.Text = value;
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the border around the button when is disabled"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "Gainsboro")
        ]
        public Color BorderColorDisabled
        {
            get { return _BtnInfo.GetBorderColor(BtnState.Inactive); }
            set
            {
                _BtnInfo.SetBorderColor(BtnState.Inactive, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the border around the button when is in normal state"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "Black")
        ]
        public Color BorderColor
        {
            get { return _BtnInfo.GetBorderColor(BtnState.Normal); }
            set
            {
                _BtnInfo.SetBorderColor(BtnState.Normal, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the border around the button when mouse is over"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "Black")
        ]
        public Color BorderColorMouseOver
        {
            get { return _BtnInfo.GetBorderColor(BtnState.MouseOver); }
            set
            {
                _BtnInfo.SetBorderColor(BtnState.MouseOver, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the border around the button when is pushed"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "Black")
        ]
        public Color BorderColorPushed
        {
            get { return _BtnInfo.GetBorderColor(BtnState.Pushed); }
            set
            {
                _BtnInfo.SetBorderColor(BtnState.Pushed, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the button when is disabled"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "Control")
        ]
        public Color ButtonColorDisabled
        {
            get { return _BtnInfo.GetBackColor(BtnState.Inactive); }
            set
            {
                _BtnInfo.SetBackColor(BtnState.Inactive, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the button when is in normal state"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "Gainsboro")
        ]
        public Color ButtonColor
        {
            get { return _BtnInfo.GetBackColor(BtnState.Normal); }
            set
            {
                _BtnInfo.SetBackColor(BtnState.Normal, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the button when mouse is over"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "Gainsboro")
        ]
        public Color ButtonColorMouseOver
        {
            get { return _BtnInfo.GetBackColor(BtnState.MouseOver); }
            set
            {
                _BtnInfo.SetBackColor(BtnState.MouseOver, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the button when is pushed"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "Gainsboro")
        ]
        public Color ButtonColorPushed
        {
            get { return _BtnInfo.GetBackColor(BtnState.Pushed); }
            set
            {
                _BtnInfo.SetBackColor(BtnState.Pushed, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the inner border when the button is disabled"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "WhiteSmoke")
        ]
        public Color InnerBorderColorDisabled
        {
            get { return _BtnInfo.GetInnerBorderColor(BtnState.Inactive); }
            set
            {
                _BtnInfo.SetInnerBorderColor(BtnState.Inactive, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the inner border when the button is in normal state"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "LightGray")
        ]
        public Color InnerBorderColor
        {
            get { return _BtnInfo.GetInnerBorderColor(BtnState.Normal); }
            set
            {
                _BtnInfo.SetInnerBorderColor(BtnState.Normal, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the inner border when the mouse is over the button"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "LightGray")
        ]
        public Color InnerBorderColorMouseOver
        {
            get { return _BtnInfo.GetInnerBorderColor(BtnState.MouseOver); }
            set
            {
                _BtnInfo.SetInnerBorderColor(BtnState.MouseOver, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Color of the inner border when the button is pushed"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(Color), "Gray")
        ]
        public Color InnerBorderColorPushed
        {
            get { return _BtnInfo.GetInnerBorderColor(BtnState.Pushed); }
            set
            {
                _BtnInfo.SetInnerBorderColor(BtnState.Pushed, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Image to be displayed while the button is disabled"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(null)
        ]
        public Image ImageDisabled
        {
            get { return _BtnInfo.GetImage(BtnState.Inactive); }
            set
            {
                _BtnInfo.SetImage(BtnState.Inactive, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Image to be displayed while the button state is in normal state"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(null)
        ]
        public Image ImageNormal
        {
            get { return _BtnInfo.GetImage(BtnState.Normal); }
            set
            {
                _BtnInfo.SetImage(BtnState.Normal, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Image to be displayed while the button state is MouseOver"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(null)
        ]
        public Image ImageMouseOver
        {
            get { return _BtnInfo.GetImage(BtnState.MouseOver); }
            set
            {
                _BtnInfo.SetImage(BtnState.MouseOver, value);
                Invalidate();
            }
        }

        [CategoryAttribute("Appearance"),
        Description("Image to be displayed while the button state is pushed"),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(null)
        ]
        public Image ImagePushed
        {
            get { return _BtnInfo.GetImage(BtnState.Pushed); }
            set
            {
                _BtnInfo.SetImage(BtnState.Pushed, value);
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
        DefaultValue(4)
        ]
        public int CornerRadius
        {
            get { return _BtnInfo.CornerRadius; }
            set
            {
                _BtnInfo.CornerRadius = value;
                Invalidate();
            }
        }

        [Category("Appearance"),
        Description("The alignment of the button text that is displayed on the control."),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(ContentAlignment), "MiddleCenter")
        ]
        public ContentAlignment TextAlign
        {
            get { return _BtnInfo.TextAlign; }
            set
            {
                _BtnInfo.TextAlign = value;
                Invalidate();
            }
        }

        [Category("Appearance"),
        Description("The alignment of the image in relation to the button."),
        RefreshProperties(RefreshProperties.Repaint),
        DefaultValue(typeof(ContentAlignment), "MiddleCenter")
        ]
        public ContentAlignment ImageAlign
        {
            get { return _BtnInfo.ImageAlign; }
            set
            {
                _BtnInfo.ImageAlign = value;
                Invalidate();
            }
        }

        [Category("Behavior"),
        Description("Time (Ms) for long click."),
        DefaultValue(2000)
        ]
        public int LongClickTout
        {
            get { return _LongClickMs; }
            set { _LongClickMs = value; }
        }

        public UrrHMIButton()
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
        }

        public void Reset(string text, bool drawX, Color buttonColor)
        {
            _DrawX = drawX;
            _BtnInfo.Text = text;
            _BtnInfo.SetBackColor(BtnState.Normal, buttonColor);

            Invalidate();
        }

        //////////////////////////////////////////////////////////////
        //draw left triangle function  VMG 30/01/18
        void DrawLeftTriangle(Graphics g)
        {
            //Colores para el degradado
            Color c1 = Color.FromArgb(color1Transparent, color1);
            Color c2 = Color.FromArgb(color2Transparent, color2);

            //Pintamos el rectángulo que va a contener el área seleccionable
            Brush b = new SolidBrush(SystemColors.ButtonFace);
            g.FillRectangle(b, 0, 0, this.Width, this.Height);

            //Degradado
            Brush brush2 = new LinearGradientBrush(ClientRectangle, c1, c2, angle);

            //Los tres puntos que representan al triángulo
            Point[] points = { 
                new Point(this.Width, this.Height), 
                new Point(0, this.Height/2), 
                new Point(this.Width, 0) 
            };
            g.FillPolygon(brush2, points);

            //Con esto pintamos el grosor de los bordes. i representa el tamaño del grosor que nos mandan
            // incrementamos o decrementamos los segmentos en base a i (grosor)
            for (int i = 0; i < borderWidth; i++)
            {
                //
                g.DrawLine(new Pen(buttonborder_1), this.Width - i - 1, 0 + i, this.Width - i - 1, this.Height - i);
                //
                g.DrawLine(new Pen(buttonborder_1), 0 + i, (this.Height / 2), (this.Width) - (i / 2), 0 + i);
                //
                g.DrawLine(new Pen(buttonborder_1), 0 + i, (this.Height / 2), this.Width, this.Height - i);
            }

            //Mostrar texto asociado
            if (showButtonText)
            {
                // Create font and brush.
                //Font drawFont = new Font("Arial", 16);
                SolidBrush drawBrush = new SolidBrush(Color.Black);

                // Create point for upper-left corner of drawing.
                Point drawPoint = new Point(textX, textY);

                // Draw string to screen.
                g.DrawString(text, this.Font, drawBrush, drawPoint);
            }

            b.Dispose();
        }

        //////////////////////////////////////////////////////////////
        //draw right triangle function  VMG 30/01/18
        void DrawRightTriangle(Graphics g)
        {
            //Colores para el degradado
            Color c1 = Color.FromArgb(color1Transparent, color1);
            Color c2 = Color.FromArgb(color2Transparent, color2);

            //Pintamos el rectángulo que va a contener el área seleccionable
            Brush b = new SolidBrush(SystemColors.ButtonFace);
            g.FillRectangle(b, 0, 0, this.Width, this.Height);

            //Degradado
            Brush brush2 = new LinearGradientBrush(ClientRectangle, c1, c2, angle);

            //Los tres puntos que representan al triángulo
            Point[] points = { 
                new Point(this.Width, this.Height/2), 
                new Point(0, this.Height), 
                new Point(0, 0) 
            };
            g.FillPolygon(brush2, points);

            //Con esto pintamos el grosor de los bordes. i representa el tamaño del grosor que nos mandan
            // incrementamos o decrementamos los segmentos en base a i (grosor)
            for (int i = 0; i < borderWidth; i++)
            {
                g.DrawLine(new Pen(buttonborder_1), 0 + i, 0, this.Width + i, (this.Height / 2));
                g.DrawLine(new Pen(buttonborder_1), this.Width, (this.Height / 2) - i, 0, this.Height - i);
                g.DrawLine(new Pen(buttonborder_1), 0 + i, 0 + i, 0 + i, this.Height + i);
            }

            //Mostrar texto asociado
            if (showButtonText)
            {
                // Create font and brush.
                //Font drawFont = new Font("Arial", 16);
                SolidBrush drawBrush = new SolidBrush(Color.Black);

                // Create point for upper-left corner of drawing.
                Point drawPoint = new Point(textX, textY);

                // Draw string to screen.
                g.DrawString(text, this.Font, drawBrush, drawPoint);
            }

            b.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            switch (buttonShape)
            {
                case ButtonsShapes.Rect:
                    BtnRenderer.Draw(e.Graphics, _BtnInfo[_BtnState], _DrawX);
                    break;

                case ButtonsShapes.LeftTriangle:
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    this.DrawLeftTriangle(e.Graphics);
                    break;
                case ButtonsShapes.RightTriangle:
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    this.DrawRightTriangle(e.Graphics);
                    break;
            }


            /*
            if (_DrawX)
            {
                using (Pen p = new Pen(Color.Red, 5))
                {
                    e.Graphics.DrawLine(p, 6, 6, Width - 6, Height - 6);
                    e.Graphics.DrawLine(p, Width - 6, 6, 6, Height - 6);
                }
            }
             */
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
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_BtnState != BtnState.Inactive)
            {
                base.OnMouseMove(e);

                if (_BtnState == BtnState.Pushed)
                {
                    if (!ClientRectangle.Contains(e.Location))
                    {
                        _Timer.Enabled = false;
                        base.OnMouseUp(e);

                        _BtnState = BtnState.Normal;
                        Capture = false;

                        Invalidate();
                    }
                }
                else if (_BtnState != BtnState.MouseOver)
                {
                    _BtnState = BtnState.MouseOver;
                    Invalidate();
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if ((_BtnState != BtnState.Normal) && (_BtnState != BtnState.Inactive))
            {
                _BtnState = BtnState.Normal;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_BtnState != BtnState.Inactive)
            {
                base.OnMouseDown(e);

                _BtnState = BtnState.Pushed;
                _Timer.Enabled = LongClick != null;

                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_BtnState != BtnState.Inactive)
            {
                base.OnMouseUp(e);

                if (_BtnState == BtnState.Pushed)
                {
                    _BtnState = BtnState.Normal;
                    _Timer.Enabled = false;

                    Invalidate();
                    General.SafeLaunchEvent(Click, this);
                }
            }
            else if (Enabled)
            {
                // Esto sólo se produce cuando hemos puesto el estado a Inactive
                // directamente pero queremos seguir detectando clicks
                General.SafeLaunchEvent(Click, this);
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (_BtnState == BtnState.Pushed)
            {
                _Timer.Enabled = false;
                base.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
            }

            base.OnEnabledChanged(e);

            _BtnState = Enabled ? BtnState.Normal : BtnState.Inactive;
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if ((_BtnState != BtnState.Inactive) && (_BtnState != BtnState.Normal))
            {
                _BtnState = BtnState.Normal;
                Invalidate();
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            _BtnInfo.Font = Font;
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            _BtnInfo.SetForeColor(BtnState.Normal, ForeColor);
        }

        protected virtual void OnLongClick(object sender, EventArgs e)
        {
            if (_BtnState == BtnState.Pushed)
            {
                _BtnState = BtnState.Normal;
                _Timer.Enabled = false;

                Capture = false;
                Invalidate();

                General.SafeLaunchEvent(LongClick, this);
            }
        }
    }
}

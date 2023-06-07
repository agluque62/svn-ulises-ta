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
	public class HMIButtonConference : HMIButton
	{
		
		

    }
	public class HMIButton : Control
	{
		private int _Id = 0;
		private Timer _Timer = new Timer();
		private bool _DrawX = false;
		private BtnState _BtnState = BtnState.Normal;
		protected BtnInfo _BtnInfo = new BtnInfo();
		private int _LongClickMs = 2000;
        private Rectangle _TypeRect = new Rectangle(4, 4, 24, 24);
		//Errores #4805 Las funciones no permitidas no deberian  presentarse
		private bool _Permitted = true;
		private bool _IsConferencePreprogramada = false;
        public bool Permitted
        {
            get { return _Permitted; }
            set { _Permitted = value; }
        }
        public bool IsConferencePreprogramada
        {
            get { return _IsConferencePreprogramada; }
            set { _IsConferencePreprogramada = value; }
        }

        public new event EventHandler Click;
		public event EventHandler LongClick;

		[Browsable(false),
		DefaultValue(0)
		]
		public int Id
		{
			get { return _Id; }
			set { _Id = value; }
		}

		[Browsable(false),
		DefaultValue(true)
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

		public HMIButton()
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

        public void Reset(string text, bool drawX, Color buttonColor, Image img = null)
        {
            _DrawX = drawX;
            _BtnInfo.Text = text;
            _BtnInfo.SetBackColor(BtnState.Normal, buttonColor);
            SetImage(img);

            Invalidate();
        }

        public void SetImage(Image img)
        {
            _BtnInfo.ImageAlign = ContentAlignment.TopCenter;
            _BtnInfo.SetImage(BtnState.Normal, img);
        }

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			BtnRenderer.Draw(e.Graphics, _BtnInfo[_BtnState], _DrawX);

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

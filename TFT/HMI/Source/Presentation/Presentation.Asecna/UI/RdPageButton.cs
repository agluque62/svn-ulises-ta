using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using HMI.Model.Module.UI;
using Utilities;

namespace HMI.Presentation.Asecna.UI
{
	public class RdPageButton : Control
	{
		private int _Page = 0;

		private BtnState _UpBtnState = BtnState.Normal;
		private BtnState _DownBtnState = BtnState.Normal;

		private BtnInfo _BtnInfo = new BtnInfo();
		private BtnInfo _UpBtnInfo = new BtnInfo();
		private BtnInfo _DownBtnInfo = new BtnInfo();

		public event GenericEventHandler UpClick;
		public event GenericEventHandler DownClick;

		[CategoryAttribute("Appearance"),
		Description("Image of down button in normal state"),
		RefreshProperties(RefreshProperties.Repaint),
		DefaultValue(null)
		]
		public Image DownEnabledImage
		{
			get { return _DownBtnInfo.GetImage(BtnState.Normal); }
			set 
			{ 
				_DownBtnInfo.SetImage(BtnState.Normal, value);
				Invalidate(_DownBtnInfo.Rect);
			}
		}

		[CategoryAttribute("Appearance"),
		Description("Image of down button when disabled"),
		RefreshProperties(RefreshProperties.Repaint),
		DefaultValue(null)
		]
		public Image DownDisabledImage
		{
			get { return _DownBtnInfo.GetImage(BtnState.Inactive); }
			set 
			{ 
				_DownBtnInfo.SetImage(BtnState.Inactive, value);
				Invalidate(_DownBtnInfo.Rect);
			}
		}

		[CategoryAttribute("Appearance"),
		Description("Image of up button in normal state"),
		RefreshProperties(RefreshProperties.Repaint),
		DefaultValue(null)
		]
		public Image UpEnabledImage
		{
			get { return _UpBtnInfo.GetImage(BtnState.Normal); }
			set 
			{ 
				_UpBtnInfo.SetImage(BtnState.Normal, value);
				Invalidate(_UpBtnInfo.Rect);
			}
		}

		[CategoryAttribute("Appearance"),
		Description("Image of up button when disabled"),
		RefreshProperties(RefreshProperties.Repaint),
		DefaultValue(null)
		]
		public Image UpDisabledImage
		{
			get { return _UpBtnInfo.GetImage(BtnState.Inactive); }
			set 
			{ 
				_UpBtnInfo.SetImage(BtnState.Inactive, value);
				Invalidate(_UpBtnInfo.Rect);
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
		public int CornerRadius
		{
			get { return _BtnInfo.CornerRadius; }
			set
			{
				_BtnInfo.CornerRadius = _UpBtnInfo.CornerRadius = _DownBtnInfo.CornerRadius = value;
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

		[Browsable(false),
		DefaultValue(0)
		]
		public int Page
		{
			get { return _Page; }
			set 
			{
				if (_Page != value)
				{
					Debug.Assert(value >= 0);

					_Page = value;
					_BtnInfo.Text = (_Page + 1).ToString();
					Invalidate();
				}
			}
		}
		
		public RdPageButton()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.Selectable, true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			SetStyle(ControlStyles.UserPaint, true);
			BackColor = Color.Transparent;
 
			_BtnInfo.Text = (_Page + 1).ToString();
			_BtnInfo.Font = Font;

			_BtnInfo.SetForeColor(BtnState.Normal, ForeColor);
			_BtnInfo.SetInnerBorderColor(BtnState.Inactive, Color.Transparent);
			_BtnInfo.SetInnerBorderColor(BtnState.Normal, Color.Transparent);
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

			int width = (int)(Width / 3.0);

			_UpBtnInfo.Rect = new Rectangle(0, 0, width, Height);
			_DownBtnInfo.Rect = new Rectangle(Width - width, 0, width, Height);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (_UpBtnInfo.Rect.Contains(e.Location))
			{
				if ((_UpBtnState != BtnState.MouseOver) && (_UpBtnState != BtnState.Pushed))
				{
					_UpBtnState = BtnState.MouseOver;
					Invalidate(_UpBtnInfo.Rect);
				}
			}
			else if ((_UpBtnState == BtnState.MouseOver) || (_UpBtnState == BtnState.Pushed))
			{
				_UpBtnState = BtnState.Normal;
				Invalidate(_UpBtnInfo.Rect);
			}

			if (_DownBtnInfo.Rect.Contains(e.Location))
			{
				if ((_DownBtnState != BtnState.MouseOver) && (_DownBtnState != BtnState.Pushed))
				{
					_DownBtnState = BtnState.MouseOver;
					Invalidate(_DownBtnInfo.Rect);
				}
			}
			else if ((_DownBtnState == BtnState.MouseOver) || (_DownBtnState == BtnState.Pushed))
			{
				_DownBtnState = BtnState.Normal;
				Invalidate(_DownBtnInfo.Rect);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			Capture = false;

			if (_UpBtnState == BtnState.MouseOver)
			{
				_UpBtnState = BtnState.Pushed;
			}
			else if (_DownBtnState == BtnState.MouseOver)
			{
				_DownBtnState = BtnState.Pushed;
			}

			Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (_UpBtnState == BtnState.Pushed)
			{
				_UpBtnState = BtnState.MouseOver;
				Invalidate(_UpBtnInfo.Rect);

				General.SafeLaunchEvent(UpClick, this);
			}
			else if (_DownBtnState == BtnState.Pushed)
			{
				_DownBtnState = BtnState.MouseOver;
				Invalidate(_DownBtnInfo.Rect);

				General.SafeLaunchEvent(DownClick, this);
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			if (_UpBtnState != BtnState.Normal)
			{
				_UpBtnState = BtnState.Normal;
				Invalidate(_UpBtnInfo.Rect);
			}
			else if (_DownBtnState != BtnState.Normal)
			{
				_DownBtnState = BtnState.Normal;
				Invalidate(_DownBtnInfo.Rect);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
//			base.OnPaint(e);

			BtnState st = !Enabled || (_UpBtnState == BtnState.Inactive) ? BtnState.Inactive : BtnState.Normal;
			BtnState upSt = !Enabled ? BtnState.Inactive : _UpBtnState;
			BtnState downSt = !Enabled ? BtnState.Inactive : _DownBtnState;

			BtnRenderer.Draw(e.Graphics, _BtnInfo[st]);
			BtnRenderer.Draw(e.Graphics, _UpBtnInfo[upSt]);
			BtnRenderer.Draw(e.Graphics, _DownBtnInfo[downSt]);
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
	}
}

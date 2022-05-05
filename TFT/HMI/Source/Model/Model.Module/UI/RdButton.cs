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
using HMI.Model.Module.BusinessEntities;

namespace HMI.Model.Module.UI
{
	public class RdButton : Control
	{
		private const int _LongClickMs = 2000;

		private BtnState _State = BtnState.Normal;
		private BtnState _StateBT1 = BtnState.Normal;
		private BtnState _StateBT2 = BtnState.Normal;

		private int _Id = 0;
		private Rectangle _PttRect = new Rectangle();
		private Rectangle _SquelchRect = new Rectangle();
		private Font _SmallFont = new Font("Microsoft Sans Serif", 7.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        private Font _MediumFontBold = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        private Font _SmallFontBold1 = new Font("Microsoft Sans Serif", 7.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
        private Font _MediumFontBold2 = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        private Font _BigFont = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
		private Image _PttImage = null;
		private Image _SquelchImage = null;
		private string _Frecuency = "";
		private string _Alias = "";
		//RQF34 
		private string _IdFrecuency = "";
		private string _NameFrecuency = "";

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
		public int CornerRadius
		{
			get { return _BtnInfo.CornerRadius; }
			set
			{
				_BtnInfo.CornerRadius = _TxBtnInfo.CornerRadius = _RxBtnInfo.CornerRadius = value;
				Invalidate();
			}
		}
        public Color CurrentBackColor
        {
            get { return _CurrentBackColor; }
            set { _CurrentBackColor = value; }
        }

		public RdButton()
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

			_TxBtnInfo.Text = "Tx";
			_TxBtnInfo.Font = _BigFont;
			_RxBtnInfo.Text = "Rx";
			_RxBtnInfo.Font = _BigFont;
        }

		// RQF34 Esta funcion hay que dejar por compatibilidad con otros entornos.
        public void Reset(string frecuency, string alias, bool drawX, bool allAsOneBt, int rtxGroup, Image ptt, Image squelch, Image audio, Color title, Color tx, Color rx, Color txForeColor, Color rxForeColor, Color titleForeColor, 
            string qidxResource, uint qidxValue, FrequencyState state)
        {
            //_Alias = qidxResource;
            _QidxValue = (int)qidxValue;
            Reset(frecuency, alias, drawX, allAsOneBt, rtxGroup, ptt, squelch, audio, title, tx, rx, txForeColor, rxForeColor, titleForeColor, state);
        }

		// RQF34 nueva funcion con los pararametros idfrecuency en dos parametros
        public void Reset(string idfrecuency,string frecuency, string alias, bool drawX, bool allAsOneBt, int rtxGroup, Image ptt, Image squelch, Image audio, Color title, Color tx, Color rx, Color txForeColor, Color rxForeColor, Color titleForeColor, 
            string qidxResource, uint qidxValue, FrequencyState state)
        {
            //_Alias = qidxResource;
            _QidxValue = (int)qidxValue;
			// RQF34 este modulo puede tener conflictos con las distintas instalaciones.

            Reset(idfrecuency,frecuency, alias, drawX, allAsOneBt, rtxGroup, ptt, squelch, audio, title, tx, rx, txForeColor, rxForeColor, titleForeColor, state);
        }

		//LALM 210223  Errores #4756 prioridad.
		// Inserto un nuevo parametro prioridad.
		public void Reset(string frecuency, string alias, bool drawX, bool allAsOneBt, int rtxGroup, Image ptt, Image squelch, Image audio, Color title, Color tx, Color rx, Color txForeColor, 
            Color rxForeColor, Color titleForeColor, FrequencyState state = FrequencyState.Available, int prioridad=0, bool bloqueo=false)
		{
			_Frecuency = frecuency;
			_Alias = alias.Length > 11 ? (alias.Substring(0,8)+"...") : alias ;
			_RtxGroup = rtxGroup;
			_PttImage = ptt;
			_SquelchImage = squelch;

			

            ForeColor = titleForeColor;
//            BackColor = titleForeColor;
            if (drawX)
                _CurrentBackColor = VisualStyle.ButtonColor;
            else
            {
                if ((title == VisualStyle.Colors.Yellow) || (title == VisualStyle.Colors.Red))
                    _CurrentBackColor = title; //error cases and rtx formation with priority over other colors
                else if (state == FrequencyState.Degraded)
                    _CurrentBackColor = Color.OrangeRed;
                else if (state == FrequencyState.Available)
                {
					_CurrentBackColor = VisualStyle.ButtonColor;
					////LALM 210223  Errores #4756 prioridad.
					// Cambio el color de la tecla cuando la prioridad es 3=prio violet, cuando es 4=emergencia blue
					if (prioridad==3)
						_CurrentBackColor = VisualStyle.Colors.Violet;
					else if (prioridad == 4)
						_CurrentBackColor = VisualStyle.Colors.Blue;
				}
				else //FrequencyState.NotAvailable
                {
                     _CurrentBackColor = title;              
                }
				//LALM 210707 nuevo parametro bloqueo para pintar otro color.
				if (bloqueo)
					_CurrentBackColor = VisualStyle.Colors.Orange;
            }

            _BtnInfo.SetBackColor(BtnState.Normal, _CurrentBackColor);
            _TxBtnInfo.SetBackColor(BtnState.Normal, tx);
			_TxBtnInfo.SetForeColor(BtnState.Normal, txForeColor);
			_RxBtnInfo.SetBackColor(BtnState.Normal, rx);
			_RxBtnInfo.SetForeColor(BtnState.Normal, rxForeColor);
			_RxBtnInfo.SetImage(BtnState.Normal, audio);
			_RxBtnInfo.Text = audio == null ? "Rx" : "";

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

		//LALM 210223  Errores #4756 prioridad.
		// Inserto un nuevo parametro prioridad.
		// RQF34, Inserto nuevo parametro idfrecuency
		public void Reset(string idfrecuency,string namefrecuency, string alias, bool drawX, bool allAsOneBt, int rtxGroup, Image ptt, Image squelch, Image audio, Color title, Color tx, Color rx, Color txForeColor,
			Color rxForeColor, Color titleForeColor, FrequencyState state = FrequencyState.Available, int prioridad = 0, bool bloqueo = false)
		{
			_Frecuency = namefrecuency;//RQF34 desaparecerá esta linea se sustituye por las dos ultimas
			_Alias = alias.Length > 11 ? (alias.Substring(0, 8) + "...") : alias;
			_RtxGroup = rtxGroup;
			_PttImage = ptt;
			_SquelchImage = squelch;

			_IdFrecuency = idfrecuency;// RQF34
			_NameFrecuency = namefrecuency;// RQF34


			ForeColor = titleForeColor;
			//            BackColor = titleForeColor;
			if (drawX)
				_CurrentBackColor = VisualStyle.ButtonColor;
			else
			{
				if ((title == VisualStyle.Colors.Yellow) || (title == VisualStyle.Colors.Red))
					_CurrentBackColor = title; //error cases and rtx formation with priority over other colors
				else if (state == FrequencyState.Degraded)
					_CurrentBackColor = Color.OrangeRed;
				else if (state == FrequencyState.Available)
				{
					_CurrentBackColor = VisualStyle.ButtonColor;
					////LALM 210223  Errores #4756 prioridad.
					// Cambio el color de la tecla cuando la prioridad es 3=prio violet, cuando es 4=emergencia blue
					if (prioridad == 3)
						_CurrentBackColor = VisualStyle.Colors.Violet;
					else if (prioridad == 4)
						_CurrentBackColor = VisualStyle.Colors.Blue;
				}
				else //FrequencyState.NotAvailable
				{
					_CurrentBackColor = title;
				}
				//LALM 210707 nuevo parametro bloqueo para pintar otro color.
				if (bloqueo)
					_CurrentBackColor = VisualStyle.Colors.Orange;
			}

			_BtnInfo.SetBackColor(BtnState.Normal, _CurrentBackColor);
			_TxBtnInfo.SetBackColor(BtnState.Normal, tx);
			_TxBtnInfo.SetForeColor(BtnState.Normal, txForeColor);
			_RxBtnInfo.SetBackColor(BtnState.Normal, rx);
			_RxBtnInfo.SetForeColor(BtnState.Normal, rxForeColor);
			_RxBtnInfo.SetImage(BtnState.Normal, audio);
			_RxBtnInfo.Text = audio == null ? "Rx" : "";

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
#if DEBUG1
            _PttRect = new Rectangle(1, 10, 12, 12);
            _SquelchRect = new Rectangle(Width - (1+12), 10, 12, 12);
#else
            _PttRect = new Rectangle(1, 4, 25, 25);
            _SquelchRect = new Rectangle(Width - 22, 4, 25, 25);
#endif

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
				(_StateBT1 == BtnState.MouseOver) && !_AllAsOneBt? BtnState.MouseOver : BtnState.Normal;

			BtnState stBT2 = !Enabled ? BtnState.Inactive :
				(_StateBT2 == BtnState.Pushed) || (st == BtnState.Pushed) ? BtnState.Pushed :
				(_StateBT2 == BtnState.MouseOver) && !_AllAsOneBt ? BtnState.MouseOver : BtnState.Normal;

			BtnRenderer.Draw(e.Graphics, _BtnInfo[st]);

			if (_PttImage != null)
			{
#if DEBUG1
				e.Graphics.DrawImage(_PttImage, _PttRect);
#else
				e.Graphics.DrawImage(_PttImage, _PttRect.X, _PttRect.Y);
#endif
            }
			if (_SquelchImage != null)
			{
#if DEBUG1
                e.Graphics.DrawImage(_SquelchImage, _SquelchRect);
#else
				e.Graphics.DrawImage(_SquelchImage, _SquelchRect.X, _SquelchRect.Y);
#endif

                if (_QidxValue >= 0)
                {
                    Rectangle txtRect = ClientRectangle;
                    txtRect.Offset(32, 2);
                    BtnRenderer.DrawString(e.Graphics, txtRect, Color.Transparent, st, _QidxValue.ToString(), _MediumFontBold, ContentAlignment.TopCenter, Color.Black);
                }
			}

            BtnRenderer.Draw(e.Graphics, _TxBtnInfo[stBT1]);
            BtnRenderer.Draw(e.Graphics, _RxBtnInfo[stBT2]);
            
            if (_DrawX)
            {
                using (Pen p = new Pen(Color.Red, 5))
                {
                    e.Graphics.DrawLine(p, 6, 6, Width - 6, Height - 6);
                    e.Graphics.DrawLine(p, Width - 6, 6, 6, Height - 6);
                }
            }
            
            Rectangle textRect = ClientRectangle;
            /** 20180608. Las frecuencias con ID de mas de 7 Caracteres utilizan un FONT ligeramente inferior */
#if DEBUG1
            textRect.Offset(0, -5);
            //_Frecuency = _Frecuency=="125.200" ? "EMERGENCIA" : _Frecuency;
            Font fontToUse = _Frecuency.Length > 7 ? _SmallFontBold1 : _MediumFontBold;
            BtnRenderer.DrawString(e.Graphics, textRect, _BtnInfo.GetBackColor(st), st, _Frecuency, fontToUse, ContentAlignment.TopCenter, ForeColor);
#else
            textRect.Offset(0, -5);
			//Font fontToUse = _Frecuency.Length > 7 ? _SmallFontBold1 : _MediumFontBold;// RQF34 se cambia esta linea por la siguiente
			Font fontToUse = _NameFrecuency.Length > 7 ? _SmallFontBold1 : _MediumFontBold;
            if (global::HMI.Model.Module.Properties.Settings.Default.BigFonts)
                fontToUse = _BigFont;
			// RQF 35 cambio esta linea por la siguiente
			//BtnRenderer.DrawString(e.Graphics, textRect, _BtnInfo.GetBackColor(st), st, _Frecuency, fontToUse, ContentAlignment.TopCenter, ForeColor);
			BtnRenderer.DrawString(e.Graphics, textRect, _BtnInfo.GetBackColor(st), st, _NameFrecuency, fontToUse, ContentAlignment.TopCenter, ForeColor);
#endif
            if (global::HMI.Model.Module.Properties.Settings.Default.BigFonts)
            {
                fontToUse = _MediumFontBold2;
                textRect.Offset(0, 20);
            }
            else
            {
                textRect.Offset(0, 13);
                fontToUse = _SmallFont;
            }
            BtnRenderer.DrawString(e.Graphics, textRect, _BtnInfo.GetBackColor(st), st, _Alias, fontToUse, ContentAlignment.TopCenter, ForeColor);
            
            if (_RtxGroup > 0)
			{
				string rtxGroup = ((char)('G' + _RtxGroup - 1)).ToString();
				e.Graphics.DrawString(rtxGroup, _MediumFontBold, Brushes.Black, 3, _TxBtnInfo.Rect.Top - 15);
			}
			else if (_RtxGroup == -1)
			{
				e.Graphics.DrawString("R", _MediumFontBold, Brushes.Black, Width - 15, _TxBtnInfo.Rect.Top - 15);
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

        public void Reset(string frecuency, string alias, bool unavailable, bool allAsOneBt, int rtxGroup, Image ptt, Image squelch, Image audio, Color title, Color tx, Color rx, Color txForeColor, Color rxForeColor, Color titleForeColor, FrequencyState state, object priority)
        {
            throw new NotImplementedException();
        }
    }
}

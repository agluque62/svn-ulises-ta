using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using HMI.Model.Module.BusinessEntities;

namespace HMI.Model.Module.UI
{
	public class LcButton : HMIButton
	{
		private Rectangle _TxRect = new Rectangle();
		private Rectangle _RxRect = new Rectangle();
		private Color _TxBackColor = VisualStyle.ButtonColor;
		private Color _RxBackColor = VisualStyle.ButtonColor;
		private bool _DrawX = false;

		[Browsable(false),
		DefaultValue(false)
		]
		public new bool DrawX
		{

			get { return _DrawX; }
			set 
			{ 
				_DrawX = value;
				Invalidate();
			}
		}

		[Browsable(false),
		DefaultValue(typeof(Color), "Gainsboro")
		]
		public Color TxBackColor
		{
			get { return _TxBackColor; }
			set
			{
				_TxBackColor = value;
				Invalidate(_TxRect);
			}
		}

		public LcButton()
		{
			TextAlign = ContentAlignment.TopCenter;
			if (VisualStyle.ModoNocturno)
			{
                VisualStyle.ButtonColor = VisualStyle.ButtonColorN;
                //_TxBackColor = VisualStyle.ButtonColor;
                //_RxBackColor = VisualStyle.ButtonColor;
                VisualStyle.TextoTfColor = VisualStyle.TextoTfColorN;
            }
		}

		//LALM 210618
		// Funcion Que limita el numero maximo de caracteres de una agrupacion a 16.
		private String GenIdAgrupacion(String agrupacion)
		{
			int longmax = 16;
			String IdAgrupacion = agrupacion;
			int len = agrupacion.Length;
			if (len > longmax)
			{
				int mitad = longmax / 2;
				IdAgrupacion = agrupacion.Substring(0, mitad - 1) + ".." + agrupacion.Substring(len - (mitad - 1), mitad - 1);
			}
			return IdAgrupacion;
		}

		public void Reset(string dst, bool drawX, Color rx, Color tx)
		{
			_TxBackColor = tx;
			_RxBackColor = rx;
			_DrawX = drawX;
			//base.Text = dst.Length > 20 ? dst.Substring(0, 17) + "..." : dst;
			base.Text = GenIdAgrupacion(dst);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			int top = (int)(Height * 0.5);

			_TxRect = new Rectangle(2, 2, Width - 5, top - 4);
			_RxRect = new Rectangle(2, top + 2, Width - 5, Height - top - 5);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (Enabled)
			{
				if (_TxBackColor != ButtonColor)
				{
					using (GraphicsPath path = BtnRenderer.GetRoundedRect(_TxRect, _BtnInfo.CornerRadius))
					using (Brush brush = new SolidBrush(Color.FromArgb(VisualStyle.UseModernStyle ? 175 : 255, _TxBackColor)))
					{
						e.Graphics.FillPath(brush, path);
					}

                    //BtnRenderer.DrawString(e.Graphics, ClientRectangle, _TxBackColor, BtnState, Text, Font, ContentAlignment.TopCenter, ForeColor);
				}
				if (_RxBackColor != ButtonColor)
				{
					using (GraphicsPath path = BtnRenderer.GetRoundedRect(_RxRect, _BtnInfo.CornerRadius))
					using (Brush brush = new SolidBrush(Color.FromArgb(VisualStyle.UseModernStyle ? 175 : 255, _RxBackColor)))
					{
						e.Graphics.FillPath(brush, path);
					}
				}
			}

			using (Pen linePen = new Pen(Enabled ? _BtnInfo.GetBorderColor(BtnState.Normal) : _BtnInfo.GetBorderColor(BtnState.Inactive), 2))
			{
				e.Graphics.DrawLine(linePen, 1, _RxRect.Top - 2, Width - 3, _RxRect.Top - 2);
			}

			if (_DrawX)
			{
				// LALM: 210129 Aspa configurable.
				if (!VisualStyle.ModoNocturno)
				{
					using (Pen p = new Pen(Color.Red, 5))
					{
						e.Graphics.DrawLine(p, 6, 6, Width - 6, Height - 6);
						e.Graphics.DrawLine(p, Width - 6, 6, 6, Height - 6);
					}
                    //BtnRenderer.DrawString(e.Graphics, ClientRectangle, _TxBackColor, BtnState, Text, Font, ContentAlignment.TopCenter, ForeColor);
                }
                else
                {
                    using (Pen p = new Pen(VisualStyle.AspaColor, 5))
                    {
                        e.Graphics.DrawLine(p, 6, 6, Width - 6, Height - 6);
                        e.Graphics.DrawLine(p, Width - 6, 6, 6, Height - 6);
                    }
                    //BtnRenderer.DrawString(e.Graphics, ClientRectangle, _TxBackColor, BtnState, Text, Font, ContentAlignment.TopCenter, VisualStyle.TextoTfColor);
                }
            }

			if (!VisualStyle.ModoNocturno)
				BtnRenderer.DrawString(e.Graphics, ClientRectangle, _TxBackColor, BtnState, Text, Font, ContentAlignment.TopCenter, ForeColor);
			else
			{
				BtnRenderer.DrawString(e.Graphics, ClientRectangle, _TxBackColor, BtnState, Text, Font, ContentAlignment.TopCenter, VisualStyle.TextoTfColor);
			}
		}
	}
}

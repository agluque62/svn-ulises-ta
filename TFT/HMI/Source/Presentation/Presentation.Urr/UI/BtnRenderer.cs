using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace HMI.Presentation.Urr.UI
{
	public enum BtnState
	{
		Inactive,
		Normal,
		MouseOver,
		Pushed,
		MaxBtnStates
	}

	public class BtnStateInfo
	{
		public Rectangle Rect;
		public BtnState State;
		public BtnStyle Style;
		public int CornerRadius;
		public Color BorderColor;
		public Color InnerBorderColor;
		public Color BackColor;
		public ColorBlend Blend;
		public string Text;
		public Font Font;
		public StringFormat TextFormat;
		public Color ForeColor;
		public Rectangle TextRect;
		public Image Image;
		public ContentAlignment ImageAlign;
		public GraphicsPath ImgPath;
		public Rectangle ImgRect;

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

		public BtnStateInfo(Rectangle rect, BtnState state, BtnStyle style,
			int cornerRadius, Color borderColor, Color innerBorderColor, Color backColor, ColorBlend blend,
			string text, Font font, StringFormat textFormat, Color foreColor, Rectangle textRect,
			Image image, ContentAlignment imageAlign, GraphicsPath imgPath, Rectangle imgRect)
		{
			Rect = rect;
			State = state;
			Style = style;
			CornerRadius = cornerRadius;
			BorderColor = borderColor;
			InnerBorderColor = innerBorderColor;
			BackColor = backColor;
			Blend = blend;
			//Text = text.Length > 20 ? text.Substring(0, 17) + "..." : text;
			Text = GenIdAgrupacion(text);
			Font = font;
			TextFormat = textFormat;
			ForeColor = foreColor;
			TextRect = textRect;
			Image = image;
			ImageAlign = imageAlign;
			ImgPath = imgPath;
			ImgRect = imgRect;
		}
	}

	public class BtnInfo
	{
		private Rectangle _Rect;
		private BtnStyle _Style = VisualStyle.UseModernStyle ? BtnStyle.ModernStyle : BtnStyle.Fixed3D;
		private int _CornerRadius = VisualStyle.ButtonCornerRadius;
		private Color?[] _BorderColors = { null, null, null, null };
		private Color?[] _InnerBorderColors = { null, null, null, null };
		private Color?[] _BackColors = { null, null, null, null };
		private ColorBlend[] _Blends = { new ColorBlend(), new ColorBlend(), new ColorBlend(), new ColorBlend() };
		private string _Text = "";
		private Font _Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
		private ContentAlignment _TextAlign = ContentAlignment.MiddleCenter;
		private Color?[] _ForeColors = { null, null, null, null };
		private Rectangle[] _TextRects = new Rectangle[(int)BtnState.MaxBtnStates];
		private StringFormat _TextFormat = BtnRenderer.StringFormatAlignment(ContentAlignment.MiddleCenter);
		private Image[] _Images = { null, null, null, null };
		private Image _NormalGrayImg = null;
		private Image _PushedImg = null;
		private GraphicsPath _PushedImgPath = null;
		private Rectangle _PushedImgRect;
		private ContentAlignment _ImageAlign = ContentAlignment.MiddleCenter;
		private GraphicsPath[] _ImgPaths = { null, null, null, null };
		private Rectangle[] _ImgRects = new Rectangle[(int)BtnState.MaxBtnStates];

		public Rectangle Rect
		{
			get { return _Rect; }
			set 
			{ 
				if (_Rect != value)
				{
					_Rect = value;

					CalculateTextRects();
					CalculateImageRects();
				}
			}
		}

		public BtnStyle Style
		{
			get { return _Style; }
			set 
			{ 
				if (_Style != value)
				{
					_Style = value;
					CalculateBlends();
				}
			}
		}

		public int CornerRadius
		{
			get { return (_Style != BtnStyle.Fixed3D ? _CornerRadius : 0); }
			set { _CornerRadius = value; }
		}

		public string Text
		{
			get { return _Text; }
			set { _Text = value; }
		}

		public Font Font
		{
			get { return _Font; }
			set { _Font = value; }
		}

		public ContentAlignment TextAlign
		{
			get { return _TextAlign; }
			set 
			{ 
				if (_TextAlign != value)
				{
					_TextAlign = value;

					_TextFormat.Dispose();
					_TextFormat = BtnRenderer.StringFormatAlignment(_TextAlign);
				}
			}
		}

		public ContentAlignment ImageAlign
		{
			get { return _ImageAlign; }
			set 
			{ 
				if (_ImageAlign != value)
				{
					_ImageAlign = value;
					CalculateImageRects();
				}
			}
		}

		public BtnStateInfo this[BtnState st]
		{
			get
			{
				Color borderColor = GetBorderColor(st);
				Color innerBorderColor = GetInnerBorderColor(st);
				Color backColor = GetBackColor(st);
				Color foreColor = GetForeColor(st);
				ColorBlend blend = GetBlend(st);

				Image img = _Images[(int)st];
				GraphicsPath imgPath = _ImgPaths[(int)st];
				Rectangle imgRect = _ImgRects[(int)st];

				if (img == null)
				{
					switch (st)
					{
						case BtnState.Inactive:
							img = _NormalGrayImg;
							imgPath = _ImgPaths[(int)BtnState.Normal];
							imgRect = _ImgRects[(int)BtnState.Normal];
							break;
						case BtnState.MouseOver:
							img = _Images[(int)BtnState.Normal];
							imgPath = _ImgPaths[(int)BtnState.Normal];
							imgRect = _ImgRects[(int)BtnState.Normal];
							break;
						case BtnState.Pushed:
							img = _PushedImg;
							imgPath = _PushedImgPath;
							imgRect = _PushedImgRect;
							break;
					}
				}

				return new BtnStateInfo(_Rect, st, _Style,
					CornerRadius, borderColor, innerBorderColor, backColor, blend,
					_Text, _Font, _TextFormat, foreColor, _TextRects[(int)st],
					img, _ImageAlign, imgPath, imgRect);
			}
		}

		public BtnInfo()
		{
			CalculateBlends();
		}

		public void SetBorderColor(BtnState st, Color color)
		{
			_BorderColors[(int)st] = color;
		}

		public Color GetBorderColor(BtnState st)
		{
			Color borderColor;

			switch (st)
			{
				case BtnState.Inactive:
					borderColor = _BorderColors[(int)st] ?? VisualStyle.BorderColorDisabled;
					break;
				case BtnState.MouseOver:
				case BtnState.Pushed:
					borderColor = _BorderColors[(int)st] ?? GetBorderColor(BtnState.Normal);
					break;
				default:
					borderColor = _BorderColors[(int)st] ?? VisualStyle.BorderColor;
					break;
			}

			return borderColor;
		}

		public void SetInnerBorderColor(BtnState st, Color color)
		{
			_InnerBorderColors[(int)st] = color;
		}

		public Color GetInnerBorderColor(BtnState st)
		{
			Color innerBorderColor;

			switch (st)
			{
				case BtnState.Inactive:
					innerBorderColor = _InnerBorderColors[(int)st] ?? VisualStyle.InnerBorderColorDisabled;
					break;
				case BtnState.MouseOver:
					innerBorderColor = _InnerBorderColors[(int)st] ?? VisualStyle.InnerBorderColorMouseOver;
					break;
				case BtnState.Pushed:
					innerBorderColor = _InnerBorderColors[(int)st] ?? VisualStyle.InnerBorderColorPushed;
					break;
				default:
					innerBorderColor = _InnerBorderColors[(int)st] ?? VisualStyle.InnerBorderColorNormal;
					break;
			}

			return innerBorderColor;
		}

		public void SetBackColor(BtnState st, Color color)
		{
			_BackColors[(int)st] = color;

			if (_Style != BtnStyle.Fixed3D)
			{
				CalculateBlend(st);
			}
		}

		public Color GetBackColor(BtnState st)
		{
			Color backColor;

			switch (st)
			{
				case BtnState.Inactive:
					if (_BackColors[(int)st] != null)
					{
						backColor = _BackColors[(int)st].Value;
					}
					else if ((_BackColors[(int)BtnState.Normal] != null) && (_BackColors[(int)BtnState.Normal] != VisualStyle.ButtonColor))
					{
						backColor = _BackColors[(int)BtnState.Normal].Value;
					}
					else
					{
						backColor = VisualStyle.ButtonColorDisabled;
					}
					//backColor = _BackColors[(int)st] ?? VisualStyle.ButtonColorDisabled;
					break;
				case BtnState.MouseOver:
				case BtnState.Pushed:
					backColor = _BackColors[(int)st] ?? GetBackColor(BtnState.Normal);
					break;
				default:
					backColor = _BackColors[(int)st] ?? VisualStyle.ButtonColor;
					break;
			}

			return backColor;
		}

		public void SetForeColor(BtnState st, Color color)
		{
			_ForeColors[(int)st] = color;
		}

		public Color GetForeColor(BtnState st)
		{
			Color foreColor;

			switch (st)
			{
				case BtnState.Inactive:
					foreColor = _ForeColors[(int)st] ?? ControlPaint.Dark(GetBackColor(st));
					break;
				case BtnState.MouseOver:
				case BtnState.Pushed:
					foreColor = _ForeColors[(int)st] ?? GetForeColor(BtnState.Normal);
					break;
				default:
					foreColor = _ForeColors[(int)st] ?? Color.Black;
					break;
			}

			return foreColor;
		}

		public void SetImage(BtnState st, Image img)
		{
			Image oldImage = _Images[(int)st];
			_Images[(int)st] = img;

			GraphicsPath oldPath = _ImgPaths[(int)st];
			if (oldPath != null)
			{
				oldPath.Dispose();
				_ImgPaths[(int)st] = null;
			}

			if (img != null)
			{
				Rectangle rr = ImageRectangleAlignment(_Rect, img, _ImageAlign);
				if (st == BtnState.Pushed)
				{
					rr.Offset(2, 2);
				}

				_ImgRects[(int)st] = rr;
				_ImgPaths[(int)st] = CalculateBitmapGraphicsPath(rr.Location, new Bitmap(img));

				if (st == BtnState.Normal)
				{
					_NormalGrayImg = ConvertToGrayScale(new Bitmap(img));

					rr.Offset(2, 2);

					_PushedImg = img;
					_PushedImgRect = rr;
					_PushedImgPath = CalculateBitmapGraphicsPath(rr.Location, new Bitmap(img));
				}
			}
			else if (st == BtnState.Normal)
			{
				if (_NormalGrayImg != null)
				{
					_NormalGrayImg.Dispose();
					_NormalGrayImg = null;
				}
				if (_PushedImg != null)
				{
					_PushedImgPath.Dispose();
					_PushedImg = null;
					_PushedImgPath = null;
				}
			}
		}

		public Image GetImage(BtnState st)
		{
			return _Images[(int)st];
		}

		private ColorBlend GetBlend(BtnState st)
		{
			ColorBlend blend;

			switch (st)
			{
				case BtnState.Inactive:
					if ((_BackColors[(int)st] != null) || (_BackColors[(int)BtnState.Normal] == null) ||
						(_BackColors[(int)BtnState.Normal] == VisualStyle.ButtonColor))
					{
						blend = _Blends[(int)st];
					}
					else
					{
						blend = _Blends[(int)BtnState.Normal];
					}
					break;
				case BtnState.MouseOver:
				case BtnState.Pushed:
					blend = _BackColors[(int)st] != null ? _Blends[(int)st] : _Blends[(int)BtnState.Normal];
					break;
				default:
					blend = _Blends[(int)st];
					break;
			}

			return blend;
		}

		private void CalculateTextRects()
		{
			Rectangle rr = new Rectangle(_Rect.X + 3, _Rect.Y + 6, _Rect.Width - 7, _Rect.Height - 13);

			_TextRects[(int)BtnState.Inactive] = rr;
			_TextRects[(int)BtnState.Normal] = rr;
			_TextRects[(int)BtnState.MouseOver] = rr;

			rr.Offset(2, 2);
			_TextRects[(int)BtnState.Pushed] = rr;
		}

		private void CalculateBlends()
		{
			if (_Style == BtnStyle.ModernStyle)
			{
				CalculateBlend(BtnState.Inactive);
				CalculateBlend(BtnState.Normal);
				CalculateBlend(BtnState.MouseOver);
				CalculateBlend(BtnState.Pushed);
			}
		}

		private void CalculateImageRects()
		{
			for (int i = 0; i < (int)BtnState.MaxBtnStates; i++)
			{
				SetImage((BtnState)i, _Images[i]);
			}
		}

		private void CalculateBlend(BtnState st)
		{
			Color bColor = GetBackColor(st);

			if (st == BtnState.Pushed)
			{
				_Blends[(int)st].Colors = new Color[] {
							Blend(bColor, Color.White, 80),
							Blend(bColor, Color.White, 40),
							Blend(bColor, Color.Black, 0),
							Blend(bColor, Color.Black, 0),
							Blend(bColor, Color.White, 40),
							Blend(bColor, Color.White, 80),
						};
				_Blends[(int)st].Positions = new float[] { 0.0f, .05f, .40f, .60f, .95f, 1.0f };
			}
			else
			{
				_Blends[(int)st].Colors = new Color[] {
								Blend(bColor, Color.White, 70),
								Blend(bColor, Color.White, 50),
								Blend(bColor, Color.White, 30),
								Blend(bColor, Color.White, 00),
								Blend(bColor, Color.Gray, 20),
								Blend(bColor, Color.Gray, 40),
						};
				_Blends[(int)st].Positions = new float[] { 0.0f, .15f, .40f, .65f, .80f, 1.0f };
			}
		}

		private static Color Blend(Color srcColor, Color dstColor, int percentage)
		{
			int r = srcColor.R + ((dstColor.R - srcColor.R) * percentage) / 100;
			int g = srcColor.G + ((dstColor.G - srcColor.G) * percentage) / 100;
			int b = srcColor.B + ((dstColor.B - srcColor.B) * percentage) / 100;

			return Color.FromArgb(r, g, b);
		}

		private static Rectangle ImageRectangleAlignment(Rectangle rect, Image image, ContentAlignment imageAlign)
		{
			Rectangle r = new Rectangle(8, 8, image.Width, image.Height);

			switch (imageAlign)
			{
				case ContentAlignment.TopCenter:
					r = new Rectangle(rect.Width / 2 - image.Width / 2, 8, image.Width, image.Height);
					break;
				case ContentAlignment.TopRight:
					r = new Rectangle(rect.Width - 8 - image.Width, 8, image.Width, image.Height);
					break;
				case ContentAlignment.MiddleLeft:
					r = new Rectangle(8, rect.Height / 2 - image.Height / 2, image.Width, image.Height);
					break;
				case ContentAlignment.MiddleCenter:
					r = new Rectangle(rect.Width / 2 - image.Width / 2, rect.Height / 2 - image.Height / 2, image.Width, image.Height);
					break;
				case ContentAlignment.MiddleRight:
					r = new Rectangle(rect.Width - 8 - image.Width, rect.Height / 2 - image.Height / 2, image.Width, image.Height);
					break;
				case ContentAlignment.BottomLeft:
					r = new Rectangle(8, rect.Height - 8 - image.Height, image.Width, image.Height);
					break;
				case ContentAlignment.BottomCenter:
					r = new Rectangle(rect.Width / 2 - image.Width / 2, rect.Height - 8 - image.Height, image.Width, image.Height);
					break;
				case ContentAlignment.BottomRight:
					r = new Rectangle(rect.Width - 8 - image.Width, rect.Height - 8 - image.Height, image.Width, image.Height);
					break;
			}

			r.Offset(rect.X, rect.Y);
			return r;
		}

		private static GraphicsPath CalculateBitmapGraphicsPath(Point location, Bitmap bitmap)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			Color colorTransparent = bitmap.GetPixel(0, 0);

			for (int row = 0; row < bitmap.Height; row++)
			{
				int colOpaquePixel = 0;

				for (int col = 0; col < bitmap.Width; col++)
				{
					// If this is an opaque pixel, mark it and search for anymore trailing behind
					if (bitmap.GetPixel(col, row) != colorTransparent)
					{
						colOpaquePixel = col++;

						while ((col < bitmap.Width) && (bitmap.GetPixel(col, row) != colorTransparent))
						{
							col++;
						}

						graphicsPath.AddRectangle(new Rectangle(location.X + colOpaquePixel, location.Y + row, col - colOpaquePixel, 1));
					}
				}
			}

			return graphicsPath;
		}

		private static Bitmap ConvertToGrayScale(Bitmap source)
		{
			Bitmap bm = new Bitmap(source.Width, source.Height);
			Color transparent = source.GetPixel(0, 0);

			for (int y = 0; y < bm.Height; y++)
			{
				for (int x = 0; x < bm.Width; x++)
				{
					Color c = source.GetPixel(x, y);

					if (c != transparent)
					{
						int luma = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);
						bm.SetPixel(x, y, Color.FromArgb(luma, luma, luma));
					}
				}
			}

			return bm;
		}
	}

	public static class BtnRenderer
	{
        public static void Draw(Graphics g, BtnStateInfo info, bool crossed)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            DrawBackground(g, info.Rect, info.Style, info.State, info.CornerRadius,
                info.BorderColor, info.InnerBorderColor, info.BackColor, info.Blend);

            // Dibuja un aspa
            if (crossed)
            {
                using (Pen p = new Pen(Color.Red, 5))
                {
                    g.DrawLine(p, 6, 6, info.Rect.Width - 6, info.Rect.Height - 6);
                    g.DrawLine(p, info.Rect.Width - 6, 6, 6, info.Rect.Height - 6);
                }
            }

            if (info.Image != null)
            {
                g.SetClip(info.ImgPath);
                g.DrawImage(info.Image, info.ImgRect);
                g.ResetClip();
            }

            if (!string.IsNullOrEmpty(info.Text))
            {
                if (info.State == BtnState.Inactive)
                {
                    Rectangle rr = new Rectangle(info.TextRect.X + 1, info.TextRect.Y + 1, info.TextRect.Width, info.TextRect.Height);
                    g.DrawString(info.Text, info.Font, Brushes.White, rr, info.TextFormat);
                }

                using (SolidBrush brush = new SolidBrush(info.ForeColor))
                {
                    g.DrawString(info.Text, info.Font, brush, info.TextRect, info.TextFormat);
                }
            }
        }

		public static void Draw(Graphics g, BtnStateInfo info)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;

			DrawBackground(g, info.Rect, info.Style, info.State, info.CornerRadius,
				info.BorderColor, info.InnerBorderColor, info.BackColor, info.Blend);

			if (info.Image != null)
			{
				g.SetClip(info.ImgPath);
				g.DrawImage(info.Image, info.ImgRect);
				g.ResetClip();
			}

			if (!string.IsNullOrEmpty(info.Text))
			{
				if (info.State == BtnState.Inactive)
				{
					Rectangle rr = new Rectangle(info.TextRect.X + 1, info.TextRect.Y + 1, info.TextRect.Width, info.TextRect.Height);
					g.DrawString(info.Text, info.Font, Brushes.White, rr, info.TextFormat);
				}

				using (SolidBrush brush = new SolidBrush(info.ForeColor))
				{
					g.DrawString(info.Text, info.Font, brush, info.TextRect, info.TextFormat);
				}
			}
		}

		public static void DrawString(Graphics g, Rectangle rect, Color backColor, BtnState btnState, 
			string text, Font font, ContentAlignment textAlign, Color foreColor)
		{
			if (!string.IsNullOrEmpty(text))
			{
				using (StringFormat sf = StringFormatAlignment(textAlign))
				{
					Rectangle rr = new Rectangle(rect.X + 3, rect.Y + 6, rect.Width - 7, rect.Height - 13);

					if (btnState == BtnState.Inactive)
					{
						g.DrawString(text, font, Brushes.White, new Rectangle(rr.X + 1, rr.Y + 1, rr.Width, rr.Height), sf);
						using (Brush brush = new SolidBrush(ControlPaint.Dark(backColor)))
						{
							g.DrawString(text, font, brush, rr, sf);
						}
					}
					else
					{
						if (btnState == BtnState.Pushed)
						{
							rr.Offset(2, 2);
						}

						using (Brush brush = new SolidBrush(foreColor))
						{
							g.DrawString(text, font, brush, rr, sf);
						}
					}
				}
			}
		}

		public static GraphicsPath GetRoundedRect(Rectangle baseRect, int radius)
		{
			//float x = baseRect.X, y = baseRect.Y, w = baseRect.Width, h = baseRect.Height;
			//GraphicsPath rr = new GraphicsPath();

			//rr.AddBezier(x, y + radius, x, y, x + radius, y, x + radius, y);
			//rr.AddLine(x + radius, y, x + w - radius, y);
			//rr.AddBezier(x + w - radius, y, x + w, y, x + w, y + radius, x + w, y + radius);
			//rr.AddLine(x + w, y + radius, x + w, y + h - radius);
			//rr.AddBezier(x + w, y + h - radius, x + w, y + h, x + w - radius, y + h, x + w - radius, y + h);
			//rr.AddLine(x + w - radius, y + h, x + radius, y + h);
			//rr.AddBezier(x + radius, y + h, x, y + h, x, y + h - radius, x, y + h - radius);
			//rr.AddLine(x, y + h - radius, x, y + radius);

			//return rr;
			GraphicsPath path = new GraphicsPath();

			if (radius <= 0)
			{
				path.AddRectangle(baseRect);
			}
			else if (radius >= (Math.Min(baseRect.Width, baseRect.Height)) / 2)
			{
				if (baseRect.Width > baseRect.Height)
				{
					// return horizontal capsule 
					int diameter = baseRect.Height;
					Rectangle arc = new Rectangle(baseRect.Location, new Size(diameter, diameter));

					path.AddArc(arc, 90, 180);
					arc.X = baseRect.Right - diameter;
					path.AddArc(arc, 270, 180);
				}
				else if (baseRect.Width < baseRect.Height)
				{
					// return vertical capsule 
					int diameter = baseRect.Width;
					Rectangle arc = new Rectangle(baseRect.Location, new Size(diameter, diameter));

					path.AddArc(arc, 180, 180);
					arc.Y = baseRect.Bottom - diameter;
					path.AddArc(arc, 0, 180);
				}
				else
				{
					// return circle 
					path.AddEllipse(baseRect);
				}
			}
			else
			{
				int diameter = radius * 2;
				Rectangle arc = new Rectangle(baseRect.Location, new Size(diameter, diameter));

				// top left arc 
				path.AddArc(arc, 180, 90);

				// top right arc 
				arc.X = baseRect.Right - diameter;
				path.AddArc(arc, 270, 90);

				// bottom right arc 
				arc.Y = baseRect.Bottom - diameter;
				path.AddArc(arc, 0, 90);

				// bottom left arc
				arc.X = baseRect.Left;
				path.AddArc(arc, 90, 90);
			}

			path.CloseFigure();
			return path;
		}

		public static StringFormat StringFormatAlignment(ContentAlignment textAlign)
		{
			StringFormat sf = new StringFormat();

			switch (textAlign)
			{
				case ContentAlignment.TopLeft:
				case ContentAlignment.TopCenter:
				case ContentAlignment.TopRight:
					sf.LineAlignment = StringAlignment.Near;
					break;
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleRight:
					sf.LineAlignment = StringAlignment.Center;
					break;
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight:
					sf.LineAlignment = StringAlignment.Far;
					break;
			}
			switch (textAlign)
			{
				case ContentAlignment.TopLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.BottomLeft:
					sf.Alignment = StringAlignment.Near;
					break;
				case ContentAlignment.TopCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.BottomCenter:
					sf.Alignment = StringAlignment.Center;
					break;
				case ContentAlignment.TopRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.BottomRight:
					sf.Alignment = StringAlignment.Far;
					break;
			}

			return sf;
		}

		private static void DrawBackground(Graphics g, Rectangle rect, BtnStyle style, BtnState state,
			int cornerRadius, Color borderColor, Color innerBorderColor, Color backColor, ColorBlend blend)
		{
			Rectangle rr = new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

			if (style == BtnStyle.Flat)
			{
				using (GraphicsPath path = GetRoundedRect(rr, cornerRadius))
				{
					using (SolidBrush brush = new SolidBrush(backColor))
					{
						g.FillPath(brush, path);
					}

					using (Pen p = new Pen(borderColor, 1))
					{
						g.DrawPath(p, path);
					}
				}
			}
			else if (style == BtnStyle.Fixed3D)
			{
				using (SolidBrush brush = new SolidBrush(backColor))
				{
					g.FillRectangle(brush, rr);
				}

				using (Pen blackpen = new Pen(Color.Black, 2))
				using (Pen whitepen = new Pen(Color.White, 2))
				{
					g.DrawLine(state == BtnState.Pushed ? blackpen : whitepen, rr.Left, rr.Top, rr.Right - 1, rr.Top);
					g.DrawLine(state == BtnState.Pushed ? blackpen : whitepen, rr.Left, rr.Top, rr.Left, rr.Bottom - 1);
					g.DrawLine(state == BtnState.Pushed ? whitepen : blackpen, rr.Right, rr.Top, rr.Right, rr.Bottom);
					g.DrawLine(state == BtnState.Pushed ? whitepen : blackpen, rr.Left, rr.Bottom, rr.Right - 1, rr.Bottom);
				}
			}
			else
			{
				using (GraphicsPath path = GetRoundedRect(rr, cornerRadius))
				{
					using (LinearGradientBrush brush = new LinearGradientBrush(rr, backColor, backColor, LinearGradientMode.Vertical))
					{
						brush.InterpolationColors = blend;
						g.FillPath(brush, path);
					}

					rr = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);

					if (innerBorderColor != Color.Transparent)
					{
						using (GraphicsPath innerPath = GetRoundedRect(rr, cornerRadius))
						using (Pen p = new Pen(innerBorderColor, 2))
						{
							g.DrawPath(p, innerPath);
						}
					}

					using (Pen p = new Pen(borderColor, 1))
					{
						g.DrawPath(p, path);
					}
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace HMI.Model.Module.UI
{
	public enum BtnStyle
	{
		Flat,
		Fixed3D,
		ModernStyle
	}

	public static class VisualStyle
	{
		public static class Colors
		{
            public static Color HfColor = Color.FromArgb(55, 145, 108);
            public static Color FDColor = Color.FromArgb(90, 90, 90);
            public static Color Blue = Color.FromArgb(0, 204, 255);			//Tecla Azul(0,204,255)
			public static Color HeaderBlue = Color.FromArgb(51, 102, 255);	//Fondo Área Información Azul Aena(51,102,255)
			public static Color LightBlue = Color.FromArgb(179, 217, 255);
			public static Color Orange = Color.FromArgb(255, 153, 0);		//Tecla Naranja(255,153,0)
			public static Color Green = Color.FromArgb(0, 204, 153);		//Tecla Verde(0,204,153)
			public static Color LightGreen = Color.FromArgb(213, 255, 213);
			public static Color Red = Color.FromArgb(255, 0, 0);			//Tecla Rojo(255,0,0)
			public static Color Gray = Color.FromArgb(192, 192, 192);		//Tecla Gris(192,192,192)
			public static Color LightGray = Color.FromArgb(221, 221, 221);
			public static Color DarkGray = Color.FromArgb(114, 114, 114);	//Tecla Gris Oscuro(114,114,114)
			public static Color Yellow = Color.FromArgb(255, 255, 0);		//Tecla Amarillo(255,255,0)
			public static Color White = Color.FromArgb(255, 255, 255);		//Tecla Blanco(255,255,255)
			public static Color Violet = Color.FromArgb(204, 153, 255);		//Tecla Violeta(204,153,255)
			public static Color Brown = Color.FromArgb(212, 211, 160);
            public static Color Black = Color.Black;

			// ANEXO I: COLORES A UTILIZAR EN EL HMI
			// Fondo Gris(221,221,221)
			// Fondo Negro(0,0,0)
			// Fondo Azul(179,217,255)
			// Fondo Verde(213,255,213)
			// Fondo Marrón(212,211,160)
		}

		public static Color UnknownScv = Color.PaleTurquoise;
		public static Color ScvA = Color.AliceBlue;
		public static Color ScvB = Color.Beige;
		public static Color InnerBorderColorDisabled = Color.WhiteSmoke;
		public static Color InnerBorderColorNormal = Color.LightGray;
		public static Color InnerBorderColorMouseOver = Color.LightGray;
		public static Color InnerBorderColorPushed = Color.Gray;
		public static Color ButtonColor = Color.Gainsboro;
		public static Color ButtonColorDisabled = SystemColors.Control;
		public static Color BorderColor = Color.Black;
		public static Color BorderColorDisabled = Color.Gainsboro;
		public static int ButtonCornerRadius = 4;
		public static bool UseModernStyle = true;
	}
}

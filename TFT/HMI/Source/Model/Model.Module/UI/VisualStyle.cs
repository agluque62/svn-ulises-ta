using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
//using HMI.Presentation.Twr.Views;
using System.IO;

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
        public static bool GetModoNocturno()
        {
            var filename = "hmi.exe.config";
            var valor = "True";
            var clave = "ModoNocturno";
            var find = $"      <setting name=\"{clave}\" serializeAs=\"String\">\r\n        <value>{valor}</value>\r\n      </setting>";
            var data = File.ReadAllText(filename);
            //data = data.Trim();
            //find = find.Trim();
            if (data.IndexOf(find) == -1)
                return false;
            return true;
        }
        public static class Colors
        {
            public static Color HfColor = Color.FromArgb(55, 145, 108);
            public static Color FDColor = Color.FromArgb(90, 90, 90);
            public static Color Blue = Color.FromArgb(0, 204, 255);         //Tecla Azul(0,204,255)
            public static Color HeaderBlue = Color.FromArgb(51, 102, 255);  //Fondo Área Información Azul Aena(51,102,255)
            public static Color LightBlue = Color.FromArgb(179, 217, 255);
            public static Color Orange = Color.FromArgb(255, 153, 0);       //Tecla Naranja(255,153,0)
            public static Color Green = Color.FromArgb(0, 204, 153);        //Tecla Verde(0,204,153)
            public static Color LightGreen = Color.FromArgb(213, 255, 213);
            public static Color Red = Color.FromArgb(255, 0, 0);            //Tecla Rojo(255,0,0)
            public static Color Gray = Color.FromArgb(192, 192, 192);       //Tecla Gris(192,192,192)
            public static Color LightGray = Color.FromArgb(221, 221, 221);
            public static Color DarkGray = Color.FromArgb(114, 114, 114);   //Tecla Gris Oscuro(114,114,114)
            public static Color Yellow = Color.FromArgb(255, 255, 0);       //Tecla Amarillo(255,255,0)
            public static Color White = Color.FromArgb(255, 255, 255);      //Tecla Blanco(255,255,255)
            public static Color Violet = Color.FromArgb(204, 153, 255);     //Tecla Violeta(204,153,255)
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
        public static Color ButtonColorN = Color.Black;
        public static Color ButtonColorDisabled = SystemColors.Control;
        public static Color BorderColor = Color.Black;
        public static Color BorderColorDisabled = Color.Gainsboro;
        public static int ButtonCornerRadius = 4;
        public static bool UseModernStyle = true;
        public static bool ModoNocturno = true;
        public static Color AspaColor = Color.Magenta;
        public static Color HeaderBlue = Color.FromArgb(0, 51, 102, 255);
        public static Color HeaderKhaki = Color.Khaki;
        public static Color cccmnProgressBar = Color.Green;
        public static Color TextoRadioColor = Color.Black;
        public static Color TextoTfColor = Color.Black;
        public static Color TextoTfColorN = Color.White;
        public static Color TextoHeaderColor = Color.Black;
        public static Color TextoRColor = Color.Black;// letra R
        public static Color TextoFrColor = Color.Red;// Frecuencia
        public static Color TextoEmColor = Color.White;// Emplazamiento
        public static Color PttColor = Color.Red;
        public static Color SquColor = Color.Green;
        public static Color TextColor = Color.Black;
        public static Color TextColorDisabled = Color.White;
        public static Color FondoColorConversacion = Color.Green;
        public static Color FondoColorBloqueo = Color.Red;
        public static Color FondoColorConferencia = Color.Green;
        public static Color TextoColorConferencia = Color.Green;
        public static Color FondoColorRetenida = Color.Yellow;
        public static Color TextoColorRetenida = Color.Black;
        public static Color FondoColorFuncionActiva = Color.Blue;
        public static Color TextoFondoFuncionActiva = Color.Black;
        public static Color FondoColorMemorizada = Color.Orange;
        public static Color FondoColorRemoteMem = Color.DarkGray;
        public static Color FondoColorHold = Color.Green;

        public static Color FondoColorRemoteIn = Color.DarkGray;
        public static Color FondoColorCongestion = Color.Red;
        public static Color FondoColorInPrio = Color.Orange;
        public static Color FondoColorNotAllowed = Color.Yellow;
        public static Color FondoColorInProcess = Color.Yellow;
        // 
        public static Color FondoColorOut = Color.Blue;
        public static Color FondoColorConf = Color.Green;
        public static Color FondoColorbusy = Color.Red;
        public static Color FondoColorMem = Color.Orange;
        public static Color FondoColorNotAllow = Color.Yellow;

        //Linea caliente
        public static Color FondoLcRxRx = Color.Green;
        public static Color FondoLcRxMem = Color.Orange;
        public static Color FondolcTxtx = Color.Green;
        public static Color FondolcTxCongestion = Color.Red;
        public static Color FondolcTxBusy = Color.Red;
        // nuevos colores
        public static Color Color_HfColor = Colors.HfColor;
        public static Color Color_FDColor = Colors.FDColor;
        public static Color Color_TlfStateOut = Colors.Blue;
        public static Color Color_IsListen = Colors.Orange;
        public static Color Color_Busy = Colors.Red;
        public static Color Color_blink = Colors.Yellow;
        public static Color Color_congestion = Colors.Red;

        // LALM: 210203
        // Funcion nueva permite elegir un brush del stack, sin crearlo
        public static Brush BrushG(Color c)
        {
            String tmp = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Color)).ConvertToString(c);
            if (tmp == "Black")
                return Brushes.Black;
            else if (tmp == "White")
                return Brushes.White;
            else if (tmp == "Blue")
                return Brushes.Blue;
            else if (tmp == "LightBlue")
                return Brushes.LightBlue;
            else if (tmp == "Orange")
                return Brushes.Orange;
            else if (tmp == "Green")
                return Brushes.Green;
            else if (tmp == "LightGreen")
                return Brushes.LightGreen;
            else if (tmp == "Red")
                return Brushes.Red;
            else if (tmp == "Gray")
                return Brushes.Gray;
            else if (tmp == "LightGray")
                return Brushes.LightGray;
            else if (tmp == "DarkGray")
                return Brushes.DarkGray;
            else if (tmp == "Yellow")
                return Brushes.Yellow;
            else if (tmp == "White")
                return Brushes.White;
            else if (tmp == "Violet")
                return Brushes.Violet;
            else if (tmp == "Brown")
                return Brushes.Brown;
            else if (tmp == "Black")
                return Brushes.Black;
            else
                return Brushes.Magenta;

        }
    }
}

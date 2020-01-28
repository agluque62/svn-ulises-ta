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

namespace HMI.Presentation.Urr.UI
{
    public class UrrWidthBar : System.Windows.Forms.Button
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
        public int cursorLevel = 1;
        public int actualValue = 0;
        public int cursorPosition = 0;
        public int maximum = 100;
        public int minimum = 0;
        //Available forms -- Only Right Arrow or Triangle
        public enum ButtonsShapes
        {
            Rect,
            //RoundRect,
            //Circle,
            //RightTriangle
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

        public String ButtonText
        {
            get { return text; }
            set { text = value; Invalidate(); }
        }

        public int BorderWidth
        {
            get { return borderWidth; }
            set { borderWidth = value; Invalidate(); }
        }

        public int Value
        {
            get { return actualValue; }
            set { actualValue = value; Invalidate(); }
        }
        public int Maximum
        {
            get { return maximum; }
            set { maximum = value; Invalidate(); }
        }
        public int Minimum
        {
            get { return minimum; }
            set { minimum = value; Invalidate(); }
        }
        
        void SetBorderColor(Color bdrColor)
        {
            int red = bdrColor.R - 40;
            int green = bdrColor.G - 40;
            int blue = bdrColor.B - 40;
            if (red < 0)
                red = 0;
            if (green < 0)
                green = 0;
            if (blue < 0)
                blue = 0;

            buttonborder_1 = Color.FromArgb(red, green, blue);
            buttonborder_2 = bdrColor;
        }


        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                if (borderColor == Color.Transparent)
                {
                    buttonborder_1 = Color.FromArgb(220, 220, 220);
                    buttonborder_2 = Color.FromArgb(150, 150, 150);
                }
                else
                {
                    SetBorderColor(borderColor);
                }

            }
        }

        public Color StartColor
        {
            get { return color1; }
            set { color1 = value; Invalidate(); }
        }
        public Color EndColor
        {
            get { return color2; }
            set { color2 = value; Invalidate(); }
        }
        public Color MouseHoverColor1
        {
            get { return m_hovercolor1; }
            set { m_hovercolor1 = value; Invalidate(); }
        }
        public Color MouseHoverColor2
        {
            get { return m_hovercolor2; }
            set { m_hovercolor2 = value; Invalidate(); }
        }
        public Color MouseClickColor1
        {
            get { return clickcolor1; }
            set { clickcolor1 = value; Invalidate(); }
        }
        public Color MouseClickColor2
        {
            get { return clickcolor2; }
            set { clickcolor2 = value; Invalidate(); }
        }

        public int Transparent1
        {
            get { return color1Transparent; }
            set
            {
                color1Transparent = value;
                if (color1Transparent > 255)
                {
                    color1Transparent = 255;
                    Invalidate();
                }
                else
                    Invalidate();
            }
        }

        public int Transparent2
        {
            get { return color2Transparent; }
            set
            {
                color2Transparent = value;
                if (color2Transparent > 255)
                {
                    color2Transparent = 255;
                    Invalidate();
                }
                else
                    Invalidate();
            }
        }

        public int GradientAngle
        {
            get { return angle; }
            set { angle = value; Invalidate(); }
        }

        public int TextLocation_X
        {
            get { return textX; }
            set { textX = value; Invalidate(); }
        }
        public int TextLocation_Y
        {
            get { return textY; }
            set { textY = value; Invalidate(); }
        }

        public Boolean ShowButtontext
        {
            get { return showButtonText; }
            set { showButtonText = value; Invalidate(); }
        }

        //Constructor
        public UrrWidthBar()
        {
            this.Size = new Size(110, 10);
            this.BackColor = Color.Transparent;
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 1;
            this.FlatAppearance.MouseOverBackColor = Color.Transparent;
            this.FlatAppearance.MouseDownBackColor = Color.Transparent;
            text = this.Text;
        }


        //method mouse enter  
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            clr1 = color1;
            clr2 = color2;
            color1 = m_hovercolor1;
            color2 = m_hovercolor2;
        }

        //method mouse leave  
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            color1 = clr1;
            color2 = clr2;
            SetBorderColor(borderColor);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            color1 = clickcolor1;
            color2 = clickcolor2;

            int red = borderColor.R - 40;
            int green = borderColor.G - 40;
            int blue = borderColor.B - 40;
            if (red < 0)
                red = 0;
            if (green < 0)
                green = 0;
            if (blue < 0)
                blue = 0;

            buttonborder_2 = Color.FromArgb(red, green, blue);
            buttonborder_1 = borderColor;
            this.Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            OnMouseLeave(mevent);
            color1 = clr1;
            color2 = clr2;
            SetBorderColor(borderColor);
            this.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            color1 = clr1;
            color2 = clr2;
            this.Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            textX = (int)((this.Width / 3) - 1);
            textY = (int)((this.Height / 3) + 5);
        }


        //draw circular button function  
        void DrawCircularButton(Graphics g)
        {
            Color c1 = Color.FromArgb(color1Transparent, color1);
            Color c2 = Color.FromArgb(color2Transparent, color2);


            Brush b = new System.Drawing.Drawing2D.LinearGradientBrush(ClientRectangle, c1, c2, angle);
            g.FillEllipse(b, 5, 5, this.Width - 10, this.Height - 10);


            for (int i = 0; i < borderWidth; i++)
            {
                g.DrawArc(new Pen(new SolidBrush(buttonborder_1)), 5 + i, 5, this.Width - 10, this.Height - 10, 120, 180);
                g.DrawArc(new Pen(new SolidBrush(buttonborder_2)), 5, 5, this.Width - (10 + i), this.Height - 10, 300, 180);
            }

            if (showButtonText)
            {
                Point p = new Point(textX, textY);
                SolidBrush frcolor = new SolidBrush(this.ForeColor);
                g.DrawString(text, this.Font, frcolor, p);
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

            //Pintamos el rect�ngulo que va a contener el �rea seleccionable
            Brush b = new SolidBrush(Color.Gainsboro);
            g.FillRectangle(b, 0, 0, this.Width, this.Height);

            //Degradado
            Brush brush2 = new LinearGradientBrush(ClientRectangle, c1, c2, angle);
            
            //Los tres puntos que representan al tri�ngulo
            Point[] points = { 
                new Point(this.Width, this.Height/2), 
                new Point(0, this.Height), 
                new Point(0, 0) 
            };
            g.FillPolygon(brush2, points);

            //Con esto pintamos el grosor de los bordes. i representa el tama�o del grosor que nos mandan
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

        //Draws the bar and the white rectangle 
        void DrawRectangularButton(Graphics g)
        {
            Color c1 = Color.FromArgb(color1Transparent, color1);
            Color c2 = Color.FromArgb(color2Transparent, color2);

            //Bar
            Brush b = new System.Drawing.Drawing2D.LinearGradientBrush(ClientRectangle, c1, c2, angle);
            g.FillRectangle(b, 0, 0, this.Width, this.Height);


            for (int i = 0; i < borderWidth; i++)
            {
                g.DrawLine(new Pen(new SolidBrush(buttonborder_1)), this.Width - i, 0, this.Width - i, this.Height);
                g.DrawLine(new Pen(new SolidBrush(buttonborder_1)), 0, this.Height - i, this.Width, this.Height - i);

                g.DrawLine(new Pen(new SolidBrush(buttonborder_2)), 0 + i, 0, 0 + i, this.Height);
                g.DrawLine(new Pen(new SolidBrush(buttonborder_2)), 0, 0 + i, this.Width, i);
            }

            //White rectangle
            Brush b2 = new SolidBrush(Color.White);
            //actualValue=0;;actualValue=a lo que sea
            //cursorPosition=0;;cursorPosition=6

            //Normalizar los valores
            if ((actualValue - cursorPosition)>2 )
                actualValue -= 1;
            
            if ((actualValue - cursorPosition)<=-1)
                actualValue += 1;


            if (actualValue >= 1 && actualValue <= 7)
                cursorPosition = actualValue - 1;

            if (actualValue != 7)//Para pintar el trozo final de la barra de blanco
                g.FillRectangle(b2, cursorPosition * (this.Width / 7), 0, this.Width / 7, this.Height);
            else
                g.FillRectangle(b2, cursorPosition * (this.Width / 7), 0, this.Width, this.Height);


            //Black Marks
            Brush b3 = new SolidBrush(Color.Black);

            for (int i = 0; i < 8; i++)
                g.DrawLine(new Pen(Color.Black), i * (this.Width / 7), 0, i * (this.Width / 7), this.Height);
            //g.DrawLine(new Pen(new SolidBrush(Color.Black)), cursorLevel * (this.Width / 7), this.Height, this.Width / 7, this.Height);

            /*Brush b3 = new SolidBrush(Color.Black);
            Pen pen = new Pen(b3);
            Point[] points = { 
                new Point(0, 0),
                new Point(this.Width / 7, this.Height /7), 
                new Point(0, this.Height / 7)
            };
            g.DrawPolygon(pen, points);
            */
            if (showButtonText)
            {
                Point p = new Point(textX, textY);
                SolidBrush frcolor = new SolidBrush(this.ForeColor);
                g.DrawString(text, this.Font, frcolor, p);
            }


            b.Dispose();
        }


        //draw round rectangular button function  
        void DrawRoundRectangularButton(Graphics g)
        {
            Color c1 = Color.FromArgb(color1Transparent, color1);
            Color c2 = Color.FromArgb(color2Transparent, color2);


            Brush b = new System.Drawing.Drawing2D.LinearGradientBrush(ClientRectangle, c1, c2, angle);

            Region region = new System.Drawing.Region(new Rectangle(5, 5, this.Width, this.Height));

            GraphicsPath grp = new GraphicsPath();
            grp.AddArc(5, 5, 40, 40, 180, 90);
            grp.AddLine(25, 5, this.Width - 25, 5);
            grp.AddArc(this.Width - 45, 5, 40, 40, 270, 90);
            grp.AddLine(this.Width - 5, 25, this.Width - 5, this.Height - 25);
            grp.AddArc(this.Width - 45, this.Height - 45, 40, 40, 0, 90);
            grp.AddLine(25, this.Height - 5, this.Width - 25, this.Height - 5);
            grp.AddArc(5, this.Height - 45, 40, 40, 90, 90);
            grp.AddLine(5, 25, 5, this.Height - 25);

            region.Intersect(grp);

            g.FillRegion(b, region);

            for (int i = 0; i < borderWidth; i++)
            {
                g.DrawArc(new Pen(buttonborder_1), 5 + i, 5 + i, 40, 40, 180, 90);
                g.DrawLine(new Pen(buttonborder_1), 25, 5 + i, this.Width - 25, 5 + i);
                g.DrawArc(new Pen(buttonborder_1), this.Width - 45 - i, 5 + i, 40, 40, 270, 90);
                g.DrawLine(new Pen(buttonborder_1), 5 + i, 25, 5 + i, this.Height - 25);


                g.DrawLine(new Pen(buttonborder_2), this.Width - 5 - i, 25, this.Width - 5 - i, this.Height - 25);
                g.DrawArc(new Pen(buttonborder_2), this.Width - 45 - i, this.Height - 45 - i, 40, 40, 0, 90);
                g.DrawLine(new Pen(buttonborder_2), 25, this.Height - 5 - i, this.Width - 25, this.Height - 5 - i);
                g.DrawArc(new Pen(buttonborder_2), 5 + i, this.Height - 45 - i, 40, 40, 90, 90);

            }



            if (showButtonText)
            {
                Point p = new Point(textX, textY);
                SolidBrush frcolor = new SolidBrush(this.ForeColor);
                g.DrawString(text, this.Font, frcolor, p);
            }

            b.Dispose();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            switch (buttonShape)
            {
                /*case ButtonsShapes.RightTriangle:
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    this.DrawRightTriangle(e.Graphics);
                    break;
                */
                /*case ButtonsShapes.Circle:
                    this.DrawCircularButton(e.Graphics);
                    break;
                */
                case ButtonsShapes.Rect:
                    this.DrawRectangularButton(e.Graphics);
                    break;
                /*
                case ButtonsShapes.RoundRect:
                    this.DrawRoundRectangularButton(e.Graphics);
                    break;
                */
            }
        }

    }  
}

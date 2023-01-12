
using HMI.Model.Module.UI;
using System.Collections.Generic;

namespace HMI.Presentation.Twr.Views
{
    partial class CambioFrecuenciaView
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label _TitleLB;
            this.listboxFr = new System.Windows.Forms.ListBox();
            this.hmiButtonCancelar = new HMI.Model.Module.UI.HMIButton();
            this.hmiButtonAceptar = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton1 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton2 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton3 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton4 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton8 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton7 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton6 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton5 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton12 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton11 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton10 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton9 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton16 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton15 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton14 = new HMI.Model.Module.UI.HMIButton();
            this.hmiButton13 = new HMI.Model.Module.UI.HMIButton();
            _TitleLB = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _TitleLB
            // 
            _TitleLB.AccessibleName = "Titulo";
            _TitleLB.AllowDrop = true;
            _TitleLB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            _TitleLB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            _TitleLB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _TitleLB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            _TitleLB.Location = new System.Drawing.Point(34, 20);
            _TitleLB.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            _TitleLB.Name = "_TitleLB";
            _TitleLB.Size = new System.Drawing.Size(156, 40);
            _TitleLB.TabIndex = 1;
            _TitleLB.Text = "Cambio de Frecuencia";
            _TitleLB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _TitleLB.Click += new System.EventHandler(this._TitleLB_Click);
            // 
            // listboxFr
            // 
            this.listboxFr.FormattingEnabled = true;
            this.listboxFr.Items.AddRange(new object[] {
            "122.001",
            "122.002",
            "122.003",
            "122.004",
            "122.005"});
            this.listboxFr.Location = new System.Drawing.Point(129, 335);
            this.listboxFr.Name = "listboxFr";
            this.listboxFr.Size = new System.Drawing.Size(61, 17);
            this.listboxFr.TabIndex = 2;
            this.listboxFr.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.listboxFr.VisibleChanged += new System.EventHandler(this.listboxFr_VisibleChanged);
            // 
            // hmiButtonCancelar
            // 
            this.hmiButtonCancelar.DrawX = false;
            this.hmiButtonCancelar.Location = new System.Drawing.Point(158, 228);
            this.hmiButtonCancelar.Name = "hmiButtonCancelar";
            this.hmiButtonCancelar.Permitted = true;
            this.hmiButtonCancelar.Size = new System.Drawing.Size(117, 41);
            this.hmiButtonCancelar.TabIndex = 5;
            this.hmiButtonCancelar.Text = "Cancelar";
            this.hmiButtonCancelar.Click += new System.EventHandler(this.Cancelar_Click);
            // 
            // hmiButtonAceptar
            // 
            this.hmiButtonAceptar.DrawX = false;
            this.hmiButtonAceptar.Location = new System.Drawing.Point(15, 228);
            this.hmiButtonAceptar.Name = "hmiButtonAceptar";
            this.hmiButtonAceptar.Permitted = true;
            this.hmiButtonAceptar.Size = new System.Drawing.Size(116, 41);
            this.hmiButtonAceptar.TabIndex = 6;
            this.hmiButtonAceptar.Text = "Aceptar";
            this.hmiButtonAceptar.Click += new System.EventHandler(this.Aceptar_Click);
            // hmiButton1
            // 
            this.hmiButton1.DrawX = false;
            this.hmiButton1.Location = new System.Drawing.Point(3, 66);
            this.hmiButton1.Name = "hmiButton1";
            this.hmiButton1.Permitted = true;
            this.hmiButton1.Size = new System.Drawing.Size(66, 33);
            this.hmiButton1.TabIndex = 18;
            this.hmiButton1.Text = "hmiButton1";
            this.hmiButton1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.hmiButton_MouseDown);
            this.hmiButton1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.hmiButton_MouseUp);
            // 
            // hmiButton2
            // 
            this.hmiButton2.DrawX = false;
            this.hmiButton2.Location = new System.Drawing.Point(75, 66);
            this.hmiButton2.Name = "hmiButton2";
            this.hmiButton2.Permitted = true;
            this.hmiButton2.Size = new System.Drawing.Size(66, 33);
            this.hmiButton2.TabIndex = 19;
            this.hmiButton2.Text = "hmiButton1";
            // 
            // hmiButton3
            // 
            this.hmiButton3.DrawX = false;
            this.hmiButton3.Location = new System.Drawing.Point(147, 66);
            this.hmiButton3.Name = "hmiButton3";
            this.hmiButton3.Permitted = true;
            this.hmiButton3.Size = new System.Drawing.Size(66, 33);
            this.hmiButton3.TabIndex = 20;
            this.hmiButton3.Text = "hmiButton1";
            // 
            // hmiButton4
            // 
            this.hmiButton4.DrawX = false;
            this.hmiButton4.Location = new System.Drawing.Point(219, 66);
            this.hmiButton4.Name = "hmiButton4";
            this.hmiButton4.Permitted = true;
            this.hmiButton4.Size = new System.Drawing.Size(66, 33);
            this.hmiButton4.TabIndex = 21;
            this.hmiButton4.Text = "hmiButton1";
            // 
            // hmiButton8
            // 
            this.hmiButton8.DrawX = false;
            this.hmiButton8.Location = new System.Drawing.Point(3, 105);
            this.hmiButton8.Name = "hmiButton8";
            this.hmiButton8.Permitted = true;
            this.hmiButton8.Size = new System.Drawing.Size(66, 33);
            this.hmiButton8.TabIndex = 25;
            this.hmiButton8.Text = "hmiButton1";
            // 
            // hmiButton7
            // 
            this.hmiButton7.DrawX = false;
            this.hmiButton7.Location = new System.Drawing.Point(75, 105);
            this.hmiButton7.Name = "hmiButton7";
            this.hmiButton7.Permitted = true;
            this.hmiButton7.Size = new System.Drawing.Size(66, 33);
            this.hmiButton7.TabIndex = 24;
            this.hmiButton7.Text = "hmiButton1";
            // 
            // hmiButton6
            // 
            this.hmiButton6.DrawX = false;
            this.hmiButton6.Location = new System.Drawing.Point(147, 105);
            this.hmiButton6.Name = "hmiButton6";
            this.hmiButton6.Permitted = true;
            this.hmiButton6.Size = new System.Drawing.Size(66, 33);
            this.hmiButton6.TabIndex = 23;
            this.hmiButton6.Text = "hmiButton1";
            // 
            // hmiButton5
            // 
            this.hmiButton5.DrawX = false;
            this.hmiButton5.Location = new System.Drawing.Point(219, 105);
            this.hmiButton5.Name = "hmiButton5";
            this.hmiButton5.Permitted = true;
            this.hmiButton5.Size = new System.Drawing.Size(66, 33);
            this.hmiButton5.TabIndex = 22;
            this.hmiButton5.Text = "hmiButton1";
            // 
            // hmiButton12
            // 
            this.hmiButton12.DrawX = false;
            this.hmiButton12.Location = new System.Drawing.Point(3, 144);
            this.hmiButton12.Name = "hmiButton12";
            this.hmiButton12.Permitted = true;
            this.hmiButton12.Size = new System.Drawing.Size(66, 33);
            this.hmiButton12.TabIndex = 29;
            this.hmiButton12.Text = "hmiButton1";
            // 
            // hmiButton11
            // 
            this.hmiButton11.DrawX = false;
            this.hmiButton11.Location = new System.Drawing.Point(75, 144);
            this.hmiButton11.Name = "hmiButton11";
            this.hmiButton11.Permitted = true;
            this.hmiButton11.Size = new System.Drawing.Size(66, 33);
            this.hmiButton11.TabIndex = 28;
            this.hmiButton11.Text = "hmiButton1";
            // 
            // hmiButton10
            // 
            this.hmiButton10.DrawX = false;
            this.hmiButton10.Location = new System.Drawing.Point(147, 144);
            this.hmiButton10.Name = "hmiButton10";
            this.hmiButton10.Permitted = true;
            this.hmiButton10.Size = new System.Drawing.Size(66, 33);
            this.hmiButton10.TabIndex = 27;
            this.hmiButton10.Text = "hmiButton1";
            // 
            // hmiButton9
            // 
            this.hmiButton9.DrawX = false;
            this.hmiButton9.Location = new System.Drawing.Point(219, 144);
            this.hmiButton9.Name = "hmiButton9";
            this.hmiButton9.Permitted = true;
            this.hmiButton9.Size = new System.Drawing.Size(66, 33);
            this.hmiButton9.TabIndex = 26;
            this.hmiButton9.Text = "hmiButton1";
            // 
            // hmiButton16
            // 
            this.hmiButton16.DrawX = false;
            this.hmiButton16.Location = new System.Drawing.Point(3, 180);
            this.hmiButton16.Name = "hmiButton16";
            this.hmiButton16.Permitted = true;
            this.hmiButton16.Size = new System.Drawing.Size(66, 33);
            this.hmiButton16.TabIndex = 33;
            this.hmiButton16.Text = "hmiButton1";
            // 
            // hmiButton15
            // 
            this.hmiButton15.DrawX = false;
            this.hmiButton15.Location = new System.Drawing.Point(75, 180);
            this.hmiButton15.Name = "hmiButton15";
            this.hmiButton15.Permitted = true;
            this.hmiButton15.Size = new System.Drawing.Size(66, 33);
            this.hmiButton15.TabIndex = 32;
            this.hmiButton15.Text = "hmiButton1";
            // 
            // hmiButton14
            // 
            this.hmiButton14.DrawX = false;
            this.hmiButton14.Location = new System.Drawing.Point(147, 180);
            this.hmiButton14.Name = "hmiButton14";
            this.hmiButton14.Permitted = true;
            this.hmiButton14.Size = new System.Drawing.Size(66, 33);
            this.hmiButton14.TabIndex = 31;
            this.hmiButton14.Text = "hmiButton1";
            // 
            // hmiButton13
            // 
            this.hmiButton13.DrawX = false;
            this.hmiButton13.Location = new System.Drawing.Point(219, 180);
            this.hmiButton13.Name = "hmiButton13";
            this.hmiButton13.Permitted = true;
            this.hmiButton13.Size = new System.Drawing.Size(66, 33);
            this.hmiButton13.TabIndex = 30;
            this.hmiButton13.Text = "hmiButton1";
            // 
            // CambioFrecuenciaView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.hmiButton16);
            this.Controls.Add(this.hmiButton15);
            this.Controls.Add(this.hmiButton14);
            this.Controls.Add(this.hmiButton13);
            this.Controls.Add(this.hmiButton12);
            this.Controls.Add(this.hmiButton11);
            this.Controls.Add(this.hmiButton10);
            this.Controls.Add(this.hmiButton9);
            this.Controls.Add(this.hmiButton8);
            this.Controls.Add(this.hmiButton7);
            this.Controls.Add(this.hmiButton6);
            this.Controls.Add(this.hmiButton5);
            this.Controls.Add(this.hmiButton4);
            this.Controls.Add(this.hmiButton3);
            this.Controls.Add(this.hmiButton2);
            this.Controls.Add(this.hmiButton1);
            this.Controls.Add(this.hmiButtonAceptar);
            this.Controls.Add(this.hmiButtonCancelar);
            this.Controls.Add(this.listboxFr);
            this.Controls.Add(_TitleLB);
            this.Name = "CambioFrecuenciaView";
            this.Size = new System.Drawing.Size(349, 659);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listboxFr;
        private Model.Module.UI.HMIButton hmiButtonCancelar;
        private Model.Module.UI.HMIButton hmiButtonAceptar;
        private List<RdButton> _RdButtons = new List<RdButton>();
        private List<HMIButton> _HMIButtons = new List<HMIButton>();
        private HMIButton hmiButton1;
        private HMIButton hmiButton2;
        private HMIButton hmiButton3;
        private HMIButton hmiButton4;
        private HMIButton hmiButton8;
        private HMIButton hmiButton7;
        private HMIButton hmiButton6;
        private HMIButton hmiButton5;
        private HMIButton hmiButton12;
        private HMIButton hmiButton11;
        private HMIButton hmiButton10;
        private HMIButton hmiButton9;
        private HMIButton hmiButton16;
        private HMIButton hmiButton15;
        private HMIButton hmiButton14;
        private HMIButton hmiButton13;
    }
}

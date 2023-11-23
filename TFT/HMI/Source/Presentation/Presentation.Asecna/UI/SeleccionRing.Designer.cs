
namespace HMI.Presentation.Asecna.UI
{
    partial class SeleccionRing
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
            this._MemTLP = new System.Windows.Forms.TableLayoutPanel();
            this.Tono = new System.Windows.Forms.Label();
            this.TipoLlamadaComboBox = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.aceptarButton = new HMI.Model.Module.UI.HMIButton();
            this.tonoComboBox = new System.Windows.Forms.ComboBox();
            this.tonoprioComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this._MemTLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // _MemTLP
            // 
            this._MemTLP.ColumnCount = 2;
            this._MemTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._MemTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this._MemTLP.Controls.Add(this.Tono, 0, 3);
            this._MemTLP.Controls.Add(this.Label1, 0, 1);
            this._MemTLP.Controls.Add(this.aceptarButton, 0, 12);
            this._MemTLP.Controls.Add(this.tonoComboBox, 0, 7);
            this._MemTLP.Controls.Add(this.label2, 0, 8);
            this._MemTLP.Controls.Add(this.tonoprioComboBox, 0, 9);
            this._MemTLP.Controls.Add(this.TipoLlamadaComboBox, 0, 2);
            this._MemTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this._MemTLP.Location = new System.Drawing.Point(0, 0);
            this._MemTLP.Name = "_MemTLP";
            this._MemTLP.RowCount = 15;
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._MemTLP.Size = new System.Drawing.Size(225, 171);
            this._MemTLP.TabIndex = 1;
            // 
            // Tono
            // 
            this.Tono.AllowDrop = true;
            this.Tono.AutoSize = true;
            this.Tono.Location = new System.Drawing.Point(3, 40);
            this.Tono.Name = "Tono";
            this.Tono.Size = new System.Drawing.Size(32, 13);
            this.Tono.TabIndex = 5;
            this.Tono.Text = "Tono";
            // 
            // TipoLlamadaComboBox
            // 
            this._MemTLP.SetColumnSpan(this.TipoLlamadaComboBox, 2);
            this.TipoLlamadaComboBox.FormattingEnabled = true;
            this.TipoLlamadaComboBox.Location = new System.Drawing.Point(3, 16);
            this.TipoLlamadaComboBox.Name = "TipoLlamadaComboBox";
            this.TipoLlamadaComboBox.Size = new System.Drawing.Size(216, 21);
            this.TipoLlamadaComboBox.TabIndex = 6;
            this.TipoLlamadaComboBox.SelectedIndexChanged += new System.EventHandler(this.TipoLlamadaComboBox_SelectedIndexChanged);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this._MemTLP.SetColumnSpan(this.Label1, 2);
            this.Label1.Location = new System.Drawing.Point(3, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(71, 13);
            this.Label1.TabIndex = 4;
            this.Label1.Text = "Tipo Llamada";
            // 
            // aceptarButton
            // 
            this.aceptarButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.aceptarButton.DrawX = false;
            this.aceptarButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.aceptarButton.IsConferencePreprogramada = false;
            this.aceptarButton.Location = new System.Drawing.Point(0, 125);
            this.aceptarButton.Margin = new System.Windows.Forms.Padding(0, 5, 5, 5);
            this.aceptarButton.Name = "aceptarButton";
            this.aceptarButton.Permitted = true;
            this.aceptarButton.Size = new System.Drawing.Size(219, 21);
            this.aceptarButton.TabIndex = 0;
            this.aceptarButton.Text = "Aceptar";
            this.aceptarButton.Click += new System.EventHandler(this.aceptarButton_Click);
            // 
            // tonoComboBox
            // 
            this.tonoComboBox.AllowDrop = true;
            this.tonoComboBox.FormattingEnabled = true;
            this.tonoComboBox.Location = new System.Drawing.Point(3, 96);
            this.tonoComboBox.Name = "tonoComboBox";
            this.tonoComboBox.Size = new System.Drawing.Size(216, 21);
            this.tonoComboBox.TabIndex = 6;
            this.tonoComboBox.SelectedIndexChanged += new System.EventHandler(this.tonoComboBox_SelectedIndexChanged);
            // 
            // tonoprioComboBox
            // 
            this.tonoprioComboBox.AllowDrop = true;
            this.tonoprioComboBox.FormattingEnabled = true;
            this.tonoprioComboBox.Location = new System.Drawing.Point(3, 56);
            this.tonoprioComboBox.Name = "tonoprioComboBox";
            this.tonoprioComboBox.Size = new System.Drawing.Size(216, 21);
            this.tonoprioComboBox.TabIndex = 10;
            this.tonoprioComboBox.SelectedIndexChanged += new System.EventHandler(this.tonoprioComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AllowDrop = true;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Tono Prioritario";
            // 
            // SeleccionRing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._MemTLP);
            this.Name = "SeleccionRing";
            this.Size = new System.Drawing.Size(225, 171);
            this.VisibleChanged += new System.EventHandler(this.SeleccionRing_VisibleChanged);
            this._MemTLP.ResumeLayout(false);
            this._MemTLP.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _MemTLP;
        private Model.Module.UI.HMIButton aceptarButton;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Label Tono;
        private System.Windows.Forms.ComboBox TipoLlamadaComboBox;
        private System.Windows.Forms.ComboBox tonoComboBox;
        private System.Windows.Forms.ComboBox tonoprioComboBox;
        private System.Windows.Forms.Label label2;
    }
}

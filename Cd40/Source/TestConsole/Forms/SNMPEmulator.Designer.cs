﻿namespace AdminConsole
{
    partial class SNMPEmulator
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SNMPEmulator));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.menuClose = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.newToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.menuOpen = new System.Windows.Forms.ToolStripButton();
            this.saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ElementList = new System.Windows.Forms.FlowLayoutPanel();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuClose,
            this.toolStripSeparator2,
            this.newToolStripButton,
            this.menuOpen,
            this.saveToolStripButton,
            this.toolStripSeparator1,
            this.helpToolStripButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(567, 25);
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "ToolStrip";
            // 
            // menuClose
            // 
            this.menuClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.menuClose.Image = ((System.Drawing.Image)(resources.GetObject("menuClose.Image")));
            this.menuClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.menuClose.Name = "menuClose";
            this.menuClose.Size = new System.Drawing.Size(23, 22);
            this.menuClose.Text = "menuClose";
            this.menuClose.Click += new System.EventHandler(this.menuClose_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // newToolStripButton
            // 
            this.newToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripButton.Image")));
            this.newToolStripButton.ImageTransparentColor = System.Drawing.Color.Black;
            this.newToolStripButton.Name = "newToolStripButton";
            this.newToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.newToolStripButton.Text = "Nuevo";
            // 
            // menuOpen
            // 
            this.menuOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.menuOpen.Image = ((System.Drawing.Image)(resources.GetObject("menuOpen.Image")));
            this.menuOpen.ImageTransparentColor = System.Drawing.Color.Black;
            this.menuOpen.Name = "menuOpen";
            this.menuOpen.Size = new System.Drawing.Size(23, 22);
            this.menuOpen.Text = "Abrir";
            this.menuOpen.Click += new System.EventHandler(this.menuOpen_Click);
            // 
            // saveToolStripButton
            // 
            this.saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripButton.Image")));
            this.saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Black;
            this.saveToolStripButton.Name = "saveToolStripButton";
            this.saveToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.saveToolStripButton.Text = "Guardar";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // helpToolStripButton
            // 
            this.helpToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.helpToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("helpToolStripButton.Image")));
            this.helpToolStripButton.ImageTransparentColor = System.Drawing.Color.Black;
            this.helpToolStripButton.Name = "helpToolStripButton";
            this.helpToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.helpToolStripButton.Text = "Ayuda";
            // 
            // ElementList
            // 
            this.ElementList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ElementList.Location = new System.Drawing.Point(0, 25);
            this.ElementList.Name = "ElementList";
            this.ElementList.Size = new System.Drawing.Size(567, 330);
            this.ElementList.TabIndex = 3;
            // 
            // SNMPEmulator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 355);
            this.Controls.Add(this.ElementList);
            this.Controls.Add(this.toolStrip);
            this.Name = "SNMPEmulator";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Title";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton newToolStripButton;
        private System.Windows.Forms.ToolStripButton menuOpen;
        private System.Windows.Forms.ToolStripButton saveToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton helpToolStripButton;
        private System.Windows.Forms.ToolStripButton menuClose;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.FlowLayoutPanel ElementList;

    }
}


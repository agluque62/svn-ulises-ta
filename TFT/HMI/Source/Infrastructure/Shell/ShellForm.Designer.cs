//----------------------------------------------------------------------------------------
// patterns & practices - Smart Client Software Factory - Guidance Package
//
// This file was generated by this guidance package as part of the solution template
//
// The FormShell class represent the main form of your application.
// 
// For more information see: 
// ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/03-01-010-How_to_Create_Smart_Client_Solutions.htm
//
// Latest version of this Guidance Package: http://go.microsoft.com/fwlink/?LinkId=62182
//----------------------------------------------------------------------------------------

namespace HMI.Infrastructure.Shell
{
    partial class ShellForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ContextMenuStrip _MainCMS;
            this._layoutWorkspace = new Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace();
            _MainCMS = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // _MainCMS
            // 
            _MainCMS.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            _MainCMS.Name = "_MainCMS";
            _MainCMS.Size = new System.Drawing.Size(61, 4);
            // 
            // _layoutWorkspace
            // 
            this._layoutWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layoutWorkspace.Location = new System.Drawing.Point(0, 0);
            this._layoutWorkspace.Margin = new System.Windows.Forms.Padding(4);
            this._layoutWorkspace.Name = "_layoutWorkspace";
            this._layoutWorkspace.Size = new System.Drawing.Size(800, 600);
            this._layoutWorkspace.TabIndex = 0;
            this._layoutWorkspace.Text = "_layoutWorkspace";
            // 
            // ShellForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.ContextMenuStrip = _MainCMS;
            this.Controls.Add(this._layoutWorkspace);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ShellForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "ShellForm";
            this.ResumeLayout(false);

        }

        #endregion

		 private Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace _layoutWorkspace;
    }
}

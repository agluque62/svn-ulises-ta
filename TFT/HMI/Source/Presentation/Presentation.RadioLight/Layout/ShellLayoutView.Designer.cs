namespace HMI.Presentation.RadioLight.Layout
{
    partial class ShellLayoutView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._MainTLP = new System.Windows.Forms.TableLayoutPanel();
            this._MainToolsWS = new Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace();
            this._RdWS = new Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace();
            this._MainTLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // _MainTLP
            // 
            this._MainTLP.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this._MainTLP.ColumnCount = 1;
            this._MainTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 98.81306F));
            this._MainTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.186944F));
            this._MainTLP.Controls.Add(this._MainToolsWS, 0, 0);
            this._MainTLP.Controls.Add(this._RdWS, 0, 1);
            this._MainTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this._MainTLP.Location = new System.Drawing.Point(0, 0);
            this._MainTLP.Margin = new System.Windows.Forms.Padding(0);
            this._MainTLP.Name = "_MainTLP";
            this._MainTLP.RowCount = 2;
            this._MainTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this._MainTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 74.76099F));
            this._MainTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.19312F));
            this._MainTLP.Size = new System.Drawing.Size(675, 524);
            this._MainTLP.TabIndex = 0;
            // 
            // _MainToolsWS
            // 
            this._MainTLP.SetColumnSpan(this._MainToolsWS, 2);
            this._MainToolsWS.Dock = System.Windows.Forms.DockStyle.Fill;
            this._MainToolsWS.Location = new System.Drawing.Point(1, 1);
            this._MainToolsWS.Margin = new System.Windows.Forms.Padding(0);
            this._MainToolsWS.Name = "_MainToolsWS";
            this._MainToolsWS.Size = new System.Drawing.Size(673, 72);
            this._MainToolsWS.TabIndex = 0;
            this._MainToolsWS.Text = "deckWorkspace1";
            // 
            // _RdWS
            // 
            this._RdWS.Dock = System.Windows.Forms.DockStyle.Fill;
            this._RdWS.Location = new System.Drawing.Point(1, 74);
            this._RdWS.Margin = new System.Windows.Forms.Padding(0);
            this._RdWS.Name = "_RdWS";
            this._RdWS.Size = new System.Drawing.Size(673, 449);
            this._RdWS.TabIndex = 1;
            this._RdWS.Text = "deckWorkspace2";
            // 
            // ShellLayoutView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this._MainTLP);
            this.Name = "ShellLayoutView";
            this.Size = new System.Drawing.Size(675, 524);
            this._MainTLP.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace _MainToolsWS;
        private Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace _RdWS;
        private System.Windows.Forms.TableLayoutPanel _MainTLP;

    }
}


namespace HMI.Presentation.Urr.Layout
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
            System.Windows.Forms.TableLayoutPanel _MainTLP;
            this._MainToolsWS = new Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace();
            this._RdWS = new Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace();
            this._TlfWS = new Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace();
            this._LcWS = new Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace();
            _MainTLP = new System.Windows.Forms.TableLayoutPanel();
            _MainTLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // _MainTLP
            // 
            _MainTLP.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            _MainTLP.ColumnCount = 2;
            _MainTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.91395F));
            _MainTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.08605F));
            _MainTLP.Controls.Add(this._MainToolsWS, 0, 0);
            _MainTLP.Controls.Add(this._RdWS, 0, 1);
            _MainTLP.Controls.Add(this._TlfWS, 1, 1);
            _MainTLP.Controls.Add(this._LcWS, 0, 2);
            _MainTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            _MainTLP.Location = new System.Drawing.Point(0, 0);
            _MainTLP.Margin = new System.Windows.Forms.Padding(0);
            _MainTLP.Name = "_MainTLP";
            _MainTLP.RowCount = 3;
            _MainTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12F));
            _MainTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 72.27534F));
            _MainTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.67878F));
            _MainTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            _MainTLP.Size = new System.Drawing.Size(675, 524);
            _MainTLP.TabIndex = 0;
            // 
            // _MainToolsWS
            // 
            _MainTLP.SetColumnSpan(this._MainToolsWS, 2);
            this._MainToolsWS.Dock = System.Windows.Forms.DockStyle.Fill;
            this._MainToolsWS.Location = new System.Drawing.Point(1, 1);
            this._MainToolsWS.Margin = new System.Windows.Forms.Padding(0);
            this._MainToolsWS.Name = "_MainToolsWS";
            this._MainToolsWS.Size = new System.Drawing.Size(673, 62);
            this._MainToolsWS.TabIndex = 0;
            this._MainToolsWS.Text = "deckWorkspace1";
            // 
            // _RdWS
            // 
            this._RdWS.Dock = System.Windows.Forms.DockStyle.Fill;
            this._RdWS.Location = new System.Drawing.Point(1, 64);
            this._RdWS.Margin = new System.Windows.Forms.Padding(0);
            this._RdWS.Name = "_RdWS";
            this._RdWS.Size = new System.Drawing.Size(449, 376);
            this._RdWS.TabIndex = 1;
            this._RdWS.Text = "deckWorkspace2";
            // 
            // _TlfWS
            // 
            this._TlfWS.Dock = System.Windows.Forms.DockStyle.Fill;
            this._TlfWS.Location = new System.Drawing.Point(451, 64);
            this._TlfWS.Margin = new System.Windows.Forms.Padding(0);
            this._TlfWS.Name = "_TlfWS";
            this._TlfWS.Size = new System.Drawing.Size(223, 376);
            this._TlfWS.TabIndex = 2;
            this._TlfWS.Text = "deckWorkspace3";
            // 
            // _LcWS
            // 
            _MainTLP.SetColumnSpan(this._LcWS, 2);
            this._LcWS.Dock = System.Windows.Forms.DockStyle.Fill;
            this._LcWS.Location = new System.Drawing.Point(1, 441);
            this._LcWS.Margin = new System.Windows.Forms.Padding(0);
            this._LcWS.Name = "_LcWS";
            this._LcWS.Size = new System.Drawing.Size(673, 82);
            this._LcWS.TabIndex = 3;
            this._LcWS.Text = "deckWorkspace4";
            // 
            // ShellLayoutView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.Controls.Add(_MainTLP);
            this.Name = "ShellLayoutView";
            this.Size = new System.Drawing.Size(675, 524);
            _MainTLP.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace _MainToolsWS;
        private Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace _RdWS;
        private Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace _LcWS;
        private Microsoft.Practices.CompositeUI.WinForms.DeckWorkspace _TlfWS;

    }
}


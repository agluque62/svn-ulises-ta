namespace HMI.Presentation.AUrr.UI
{
    partial class UrrSplitUC
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
            if (disposing)
            {
                if (components != null)
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
            this._SplitTLP = new System.Windows.Forms.TableLayoutPanel();
            this._Jack1PB = new System.Windows.Forms.PictureBox();
            this._Jack4PB = new System.Windows.Forms.PictureBox();
            this._SplitTLP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._Jack1PB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._Jack4PB)).BeginInit();
            this.SuspendLayout();
            // 
            // _SplitTLP
            // 
            this._SplitTLP.ColumnCount = 2;
            this._SplitTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._SplitTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._SplitTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._SplitTLP.Controls.Add(this._Jack1PB, 0, 0);
            this._SplitTLP.Controls.Add(this._Jack4PB, 1, 0);
            this._SplitTLP.Dock = System.Windows.Forms.DockStyle.Top;
            this._SplitTLP.Location = new System.Drawing.Point(0, 0);
            this._SplitTLP.Margin = new System.Windows.Forms.Padding(0);
            this._SplitTLP.Name = "_SplitTLP";
            this._SplitTLP.RowCount = 1;
            this._SplitTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._SplitTLP.Size = new System.Drawing.Size(60, 100);
            this._SplitTLP.TabIndex = 0;
            // 
            // _Jack1PB
            // 
            this._Jack1PB.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this._Jack1PB.Dock = System.Windows.Forms.DockStyle.Fill;
            this._Jack1PB.Location = new System.Drawing.Point(2, 2);
            this._Jack1PB.Margin = new System.Windows.Forms.Padding(2);
            this._Jack1PB.Name = "_Jack1PB";
            this._Jack1PB.Size = new System.Drawing.Size(26, 96);
            this._Jack1PB.TabIndex = 0;
            this._Jack1PB.TabStop = false;
            // 
            // _Jack4PB
            // 
            this._Jack4PB.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this._Jack4PB.Dock = System.Windows.Forms.DockStyle.Fill;
            this._Jack4PB.Location = new System.Drawing.Point(32, 2);
            this._Jack4PB.Margin = new System.Windows.Forms.Padding(2);
            this._Jack4PB.Name = "_Jack4PB";
            this._Jack4PB.Size = new System.Drawing.Size(26, 96);
            this._Jack4PB.TabIndex = 3;
            this._Jack4PB.TabStop = false;
            // 
            // UrrSplitUC
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this._SplitTLP);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UrrSplitUC";
            this.Size = new System.Drawing.Size(60, 60);
            this._SplitTLP.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._Jack1PB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._Jack4PB)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox _Jack1PB;
        private System.Windows.Forms.PictureBox _Jack4PB;
        private System.Windows.Forms.TableLayoutPanel _SplitTLP;
    }
}


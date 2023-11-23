using System.Drawing;
using HMI.Presentation.Twr.UI;

namespace HMI.Presentation.Twr.UI
{
	partial class UpDownButton
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
            System.Windows.Forms.TableLayoutPanel _UpDownButtonTLP;
            //this._LevelBar = new System.Windows.Forms.ProgressBar();
            this._LevelBar = new ColorProgressBar();
            // LALM: 210202 Nueva Progress Bar con colores

            this._DownBT = new HMI.Model.Module.UI.HMIButton();
            this._UpBT = new HMI.Model.Module.UI.HMIButton();
            _UpDownButtonTLP = new System.Windows.Forms.TableLayoutPanel();
            _UpDownButtonTLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // _UpDownButtonTLP
            // 
            _UpDownButtonTLP.ColumnCount = 2;
            _UpDownButtonTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            _UpDownButtonTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            _UpDownButtonTLP.Controls.Add(this._LevelBar, 0, 1);
            _UpDownButtonTLP.Controls.Add(this._DownBT, 0, 0);
            _UpDownButtonTLP.Controls.Add(this._UpBT, 1, 0);
            _UpDownButtonTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            _UpDownButtonTLP.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            _UpDownButtonTLP.Location = new System.Drawing.Point(0, 0);
            _UpDownButtonTLP.Margin = new System.Windows.Forms.Padding(1);
            _UpDownButtonTLP.Name = "_UpDownButtonTLP";
            _UpDownButtonTLP.RowCount = 2;
            _UpDownButtonTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
            _UpDownButtonTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            _UpDownButtonTLP.Size = new System.Drawing.Size(102, 84);
            _UpDownButtonTLP.TabIndex = 0;
            // 
            // _LevelBar
            // 
            this._LevelBar.BackColor = System.Drawing.Color.White;
            _UpDownButtonTLP.SetColumnSpan(this._LevelBar, 2);
            this._LevelBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this._LevelBar.Location = new System.Drawing.Point(0, 63);
            this._LevelBar.Margin = new System.Windows.Forms.Padding(0);
            this._LevelBar.Maximum = 7;
            this._LevelBar.Name = "_LevelBar";
            this._LevelBar.Size = new System.Drawing.Size(102, 21);
            this._LevelBar.Step = 1;
            this._LevelBar.TabIndex = 0;
            // 
            // _DownBT
            // 
            this._DownBT.Dock = System.Windows.Forms.DockStyle.Fill;
            this._DownBT.Location = new System.Drawing.Point(0, 0);
            this._DownBT.Margin = new System.Windows.Forms.Padding(0);
            this._DownBT.Name = "_DownBT";
            this._DownBT.Size = new System.Drawing.Size(51, 63);
            this._DownBT.TabIndex = 1;
            this._DownBT.Click += new System.EventHandler(this._DownBT_Click);
            // 
            // _UpBT
            // 
            this._UpBT.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UpBT.Location = new System.Drawing.Point(51, 0);
            this._UpBT.Margin = new System.Windows.Forms.Padding(0);
            this._UpBT.Name = "_UpBT";
            this._UpBT.Size = new System.Drawing.Size(51, 63);
            this._UpBT.TabIndex = 2;
            this._UpBT.Click += new System.EventHandler(this._UpBT_Click);
            // 
            // UpDownButton
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(_UpDownButtonTLP);
            this.Name = "UpDownButton";
            this.Size = new System.Drawing.Size(102, 84);
            _UpDownButtonTLP.ResumeLayout(false);
            this.ResumeLayout(false);

		}
        public void setColor(Brush brush)
        {
            this._LevelBar.setColor(brush);

        }
        public void setColor(Color color)
        {
            Brush brush = null;
            string name = color.ToString().ToUpper().Split('[')[1].Split(']')[0];
            if (name == "Black".ToUpper()) brush = Brushes.Black;
            else if (name == "Red".ToUpper()) brush = Brushes.Red;
            else if (name == "Green".ToUpper()) brush = Brushes.Green;
            else if (name == "Magenta".ToUpper()) brush = Brushes.Magenta;
            else if (name == "Blue".ToUpper()) brush = Brushes.Blue;
            else if (name == "Cyan".ToUpper()) brush = Brushes.Cyan;
            else brush = Brushes.Green;

            this._LevelBar.setColor(brush);

        }

        #endregion

        //private System.Windows.Forms.ProgressBar _LevelBar;
        private HMI.Presentation.Twr.UI.ColorProgressBar _LevelBar;
        private HMI.Model.Module.UI.HMIButton _DownBT;
		private HMI.Model.Module.UI.HMIButton _UpBT;

	}
}

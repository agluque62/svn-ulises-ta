namespace HMI.Presentation.Asecna.Views
{
	partial class MessageBoxView
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
			this.components = new System.ComponentModel.Container();
			this._MessageViewTLP = new System.Windows.Forms.TableLayoutPanel();
			this._Timer = new System.Windows.Forms.Timer(this.components);
			this._TextLB = new System.Windows.Forms.Label();
			this._TypePB = new System.Windows.Forms.PictureBox();
			this._OkBT = new HMI.Model.Module.UI.HMIButton();
			this._CancelBT = new HMI.Model.Module.UI.HMIButton();
			this._MessageViewTLP.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._TypePB)).BeginInit();
			this.SuspendLayout();
			// 
			// _MessageViewTLP
			// 
			this._MessageViewTLP.ColumnCount = 4;
			this._MessageViewTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this._MessageViewTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this._MessageViewTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this._MessageViewTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this._MessageViewTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this._MessageViewTLP.Controls.Add(this._OkBT, 0, 0);
			this._MessageViewTLP.Controls.Add(this._CancelBT, 2, 0);
			this._MessageViewTLP.Dock = System.Windows.Forms.DockStyle.Bottom;
			this._MessageViewTLP.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
			this._MessageViewTLP.Location = new System.Drawing.Point(0, 64);
			this._MessageViewTLP.Name = "_MessageViewTLP";
			this._MessageViewTLP.RowCount = 1;
			this._MessageViewTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._MessageViewTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this._MessageViewTLP.Size = new System.Drawing.Size(350, 46);
			this._MessageViewTLP.TabIndex = 3;
			// 
			// _Timer
			// 
			this._Timer.Tick += new System.EventHandler(this._Timer_Tick);
			// 
			// _TextLB
			// 
			this._TextLB.Dock = System.Windows.Forms.DockStyle.Fill;
			this._TextLB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._TextLB.Location = new System.Drawing.Point(64, 0);
			this._TextLB.Margin = new System.Windows.Forms.Padding(3);
			this._TextLB.Name = "_TextLB";
			this._TextLB.Size = new System.Drawing.Size(286, 64);
			this._TextLB.TabIndex = 7;
			this._TextLB.Text = "label1";
			this._TextLB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// _TypePB
			// 
			this._TypePB.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this._TypePB.Dock = System.Windows.Forms.DockStyle.Left;
			this._TypePB.Location = new System.Drawing.Point(0, 0);
			this._TypePB.Name = "_TypePB";
			this._TypePB.Size = new System.Drawing.Size(64, 64);
			this._TypePB.TabIndex = 8;
			this._TypePB.TabStop = false;
			// 
			// _OkBT
			// 
			this._MessageViewTLP.SetColumnSpan(this._OkBT, 2);
			this._OkBT.Dock = System.Windows.Forms.DockStyle.Fill;
			this._OkBT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._OkBT.Location = new System.Drawing.Point(3, 3);
			this._OkBT.Name = "_OkBT";
			this._OkBT.Size = new System.Drawing.Size(168, 40);
			this._OkBT.TabIndex = 2;
			this._OkBT.Text = "Aceptar";
			this._OkBT.Click += new System.EventHandler(this._OkBT_Click);
			// 
			// _CancelBT
			// 
			this._MessageViewTLP.SetColumnSpan(this._CancelBT, 2);
			this._CancelBT.Dock = System.Windows.Forms.DockStyle.Fill;
			this._CancelBT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._CancelBT.Location = new System.Drawing.Point(177, 3);
			this._CancelBT.Name = "_CancelBT";
			this._CancelBT.Size = new System.Drawing.Size(170, 40);
			this._CancelBT.TabIndex = 1;
			this._CancelBT.Text = "Cancelar";
			this._CancelBT.Click += new System.EventHandler(this._CancelBT_Click);
			// 
			// MessageBoxView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this._TextLB);
			this.Controls.Add(this._TypePB);
			this.Controls.Add(this._MessageViewTLP);
			this.Name = "MessageBoxView";
			this.Size = new System.Drawing.Size(350, 110);
			this._MessageViewTLP.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._TypePB)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

        private HMI.Model.Module.UI.HMIButton _CancelBT;
		private HMI.Model.Module.UI.HMIButton _OkBT;
		private System.Windows.Forms.TableLayoutPanel _MessageViewTLP;
		private System.Windows.Forms.Timer _Timer;
		private System.Windows.Forms.Label _TextLB;
		private System.Windows.Forms.PictureBox _TypePB;
	}
}

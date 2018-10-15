namespace HMI.Presentation.Asecna.UI
{
	partial class MemUC
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
            this._MemTLP = new System.Windows.Forms.TableLayoutPanel();
            this._OkBT = new HMI.Model.Module.UI.HMIButton();
            this._CancelBT = new HMI.Model.Module.UI.HMIButton();
            this._MemLB = new System.Windows.Forms.ListBox();
            this._MemTLP.SuspendLayout();
            this.SuspendLayout();
            // 
            // _MemTLP
            // 
            this._MemTLP.ColumnCount = 2;
            this._MemTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._MemTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._MemTLP.Controls.Add(this._OkBT, 0, 1);
            this._MemTLP.Controls.Add(this._CancelBT, 1, 1);
            this._MemTLP.Controls.Add(this._MemLB, 0, 0);
            this._MemTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this._MemTLP.Location = new System.Drawing.Point(0, 0);
            this._MemTLP.Name = "_MemTLP";
            this._MemTLP.RowCount = 2;
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._MemTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this._MemTLP.Size = new System.Drawing.Size(288, 344);
            this._MemTLP.TabIndex = 0;
            // 
            // _OkBT
            // 
            this._OkBT.Dock = System.Windows.Forms.DockStyle.Fill;
            this._OkBT.Enabled = false;
            this._OkBT.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._OkBT.Location = new System.Drawing.Point(0, 299);
            this._OkBT.Margin = new System.Windows.Forms.Padding(0, 5, 5, 5);
            this._OkBT.Name = "_OkBT";
            this._OkBT.Size = new System.Drawing.Size(139, 40);
            this._OkBT.TabIndex = 0;
            this._OkBT.Text = "Aceptar";
            
            this._OkBT.Click += new System.EventHandler(this._OkBT_Click);
            // 
            // _CancelBT
            // 
            this._CancelBT.Dock = System.Windows.Forms.DockStyle.Fill;
            this._CancelBT.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._CancelBT.Location = new System.Drawing.Point(149, 299);
            this._CancelBT.Margin = new System.Windows.Forms.Padding(5, 5, 0, 5);
            this._CancelBT.Name = "_CancelBT";
            this._CancelBT.Size = new System.Drawing.Size(139, 40);
            this._CancelBT.TabIndex = 1;
            this._CancelBT.Text = "Cancelar";
            this._CancelBT.Click += new System.EventHandler(this._CancelBT_Click);
            // 
            // _MemLB
            // 
            this._MemTLP.SetColumnSpan(this._MemLB, 2);
            this._MemLB.Dock = System.Windows.Forms.DockStyle.Fill;
            this._MemLB.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._MemLB.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._MemLB.FormattingEnabled = true;
            this._MemLB.ItemHeight = 25;
            this._MemLB.Location = new System.Drawing.Point(3, 3);
            this._MemLB.Name = "_MemLB";
            this._MemLB.Size = new System.Drawing.Size(282, 288);
            this._MemLB.Sorted = true;
            this._MemLB.TabIndex = 2;
            this._MemLB.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this._MemLB_DrawItem);
            this._MemLB.SelectedIndexChanged += new System.EventHandler(this._MemLB_SelectedIndexChanged);
            // 
            // MemUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._MemTLP);
            this.Name = "MemUC";
            this.Size = new System.Drawing.Size(288, 344);
            this._MemTLP.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel _MemTLP;
		private HMI.Model.Module.UI.HMIButton _OkBT;
		private HMI.Model.Module.UI.HMIButton _CancelBT;
		private System.Windows.Forms.ListBox _MemLB;
	}
}

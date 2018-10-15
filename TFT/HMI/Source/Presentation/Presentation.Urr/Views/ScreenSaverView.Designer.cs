namespace HMI.Presentation.Urr.Views
{
    partial class ScreenSaverView
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
            this._LogoPB = new System.Windows.Forms.PictureBox();
            this._RdLB = new System.Windows.Forms.Label();
            this._GlobalLB = new System.Windows.Forms.Label();
            this._TlfLB = new System.Windows.Forms.Label();
            this._Timer = new System.Windows.Forms.Timer(this.components);
            this._version = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._LogoPB)).BeginInit();
            this.SuspendLayout();
            // 
            // _LogoPB
            // 
            this._LogoPB.BackColor = System.Drawing.Color.Black;
            this._LogoPB.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            //this._LogoPB.Location = new System.Drawing.Point(31, 43);//Logo Aena
            this._LogoPB.Location = new System.Drawing.Point(20, 43);//Logo Enaire
            this._LogoPB.Name = "_LogoPB";
            //this._LogoPB.Size = new System.Drawing.Size(270, 194);//Logo Aena
            this._LogoPB.Size = new System.Drawing.Size(350, 52);//Logo Enaire
            this._LogoPB.TabIndex = 0;
            this._LogoPB.TabStop = false;
            // 
            // _RdLB
            // 
            this._RdLB.AutoSize = true;
            this._RdLB.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._RdLB.ForeColor = System.Drawing.Color.RoyalBlue;
            this._RdLB.Location = new System.Drawing.Point(287, 153);
            this._RdLB.Name = "_RdLB";
            this._RdLB.Size = new System.Drawing.Size(438, 39);
            this._RdLB.TabIndex = 1;
            this._RdLB.Text = "Comunicaciones radio Ok";
            // 
            // _GlobalLB
            // 
            this._GlobalLB.AutoSize = true;
            this._GlobalLB.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._GlobalLB.ForeColor = System.Drawing.Color.Lime;
            this._GlobalLB.Location = new System.Drawing.Point(175, 281);
            this._GlobalLB.Name = "_GlobalLB";
            this._GlobalLB.Size = new System.Drawing.Size(346, 39);
            this._GlobalLB.TabIndex = 2;
            this._GlobalLB.Text = "Comunicaciones Ok";
            // 
            // _TlfLB
            // 
            this._TlfLB.AutoSize = true;
            this._TlfLB.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._TlfLB.ForeColor = System.Drawing.Color.RoyalBlue;
            this._TlfLB.Location = new System.Drawing.Point(52, 438);
            this._TlfLB.Name = "_TlfLB";
            this._TlfLB.Size = new System.Drawing.Size(496, 39);
            this._TlfLB.TabIndex = 3;
            this._TlfLB.Text = "Comunicaciones telefonía Ok";
            // 
            // _Timer
            // 
            this._Timer.Tick += new System.EventHandler(this._Timer_Tick);
            // 
            // _version
            // 
            this._version.AutoSize = true;
            this._version.ForeColor = System.Drawing.Color.RoyalBlue;
            this._version.Location = new System.Drawing.Point(749, 573);
            this._version.Name = "_version";
            this._version.Size = new System.Drawing.Size(31, 13);
            this._version.TabIndex = 5;
            this._version.Text = "4.5.0";
            this._version.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // ScreenSaverView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this._version);
            this.Controls.Add(this._TlfLB);
            this.Controls.Add(this._GlobalLB);
            this.Controls.Add(this._RdLB);
            this.Controls.Add(this._LogoPB);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ScreenSaverView";
            this.Size = new System.Drawing.Size(800, 600);
            ((System.ComponentModel.ISupportInitialize)(this._LogoPB)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox _LogoPB;
        private System.Windows.Forms.Label _RdLB;
        private System.Windows.Forms.Label _GlobalLB;
        private System.Windows.Forms.Label _TlfLB;
        private System.Windows.Forms.Timer _Timer;
        private System.Windows.Forms.Label _version;
    }
}

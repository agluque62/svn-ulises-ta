
namespace HMI.Presentation.Twr.UI
{
    partial class ControlRecord
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlRecord));
            this.panel1 = new System.Windows.Forms.Panel();
            this.hmiButtonPlay = new HMI.Model.Module.UI.HMIButton();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.hmiButtonStop = new HMI.Model.Module.UI.HMIButton();
            this.uiTimer1 = new HMI.Model.Module.BusinessEntities.UiTimer();
            this.uiTimer2 = new HMI.Model.Module.BusinessEntities.UiTimer();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiTimer1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiTimer2)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.hmiButtonPlay);
            this.panel1.Controls.Add(this.ProgressBar);
            this.panel1.Controls.Add(this.hmiButtonStop);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(34, 48);
            this.panel1.TabIndex = 3;
            // 
            // hmiButtonPlay
            // 
            this.hmiButtonPlay.DrawX = false;
            this.hmiButtonPlay.ImageDisabled = global::HMI.Presentation.Twr.Properties.Resources.play_no_disponible;
            this.hmiButtonPlay.ImageNormal = ((System.Drawing.Image)(resources.GetObject("hmiButtonPlay.ImageNormal")));
            this.hmiButtonPlay.Location = new System.Drawing.Point(0, 0);
            this.hmiButtonPlay.Margin = new System.Windows.Forms.Padding(0);
            this.hmiButtonPlay.Name = "hmiButtonPlay";
            this.hmiButtonPlay.Permitted = true;
            this.hmiButtonPlay.Size = new System.Drawing.Size(34, 42);
            this.hmiButtonPlay.TabIndex = 1;
            this.hmiButtonPlay.Text = "play";
            this.hmiButtonPlay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.hmiButtonPlay_MouseUp);
            // 
            // ProgressBar
            // 
            this.ProgressBar.Enabled = false;
            this.ProgressBar.Location = new System.Drawing.Point(0, 32);
            this.ProgressBar.Maximum = 30;
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.RightToLeftLayout = true;
            this.ProgressBar.Size = new System.Drawing.Size(33, 16);
            this.ProgressBar.TabIndex = 2;
            this.ProgressBar.Click += new System.EventHandler(this.ProgressBar_Click);
            // 
            // hmiButtonStop
            // 
            this.hmiButtonStop.Dock = System.Windows.Forms.DockStyle.Top;
            this.hmiButtonStop.DrawX = false;
            this.hmiButtonStop.ImageDisabled = global::HMI.Presentation.Twr.Properties.Resources.play_disabled;
            this.hmiButtonStop.ImageNormal = ((System.Drawing.Image)(resources.GetObject("hmiButtonStop.ImageNormal")));
            this.hmiButtonStop.Location = new System.Drawing.Point(0, 0);
            this.hmiButtonStop.Name = "hmiButtonStop";
            this.hmiButtonStop.Permitted = true;
            this.hmiButtonStop.Size = new System.Drawing.Size(34, 32);
            this.hmiButtonStop.TabIndex = 4;
            this.hmiButtonStop.Visible = false;
            this.hmiButtonStop.Click += new System.EventHandler(this.hmiButtonStop_Click);
            this.hmiButtonStop.Leave += new System.EventHandler(this.hmiButtonStop_Leave);
            this.hmiButtonStop.MouseUp += new System.Windows.Forms.MouseEventHandler(this.hmiButtonStop_MouseUp);
            // 
            // uiTimer1
            // 
            this.uiTimer1.Enabled = true;
            this.uiTimer1.Interval = 1000D;
            this.uiTimer1.SynchronizingObject = this;
            this.uiTimer1.Elapsed += new System.Timers.ElapsedEventHandler(this.uiTimer1_Elapsed);
            // 
            // uiTimer2
            // 
            this.uiTimer2.Enabled = true;
            this.uiTimer2.Interval = 10000D;
            this.uiTimer2.SynchronizingObject = this;
            this.uiTimer2.Elapsed += new System.Timers.ElapsedEventHandler(this.uiTimer2_Elapsed);
            // 
            // ControlRecord
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Controls.Add(this.panel1);
            this.Name = "ControlRecord";
            this.Size = new System.Drawing.Size(40, 54);
            this.Leave += new System.EventHandler(this.ControlRecord_Leave);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.uiTimer1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiTimer2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ProgressBar ProgressBar;
        private Model.Module.UI.HMIButton hmiButtonStop;
        private Model.Module.UI.HMIButton hmiButtonPlay;
        public Model.Module.BusinessEntities.UiTimer uiTimer1;
        public Model.Module.BusinessEntities.UiTimer uiTimer2;
    }
}

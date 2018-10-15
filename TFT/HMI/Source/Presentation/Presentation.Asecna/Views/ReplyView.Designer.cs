namespace HMI.Presentation.Asecna.Views
{
    partial class ReplyView
    {
        /// <summary> 
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
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
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            this._BtnClose = new HMI.Model.Module.UI.HMIButton();
            this._BtnErase = new HMI.Model.Module.UI.HMIButton();
            this._BtnPlay = new HMI.Model.Module.UI.HMIButton();
            this._BtnStop = new HMI.Model.Module.UI.HMIButton();
            this._LblReplay = new System.Windows.Forms.Label();
            this._LVSessions = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._PnlViasAudio = new System.Windows.Forms.Panel();
            this._RBAltavozLC = new System.Windows.Forms.RadioButton();
            this._RBAltavozRadio = new System.Windows.Forms.RadioButton();
            this._RBJacksInstructor = new System.Windows.Forms.RadioButton();
            this._RBJacksAlumno = new System.Windows.Forms.RadioButton();
            this._PBFichero = new System.Windows.Forms.ProgressBar();
            this._timerPlaying = new System.Windows.Forms.Timer(this.components);
            this._SlowBlinkTimer = new System.Windows.Forms.Timer(this.components);
            this._FastBlinkTimer = new System.Windows.Forms.Timer(this.components);
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            this._PnlViasAudio.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel1.Controls.Add(this._BtnClose, 1, 8);
            tableLayoutPanel1.Controls.Add(this._BtnErase, 1, 3);
            tableLayoutPanel1.Controls.Add(this._BtnPlay, 1, 7);
            tableLayoutPanel1.Controls.Add(this._BtnStop, 1, 6);
            tableLayoutPanel1.Controls.Add(this._LblReplay, 0, 0);
            tableLayoutPanel1.Controls.Add(this._LVSessions, 0, 2);
            tableLayoutPanel1.Controls.Add(this._PnlViasAudio, 1, 0);
            tableLayoutPanel1.Controls.Add(this._PBFichero, 0, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 9;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.772391F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.54646F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.38302F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.38302F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.38302F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.38302F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.38302F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.38302F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.38302F));
            tableLayoutPanel1.Size = new System.Drawing.Size(440, 462);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // _BtnClose
            // 
            this._BtnClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtnClose.Location = new System.Drawing.Point(355, 407);
            this._BtnClose.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this._BtnClose.Name = "_BtnClose";
            this._BtnClose.Size = new System.Drawing.Size(82, 50);
            this._BtnClose.TabIndex = 1;
            this._BtnClose.Text = "Close";
            this._BtnClose.Click += new System.EventHandler(this._BtnClose_Click);
            // 
            // _BtnErase
            // 
            this._BtnErase.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtnErase.ImageNormal = global::HMI.Presentation.Asecna.Properties.Resources.erase;
            this._BtnErase.Location = new System.Drawing.Point(355, 234);
            this._BtnErase.Name = "_BtnErase";
            this._BtnErase.Size = new System.Drawing.Size(82, 51);
            this._BtnErase.TabIndex = 2;
            this._BtnErase.Click += new System.EventHandler(this._BtnErase_Click);
            // 
            // _BtnPlay
            // 
            this._BtnPlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtnPlay.ImageNormal = global::HMI.Presentation.Asecna.Properties.Resources.play;
            this._BtnPlay.Location = new System.Drawing.Point(355, 348);
            this._BtnPlay.Name = "_BtnPlay";
            this._BtnPlay.Size = new System.Drawing.Size(82, 51);
            this._BtnPlay.TabIndex = 3;
            this._BtnPlay.Click += new System.EventHandler(this._BtnPlay_Click);
            // 
            // _BtnStop
            // 
            this._BtnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this._BtnStop.Enabled = false;
            this._BtnStop.ImageNormal = global::HMI.Presentation.Asecna.Properties.Resources.stop;
            this._BtnStop.Location = new System.Drawing.Point(355, 291);
            this._BtnStop.Name = "_BtnStop";
            this._BtnStop.Size = new System.Drawing.Size(82, 51);
            this._BtnStop.TabIndex = 6;
            this._BtnStop.Click += new System.EventHandler(this._BtnStop_Click);
            // 
            // _LblReplay
            // 
            this._LblReplay.AutoSize = true;
            this._LblReplay.BackColor = System.Drawing.Color.Green;
            this._LblReplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this._LblReplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._LblReplay.Location = new System.Drawing.Point(3, 0);
            this._LblReplay.Name = "_LblReplay";
            this._LblReplay.Size = new System.Drawing.Size(346, 26);
            this._LblReplay.TabIndex = 7;
            this._LblReplay.Text = "Short Term Recorder";
            this._LblReplay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _LVSessions
            // 
            this._LVSessions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this._LVSessions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._LVSessions.FullRowSelect = true;
            this._LVSessions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._LVSessions.HideSelection = false;
            this._LVSessions.Location = new System.Drawing.Point(3, 63);
            this._LVSessions.MultiSelect = false;
            this._LVSessions.Name = "_LVSessions";
            tableLayoutPanel1.SetRowSpan(this._LVSessions, 7);
            this._LVSessions.Size = new System.Drawing.Size(346, 396);
            this._LVSessions.Sorting = System.Windows.Forms.SortOrder.Descending;
            this._LVSessions.TabIndex = 8;
            this._LVSessions.UseCompatibleStateImageBehavior = false;
            this._LVSessions.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Time";
            this.columnHeader1.Width = 115;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Source";
            this.columnHeader2.Width = 80;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Duration";
            this.columnHeader3.Width = 50;
            // 
            // _PnlViasAudio
            // 
            this._PnlViasAudio.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._PnlViasAudio.Controls.Add(this._RBAltavozLC);
            this._PnlViasAudio.Controls.Add(this._RBAltavozRadio);
            this._PnlViasAudio.Controls.Add(this._RBJacksInstructor);
            this._PnlViasAudio.Controls.Add(this._RBJacksAlumno);
            this._PnlViasAudio.Location = new System.Drawing.Point(355, 3);
            this._PnlViasAudio.Name = "_PnlViasAudio";
            tableLayoutPanel1.SetRowSpan(this._PnlViasAudio, 5);
            this._PnlViasAudio.Size = new System.Drawing.Size(82, 215);
            this._PnlViasAudio.TabIndex = 9;
            // 
            // _RBAltavozLC
            // 
            this._RBAltavozLC.AutoSize = true;
            this._RBAltavozLC.Image = global::HMI.Presentation.Asecna.Properties.Resources.AltavozLC;
            this._RBAltavozLC.Location = new System.Drawing.Point(5, 129);
            this._RBAltavozLC.Name = "_RBAltavozLC";
            this._RBAltavozLC.Size = new System.Drawing.Size(46, 32);
            this._RBAltavozLC.TabIndex = 3;
            this._RBAltavozLC.UseVisualStyleBackColor = true;
            // 
            // _RBAltavozRadio
            // 
            this._RBAltavozRadio.AutoSize = true;
            this._RBAltavozRadio.Image = global::HMI.Presentation.Asecna.Properties.Resources.AltavozRadio;
            this._RBAltavozRadio.Location = new System.Drawing.Point(5, 87);
            this._RBAltavozRadio.Name = "_RBAltavozRadio";
            this._RBAltavozRadio.Size = new System.Drawing.Size(46, 32);
            this._RBAltavozRadio.TabIndex = 2;
            this._RBAltavozRadio.UseVisualStyleBackColor = true;
            // 
            // _RBJacksInstructor
            // 
            this._RBJacksInstructor.AutoSize = true;
            this._RBJacksInstructor.Image = global::HMI.Presentation.Asecna.Properties.Resources.CascosInstructor;
            this._RBJacksInstructor.Location = new System.Drawing.Point(5, 45);
            this._RBJacksInstructor.Name = "_RBJacksInstructor";
            this._RBJacksInstructor.Size = new System.Drawing.Size(46, 32);
            this._RBJacksInstructor.TabIndex = 1;
            this._RBJacksInstructor.UseVisualStyleBackColor = true;
            // 
            // _RBJacksAlumno
            // 
            this._RBJacksAlumno.AutoSize = true;
            this._RBJacksAlumno.Checked = true;
            this._RBJacksAlumno.Image = global::HMI.Presentation.Asecna.Properties.Resources.CascosAlumno;
            this._RBJacksAlumno.Location = new System.Drawing.Point(5, 3);
            this._RBJacksAlumno.Name = "_RBJacksAlumno";
            this._RBJacksAlumno.Size = new System.Drawing.Size(46, 32);
            this._RBJacksAlumno.TabIndex = 0;
            this._RBJacksAlumno.TabStop = true;
            this._RBJacksAlumno.Tag = "";
            this._RBJacksAlumno.UseVisualStyleBackColor = true;
            // 
            // _PBFichero
            // 
            this._PBFichero.Dock = System.Windows.Forms.DockStyle.Fill;
            this._PBFichero.ForeColor = System.Drawing.Color.Green;
            this._PBFichero.Location = new System.Drawing.Point(3, 29);
            this._PBFichero.Maximum = 500;
            this._PBFichero.Name = "_PBFichero";
            this._PBFichero.Size = new System.Drawing.Size(346, 28);
            this._PBFichero.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._PBFichero.TabIndex = 10;
            // 
            // _timerPlaying
            // 
            this._timerPlaying.Tick += new System.EventHandler(this.OnTimerPlaying);
            // 
            // _SlowBlinkTimer
            // 
            this._SlowBlinkTimer.Interval = 250;
            this._SlowBlinkTimer.Tick += new System.EventHandler(this._SlowBlinkTimer_Tick);
            // 
            // _FastBlinkTimer
            // 
            this._FastBlinkTimer.Interval = 500;
            this._FastBlinkTimer.Tick += new System.EventHandler(this._FastBlinkTimer_Tick);
            // 
            // ReplyView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(tableLayoutPanel1);
            this.Name = "ReplyView";
            this.Size = new System.Drawing.Size(440, 462);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            this._PnlViasAudio.ResumeLayout(false);
            this._PnlViasAudio.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Model.Module.UI.HMIButton _BtnClose;
        private Model.Module.UI.HMIButton _BtnErase;
        private Model.Module.UI.HMIButton _BtnPlay;
        private Model.Module.UI.HMIButton _BtnStop;
        private System.Windows.Forms.Label _LblReplay;
        private System.Windows.Forms.ListView _LVSessions;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Panel _PnlViasAudio;
        private System.Windows.Forms.RadioButton _RBAltavozLC;
        private System.Windows.Forms.RadioButton _RBAltavozRadio;
        private System.Windows.Forms.RadioButton _RBJacksInstructor;
        private System.Windows.Forms.RadioButton _RBJacksAlumno;
        private System.Windows.Forms.ProgressBar _PBFichero;
        private System.Windows.Forms.Timer _timerPlaying;
        private System.Windows.Forms.Timer _SlowBlinkTimer;
        private System.Windows.Forms.Timer _FastBlinkTimer;
    }
}

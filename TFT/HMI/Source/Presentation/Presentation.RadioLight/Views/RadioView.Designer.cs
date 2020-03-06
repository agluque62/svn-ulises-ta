
//----------------------------------------------------------------------------------------
// patterns & practices - Smart Client Software Factory - Guidance Package
//
// This file was generated by the "Add View" recipe.
//
// For more information see: 
// ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/02-09-010-ModelViewPresenter_MVP.htm
//
// Latest version of this Guidance Package: http://go.microsoft.com/fwlink/?LinkId=62182
//----------------------------------------------------------------------------------------

namespace HMI.Presentation.RadioLight.Views
{
	partial class RadioView
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TableLayoutPanel _RadioTLP;
            System.Windows.Forms.Panel _RadioHeadP;
            this._RtxBT = new HMI.Model.Module.UI.HMIButton();
            this._PttBT = new HMI.Model.Module.UI.HMIButton();
            this._RdHfSpeakerUDB = new HMI.Presentation.RadioLight.UI.UpDownButton();
            this._SiteManagerBT = new HMI.Model.Module.UI.HMIButton();
            this._RdPageBT = new HMI.Presentation.RadioLight.UI.RdPageButton();
            this._RdHeadPhonesUDB = new HMI.Presentation.RadioLight.UI.UpDownButton();
            this._RdSpeakerUDB = new HMI.Presentation.RadioLight.UI.UpDownButton();
            this._RdButtonsTLP = new System.Windows.Forms.TableLayoutPanel();
            this._PttBlinkTimer = new System.Windows.Forms.Timer(this.components);
            this._SquelchBlinkTimer = new System.Windows.Forms.Timer(this.components);
            this._RtxBlinkTimer = new System.Windows.Forms.Timer(this.components);
            _RadioTLP = new System.Windows.Forms.TableLayoutPanel();
            _RadioHeadP = new System.Windows.Forms.Panel();
            _RadioTLP.SuspendLayout();
            _RadioHeadP.SuspendLayout();
            this.SuspendLayout();
            // 
            // _RadioTLP
            // 
            _RadioTLP.ColumnCount = 1;
            _RadioTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _RadioTLP.Controls.Add(_RadioHeadP, 0, 0);
            _RadioTLP.Controls.Add(this._RdButtonsTLP, 0, 1);
            _RadioTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            _RadioTLP.Location = new System.Drawing.Point(0, 0);
            _RadioTLP.Name = "_RadioTLP";
            _RadioTLP.RowCount = 2;
            _RadioTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18F));
            _RadioTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 82F));
            _RadioTLP.Size = new System.Drawing.Size(600, 470);
            _RadioTLP.TabIndex = 0;
            // 
            // _RadioHeadP
            // 
            _RadioHeadP.Controls.Add(this._RtxBT);
            _RadioHeadP.Controls.Add(this._PttBT);
            _RadioHeadP.Controls.Add(this._RdHfSpeakerUDB);
            _RadioHeadP.Controls.Add(this._SiteManagerBT);
            _RadioHeadP.Controls.Add(this._RdPageBT);
            _RadioHeadP.Controls.Add(this._RdHeadPhonesUDB);
            _RadioHeadP.Controls.Add(this._RdSpeakerUDB);
            _RadioHeadP.Dock = System.Windows.Forms.DockStyle.Fill;
            _RadioHeadP.Location = new System.Drawing.Point(3, 3);
            _RadioHeadP.Name = "_RadioHeadP";
            _RadioHeadP.Size = new System.Drawing.Size(594, 78);
            _RadioHeadP.TabIndex = 0;
            // 
            // _RtxBT
            // 
            this._RtxBT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._RtxBT.DrawX = false;
            this._RtxBT.Enabled = false;
            this._RtxBT.Font = new System.Drawing.Font("Arial Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._RtxBT.Location = new System.Drawing.Point(271, 3);
            this._RtxBT.Name = "_RtxBT";
            this._RtxBT.Size = new System.Drawing.Size(50, 72);
            this._RtxBT.TabIndex = 4;
            this._RtxBT.Text = "Rtx";
            this._RtxBT.Visible = false;
            this._RtxBT.Click += new System.EventHandler(this._RtxBT_Click);
            // 
            // _PttBT
            // 
            this._PttBT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._PttBT.DrawX = false;
            this._PttBT.Enabled = false;
            this._PttBT.Font = new System.Drawing.Font("Arial Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._PttBT.Location = new System.Drawing.Point(215, 3);
            this._PttBT.Name = "_PttBT";
            this._PttBT.Size = new System.Drawing.Size(50, 72);
            this._PttBT.TabIndex = 3;
            this._PttBT.Text = "PTT";
            this._PttBT.Visible = false;
            this._PttBT.MouseDown += new System.Windows.Forms.MouseEventHandler(this._PttBT_MouseDown);
            this._PttBT.MouseUp += new System.Windows.Forms.MouseEventHandler(this._PttBT_MouseUp);
            // 
            // _RdHfSpeakerUDB
            // 
            this._RdHfSpeakerUDB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._RdHfSpeakerUDB.BackColor = System.Drawing.Color.Transparent;
            this._RdHfSpeakerUDB.DownImage = global::HMI.Presentation.RadioLight.Properties.Resources.HFSpeakerDown;
            this._RdHfSpeakerUDB.DrawX = false;
            this._RdHfSpeakerUDB.Enabled = false;
            this._RdHfSpeakerUDB.Location = new System.Drawing.Point(185, 3);
            this._RdHfSpeakerUDB.Name = "_RdHfSpeakerUDB";
            this._RdHfSpeakerUDB.Size = new System.Drawing.Size(85, 72);
            this._RdHfSpeakerUDB.TabIndex = 2;
            this._RdHfSpeakerUDB.UpImage = global::HMI.Presentation.RadioLight.Properties.Resources.HFSpeakerUp;
            this._RdHfSpeakerUDB.Visible = false;
            this._RdHfSpeakerUDB.LevelDown += new System.EventHandler(this._HfSpeakerUDB_LevelDown);
            this._RdHfSpeakerUDB.LevelUp += new System.EventHandler(this._HfSpeakerUDB_LevelUp);
            // 
            // _SiteManagerBT
            // 
            this._SiteManagerBT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._SiteManagerBT.DrawX = false;
            this._SiteManagerBT.Font = new System.Drawing.Font("Arial Black", 10F, System.Drawing.FontStyle.Bold);
            this._SiteManagerBT.Location = new System.Drawing.Point(327, 3);
            this._SiteManagerBT.Name = "_SiteManagerBT";
            this._SiteManagerBT.Size = new System.Drawing.Size(50, 72);
            this._SiteManagerBT.TabIndex = 5;
            this._SiteManagerBT.Text = "Empl";
            this._SiteManagerBT.Visible = false;
            this._SiteManagerBT.Click += new System.EventHandler(this._SiteManagerBT_Click);
            // 
            // _RdPageBT
            // 
            this._RdPageBT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._RdPageBT.CornerRadius = 4;
            this._RdPageBT.DownDisabledImage = global::HMI.Presentation.RadioLight.Properties.Resources.RdPageDownDisabled;
            this._RdPageBT.DownEnabledImage = global::HMI.Presentation.RadioLight.Properties.Resources.RdPageDown;
            this._RdPageBT.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._RdPageBT.Location = new System.Drawing.Point(505, 3);
            this._RdPageBT.Name = "_RdPageBT";
            this._RdPageBT.Size = new System.Drawing.Size(86, 72);
            this._RdPageBT.TabIndex = 6;
            this._RdPageBT.UpDisabledImage = global::HMI.Presentation.RadioLight.Properties.Resources.RdPageUpDisabled;
            this._RdPageBT.UpEnabledImage = global::HMI.Presentation.RadioLight.Properties.Resources.RdPageUp;
            this._RdPageBT.UpClick += new Utilities.GenericEventHandler(this._RdPageBT_UpClick);
            this._RdPageBT.DownClick += new Utilities.GenericEventHandler(this._RdPageBT_DownClick);
            // 
            // _RdHeadPhonesUDB
            // 
            this._RdHeadPhonesUDB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._RdHeadPhonesUDB.BackColor = System.Drawing.Color.Transparent;
            this._RdHeadPhonesUDB.DownImage = global::HMI.Presentation.RadioLight.Properties.Resources.HeadPhonesDown;
            this._RdHeadPhonesUDB.DrawX = false;
            this._RdHeadPhonesUDB.Enabled = false;
            this._RdHeadPhonesUDB.Location = new System.Drawing.Point(94, 3);
            this._RdHeadPhonesUDB.Name = "_RdHeadPhonesUDB";
            this._RdHeadPhonesUDB.Size = new System.Drawing.Size(85, 72);
            this._RdHeadPhonesUDB.TabIndex = 1;
            this._RdHeadPhonesUDB.UpImage = global::HMI.Presentation.RadioLight.Properties.Resources.HeadPhonesUp;
            this._RdHeadPhonesUDB.LevelDown += new System.EventHandler(this._RdHeadPhonesUDB_LevelDown);
            this._RdHeadPhonesUDB.LevelUp += new System.EventHandler(this._RdHeadPhonesUDB_LevelUp);
            // 
            // _RdSpeakerUDB
            // 
            this._RdSpeakerUDB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._RdSpeakerUDB.BackColor = System.Drawing.Color.Transparent;
            this._RdSpeakerUDB.DownImage = global::HMI.Presentation.RadioLight.Properties.Resources.SpeakerDown;
            this._RdSpeakerUDB.DrawX = false;
            this._RdSpeakerUDB.Enabled = false;
            this._RdSpeakerUDB.Location = new System.Drawing.Point(3, 3);
            this._RdSpeakerUDB.Name = "_RdSpeakerUDB";
            this._RdSpeakerUDB.Size = new System.Drawing.Size(85, 72);
            this._RdSpeakerUDB.TabIndex = 0;
            this._RdSpeakerUDB.UpImage = global::HMI.Presentation.RadioLight.Properties.Resources.SpeakerUp;
            this._RdSpeakerUDB.LevelDown += new System.EventHandler(this._RdSpeakerUDB_LevelDown);
            this._RdSpeakerUDB.LevelUp += new System.EventHandler(this._RdSpeakerUDB_LevelUp);
            // 
            // _RdButtonsTLP
            // 
            this._RdButtonsTLP.ColumnCount = 5;
            this._RdButtonsTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this._RdButtonsTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this._RdButtonsTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this._RdButtonsTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this._RdButtonsTLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this._RdButtonsTLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this._RdButtonsTLP.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this._RdButtonsTLP.Location = new System.Drawing.Point(3, 87);
            this._RdButtonsTLP.Name = "_RdButtonsTLP";
            this._RdButtonsTLP.RowCount = 3;
            this._RdButtonsTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this._RdButtonsTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this._RdButtonsTLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this._RdButtonsTLP.Size = new System.Drawing.Size(594, 380);
            this._RdButtonsTLP.TabIndex = 1;
            // 
            // _PttBlinkTimer
            // 
            this._PttBlinkTimer.Interval = 500;
            this._PttBlinkTimer.Tick += new System.EventHandler(this._PttBlinkTimer_Tick);
            // 
            // _SquelchBlinkTimer
            // 
            this._SquelchBlinkTimer.Interval = 500;
            this._SquelchBlinkTimer.Tick += new System.EventHandler(this._SquelchBlinkTimer_Tick);
            // 
            // _RtxBlinkTimer
            // 
            this._RtxBlinkTimer.Interval = 500;
            this._RtxBlinkTimer.Tick += new System.EventHandler(this._RtxBlinkTimer_Tick);
            // 
            // RadioView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(_RadioTLP);
            this.Name = "RadioView";
            this.Size = new System.Drawing.Size(600, 470);
            this.BackColorChanged += new System.EventHandler(this.RadioView_BackColorChanged);
            _RadioTLP.ResumeLayout(false);
            _RadioHeadP.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel _RdButtonsTLP;
		private HMI.Model.Module.UI.HMIButton _RtxBT;
		private HMI.Model.Module.UI.HMIButton _PttBT;
		private HMI.Presentation.RadioLight.UI.RdPageButton _RdPageBT;
		private HMI.Presentation.RadioLight.UI.UpDownButton _RdHeadPhonesUDB;
		private HMI.Presentation.RadioLight.UI.UpDownButton _RdSpeakerUDB;
		private System.Windows.Forms.Timer _PttBlinkTimer;
		private System.Windows.Forms.Timer _SquelchBlinkTimer;
		private System.Windows.Forms.Timer _RtxBlinkTimer;
        private Model.Module.UI.HMIButton _SiteManagerBT;
        private UI.UpDownButton _RdHfSpeakerUDB;
        //private System.Windows.Forms.Timer _TxConfirmationDetectionTimer;
        //private System.Windows.Forms.Timer _CarrierDetectionTimer;
	}
}


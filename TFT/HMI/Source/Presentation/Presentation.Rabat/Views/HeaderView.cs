//----------------------------------------------------------------------------------------
// patterns & practices - Smart Client Software Factory - Guidance Package
//
// This file was generated by the "Add View" recipe.
//
// This class is the concrete implementation of a View in the Model-View-Presenter 
// pattern. Communication between the Presenter and this class is acheived through 
// an interface to facilitate separation and testability.
// Note that the Presenter generated by the same recipe, will automatically be created
// by CAB through [CreateNew] and bidirectional references will be added.
//
// For more information see:
// ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/02-09-010-ModelViewPresenter_MVP.htm
//
// Latest version of this Guidance Package: http://go.microsoft.com/fwlink/?LinkId=62182
//----------------------------------------------------------------------------------------

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;
using Microsoft.Practices.CompositeUI.EventBroker;
using Microsoft.Practices.ObjectBuilder;
using HMI.Infrastructure.Interface;
using HMI.Model.Module.Services;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using HMI.Model.Module.UI;
using HMI.Presentation.Rabat.Constants;
using HMI.Presentation.Rabat.Properties;
using NLog;

namespace HMI.Presentation.Rabat.Views
{
	[SmartPart]
	public partial class HeaderView : UserControl
	{
		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private IModelCmdManagerService _CmdManager;
		private StateManagerService _StateManager;
		private List<string> _ConfUnused = new List<string>();
        private bool ReplayOn = false;

		private bool _TitleEnabled
		{
			get { return _StateManager.Tft.Enabled; }
		}
		private bool _SplitEnabled
		{
			get { return _StateManager.Tft.Enabled && _StateManager.Engine.Operative; }
		}
		private bool _InfoEnabled
		{
			get { return _StateManager.Tft.Enabled && !ReplayOn; }
		}
		private bool _BrightnessEnabled
		{
			get { return _StateManager.Tft.Enabled; }
		}
		private bool _BuzzerEnabled
		{
			get { return _StateManager.Tft.Enabled && _StateManager.Engine.Operative; }
		}
        private bool _BriefingEnabled
        {
            get { return _StateManager.Tft.Enabled &&  _StateManager.Engine.Operative; }
        }

        private string _Info // Miguel
        {
            get { return Resources.INF; }
        }

        private static Image CambiarTamanoImagen(Image image, Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }
         
		public HeaderView([ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
		{
			InitializeComponent();
			BackColor = VisualStyle.Colors.HeaderBlue;

			_CmdManager = cmdManager;
			_StateManager = stateManager;

            //_TitleBT.BackgroundImage = _StateManager.Title.Logo;
            _TitleBT.ImageNormal = CambiarTamanoImagen(_StateManager.Title.Logo, new System.Drawing.Size(_StateManager.Title.WidthLogo, _StateManager.Title.HeightLogo));
            _TitleBT.Text = _StateManager.Title.Id;
			_TitleBT.DrawX = !_StateManager.Engine.Operative;
			_TitleBT.ButtonColor = VisualStyle.Colors.White;
			_TitleBT.ButtonColorDisabled = VisualStyle.Colors.Red;
			_TitleBT.Enabled = _TitleEnabled;

			_SplitUC.LeftJackOn = _StateManager.Jacks.LeftJack;
			_SplitUC.RightJackOn = _StateManager.Jacks.RightJack;
			_SplitUC.Mode = _StateManager.Split.Mode;
			_SplitUC.Enabled = _SplitEnabled;

			_InfoBT.Enabled = _InfoEnabled;
            _BriefingBT.Enabled = _BriefingEnabled;

			_BrightnessUDB.Level = _StateManager.Brightness.Level;
			_BrightnessUDB.Enabled = _BrightnessEnabled;

			_BuzzerUDB.Level = _StateManager.Buzzer.Level;
			_BuzzerUDB.Enabled = _BuzzerEnabled;


            _InfoBT.Text = _Info;   // Miguel
		}

		[EventSubscription(EventTopicNames.TftEnabledChanged, ThreadOption.Publisher)]
		[EventSubscription(EventTopicNames.EngineStateChanged, ThreadOption.Publisher)]
		public void OnTftEnabledChanged(object sender, EventArgs e)
		{
			//_TitleBT.Enabled = _TitleEnabled;
			_SplitUC.Enabled = _SplitEnabled;
			_InfoBT.Enabled = _InfoEnabled;
			_BrightnessUDB.Enabled = _BrightnessEnabled;
			_BuzzerUDB.Enabled = _BuzzerEnabled;
			_TitleBT.DrawX = !_StateManager.Engine.Operative;
            _BriefingBT.Enabled = _BriefingEnabled;
		}

		[EventSubscription(EventTopicNames.JacksChanged, ThreadOption.Publisher)]
		public void OnJacksChanged(object sender, EventArgs e)
		{
			Jacks jacks = _StateManager.Jacks;

			_SplitUC.LeftJackOn = jacks.LeftJack;
			_SplitUC.RightJackOn = jacks.RightJack;

			if (!string.IsNullOrEmpty(jacks.PreviusStateDescription))
			{
				_MsgLB.Items.Remove(jacks.PreviusStateDescription);
			}

			if (_StateManager.Engine.Operative)
			{
				if (!string.IsNullOrEmpty(jacks.StateDescription))
				{
					_MsgLB.Items.Add(jacks.StateDescription);
				}
			}
		}

		[EventSubscription(EventTopicNames.TitleIdChanged, ThreadOption.Publisher)]
		public void OnTitleIdChanged(object sender, EventArgs e)
		{
			_TitleBT.Text = _StateManager.Title.Id;
		}

		[EventSubscription(EventTopicNames.SplitShowModeSelectionUI, ThreadOption.Publisher)]
		public void OnSplitShowModeSelection(object sender, EventArgs e)
		{
			_SplitUC.ShowModeSelection();
		}

		[EventSubscription(EventTopicNames.SplitModeChanged, ThreadOption.Publisher)]
		public void OnSplitModeChanged(object sender, EventArgs e)
		{
			_SplitUC.Mode = _StateManager.Split.Mode;
		}

		[EventSubscription(EventTopicNames.BrightnessLevelChanged, ThreadOption.Publisher)]
		public void OnBrightnessLevelChanged(object sender, EventArgs e)
		{
			_BrightnessUDB.Level = _StateManager.Brightness.Level;
		}

		[EventSubscription(EventTopicNames.BuzzerStateChanged, ThreadOption.Publisher)]
		public void OnBuzzerStateChanged(object sender, EventArgs e)
		{
			_BuzzerUDB.DrawX = !_StateManager.Buzzer.Enabled;
		}

		[EventSubscription(EventTopicNames.BuzzerLevelChanged, ThreadOption.Publisher)]
		public void OnBuzzerLevelChanged(object sender, EventArgs e)
		{
			_BuzzerUDB.Level = _StateManager.Buzzer.Level;
		}

		[EventSubscription(EventTopicNames.TlfChanged, ThreadOption.Publisher)]
		public void OnTlfChanged(object sender, RangeMsg e)
		{
			for (int i = e.From, to = e.From + e.Count; i < to; i++)
			{
				TlfDst dst = _StateManager.Tlf[i];

				if (!string.IsNullOrEmpty(dst.PreviusStateDescription))
				{
					if (_MsgLB.Items.Contains(dst.PreviusStateDescription))
					{
						_MsgLB.Items.Remove(dst.PreviusStateDescription);
					}
					else
					{
						_ConfUnused.Remove(dst.PreviusStateDescription);
					}
				}
				if (!string.IsNullOrEmpty(dst.StateDescription))
				{
					if (_MsgLB.Items.Contains(dst.StateDescription))
					{
						_ConfUnused.Add(dst.StateDescription);
					}
					else
					{
						_MsgLB.Items.Add(dst.StateDescription);
					}
				}
			}
		}

		[EventSubscription(EventTopicNames.TlfIntrudedByChanged, ThreadOption.Publisher)]
		public void OnTlfIntrudedByChanged(object sender, EventArgs e)
		{
			IntrudedBy intrudedBy = _StateManager.Tlf.IntrudedBy;

			if (!string.IsNullOrEmpty(intrudedBy.PreviusStateDescription))
			{
				_MsgLB.Items.Remove(intrudedBy.PreviusStateDescription);
			}
			if (!string.IsNullOrEmpty(intrudedBy.StateDescription))
			{
				_MsgLB.Items.Add(intrudedBy.StateDescription);
			}
		}

		[EventSubscription(EventTopicNames.TlfInterruptedByChanged, ThreadOption.Publisher)]
		public void OnTlfInterruptedByChanged(object sender, EventArgs e)
		{
			InterruptedBy interruptedBy = _StateManager.Tlf.InterruptedBy;

			if (!string.IsNullOrEmpty(interruptedBy.PreviusStateDescription))
			{
				_MsgLB.Items.Remove(interruptedBy.PreviusStateDescription);
			}
			if (!string.IsNullOrEmpty(interruptedBy.StateDescription))
			{
				_MsgLB.Items.Add(interruptedBy.StateDescription);
			}
		}

		[EventSubscription(EventTopicNames.TlfIntrudeToChanged, ThreadOption.Publisher)]
		public void OnTlfIntrudeToChanged(object sender, EventArgs e)
		{
			IntrudeTo intrudeTo = _StateManager.Tlf.IntrudeTo;

			if (!string.IsNullOrEmpty(intrudeTo.PreviusStateDescription))
			{
				_MsgLB.Items.Remove(intrudeTo.PreviusStateDescription);
			}
			if (!string.IsNullOrEmpty(intrudeTo.StateDescription))
			{
				_MsgLB.Items.Add(intrudeTo.StateDescription);
			}
		}

		[EventSubscription(EventTopicNames.TlfListenChanged, ThreadOption.Publisher)]
		public void OnTlfListenChanged(object sender, EventArgs e)
		{
			Listen listen = _StateManager.Tlf.Listen;

			if (!string.IsNullOrEmpty(listen.PreviusStateDescription))
			{
				_MsgLB.Items.Remove(listen.PreviusStateDescription);
			}
			if (!string.IsNullOrEmpty(listen.StateDescription))
			{
				_MsgLB.Items.Add(listen.StateDescription);
			}
		}

		[EventSubscription(EventTopicNames.TlfListenByChanged, ThreadOption.Publisher)]
		public void OnTlfListenByChanged(object sender, EventArgs e)
		{
			ListenBy listenBy = _StateManager.Tlf.ListenBy;

			if (!string.IsNullOrEmpty(listenBy.PreviusStateDescription))
			{
				_MsgLB.Items.Remove(listenBy.PreviusStateDescription);
			}
			if (!string.IsNullOrEmpty(listenBy.StateDescription))
			{
				_MsgLB.Items.Add(listenBy.StateDescription);
			}

			_MsgLB.BackColor = listenBy.IsListen ? VisualStyle.Colors.Orange : VisualStyle.Colors.White;
		}

		[EventSubscription(EventTopicNames.TlfConfListChanged, ThreadOption.Publisher)]
		public void OnTlfConfListChanged(object sender, EventArgs e)
		{
			ConfList confList = _StateManager.Tlf.ConfList;

			if (!string.IsNullOrEmpty(confList.PreviusStateDescription))
			{
				if (!_ConfUnused.Remove(confList.PreviusStateDescription))
				{
					_MsgLB.Items.Remove(confList.PreviusStateDescription);
				}
			}
			if (!string.IsNullOrEmpty(confList.StateDescription))
			{
				if (_MsgLB.Items.Contains(confList.StateDescription))
				{
					_ConfUnused.Add(confList.StateDescription);
				}
				else
				{
					_MsgLB.Items.Add(confList.StateDescription);
				}
			}
		}

        [EventSubscription(EventTopicNames.BriefingChanged, ThreadOption.Publisher)]
        public void OnBriefingChanged(object sender, EventArgs e)
        {
            if (_StateManager.Tft.Briefing)
            {
                _MsgLB.BackColor = VisualStyle.Colors.Red;
                _MsgLB.Items.Add(Resources.BriefingSession);
            }
            else
            {
                _MsgLB.BackColor = VisualStyle.Colors.White;
                _MsgLB.Items.Remove(Resources.BriefingSession);
            }
        }

        [EventSubscription(EventTopicNames.ActiveViewChanging, ThreadOption.Publisher)]
        public void OnActiveViewChanging(object sender, EventArgs<string> e)
        {
            ReplayOn = e.Data == ViewNames.Replay;
            _InfoBT.Enabled = !ReplayOn;
        }

		private void _TitleBT_Click(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.DisableTft();
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR deshabilitando el TFT", ex);
			}
		}

		private void _SplitUC_SplitSelectionClick(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.ShowSplitModeSelection();
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR intentando mostrar los posibles modos de Split", ex);
			}
		}

		private void _SplitUC_SplitModeChanging(object sender, SplitMode mode)
		{
			try
			{
				_CmdManager.SetSplitMode(mode);
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR intentando cambiar modo de Split a " + mode, ex);
			}
		}

		private void _InfoBT_Click(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.ShowInfo();
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR mostrando dependencias", ex);
			}
		}

		private void _BrightnessUDB_LevelDown(object sender, EventArgs e)
		{
			int level = _BrightnessUDB.Level - 1;

			try
			{
				_CmdManager.SetBrightnessLevel(level);
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR bajando el nivel de brillo a " + level, ex);
			}
		}

		private void _BrightnessUDB_LevelUp(object sender, EventArgs e)
		{
			int level = _BrightnessUDB.Level + 1;

			try
			{
				_CmdManager.SetBrightnessLevel(level);
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR aumentando el nivel de brillo a " + level, ex);
			}
		}

		private void _BuzzerUDB_LevelDown(object sender, EventArgs e)
		{
			int level = _BuzzerUDB.Level - 1;

			try
			{
				_CmdManager.SetBuzzerLevel(level);
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR bajando el nivel de ring a " + level, ex);
			}
		}

		private void _BuzzerUDB_LevelUp(object sender, EventArgs e)
		{
			int level = _BuzzerUDB.Level + 1;

			try
			{
				_CmdManager.SetBuzzerLevel(level);
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR aumentando el nivel de ring a " + level, ex);
			}
		}

		private void _BuzzerUDB_LongClick(object sender, EventArgs e)
		{
			bool enabled = !_StateManager.Buzzer.Enabled;

			try
			{
				_CmdManager.SetBuzzerState(enabled);
			}
			catch (Exception ex)
			{
				string msg = string.Format("ERROR {0} el ring", enabled ? "habilitando" : "deshabilitando");
				_Logger.Error(msg, ex);
			}
		}

        private void _SplitUC_Load(object sender, EventArgs e)
        {

        }

        private void _timerTimer_Tick(object sender, EventArgs e)
        {
            _tbTime.Text=DateTime.Now.ToString("HH:mm:ss");
            _TbDate.Text=DateTime.Now.ToString("dd/MM/yy");
        }

        private void _BriefingBT_Click(object sender, EventArgs e)
        {
            try
            {
                _CmdManager.BriefingFunction();
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR selecting Briefing function ", ex);
            }
        }

        public IModelCmdManagerService CmdManager
        {
            get
            {
                return _CmdManager;
            }
        }
	}
}


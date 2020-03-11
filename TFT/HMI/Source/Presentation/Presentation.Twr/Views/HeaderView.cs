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
using HMI.Presentation.Twr.Constants;
using HMI.Presentation.Twr.Properties;
using NLog;

namespace HMI.Presentation.Twr.Views
{
	[SmartPart]
	public partial class HeaderView : UserControl
	{
		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private IModelCmdManagerService _CmdManager;
		private StateManagerService _StateManager;
		private List<string> _ConfUnused = new List<string>();

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
			get { return _StateManager.Tft.Enabled && (_StateManager.Tlf.Listen.State == FunctionState.Idle && !_StateManager.Tlf.ListenBy.IsListen); }
		}
		private bool _BrightnessEnabled
		{
			get { return _StateManager.Tft.Enabled && _StateManager.Brightness.Open; }
		}
		private bool _BuzzerEnabled
		{
			get { return _StateManager.Tft.Enabled && _StateManager.Engine.Operative; }
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
            if (global::HMI.Presentation.Twr.Properties.Settings.Default.JackUse != HMI.Presentation.Twr.Constants.JackUse.Both)
            {
                this._SplitUC.Size = new System.Drawing.Size(60, 79);
                this._InfoBT.Location = new System.Drawing.Point(235, 17);
                this._MsgLB.Location = new System.Drawing.Point(295, 3);
                this._MsgLB.Size = new System.Drawing.Size(320, 66);
                this._MsgLB.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            }
            _InfoBT.Enabled = _InfoEnabled;

            //_CmdManager.SetBrightnessLevel(100); 
            _BrightnessUDB.Level = _StateManager.Brightness.Level;
			_BrightnessUDB.Visible = _BrightnessEnabled;

			_BuzzerUDB.Level = _StateManager.Buzzer.Level;
			_BuzzerUDB.Enabled = _BuzzerEnabled;

            _InfoBT.Text = _Info;   // Miguel
		}

        [EventSubscription(EventTopicNames.ProxyPresent, ThreadOption.Publisher)]
        public void OnProxyPresent(object sender, EventArgs e)
        {
            if (Settings.Default.EmergencyModeWarn)
                BackColor = _StateManager.Scv.ProxyState ? VisualStyle.Colors.HeaderBlue : BackColor = System.Drawing.Color.Khaki;
        }

        [EventSubscription(EventTopicNames.TftEnabledChanged, ThreadOption.Publisher)]
		[EventSubscription(EventTopicNames.EngineStateChanged, ThreadOption.Publisher)]
		public void OnTftEnabledChanged(object sender, EventArgs e)
		{
			_TitleBT.Enabled = _TitleEnabled;
			_SplitUC.Enabled = _SplitEnabled;
			_InfoBT.Enabled = _InfoEnabled;
			_BrightnessUDB.Visible = _BrightnessEnabled;
			_BuzzerUDB.Enabled = _BuzzerEnabled;
			_TitleBT.DrawX = !_StateManager.Engine.Operative;
            if (sender.GetType() == typeof(Engine))
            {
                MensajePresenciaAltavoces(typeof(RdSpeaker));
                MensajePresenciaAltavoces(typeof(LcSpeaker));
            }
        }

		[EventSubscription(EventTopicNames.JacksChanged, ThreadOption.Publisher)]
		public void OnJacksChanged(object sender, EventArgs e)
		{
			Jacks jacks = _StateManager.Jacks;

			_SplitUC.LeftJackOn = jacks.LeftJack;
			_SplitUC.RightJackOn = jacks.RightJack;

			if (!string.IsNullOrEmpty(jacks.PreviusStateDescription))
			{
                _MsgLB.Text = _MsgLB.Text.Replace(jacks.PreviusStateDescription, "");
			}

			if (_StateManager.Engine.Operative)
			{
				if (!string.IsNullOrEmpty(jacks.StateDescription))
				{
					_MsgLB.Text += jacks.StateDescription;
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
                    if (_MsgLB.Text.Contains(dst.PreviusStateDescription))
                    {
                        _MsgLB.Text = _MsgLB.Text.Replace(dst.PreviusStateDescription, "");
                    }
                    else
                    {
                        _ConfUnused.Remove(dst.PreviusStateDescription);
                    }
                }
                if (!string.IsNullOrEmpty(dst.StateDescription))
                {
                    if (_MsgLB.Text.Contains(dst.StateDescription))
                    {
                        _ConfUnused.Add(dst.StateDescription);
                    }
                    else
                    {
                        _MsgLB.Text += dst.StateDescription;
                    }
                }
			}
		}

        [EventSubscription(EventTopicNames.TlfForwardChanged, ThreadOption.Publisher)]
        [EventSubscription(EventTopicNames.TlfPickUpChanged, ThreadOption.Publisher)]
        [EventSubscription(EventTopicNames.TlfIntrudeToChanged, ThreadOption.Publisher)]
        [EventSubscription(EventTopicNames.TlfInterruptedByChanged, ThreadOption.Publisher)]
        [EventSubscription(EventTopicNames.TlfIntrudedByChanged, ThreadOption.Publisher)]
        [EventSubscription(EventTopicNames.TlfListenChanged, ThreadOption.Publisher)]
        [EventSubscription(EventTopicNames.TlfListenByChanged, ThreadOption.Publisher)]
        public void OnEntityDescriptionChanged(object sender, EventArgs e)
        {
            Description entity = (Description) sender;

            if (!string.IsNullOrEmpty(entity.PreviusStateDescription))
            {
                _MsgLB.Text=_MsgLB.Text.Replace(entity.PreviusStateDescription, "");
            }
            if (!string.IsNullOrEmpty(entity.StateDescription))
            {
                if (!_MsgLB.Text.Contains(entity.StateDescription))
                    _MsgLB.Text += entity.StateDescription;                    
            }
        }

		[EventSubscription(EventTopicNames.TlfListenChanged, ThreadOption.Publisher)]
		public void OnTlfListenChanged(object sender, EventArgs e)
		{
            _InfoBT.Enabled = _InfoEnabled;
		}

		[EventSubscription(EventTopicNames.TlfListenByChanged, ThreadOption.Publisher)]
		public void OnTlfListenByChanged(object sender, EventArgs e)
		{
            ListenBy listenBy = (ListenBy) sender;
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
                    _MsgLB.Text=_MsgLB.Text.Replace(confList.PreviusStateDescription, "");
                }
            }
            if (!string.IsNullOrEmpty(confList.StateDescription))
            {
                if (_MsgLB.Text.Contains(confList.StateDescription))
                {
                    _ConfUnused.Add(confList.StateDescription);
                }
                else
                {
                    _MsgLB.Text +=confList.StateDescription;
                }
            }
		}

        /// <summary>
        /// Este evento llega cuando hay un cambio en la presencia de un altavoz.
        /// Se utiliza para poner un mensaje 
        /// </summary>
        /// <param name="sender">no se usa</param>
        /// <param name="e">no se usa</param>
        [EventSubscription(EventTopicNames.SpeakerChanged, ThreadOption.Publisher)]
        public void OnSpeakerChanged(object sender, EventArgs e)
        {
            MensajePresenciaAltavoces(sender.GetType());
        }

        private void MensajePresenciaAltavoces(Type tipo)
        {
            if (tipo == typeof(RdSpeaker))
            {
                if (_StateManager.RdSpeaker.Presencia)
                {
                    _MsgLB.Text = _MsgLB.Text.Replace(Resources.SpeakerMissing + Resources.Radio + Environment.NewLine,"");
                }
                else if (_StateManager.Engine.Operative)
                {
                    _MsgLB.Text += Resources.SpeakerMissing + Resources.Radio + Environment.NewLine;
                }
            }
            else if (tipo == typeof(LcSpeaker))
            {
                if (_StateManager.LcSpeaker.Presencia)
                {
                    _MsgLB.Text = _MsgLB.Text.Replace(Resources.SpeakerMissing + Resources.TelephonyLC + Environment.NewLine,"");
                }
                else if (_StateManager.Engine.Operative)
                {
                    _MsgLB.Text += Resources.SpeakerMissing + Resources.TelephonyLC + Environment.NewLine;
                }
            }
            else if (tipo == typeof(HfSpeaker))
            {
                if (_StateManager.HfSpeaker.Presencia)
                {
                    _MsgLB.Text = _MsgLB.Text.Replace(Resources.SpeakerMissing + Resources.RadioAux + Environment.NewLine,"");
                }
                else if ((_StateManager.Engine.Operative) && (_StateManager.Radio.DoubleRadioSpeaker))
                {
                    _MsgLB.Text += Resources.SpeakerMissing + Resources.RadioAux + Environment.NewLine;
                }
            }
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
        public IModelCmdManagerService CmdManager
        {
            get
            {
                return _CmdManager;
            }
        }
    }
}


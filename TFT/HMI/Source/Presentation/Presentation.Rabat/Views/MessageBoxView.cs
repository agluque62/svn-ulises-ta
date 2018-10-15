using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Presentation.Rabat.Properties;
using HMI.Infrastructure.Interface;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Services;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Messages;
using Utilities;
using NLog;

         

namespace HMI.Presentation.Rabat.Views
{
	[SmartPart]
	public partial class MessageBoxView : UserControl
	{
		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private IModelCmdManagerService _CmdManager = null;
		private StateManagerService _StateManager = null;
		private NotifMsg _Message = null;

		public NotifMsg Message
		{
			get { return _Message; }
			set 
			{
				_Timer.Enabled = false;
				_Message = value;

				if (_Message != null)
				{
					_TextLB.Text = _Message.Text;

					_MessageViewTLP.Controls.Remove(_CancelBT);
					_MessageViewTLP.Controls.Remove(_OkBT);

					switch (_Message.Type)
					{
						case MessageType.Error:
							_TypePB.BackgroundImage = Resources.Error;
							break;
						case MessageType.Warning:
							_TypePB.BackgroundImage = Resources.Warning;
							break;
						case MessageType.Processing:
							_TypePB.BackgroundImage = Resources.Wait;
							break;
						default:
							_TypePB.BackgroundImage = Resources.Info;
							break;
					}

					switch (_Message.Buttons)
					{
						case MessageButtons.None:
							_OkBT.Visible = false;
							_CancelBT.Visible = false;
							break;
						case MessageButtons.Ok:
							_MessageViewTLP.Controls.Add(_OkBT, 1, 0);
							_MessageViewTLP.SetColumnSpan(_OkBT, 2);
							_OkBT.Visible = true;
							break;
						case MessageButtons.OkCancel:
							_MessageViewTLP.Controls.Add(_OkBT, 0, 0);
							_MessageViewTLP.SetColumnSpan(_OkBT, 2);
							_MessageViewTLP.Controls.Add(_CancelBT, 2, 0);
							_MessageViewTLP.SetColumnSpan(_CancelBT, 2);
							_OkBT.Visible = true;
							_CancelBT.Visible = true;
							break;
					}

					if (_Message.TimeoutMs > 0)
					{
						_Timer.Interval = _Message.TimeoutMs;
						_Timer.Enabled = true;
					}
				}
			}
		}

        private string _Aceptar // Miguel
        {
            get { return Resources.Aceptar; }
        }

        private string _Cancelar // Miguel
        {
            get { return Resources.Cancelar; }
        }

		public MessageBoxView([ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
		{

            InitializeComponent();

			_CmdManager = cmdManager;
			_StateManager = stateManager;
            
            // Miguel
            _OkBT.Text = _Aceptar;
            _CancelBT.Text = _Cancelar;

		}

		[EventSubscription(EventTopicNames.TftEnabledChanged, ThreadOption.Publisher)]
		public void OnTftEnabledChanged(object sender, EventArgs e)
		{
			_OkBT.Enabled = _StateManager.Tft.Enabled;
			_CancelBT.Enabled = _StateManager.Tft.Enabled;
		}

		private void _OkBT_Click(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.MessageResponse(_Message, NotifMsgResponse.Ok);
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR respondiendo Ok al mensaje " + _Message.Text, ex);
			}
		}


		private void _CancelBT_Click(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.MessageResponse(_Message, NotifMsgResponse.Cancel);
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR respondiendo Cancel al mensaje " + _Message.Text, ex);
			}
		}

		private void _Timer_Tick(object sender, EventArgs e)
		{
			_Timer.Enabled = false;

			try
			{
				_CmdManager.MessageResponse(_Message, NotifMsgResponse.Timeout);
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR respondiendo Timeout al mensaje " + _Message.Text, ex);
			}
		}
      
	}
}

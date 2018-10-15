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
using HMI.Infrastructure.Interface;
using HMI.Model.Module.Services;
using HMI.Presentation.Asecna.Properties;
using HMI.Presentation.Asecna.Constants;
using NLog;

namespace HMI.Presentation.Asecna.Views
{
	[SmartPart]
	public partial class ScreenSaverView : UserControl
	{
		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private StateManagerService _StateManager;
		private int _RdOffset = -5;
		private int _GlobalOffset = -5;
		private int _TlfOffset = -5;

		private string _RdText
		{
			get { return _StateManager.Engine.Operative ? Resources.RdOk : Resources.RdNoOk; }
		}
		private string _TlfText
		{
			get { return _StateManager.Engine.Operative ? Resources.TlfOk : Resources.TlfNoOk; }
		}
		private string _GlobalText
		{
			get
			{
				return !_StateManager.Engine.Connected ? Resources.GlobalDisconnected : 
					_StateManager.Engine.Isolated ? Resources.GlobalIsolated : Resources.GlobalOk;
			}
		}
		private Color _GlobalColor
		{
			get { return _StateManager.Engine.Operative ? Color.Lime : Color.Red; }
		}

		public ScreenSaverView([ServiceDependency] StateManagerService stateManager)
		{
			InitializeComponent();

			_StateManager = stateManager;

			_LogoPB.BackgroundImage = _StateManager.Title.Logo;
			_RdLB.Text = _RdText;
			_GlobalLB.Text = _GlobalText;
			_TlfLB.Text = _TlfText;
			_GlobalLB.ForeColor = _GlobalColor;
		}

		[EventSubscription(EventTopicNames.EngineStateChanged, ThreadOption.Publisher)]
		public void OnEngineStateChanged(object sender, EventArgs e)
		{
			_RdLB.Text = _RdText;
			_GlobalLB.Text = _GlobalText;
			_TlfLB.Text = _TlfText;
			_GlobalLB.ForeColor = _GlobalColor;
		}

		[EventSubscription(EventTopicNames.ActiveViewChanging, ThreadOption.Publisher)]
		public void OnActiveViewChanging(object sender, EventArgs<string> e)
		{
			_Timer.Enabled = e.Data == ViewNames.ScreenSaver;
		}

		private void _Timer_Tick(object sender, EventArgs e)
		{
			try
			{
				if ((_RdLB.Location.X < 30) || (_RdLB.Location.X + _RdLB.Width > Width - 30))
				{
					_RdOffset *= -1;
				}
				if ((_GlobalLB.Location.X < 30) || (_GlobalLB.Location.X + _GlobalLB.Width > Width - 30))
				{
					_GlobalOffset *= -1;
				}
				if ((_TlfLB.Location.X < 30) || (_TlfLB.Location.X + _TlfLB.Width > Width - 30))
				{
					_TlfOffset *= -1;
				}

				_RdLB.Location = new Point(_RdLB.Location.X + _RdOffset, _RdLB.Location.Y);
				_GlobalLB.Location = new Point(_GlobalLB.Location.X + _GlobalOffset, _GlobalLB.Location.Y);
				_TlfLB.Location = new Point(_TlfLB.Location.X + _TlfOffset, _TlfLB.Location.Y);
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR modificando posicion de mensajes en el salvapantallas", ex);
			}
		}
	}
}

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;
using HMI.Model.Module.Services;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using Utilities;

namespace HMI.Model.Module.UI
{
	[SmartPart]
	public partial class Keypad : UserControl
	{
		public event GenericEventHandler<char> NewKey;
		public event GenericEventHandler ClearClick;
        public event GenericEventHandler<bool> ChgMode;//230804

        private const string _Exit = "#13790*";
		private const string _Reset = "#13791*";
		private const string _EngineInfo = "#13792*";
		private const string _StateInfo = "#13793*";
		private const string _ModoDiurno = "#13707*";
		private const string _ModoNocturno = "#13723*";
        private const string _Pause = ",";

		private StateManagerService _StateManager;
		private string _Dst = "";
		private string _Digits = "";

		[Browsable(false),
		DefaultValue("")
		]
		public string Display
		{
			get { return _DisplayTB.Text; }
		}

		[Browsable(false),
		DefaultValue("")
		]
		public string Dst
		{
			get { return _Dst; }
			set 
			{ 
				_Dst = value;
				_DisplayTB.Text = _Dst;
				_DisplayTB.SelectionStart = _DisplayTB.Text.Length;
				_DisplayTB.ScrollToCaret();

				_ClearBT.Enabled = Enabled && (_Dst.Length == 0) && (_Digits.Length > 0) ? true : false;
			}
		}

		[Browsable(false),
		DefaultValue("")
		]
		public string Digits
		{
			get { return _Digits; }
			set 
			{ 
				_Digits = value;
				_DisplayTB.Text = _Dst + _Digits;
				_DisplayTB.SelectionStart = _DisplayTB.Text.Length;
				_DisplayTB.ScrollToCaret();

				_ClearBT.Enabled = Enabled && (_Dst.Length == 0) && (_Digits.Length > 0) ? true : false;
			}
		}

		public new bool Enabled
		{
			get { return _DisplayTB.Enabled; }
			set
			{
				if (value != _DisplayTB.Enabled)
				{
					_DisplayTB.Enabled = value;

					BtnState st = value ? BtnState.Normal : BtnState.Inactive;

					_0BT.Enabled = value;
					_1BT.Enabled = value;
					_2BT.Enabled = value;
					_3BT.Enabled = value;
					_4BT.Enabled = value;
					_5BT.Enabled = value;
					_6BT.Enabled = value;
					_7BT.Enabled = value;
					_8BT.Enabled = value;
					_9BT.Enabled = value;
					_AlmBT.Enabled = value;
					_AstBT.Enabled = value;
                    _PauseBt.Enabled = value;
					_ClearBT.Enabled = value && (_Dst.Length == 0) && (_Digits.Length > 0) ? true : false;
				}
			}
		}

		public Keypad([ServiceDependency] StateManagerService stateManager)
		{
			InitializeComponent();

			_StateManager = stateManager;
		}

		private void _BT_Click(object sender, EventArgs e)
		{
			string text = ((HMIButton)sender).Text;
			Digits = _Digits + text[0];

			General.SafeLaunchEvent(NewKey, this, text[0]);

			if (Display == _Exit)
			{
				Application.Exit();
			}
			else if (Display == _Reset)
			{
				Process.Start("Launcher.exe", "HMI.exe");
			}
			else if (Display == _EngineInfo)
			{
				ToolStripItem item = ParentForm.ContextMenuStrip.Items["EngineInfo"];

                //Assembly.GetExecutingAssembly().GetName().Version.ToString()

				if (item != null)
				{
					item.PerformClick();
				}
			}
			else if (Display == _StateInfo)
			{
				string str = string.Format("Unavailable = {0}, Idle = {1}, PaPBusy = {2}{3}", 
					_StateManager.Tlf[TlfState.Unavailable], _StateManager.Tlf[TlfState.Idle], _StateManager.Tlf[TlfState.PaPBusy], Environment.NewLine);
				str += string.Format("RemoteMem = {0}, Mem = {1}, Hold = {2}{3}", 
					_StateManager.Tlf[TlfState.RemoteMem], _StateManager.Tlf[TlfState.Mem], _StateManager.Tlf[TlfState.Hold], Environment.NewLine);
				str += string.Format("RemoteHold = {0}, Out = {1}, Set = {2}{3}", 
					_StateManager.Tlf[TlfState.RemoteHold], _StateManager.Tlf[TlfState.Out], _StateManager.Tlf[TlfState.Set], Environment.NewLine);
				str += string.Format("Conf = {0}, Congestion = {1}, Busy = {2}{3}", 
					_StateManager.Tlf[TlfState.Conf], _StateManager.Tlf[TlfState.Congestion], _StateManager.Tlf[TlfState.Busy], Environment.NewLine);
				str += string.Format("RemoteIn = {0}, In = {1}, InPrio = {2}{3}",
					_StateManager.Tlf[TlfState.RemoteIn], _StateManager.Tlf[TlfState.In], _StateManager.Tlf[TlfState.InPrio], Environment.NewLine);
				str += string.Format("NotAllowed = {0}, Inactive = {1}",
                    _StateManager.Tlf[TlfState.NotAllowed], _StateManager.Tlf[TlfState.Inactive], Environment.NewLine);

				NotifMsg msg = new NotifMsg("StateInfo", "Estado", str, 0, MessageType.Information, MessageButtons.Ok);
				_StateManager.ShowUIMessage(msg);
			}
            else if (Display == _ModoNocturno)
            {
                BtChgMode(true);//230804
            }
            else if (Display == _ModoDiurno)
            {
                BtChgMode(false);//230804
            }
            else if (Display == "***")
            {
                BtChgMode(true);//230804
            }
            else if (Display == "###")
            {
                BtChgMode(false);//230804
            }

        }

        private void _ClearBT_Click(object sender, EventArgs e)
		{
			if (_Digits.Length > 0)
			{
				Digits = _Digits.Substring(0, _Digits.Length - 1);
				General.SafeLaunchEvent(ClearClick, this);
			}
		}

		private void _ClearBT_LongClick(object sender, EventArgs e)
		{
			if (_Digits.Length > 0)
			{
				Digits = "";
				General.SafeLaunchEvent(ClearClick, this);
			}
		}

        private void _PauseBt_Click(object sender, EventArgs e)
        {
            Digits = _Digits + _Pause;

            General.SafeLaunchEvent(NewKey, this, _Pause[0]);
        }
        private void BtChgMode(bool modo)
        {
            General.SafeLaunchEvent(ChgMode, this, modo);
        }
    }
}

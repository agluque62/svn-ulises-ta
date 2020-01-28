using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;
using NLog;

namespace HMI.CD40.Module.BusinessEntities
{
#if DEBUG
    public class TlfListen
#else
	class TlfListen
#endif
	{
		public event GenericEventHandler<ListenPickUpMsg> ListenChanged;
		public event GenericEventHandler<ListenPickUpMsg> RemoteListenChanged;
		public event GenericEventHandler<SnmpStringMsg<string, string>> SetSnmpString;

		public FunctionState State
		{
			get { return _State; }
			private set
			{
				if (value != _State)
				{
					_State = value;

					if (_State == FunctionState.Executing)
					{
						Debug.Assert(_To != null);
						General.SafeLaunchEvent(ListenChanged, this, new ListenPickUpMsg(_State, _To.Literal));
					}
					else
					{
						General.SafeLaunchEvent(ListenChanged, this, new ListenPickUpMsg(_State));
					}
				}
			}
		}

		public TlfListen()
		{
			Top.Cfg.ConfigChanged += OnConfigChanged;

			Top.Sip.IncomingMonitoringCall += OnIncomingMonitoringCall;
			Top.Sip.MonitoringCallStateChanged += OnMonitoringCallStateChanged;
		}

		public void To(int id)
		{
			if (_State == FunctionState.Idle)
			{
				Debug.Assert(id < Tlf.NumDestinations);

				TlfPosition to = Top.Tlf[id];
				FunctionState st = FunctionState.Error;

				if (to.IsTop)
				{
					_To = new TlfIaPosition(to);
					_To.Listen();

					if (_To.State == TlfState.Out)
					{
						_To.TlfPosStateChanged += OnToMonitoringCallStateChanged;
						st = FunctionState.Executing;

						Top.WorkingThread.Enqueue("SetSnmp", delegate()
						{
							string snmpString = Top.Cfg.PositionId + "_" + "LISTEN" + "_" + _To.Literal;
							General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.TlfFacilityOid, snmpString));
						});
					}
				}

				State = st;
			}
		}

		public void To(uint prefix, string dst, string number)
		{
			if (_State == FunctionState.Idle)
			{
				FunctionState st = FunctionState.Error;

				if (prefix == Cd40Cfg.INT_DST)
				{
					_To = new TlfIaPosition(prefix, dst);
					_To.Listen();

					if (_To.State == TlfState.Out)
					{
						_To.TlfPosStateChanged += OnToMonitoringCallStateChanged;
						st = FunctionState.Executing;

						Top.WorkingThread.Enqueue("SetSnmp", delegate()
						{
							string snmpString = Top.Cfg.PositionId + "_" + "LISTEN" + "_" + _To.Number;
							General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.TlfFacilityOid, snmpString));
						});
					}
				}

				State = st;
			}
		}

		public void Accept(int call)
		{
			TlfIaPosition from;

			if (_Froms.TryGetValue(call, out from))
			{
				from.Accept(null);
			}
		}

		public void Cancel(int call)
		{
			if (call == -1)
			{
				if (_To != null)
				{
					_To.TlfPosStateChanged -= OnToMonitoringCallStateChanged;
					_To.HangUp(0);
					_To.Dispose();
					_To = null;
				}

				State = FunctionState.Idle;
			}
			else
			{
				TlfIaPosition from;

				if (_Froms.TryGetValue(call, out from))
				{
					from.TlfPosStateChanged -= OnFromMonitoringCallStateChanged;
					from.Reject(SipAgent.SIP_NOT_ACCEPTABLE_HERE);
					from.Dispose();

					_Froms.Remove(call);
				}
			}
		}

		#region Private Members

		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private FunctionState _State = FunctionState.Idle;
		private TlfIaPosition _To = null;
		private Dictionary<int, TlfIaPosition> _Froms = new Dictionary<int, TlfIaPosition>();

		private void OnConfigChanged(object sender)
		{
			if (_To != null)
			{
				_To.Update();
			}

			foreach (TlfIaPosition from in _Froms.Values)
			{
				from.Update();
			}
		}

		private void OnIncomingMonitoringCall(object sender, int call, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo, CORESIP_Answer answer)
		{
			Debug.Assert(!_Froms.ContainsKey(call));
			TlfIaPosition from = new TlfIaPosition(1000);

			int code = from.HandleIncomingCall(call, -1, info, inInfo);
			Debug.Assert(code == SipAgent.SIP_RINGING);

			if (!from.IsTop)
			{
				answer.Value = SipAgent.SIP_NOT_ACCEPTABLE_HERE;
				return;
			}

			answer.Value = 0;
			from.TlfPosStateChanged += OnFromMonitoringCallStateChanged;
			_Froms[call] = from;

			General.SafeLaunchEvent(RemoteListenChanged, this, new ListenPickUpMsg(FunctionState.Executing, from.Literal, call));
		}

		private void OnMonitoringCallStateChanged(object sender, int call, CORESIP_CallStateInfo info)
		{
			if ((_To == null) || !_To.HandleChangeInCallState(call, info))
			{
				TlfIaPosition from;

				if (_Froms.TryGetValue(call, out from))
				{
					bool handled = from.HandleChangeInCallState(call, info);
					Debug.Assert(handled);
				}
			}
		}

		private void OnToMonitoringCallStateChanged(object sender)
		{
			if (_To.CallId == -1)
			{
				State = FunctionState.Error;
			}
		}

		private void OnFromMonitoringCallStateChanged(object sender)
		{
			TlfIaPosition from = (TlfIaPosition)sender;

			if (from.CallId == -1)
			{
				foreach (KeyValuePair<int, TlfIaPosition> p in _Froms)
				{
					if (p.Value == from)
					{
						from.TlfPosStateChanged -= OnFromMonitoringCallStateChanged;
						from.Dispose();

						_Froms.Remove(p.Key);
						General.SafeLaunchEvent(RemoteListenChanged, this, new ListenPickUpMsg(FunctionState.Idle, from.Literal, p.Key));

						break;
					}
				}

			}
		}

		#endregion
	}
}

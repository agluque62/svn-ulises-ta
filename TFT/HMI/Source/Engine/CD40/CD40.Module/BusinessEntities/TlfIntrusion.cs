using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using HMI.Model.Module.BusinessEntities;
using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;
using NLog;

namespace HMI.CD40.Module.BusinessEntities
{
	class TlfIntrusion : IDisposable
	{
		public event GenericEventHandler Finished;
		// public event GenericEventHandler<HMI.Model.Module.Messages.StateMsg<string>> CompletedIntrusion;
        public static String INTRUSION_IN_PROGRESS = "Intrusion in progress";
        public static String INTRUSION_COMPLETED = "Intrusion completed";
        public static String INTERRUPTION_IMPENDING = "Interruption is impending";
        public static String INTERRUPTION_TERMINATED = "Interruption Terminated";

		public TlfIntrusion(TlfPosition intruderCall)
		{
			Top.Tlf.ActivityChanged += OnActivityChanged;

			_State = IntrusionState.Queued;
			_AssociateCall = intruderCall;
			_AssociateCall.StateChanged += OnAssociateCallStateChanged;

			_IntrusionTout.AutoReset = false;
			_IntrusionTout.Elapsed += OnIntrusionTimeout;
			_IntrusionTout.Interval = Math.Max(1, Settings.Default.IntrusionTout);
			_IntrusionTout.Enabled = true;
		}

		public void Dispose()
		{
			if (_State == IntrusionState.Queued)
			{
				_IntrusionTout.Enabled = false;
				Top.Tlf.ActivityChanged -= OnActivityChanged;
				_AssociateCall.StateChanged -= OnAssociateCallStateChanged;
			}
			else if (_State == IntrusionState.InProgress)
			{
				if (_Conference != null)
				{
					Debug.Assert(_OwnConference);

					_Conference.MemberAdded -= OnMemberAdded;
					_Conference.Finished -= OnFinished;
					_Conference.Dispose();
					_Conference = null;
				}
				_AssociateCall.StateChanged -= OnAssociateCallStateChanged;
			}
			else if (_State == IntrusionState.On)
			{
				if (_Conference != null)
				{
					_Conference.MemberAdded -= OnMemberAdded;
					_Conference.Finished -= OnFinished;

					if (_OwnConference)
					{
						_Conference.Dispose();
					}
					_Conference = null;
				}
			}

			_State = IntrusionState.Off;
			General.SafeLaunchEvent(Finished, this);
		}

		#region Private Members

		enum IntrusionState { Off, Queued, InProgress, On }

		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private IntrusionState _State = IntrusionState.Off;
		private TlfPosition _AssociateCall = null;
		private Timer _IntrusionTout = new Timer();
		private TlfConference _Conference = null;
		private bool _OwnConference = false;

		private void OnActivityChanged(object sender)
		{
			Debug.Assert(_State == IntrusionState.Queued);

            _Logger.Debug("Intrusion activity: {0}", _State);

			if (!Top.Tlf.Activity())
			{
				Dispose();

				if (Settings.Default.TlfInPrioAutoAnswer && 
					((Top.Cfg.Permissions & Permissions.Intruded) == Permissions.Intruded))
				{
					Top.Tlf.Accept(_AssociateCall.Pos, null);
				}
			}
		}

		private void OnAssociateCallStateChanged(object sender)
		{
			Debug.Assert((_State == IntrusionState.Queued) || (_State == IntrusionState.InProgress));
			Debug.Assert(sender == _AssociateCall);

			switch (_AssociateCall.State)
			{
				case TlfState.Unavailable:
				case TlfState.Idle:
				case TlfState.PaPBusy:
					Dispose();
					break;
			}
		}

		private void OnIntrusionTimeout(object sender, ElapsedEventArgs e)
		{
			Top.WorkingThread.Enqueue("OnIntrusionTimeout", delegate()
			{
				if (_State == IntrusionState.Queued)
				{
					if (!MakeIntrusion())
					{
						Dispose();
					}
				}
			});
		}

		private void OnMemberAdded(object sender, TlfPosition tlf)
		{
			if (_State == IntrusionState.InProgress)
			{
				if (!CompleteIntrusion())
				{
					Dispose();
				}
			}
		}

		private void OnFinished(object sender)
		{
			if ((_State == IntrusionState.On) && (_AssociateCall.CallId >= 0))
			{
                SipAgent.SendInfo(_AssociateCall.CallId, INTRUSION_COMPLETED);
			}

			_Conference = null;
			Dispose();
		}

		private bool MakeIntrusion()
		{
			List<TlfPosition> activeCalls = Top.Tlf.ActiveCalls;
			Debug.Assert(activeCalls.Count == 1);

			if ((Top.Cfg.Permissions & Permissions.Intruded) != Permissions.Intruded)
			{
				return false;
			}

			foreach (TlfPosition tlf in activeCalls)
			{
				if (tlf.EfectivePriority == CORESIP_Priority.CORESIP_PR_EMERGENCY)
				{
					return false;
				}

				switch (tlf.State)
				{
					case TlfState.In:			// Ocurre cuando hemos aceptado la llamada pero todavía no tenemos ack
					case TlfState.RemoteIn:
					case TlfState.InPrio:
					case TlfState.Out:
					case TlfState.Congestion:
					case TlfState.OutOfService:
					case TlfState.Busy:
						return false;
				}
			}

            TlfPosition actTlf;
            //Protección para un caso visto en pruebas
            if (activeCalls.Count > 0)
            {
                actTlf = activeCalls[0];
            }
            else
            {
                _Logger.Error("ERROR intruyendo, no active call");
                return false;
            }

			_Conference = new TlfConference();
			_Conference.MemberAdded += OnMemberAdded;
			_Conference.Finished += OnFinished;
			_OwnConference = true;

			try
			{
				_Conference.TryAddExisting(actTlf);

				_State = IntrusionState.InProgress;
				Top.Tlf.ActivityChanged -= OnActivityChanged;
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR intruyendo " + actTlf.Literal, ex);
				return false;
			}

			return true;
		}

		private bool CompleteIntrusion()
		{
			Debug.Assert(_State != IntrusionState.Off);
			Debug.Assert(_AssociateCall != null);
			Debug.Assert(_AssociateCall.State == TlfState.InPrio);
			Debug.Assert(_Conference != null);

			try
			{
				SipAgent.AnswerCall(_AssociateCall.CallId, SipAgent.SIP_INTRUSION_IN_PROGRESS);
                Top.Tlf.setShortTone(1000, "Warning_Operator_Intervening.wav");
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR notificando intrusion en progreso a llamada prioritaria " + _AssociateCall.Literal, ex);
			}

			try
			{
				_Conference.TryAddIncoming(_AssociateCall);
				_State = IntrusionState.On;
				_AssociateCall.StateChanged -= OnAssociateCallStateChanged;
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR aceptando llamada prioritaria " + _AssociateCall.Literal, ex);
				return false;
			}

			foreach (TlfPosition tlf in _Conference.Members)
			{
                SipAgent.SendInfo(tlf.CallId, INTRUSION_IN_PROGRESS);
			}

			return true;
		}

		#endregion
	}
}

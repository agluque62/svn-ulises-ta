using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;
using NLog;

namespace HMI.CD40.Module.BusinessEntities
{
#if DEBUG
    public class TlfTransfer
#else
	class TlfTransfer
#endif
    {
		public event GenericEventHandler StateChanged;
		public event GenericEventHandler<SnmpStringMsg<string, string>> SetSnmpString;

		public FunctionState State
		{
			get { return _State; }
			private set
			{
				if (value != _State)
				{
					_State = value;
					General.SafeLaunchEvent(StateChanged, this);
				}
			}
		}

		public TlfPosition AssociateCall
		{
			get { return _AssociateCall; }
			private set
			{
				if (_AssociateCall != null)
				{
					_AssociateCall.TlfPosStateChanged -= OnAssociateCallStateChanged;
					_AssociateCall = null;
				}

				if (value != null)
				{
					_AssociateCall = value;
					_AssociateCall.TlfPosStateChanged += OnAssociateCallStateChanged;
				}
			}
		}

		public void Cancel()
		{
			if (_State == FunctionState.Executing)
			{
				AssociateCall = null;
				Top.Sip.TlfTransferStatus -= OnTransferStatus;
			}

			State = FunctionState.Idle;
		}

		public void To(int id)
		{
            _FirstTransferTryKO = false;
            _ToTransfer = null;
            _FromTransferDisplayName = "";
            if (_State == FunctionState.Idle)
			{
				List<TlfPosition> activeCalls = Top.Tlf.ActiveCalls;
				FunctionState st = FunctionState.Error;

				if (activeCalls.Count == 1)
				{

                    if ((activeCalls[0].State == TlfState.Set) || (activeCalls[0].State == TlfState.Conf))
					{
                        TlfPosition tlf = activeCalls[0];
                        TlfPosition to = Top.Tlf[id];

                        bool transferDone = false;
                        if (Top.Tlf[id].State == TlfState.Idle)
						{
                            string toUri = to.Uri;
                            if (TlfManager.GetDisplayName(to.Uri) == null && to.Literal.Length > 0)
                            {
                                //Si to.Uri no tiene display name se añade el Literal como display name en la transferencia directa
                                toUri = "\"" + to.Literal + "\" " + to.Uri;
                            }
                            SipAgent.TransferCall(tlf.CallId, -1, toUri, null);

                            transferDone = true;                            
						}
                        else if (Top.Tlf[id].State == TlfState.Hold)
						{
                            SipAgent.HoldCall(activeCalls[0].CallId);
                            System.Threading.Thread.Sleep(50);
                            SipAgent.TransferCall(tlf.CallId, to.CallId, null, "\"" + to.Literal + "\"");
                            transferDone = true;
                            _ToTransfer = to;
                            _FromTransferDisplayName = "\"" + tlf.Literal + "\"";
                        }
                        if (transferDone)
                        {
                            _Logger.Debug("Iniciando transferencia...");
                            Top.Sip.TlfTransferStatus += OnTransferStatus;
                            AssociateCall = tlf;

                            st = FunctionState.Executing;

                            Top.WorkingThread.Enqueue("SetSnmp", delegate()
                            {
                                string snmpString = Top.Cfg.PositionId + "_" + "TRANSFER" + "_" + tlf.Literal;
                                General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.TlfFacilityOid, snmpString));
                            });
                        }
					}
				}

				State = st;
			}
		}

		public void To(uint prefix, string dst, string number)
		{
            _FirstTransferTryKO = false;
            _ToTransfer = null;
            _FromTransferDisplayName = "";
            if (_State == FunctionState.Idle)
			{
				List<TlfPosition> activeCalls = Top.Tlf.ActiveCalls;
				FunctionState st = FunctionState.Error;

				if (activeCalls.Count == 1)
				{
					TlfPosition tlf = activeCalls[0];

					if ((tlf.State == TlfState.Set) || (tlf.State == TlfState.Conf))
					{
						using (TlfIaPosition to = new TlfIaPosition(prefix, dst))
						{
                            string toUri = to.Uri;
                            if (TlfManager.GetDisplayName(to.Uri) == null && to.Literal.Length > 0)
                            {
                                //Si to.uri no tiene display name se añade el Literal como display name en la transferencia directa
                                toUri = "\"" + to.Literal + "\" " + to.Uri;
                            }
                            SipAgent.TransferCall(tlf.CallId, -1, toUri, null);

							Top.Sip.TlfTransferStatus += OnTransferStatus;

							AssociateCall = tlf;
							st = FunctionState.Executing;

							Top.WorkingThread.Enqueue("SetSnmp", delegate()
							{
								string snmpString = Top.Cfg.PositionId + "_" + "TRANSFER" + "_" + tlf.Literal;
								General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.TlfFacilityOid, snmpString));
							});
						}
					}
				}

				State = st;
			}
		}

		#region Private Members

		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private FunctionState _State = FunctionState.Idle;
		private TlfPosition _AssociateCall = null;

        //Cuando falla una transferencia atendida en un sentido, se intenta en el sentido contrario
        //Vale true si se ha intentado la transferencia a una de las partes y ha dado error
        //     false en caso contrario
        private bool _FirstTransferTryKO = false;
        //Variables usadas para hacer la transferencia en el otro sentido, si falla el primer intento.
        private TlfPosition _ToTransfer = null;
        private string _FromTransferDisplayName = "";

		private void OnAssociateCallStateChanged(object sender)
		{
			Debug.Assert(_State == FunctionState.Executing);
			Debug.Assert(sender == _AssociateCall);

			switch (_AssociateCall.State)
			{
				case TlfState.Unavailable:
				case TlfState.Idle:
				case TlfState.PaPBusy:
					AssociateCall = null;
					break;
			}
		}

        /// <summary>
        /// Evento recibido con el resultado de la transferencia
        /// Si se recibe un codigo de error se intenta la transferencia con el otro participante
        /// </summary>
        /// <param name="newRtxGroup"></param>
        /// <returns></returns>
        private void OnTransferStatus(object sender, int callId, int code)
		{
			if ((code > 101) && (code < 300) && (code != SipAgent.SIP_ACCEPTED))
			{
				if (_AssociateCall != null)
				{
					_AssociateCall.HangUp(0);
				}
				Top.Sip.TlfTransferStatus -= OnTransferStatus;
				State = FunctionState.Idle;
                _ToTransfer = null;
                _FromTransferDisplayName = "";
			}
            else if ((code >= 300) && !_FirstTransferTryKO && (_ToTransfer != null))
            {
                _FirstTransferTryKO = true;
                //Try transfer with other participant
                SipAgent.TransferCall(_ToTransfer.CallId, AssociateCall.CallId, null, _FromTransferDisplayName);
                AssociateCall = _ToTransfer;
                _ToTransfer = null;
                _FromTransferDisplayName = "";
            }
            else if ((code >= 300) && _FirstTransferTryKO)
            {
                AssociateCall = null;
                Top.Sip.TlfTransferStatus -= OnTransferStatus;
                State = FunctionState.Error;
                _ToTransfer = null;
                _FromTransferDisplayName = "";
            }
		}

		#endregion
	}
}

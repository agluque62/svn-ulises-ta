using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;

using NLog;

namespace HMI.CD40.Module.BusinessEntities
{
	class CORESIP_Answer
	{
		public int Value;

		public CORESIP_Answer(int value)
		{
			Value = value;
		}
	}

	class SipManager
	{
		public event GenericEventHandler<int, CORESIP_CallStateInfo> TlfCallStateChanged;
		public event GenericEventHandler<int, CORESIP_CallStateInfo> LcCallStateChanged;
		public event GenericEventHandler<int, CORESIP_CallStateInfo> MonitoringCallStateChanged;
		public event GenericEventHandler<int, int, CORESIP_CallInfo, CORESIP_CallInInfo, CORESIP_Answer> IncomingTlfCall;
		public event GenericEventHandler<int, CORESIP_CallInfo, CORESIP_CallInInfo, CORESIP_Answer> IncomingLcCall;
		public event GenericEventHandler<int, CORESIP_CallInfo, CORESIP_CallInInfo, CORESIP_Answer> IncomingMonitoringCall;
		public event GenericEventHandler<int, CORESIP_CallInfo, CORESIP_CallTransferInfo, CORESIP_Answer> TlfTransferRequest;
		public event GenericEventHandler<int, string, uint> InfoReceived;
        //Evento que envia la CORESIP cuando se recibe una suscripcion del evento 'conference'
        public event GenericEventHandler<int, string, uint> IncomingSubscribeConf;
        public event GenericEventHandler<int, int> TlfTransferStatus;
		public event GenericEventHandler<int, CORESIP_ConfInfo> TlfCallConfInfo;

		public void Init()
		{
			Top.Cfg.ConfigChanged += OnConfigChanged;

			SipAgent.CallState += OnCallState;
			SipAgent.CallIncoming += OnCallIncoming;
			SipAgent.TransferRequest += OnTransferRequest;
			SipAgent.TransferStatus += OnTransferStatus;
			SipAgent.OptionsReceive += OnOptionsReceive;
			SipAgent.ConfInfo += OnConfInfoReceive;
			SipAgent.InfoReceived += OnInfoReceived;
            SipAgent.IncomingSubscribeConf += OnIncomingSubscribeConf;

			SipAgent.Init(Top.HostId, Top.SipIp, Settings.Default.SipPort, Settings.Default.MaxCalls);
		}

		public void Start()
		{
			SipAgent.Start();
            ///** 20180615. AGL. Provisional para las pruebas... */
            //SipAgent.EchoCancellerLCMic(Properties.Settings.Default.EchoCancellerLCMic);
		}

		public void End()
		{
			SipAgent.End();
		}

		#region Private Members

		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private void OnConfigChanged(object sender)
		{
            SipAgent.DestroyAccounts();

            string idEquipo;
            string proxyIP = Top.Cfg.GetProxyIp(out idEquipo);
            uint expire = Settings.Default.ExpireInProxy;

            foreach (StrNumeroAbonado num in Top.Cfg.HostAddresses)
            { 
                if ((proxyIP == null) || (proxyIP.Length == 0))
                    SipAgent.CreateAccount(num.NumeroAbonado);
                else
                {
                    SipAgent.CreateAccountAndRegisterInProxy(num.NumeroAbonado, proxyIP, expire, num.NumeroAbonado, num.NumeroAbonado, num.IdAgrupacion);
                }
            }

            // JCAM. 20170324
            // Incorporar las direcciones de los grabadores en la configuración 
            // y hacer que el módulo de grabación se entere del cambio de configuración
            AsignacionUsuariosTV tv = Top.Cfg.GetUserTv(Top.Cfg.PositionId);
            if (tv != null)
            {
                SipAgent.PictRecordingCfg(tv.IpGrabador1, tv.IpGrabador2, tv.RtspPort);
            }
        }

		private void OnCallState(int call, CORESIP_CallInfo info, CORESIP_CallStateInfo stateInfo)
		{
			switch (info.Type)
			{
				case CORESIP_CallType.CORESIP_CALL_DIA:
					if ((stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED) ||
						(stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED) ||
						((stateInfo.Role == CORESIP_CallRole.CORESIP_CALL_ROLE_UAC) &&
						(stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_EARLY)))
					{
						Top.WorkingThread.Enqueue("DIAStateChanged", delegate()
						{
							General.SafeLaunchEvent(TlfCallStateChanged, this, call, stateInfo);
						});
					}
					break;
				case CORESIP_CallType.CORESIP_CALL_IA:
					if ((stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED) ||
						(stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED))
					{
						Top.WorkingThread.Enqueue("IAStateChanged", delegate()
						{
							General.SafeLaunchEvent(LcCallStateChanged, this, call, stateInfo);
						});
					}
					break;
				case CORESIP_CallType.CORESIP_CALL_MONITORING:
				case CORESIP_CallType.CORESIP_CALL_GG_MONITORING:               
				case CORESIP_CallType.CORESIP_CALL_AG_MONITORING:
					if ((stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED) ||
						(stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED))
					{
						Top.WorkingThread.Enqueue("MonitoringStateChanged", delegate()
						{
							General.SafeLaunchEvent(MonitoringCallStateChanged, this, call, stateInfo);
						});
					}
					break;
			}
		}

		private void OnCallIncoming(int call, int call2replace, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo)
		{
			_Logger.Debug("Nueva llamada entrante [SrdId={0}] [SrcIp={1}] [SrcSubId={2}] [SrcRs={3}] [DstId={4}] [DstIp={5}] [DstSubId={6}]",
				inInfo.SrcId, inInfo.SrcIp, inInfo.SrcSubId, inInfo.SrcRs, inInfo.DstId, inInfo.DstIp, inInfo.DstSubId);

			Top.WorkingThread.Enqueue("OnCallIncoming", delegate()
			{
				CORESIP_Answer answer = new CORESIP_Answer(SipAgent.SIP_NOT_FOUND);
				string dstId = string.IsNullOrEmpty(inInfo.DstSubId) ? inInfo.DstId : inInfo.DstSubId;

				foreach (StrNumeroAbonado alias in Top.Cfg.HostAddresses)
				{
					if (string.Compare(dstId, alias.NumeroAbonado, true) == 0)
					{
						answer.Value = SipAgent.SIP_DECLINE;

						switch (info.Type)
						{
							case CORESIP_CallType.CORESIP_CALL_DIA:
								General.SafeLaunchEvent(IncomingTlfCall, this, call, call2replace, info, inInfo, answer);
								break;
							case CORESIP_CallType.CORESIP_CALL_IA:
								General.SafeLaunchEvent(IncomingLcCall, this, call, info, inInfo, answer);
								break;
                            case CORESIP_CallType.CORESIP_CALL_GG_MONITORING:       //ULISES solo soporta G/G monitoring
								General.SafeLaunchEvent(IncomingMonitoringCall, this, call, info, inInfo, answer);
								break;
                            case CORESIP_CallType.CORESIP_CALL_MONITORING:
                            case CORESIP_CallType.CORESIP_CALL_AG_MONITORING:
                                answer.Value = SipAgent.SIP_BAD_REQUEST;            //ULISES solo soporta G/G monitoring
                                break;
						}

						break;
					}
				}

				if (answer.Value != 0 && answer.Value != SipAgent.SIP_DECLINE)
				{
                    try 
                    {
					    SipAgent.AnswerCall(call, answer.Value);
                    }
                    catch (Exception exc)
                    {
                        _Logger.Error("SipAgent.AnswerCall", exc.Message);
                    }
				}
			});
		}

		private void OnTransferRequest(int call, CORESIP_CallInfo info, CORESIP_CallTransferInfo transferInfo)
		{
			Debug.Assert(info.Type == CORESIP_CallType.CORESIP_CALL_DIA);
			_Logger.Debug("Nueva peticion de transferencia [CallId={0}] [DstId={1}] [DstIp={2}] [DstSubId={3}] [DstRs={4}]",
				call, transferInfo.DstId, transferInfo.DstIp, transferInfo.DstSubId, transferInfo.DstRs);

			Top.WorkingThread.Enqueue("OnTransferRequest", delegate()
			{
				CORESIP_Answer answer = new CORESIP_Answer(SipAgent.SIP_DECLINE);
				General.SafeLaunchEvent(TlfTransferRequest, this, call, info, transferInfo, answer);

				if (answer.Value != 0)
				{
					SipAgent.TransferAnswer(transferInfo.TsxKey, transferInfo.TxData, transferInfo.EvSub, answer.Value);
				}
			});
		}

		private void OnTransferStatus(int call, int code)
		{
			_Logger.Debug("Notificacion sobre transferencia [CallId={0}] [Code={1}]", call, code);

			Top.WorkingThread.Enqueue("OnTransferStatus", delegate()
			{
				General.SafeLaunchEvent(TlfTransferStatus, this, call, code);
			});
		}

		private void OnOptionsReceive(string fromUri, string callid, int statusCodem, string supported, string allow)
		{
		}

		private void OnConfInfoReceive(int call, CORESIP_ConfInfo confInfo)
		{
			_Logger.Debug("Recibida informacion sobre conferencia [CallId={0}] [Version={1}] [State={2}] [NumUsers={3}]",
				call, confInfo.Version, confInfo.State, confInfo.UsersCount);

			Top.WorkingThread.Enqueue("OnConfInfoReceive", delegate()
			{
				General.SafeLaunchEvent(TlfCallConfInfo, this, call, confInfo);
			});
		}

		private void OnInfoReceived(int call, string info, uint lenInfo)
		{
			Top.WorkingThread.Enqueue("OnInfoReceived", delegate()
			{
				General.SafeLaunchEvent(InfoReceived,this, call, info, lenInfo);
			});
		}
        private void OnIncomingSubscribeConf(int call, string info, uint lenInfo)
        {
            Top.WorkingThread.Enqueue("IncomingSubscribeConf", delegate()
            {
                General.SafeLaunchEvent(IncomingSubscribeConf, this, call, info, lenInfo);
            });
        }
        #endregion
	}
}

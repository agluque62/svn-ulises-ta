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
#if DEBUG
    public class CORESIP_Answer
#else
	class CORESIP_Answer
#endif		
	{
		public int Value;
        public string redirectTo = "";
		public CORESIP_Answer(int value)
		{
			Value = value;
		}
	}

#if DEBUG
    public class SipManager
#else
	class SipManager
#endif	
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
        public event GenericEventHandler<string, string, CORESIP_ConfInfo> TlfCallConfInfoAcc;
        public event GenericEventHandler<string> NotifyDialog;
        public event GenericEventHandler<string, string, CORESIP_CFWR_OPT_TYPE, string, uint> CallForwardAsk;
        public event GenericEventHandler<string, int, CORESIP_CFWR_OPT_TYPE, string> CallForwardResp;
        public event GenericEventHandler<int, string> CallMoved;
        public event GenericEventHandler<string> SipMessage;

        public void Init()
		{
			Top.Cfg.ConfigChanged += OnConfigChanged;

			SipAgent.CallState += OnCallState;
			SipAgent.CallIncoming += OnCallIncoming;
			SipAgent.TransferRequest += OnTransferRequest;
			SipAgent.TransferStatus += OnTransferStatus;
			SipAgent.OptionsReceive += OnOptionsReceive;
			SipAgent.ConfInfo += OnConfInfoReceive;
            SipAgent.ConfInfoAcc += OnConfInfoReceive;
			SipAgent.InfoReceived += OnInfoReceived;
            SipAgent.IncomingSubscribeConf += OnIncomingSubscribeConf;
            SipAgent.DialogNotify += OnDialogNotify;
            SipAgent.CfwrOptReceived += OnCfwrOptReceived;
            SipAgent.CfwrOptResponse += OnCfwrOptResponse;
            SipAgent.Pager += OnSipMessageReceived;
            SipAgent.MovedTemporally += OnSipMovedTemp;
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
        string _ProxyIP = null;
        /// <summary>
        /// Class to manage configuration data for Sip Accounts
        /// </summary>
        private class AccountDataSip
        {
            public string NumeroAbonado;
            public string IdAgrupacion;
            public AccountDataSip(StrNumeroAbonado num)
            {
                NumeroAbonado = num.NumeroAbonado;
                IdAgrupacion = num.IdAgrupacion;
            }
            public override bool Equals(object obj)
            {
                bool ret = false;
                if (obj == null)
                    return ret;
                if (this.GetType() != obj.GetType()) return ret;

                AccountDataSip p = (AccountDataSip)obj;
                if ((p.IdAgrupacion.Equals(IdAgrupacion)) &&
                     (p.NumeroAbonado.Equals(NumeroAbonado)))
                    ret = true;
                return ret;
            }
        }
        private List<AccountDataSip>AccDataList = new List <AccountDataSip>();
        /// <summary>
        /// Handler to receive new configuration
        /// Avoid to re-create account if its data haven't changed
        /// </summary>
        /// <param name="sender"></param>
		private void OnConfigChanged(object sender)
		{
            string idEquipo;
            string newProxy = Top.Cfg.GetProxyIp(out idEquipo);
            List<AccountDataSip> NewAccDataList = new List<AccountDataSip>();
            foreach (StrNumeroAbonado num in Top.Cfg.HostAddresses)
            { 
                if (num.Prefijo == Cd40Cfg.ATS_DST)
                    NewAccDataList.Add(new AccountDataSip(num));
            }

            if (_ProxyIP != newProxy)
            {
                // If proxy has changed all accounts are created again
                SipAgent.DestroyAccounts();
                AccDataList = new List <AccountDataSip>(NewAccDataList);
                _ProxyIP = newProxy;
                foreach( AccountDataSip data in AccDataList)
                {
                    if ((_ProxyIP == null) || (_ProxyIP.Length == 0))
                        SipAgent.CreateAccount(data.NumeroAbonado);
                    else
                    {
                        SipAgent.CreateAccountAndRegisterInProxy(data.NumeroAbonado, _ProxyIP, Settings.Default.ExpireInProxy,
                            data.NumeroAbonado, data.NumeroAbonado, GenIdAgrupacion(data.IdAgrupacion));
                    }
                }
            }
            else
                SynchronizeAccountsList(NewAccDataList);


            // JCAM. 20170324
            // Incorporar las direcciones de los grabadores en la configuración 
            // y hacer que el módulo de grabación se entere del cambio de configuración
            AsignacionUsuariosTV tv = Top.Cfg.GetUserTv(Top.Cfg.MainId);
            if (tv != null)
            {
                //SipAgent.PictRecordingCfg(tv.IpGrabador1, tv.IpGrabador2, tv.RtspPort);
                //RQF-24
                bool EnableGrabacionEd137ant = SipAgent.GetEnableGrabacionED137();
                SipAgent.PictRecordingCfg(tv.IpGrabador1, tv.IpGrabador2, tv.RtspPort, tv.RtspPort2,
                                          tv.EnableGrabacionEd137);

                //RQF24
                bool EnableGrabacionEd137 = tv.EnableGrabacionEd137;
                //EnableGrabacionEd137 = 1;
                if (EnableGrabacionEd137!= EnableGrabacionEd137ant)
                Top.WorkingThread.Enqueue("EnableGrabacionEd137", delegate ()
                {
                    SipAgent.Record(EnableGrabacionEd137) ;
                });
            }
        }
        /// <summary>
        /// Update AccDataList with new AccDataList built with new configuration
        /// Delete and create only accounts with data that has changed, to avoid
        /// interruption of service if data haven't changed
        /// </summary>
        /// <param name="NewAccDataList">list of new configurated params</param>
        private void SynchronizeAccountsList(List<AccountDataSip> NewAccDataList)
        {
            AccountDataSip found = null;
            
            List<AccountDataSip> AccDataListCopy = new List<AccountDataSip>(AccDataList);
            foreach (AccountDataSip data in AccDataListCopy)
            {
                found = NewAccDataList.Find(x=>data.Equals(x));
                if (found == null)
                {
                    AccDataList.Remove(data);                    
                    SipAgent.DestroyAccount(data.NumeroAbonado);
                }
                else
                    NewAccDataList.Remove(found);
            }
            foreach (AccountDataSip newData in NewAccDataList)
            {
                AccDataList.Add(newData);
                if ((_ProxyIP == null) || (_ProxyIP.Length == 0))
                    SipAgent.CreateAccount(newData.NumeroAbonado);
                else
                    SipAgent.CreateAccountAndRegisterInProxy(newData.NumeroAbonado, _ProxyIP, Settings.Default.ExpireInProxy,
                        newData.NumeroAbonado, newData.NumeroAbonado, GenIdAgrupacion(newData.IdAgrupacion));
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
                    try
                    {
                        if (answer.Value == SipAgent.SIP_MOVED_TEMPORARILY)
                            SipAgent.MovedTemporallyAnswerCall(call, answer.redirectTo, "unconditional");
                        else if (answer.Value != 0 && answer.Value != SipAgent.SIP_DECLINE)
                        {
                            SipAgent.AnswerCall(call, answer.Value);
                        }
                    }
                    catch (Exception exc)
                    {
                        _Logger.Error("SipAgent.AnswerCall", exc.Message);
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
         private void OnConfInfoReceive(int call, CORESIP_ConfInfo confInfo, string from, uint lenfrom)
		{
			_Logger.Debug("Recibida informacion sobre conferencia [CallId={0}] [Version={1}] [State={2}] [NumUsers={3}]",
				call, confInfo.Version, confInfo.State, confInfo.UsersCount);

			Top.WorkingThread.Enqueue("OnConfInfoReceive", delegate()
			{
				General.SafeLaunchEvent(TlfCallConfInfo, this, call, confInfo);
			});
		}
        private void OnConfInfoReceive(string accountId, CORESIP_ConfInfo confInfo, string from, uint lenfrom)
        {
            _Logger.Debug("Recibida informacion sobre conferencia [from={0}] [Version={1}] [State={2}] [NumUsers={3}] [from={4}] ",
                from, confInfo.Version, confInfo.State, confInfo.UsersCount, accountId);

            Top.WorkingThread.Enqueue("OnConfInfoReceive", delegate()
            {
                General.SafeLaunchEvent(TlfCallConfInfoAcc, this, accountId, from, confInfo);
            });
        }

        private void OnDialogNotify(string xml_body, uint length)
        {
            _Logger.Debug("Recibido notify sobre dialogo");

            Top.WorkingThread.Enqueue("OnDialogNotify", delegate()
            {
                General.SafeLaunchEvent(NotifyDialog, this, xml_body);
            });
        }

        private void OnSipMessageReceived(string from_uri, uint from_uri_len,
             string to_uri, uint to_uri_len, string contact_uri, uint contact_uri_len,
             string mime_type, uint mime_type_len, string body, uint body_len)
        {
            _Logger.Debug("Recibido mensaje texto sobre dialogo");

            Top.WorkingThread.Enqueue("OnSipMessageReceived", delegate()
            {
                General.SafeLaunchEvent(SipMessage, this, body);
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

        private void OnCfwrOptReceived(string accId, string from_uri, CORESIP_CFWR_OPT_TYPE cfwr_options_type, string body, uint hresp)
        {
            _Logger.Debug("Recibido options call forward");

            Top.WorkingThread.Enqueue("OnCfwrOpt", delegate ()
            {
                General.SafeLaunchEvent(CallForwardAsk, this, accId, from_uri, cfwr_options_type, body, hresp);
            });
        }

        private void OnCfwrOptResponse(string accId, string dstUri, string callid, int st_code, CORESIP_CFWR_OPT_TYPE cfwr_options_type, string body)
        {
            _Logger.Debug("Recibido options call forward");

            Top.WorkingThread.Enqueue("OnCfwrOpt", delegate ()
            {
                General.SafeLaunchEvent(CallForwardResp, this, accId, st_code, cfwr_options_type, body);
            });
        }

        private void OnSipMovedTemp(int call, string dstUri)
        {
            _Logger.Debug("Recibido respuesta moved temporarly para hacer un desvío");

            Top.WorkingThread.Enqueue("OnCfwrOpt", delegate()
            {
                General.SafeLaunchEvent(CallMoved, this, call, dstUri);
            });
        }

        #endregion
        //LALM 210618
        // Funcion Que limita el numero maximo de caracteres de una agrupacion a 16.
        private String GenIdAgrupacion(String agrupacion)
        {
            int longmax = 16;
            String IdAgrupacion = agrupacion;
            int len = agrupacion.Length;
            if (len > longmax)
            {
                int mitad = longmax / 2;
                IdAgrupacion = agrupacion.Substring(0, mitad - 1) + ".." + agrupacion.Substring(len - (mitad - 1), mitad - 1);
            }
            return IdAgrupacion;
        }
    }
}

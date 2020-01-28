using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;
using NLog;

namespace HMI.CD40.Module.BusinessEntities
{
#if DEBUG
    public class LcManager
#else
	class LcManager
#endif
    {
		public event GenericEventHandler ActivityChanged;
		public event GenericEventHandler<RangeMsg<LcInfo>> NewPositions;
		public event GenericEventHandler<RangeMsg<LcState>> PositionsChanged;
		public event GenericEventHandler<SnmpStringMsg<string, string>> SetSnmpString;
        /// <summary>
        /// Evento lanzado para poner una llamada telefonica en espera de forma automática
        /// </summary>
        public event GenericEventHandler<StateMsg<bool>> HoldTlfCallEvent;

		public bool Activity
		{
			get { return _Activity; }
			private set
			{
				if (_Activity != value)
				{
					_Activity = value;
					General.SafeLaunchEvent(ActivityChanged, this);
				}
			}
		}

        public bool AnyActiveLcRx //Miguel
        {
            get { return _AnyActiveLcRx; }
        }

		public void Init()
		{
			Top.Cfg.ConfigChanged += OnConfigChanged;
			Top.Sip.LcCallStateChanged += OnCallStateChanged;
			Top.Sip.IncomingLcCall += OnIncomingCall;
            Top.Hw.JacksChangedHw += OnJacksChanged;


			for (int i = 0, to = _LcPositions.Length; i < to; i++)
			{
				_LcPositions[i] = new LcPosition(i);
				_LcPositions[i].StateChanged += OnLcStateChanged;
			}
		}

		public void Start()
		{
		}

		public void End()
		{
            for (int i = 0; i < _LcPositions.Length; i++)
            {
                if (_LcPositions[i].RxState == LcRxState.Rx)
                {
                    _LcPositions[i].HangUpRx();
                    break;
                }
            }
		}

		public void SetLc(int id, bool on)
		{
			Debug.Assert(id < _LcPositions.Length);

			if (on)
			{
				_LcPositions[id].Call();
                _TonoFalsaManiobra = true;
            }
			else
			{
                _TonoFalsaManiobra = false;
                _LcPositions[id].HangUpTx();
			}
		}
        /// <summary>
        /// Funcion para cambiar el estado del atributo _HoldedTlf, se usa desde las LcPosition
        /// </summary>
        public bool HoldedTlf
        {
            get
            {
                /// Devuelve true si está aparcada
                if (_HoldedTlf > 0) return true;
                else return false;
            }
            set
            {   // Solo aplica si comparten altavoz con la telefonía
                if (Top.Mixer.RxTlfAudioVia == TlfRxAudioVia.Speaker)
                {
                    // se aparca la llamada si hay llamadas telefonicas activas (aparcada no es activa)
                    // se desaparca la llamada, si se ha aparcado antes desde aquí.
                    if ((value && (Top.Tlf.Activity() || Top.Rd.HoldedByPtt)) || (!value && _HoldedTlf == 1 && !Top.Rd.HoldedByPtt))
                        General.SafeLaunchEvent(HoldTlfCallEvent, this, new StateMsg<bool>(value));
                    if (value && ((_HoldedTlf != 0) || (Top.Tlf.Activity() || Top.Rd.HoldedByPtt)))
                        _HoldedTlf++;
                    else if (_HoldedTlf > 0)
                        _HoldedTlf--;
                }
                else 
                {
                    if (Top.Mixer.RxTlfAudioVia == TlfRxAudioVia.HeadPhones)
                    {
                        //Desaparco la llamada si se cambió a cascos
                        if (_HoldedTlf > 0)
                            General.SafeLaunchEvent(HoldTlfCallEvent, this, new StateMsg<bool>(false));
                    }
                    _HoldedTlf = 0;
                }
            }
        }

		#region Private Members

		private bool _Activity = false;
		private bool _ChangingCfg = false;
		private LcPosition[] _LcPositions = new LcPosition[Lc.NumDestinations];
		private LcPosition _ActiveCall = null;
        private bool _AnyActiveLcRx = false;//Miguel
        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        private bool _TonoFalsaManiobra = false;
        /// <summary>
        /// Guarda las peticiones para aparcar una llamada de telefonia por cualquier LcPosition
        /// </summary>
        private int _HoldedTlf = 0;

		private void OnConfigChanged(object sender)
		{
			_ChangingCfg = true;
            try
            {
			RangeMsg<LcInfo> lcPositions = new RangeMsg<LcInfo>(0, _LcPositions.Length);

			foreach (CfgEnlaceInterno link in Top.Cfg.LcLinks)
			{
                try
                {
                    int pos = (int)link.PosicionHMI - 1;

                    if (pos < _LcPositions.Length)
                    {
                        LcPosition lc = _LcPositions[pos];
                        lc.Reset(link);

                        LcInfo posInfo = new LcInfo(lc.Literal, lc.RxState, lc.TxState, lc.Group);
                        lcPositions.Info[pos] = posInfo;
                    }
                }
                catch (Exception excep)
                {
                    _Logger.Error("Excepcion OnConfigChanged. Mensaje: {0}", excep.Message);
                }
			}

			for (int i = 0, to = _LcPositions.Length; i < to; i++)
			{
				if (lcPositions.Info[i] == null)
				{
					LcPosition lc = _LcPositions[i];
					lc.Reset();

					LcInfo posInfo = new LcInfo(lc.Literal, lc.RxState, lc.TxState, lc.Group);
					lcPositions.Info[i] = posInfo;
				}
			}
			General.SafeLaunchEvent(NewPositions, this, lcPositions);
		}
            catch (Exception exc)
            {
                _Logger.Error(String.Format("LcManager:OnConfigChanged exception {0}, {1}", exc.Message, exc.StackTrace));
            }
            finally
            {
                _ChangingCfg = false;
            }
		}

		private void OnCallStateChanged(object sender, int call, CORESIP_CallStateInfo stateInfo)
		{
			foreach (LcPosition lc in _LcPositions)
			{
				if (lc.HandleChangeInCallState(call, stateInfo))
				{
					break;
				}
			}
		}

		private void OnIncomingCall(object sender, int call, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo, CORESIP_Answer answer)
		{
			foreach (LcPosition lc in _LcPositions)
			{
				answer.Value = lc.HandleIncomingCall(call, info, inInfo);
				if (answer.Value != SipAgent.SIP_DECLINE)
				{
					break;
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="st"></param>
        private void OnJacksChanged(object sender, JacksStateMsg st)
        {
            try
            {
            bool jacks = st.LeftJack || st.RightJack;
            _Logger.Debug("LcManager OnJacksChanged {0} || {1} = {2}", st.LeftJack, st.RightJack, jacks);
            foreach (LcPosition lc in _LcPositions)
            {
                if (lc.TxState == LcTxState.Tx)
                    lc.HangUpTx();
            }
        }
            catch (Exception e)
            {
                _Logger.Debug("Excepcion. Mensaje: {0}", e.Message);
            }
        }

		private void OnLcStateChanged(object sender)
		{
			LcPosition lc = (LcPosition)sender;
            _AnyActiveLcRx = DetectedAnyLC();// Miguel

			switch (lc.TxState)
			{
				case LcTxState.Out:
				case LcTxState.Busy:
				case LcTxState.Congestion:
				case LcTxState.Tx:
					Debug.Assert((_ActiveCall == null) || (_ActiveCall == lc));
					_ActiveCall = lc;
					Activity = true;

                    if (lc.TxState == LcTxState.Tx && lc.OldTxState != LcTxState.Tx)
                    {
                        Top.Recorder.SessionGlp(FuentesGlp.TxLc, true);

                        Top.WorkingThread.Enqueue("SetSnmp", delegate()
                        {
                            string snmpString = Top.Cfg.PositionId + "_" + "LC" + "_" + lc.Literal + "_" + lc.TxState.ToString();
                            General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.OutgoingTfCallOid, snmpString));
                        });
                    }

					break;
				default:
					if (_ActiveCall == lc)
					{
						_ActiveCall = null;
						Activity = false;

                        if (lc.OldTxState != LcTxState.Unavailable && lc.TxState == LcTxState.Idle)
                        {
                            // Tratamiento para cuando mientras se está transmitiendo, 
                            // el colateral se cae.
                            if (lc.OldTxState == LcTxState.Tx && _TonoFalsaManiobra)
                            {
                                _TonoFalsaManiobra = false;
                                int _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/Falsa_Maniobra.wav", true);
                                Top.Mixer.Link(_BadOperationTone, MixerDev.SpkLc, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxLc);

                                Top.WorkingThread.Enqueue("Wait", delegate()
                                {
                                    System.Threading.Thread.Sleep(500);
                                });

                                Top.Mixer.Unlink(_BadOperationTone);
                                SipAgent.DestroyWavPlayer(_BadOperationTone);
                            }

                            Top.Recorder.SessionGlp(FuentesGlp.TxLc, false);
                            Top.WorkingThread.Enqueue("SetSnmp", delegate()
                            {
                                string snmpString = Top.Cfg.PositionId + "_" + "LC" + "_" + lc.Literal + "_" + lc.TxState.ToString();
                                General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.OutgoingTfCallOid, snmpString));
                            });
                        }
                    }
					break;
			}

            // Sólo para gestión SNMP
            if (lc.OldRxState != LcRxState.Unavailable)
            {
                switch (lc.RxState)
                {
                    case LcRxState.Rx:
                        if (lc.OldRxState != LcRxState.Rx)
                        {
                            Top.WorkingThread.Enqueue("SetSnmp", delegate()
                            {
                                string snmpString = Top.Cfg.PositionId + "_" + "LC" + "_" + lc.Literal + "_" + lc.RxState.ToString();
                                General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.IncommingTfCallOid, snmpString));
                            });
                        }
                        break;
                    case LcRxState.Idle:
                        if (lc.OldRxState == LcRxState.Rx)
                        {
                            Top.WorkingThread.Enqueue("SetSnmp", delegate()
                            {
                                string snmpString = Top.Cfg.PositionId + "_" + "LC" + "_" + lc.Literal + "_" + lc.RxState.ToString();
                                General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.IncommingTfCallOid, snmpString));
                            });
                        }
                        break;
                    default:
                        break;
                }
            }

			if (!_ChangingCfg)
			{
				RangeMsg<LcState> state = new RangeMsg<LcState>(lc.Pos, new LcState(lc.RxState, lc.TxState));
				General.SafeLaunchEvent(PositionsChanged, this, state);
			}
		}

        private bool DetectedAnyLC() // 21/06/12 Miguel
        {
            bool AnyLcIN = false;
            
            for (int i = 0, to = _LcPositions.Length; i < to; i++)			
            {
                if (_LcPositions[i].RxState == LcRxState.Rx || _LcPositions[i].RxState == LcRxState.RxNotif)
                {
                    AnyLcIN = true;
                    break;
                }

            }

            return AnyLcIN;
        }
        

		#endregion
	}
}

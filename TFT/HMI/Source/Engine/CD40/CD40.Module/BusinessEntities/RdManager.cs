#define _HF_GLOBAL_STATUS_
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Timers;
using System.Threading.Tasks;

using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using HMI.CD40.Module.Properties;
using HMI.CD40.Module.Snmp;


using U5ki.Infrastructure;
using Utilities;
using NLog;
using System.Net.NetworkInformation;

namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// 
    /// </summary>
#if DEBUG
    public class RdManager
#else
	class RdManager
#endif
    {
        /// <summary>
        /// 
        /// </summary>
		public event GenericEventHandler<RangeMsg<RdInfo>> NewPositions;
		public event GenericEventHandler<RangeMsg<RdState>> PositionsChanged;
        public event GenericEventHandler<RdFrAsignedToOtherMsg> TxAssign;
        public event GenericEventHandler<RdHfFrAssigned> TxHfAssign;
        public event GenericEventHandler<StateMsg<bool>> PttChanged;
        public event GenericEventHandler<StateMsg<string>> SelCalMessage;
		public event GenericEventHandler PTTMaxTime;
        public event GenericEventHandler<ChangeSiteRsp> SiteChangedResultMessage;
        public event GenericEventHandler<RdRxAudioVia> AudioViaNotAvailable;

#if _HF_GLOBAL_STATUS_
        public event GenericEventHandler<HFStatusCodes> HFGlobalStatus;
#endif

        /// <summary>
        /// 
        /// </summary>
		public event GenericEventHandler<StateMsg<bool>> HoldTlfCall;

        /// <summary>
        /// 
        /// </summary>
		public event GenericEventHandler<SnmpStringMsg<string, string>> SetSnmpString;
		public event GenericEventHandler<SnmpIntMsg<string, int>> SetSnmpInt;

        /// <summary>
        /// 
        /// </summary>
		public PttSource PttSource
		{
			get { return _PttSource; }
			private set
			{
				if (_PttSource != value)
				{
					bool oldPttOn = (_PttSource != PttSource.NoPtt);
					bool newPttOn = (value != PttSource.NoPtt);

                    _OldPttSource = _PttSource;
					_PttSource = value;
					if (oldPttOn != newPttOn)
					{
						General.SafeLaunchEvent(PttChanged, this, new StateMsg<bool>(newPttOn));
					}
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        public bool AnySquelch
        {
            get { return DetectedAnySquech(); }
        }

        public bool ScreenSaverStatus
        {
            get { return _ScreenSaver; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SiteManager
        {
            get { return _SiteManager; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SiteManaging
        {
            get { return _SiteManaging; }
            set { _SiteManaging = value; }
        }

        public bool DoubleRadioSpeaker
        {
            get { return _DoubleRadioSpeaker; }
            set { _DoubleRadioSpeaker = value; }
        }

        public bool HoldedByPtt
        {
            get { return _HoldedByPtt; }
            set { _HoldedByPtt = value; }
        }

        //Devuelve true si está conectado sin fallo y configurado
        public bool HFSpeakerAvailable ()
        {
            return (Top.Hw.HfSpeaker && (Top.Mixer.HfSpeakerDev > 0 || Top.Hw is SimCMediaHwManager));
        }

        // LALM 210521 
        // Peticiones #4816
        public static bool GetIsMyNetworkAvailable()
        {
            String SipIp = Top.SipIp;
            List<string> ips = new List<string>();
            NetworkInterface[] nets = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface iface in nets)
                if ((iface.NetworkInterfaceType != NetworkInterfaceType.Loopback) &&
                    (iface.OperationalStatus == OperationalStatus.Up))
                    foreach (UnicastIPAddressInformation ip in iface.GetIPProperties().UnicastAddresses)
                        ips.Add(ip.Address.ToString());
            foreach (String ip in ips)
                if (ip == SipIp)
                    return true;
            return false;
        }
            /// <summary>
            /// 
            /// </summary>
            public void Init()
		{
			_PttTimer.AutoReset = false;
			_PttTimer.Elapsed += OnPttTimerElapsed;
            _PttBadOpeTimer.AutoReset = false;
            _PttBadOpeTimer.Elapsed += OnPttBadOpeTimerElapsed;

            _TimerNetworkStatus.AutoReset = true;
            _TimerNetworkStatus.Elapsed += _TimerNetworkStatus_Elapsed;

			Top.Cfg.ConfigChanged += OnConfigChanged;
			Top.Hw.PttPulsed += OnPttPulsed;
			Top.Hw.JacksChangedHw += OnHwChanged;
            Top.Hw.SpeakerExtChangedHw += OnHwChanged;
            Top.Hw.SpeakerChangedHw += OnHwChanged;
            Top.Lc.ActivityChanged += OnLcActivityChanged;
			Top.Mixer.SplitModeChanged += OnSplitModeChanged;

			for (int i = 0, to = _RdPositions.Length; i < to; i++)
			{
				_RdPositions[i] = new RdPosition(i);
				_RdPositions[i].StateChanged += OnRdStateChanged;
				_RdPositions[i].TxAlreadyAssigned += OnRdTxAlreadyAssigned;
                _RdPositions[i].RxAlreadyAssigned += OnRdRxAlreadyAssigned;
				_RdPositions[i].TxAssignedByOther += OnRdTxAssignedByOther;
                _RdPositions[i].TxHfAlreadyAssigned += OnRdTxHfAlreadyAssigned;
                _RdPositions[i].SelCalPrepareResponse += OnRdSelCalPrepareResponse;
                _RdPositions[i].SiteChangedResult += OnSiteChangedResult;
                _RdPositions[i].AudioViaNotAvailable += OnAudioViaNotAvailable;

#if _HF_GLOBAL_STATUS_
                _RdPositions[i].HfGlobalStatus += OnRdHFGlobalStatus;
#endif
			}
		}

        void _TimerNetworkStatus_Elapsed(object sender, ElapsedEventArgs e)
        {
            // if (_StatusNetworkOn && !System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            // LALM 210521 
            // Peticiones #4816 Comprobar si hay varios interfaces de red activos
            if (_StatusNetworkOn && !GetIsMyNetworkAvailable())
            {
                _StatusNetworkOn = false;

                _Logger.Error("Reset HMI por pérdida de red.");
                Process.Start("Launcher.exe", "HMI.exe");
            }
            // else if (!_StatusNetworkOn && System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            // LALM 210521 
            // Peticiones #4816 Comprobar si hay varios interfaces de red activos
            else if (!_StatusNetworkOn && GetIsMyNetworkAvailable())
            {
                _StatusNetworkOn = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
		public void Start()
		{
		}

        /// <summary>
        /// 
        /// </summary>
		public void End()
		{
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newRtxGroup"></param>
        /// <returns></returns>
		public int HowManySquelchsInRtxGroup(Dictionary<int, RtxState> newRtxGroup)
		{
			int howMany = 0;
			foreach (KeyValuePair<int, RtxState> p in newRtxGroup)
			{
				Debug.Assert(p.Key < _RdPositions.Length);
				RdPosition rd = _RdPositions[p.Key];

				// Sólo las frecuencias que están en la página
				// actual se tienen en cuenta
				if (Top.Cfg.NumFrecByPage * _Page <= p.Key &&
					Top.Cfg.NumFrecByPage * (_Page + 1) > p.Key)
				{
					if (rd.Squelch != SquelchState.NoSquelch)
					{
						howMany++;
					}
				}
			}

			return howMany;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		public int GetRtxGroupPosition(int id)
		{
			Debug.Assert(id < _RdPositions.Length);
			return _RdPositions[id].RtxGroup;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldPage"></param>
        /// <param name="newPage"></param>
        /// <param name="numPosByPage"></param>
		public void SetRdPage(int oldPage, int newPage, int numPosByPage)
		{
			_Page = newPage;

			for (int i = oldPage * numPosByPage, to = Math.Min(_RdPositions.Length, (oldPage + 1) * numPosByPage); i < to; i++)
			{
				_RdPositions[i].SetRx(false);
                _RdPositions[i].SetTx(false, false);
            }

            for (int i = newPage * numPosByPage, to = Math.Min(_RdPositions.Length, (newPage + 1) * numPosByPage); i < to; i++)
            {
                if (_RdPositions[i].Monitoring)
                    _RdPositions[i].SetRx(true);
			}

			Top.WorkingThread.Enqueue("SetSnmp", delegate()
			{
				General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.RadioPageOid, _Page + 1));
			});
		}

                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="on"></param>
        /// <param name="src"></param>
		public void SetPtt(bool on, PttSource src)
		{
			Debug.Assert(src != PttSource.NoPtt);

#if _PTT_FILTER_SIMPLE_
            if (PttFilter.CanProcessEvent(on, src) == false)
                return;
#else
            if (PttFilter.CanProcessEventMult(on, src) == false)
                return;
#endif

            if (on && ((int)src > (int)_PttSource))
			{
                if ((_RdPositionsInTx.Count > 0) && (!Top.Lc.Activity || (Top.Mixer.SplitMode == SplitMode.LcTf)))
				{
#if _NOT_HOLD_ON_PTT_AND_PRIORITY_CALL
                    if ((Top.Tlf.Activity() && !Top.Tlf.PriorityCall && Top.Mixer.SplitMode == SplitMode.Off) ||
                        Top.Lc.HoldedTlf)
                    {
                        _HoldedByPtt = true;
                        General.SafeLaunchEvent(HoldTlfCall, this, new StateMsg<bool>(_HoldedByPtt));
                        System.Threading.Thread.Sleep(50);
                    }
#else
					if (Top.Tlf.Activity() && Top.Mixer.SplitMode == SplitMode.Off)
					{
						HoldedByPtt = true;
						General.SafeLaunchEvent(HoldTlfCall, this, new StateMsg<bool>(HoldedByPtt));
                        System.Threading.Thread.Sleep(50);
					}
#endif
                    SetPtt(src);

					Top.WorkingThread.Enqueue("SetSnmp", delegate()
					{
						string statePtt = "1_" + Top.Cfg.PositionId;
						//SnmpStringObject.Get(Settings.Default.PttOid).Value = statePtt;
						General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.PttOid, statePtt));
					});
				}
				else //if (!Top.Lc.Activity)
				{
					BadOperation(true);
                    if (Top.Recorder.Briefing)
                        Top.Recorder.SessionGlp(FuentesGlp.Briefing, false);
				}

				PttSource = src;
			}
			else if (!on && (_PttSource != PttSource.NoPtt) && ((int)src >= (int)_PttSource))
			{
				if ((_RdPositionsInTx.Count > 0) && (!Top.Lc.Activity || (Top.Mixer.SplitMode == SplitMode.LcTf)))
				{
                    RemovePtt();

					Top.WorkingThread.Enqueue("SetSnmp", delegate()
					{
						string statePtt = "0_" + Top.Cfg.PositionId;
						//SnmpStringObject.Get(Settings.Default.PttOid).Value = statePtt;

						General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.PttOid, statePtt));
					});

                    if (_HoldedByPtt)
						//if (!Top.Tlf.Activity && Top.Tlf.Holded)
					{
                        _HoldedByPtt = false;
                        if (Top.Lc.HoldedTlf == false)
                        {
                            General.SafeLaunchEvent(HoldTlfCall, this, new StateMsg<bool>(_HoldedByPtt));
                        }
					}
				}

				BadOperation(false);
				PttSource = PttSource.NoPtt;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="on"></param>
		public void SetRx(int id, bool on, bool forced)
		{
			Debug.Assert(id < _RdPositions.Length);
			_RdPositions[id].SetRx(on, forced);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="on"></param>
		public void SetTx(int id, bool on)
		{
			Debug.Assert(id < _RdPositions.Length);
			_RdPositions[id].SetTx(on, true);
		}

        /// <summary>
        /// 
        /// </summary>
        public void ForceTxOff(int id)
        {
            Debug.Assert(id < _RdPositions.Length);
            _RdPositions[id].ForceTxOff();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
		public void ConfirmTx(int id)
		{
			Debug.Assert(id < _RdPositions.Length);
			_RdPositions[id].SetTx(true, false);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="audioVia"></param>
		public void SetAudioVia(int id, RdRxAudioVia audioVia)
		{
			Debug.Assert(id < _RdPositions.Length);
			_RdPositions[id].SetAudioVia(audioVia);
		}

        /// <summary>
        /// Cambia al siguiente audio via en Rx, según rotación
        /// </summary>
        /// <param name="id"></param>
        /// <param name="audioVia"></param>
        public void NextAudioVia(int id)
        {
            Debug.Assert(id < _RdPositions.Length);
            _RdPositions[id].SetNextAudioVia();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
		public void SetQuiet(int id)
		{
			Debug.Assert(id < _RdPositions.Length);

			//RemovePtt();
			_RdPositions[id].SetQuiet();
			//_RdPositions[id].SetRx(false);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rtxGroup"></param>
        /// <param name="newRtxGroup"></param>
		public void SetRtxGroup(int rtxGroup, Dictionary<int, RtxState> newRtxGroup)
		{
			Debug.Assert(rtxGroup > 0);

			List<string> frIds = new List<string>();
			List<RtxGroupChangeAsk.ChangeType> changes = new List<RtxGroupChangeAsk.ChangeType>();

			foreach (KeyValuePair<int, RtxState> p in newRtxGroup)
			{
				Debug.Assert(p.Key < _RdPositions.Length);

				// Sólo las frecuencias que están en la página actual se meten en el grupo de retransmisión
				if (Top.Cfg.NumFrecByPage * _Page <= p.Key &&
					Top.Cfg.NumFrecByPage * (_Page + 1) > p.Key)
				{
					RdPosition rd = _RdPositions[p.Key];

					if (rd.Tx)
					{
						frIds.Add(rd.Literal);
						changes.Add((RtxGroupChangeAsk.ChangeType)p.Value);
					}
				}
			}

			Top.Registry.ChangeRtxGroup(rtxGroup, frIds, changes);
		}

        public void ChangSite(int pos, string alias)
        {
                Top.Registry.ChangeSite(_RdPositions[pos].Literal, alias);
        }

        public void PrepareSelCal(bool onOff, string code)
        {
            Top.Registry.PrepareSelCal(onOff, code);
        }

        public void SetScreenSaverStatus(bool on)
        {
            _ScreenSaver = on;
        }

        public string ChangeSite(int id)
        {
            return _RdPositions[id].ChangeAlias();
        }

        /// <summary>
        /// Gestiona el led del altavoz de radio, según se utilice en la conmutación automática
        /// por competición de la telefonía
        /// </summary>
        public void UpdateRadioSpeakerLed()
        {
            Top.Hw.EnciendeLed(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, DetectedAnySquechInSpk(RdRxAudioVia.Speaker) ? HwManager.ON : HwManager.OFF);
            Top.Hw.EnciendeLed(CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER, DetectedAnySquechInSpk(RdRxAudioVia.HfSpeaker) ? HwManager.ON : HwManager.OFF);
        }


        /** 20180716. Acceso a un tono de falsa maniobra radio...*/
        /** 20190206. Un valor -1 Activa el tono hasta que desaparezca la Pulsacion PTT */
        Task generatingToneTask = null;
        public void GenerateBadOperationTone(int durationInMsec)
        {
            if (generatingToneTask == null)
            {
                generatingToneTask = Task.Run(() =>
                {
                    BadOperation(true);
                    if (durationInMsec > 0)
                        Task.Delay(durationInMsec).Wait();
                    else
                    {
                        while (PttSource != PttSource.NoPtt)
                        {
                            Task.Delay(50).Wait();
                        }
                    }
                    BadOperation(false);
                    generatingToneTask = null;
                });
            }
        }

#region Private Members

        /// <summary>
        /// 
        /// </summary>
		private bool _ChangingCfg = false;
		private RdPosition[] _RdPositions = new RdPosition[Radio.NumDestinations];
		private PttSource _PttSource = PttSource.NoPtt;
        private PttSource _OldPttSource = PttSource.NoPtt;
        private bool _AnySquelch = false;
        private Timer _PttTimer = new Timer(Settings.Default.PttCheckTime);
        // Temporizador para que no suene la falsa maniobra en transitorios cortos 
        // (por ejemplo al insertar o extraer los jacks)
        const int DELAY_BAD_OPERATION = 1500;
        private Timer _PttBadOpeTimer = new Timer(DELAY_BAD_OPERATION);
        private List<RdPosition> _RdPositionsInTx = new List<RdPosition>();
		private List<RdPosition> _RdPositionsInPtt = new List<RdPosition>();
		private int _BadOperationTone = -1;
        private int _Page = 0;
        private bool _HoldedByPtt = false;
        private bool _ScreenSaver = false;
        private bool _SiteManager = false;
        private bool _SiteManaging = false;
        // JCAM. 20170314
        //Timer que controla el estado de la red. Si no hay red, resetear las posiciones de radio
        private Timer _TimerNetworkStatus = new Timer(1000);
        private bool _StatusNetworkOn = false;
        private bool _DoubleRadioSpeaker = false;

        // Guarda el ultimo estado global de HF recibido para enviarlo sólo una vez hacia la presentation
        int _LastStatusHF = -1;
        private static Logger _Logger = LogManager.GetCurrentClassLogger();
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void OnPttTimerElapsed(object sender, ElapsedEventArgs e)
		{
			Top.WorkingThread.Enqueue("OnPttTimerElapsed", delegate
			{
				if ((_RdPositionsInPtt.Count != 0) && (_PttSource != PttSource.NoPtt) && 
					(_RdPositionsInTx.Count > 0) && !Top.Lc.Activity)
				{
					PttSource src = PttSource;

					General.SafeLaunchEvent(PTTMaxTime, this);
					BadOperation(true);

				}
			});
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPttBadOpeTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _PttBadOpeTimer.Enabled = false;
            BadOperation(true);
         }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
		private void OnConfigChanged(object sender)
		{
			_ChangingCfg = true;
            try
            {
            _TimerNetworkStatus.Enabled = true;

            _SiteManager = Top.Cfg.SitesConfiguration();

			RangeMsg<RdInfo> rdPositions = new RangeMsg<RdInfo>(0, Radio.NumDestinations);

			foreach (CfgEnlaceExterno link in Top.Cfg.RdLinks)
			{
				foreach (uint hmiPos in link.ListaPosicionesEnHmi)
				{
					uint pos = hmiPos - 1;

					//if (pos < (uint)_RdPositions.Length)
					if (pos < Radio.NumDestinations)
					{
						RdPosition rd = _RdPositions[pos];
						rd.Reset(link);

                            RdInfo posInfo = new RdInfo(rd.Literal, rd.Alias, rd.Tx, rd.Rx, rd.Ptt, rd.Squelch, rd.AudioVia, rd.RtxGroup, rd.TipoFrecuencia, rd.Monitoring, (FrequencyState)rd.Estado, rd.RxOnly);
                            /** 20180321. AGL. ALIAS a mostrar en la tecla... */
                            posInfo.KeyAlias = rd.KeyAlias;
						rdPositions.Info[pos] = posInfo;
					}
				}
			}

			for (int i = 0, to = Radio.NumDestinations; i < to; i++)
			{
				if (rdPositions.Info[i] == null)
				{
					RdPosition rd = _RdPositions[i];
					rd.Reset();

                        RdInfo posInfo = new RdInfo(rd.Literal, rd.Alias, rd.Tx, rd.Rx, rd.Ptt, rd.Squelch, rd.AudioVia, rd.RtxGroup, rd.TipoFrecuencia, rd.Monitoring, (FrequencyState)rd.Estado, rd.RxOnly);
                        /** 20180321. AGL. ALIAS a mostrar en la tecla... */
                        posInfo.KeyAlias = rd.KeyAlias;

					rdPositions.Info[i] = posInfo;
				}
			}
			General.SafeLaunchEvent(NewPositions, this, rdPositions);
		}
            catch (Exception exc)
            {
                _Logger.Error(String.Format("RdManager:OnConfigChanged exception {0}, {1}", exc.Message, exc.StackTrace));
            }
            finally
            {
                _ChangingCfg = false;
            }
		}

        /// <summary>
        /// Tratamiento del evento Ptt pulsado.
        /// Si el PTT procede de un jack o pedal, se comprueba que el jack está insertado.
        /// Si el PTT procede de un jack se comprueba el modo agregado/disgregado de operadores
        /// </summary>
        /// <param name="pttSrc"></param>
        /// <param name="on"></param>
		private void OnPttPulsed(object pttSrc, bool on)
		{
			PttSource src = (PttSource)pttSrc;
			SplitMode mode = Top.Mixer.SplitMode;
            //Me aseguro de quitar la falsa maniobra cuando desaparece un Ptt
            if (on == false)
            {
                _PttBadOpeTimer.Enabled = false;
                BadOperation(false);
            }
            if (((src == PttSource.Instructor) && (Top.Hw.InstructorJack == false)) ||
               ((src == PttSource.Alumn) && (Top.Hw.AlumnJack == false)))
            {
                // Esta falsa maniobara se lanza con retraso para evitar que suene en los 
                // transitorios que hay en la inserción y extracción de los jacks.
                _PttBadOpeTimer.Enabled = on;
                return;
            }

            if (((src == PttSource.Instructor) && (Top.Mixer.InstructorDev >= 0) && ((mode == SplitMode.Off) || (mode == SplitMode.LcTf))) ||
				((src == PttSource.Alumn) && (Top.Mixer.AlumnDev >= 0) && ((mode == SplitMode.Off) || (mode == SplitMode.RdLc))))
			{
				SetPtt(on, src);
                return;
			}
            /** 20171219. AGL. Para la simulacion */
            if (Top.Hw is SimCMediaHwManager)
            {
                if (((src == PttSource.Instructor) && ((mode == SplitMode.Off) || (mode == SplitMode.LcTf))) ||
                    ((src == PttSource.Alumn) && ((mode == SplitMode.Off) || (mode == SplitMode.RdLc))))
                {
                    SetPtt(on, src);
                    return;
                }
            }

			//string statePtt = (on ? "1" : "0") + "_" +
			//                    (on ? src : PttSource.NoPtt);
							
			//SnmpStringObject.Get(Settings.Default.PttOid).Value = statePtt;
		}

        /// <summary>
        /// Se recibe cuando hay un cambio de estado de jacks o de cualquiera de los altavoces radio
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="st"></param>
		private void OnHwChanged(object sender, JacksStateMsg st)
		{
            try
            {
			bool jacks = st.LeftJack || st.RightJack;
            _Logger.Debug("RdManager OnJacksChanged {2}: {0} || {1}", st.LeftJack, st.RightJack, sender.GetType());
            foreach (RdPosition rd in _RdPositions)
			{
                rd.CheckAudioVia();
			}
		}
            catch (Exception e)
            {
                _Logger.Debug("Excepcion. Mensaje: {0}", e.Message);
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
		private void OnLcActivityChanged(object sender)
		{
			if (_PttSource != PttSource.NoPtt)
			{
				if (Top.Lc.Activity)
				{
					BadOperation(false);

					if ((_RdPositionsInTx.Count > 0) && (Top.Mixer.SplitMode != SplitMode.LcTf))
					{
						RemovePtt();
                        
                        if (_HoldedByPtt)                        
                        {
                            //Se ha activado LC. Las llamadas estan aparcadas. Entonces se desaparcan.
                            _HoldedByPtt = false;
                            if (Top.Lc.HoldedTlf == false)
                            {
                                General.SafeLaunchEvent(HoldTlfCall, this, new StateMsg<bool>(_HoldedByPtt));
                            }
                        }
					}
				}
				else
				{
					if (_RdPositionsInTx.Count == 0)
					{
						BadOperation(true);
					}
					else if (Top.Mixer.SplitMode != SplitMode.LcTf)
					{
                        if ((Top.Tlf.Activity() && !Top.Tlf.PriorityCall && Top.Mixer.SplitMode == SplitMode.Off) ||
                            Top.Lc.HoldedTlf)
                        {
                            _HoldedByPtt = true;
                            General.SafeLaunchEvent(HoldTlfCall, this, new StateMsg<bool>(_HoldedByPtt));
                            System.Threading.Thread.Sleep(50);
                        }

						PttSource src = _PttSource;
						_PttSource = PttSource.NoPtt;

						SetPtt(src);

						_PttSource = src;
					}
					else if (_RdPositionsInPtt.Count == 0)
					{
						BadOperation(true);
					}
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="oldMode"></param>
		private void OnSplitModeChanged(object sender, SplitMode oldMode)
		{
			if ((_PttSource != PttSource.NoPtt) && (_RdPositionsInTx.Count > 0) && Top.Lc.Activity)
			{
				if (Top.Mixer.SplitMode != SplitMode.LcTf)
				{
					if (oldMode == SplitMode.LcTf)
					{
						RemovePtt();
					}
				}
				else
				{
					PttSource src = _PttSource;
					_PttSource = PttSource.NoPtt;

					SetPtt(src);

					_PttSource = src;
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
		private void OnRdStateChanged(object sender)
		{
			RdPosition rd = (RdPosition)sender;
            bool _oldAnySquelch = _AnySquelch;
            _AnySquelch = DetectedAnySquech();

            UpdateRadioSpeakerLed();

            //Comentado porque produce sesiones de grabación de 0 segundos
            //if (_oldAnySquelch != _AnySquelch)
            //    Top.Recorder.SessionGlp(FuentesGlp.RxRadio, _AnySquelch);

			if (rd.Tx && !_RdPositionsInTx.Contains(rd))
			{
				_RdPositionsInTx.Add(rd);

				if ((_RdPositionsInTx.Count == 1) && (_PttSource != PttSource.NoPtt) &&  
					(!Top.Lc.Activity || (Top.Mixer.SplitMode == SplitMode.LcTf)))
				{
					PttSource src = _PttSource;
					_PttSource = PttSource.NoPtt;

					SetPtt(src);
#if _BAD_OPERATION_OLD_
                    BadOperation(false);
#endif
					_PttSource = src;
				}
			}
			else if (!rd.Tx && _RdPositionsInTx.Remove(rd) && (_RdPositionsInTx.Count == 0) &&
				(_PttSource != PttSource.NoPtt) && (!Top.Lc.Activity || (Top.Mixer.SplitMode == SplitMode.LcTf)))
			{
				RemovePtt();

				if (!Top.Lc.Activity)
				{
#if _BAD_OPERATION_OLD_
					BadOperation(true);
#endif
				}
			}

			if ((rd.Ptt == PttState.PttOnlyPort) || (rd.Ptt == PttState.PttPortAndMod))
			{
				if (!_RdPositionsInPtt.Contains(rd))
				{
					_RdPositionsInPtt.Add(rd);

					if ((_RdPositionsInPtt.Count == 1) && (_PttSource != PttSource.NoPtt) && 
						(_RdPositionsInTx.Count > 0) && (!Top.Lc.Activity || (Top.Mixer.SplitMode == SplitMode.LcTf)))
					{
                        //_PttTimer.Enabled = false;
#if _BAD_OPERATION_OLD_
                        /** 20190319. No veo la pertinencia de esto, por lo demas, en casos de transmision con bloqueos,
                         aborta precipitadamente la señalizacion acustica de la falsa maniobra. */
						BadOperation(false);
#endif
					}
				}
			}
			else if (_RdPositionsInPtt.Remove(rd) && (_RdPositionsInPtt.Count == 0) && !_PttTimer.Enabled &&
				(_PttSource != PttSource.NoPtt) && (_RdPositionsInTx.Count > 0) &&
				(!Top.Lc.Activity || (Top.Mixer.SplitMode == SplitMode.LcTf)))
			{
				if (!Top.Lc.Activity)
				{
#if _BAD_OPERATION_OLD_
					BadOperation(true);
#endif
				}
			}
			else if ((rd.Ptt == PttState.Blocked || rd.Ptt == PttState.Error) && _PttSource != PttSource.NoPtt)
			{
				if (!Top.Lc.Activity)
				{
#if _BAD_OPERATION_OLD_
					BadOperation(true);
#endif
				}
			}

#if !_BAD_OPERATION_OLD_
            BadOperationManagement();
#endif
            if (!_ChangingCfg)
			{
				RdState st = new RdState(rd.Tx, rd.Rx, rd.PttSrcId, rd.Ptt, rd.Squelch, rd.AudioVia, rd.RtxGroup, 
                                            (FrequencyState)rd.Estado, rd.QidxMethod, rd.QidxValue, rd.QidxResource);
				RangeMsg<RdState> state = new RangeMsg<RdState>(rd.Pos, st);
				General.SafeLaunchEvent(PositionsChanged, this, state);

                /* GRABACION VOIP START */
#region GRABACION VOIP START

                bool IsPttRtx = rd.PttCausadoPorRetransmision();
                if (rd.Tx && (rd.Ptt == PttState.PttOnlyPort || rd.Ptt == PttState.PttPortAndMod || IsPttRtx))
                {
                    //Solo puede haber una posicion con Tx seleccionado y con el PTT activado
                    uint prior = IsPttRtx ? (uint)CORESIP_PttType.CORESIP_PTT_COUPLING : rd.GetPttPriority();
                    if (_PttSource == U5ki.Infrastructure.PttSource.Hmi)
                    {
                        //_Logger.Trace("Starting recording PTT:{0} Frequency:{1} Device: {2}", rd.Ptt, rd.Literal, Top.Mixer.AlumnDev);
                        SipAgent.RdPttEvent(true, rd.Literal, Top.Mixer.AlumnDev, prior);
                        //_Logger.Trace("Starting recording PTT:{0} Frequency:{1} Device: {2}", rd.Ptt, rd.Literal, Top.Mixer.InstructorDev);
                        SipAgent.RdPttEvent(true, rd.Literal, Top.Mixer.InstructorDev, prior);
                    }
                    else
                    {
                        //_Logger.Trace("Starting recording PTT:{0} Frequency:{1} Device: {2}", rd.Ptt, rd.Literal, (_PttSource == U5ki.Infrastructure.PttSource.Alumn ? Top.Mixer.AlumnDev : Top.Mixer.InstructorDev));
                        SipAgent.RdPttEvent(true, rd.Literal, (_PttSource == U5ki.Infrastructure.PttSource.Alumn ? Top.Mixer.AlumnDev : Top.Mixer.InstructorDev), prior);
                    }
                }
                else if (rd.Tx && (rd.Ptt != PttState.PttOnlyPort && rd.Ptt != PttState.PttPortAndMod && !IsPttRtx))
                {
                    //El evento de PTT off solo se puede producir con el Tx seleccionado.
                    //No es posible desactivar la seleccion TX con el PTT activado

                    uint prior = (uint)CORESIP_PttType.CORESIP_PTT_OFF;
                    if (_OldPttSource == U5ki.Infrastructure.PttSource.Hmi)
                    {
                        //_Logger.Trace("Ending recording PTT:{0} Frequency:{1} Device: {2}", rd.Ptt, rd.Literal, Top.Mixer.AlumnDev);
                        SipAgent.RdPttEvent(false, rd.Literal, Top.Mixer.AlumnDev, prior);
                        //_Logger.Trace("Ending recording PTT:{0} Frequency:{1} Device: {2}", rd.Ptt, rd.Literal, Top.Mixer.InstructorDev);
                        SipAgent.RdPttEvent(false, rd.Literal, Top.Mixer.InstructorDev, prior);
                    }
                    else
                    {
                        //_Logger.Trace("Ending recording PTT:{0} Frequency:{1} Device: {2}", rd.Ptt, rd.Literal, (_OldPttSource == U5ki.Infrastructure.PttSource.Alumn ? Top.Mixer.AlumnDev : Top.Mixer.InstructorDev));
                        SipAgent.RdPttEvent(false, rd.Literal, (_OldPttSource == U5ki.Infrastructure.PttSource.Alumn ? Top.Mixer.AlumnDev : Top.Mixer.InstructorDev), prior);
                    }
                }

                if (rd.Rx && (rd.Squelch == SquelchState.SquelchOnlyPort || rd.Squelch == SquelchState.SquelchPortAndMod))
                {
                    //Se considera evento de squelch si la posicion tiene activada en RX y el squelch esta activado

                    // JCAM. 20170323
                    // Adaptación para tratamiento BSS
                    //_Logger.Trace("Starting recording SQUELCH:{0} Frequency:{1} Resource:{2} BssMethod:{3} Qidx:{4}", rd.Squelch, rd.Literal,rd.QidxResource,rd.QidxMethod,rd.QidxValue);
                    if (rd.TipoFrecuencia == TipoFrecuencia_t.FD)  
                    {
                        //Es una frecuencia desplazada
                        SipAgent.RdSquEvent(true, rd.Literal, rd.QidxResource, rd.QidxMethod, rd.QidxValue);
                    }
                    else
                    {   //no es frecuencia desplazada.
                        SipAgent.RdSquEvent(true, rd.Literal, rd.QidxResource, "", 0);
                    }
                }
                else 
                {
                    //Se envia evento de Squelch off al grabador en el caso de que no haya ninguna posicion de esta 
                    //frecuencia en el HMI que tenga squelch on, o que haya desaparecido la selección de Rx en todas
                    //las posiciones de esa frecuencia.

                    bool rx_seleccionado = false;
                    bool squ_on = false;
                    foreach (RdPosition rdpos in _RdPositions)
                    {
                        if (rdpos.Literal != "" && rdpos.Literal == rd.Literal)
                        {
                            if (rdpos.Rx)
                            {
                                //Existe al menos una posicion en el HMI para esta frecuencia con el Rx seleccionado
                                rx_seleccionado = true;
                                if (rdpos.Squelch == SquelchState.SquelchOnlyPort || rdpos.Squelch == SquelchState.SquelchPortAndMod)
                                {
                                    //Existe al menos una posicion en el HMI para esa frecuencia con squ on
                                    squ_on = true;
                                }
                                break;      //Sólo puede haber una posicion con el Rx seleccionado en el HMI
                            }
                        }
                    }

                    if (!rx_seleccionado || !squ_on)
                    {
                        //_Logger.Trace("Ending recording SQUELCH:{0} Frequency:{1} Resource:{2} BssMethod:{3} Qidx:{4}", rd.Squelch, rd.Literal, rd.QidxResource, rd.QidxMethod, rd.QidxValue);
                        if (rd.TipoFrecuencia == TipoFrecuencia_t.FD)
                        {
                            //Es una frecuencia desplazada
                            SipAgent.RdSquEvent(false, rd.Literal, rd.QidxResource, rd.QidxMethod, rd.QidxValue);
                        }
                        else
                        {   //no es frecuencia desplazada.
                            SipAgent.RdSquEvent(false, rd.Literal, rd.QidxResource, "", 0);
                        }
                    }
                }
#endregion
                /* GRABACION VOIP END */

			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void OnRdRxAlreadyAssigned(object sender)
        {
            _AnySquelch = DetectedAnySquech();

            // Si existe squelch por esta posición, empezar grabación
            RdPosition rd = (RdPosition)sender;
            if (rd.Rx && (rd.Squelch == SquelchState.SquelchOnlyPort || rd.Squelch == SquelchState.SquelchPortAndMod))
            {
                // JCAM. 20170323
                // Adaptación para tratamiento BSS
                //_Logger.Debug("Starting recording SQUELCH:{0} Frequency:{1} Resource:{2} BssMethod:{3} Qidx:{4}", rd.Squelch, rd.Literal, rd.QidxResource, rd.QidxMethod, rd.QidxValue);
                SipAgent.RdSquEvent(true, rd.Literal,rd.QidxResource,rd.QidxMethod,rd.QidxValue);
            }
            else if (!rd.Rx)
            {
                // JCAM. 20170323
                // Adaptación para tratamiento BSS
                //_Logger.Debug("Ending recording SQUELCH:{0} Frequency:{1} Resource:{2} BssMethod:{3} Qidx:{4}", rd.Squelch, rd.Literal, rd.QidxResource, rd.QidxMethod, rd.QidxValue);
                SipAgent.RdSquEvent(false, rd.Literal,rd.QidxResource,rd.QidxMethod,rd.QidxValue);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
		private void OnRdTxAlreadyAssigned(object sender)
		{
			RdPosition rd = (RdPosition)sender;

			General.SafeLaunchEvent(TxAssign, this, new RdFrAsignedToOtherMsg(rd.Pos, null));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="owner"></param>
		private void OnRdTxAssignedByOther(object sender, string owner)
		{
			RdPosition rd = (RdPosition)sender;

			General.SafeLaunchEvent(TxAssign, this, new RdFrAsignedToOtherMsg(rd.Pos, owner));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void OnRdTxHfAlreadyAssigned(object sender, uint estado)
        {
            RdPosition rd = (RdPosition)sender;

            General.SafeLaunchEvent(TxHfAssign, this, new RdHfFrAssigned(rd.Pos, estado));
        }

        private void OnRdSelCalPrepareResponse(object sender, string msg)
        {
            if (msg != string.Empty)
            {
                General.SafeLaunchEvent(SelCalMessage, this, new StateMsg<string>(msg));

                if (msg != "Error")  // Prepare SELCAL On
                {
                    if (Settings.Default.SelCalTonesMessagingInfo)
                        // Envío de tonos con  mensaje INFO
                        SendSelCalTonesMessagingInfo(msg);
                    else
                    {
                        // Envío de tonos con fichero
                        if (msg != string.Empty)
                        {
                            string filename = "";
                            SelcalGen selcal = new SelcalGen() { SampleRate = 8000, Gain = 0.7 };
                            string code1 = ((string)msg).Substring(0, 1);
                            string code2 = ((string)msg).Substring(1, 1);
                            string code3 = ((string)msg).Substring(2, 1);
                            string code4 = ((string)msg).Substring(3, 1);

                            if (selcal.Generate(code1, code2, code3, code4, out filename) == true)
                            {
                                //Console.WriteLine("SelCal generado {0} ...", filename + DateTime.Today.ToString());
                                SendSelCalTonesStreamingFile(filename);
                            }
                            //else
                                //Console.WriteLine("Error al generar SelCal...");
                        }
                    }
                }
            }
            else // Fin proceso envío tonos
            {
                if (!Settings.Default.SelCalTonesMessagingInfo)
                {
                    try
                    {
                        // Eliminar el fichero generado
                        foreach (string file in System.IO.Directory.GetFiles(".", "sc_*.wav", System.IO.SearchOption.TopDirectoryOnly))
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                    catch (Exception )
                    {
                    }
                }
            }
        }

        private void OnSiteChangedResult(object sender, ChangeSiteRsp res)
        {
            RdPosition rd = (RdPosition)sender;

            General.SafeLaunchEvent(SiteChangedResultMessage, this, res);
        }

        private void OnAudioViaNotAvailable(object sender, RdRxAudioVia res)
        {
            General.SafeLaunchEvent(AudioViaNotAvailable, this, res);
        }

#if _HF_GLOBAL_STATUS_
        private void OnRdHFGlobalStatus(object sender, HFStatusCodes status)
        {
            if (_LastStatusHF != (int)status)
            {
                _LastStatusHF = (int)status;
            General.SafeLaunchEvent(HFGlobalStatus, this, status);
        }
        }
#endif
        /// <summary>
        /// Envío de tonos SELCAL con mensaje INFO
        /// </summary>
        /// <param name="tones"></param>
        private void SendSelCalTonesMessagingInfo(string tones)
        {
            /** AGL. Poner como fuente SELCAL <Estaba Instructor> */
            PttSource src = PttSource.SelCal;
            SetPtt(true, src);
            System.Threading.Thread.Sleep(Settings.Default.SelCalPttTimeOut);
            Top.Registry.SendTonesSelCal("tones: " + tones);
            System.Threading.Thread.Sleep(2500);
            SetPtt(false, src);

            // Recoger respuesta mensaje INFO para poder notificar prepare SELCAL a Off
            // Pendiente ...
        }

        /// <summary>
        /// Envío de tonos SELCAL con fichero generado
        /// </summary>
        /// <param name="file"></param>
        private void SendSelCalTonesStreamingFile(string file)
        {
            SetPtt(true, PttSource.Instructor);
            SipAgent.UnsendToRemote(Top.Mixer.InstructorDev);

            SipAgent.Wav2Remote(file, Top.HostId, Settings.Default.RdSrvListenIp, Convert.ToInt32(Settings.Default.RdSrvListenPort));
            System.Threading.Thread.Sleep(2500);
            SipAgent.Wav2RemoteEnd(IntPtr.Zero);

            // Notificar para prepare SELCAL a Off
            General.SafeLaunchEvent(SelCalMessage, this, new StateMsg<string>(""));

            SipAgent.SendToRemote(Top.Mixer.InstructorDev, Top.HostId, Settings.Default.RdSrvListenIp, Convert.ToUInt32(Settings.Default.RdSrvListenPort));
            SetPtt(false, PttSource.Instructor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
		private void SetPtt(PttSource src)
		{
			if (src == PttSource.Hmi)
			{
				if (_PttSource != PttSource.Instructor && Top.Mixer.SplitMode != SplitMode.RdLc)
				{
					Top.Mixer.LinkRdInstructorTx();
                    Top.Recorder.SessionGlp(FuentesGlp.TxRadio, true);
                    Top.Recorder.SessionGlp(FuentesGlp.Briefing, false);
                }
				if (_PttSource != PttSource.Alumn && Top.Mixer.SplitMode != SplitMode.LcTf)
				{
					Top.Mixer.LinkRdAlumnTx();
                    Top.Recorder.SessionGlp(FuentesGlp.TxRadio, true);
                    Top.Recorder.SessionGlp(FuentesGlp.Briefing, false);
                }
			}
			else if (src == PttSource.Instructor)
			{
                Top.Recorder.SessionGlp(FuentesGlp.TxRadio, true);
                Top.Recorder.SessionGlp(FuentesGlp.Briefing, false);
                if (_PttSource == PttSource.Alumn)
				{
					Top.Mixer.UnlinkRdAlumnTx();
                    Top.Recorder.SessionGlp(FuentesGlp.TxRadio, false);
                }
				Top.Mixer.LinkRdInstructorTx();
            }
            else if (src == PttSource.Alumn)
			{
				//Debug.Assert(src == PttSource.Alumn);
				Top.Mixer.LinkRdAlumnTx();
                Top.Recorder.SessionGlp(FuentesGlp.TxRadio, true);
                Top.Recorder.SessionGlp(FuentesGlp.Briefing, false);
            }

			Top.Registry.SetPtt(src);
			_PttTimer.Enabled = true;
		}

        /// <summary>
        /// 
        /// </summary>
		private void RemovePtt()
		{
			if ((_PttSource == PttSource.Instructor) || (_PttSource == PttSource.Hmi))
			{
				Top.Mixer.UnlinkRdInstructorTx();
                Top.Recorder.SessionGlp(FuentesGlp.TxRadio, false);
            }
			if ((_PttSource == PttSource.Alumn) || (_PttSource == PttSource.Hmi))
			{
				Top.Mixer.UnlinkRdAlumnTx();
                Top.Recorder.SessionGlp(FuentesGlp.TxRadio, false);
            }

			Top.Registry.SetPtt(PttSource.NoPtt);
			_PttTimer.Enabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="on"></param>
        private void BadOperation(bool on,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
		{
			if (on && (_BadOperationTone == -1))
			{
                //Debug.Assert(!Top.Lc.Activity);
                _Logger.Debug(String.Format("BadOperation TONE Start Playing From {0}:{1}.", caller, lineNumber));
                _BadOperationTone = SipAgent.CreateWavPlayer("Resources/Tones/RdBadOperation.wav", true);
                if (Top.Hw.RdSpeaker)
				  Top.Mixer.Link(_BadOperationTone, MixerDev.SpkRd, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                else if (HFSpeakerAvailable())
                    Top.Mixer.Link(_BadOperationTone, MixerDev.SpkHf, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
            }
			else if (!on && (_BadOperationTone >= 0))
			{
				Top.Mixer.Unlink(_BadOperationTone);
				SipAgent.DestroyWavPlayer(_BadOperationTone);
				_BadOperationTone = -1;
                _Logger.Debug(String.Format("BadOperation TONE End Playing From {0}:{1}.", caller, lineNumber));
            }
        }

        private void BadOperationManagement()
        {
            /** Gestion de Falsa Maniobra*/
            var pttencurso = _PttSource == PttSource.Hmi || _PttSource == PttSource.Instructor || _PttSource == PttSource.Alumn;
            var bloqueados = _RdPositions.Where(p => p.Ptt == PttState.Blocked).Count();
            var enerror = _RdPositions.Where(p => p.Ptt == PttState.CarrierError || p.Ptt == PttState.Error || p.Ptt == PttState.TxError).Count();
            if (pttencurso)
            {
                if (bloqueados > 0 || enerror > 0)
                {
                    // El tono de falsa maniobra debe estar activo....
                    if (!Top.Lc.Activity)
                    {
                        BadOperation(true);
                    }
                }
                else
                {
                    // El tono de falsa maniobra no debe estar activo.
                    BadOperation(false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool DetectedAnySquech()
        {
            bool AnySquelchDetected = false;


            for (int i = _Page * (int)Top.Cfg.NumFrecByPage, to = ((_Page + 1) * (int)Top.Cfg.NumFrecByPage); i < to; i++)
            {
                if (_RdPositions[i].AnySquelch &&
                    _RdPositions[i].Ptt != PttState.PttOnlyPort &&
                    _RdPositions[i].AudioVia != RdRxAudioVia.NoAudio)
                {
                    AnySquelchDetected = true;
                    break;
                }

            }

            return AnySquelchDetected;
        }

        /// <summary>
        /// Devuelve true si hay alguna RdPosition con SQ y saliendo por el altavoz
        /// </summary>
        /// <param>speaker altavoz por el que se quiere buscar</param>
        /// <returns></returns>
        private bool DetectedAnySquechInSpk(RdRxAudioVia speaker)
        {
            bool AnySquelchDetected = false;
            
            for (int i = _Page * (int)Top.Cfg.NumFrecByPage, to = ((_Page + 1) * (int)Top.Cfg.NumFrecByPage); i < to; i++)
            {       
                if (_RdPositions[i].AnySquelch &&
                    // Está selecionado altavoz o hay salida de radio a altavoz automatica 
                    // por competir con la telefonía aunque esté seleccionados cascos
                    _RdPositions[i].InSpeaker(speaker))
                {
                    AnySquelchDetected = true;
                    break;
                }
            }
            return AnySquelchDetected;
        }

#endregion
	}
#region PTT-FILTER
    /** */
    /** 20180509. Para Filtrar Eventos PTT Rapidos */
    class PttFilter
    {
#if _PTT_FILTER_SIMPLE_
        static bool lastProcessedPttState = false;
        static bool lastNotifiedPttState = false;
        static PttSource lastNotifiedPttSource = PttSource.NoPtt;
        static PttSource lastProcessedPttSource = PttSource.NoPtt;
        static Task PttMinDelayTask = null;
#endif
        static int PttMinTime = Properties.Settings.Default.CwpPttFilterMsec;
        static object Locker = new object();
#if _PTT_FILTER_SIMPLE_
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ptt"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public static bool CanProcessEvent(bool ptt, PttSource src)
        {
            if (PttMinTime <= 10)
                return true;

            lock (Locker)
            {
                lastNotifiedPttState = ptt;
                lastNotifiedPttSource = src;

                if (PttMinDelayTask == null)
                {
                    lastProcessedPttState = ptt;
                    lastProcessedPttSource = src;
                    PttMinDelayTask = Task.Factory.StartNew(() =>
                    {
                        Task.Delay(PttMinTime).Wait();
                        lock (Locker)
                        {
                            var change = lastNotifiedPttState != lastProcessedPttState || lastNotifiedPttSource != lastProcessedPttSource;
                            if (change)
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    Task.Delay(10).Wait();
                                    Top.WorkingThread.Enqueue("SetRdPtt", delegate ()
                                    {
                                        Top.Rd.SetPtt(lastNotifiedPttState, lastNotifiedPttSource);
                                    });
                                });
                            }
                            PttMinDelayTask = null;
                        }
                    });
                    return true;
                }
                return false;
            }
        }

#endif

        public class PttFilterEventData
        {
            public Task Task { get; set; }
            public bool LastNotification { get; set; }
            public bool LastProcessed { get; set; }
        }
        public static Dictionary<PttSource, PttFilterEventData> PttFilterControl = new Dictionary<PttSource, PttFilterEventData>()
        {
            {PttSource.Instructor, new PttFilterEventData(){ Task=null, LastNotification=false, LastProcessed=false } },
            {PttSource.Alumn, new PttFilterEventData(){ Task=null, LastNotification=false, LastProcessed=false } },
            {PttSource.Hmi, new PttFilterEventData(){ Task=null, LastNotification=false, LastProcessed=false } }
        };
        public static bool CanProcessEventMult(bool ptt, PttSource src)
        {
            if (PttMinTime <= 10)
                return true;

            if (src != PttSource.Alumn && src != PttSource.Instructor && src != PttSource.Hmi)
                return true;

            lock (Locker)
            {
                PttFilterControl[src].LastNotification = ptt;
                if (PttFilterControl[src].Task == null)
                {
                    PttFilterControl[src].LastProcessed = ptt;
                    PttFilterControl[src].Task = Task.Factory.StartNew(() =>
                    {
                        Task.Delay(PttMinTime).Wait();
                        lock (Locker)
                        {
                            var change = PttFilterControl[src].LastNotification != PttFilterControl[src].LastProcessed;
                            if (change)
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    Task.Delay(10).Wait();
                                    Top.WorkingThread.Enqueue("SetRdPtt", delegate ()
                                    {
                                        Top.Rd.SetPtt(PttFilterControl[src].LastNotification, src);
                                    });
                                });
                            }
                            PttFilterControl[src].Task = null;
                        }
                    });
                    return true;
                }
                return false;
            }
        }
    }
#endregion PTT-FILTER
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Timers;

using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;
using NLog;

namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// 
    /// </summary>
	class TlfManager
	{
        /// <summary>
        /// 
        /// </summary>
		public event GenericEventHandler ActivityChanged;
		public event GenericEventHandler<RangeMsg<TlfInfo>> NewPositions;
		public event GenericEventHandler<RangeMsg<TlfState>> PositionsChanged;
		public event GenericEventHandler<RangeMsg<TlfIaDestination>> IaPositionsChanged;
		public event GenericEventHandler<bool> HangToneChanged;
		public event GenericEventHandler<RangeMsg<string>> ConfListChanged;
		public event GenericEventHandler<StateMsg<string>> CompletedIntrusion;
        public event GenericEventHandler<StateMsg<string>> IntrudeToStateEngine;
		public event GenericEventHandler<StateMsg<string>> BegeningIntrudeTo;
		public event GenericEventHandler<StateMsg<string>> IntrudedTo;
		public event GenericEventHandler<SnmpStringMsg<string, string>> SetSnmpString;
		public event GenericEventHandler<SnmpStringMsg<string, string>> SendSnmpTrapString;
        public event GenericEventHandler<RangeMsg<LlamadaHistorica>> HistoricalOfLocalCallsEngine;

        public enum TlfRxAudioVia
        {
            NoAudio,
            HeadPhones,
            Speaker,
        }

        /// <summary>
        /// 
        /// </summary>
		public List<TlfPosition> ActiveCalls
		{
			get { return _ActiveCalls; }
		}

        /// <summary>
        /// 
        /// </summary>
		public TlfTransfer Transfer
		{
			get { return _Transfer; }
		}

        /// <summary>
        /// 
        /// </summary>
		public TlfListen Listen
		{
			get { return _Listen; }
		}

        /// <summary>
        /// 
        /// </summary>
		public bool Activity()
		{
                return (_ActiveCalls.Count > 0);
        }
        /// <summary>
        /// 
        /// </summary>
		public bool Holded
		{
			get { return _Holded; }
			private set
			{
				if (_Holded != value)
				{
					_Holded = value;
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        public bool PriorityCall
        {
            get { return Activity() && _PriorityCall; }
            set { _PriorityCall = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		public TlfPosition this[int id]
		{
			get 
			{
				Debug.Assert(id < _TlfPositions.Length);
				return _TlfPositions[id];
			}
		}

        /// <summary>
        /// 
        /// </summary>
        public bool HayConferencia
        {
            get
            {
                return _Conference != null && _Conference.Members.Count > 0 && _Conference.ConferenceState == ConfState.Executing;
            }
        }


        /// <summary>
        /// 
        /// </summary>
		public void Init()
		{
			Top.Cfg.ConfigChanged += OnConfigChanged;
			Top.Sip.TlfCallStateChanged += OnCallStateChanged;
			Top.Sip.IncomingTlfCall += OnIncomingCall;
			Top.Sip.TlfTransferRequest += OnTransferRequest;
			Top.Sip.TlfCallConfInfo += OnConfInfoReceived;
			Top.Sip.InfoReceived += OnInfoReceived;
            
			for (int i = 0, to = Tlf.NumDestinations; i < to; i++)
			{
				_TlfPositions[i] = new TlfPosition(i);
				_TlfPositions[i].StateChanged += OnTlfStateChanged;
			}

			for (int i = Tlf.NumDestinations, to = Tlf.NumDestinations + Tlf.NumIaDestinations; i < to; i++)
			{
				_TlfPositions[i] = new TlfIaPosition(i);
				_TlfPositions[i].StateChanged += OnTlfIaStateChanged;
			}
            _ToneTimer = new Timer(1000);
            _ToneTimer.AutoReset = false;
            _ToneTimer.Elapsed += OnToneEnd;

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
        /// Gestiona las llamadas cuando proceden de un destino configurado 
        /// o de la 15+1 si tiene configuración
        /// </summary>
        /// <param name="id"></param>
        /// <param name="prio"></param>
		public void Call(int id, bool prio)
		{
			Debug.Assert(id < Tlf.NumDestinations+1);
			TlfPosition tlf = _TlfPositions[id];
            if ((id == Tlf.NumDestinations) && (tlf.Cfg == null))
                return;

			switch (tlf.State)
			{
				case TlfState.Idle:
				case TlfState.PaPBusy:
					tlf.Call(prio);
					//if (tlf.State != TlfState.Idle)	// Si no tiene permiso para hacer la llamada el estado es Idle y no tiene que cambiar el audio
						ResetActiveCalls(tlf);
					break;

				case TlfState.In:
				case TlfState.InPrio:
				case TlfState.RemoteIn:
					tlf.Accept(null);
					ResetActiveCalls(tlf);
					break;
				case TlfState.NotAllowed:
					tlf.HangUp(0);
					ResetActiveCalls(tlf);
					break;

			}
		}

        /// <summary>
        /// Funcion que gestiona la llamada cuando es externa
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="dst"></param>
        /// <param name="number"></param>
        /// <param name="prio"></param>
		public void Call(uint prefix, string dst, string number, bool prio, string literal)
		{
			ResetActiveCalls(null);

			foreach (TlfPosition tlf in _TlfPositions)
			{
				if (tlf.CanHandleOutputCall(prefix, dst, number,literal))
				{
					switch (tlf.State)
					{
						case TlfState.Unavailable:
						case TlfState.PaPBusy:
						case TlfState.Idle:
							tlf.Call(prio);
							//if (tlf.State != TlfState.Idle)	// Si no tiene permiso para hacer la llamada el estado es Idle y no tiene que cambiar el audio
							//	ResetActiveCalls(tlf);
							break;
						case TlfState.In:
						case TlfState.InPrio:
						case TlfState.RemoteIn:
							tlf.Accept(null);
							break;
						case TlfState.Hold:
							tlf.Unhold();
							break;
						case TlfState.NotAllowed:
							tlf.HangUp(0);
							//ResetActiveCalls(tlf);
							break;
						default:
							Debug.Assert("Estado inesperado ejecutando llamada a numero" == null);
							break;
					}

					//if (tlf.State != TlfState.Idle)	// Si no tiene permiso para hacer la llamada el estado es Idle y no tiene que cambiar el audio
					AddActiveCall(tlf);
					return;
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
		public void RetryCall(int id)
		{
			Debug.Assert(id < _TlfPositions.Length);
			TlfPosition tlf = _TlfPositions[id];

			switch (tlf.State)
			{
				case TlfState.OutOfService:
				case TlfState.Congestion:
				case TlfState.Busy:
					tlf.RetryCall();
					break;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="conference"></param>
		public void Accept(int id, TlfConference conference)
		{
			Debug.Assert(id < _TlfPositions.Length);
			TlfPosition tlf = _TlfPositions[id];

			switch (tlf.State)
			{
				case TlfState.In:
				case TlfState.InPrio:
				case TlfState.RemoteIn:
					tlf.Accept(conference);
					ResetActiveCalls(tlf);
					break;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="on"></param>
        public void Hold(bool on)
        {
            foreach (TlfPosition tlf in _TlfPositions)
            {
                if (on &&  (tlf.State == TlfState.Conf || tlf.State == TlfState.Set))
                {
                    tlf.Hold(false);
                }
                else if (_Conference != null &&
                         tlf.State == TlfState.Hold && 
                         !on)
                {
                    tlf.Unhold();
                    //ResetActiveCalls(tlf);
                    IfNotExistsActiveCalls(tlf);
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="on"></param>
		public void Hold(int id, bool on)
		{
			Debug.Assert(id < _TlfPositions.Length);
			TlfPosition tlf = _TlfPositions[id];
            if (!on)
                tlf.HoldOnEstablish = on;

			switch (tlf.State)
			{
				case TlfState.Set:
				case TlfState.Conf:
				case TlfState.RemoteHold:
					if (on)
					{
						tlf.Hold(false);
					}
                    else
					{
						tlf.Unhold();
					}
					break;
				case TlfState.Hold:
					if (!on)
					{
						tlf.Unhold();
					}
					break;
                case TlfState.Out:
                    //Aparco en diferido, cuando se establezca la llamada
                    tlf.HoldOnEstablish = on;
                    break;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
		public void HangUp(int id)
		{
			Debug.Assert(id < _TlfPositions.Length);
			_TlfPositions[id].HangUp(0);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
		public void RecognizeTlfState(int id)
		{
			Debug.Assert(id < _TlfPositions.Length);

			// Esta funcion es para quitar Mem y RemoteMem, que no manejamos aquí (si lo hace la OPE)
			if (_TlfPositions[id].State == TlfState.Idle)
			{
				if (id < Tlf.NumDestinations)
				{
					RangeMsg<TlfState> state = new RangeMsg<TlfState>(id, TlfState.Idle);
					General.SafeLaunchEvent(PositionsChanged, this, state);
				}
				else
				{
					TlfIaPosition tlfIa = (TlfIaPosition)_TlfPositions[id];
					TlfIaDestination st = new TlfIaDestination(tlfIa.Literal, tlfIa.Number, tlfIa.State);
					RangeMsg<TlfIaDestination> state = new RangeMsg<TlfIaDestination>(tlfIa.Pos, st);
					General.SafeLaunchEvent(IaPositionsChanged, this, state);
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public void SetHangToneOff()
		{
			if (_HangUpTone >= 0)
			{
				Top.Mixer.Unlink(_HangUpTone);
				SipAgent.DestroyWavPlayer(_HangUpTone);
				_HangUpTone = -1;

				General.SafeLaunchEvent(HangToneChanged, this, false);

			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="on"></param>
        public void HoldConference(bool on)
        {
            Debug.Assert(_Conference != null);

            _Conference.Hold(on);

            Hold(on);
        }
        public void setShortTone(int time, String toneFile)
        {
            _Logger.Debug("Starting tones: " + toneFile);
            _IntrusionTone = SipAgent.CreateWavPlayer("Resources/Tones/" + toneFile, true);
            Top.Mixer.LinkTlf(_IntrusionTone, MixerDir.Send, Mixer.TLF_PRIORITY);

            _ToneTimer.Interval = time;
            _ToneTimer.Start();

        }
        public void OnToneEnd(object sender, ElapsedEventArgs e)
        {
            if (_IntrusionTone != -1)
            {
                Top.Mixer.Unlink(_IntrusionTone);
                SipAgent.DestroyWavPlayer(_IntrusionTone);
                _IntrusionTone = -1;
                _Logger.Debug("Ending call tones");
            }
            _ToneTimer.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        public void MakeConference()
        {
            Debug.Assert(_Conference == null || _Conference.ConferenceState == ConfState.Hold);

            if (_Conference == null)
                _Conference = new TlfConference();
            else if (_Conference.ConferenceState == ConfState.Hold)
                ResetConference();

            foreach (TlfPosition tlf in _TlfPositions)
            {
                try
                {
                    if (tlf.State == TlfState.Set || tlf.State == TlfState.Hold)
                    {
                        if (_Conference.TryAddExisting(tlf))
                            IfNotExistsActiveCalls(tlf);
                    }
                }
                catch (Exception ex)
                {
                    _Logger.Error("ERROR estableciendo conferencia " + tlf.Literal, ex);
                }
            }
        }

        /// <summary>
        /// HangUpConf: 
        /// Se llama cuando se pulsa la tecla conferencia, entonces hay que colgar las llamadas en conferencia.
      /// </summary>
        public void HangUpConf()
        {
            List<TlfPosition> activeCalls = new List<TlfPosition>(_ActiveCalls);
            foreach (TlfPosition actTlf in activeCalls)
            {
                if (actTlf.Conference != null)
                {
                    actTlf.Conference.HangUp();
                    break;
                }
             }

            // PosInConference = 0;
            if (_Conference != null)
                _Conference.Dispose();
            _Conference = null;
        }

        /// <summary>
        /// HangUpAll: 
        /// Se llama cuando se pulsa la tecla anular y estamos en intrusion. 
        /// Pero estas llamadas no tienen conferencia, asi que tenemos que borrar todas las llamadas
        /// </summary>
        public void EndTlfAll()
        {
            List<TlfPosition> activeCalls = new List<TlfPosition>(_ActiveCalls);
            foreach (TlfPosition actTlf in activeCalls)
            {
                if (actTlf.Conference != null)
                {
                    actTlf.Conference.HangUp();
                    break;
                }
                actTlf.HangUp(0);
            }

            // PosInConference = 0;
            if (_Conference != null)
                _Conference.Dispose();
            _Conference = null;
        }

        /// <summary>
        /// Extrae el display name de una uri. Retorna null si no tiene
        /// </summary>
        /// <param name="uri"></param>
        public static string GetDisplayName(string uri)
        {
            string display_name = null;
            int dn_start = uri.IndexOf('\"');  //Obtiene indice de las primeras dobles comillas
            int sip_idx = uri.IndexOf('<');    //Obtiene indice de la uri sin display name
            if (sip_idx == -1)
            {
                sip_idx = uri.IndexOf("sip:");
                if (sip_idx == -1)
                {
                    sip_idx = uri.IndexOf("sips:");
                    if (sip_idx == -1)
                    {
                        sip_idx = uri.IndexOf("tel:");
                    }
                }
            }
            if (dn_start >= 0 && sip_idx > dn_start)
            {
                //Las primeras dobles comillas tienen que tener un indice inferior que el literal sip:
                int dn_end = uri.IndexOf('\"', dn_start + 1);   //Indice de las segundas dobles comillas
                if (dn_end >= 0 && sip_idx > dn_end)
                {
                    //Las segundas dobles comillas tienen que tener un indice inferior que el literal sip:
                    display_name = uri.Substring(dn_start + 1, dn_end - (dn_start + 1));
                }
            }
            return display_name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="dst"></param>
        /// <param name="numberWithPrefix"></param>
		public static void ExtractIaInfo(CfgRecursoEnlaceInterno rs, out string dst, out string numberWithPrefix)
		{
            string numero = null;
			switch (rs.Prefijo)
			{
				case Cd40Cfg.INT_DST:
                    dst = rs.NombreRecurso;
                    if (String.IsNullOrEmpty(rs.NumeroAbonado))
                        numero = Top.Cfg.GetNumeroAbonado(rs.NombreRecurso, Cd40Cfg.ATS_DST);
                    else
                        numero = rs.NumeroAbonado;
					if (numero != null)
					{
                        string[] lit;

                        lit = Top.Cfg.GetUserFromAddress(Cd40Cfg.ATS_DST, numero);    // Buscar el literal del sector/agrupación asociado al número ATS del sector
                        if (lit != null)
                            dst = lit[0];   // Nombre de la agrupación si el sector estuviera integrado con otro/s

                        numberWithPrefix = string.Format("{0:D2}{1}", Cd40Cfg.ATS_DST, numero);
					}
					else
					{
						numberWithPrefix = string.Format("{0:D2}{1}", rs.Prefijo, rs.NombreRecurso);
					}
					break;
				case Cd40Cfg.PP_DST:
					if (string.IsNullOrEmpty(rs.NumeroAbonado))
					{
						dst = rs.NombreRecurso;
						numberWithPrefix = string.Format("{0:D2}{1}", rs.Prefijo, rs.NombreRecurso);
					}
					else
					{
						dst = rs.NumeroAbonado;
						//number = string.Format("{0:D2}{1}@{2}", rs.Prefijo, rs.NombreRecurso, rs.NumeroAbonado);
                        numberWithPrefix = string.Format("{0:D2}{1}", rs.Prefijo, rs.NumeroAbonado);
					}
					break;
                case Cd40Cfg.IP_DST:                   
                case Cd40Cfg.UNKNOWN_DST:
                    dst = rs.NombreRecurso;
                    numberWithPrefix = string.Format("{0:D2}{1}", rs.Prefijo, rs.NumeroAbonado);
                    break;
                // Pruebas lineas prefijos RTB por el plugin, que lleguen con prefijo en el user de la uri
                //case Cd40Cfg.RTB_DST:
                //    if (rs.NumeroAbonado.StartsWith("0" + rs.Prefijo.ToString()))
                //    {
                //        dst = rs.NumeroAbonado.Substring(2);
                //        numberWithPrefix = rs.NumeroAbonado;
                //        numeroIncluyePrefijo = true;
                //        break;
                //    }
                //    goto default;
                default:
					dst = rs.NumeroAbonado;
					numberWithPrefix = string.Format("{0:D2}{1}", rs.Prefijo, rs.NumeroAbonado);
					break;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="dst"></param>
        /// <param name="literal"></param>
        /// <param name="rs"></param>
		public static void EncapsuleIaInfo(uint prefix, string dst, out string literal, out CfgRecursoEnlaceInterno rs)
		{
			rs = new CfgRecursoEnlaceInterno();
			rs.Prefijo = prefix;

			switch (prefix)
			{
				case Cd40Cfg.INT_DST:
					rs.NombreRecurso = dst;
					literal = rs.NombreRecurso;
					break;
                // Por acceso indirecto, el prefijo PP
                // se comporta como un abonado con marcación
                /*
				case Cd40Cfg.PP_DST:
					string[] rsNum = dst.Split('@');
					rs.NombreRecurso = rsNum[0];
					literal = rs.NombreRecurso;
					if (rsNum.Length > 1)
					{
						rs.NumeroAbonado = rsNum[1];
						literal = rs.NumeroAbonado;
					}
					break;
                 */
                case Cd40Cfg.UNKNOWN_DST:
                    rs.NombreRecurso = dst.IndexOf('@') != -1 ? dst.Substring(0, dst.IndexOf('@')) : dst;
                    int index = rs.NombreRecurso.IndexOf("sip:");
                    rs.NombreRecurso = rs.NombreRecurso.IndexOf("sip:") != -1 ? rs.NombreRecurso.Substring("sip:".Length) : rs.NombreRecurso;
                    rs.NumeroAbonado = dst;
                    literal = rs.NombreRecurso;
                    break;
                case Cd40Cfg.IP_DST:
                    rs.NombreRecurso = dst.IndexOf('@') != -1 ? dst.Substring(0, dst.IndexOf('@')) : dst;
                    index = rs.NombreRecurso.IndexOf("sip:");
                    rs.NombreRecurso = rs.NombreRecurso.IndexOf("sip:") != -1 ? rs.NombreRecurso.Substring("sip:".Length) : rs.NombreRecurso;
                    rs.NumeroAbonado = dst;
                    literal = rs.NumeroAbonado;
                    break;
				default:
					rs.NumeroAbonado = dst;
					literal = rs.NumeroAbonado;
					break;
			}
		}

        /// <summary>
        /// Guarda el atributo de salida del audio para telefonía
        /// </summary>
        /// <param name="speaker"> true if speaker for telephone selected</param>
        //public void SetAudioVia(bool speaker)
        //{
        //    if (speaker)
        //        _RxAudioVia = TlfRxAudioVia.Speaker;
        //    else
        //        _RxAudioVia = TlfRxAudioVia.HeadPhones;
        //}

		#region Private Members

        /// <summary>
        /// 
        /// </summary>
		class TransferRequest
		{
			public TlfPosition By;
			public TlfPosition To;

			public TransferRequest(TlfPosition by, TlfPosition to)
			{
				By = by;
				To = to;
			}
		}

        /// <summary>
        /// 
        /// </summary>
		private static Logger _Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        private bool _PriorityCall = false;
		private bool _Holded = false;
		private bool _ChangingCfg = false;
		private TlfPosition[] _TlfPositions = new TlfPosition[Tlf.NumDestinations + Tlf.NumIaDestinations];
		private List<TlfPosition> _ActiveCalls = new List<TlfPosition>();
		private TlfTransfer _Transfer = new TlfTransfer();
		private TlfListen _Listen = new TlfListen();
		private List<TlfIntrusion> _Intrusions = new List<TlfIntrusion>();
		private TransferRequest _TransferRequest = null;
		private int _HangUpTone = -1;
		private int _IntrusionTone = -1;
		private Dictionary<TlfPosition, CORESIP_ConfInfo> _ConfInfos = new Dictionary<TlfPosition, CORESIP_ConfInfo>();
        private TlfConference _Conference = null;
        // private int PosInConference = 0;
        private Timer _ToneTimer;
        //private TlfRxAudioVia _RxAudioVia = TlfRxAudioVia.NoAudio;

        /// <summary>
        /// 
        /// </summary>
        private void ResetConference()
        {
            // Si hacemos el Dispose, hay que pulsar dos veces al botón de Conferencia para que esta se restablezca (¿?)
            //_Conference.Dispose();
            //_Conference = null;

            _Conference = new TlfConference();

            foreach (TlfPosition actTlf in _ActiveCalls)
            {
                if (actTlf.Conference != null)
                {
                    actTlf.Conference.HangUp();
                }
            }
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
			RangeMsg<TlfInfo> tlfPositions = new RangeMsg<TlfInfo>(0, Tlf.NumDestinations);
			RangeMsg<TlfIaDestination> tlfIaPositions = new RangeMsg<TlfIaDestination>(Tlf.NumDestinations, Tlf.NumIaDestinations);

			foreach (CfgEnlaceInterno link in Top.Cfg.TlfLinks)
			{
				int pos = (int)link.PosicionHMI - 1;

				if (pos < Tlf.NumDestinations)
				{
					TlfPosition tlf = _TlfPositions[pos];

					tlf.Reset(link);

                    TlfInfo posInfo = new TlfInfo(tlf.Literal, tlf.State, tlf.ChAllowsPriority());
					tlfPositions.Info[pos] = posInfo;
				}
			}

			for (int i = 0, to = Tlf.NumDestinations; i < to; i++)
			{
				if (tlfPositions.Info[i] == null)
				{
					TlfPosition tlf = _TlfPositions[i];

					tlf.Reset();

                    TlfInfo posInfo = new TlfInfo(tlf.Literal, tlf.State, tlf.ChAllowsPriority());
					tlfPositions.Info[i] = posInfo;
				}
			}

			for (int i = Tlf.NumDestinations, to = Tlf.NumDestinations + Tlf.NumIaDestinations; i < to; i++)
			{
				TlfIaPosition tlf = (TlfIaPosition)_TlfPositions[i];

				tlf.Update();

				TlfIaDestination posInfo = new TlfIaDestination(tlf.Literal, tlf.Number, tlf.State);
				tlfIaPositions.Info[i - Tlf.NumDestinations] = posInfo;
			}
			General.SafeLaunchEvent(NewPositions, this, tlfPositions);
			General.SafeLaunchEvent(IaPositionsChanged, this, tlfIaPositions);
		}
            catch
            {
                throw;
            }
            finally
            {
                _ChangingCfg = false;
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="call"></param>
        /// <param name="stateInfo"></param>
		private void OnCallStateChanged(object sender, int call, CORESIP_CallStateInfo stateInfo)
		{
            /* AGL. Captura Excepciones para marcarlas en el LOG */
                foreach (TlfPosition tlf in _TlfPositions)
                {
                    try
                    {
                        if (tlf.HandleChangeInCallState(call, stateInfo))
                        {
                            List<LlamadaHistorica> listaLlamadas = HistoricalManager.GetHistoricalCalls(Top.Cfg.PositionId);

                            General.SafeLaunchEvent(HistoricalOfLocalCallsEngine, this, new RangeMsg<LlamadaHistorica>(listaLlamadas.ToArray()));

                            break;
                        }
                    }
                    catch (Exception x)
                    {
                        _Logger.Error(String.Format("TlfManager:OnCallStateChanged: {0},{1},{2},{3}", tlf.Literal, stateInfo.State, stateInfo.LastCode,stateInfo.MediaStatus), x);
                    }
                }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="call"></param>
        /// <param name="call2replace"></param>
        /// <param name="info"></param>
        /// <param name="inInfo"></param>
        /// <param name="answer"></param>
		private void OnIncomingCall(object sender, int call, int call2replace, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo, CORESIP_Answer answer)
		{
			TlfPosition replace = null;
             _Logger.Debug("OnIncomingCall: " + inInfo.SrcId);

			if (call2replace >= 0)
			{
				if ((_ActiveCalls.Count > 1) || 
					((_ActiveCalls.Count == 1) && (_ActiveCalls[0].CallId != call2replace)))
				{
					answer.Value = SipAgent.SIP_BUSY;
					return;
				}

				if (_ActiveCalls.Count == 1)
				{
					replace = _ActiveCalls[0];
				}
				else
				{
					// La llamada a sustituir esta aparcada
					foreach (TlfPosition tlf in _TlfPositions)
					{
						if (tlf.CallId == call2replace)
						{
							replace = tlf;
							break;
						}
					}
				}

				if (replace == null)
				{
					// Nunca deberíamos llegar aquí, pero nos curamos en salud
					Debug.Assert(replace != null);
					SipAgent.HangupCall(call2replace, SipAgent.SIP_GONE);
				}
			}

			foreach (TlfPosition tlf in _TlfPositions)
			{
				answer.Value = tlf.HandleIncomingCall(call, call2replace, info, inInfo);
				if (answer.Value != SipAgent.SIP_DECLINE)
				{
					if (answer.Value == SipAgent.SIP_RINGING)
					{
                        // Se pide en ENNA que se quite el estado "en espera de cuelgue con una llamada entrante
                        // TODO: confirmar
                        // SetHangToneOff();
						if (replace != null)
						{
							AddActiveCall(tlf);
							replace.HangUp(SipAgent.SIP_GONE);

							answer.Value = SipAgent.SIP_OK;
							return;
						}

						if (Activity() && (tlf.State == TlfState.InPrio) &&
							((Top.Cfg.Permissions & Permissions.Intruded) == Permissions.Intruded))
						{
							foreach (TlfPosition tlfPos in _TlfPositions)
							{
								if ((tlfPos != tlf) && (tlfPos.State == TlfState.InPrio))
								{
									return;
								}
							}
							foreach (TlfPosition tlfPos in _ActiveCalls)
							{
								if (tlfPos.EfectivePriority == CORESIP_Priority.CORESIP_PR_EMERGENCY)
								{
									return;
								}
							}

							TlfIntrusion intrusion = new TlfIntrusion(tlf);
							intrusion.Finished += OnIntrusionFinished;
							_Intrusions.Add(intrusion);

							answer.Value = SipAgent.SIP_QUEUED;

							StateMsg<string> state = new StateMsg<string>(tlf.Literal);
							General.SafeLaunchEvent(CompletedIntrusion, this, state);

							Top.WorkingThread.Enqueue("SetSnmp", delegate()
							{
								string snmpString = Top.Cfg.PositionId + "_" + "PRIORITY" + "_" + tlf.Literal;
								General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.TlfFacilityOid, snmpString));
							});
						}
					}
					else if ((answer.Value & 0x0000FFFF) == SipAgent.SIP_OK)
					{
						AddActiveCall(tlf);
					}

					return;
				}
			}
            _Logger.Warn("Rechazada llamada entrante {0}@{1} no path o no tiene permiso de red", inInfo.SrcId, inInfo.SrcIp);
			answer.Value = SipAgent.SIP_BUSY;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="call"></param>
        /// <param name="info"></param>
        /// <param name="transferInfo"></param>
        /// <param name="answer"></param>
		private void OnTransferRequest(object sender, int call, CORESIP_CallInfo info, CORESIP_CallTransferInfo transferInfo, CORESIP_Answer answer)
		{
            int SIPanswer = SipAgent.SIP_BUSY;

			if ((_ActiveCalls.Count == 1) && (_ActiveCalls[0].CallId == call))
			{
				TlfPosition by = _ActiveCalls[0];

				if (by.CallId == call)
				{
                    //Valores por defecto si es un recurso externo no configurado
                    uint prefix = Cd40Cfg.UNKNOWN_DST;
                    string dst = transferInfo.DstId;
                    string number = string.Format("sip:{0}@{1}", transferInfo.DstId, transferInfo.DstIp);
                    bool unknownResource = true;
                    string literal = transferInfo.DstId;

                    //Extraemos el display name del Refer-To. Si lo tiene                     
                    string referto_display_name = GetDisplayName(transferInfo.ReferTo);
                    
					if (!string.IsNullOrEmpty(transferInfo.DstSubId))
					{
						prefix = Cd40Cfg.ATS_DST;
						dst = transferInfo.DstSubId;
						number = string.Format("{0:D2}{1}", prefix, dst);
                        unknownResource = false;
					}
                    else
					{
                        CfgRecursoEnlaceInterno rs = Top.Cfg.GetResourceFromUri(transferInfo.DstId, transferInfo.DstIp, transferInfo.DstSubId, transferInfo.DstRs);
                        if (rs != null)
						{
                            ExtractIaInfo(rs, out dst, out number);
						    prefix = rs.Prefijo;
                            literal = dst;
                            unknownResource = false;
					    }
					}
					foreach (TlfPosition tlf in _TlfPositions)
					{
                        if (tlf.Pos >= Tlf.NumDestinations)
                        {
                            //La llamada saldria por 19+1. El literal seria el display name
                            if (referto_display_name != null && referto_display_name.Length > 0)
                            {
                                literal = referto_display_name;
                            }
                        }
                         
                        if (tlf.CanHandleOutputCall(prefix, dst, number, literal, unknownResource))
						{
                            // Se admite cualquier transferencia si tiene AD configurado con el colateral 
                            // o, no teniéndolo (AID) no es un destino PP. (#2272) 
                            // Se quita la restricción por #3264
                            //if (!(tlf is TlfIaPosition) || prefix != Cd40Cfg.PP_DST)
                            //{
                                SipAgent.TransferAnswer(transferInfo.TsxKey, transferInfo.TxData, transferInfo.EvSub, SipAgent.SIP_ACCEPTED);
                                answer.Value = 0;

                                tlf.Call(transferInfo);
                                AddActiveCall(tlf);
                                by.Hold(true);

                                if (tlf.State != TlfState.Congestion && tlf.State != TlfState.OutOfService)
                                {
                                    Debug.Assert(_TransferRequest == null);
                                    _TransferRequest = new TransferRequest(by, tlf);
                                }

                                return;
                            //}
                            //else
                            //    // JCAM. 23/12/2016.
                            //    // Antes se devolvía SIP_BUSY pero en un escenario en el que se recibe una transferencia directa
                            //    // de una línea PaP con la que el puesto no tiene AD, debe devolve DECLINE.
                            //    // Tenerlo en cuenta para otros posible escenarios
                            //    SIPanswer = SipAgent.SIP_DECLINE;
						}
					}
				}
			}

            answer.Value = SIPanswer;
		}

         /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="call"></param>
        /// <param name="info"></param>
        /// <param name="lenInfo"></param>
 
		private void OnInfoReceived(object sender, int call, string info, uint lenInfo)
		{
            if (info == TlfIntrusion.INTRUSION_IN_PROGRESS)
			{
                   setShortTone(1000, "Warning_Operator_Intervening.wav");
			}
			else if (info.StartsWith("Intruido por "))
			{
				foreach (TlfPosition tlf in _TlfPositions)
				{
					if (tlf.CallId == call)
					{
						StateMsg<string> stateIntrusion = new StateMsg<string>(info.Remove(0,"Intruido por ".Length));
						General.SafeLaunchEvent(IntrudedTo, this, stateIntrusion);
						break;
					}
				}
			}
            else if (info == TlfIntrusion.INTRUSION_COMPLETED)
            {
                // Si se ha interrumpido una llamada por prioridad hay que eliminar el tono de interrupción por prioridad
                if (_IntrusionTone != -1)
                {
                    Top.Mixer.Unlink(_IntrusionTone);
                    SipAgent.DestroyWavPlayer(_IntrusionTone);
                    _IntrusionTone = -1;
                }

                // Eliminar el mensaje de "Intervenido por:"
                General.SafeLaunchEvent(IntrudeToStateEngine, this, null); ;
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="call"></param>
        /// <param name="info"></param>
		private void OnConfInfoReceived(object sender, int call, CORESIP_ConfInfo info)
		{
			foreach (TlfPosition tlf in _TlfPositions)
			{
				if (tlf.CallId == call)
				{
					bool publish = true;

					if (string.Compare(info.State, "deleted", true) == 0)
					{
						publish = _ConfInfos.Remove(tlf);
					}
					//else if (string.Compare(info.State, "partial", true) == 0)
					//{
					//}
					else
					{
						_ConfInfos[tlf] = info;
					}

					if (publish)
					{
						PublishConfList();
					}
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
		private void OnTlfStateChanged(object sender)
		{
            TlfPosition tlf = (TlfPosition)sender;
			TlfState st = tlf.State;

			TlfStateChanged(tlf, st);

			if (!_ChangingCfg)
			{
				if ((st == TlfState.Idle) && tlf.RemoteHangUp)
				{
					if ((tlf.OldState == TlfState.In) || (tlf.OldState == TlfState.InPrio))
					{
						st = TlfState.Mem;
					}
					else if (tlf.OldState == TlfState.RemoteIn)
					{
						st = TlfState.RemoteMem;
					}
				}

                MensajesDeIntrusion(tlf);

				if ((tlf.State == TlfState.Out && tlf.OldState == TlfState.Idle) ||	// Llamada saliente
					(tlf.State == TlfState.Set) ||									// Conversacion
					(tlf.State == TlfState.Idle && (tlf.OldState == TlfState.Out || tlf.OldState == TlfState.Set)) ||	// Fin llamada
					(tlf.State == TlfState.In && tlf.OldState == TlfState.Idle))	// Llamada entrante
				{
					string snmpOid = string.Empty;
					switch (tlf.State)
					{
						case TlfState.Set:
							snmpOid = Settings.Default.EstablishedTfCallOid;
							break;
						case TlfState.Out:
							snmpOid = Settings.Default.OutgoingTfCallOid;
							break;
						case TlfState.Idle:
							snmpOid = Settings.Default.EndingTfCallOid;
							break;
						case TlfState.In:
							snmpOid = Settings.Default.IncommingTfCallOid;
							break;
					}

					Top.WorkingThread.Enqueue("SendSnmpTrap", delegate()
					{
						string snmpString = Top.Cfg.PositionId + "_" + "TF" + "_" + tlf.Literal + "_" + tlf.State.ToString();
						General.SafeLaunchEvent(SendSnmpTrapString, this, new SnmpStringMsg<string, string>(snmpOid, snmpString));
					});
				}

				RangeMsg<TlfState> state = new RangeMsg<TlfState>(tlf.Pos, st);
				General.SafeLaunchEvent(PositionsChanged, this, state);
			}
		}

        private void MensajesDeIntrusion(TlfPosition tlf)
        {
            if (tlf.OldState == TlfState.Out &&
                (tlf.State == TlfState.Conf) &&
                tlf.EfectivePriority == CORESIP_Priority.CORESIP_PR_EMERGENCY)
            {
                //Inicio intrusión
                StateMsg<string> stateIntrusion = new StateMsg<string>(tlf.Literal);
                General.SafeLaunchEvent(BegeningIntrudeTo, this, stateIntrusion);
            }
            else if ((tlf.OldState == TlfState.Conf && tlf.State == TlfState.Idle) || // caso de intrusion en puesto
                     (tlf.OldState == TlfState.Conf && tlf.State == TlfState.PaPBusy)) //  caso de intrusión a teléfono IP
            {
                //Se deshace la intrusión
                General.SafeLaunchEvent(BegeningIntrudeTo, this, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
		private void OnTlfIaStateChanged(object sender)
		{
			TlfIaPosition tlf = (TlfIaPosition)sender;
			TlfState st = tlf.State;

			TlfStateChanged(tlf, st);

			if (!_ChangingCfg)
			{
				if ((st == TlfState.Idle) && tlf.RemoteHangUp && !string.IsNullOrEmpty(tlf.Number))
				{
					if ((tlf.OldState == TlfState.In) || (tlf.OldState == TlfState.InPrio))
					{
						st = TlfState.Mem;
					}
					else if (tlf.OldState == TlfState.RemoteIn)
					{
						st = TlfState.RemoteMem;
					}
				}

                MensajesDeIntrusion(tlf);
				TlfIaDestination iaSt = new TlfIaDestination(tlf.Literal, tlf.Number, st);
				RangeMsg<TlfIaDestination> state = new RangeMsg<TlfIaDestination>(tlf.Pos, iaSt);
				General.SafeLaunchEvent(IaPositionsChanged, this, state);


				if ((tlf.State == TlfState.Out && tlf.OldState == TlfState.Idle) ||	// Llamada saliente
					(tlf.State == TlfState.Set) ||									// Conversacion
					(tlf.State == TlfState.Idle && (tlf.OldState == TlfState.Out || tlf.OldState == TlfState.Set)) ||	// Fin llamada
					(tlf.State == TlfState.In && tlf.OldState == TlfState.Idle))	// Llamada entrante
				{
					string snmpOid = string.Empty;
					switch (tlf.State)
					{
						case TlfState.Set:
							snmpOid = Settings.Default.EstablishedTfCallOid;
							break;
						case TlfState.Out:
							snmpOid = Settings.Default.OutgoingTfCallOid;
							break;
						case TlfState.Idle:
							snmpOid = Settings.Default.EndingTfCallOid;
							break;
						case TlfState.In:
							snmpOid = Settings.Default.IncommingTfCallOid;
							break;
					}

					Top.WorkingThread.Enqueue("SendSnmpTrap", delegate()
					{
						string snmpString = Top.Cfg.PositionId + "_" + "AI" + "_" + iaSt.Number + "_" + iaSt.State.ToString();
						General.SafeLaunchEvent(SendSnmpTrapString, this, new SnmpStringMsg<string, string>(snmpOid, snmpString));
					});
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
		private void OnIntrusionFinished(object sender)
		{
			_Intrusions.Remove((TlfIntrusion)sender);

			General.SafeLaunchEvent(CompletedIntrusion, this, null);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tlf"></param>
		private void ResetActiveCalls(TlfPosition tlf)
		{
            bool oldActivityState = Activity();
            if (_ActiveCalls.Count > 0)
			{
				List<TlfPosition> activeCalls = new List<TlfPosition>(_ActiveCalls);
				TlfConference tlfConference = (tlf != null ? tlf.Conference : null);

				_ActiveCalls.Clear();

				foreach (TlfPosition actTlf in activeCalls)
				{
					if (actTlf != tlf)
					{
						if ((actTlf.Conference == null) || (actTlf.Conference != tlfConference))
						{
							actTlf.HangUp(0);
						}
						else
						{
							_ActiveCalls.Add(actTlf);
						}
					}
				}
			}

			if (tlf != null)
			{
				_ActiveCalls.Add(tlf);
				SetHangToneOff();
			}
            PublishChangeActivity(oldActivityState, Activity());
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tlf"></param>
		private void AddActiveCall(TlfPosition tlf)
		{
            bool oldActivityState = Activity();
			_ActiveCalls.Add(tlf);
			SetHangToneOff();
            PublishChangeActivity(oldActivityState, Activity());
		}

        private void PublishChangeActivity(bool oldActivityState, bool newActivityState)
        {
            if (oldActivityState != newActivityState)
                General.SafeLaunchEvent(ActivityChanged, this);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tlf"></param>
        private void IfNotExistsActiveCalls(TlfPosition tlf)
        {
            if (!_ActiveCalls.Contains(tlf))
                AddActiveCall(tlf);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tlf"></param>
		private void RemoveActiveCall(TlfPosition tlf)
		{
            bool oldActivityState = Activity();
            if (_ActiveCalls.Remove(tlf) && (_ActiveCalls.Count == 0))
			{
                // Puede ser que la llamada finalizada estuviera siendo interrumpida
                // en cuyo caso hay que eliminar los tonos de intervención
                if (_IntrusionTone != -1)
                {
                    Top.Mixer.Unlink(_IntrusionTone);
                    SipAgent.DestroyWavPlayer(_IntrusionTone);
                    _IntrusionTone = -1;
                
                    // Eliminar el mensaje de "Intervenido por:"
                    General.SafeLaunchEvent(IntrudeToStateEngine, this, null); ;
                }

				if (tlf.RemoteHangUp && (_HangUpTone == -1) &&
					((tlf.OldState == TlfState.Set) || (tlf.OldState == TlfState.Conf) || (tlf.OldState == TlfState.RemoteHold)))
				{
					// Si se ha interrumpido una llamada por prioridad hay que eliminar el tono de interrupción por prioridad
					if (_IntrusionTone != -1)
					{
						Top.Mixer.Unlink(_IntrusionTone);
						SipAgent.DestroyWavPlayer(_IntrusionTone);
						_IntrusionTone = -1;
					}

					// La telefonía sigue activa mientras esté el tono de colgado.
					_HangUpTone = SipAgent.CreateWavPlayer("Resources/Tones/HangUp.wav", true);
					Top.Mixer.LinkTlf(_HangUpTone, MixerDir.Send, Mixer.TLF_PRIORITY);

					General.SafeLaunchEvent(HangToneChanged, this, true);
				}
                else if (tlf.OldState != TlfState.NotAllowed || tlf.State != TlfState.Idle)
                {
                }
			}
            // Si la llamada intervenida estaba aparcada, al culminarse la intrusión
            // el _ActiveCall.Count == 0 y el tono de intrusión hay que quitarlo.
            else if (_ActiveCalls.Count == 0 && _IntrusionTone != -1)
            {
                Top.Mixer.Unlink(_IntrusionTone);
                SipAgent.DestroyWavPlayer(_IntrusionTone);
                _IntrusionTone = -1;

                // Eliminar el mensaje de "Intervenido por:"
                General.SafeLaunchEvent(IntrudeToStateEngine, this, null); ;
            }
            PublishChangeActivity(oldActivityState, Activity());
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tlf"></param>
        /// <param name="st"></param>
		private void TlfStateChanged(TlfPosition tlf, TlfState st)
		{
            switch (st)
			{
				case TlfState.Idle:
				case TlfState.Unavailable:
				case TlfState.PaPBusy:
				case TlfState.Hold:
					RemoveActiveCall(tlf);
					Holded = st == TlfState.Hold;
					break;
                case TlfState.Set:
                //Se hace después de la respuesta de CORESIP para 
                //evitar el desorden en hold on/off muy rapidos
                    if (tlf.OldState == TlfState.Hold)
                        ResetActiveCalls(tlf);
                    break;
			}

			if (_ConfInfos.ContainsKey(tlf))
			{
				switch (st)
				{
					case TlfState.Idle:
					case TlfState.Unavailable:
					case TlfState.PaPBusy:
						_ConfInfos.Remove(tlf);
						PublishConfList();
						break;
					case TlfState.Hold:
					case TlfState.RemoteHold:
						PublishConfList();
						break;
					case TlfState.Set:
						_ConfInfos.Remove(tlf);
						PublishConfList();
						break;
				}
			}

			if (_TransferRequest != null)
			{
				if (tlf == _TransferRequest.To)
				{
					TlfPosition by = _TransferRequest.By;
					_TransferRequest = null;

					if (!Activity() && (by != null) && (by.State == TlfState.Hold))
					{
						by.Unhold();
						ResetActiveCalls(by);
					}
				}
				else if (tlf == _TransferRequest.By)
				{
					switch (st)
					{
						case TlfState.Unavailable:
						case TlfState.Idle:
						case TlfState.PaPBusy:
							_TransferRequest.By = null;
							break;
					}
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
		private void PublishConfList()
		{
			List<string> confList = new List<string>();
            String ipAddr; // not used
            General.SafeLaunchEvent(ConfListChanged, this, null);
			foreach (KeyValuePair<TlfPosition, CORESIP_ConfInfo> p in _ConfInfos)
			{
				if (p.Key.State == TlfState.Conf)
				{
					for (int i = 0; i < p.Value.UsersCount ; i++ )
					{
                        // Entre los participantes de la conferencia viene mi identidad
                        // pero no tengo que incluirla en los textos informativos
                        if (esMiNumeroAbonado(p.Value.Users[i].Id) == false)
                        {
                            // El nombre que se envia para el texto informativo es (por orden):
                            // el literal configurado si existe,
                            // el texto que viene en el mensaje sip display-text si existe
                            // el numero de aboando que viene en la uri, si no existen los anteriores
                            string nombre = traduceANombreConocido(p.Value.Users[i].Id);
                            if (string.IsNullOrEmpty(nombre) == true)
                                 if (string.IsNullOrEmpty(p.Value.Users[i].Name) == false)
                                     nombre = p.Value.Users[i].Name;
                                 else
                                     nombre = getNumeroAbonado(p.Value.Users[i].Id, out ipAddr);

                            confList.Add(nombre);
                            if (p.Value.Users[i].Role.Equals("Intruder") == true)
                            {
                                General.SafeLaunchEvent(CompletedIntrusion, this, new StateMsg<string>(nombre));
                            }
                        }
					}
				}
			}
            if (confList.Count == 0)
                General.SafeLaunchEvent(CompletedIntrusion, this, null);
			General.SafeLaunchEvent(ConfListChanged, this, new RangeMsg<string>(confList.ToArray()));
		}

        /// <summary>
        /// Busca si la uri dada pertenece a uno de los numeros de abonado del sector que tengo asignado
        /// </summary>
        /// <param name="Uri" uri que contiene el numero de abonado en formato sip:xxx@yyy o tel:zzzz></param>
        /// <returns>true si es uno de mis numeros de abonado, false en caso contrario</returns>
        private bool esMiNumeroAbonado(String Uri)
        {
            char[] elimina = {'<','>'};
            bool sipUri = true;
            String miNumero;
            String ipAddr = null;
            String numUri = getNumeroAbonado (Uri, out ipAddr);
            if (Uri.Contains("tel:"))
            {
                sipUri = false;
            }

            foreach (StrNumeroAbonado num in Top.Cfg.HostAddresses)
            {
                if (sipUri)
                {
                    miNumero = num.NumeroAbonado;
                }
                else
                {
                    miNumero = string.Format("{0:D2}{1}", num.Prefijo, num.NumeroAbonado);
                }
                if (numUri == miNumero)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Devuelve el numero de abonado de la uri dada, en formato tel o sip.
        /// </summary>
        /// <param name="Uri" uri que contiene el numero de abonado en formato sip:xxx@yyy o tel:zzzz></param>
        /// <returns>String que contiene el numero de abonado</returns>
        private String getNumeroAbonado(String Uri, out String ipAddr) 
        {
            char[] elimina = { '<', '>' };
            String numAbonado = "";
            ipAddr = null;
            try
            {
                String trimmedUri = Uri.Trim(elimina);
                int fin = trimmedUri.IndexOf('@');
                if (fin == -1)
                {
                    fin = trimmedUri.Length;
                }
                else 
                    ipAddr = trimmedUri.Substring(fin+1);
                int ini = trimmedUri.IndexOf(':');
                if (ini != -1)
                {
                    numAbonado = trimmedUri.Substring(ini + 1, fin - (ini + 1));
                }
                else
                    _Logger.Warn("Warn identificando la uri de miembros conferencia " + Uri);
            }
            catch (Exception exc)
            {
                _Logger.Error("ERROR identificando los miembros de la conferencia " + Uri, exc);
            }

            return numAbonado;
        }
        /// <summary>
        /// Traduce el nombre de la uri dada a un nombre conocido localmente
        /// </summary>
        /// <param name="Uri" uri que contiene el numero de abonado></param>
        /// <returns>String que contiene el nombre local o cadena vacia</returns>
        private String traduceANombreConocido(String Uri)
        {
            String nombreConocido =String.Empty;
            String ipAddr = null;
            String numAbonado = getNumeroAbonado(Uri, out ipAddr);
            foreach (TlfPosition tlfPos in _TlfPositions)
            {
                if ((tlfPos.Cfg != null) && (tlfPos.NumeroAbonado.Contains(numAbonado)))
                {
                    nombreConocido = tlfPos.Literal;
                    break;
                }
            }

            return nombreConocido;
        }

		#endregion
	}
}

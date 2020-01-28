#define _HF_GLOBAL_STATUS_
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using HMI.Model.Module.BusinessEntities;

using Utilities;
using U5ki.Infrastructure;
using NLog;

namespace HMI.CD40.Module.BusinessEntities
{
    class RdPosition
	{
		public event GenericEventHandler StateChanged;
		public event GenericEventHandler TxAlreadyAssigned;
        public event GenericEventHandler RxAlreadyAssigned;
        public event GenericEventHandler<uint> TxHfAlreadyAssigned;
		public event GenericEventHandler<string> TxAssignedByOther;
        public event GenericEventHandler<string> SelCalPrepareResponse;
        public event GenericEventHandler<ChangeSiteRsp> SiteChangedResult;
        public event GenericEventHandler<RdRxAudioVia> AudioViaNotAvailable;

#if _HF_GLOBAL_STATUS_
        public event GenericEventHandler<HFStatusCodes> HfGlobalStatus;
#endif

		public int Pos
		{
			get { return _Pos; }
		}

		public string Literal
		{
			get { return _Literal; }
		}

		public string Alias
		{
			get { return _Alias; }
            set { _Alias = value; }
		}

        /** 20180321. AGL. Alias Mostrado */
        public string KeyAlias { get { return _KeyAlias; } }

		public bool Tx
		{
			get { return (_Tx == AssignState.Set); }
			private set
			{
				bool oldTx = (_Tx == AssignState.Set);
				_Tx = value ? AssignState.Set : AssignState.Idle;

				if (oldTx != value)
				{
					General.SafeLaunchEvent(StateChanged, this);
				}
			}
		}

		public bool Rx
		{
			get { return (_Rx == AssignState.Set); }
			private set
			{
				bool oldRx = (_Rx == AssignState.Set);
				_Rx = value ? AssignState.Set : AssignState.Idle;

				if (oldRx != value)
				{
					General.SafeLaunchEvent(StateChanged, this);
				}
			}
		}
        public bool Monitoring
        {
            get { return _Monitoring; }
		}

		public PttState Ptt
		{
			get { return _Ptt; }
			private set
			{
				if (_Ptt != value)
				{
					_Ptt = value;
					General.SafeLaunchEvent(StateChanged, this);
				}
			}
		}

		public SquelchState Squelch
		{
			get { return _Squelch; }
			private set
			{
				if (_Squelch != value)
				{
					_Squelch = value;
					General.SafeLaunchEvent(StateChanged, this);
				}
			}
		}
        public bool AnySquelch
        {
            get { return ((_Squelch != SquelchState.NoSquelch) && (_Squelch != SquelchState.Unavailable)); }
        }

		public RdRxAudioVia AudioVia
		{
			get { return _AudioVia; }
			private set
			{
				if (_AudioVia != value)
				{
					_AudioVia = value;
					_RealAudioVia = value;

					General.SafeLaunchEvent(StateChanged, this);
				}
			}
		}

		public int RtxGroup
		{
			get { return _RtxGroup; }
			private set
			{
				if (_RtxGroup != value)
				{
					_RtxGroup = value;
					General.SafeLaunchEvent(StateChanged, this);
				}
			}
		}

		public bool Available
		{
			get { return (_Ptt != PttState.Unavailable) && (_Squelch != SquelchState.Unavailable); }
		}

        public TipoFrecuencia_t TipoFrecuencia
        {
            get { return _TipoFrecuencia; }
            set { _TipoFrecuencia = value; }
        }

        public RdSrvFrRs.FrequencyStatusType Estado
        {
            get { return _Estado; }
            set 
            {
                if (_Estado != value)
                {
                    _Estado = value;
                    General.SafeLaunchEvent(StateChanged, this);
                }
            }
        }

        // BSS information
        public string QidxMethod
        {
            get { return _QidxMethod; }
        }

        public uint QidxValue
        {
            get { return _QidxValue; }
        }

        public string QidxResource
        {
            get { return _QidxResource; }
        }

        public bool RxOnly
        {
            get { return _RxOnly; }
        }

        /** 20190205. RTX Information */
        public string PttSrcId { get { return _PttSrcId; } }

        public RdPosition(int pos)
		{
			_Pos = pos;
            _PttOffTimer.Interval = 100;
            _PttOffTimer.AutoReset = false;
            _PttOffTimer.Elapsed += OnPttOffTimer;
		}

		public void Reset()
		{
            /** 20180626 Redmine #3610 Forzar las asignaciones a OFF cuando desaparecen por configuracion y hay PTT o SQUELCH en la Posicion */
            if (_Literal != "")
                ResetTxAndRxAsg();
            else
                SetRx(false);

			_Alias = _Literal = _KeyAlias = "";
			_Ptt = PttState.Unavailable;
            _PttSrcId = string.Empty;
			_Squelch = SquelchState.Unavailable;
			_AudioVia = RdRxAudioVia.NoAudio;
			_RealAudioVia = RdRxAudioVia.NoAudio;
			_RtxGroup = 0;
			_AssociateFrRs = null;
			_AssociateRxRs.Clear();
			_AssociateTxRs.Clear();
            _TipoFrecuencia = TipoFrecuencia_t.Basica;
            _Estado = RdSrvFrRs.FrequencyStatusType.NotAvailable;
            _RxOnly = false;
		}

		public void Reset(CfgEnlaceExterno cfg)
		{
			uint oldPriority = _Priority;
            SquelchState oldSquelch = Squelch;

			if (string.Compare(cfg.Literal, _Literal, true) != 0)
			{
				Reset();

				_AssociateFrRs = Top.Registry.GetRs<RdSrvFrRs>(cfg.Literal);

				if (_AssociateFrRs.IsValid)
				{
					RdSrvFrRs frRs = _AssociateFrRs.Info;

                    _Alias =  frRs.SqSite;
					_Squelch = oldSquelch = (SquelchState)frRs.Squelch;
					_Ptt = GetPtt(frRs.PttSrcId);
					_RtxGroup = 0;

					if (frRs.RtxGroupId != 0)
					{
						_RtxGroup = (frRs.RtxGroupOwner != Top.HostId ? -1 : (int)frRs.RtxGroupId);
					}
				}
			}
            else
            {
                _AssociateRxRs.Clear();
                _AssociateTxRs.Clear();
            }

			_AssociateFrRs.NewMsg += OnFrMsg;
			_AssociateFrRs.Changed += OnFrChanged;
            _AssociateFrRs.SelCalMsg += OnSelCalMsg;
            _AssociateFrRs.SiteChanged += OnSiteChanged;

			_Literal = cfg.Literal;
            /** 20180321. AGL. ALIAS a mostrar en la tecla... */
            var Alias = cfg.GetType().GetProperty("Alias");
            _KeyAlias = Alias == null ? "NoAlias" : Alias.GetValue(cfg) as string;
            // _KeyAlias = cfg.Alias;
			//_Alias = cfg.ListaRecursos.Count > 0 ? cfg.ListaRecursos[0].IdEmplazamiento : "";
			_Priority = cfg.Prioridad;
            _TipoFrecuencia = (TipoFrecuencia_t)cfg.TipoFrecuencia;
            _Monitoring = cfg.EstadoAsignacion == "M";
            _RxOnly = false;

            switch (cfg.EstadoAsignacion)
            {
                case "M":   // Monitor
                    Rx = true;
                    Tx = false;
                    AudioVia = RdRxAudioVia.Speaker;
                    SetTx(false, false);
                    break;
                case "T":   // Trafico
                    if (!Tx)
                        SetTx(true, false);
                    AudioVia = RdRxAudioVia.Speaker;
                    break;
                default:    // Reposo
                    break;
            }

            _RscSite.Clear();
			foreach (CfgRecursoEnlaceExterno dst in cfg.ListaRecursos)
			{
                _RscSite.Add(dst.IdRecurso, dst.IdEmplazamiento);

                // Se añaden todos los recursos que con el estado 
                // Estado=="S"elected o Estado=="A"ctivo
				if (dst.Estado == "S" || dst.Estado == "A")
				{
                    if (dst.Estado == "S")
                    {
                        if (_Alias != dst.IdEmplazamiento)
                        {
                            Squelch = oldSquelch;
                            _Alias = dst.IdEmplazamiento;
                        }
                    }
                    //_Alias = dst.Estado == "S" ?  dst.IdEmplazamiento : _Alias;

					if ((dst.Tipo == Cd40Cfg.RD_RX) || (dst.Tipo == Cd40Cfg.RD_RXTX))
					{
						Rs<RdSrvRxRs> rs = Top.Registry.GetRs<RdSrvRxRs>(dst.IdRecurso);
						rs.Changed += OnFrRxChanged;

						_AssociateRxRs.Add(rs);
					}
					if ((dst.Tipo == Cd40Cfg.RD_TX) || (dst.Tipo == Cd40Cfg.RD_RXTX))
					{
						Rs<RdSrvTxRs> rs = Top.Registry.GetRs<RdSrvTxRs>(dst.IdRecurso);
						//rs.Changed += OnFrTxChanged;

						_AssociateTxRs.Add(rs);
					}
				}
			}
            if ((_AssociateTxRs.Count == 0) && (_AssociateRxRs.Count > 0) && _TipoFrecuencia != TipoFrecuencia_t.HF)
                _RxOnly = true;
 
			if (_Rx == AssignState.Set)
			{
				Dictionary<string, int> portsToRemove = new Dictionary<string, int>(_RxPorts);

				foreach (Rs<RdSrvRxRs> rs in _AssociateRxRs)
				{
					string rsId = rs.Id.ToUpper();

					if (rs.IsValid && !portsToRemove.Remove(rsId))
					{
                        _Logger.Debug("*** M+N. Reset(sender). Llamando a CreateRdRxPort({0},{1})", ((RdSrvRxRs)rs.Content).ToString(),Top.SipIp);
                        CreateRxAudio(rs, rsId);
					}
                    else
                        _Logger.Debug("*** M+N. Reset(sender). rs.IsValid && !portsToRemove.Remove(rsId) es false)");
				}
				foreach (KeyValuePair<string, int> p in portsToRemove)
				{
					Top.Mixer.Unlink(p.Value);
                    _Logger.Debug("*** M+N. RxOff(sender). Llamando a DestroyRdRxPort({0})", p.Value);

					SipAgent.DestroyRdRxPort(p.Value);

					_RxPorts.Remove(p.Key);
				}
			}
			if ((_Tx != AssignState.Idle) && (_Priority != oldPriority))
			{
				Top.Registry.SetTx(_Literal, true, _Priority, _Tx == AssignState.Trying);
			}
		}

		public void SetRx(bool on, bool forced = false)
		{
			if (Available && (forced || Top.Rd.PttSource == PttSource.NoPtt))
			{
				if (on && (_Rx != AssignState.Set))
				{
					_Rx = AssignState.Trying;
					Top.Registry.SetRx(_Literal, true);
				}
				else if (!on && (_Rx != AssignState.Idle))
				{
					if (_Rx == AssignState.Set)
					{
						RxOff();
					}

					Rx = false;
					Top.Registry.SetRx(_Literal, false);
				}
			}
		}

		public void SetTx(bool on, bool checkAlreadyAssigned)
		{
            if (Available && Top.Rd.PttSource == PttSource.NoPtt &&  !Top.Rd.ScreenSaverStatus)
			{
				if (on && (_Tx != AssignState.Set))
				{
					_Tx = AssignState.Trying;
                    if (_Rx == AssignState.Idle)
                        _Rx = AssignState.Trying;
					Top.Registry.SetTx(_Literal, true, _Priority, checkAlreadyAssigned);
				}
				else if (!on && (_Tx != AssignState.Idle))
				{
					_RtxGroup = Math.Min(_RtxGroup, 0);
					Tx = false;
					Top.Registry.SetTx(_Literal, false, _Priority, false);
				}
			}
		}

        public void ForceTxOff()
        {
            if (Available)
            {
                if (_Tx != AssignState.Idle)
                {
                    _RtxGroup = Math.Min(_RtxGroup, 0);
                    Tx = false;
                    Top.Registry.SetTx(_Literal, false, _Priority, false);
                }
            }
        }

        /** 20180626 Redmine #3610 Forzar las asignaciones a OFF cuando desaparecen por configuracion y hay PTT o SQUELCH en la Posicion */
        public void ResetTxAndRxAsg()
        {
            if (_Rx != AssignState.Idle)
            {
                if (_Rx == AssignState.Set)
                {
                    RxOff();
                }

                Rx = false;
                Top.Registry.SetRx(_Literal, false);
            }
        }


        /// <summary>
        /// Redirige al audio hacia el destino indicado (altavoz o auricular) con las siguientes condiciones:
        /// 1-Si no hay PTT, se conecta el audio siempre.
        /// 2-Si hay PTT propio no se hace nada, no está permitido cambiar la salida del audio.
        /// 3-Si hay PTT que procede de una RTX de otro SQ y estoy en el puesto propietario del grupo y el SQ es propio, 
        /// no conecto mi audio porque se debe escuchar el que se está retransmitiendo.
        /// </summary>
        /// <param name="audioVia"></param>
        public void SetAudioVia(RdRxAudioVia audioVia)
		{

            if (/*Rx && */ (_AudioVia != audioVia) &&
                ((audioVia == RdRxAudioVia.Speaker && Top.Hw.RdSpeaker) ||
                 (audioVia == RdRxAudioVia.HfSpeaker && Top.Hw.HfSpeaker) ||
                 (audioVia == RdRxAudioVia.HeadPhones &&
                 (Top.Hw.InstructorJack || Top.Hw.AlumnJack) && !Top.Mixer.ModoSoloAltavoces)))
            {
                if (InhiboMiAudio(_Ptt, _AssociateFrRs.Info) == false)
                {
                    foreach (int port in _RxPorts.Values)
                    {
                        Top.Mixer.Unlink(port);
                        MixerDev dev = (audioVia == RdRxAudioVia.HeadPhones ? MixerDev.MhpRd : (audioVia == RdRxAudioVia.HfSpeaker ? MixerDev.SpkHf : MixerDev.SpkRd));
                        Top.Mixer.Link(port, dev, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                    }
                }

                AudioVia = audioVia;
            }
		}

        /// <summary>
        /// Redirige al audio hacia el proximo destino según rotación
        /// </summary>
        /// <param name="audioVia"></param>
        public void SetNextAudioVia()
        {
            RdRxAudioVia newAudio = NextRxAudioVia();
            if (newAudio == RdRxAudioVia.NoAudio)
                SetRx(false);
            else
                SetAudioVia(newAudio);
        }

        public bool InSpeaker(RdRxAudioVia speaker)
        {
            bool ret = false;
            if (AudioVia == speaker)
                ret = true;
            else if (AudioVia == RdRxAudioVia.HeadPhones)
            {
                MixerDev dev = (speaker == RdRxAudioVia.HfSpeaker) ? dev = MixerDev.SpkHf: MixerDev.SpkRd;
                foreach (int port in _RxPorts.Values)
                {
                    if (Top.Mixer.MatchActiveLink(dev, port))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Funcion que calcula el siguiente estado de audio para RX en la rotación, 
        /// teniendo en cuenta la configuración y el estado de los altavoces
        /// y el estado del RdDst.Tx
        /// </summary>

        /// <returns>audio via</returns>
        private RdRxAudioVia NextRxAudioVia()
        {
            RdRxAudioVia audioViaKO = RdRxAudioVia.NoAudio;
            switch (AudioVia)
            {
                case RdRxAudioVia.NoAudio:
                    if (Top.Hw.RdSpeaker)
                        return RdRxAudioVia.Speaker;
                    else if (Top.Rd.HFSpeakerAvailable() &&
                        ((TipoFrecuencia == TipoFrecuencia_t.HF) || Top.Rd.DoubleRadioSpeaker))
                        return RdRxAudioVia.HfSpeaker;
                    else if (!Top.Mixer.ModoSoloAltavoces)
                        return RdRxAudioVia.HeadPhones;
                    audioViaKO = RdRxAudioVia.Speaker;
                    break;
                case RdRxAudioVia.Speaker:
                    if (Top.Rd.HFSpeakerAvailable() &&
                       ((TipoFrecuencia == TipoFrecuencia_t.HF) || Top.Rd.DoubleRadioSpeaker))
                        return RdRxAudioVia.HfSpeaker;
                    else if (Top.Mixer.ModoSoloAltavoces)
                    {
                        if (!Tx)
                            return RdRxAudioVia.NoAudio;
                    }
                    else
                        return RdRxAudioVia.HeadPhones;
                    if (!Top.Mixer.ModoSoloAltavoces) 
                        audioViaKO = RdRxAudioVia.HeadPhones;
                    else 
                    // No saca ventana de via no disponible
                        return RdRxAudioVia.Speaker;
                    break;
                case RdRxAudioVia.HfSpeaker:
                    if (Top.Mixer.ModoSoloAltavoces)
                    {
                        if (!Tx)
                            return RdRxAudioVia.NoAudio;
                        else if (Top.Hw.RdSpeaker)
                            return RdRxAudioVia.Speaker;
                    }
                    else
                        return RdRxAudioVia.HeadPhones;
                    if (!Top.Mixer.ModoSoloAltavoces) 
                        audioViaKO = RdRxAudioVia.HeadPhones;
                    else
                        // No saca ventana de via no disponible
                        return RdRxAudioVia.HfSpeaker;
                    audioViaKO = RdRxAudioVia.HeadPhones;
                    break;
                default: // RdRxAudioVia.HeadPhones
                    if (!Tx)
                        return RdRxAudioVia.NoAudio;
                    else if (Top.Hw.RdSpeaker)
                        return RdRxAudioVia.Speaker;
                    else if (Top.Rd.HFSpeakerAvailable() &&
                       ((TipoFrecuencia == TipoFrecuencia_t.HF) || Top.Rd.DoubleRadioSpeaker))
                        return RdRxAudioVia.HfSpeaker;
                    audioViaKO = RdRxAudioVia.Speaker;
                    break;
            }
            General.SafeLaunchEvent(AudioViaNotAvailable, this, audioViaKO);
            return AudioVia;
        }
        /// <summary>
        /// Determina si el PTT es causado por una retransmision
        /// </summary>
        public bool PttCausadoPorRetransmision()
        {
            if (Ptt == PttState.ExternPtt && RtxGroup != 0 && !string.IsNullOrEmpty(_PttSrcId) && _PttSrcId.StartsWith("Rtx_"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Retorna la prioridad del Ptt
        /// </summary>
        public uint GetPttPriority()
        {
            return _Priority;
        }

        /// <summary>
        /// Función que determina si el audio debe ser inhibido
        /// 1-Si la frecuencia esta en un grupo de RTX, y estoy en el puesto propietario del grupo 
        /// y hay SQ y la retransmisión esta activa, no conecto mi audio porque se debe escuchar el que se está retransmitiendo.
        /// 2-Si tengo PTT propio no conecto mi audio
        /// </summary>
        /// <param name="frRs"> Datos de frecuencia</param>
        /// <param name="frRs"> Datos de frecuencia</param>
        private bool InhiboMiAudio(PttState ptt, RdSrvFrRs frRs)
        {
            if (ptt == PttState.ExternPtt && frRs.RtxGroupId != 0 && frRs.RtxGroupOwner == Top.HostId &&
                frRs.Squelch == RdSrvFrRs.SquelchType.SquelchOnlyPort &&
                frRs.PttSrcId.StartsWith("Rtx_"))
                return true;
            else if (ptt == PttState.PttOnlyPort) 
                return true;
            else if (_PttOffTimer.Enabled)
                return true;                //Si el timer activado despues de Ptt off sigue activo, el audio se inhibe
            else
                return false;
        }

		public void SetQuiet()
		{
			if (Rx && _AudioVia != RdRxAudioVia.NoAudio)
			{
                Dictionary<string, int> rxPortsTemp = new Dictionary<string,int>(_RxPorts);
                foreach (int port in rxPortsTemp.Values)
				{
					Top.Mixer.Unlink(port);

					SetRx(false);
					SetTx(false, false);
				}

				AudioVia = RdRxAudioVia.NoAudio;
			}
		}

        /// <summary>
        /// Funcion llamada cuando hay un cambio en la presencia de jacks o altavoces
        /// y se requiere conmutación automática de cascos a altavoz o entre altavoces.
        /// El paso de cascos a altavoz por fallo de jacks se recupera automaticamente
        /// El cambio entre altavoces o a casco por fallo de altavoces no se recupera automáticamente
        /// </summary>
        /// 
        public void CheckAudioVia()
		{
            if (Rx == false)
                return;
            bool hayAlgunJack = Top.Hw.AlumnJack || Top.Hw.InstructorJack;
            switch (_AudioVia)
            {
                case RdRxAudioVia.HeadPhones:
                    if (!hayAlgunJack)
                        if (Top.Hw.RdSpeaker)
                        {
                            SetAudioVia(RdRxAudioVia.Speaker);
                            //Cambia la audioVia inicial
                            _RealAudioVia = RdRxAudioVia.HeadPhones;
                        }
                        else if (Top.Rd.HFSpeakerAvailable())
                        {
                            SetAudioVia(RdRxAudioVia.HfSpeaker);
                            //Cambia la audioVia inicial
                            _RealAudioVia = RdRxAudioVia.HeadPhones;
                        }
                    break;
                case RdRxAudioVia.Speaker:
                    //Recupera los jacks
                    if ((_AudioVia != _RealAudioVia) && hayAlgunJack)
                    {
                        SetAudioVia(RdRxAudioVia.HeadPhones);
                    }
                    // Se pierde el altavoz
                    else if (!Top.Hw.RdSpeaker)
                    {
                        if (Top.Rd.HFSpeakerAvailable())
                        {
                            SetAudioVia(RdRxAudioVia.HfSpeaker);
                        }
                        else if (hayAlgunJack && !Top.Mixer.ModoSoloAltavoces)
                        {
                            SetAudioVia(RdRxAudioVia.HeadPhones);
                        }
                    }
                    break;
                case RdRxAudioVia.HfSpeaker:
                    //Recupera los jacks
                    if ((_AudioVia != _RealAudioVia) && hayAlgunJack)
                    {
                        SetAudioVia(RdRxAudioVia.HeadPhones);
                    }
                    // Se pierde el altavoz
                    else if (!Top.Hw.HfSpeaker)
                    {
                        if (Top.Hw.RdSpeaker)
                        {
                            SetAudioVia(RdRxAudioVia.Speaker);
                        }
                        else if (hayAlgunJack && !Top.Mixer.ModoSoloAltavoces)
                        {
                            SetAudioVia(RdRxAudioVia.HeadPhones);
                        }
                    }
                    break;
                default: //RdRxAudioVia.NoAudio
                    //no hago nada
                    break;
            }
		}

        public string ChangeAlias()
        {
            int index = 0;

            // Encontrar el emplazamiento seleccionado
            foreach (KeyValuePair<string, string> alias in _RscSite)
            {
                if (alias.Value == _Alias)
                {
                    index = ++index % _RscSite.Count;
                    break;
                }

                index = ++index % _RscSite.Count;
            }

            // Seleccionar el siguiente emplazamiento
            int i = 0;
            int sel = index;

            foreach (KeyValuePair<string, string> alias in _RscSite)
            {
                if (i == sel)
                {
                    _OldAlias = Alias;
                    Alias = alias.Value;
                    break;
                }

                i++;
            }

            return Alias;
        }

		#region Private Members

		private enum AssignState { Idle, Trying, Set }

		private static Logger _Logger = LogManager.GetCurrentClassLogger();

        private bool _Monitoring;
		private int _Pos;
		private string _Literal = "";
		private string _Alias = "";
        private string _KeyAlias = "";
        private string _OldAlias = string.Empty;
		private uint _Priority = 0;
		private AssignState _Rx = AssignState.Idle;
		private AssignState _Tx = AssignState.Idle;
		private PttState _Ptt = PttState.Unavailable;
        private string _PttSrcId = string.Empty;
		private SquelchState _Squelch = SquelchState.Unavailable;
		private RdRxAudioVia _AudioVia = RdRxAudioVia.NoAudio;
		private RdRxAudioVia _RealAudioVia = RdRxAudioVia.NoAudio;
		private int _RtxGroup = 0;
        private TipoFrecuencia_t _TipoFrecuencia = TipoFrecuencia_t.Basica;
        private RdSrvFrRs.FrequencyStatusType _Estado = RdSrvFrRs.FrequencyStatusType.NotAvailable;
        
        private string _QidxMethod = string.Empty;
        private uint _QidxValue = 0;
        private string _QidxResource = string.Empty;
        //Vale true si la frecuencia tiene configurados sólo recursos RX y no es HF
        //Se utiliza para deshabilitar la parte TX en la tecla
        private bool _RxOnly = false;
		private Rs<RdSrvFrRs> _AssociateFrRs = null;
		private List<Rs<RdSrvRxRs>> _AssociateRxRs = new List<Rs<RdSrvRxRs>>();
		private List<Rs<RdSrvTxRs>> _AssociateTxRs = new List<Rs<RdSrvTxRs>>();
		private Dictionary<string, int> _RxPorts = new Dictionary<string, int>();
        private Dictionary<string, string> _RscSite = new Dictionary<string, string>();

        private Timer _PttOffTimer = new Timer();       //Timer que se activa al producirse un PTT off

        private void OnPttOffTimer(object sender, ElapsedEventArgs e)
        {
            Top.WorkingThread.Enqueue("OnPttOffTimer", delegate()
            {
                //Al vencer el timer hay que reactivar la recepcion del Audio de Radio Rx si procede
                if (InhiboMiAudio(_Ptt, _AssociateFrRs.Info) == true)
                {
                    foreach (int port in _RxPorts.Values)
                    {
                        Top.Mixer.Unlink(port);
                    }
                }
                else if (Rx && (Squelch == SquelchState.SquelchOnlyPort))
                {
                    foreach (int port in _RxPorts.Values)
                    {
                        MixerDev dev = (_AudioVia == RdRxAudioVia.HeadPhones ? MixerDev.MhpRd : (_AudioVia == RdRxAudioVia.HfSpeaker ? MixerDev.SpkHf : MixerDev.SpkRd));
                        Top.Mixer.Link(port, dev, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                    }
                }
            });              
        }

		private bool RxOn()
		{
			//Debug.Assert(_RxPorts.Count == 0);
			//Debug.Assert(_AudioVia == RdRxAudioVia.NoAudio);

            SetAudioVia(NextRxAudioVia());
            _RealAudioVia = AudioVia;   // RdRxAudioVia.Speaker;
            if (AudioVia == RdRxAudioVia.NoAudio)
                return false;

            General.SafeLaunchEvent(RxAlreadyAssigned, this);

			foreach (Rs<RdSrvRxRs> rxRs in _AssociateRxRs)
			{
                if (rxRs.IsValid /*&& _RscSite.ContainsKey(rxRs.Id) && _RscSite[rxRs.Id] == Alias*/)
				{
                    if (!_RxPorts.ContainsKey(rxRs.Id.ToUpper()))
                    {
                        CreateRxAudio(rxRs, rxRs.Id.ToUpper());
                    }
                }
			}

            return true;
		}

		private void RxOff()
		{
			if (Rx)
			{
				foreach (int port in _RxPorts.Values)
				{
					Top.Mixer.Unlink(port);
                    SipAgent.DestroyRdRxPort(port);
                    _Logger.Debug("*** RxOff. Llamando a DestroyRdRxPort({0}) {1}", port, Literal);
				}
				_RxPorts.Clear();

				_AudioVia = RdRxAudioVia.NoAudio;
				_RealAudioVia = RdRxAudioVia.NoAudio;
				_Tx = AssignState.Idle;
				_RtxGroup = Math.Min(_RtxGroup, 0);
			}
		}

		private PttState GetPtt(string pttSrcId)
		{
			PttState ptt = PttState.NoPtt;

			if (!Tx || (Top.Rd.PttSource == PttSource.NoPtt) || 
				(Top.Lc.Activity && (Top.Mixer.SplitMode != SplitMode.LcTf)))
			{
				if (!string.IsNullOrEmpty(pttSrcId) && pttSrcId != Top.HostId)
				{
					ptt = PttState.ExternPtt;
				}
			}
			else if (pttSrcId == Top.HostId)
			{
				ptt = PttState.PttOnlyPort;
			}
			else if (pttSrcId == "NO_CARRIER")
			{
                ptt = PttState.CarrierError;
			}
            else if (pttSrcId == "ERROR")
            {
                ptt = PttState.Error;
            }
            else if (!string.IsNullOrEmpty(pttSrcId))
			{
				ptt = PttState.Blocked;
			}

			return ptt;
		}

		private void OnFrMsg(object msg, short type)
		{
            _Logger.Trace("*** OnFrMsg({0}) {1}:",type, _Literal );
            switch (type)
			{
				case Identifiers.FR_TX_CHANGE_RESPONSE_MSG:
					if (_Tx == AssignState.Trying)
					{
						if ((bool)msg)
						{
							if (!Rx)
							{
								if (RxOn())
								    _Rx = AssignState.Set;
							}

                            if (Rx)
                            {
                                Tx = true;
                                Top.Registry.SetTxAssigned(_Literal);
                            }
						}
						else
						{
							_Tx = AssignState.Idle;
							General.SafeLaunchEvent(TxAlreadyAssigned, this);
						}
					}
					break;
                case Identifiers.FR_HF_TX_CHANGE_RESPONSE_MSG:
                    if (_Tx == AssignState.Trying)
                    {
                        if ((uint)msg == 3) // Assigned
                        {
                            if (!Rx)
                            {
                                if (RxOn())
                                    _Rx = AssignState.Set;
                            }

                            if (Rx)
                            {
                                Tx = true;
                                Top.Registry.SetTxAssigned(_Literal);
                            }
                        }
                        else   // stdTxAlreadyAssigned || stdError || stdFrequencyAlreadyAssigned
                        {
                            _Tx = AssignState.Idle;
                            General.SafeLaunchEvent(TxHfAlreadyAssigned, this, (uint)msg);
                        }
                    }
                    else if (_Tx == AssignState.Set && 
                            ((uint)msg == 0xFE || (uint)msg == 1))     // stdNoGateway || stdError
                    {
                        Tx = false;
                        //_Tx = AssignState.Idle;
                    }

                    break;
                case Identifiers.FR_RX_CHANGE_RESPONSE_MSG:
					if (_Rx == AssignState.Trying)
					{
						if ((bool)msg)
						{
                            if (RxOn())
							    Rx = true;
						}
						else
						{
							_Rx = AssignState.Idle;
						}
					}
					break;
				case Identifiers.FR_TX_ASSIGNED_MSG:
					if (Tx)
					{
						General.SafeLaunchEvent(TxAssignedByOther, this, (string)msg);
					}
					break;
#if _HF_GLOBAL_STATUS_
                case Identifiers.HF_STATUS:
                    if (_TipoFrecuencia == TipoFrecuencia_t.HF) 
                    General.SafeLaunchEvent(HfGlobalStatus, this, (HFStatusCodes)msg);
                    break;
#endif
			}
		}

		private void OnFrChanged(object sender)
		{
			Rs<RdSrvFrRs> rs = (Rs<RdSrvFrRs>)sender;
			bool changed = false;
            bool changedQidx = false;

			if (!rs.IsValid)
			{
				RxOff();

                // Provocar la liberación del transmisor HF 
                // en caso de que estuviera ocupado por este usuario
                if (_TipoFrecuencia == TipoFrecuencia_t.HF)
                {
                    _RtxGroup = Math.Min(_RtxGroup, 0);
                    Tx = false;
                    Top.Registry.SetTx(_Literal, false, _Priority, false);
                }

				_Rx = AssignState.Idle;
				_RtxGroup = 0;
				_Squelch = SquelchState.Unavailable;
				_Ptt = PttState.Unavailable;
                _PttSrcId = string.Empty;
                _Estado = RdSrvFrRs.FrequencyStatusType.NotAvailable;
                _QidxValue = 0;
                _QidxResource = string.Empty;

				changed = true;
			}
			else
			{

				RdSrvFrRs frRs = rs.Info;
                
                if (frRs.PttSrcId == "TxHfOff")
                {
                    if (_Tx == AssignState.Set)
                    {
                        // Reflejar el estado real de Tx. Puede que el recurso no 
                        // esté disponble en la configuración de la pasarela. En 
                        // cuyo caso se debe quitar de transmisión
                        _Tx = AssignState.Idle;
                        _Ptt = PttState.NoPtt;
                        _PttSrcId = frRs.PttSrcId;
                    
                        // Actualizar estado de Tx a Off
                        General.SafeLaunchEvent(StateChanged, this);
                        // Enviar mensaje para ventana de error
                        General.SafeLaunchEvent(TxHfAlreadyAssigned, this, (uint)0xFE);
                    }

                    frRs.PttSrcId = string.Empty;
                    //return;
                }
                
                //
                // Tratamiento del cambio en el estado de Squelch
                //
                switch (_TipoFrecuencia)
                {
                    case TipoFrecuencia_t.FD:                        
                        if (_Squelch != (SquelchState)frRs.Squelch)
                        {
                            _Squelch = (SquelchState)frRs.Squelch;
                            changed = true;
                        }
                        if (ChangeInQidxInfo(frRs))
                        {
                            // BSS Information
                            _QidxMethod = frRs.QidxMethod;
                            _QidxResource = _Squelch == SquelchState.NoSquelch ? string.Empty : frRs.SqSite;
                            _QidxValue = _Squelch == SquelchState.NoSquelch ? 0 : frRs.QidxValue;
                            changedQidx = true;
                        }
                        break;
                    // EM
                    case TipoFrecuencia_t.ME:
                        if (_Squelch != (SquelchState)frRs.Squelch && (frRs.SqSite == Alias || frRs.SqSite == string.Empty))
                        {
                            _Squelch = (SquelchState)frRs.Squelch;
                            changed = true;

                            // BSS Information
                            _QidxMethod = frRs.QidxMethod;
                            _QidxResource = frRs.SqSite;
                            _QidxValue = frRs.QidxValue;
                        }
                        break;
                    default:
                        if (_Squelch != (SquelchState)frRs.Squelch)
                        {
                            _Squelch = (SquelchState)frRs.Squelch;
                            changed = true;
                        }
                        break;
                }

                if (_Squelch != (SquelchState)frRs.Squelch && (frRs.SqSite == Alias || frRs.SqSite == string.Empty))
				{
					_Squelch =  (SquelchState)frRs.Squelch;
					changed = true;
				}

                //
                // Tratamiento del cambio en el estado de PTT
                //
                PttState ptt = GetPtt(frRs.PttSrcId);
                if ((frRs.PttSrcId != _PttSrcId) && (ptt == PttState.Error) )
                    Top.Rd.GenerateBadOperationTone(2000);
                // Es posible que no cambie el ptt (externo) pero si cambie su procedencia:
                // Cambio de ptt externo de rtx a externo de otro HMI. 
                // En este caso hay que evaluar el audio
				if (ptt != _Ptt || changed || frRs.PttSrcId != _PttSrcId)
				{
                    if (ptt != _Ptt) 
                    {
                        if (ptt == PttState.NoPtt)
                        {
                            if ((_Ptt == PttState.ExternPtt) || (_Ptt == PttState.PttOnlyPort) || (_Ptt == PttState.PttPortAndMod))
                            {
                                //Al desactivarse el Ptt arranca un timer durante el cual se inhibe el audio de Rd Rx
                                //Solo cuando el estado anterior es un ptt #3830
                                _PttOffTimer.Enabled = true;
                            }
                        }
                        else
                        {
                            //Cualquier activacion del Ptt anula el timer
                            _PttOffTimer.Enabled = false;
                        }
                    }

                    // Si estoy en Ptt o 
                    // Estoy en RTX de otro SQ del grupo y soy dueño del grupo (dejo el otro audio)
                    // no conecto mi audio
                    if (InhiboMiAudio(ptt, frRs) == true)
					{
						foreach (int port in _RxPorts.Values)
						{
							Top.Mixer.Unlink(port);
						}
					}
					else 
					{
						foreach (int port in _RxPorts.Values)
						{
                            if (Rx)
                                if(Squelch == SquelchState.SquelchOnlyPort)
                                {
                                    MixerDev dev = (_AudioVia == RdRxAudioVia.HeadPhones ? MixerDev.MhpRd : (_AudioVia == RdRxAudioVia.HfSpeaker ? MixerDev.SpkHf : MixerDev.SpkRd));
                                    Top.Mixer.Link(port, dev, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
                                }
                                else if (Squelch == SquelchState.NoSquelch)
                                {
                                    Top.Mixer.Unlink(port);
                                }
						}
					}

					_Ptt = ptt;
                    _PttSrcId = frRs.PttSrcId;
					changed = true;
				}

                //
                // Tratamiento del cambio en el estado de la retransmisión
                //
                int rtxGroup = 0;
				if (frRs.RtxGroupId != 0)
				{
					rtxGroup = frRs.RtxGroupOwner == Top.HostId ? (int)frRs.RtxGroupId : -1;
				}
				if (rtxGroup != _RtxGroup)
				{
					_RtxGroup = rtxGroup;
					changed = true;
				}

                //
                // Tratamiento del cambio en el estado de disponibilidad de la frecuencia.
                //
                if (frRs.FrequencyStatus != _Estado)
                {
                    _Estado = frRs.FrequencyStatus;
                    changed = true;
                 }

			}

			if (changed || changedQidx)
			{
				General.SafeLaunchEvent(StateChanged, this);
			}
		}

        private bool ChangeInQidxInfo(RdSrvFrRs frRs)
        {
            return _QidxMethod != frRs.QidxMethod ||
                    _QidxResource != (_Squelch == SquelchState.NoSquelch ? string.Empty : frRs.SqSite) ||
                    _QidxValue != (_Squelch == SquelchState.NoSquelch ? 0 : frRs.QidxValue);
        }

		private void OnFrRxChanged(object sender)
		{
			Rs<RdSrvRxRs> rs = (Rs<RdSrvRxRs>)sender;
			Debug.Assert(_AssociateRxRs.Contains(rs));

			if (Rx)
			{
				string rsId = rs.Id.ToUpper();
                _Logger.Debug("*** OnFrRxchanged({2}). rs.IsValid es {0} {1}:", rs.IsValid, Pos, rsId);
				if (rs.IsValid)
				{
					// Debug.Assert(!_RxPorts.ContainsKey(rsId));
                    if (_RxPorts.ContainsKey(rsId))
                    {
                        //_RxPorts.Remove(rsId);

                        /* Provocar el paso por aspas cuando se cae la red*/
                        RxOff();

                        // Provocar la liberación del transmisor HF 
                        // en caso de que estuviera ocupado por este usuario
                        if (_TipoFrecuencia == TipoFrecuencia_t.HF)
                        {
                            _RtxGroup = Math.Min(_RtxGroup, 0);
                            Tx = false;
                            Top.Registry.SetTx(_Literal, false, _Priority, false);
                        }

                        _Rx = AssignState.Idle;
                        _RtxGroup = 0;
                        _Squelch = SquelchState.Unavailable;
                        _Ptt = PttState.Unavailable;
                        _PttSrcId = string.Empty;
                        _Estado = RdSrvFrRs.FrequencyStatusType.NotAvailable;

                        General.SafeLaunchEvent(StateChanged, this);
                        /* Fin cambio */
                    }

                    _Logger.Debug("*** OnFrRxchanged({2}). Llamando a CreateRdRxPort({0}, pos {1})", rsId, Pos, rsId);
                        CreateRxAudio(rs, rsId);
				}
				else
				{
                    int port;
					if (_RxPorts.TryGetValue(rsId, out port))
					{
						Top.Mixer.Unlink(port);

                            _Logger.Debug("*** OnFrRxChanged({2}). Llamando a DestroyRdRxPort({0})", port, rsId);

						SipAgent.DestroyRdRxPort(port);

						_RxPorts.Remove(rsId);
					}
				}
			}
		}

        private void CreateRxAudio(Rs<RdSrvRxRs> rs, string rsId)
        {
            int port = SipAgent.CreateRdRxPort(rs.Info, Top.SipIp);
            _RxPorts[rsId] = port;
            if (AnySquelch)
            {
                MixerDev dev = (_AudioVia == RdRxAudioVia.HeadPhones ? MixerDev.MhpRd : (_AudioVia == RdRxAudioVia.HfSpeaker ? MixerDev.SpkHf : MixerDev.SpkRd));
                Top.Mixer.Link(port, dev, MixerDir.Send, Mixer.RD_PRIORITY, FuentesGlp.RxRadio);
            }
            _Logger.Debug("*** CreateRxAudio. Llamando a CreateRdRxPort({0}) {1}", port, Literal);
        }

        private void OnSelCalMsg(object msg, short type)
        {
            if (type == Identifiers.SELCAL_PREPARE_RSP)
            {
                if (Tx)
                {
                    General.SafeLaunchEvent(SelCalPrepareResponse, this, (string)msg);
                }
            }
        }

        private void OnSiteChanged(object msg, short type)
        {
            if (type == Identifiers.SITE_CHANGING_RSP)
            {
                // En principio, para hacer cambio de emplazamiento
                // no tiene por qué estar en Tx
                // if (Tx)
                if (((ChangeSiteRsp)msg).resultado)
                {
                    _OldAlias = Alias = ((ChangeSiteRsp)msg).Alias;
                    // Switch audio
                    if (Rx)
                    {
                        // Rx off
                        foreach (int port in _RxPorts.Values)
                        {
                            Top.Mixer.Unlink(port);
                            SipAgent.DestroyRdRxPort(port);
                        }
                        _RxPorts.Clear();

                        // Rx on
                        foreach (Rs<RdSrvRxRs> rxRs in _AssociateRxRs)
                        {
                            if (rxRs.IsValid && _RscSite.ContainsKey(rxRs.Id) && _RscSite[rxRs.Id] == Alias)
                            {
                                if (!_RxPorts.ContainsKey(rxRs.Id.ToUpper()))
                                {
                                    CreateRxAudio(rxRs, rxRs.Id.ToUpper());
                                }
                            }
                        }
                    }
                }
                else
                    Alias = _OldAlias;

                General.SafeLaunchEvent(SiteChangedResult, this, (ChangeSiteRsp)msg);
            }
        }

        #endregion
	}
}

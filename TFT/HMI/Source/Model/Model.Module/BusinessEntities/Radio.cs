#define _HF_GLOBAL_STATUS_
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Messages;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;
using NLog;

namespace HMI.Model.Module.BusinessEntities
{
	public enum PttState
	{
		NoPtt,
		ExternPtt,
		PttOnlyPort,
		PttPortAndMod,
		Blocked,
        CarrierError,       // No detectada la portadora, ...
        TxError,
        Error,
        Unavailable
	}

	public enum SquelchState
	{
		NoSquelch,
		SquelchOnlyPort,
		SquelchPortAndMod,
		Unavailable
	}

	public enum RdRxAudioVia
	{
		NoAudio,
		HeadPhones,
		Speaker,
        HfSpeaker
	}

	public enum RtxState
	{
		NoChanged,
		Add,
		Delete
	}

    public enum FrequencyState
    {
      NotAvailable,
      Available,
      Degraded
    }

    public enum TipoFrecuencia_t
    {
        Basica = 0,
        HF = 1,
        VHF = 2,
        UHF = 3,
        DUAL = 4,
        FD = 5,
        ME = 6
    }

	public sealed class RdDst
	{
		private int _Id;
		private string _Frecuency = "";//RQF 34 en realidad es IdFrecuency
		private string _Alias = "";
		private string _NameFrecuency;//RQF34 DescDestino; En realidad es _NameFrecuency.
        /** 20180321. AGL. ALIAS a mostrar en la tecla... */
        private string _KeyAlias = "";
		//LALM 210223 Errores #4756 prioridad
		private int _Priority = 0;

		private bool _Tx = false;
		private bool _Rx = false;
		private PttState _Ptt = PttState.NoPtt;
		private SquelchState _Squelch = SquelchState.NoSquelch;
		private RdRxAudioVia _AudioVia = RdRxAudioVia.NoAudio;
		private int _RtxGroup = 0;
		private int _TempRtxGroup = 0;
        private TipoFrecuencia_t _TipoFrecuencia = TipoFrecuencia_t.Basica;
        private bool _Monitoring = false;
		//RQF-14
		private bool _FrecuenciaNoDesasignable = false;
        private bool _Restored = true;
        private string _TempAlias = string.Empty;
        private FrequencyState _State = FrequencyState.NotAvailable;
        //Vale true si la frecuencia tiene configurados sólo recursos RX y no es HF
        //Se utiliza para deshabilitar la parte TX en la tecla
        private bool _RxOnly = false;
        // BSS Information
        private string _qidxMethod = string.Empty;
        private uint _qidxValue = 0;
        private string _qidxResource = string.Empty;

        /** 20190205. RTX Information*/
        private string _PttSrcId = string.Empty;
        public string PttSrcId { get { return _PttSrcId; } }

		public int Id
		{
			get { return _Id; }
		}

		// RQF34 devuelvo una clave.
		public string IdFrecuency
        {
			get 
			{ 
				return _Frecuency; 
			}
            set { _Frecuency = value; }
        }

		public string Frecuency
		{
			get { return _Frecuency; }
		}
		
		// RQF34 Se define NameFrecuency
		public string NameFrecuency
		{
			get {
				return _Frecuency; 
				}
            set { _NameFrecuency = value; }
		}

		public string Literal
        {
            get { return _NameFrecuency; }
        }

		public string Alias
		{
			get { return _Alias; }
		}
        public string TempAlias
        {
            get { return _TempAlias; }
            set { _TempAlias = value; }
        }
		/** 20180321. AGL. ALIAS a mostrar en la tecla... */
		public string KeyAlias { get { return _KeyAlias; } }

		// 210223 LALM Errores #4756 Prioridad
		public int  Priority { 
			get { return _Priority; }
			set { _Priority = value; }
		}

		public bool Tx
		{
			get { return _Tx; }
		}

		public bool Rx
		{
			get { return _Rx; }
		}

		public RdRxAudioVia AudioVia
		{
			get { return _AudioVia; }
		}

		public PttState Ptt
		{
			get { return _Ptt; }
		}

		public SquelchState Squelch
		{
			get { return _Squelch; }
		}

		public int RtxGroup
		{
			get { return _RtxGroup; }
		}

		public int TempRtxGroup
		{
			get { return _TempRtxGroup; }
			set { _TempRtxGroup = value; }
		}

		public bool Unavailable
		{
			get { return (_Ptt == PttState.Unavailable) || (_Squelch == SquelchState.Unavailable); }
		}

        public bool IsConfigurated
        {
            get { return _Frecuency.Length > 0; }
        }

        public TipoFrecuencia_t TipoFrecuencia
        {
            get { return _TipoFrecuencia; }
        }

        public bool Monitoring
        {
            get { return _Monitoring; }

		}

		//RQF-14
		public bool FrecueciaNoDesasignable
		{
			get { return _FrecuenciaNoDesasignable; }

		}


		public bool Restored
        {
            get { return _Restored; }
        }


        public FrequencyState State
        {
            get { return _State; }
        }
        
        // BSS Information
        public string QidxMethod
        {
            get { return _qidxMethod; }
        }
        public uint QidxValue
        {
            get { return _qidxValue; }
        }
        public string QidxResource
        {
            get { return _qidxResource; }
        }

        public bool RxOnly
        {
            get { return _RxOnly; }
        }

		public RdDst(int id)
		{
			_Id = id;
		}

		public void ResetToIdle()
		{
			_Tx = false;
			_Rx = false;
			_Ptt = PttState.NoPtt;
			_Squelch = SquelchState.NoSquelch;
			_AudioVia = RdRxAudioVia.NoAudio;
			_RtxGroup = _TempRtxGroup = 0;
            _Monitoring = false;
			_FrecuenciaNoDesasignable = false;//RQF-14
            _Restored = true;
            _TipoFrecuencia = TipoFrecuencia_t.Basica;
            _qidxResource = _qidxMethod = string.Empty;
            _qidxValue = 0;
            _State = FrequencyState.NotAvailable;
            _RxOnly = false;
            /** 20190205 */
            _PttSrcId = string.Empty;
		}

		public void Reset()
		{
			_Frecuency = "";
			_TempAlias = _Alias = "";
			IdFrecuency = "";// RQF34
			_NameFrecuency = "";//RQF34
			ResetToIdle();
		}

		public void Reset(RdInfo dst)
		{
			if (dst.Dst == "")
			{
				Reset();
			}
			else
			{
				_Frecuency = dst.Dst;
				IdFrecuency = dst.KeyAlias;//RQF34 se debe usar idfrecuency y NameFrecuency en lugar de _Frecuency
											  // keyalias es el identificador único
											  // dst es el numero de frecuencia antiguamente se usaba como clave

				//rqf34
				IdFrecuency = dst.KeyAlias;
				NameFrecuency = dst.Dst;
				_Alias = dst.Alias;
				//lalm 220311 RQF34
				//lalm 220329 intercambio DescDestino por KeyAlias cuando son distintos.
				if (dst.KeyAlias==dst.DescDestino)
                {
					//_NameFrecuency = dst.IdFrecuency;
					dst.KeyAlias = "";
				}
                else 
				{
					string tmp = dst.KeyAlias;
					dst.KeyAlias = dst.DescDestino;
					dst.DescDestino = tmp;
				}

				_TempAlias = string.Empty;
				_Ptt = dst.Ptt;
				_Squelch = dst.Squelch;
				_Tx = dst.Tx;
				_Rx = dst.Rx || _Tx;
                _AudioVia = _Rx ? dst.AudioVia : RdRxAudioVia.NoAudio;
                _Monitoring = dst.Monitoring;//  && !Unavailable;
											 
				_FrecuenciaNoDesasignable = dst.FrecuenciaNoDesasignable;//RQF-14
				_RtxGroup = _TempRtxGroup = dst.RtxGroup;
                _TipoFrecuencia = dst.TipoFrecuencia;
                _State = dst.Estado;
                _qidxResource = _qidxMethod = string.Empty;
                _qidxValue = 0;
                _RxOnly = dst.RxOnly;
                /** 20180321. AGL. ALIAS a mostrar en la tecla... */
                if (Properties.Settings.Default.RadioAlias == true)
                {
                    _KeyAlias = dst.KeyAlias == dst.Dst ? "" : dst.KeyAlias;
                }
                else
                {
                    _KeyAlias = (_TempAlias != string.Empty && _TempAlias != _Alias) ? _TempAlias : _Alias;
                }
				/** */
				//LALM 210223 Errores #4756 prioridad
				_Priority = dst.Priority;

                if (!Restored && Unavailable && (dst.Ptt != PttState.Unavailable || dst.Squelch != SquelchState.Unavailable))
                    _Restored = true;
                else
                    _Restored = false;

				Debug.Assert(!_Rx || (_AudioVia != RdRxAudioVia.NoAudio));
				Debug.Assert(!_Tx || Rx);

				if ((_Squelch == SquelchState.SquelchPortAndMod) && (_Ptt == PttState.PttOnlyPort))
				{
					_Ptt = PttState.PttPortAndMod;
				}
				else if ((_Squelch == SquelchState.SquelchOnlyPort) && (_Ptt == PttState.PttPortAndMod))
				{
					_Ptt = PttState.PttOnlyPort;
				}

                //_PttSrcId = dst.PttSrcId;
			}
		}

		public void Reset(RdState st)
		{
            if (Unavailable && (st.Ptt != PttState.Unavailable || st.Squelch != SquelchState.Unavailable))
                _Restored = true;
            else
                _Restored = false;
			_Ptt = st.Ptt;
			_Squelch = st.Squelch;
			_Tx = st.Tx;
			_Rx = st.Rx || _Tx;    // || _Monitoring || _Tx;
            _AudioVia = _Rx ? st.AudioVia : RdRxAudioVia.NoAudio;// !_Monitoring ? st.AudioVia : RdRxAudioVia.Speaker;
			if (_RtxGroup != _TempRtxGroup)
				_RtxGroup = st.RtxGroup;
			else
				_RtxGroup = _TempRtxGroup = st.RtxGroup;
			//_RtxGroup = st.RtxGroup;

			Debug.Assert(!_Rx || (_AudioVia != RdRxAudioVia.NoAudio));
			Debug.Assert(!_Tx || Rx);

			if ((_Squelch == SquelchState.SquelchPortAndMod) && (_Ptt == PttState.PttOnlyPort))
			{
				_Ptt = PttState.PttPortAndMod;
			}
			else if ((_Squelch == SquelchState.SquelchOnlyPort) && (_Ptt == PttState.PttPortAndMod))
			{
				_Ptt = PttState.PttOnlyPort;
			}

            // BSS Information
            _qidxMethod = st.QidxMethod;
            _qidxResource = st.QidxResource;
            _qidxValue = st.QidxValue;

            _State = st.State;

            _PttSrcId = st.PttSrcId;
		}

		public void Reset(RdDestination dst)
		{
			if (dst.Dst == "")
			{
				Reset();
			}
			else
			{
				//_Frecuency = dst.Dst;// RQF34 desaparecerá
				_Alias = dst.Alias;
                _TempAlias = string.Empty;
				//RQF34 _Frecuency es NameFrecuency IdFrecuency es nuevo
				_NameFrecuency = dst.Dst;
				IdFrecuency = dst.IdFrecuency;
			}
		}

		public void Reset(PttState st)
		{
			_Ptt = st;

			switch (_Ptt)
			{
				case PttState.Unavailable:
					_TempRtxGroup = _RtxGroup;
					break;
				case PttState.PttOnlyPort:
					if (_Squelch == SquelchState.SquelchPortAndMod)
					{
						_Ptt = PttState.PttPortAndMod;
					}
					break;
			}
		}

		public void Reset(SquelchState st)
		{
			_Squelch = st;

			switch (_Squelch)
			{
				case SquelchState.Unavailable:
					_TempRtxGroup = _RtxGroup;
					break;
				case SquelchState.SquelchOnlyPort:
					if (_Ptt == PttState.PttPortAndMod)
					{
						_Ptt = PttState.PttOnlyPort;
					}
					break;
				case SquelchState.SquelchPortAndMod:
					if (_Ptt == PttState.PttOnlyPort)
					{
						_Ptt = PttState.PttPortAndMod;
					}
					break;
			}
		}

		public void Reset(RdAsignState st)
		{
			_Tx = st.Tx;
			_Rx = st.Rx;
			_AudioVia = st.AudioVia;

			Debug.Assert(!_Rx || (_AudioVia != RdRxAudioVia.NoAudio));
			Debug.Assert(!_Tx || Rx);

			if (!_Tx)
			{
				_RtxGroup = Math.Min(_RtxGroup, 0);
				_TempRtxGroup = _RtxGroup;
			}
		}

		public void Reset(RdRtxGroup rtx)
		{
			_RtxGroup = _TempRtxGroup = rtx.RtxGroup;
		}

        public void Reset(string alias)
        {
            _Alias = _TempAlias = alias; 
        }
	}

	public sealed class Radio
	{
		public static int NumDestinations = Settings.Default.NumRdDestinations;

		private RdDst[] _Dst = new RdDst[NumDestinations];
		private int _Page = 0;
		private bool _PttOn = false;
		private int _Rtx = 0;
		private int _NumRtx = 0;
		private bool _SiteManager = false;
		private bool _CanReleaseRtxSqu = false;//lalm 220316
		private bool _CanReleaseRtxSect = true;//lalm 220316
		private ParametrosReplay _ParametrosReplay = new ParametrosReplay(0);
		// valor configurado a true significa que hay doble altavoz de radio para todas las frecuencias
		// a false, el segundo altavoz de radio sólo está disponible para las frecuencias HF
		private bool _DoubleRadioSpeaker = Settings.Default.DoubleRadioSpeaker;

		private static Logger _Logger = LogManager.GetCurrentClassLogger();
		public bool pagina_confirmada = false;

		[EventPublication(EventTopicNames.RadioChanged, PublicationScope.Global)]
		public event EventHandler<RangeMsg> RadioChanged;

		[EventPublication(EventTopicNames.TempReplayChanged, PublicationScope.Global)]
		public event EventHandler<ParametrosReplay> TempReplayChanged;

		[EventPublication(EventTopicNames.RdPageChanged, PublicationScope.Global)]
		public event EventHandler RdPageChanged;

		[EventPublication(EventTopicNames.PttOnChanged, PublicationScope.Global)]
		public event EventHandler PttOnChanged;

		[EventPublication(EventTopicNames.SiteManagerChanged, PublicationScope.Global)]
		public event EventHandler SiteManagerChanged;

		[EventPublication(EventTopicNames.RtxChanged, PublicationScope.Global)]
		public event EventHandler RtxChanged;

		[EventPublication(EventTopicNames.SelCalResponse, PublicationScope.Global)]
		public event EventHandler<StateMsg<string>> SelCalResponse;

		[EventPublication(EventTopicNames.SiteChanged, PublicationScope.Global)]
		public event EventHandler<StateMsg<string>> SiteChanged;


#if _HF_GLOBAL_STATUS_
		[EventPublication(EventTopicNames.HfGlobalStatus, PublicationScope.Global)]
		public event EventHandler<StateMsg<string>> HfGlobalStatus;
#endif

		public RdDst this[int i]
		{
			get { return _Dst[i]; }
		}

		public IEnumerable<RdDst> Destinations
		{
			get { return _Dst; }
		}

		public int Page
		{
			get { return _Page; }
			set
			{
				if (_Page != value)
				{
					SetRtx(0, 0);

					_Page = value;
					General.SafeLaunchEvent(RdPageChanged, this);
				}
			}
		}
		/** 20180425. RSR */
		public int PageSize { get; set; }

		public bool PttOn
		{
			get { return _PttOn; }
			set
			{
				if (_PttOn != value)
				{
					_PttOn = value;

					if (_PttOn)
					{
						SetRtx(0, 0);
					}

					General.SafeLaunchEvent(PttOnChanged, this);
				}
			}
		}

		public int Rtx
		{
			get { return _Rtx; }
		}

		public int NumRtx
		{
			get { return _NumRtx; }
		}
		public bool RadioMonitoring
		{
			get
			{
				for (int i = 0; i < NumDestinations; i++)
				{
					if (_Dst[i].Monitoring)
						return true;
				}

				return false;
			}
		}

		//RQF-14 Duda, aqui se comprueba si hay alguna frecuencia
		public bool FrecuenciaNoDesasignable(int numPositionsByPage)
		{
			int inicio = _Page * numPositionsByPage;
			int fin = inicio + numPositionsByPage;
			for (int i = inicio; i < fin; i++)
			{
				if (_Dst[i].FrecueciaNoDesasignable && _Dst[i].IsConfigurated && !_Dst[i].Unavailable)
					return true;
			}

			return false;
		}

		//RQF-14 Aqui se comprueba una frecuencia en concreto.
		public bool IdFrecuenciaNoDesasignable(int id)
		{
			if (_Dst[id].FrecueciaNoDesasignable)
				return true;
			return false;
		}


		public bool SiteManager
		{
			get { return _SiteManager; }
			set
			{
				_SiteManager = value;

				General.SafeLaunchEvent(SiteManagerChanged, this);
			}
		}
		public bool DoubleRadioSpeaker
		{
			get { return _DoubleRadioSpeaker; }
		}

		public bool CanReleaseRtxSqu
		{
			get
			{
				return _CanReleaseRtxSqu;
			}
			set
			{
				_CanReleaseRtxSqu = value;
			}
		}

		public Radio()
		{
			for (int i = 0; i < NumDestinations; i++)
			{
				_Dst[i] = new RdDst(i);
			}
		}

		public void Reset()
		{
			PttOn = false;
			SetRtx(0, 0);

			for (int i = 0; i < NumDestinations; i++)
			{
				_Dst[i].Reset();
			}

			if (_Page != 0)
			{
				_Page = 0;
				General.SafeLaunchEvent(RdPageChanged, this);
			}

			General.SafeLaunchEvent(RadioChanged, this, new RangeMsg(0, NumDestinations));
		}

		public void Reset(RangeMsg<RdInfo> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				RdDst dst = _Dst[i + msg.From];
				dst.Reset(msg.Info[i]);
			}

			General.SafeLaunchEvent(RadioChanged, this, (RangeMsg)msg);
		}

		public void Reset(RangeMsg<RdDestination> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				RdDst dst = _Dst[i + msg.From];
				dst.Reset(msg.Info[i]);
			}

			General.SafeLaunchEvent(RadioChanged, this, (RangeMsg)msg);
		}

		public void Reset(RangeMsg<RdState> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				_Dst[i + msg.From].Reset(msg.Info[i]);
			}

			General.SafeLaunchEvent(RadioChanged, this, (RangeMsg)msg);
		}

		public void Reset(RangeMsg<PttState> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				_Dst[i + msg.From].Reset(msg.Info[i]);
			}

			General.SafeLaunchEvent(RadioChanged, this, (RangeMsg)msg);
		}

		public void Reset(RangeMsg<SquelchState> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				_Dst[i + msg.From].Reset(msg.Info[i]);
			}

			General.SafeLaunchEvent(RadioChanged, this, (RangeMsg)msg);
		}

		public void Reset(RangeMsg<RdAsignState> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				_Dst[i + msg.From].Reset(msg.Info[i]);
			}

			General.SafeLaunchEvent(RadioChanged, this, (RangeMsg)msg);
		}

		public void Reset(RangeMsg<RdRtxGroup> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				_Dst[i + msg.From].Reset(msg.Info[i]);
			}

			General.SafeLaunchEvent(RadioChanged, this, (RangeMsg)msg);
		}

		public void ResetAssignatedState()
		{
			General.SafeLaunchEvent(RadioChanged, this, new RangeMsg(0, NumDestinations));
		}

		public NotifMsg SwitchTempGroupIfRtxOn(int id, bool someJack)
		{
			RdDst dst = _Dst[id];

			if ((_Rtx > 0) && dst.Tx && !dst.Unavailable && ((dst.RtxGroup == 0) || (dst.RtxGroup == _Rtx)))
			{
				Debug.Assert((dst.TempRtxGroup == 0) || (dst.TempRtxGroup == _Rtx));

				if (dst.TempRtxGroup == 0)
				{
					if (!someJack)
					{
						NotifMsg msg = new NotifMsg(Resources.NoJacksError, Resources.BadOperation, Resources.NoJacksError, 0, MessageType.Error, MessageButtons.Ok);
						return msg;
					}

					if (_NumRtx == 10)
					{
						NotifMsg msg = new NotifMsg(Resources.RtxMaxFrError, Resources.MessageErrorCaption, Resources.RtxMaxFrError, 0, MessageType.Error, MessageButtons.Ok);
						return msg;
					}

					dst.TempRtxGroup = _Rtx;
					_NumRtx++;
				}
				else
				{
					// No está permitido sacar un elemento del grupo de RTX si hay una retransmisión en curso
					//Ver Incidencia #2852
					// RQF36 Posibilidad de deshacer el grupo RETRANS aun existiendo un SQ activo en una de las frecuencias que forman
					// parte del grupo
					if (((dst.Ptt == PttState.ExternPtt) ||
						(dst.Squelch == SquelchState.SquelchOnlyPort) || (dst.Squelch == SquelchState.SquelchPortAndMod))
						&& !_CanReleaseRtxSqu)
					{
						NotifMsg msg = new NotifMsg(Resources.RtxActiveNoRemove, Resources.MessageErrorCaption, Resources.RtxActiveNoRemove, 0, MessageType.Error, MessageButtons.Ok);
						return msg;
					}
					dst.TempRtxGroup = 0;
					_NumRtx--;
				}

				General.SafeLaunchEvent(RadioChanged, this, new RangeMsg(id, 1));
			}

			return null;
		}

		public Dictionary<int, RtxState> SetRtx(int group, int numPositionsByPage)
		{
			Dictionary<int, RtxState> prevRtxGroup = new Dictionary<int, RtxState>();

			if (_Rtx != group)
			{
				if (_Rtx > 0)
				{
					for (int i = 0, to = NumDestinations; i < to; i++)
					{
						RdDst dst = _Dst[i];

						if (dst.TempRtxGroup != dst.RtxGroup)
						{
							prevRtxGroup[i] = (dst.RtxGroup == 0 ? RtxState.Add : RtxState.Delete);
							dst.TempRtxGroup = dst.RtxGroup;
						}
						else if (dst.RtxGroup == _Rtx)
						{
							prevRtxGroup[i] = RtxState.NoChanged;
						}
					}
				}

				_NumRtx = 0;
				_Rtx = group;

				if (group > 0)
				{
					for (int i = _Page * numPositionsByPage, to = (_Page + 1) * numPositionsByPage; i < to; i++)
					//for (int i = 0, to = NumDestinations; i < to; i++)
					{
						RdDst dst = _Dst[i];

						if (dst.RtxGroup == group)
						{
							_NumRtx++;
						}
					}
				}

				General.SafeLaunchEvent(RtxChanged, this);
			}

			return prevRtxGroup;
		}

		public Dictionary<int, RtxState> ResetRtx()
		{
			Dictionary<int, RtxState> prevRtxGroup = new Dictionary<int, RtxState>();

			for (int i = 0, to = NumDestinations; i < to; i++)
			{
				RdDst dst = _Dst[i];

				//Debug.Assert(dst.Tx && !dst.Unavailable);
				//Debug.Assert(((dst.RtxGroup == 0) && (dst.TempRtxGroup == _Rtx)) || ((dst.RtxGroup == _Rtx) && (dst.TempRtxGroup == 0)));
				if (dst.RtxGroup != 0)
				{
					prevRtxGroup[i] = RtxState.Delete;
					dst.TempRtxGroup = 0;
				}
			}

			_NumRtx = 0;
			_Rtx = 0;

			General.SafeLaunchEvent(RtxChanged, this);

			return prevRtxGroup;
		}

		public bool ChangingSite(int i)
		{
			if (i < NumDestinations)
				return _Dst[i].TempAlias != string.Empty && _Dst[i].TempAlias != _Dst[i].Alias;

			return false;
		}

		public string GetTmpAlias(int i)
		{
			if (i < NumDestinations)
				return _Dst[i].TempAlias;

			return string.Empty;
		}

		public void SetSelCalMessage(StateMsg<string> msg)
		{
			General.SafeLaunchEvent(SelCalResponse, this, msg);
		}

		public void SetSiteChanged(StateMsg<string> msg)
		{
			// 0: Alias
			// 1: Frequency
			// 2: resultado
			string[] changeSiteRsp = msg.State.Split(',');

			for (int i = 0; i < Radio.NumDestinations; i++)
			{
				if (ChangingSite(i))
				{
					if (changeSiteRsp[2].ToUpper() == "TRUE")
						_Dst[i].Reset(_Dst[i].TempAlias);
					else
						_Dst[i].TempAlias = _Dst[i].Alias;
				}
				else if (_Dst[i].Frecuency == changeSiteRsp[1])
				{
					if (changeSiteRsp[2].ToUpper() == "TRUE")
						_Dst[i].Reset(changeSiteRsp[0]);
					else
						_Dst[i].TempAlias = _Dst[i].Alias;
				}
			}
			General.SafeLaunchEvent(SiteChanged, this, msg);
		}

#if _HF_GLOBAL_STATUS_
		public void SetHfGlobalStatus(StateMsg<string> status)
		{
			General.SafeLaunchEvent(HfGlobalStatus, this, status);
		}
#endif

		public int GetNumFrAvalilablesForRtx(int from, int count)
		{
			Debug.Assert(from + count <= NumDestinations);

			int numTx = 0;

			for (int i = 0; i < count; i++)
			{
				RdDst dst = _Dst[i + from];

				if (dst.Tx && (dst.RtxGroup != -1))
				{
					numTx++;
				}
			}

			return numTx;
		}

		public bool EnableSelCal()
		{
			for (int i = 0; i < _Dst.Length; i++)
			{
				RdDst dst = _Dst[i];

				if (dst.TipoFrecuencia == TipoFrecuencia_t.HF && dst.Tx)
				{
					return true;
				}
			}

			return false;
		}

		public void ChangingPositionSite(int id, string alias)
		{
			_Dst[id].TempAlias = alias;

			General.SafeLaunchEvent(RadioChanged, this, new RangeMsg(id, 1));
		}


		public void SetTiempoReplay(int segundos)
		{
			_ParametrosReplay.Tiempo = segundos;
			General.SafeLaunchEvent(TempReplayChanged, this, _ParametrosReplay);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using U5ki.Infrastructure;
using U5ki.RdService.Properties;
using U5ki.RdService.NM; //20180323 CONTROL M+N FORBIDDEN 
using NLog;
namespace U5ki.RdService
{
    /// <summary>
    /// 
    /// </summary>
	public enum RdRsType
	{
		Rx,
		Tx,
		RxTx
	}
    /// <summary>
    /// 
    /// </summary>
	public enum RdRsPttType
	{
		NoPtt,
		ExternalPtt,
		OwnedPtt
	}
    /// <summary>
    /// 
    /// </summary>
    public class RdResource : BaseCode, IDisposable
	{
        /************************************************************************/
        /** 201702-FD. AGL. Nuevos Atributos de Configuracion y Estado. *********/
        public class NewRdResourceParams
        {
            // Parametros de configuraci�n
            public string zona { get; set; }
            /** 20170807 */
            public string site { get; set; }

            public int cld_supervision_time { get; set; }
            public bool enable_event_pttsq { get; set; }
            public int offset_frequency { get; set; }
            public string bss_method { get; set; }

            // Parametros de estado
            public int rx_rtp_port { get; set; }
            public int rx_qidx { get; set; }
            public bool rx_selected { get; set; }

            public int tx_rtp_port { get; set; }
            public int tx_cld { get; set; }
            public int tx_owd { get; set; }

            public NewRdResourceParams()
            {
                zona = "##ZONA##";
                site = "##SITE##";

                cld_supervision_time = 1000;
                bss_method = "RSSI";
                rx_selected = false;

                rx_rtp_port = 5062;
                rx_qidx = 8;
                tx_rtp_port = 6062;
                tx_cld = 25;
                tx_owd = 10;
            }
        }
        public NewRdResourceParams new_params = new NewRdResourceParams();
        /************************************************************************/

        public enum BssMethods {
            Ninguno,RSSI,RSSI_NUCLEO
        };

        public Boolean IsForbidden { get; set; }

        /// <summary>
        /// 
        /// </summary>
		public bool Connected
		{
			get { return (_SipCallSt == CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED); }
		}
        /// <summary>
        /// 
        /// </summary>
		public int SipCallId
		{
			get { return _SipCallId; }
		}
        /// <summary>
        /// 
        /// </summary>
		public RdRsType Type
		{
			get { return _Type; }
		}
        /// <summary>
        /// Devuelve true si el recurso es de RX
        /// </summary>
        public bool isRx
        {
            get
            {
                if ((_Type == RdRsType.Rx) || (_Type == RdRsType.RxTx)) return true;
                else return false;
            }
        }
        /// <summary>
        /// Devuelve true si el recurso es de RX
        /// </summary>
        public bool isTx
        {
            get
            {
                if ((_Type == RdRsType.Tx) || (_Type == RdRsType.RxTx)) return true;
                else return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
		public ushort PttId
		{
			get { return _PttId; }
		}
        /// <summary>
        /// 
        /// </summary>
		public RdRsPttType Ptt
		{
			get { return _Ptt; }
		}
        /// <summary>
        /// 
        /// </summary>
		public bool Squelch
		{
			get { return _Squelch; }
		}

		/// <summary>
		/// 
		/// </summary>
		public IntPtr WG67Subscription
		{
			get { return _WG67Subscription; }
		}

        /// 
        /// 
        /// 
        public bool OldSelected
        {
            get { return _OldSelected; }
            set { _OldSelected = value; }
        }

        public CORESIP_PttMuteType PttMute
        {
            get { return TxMute ? CORESIP_PttMuteType.ACTIVADO : CORESIP_PttMuteType.DESACTIVADO; }
        }

        /// <summary>
        ///  Propiedad que indica en los transmisores si false que est� seleccionado para hacer PTT normal
        ///  Si es true, el PTT es mute y no sale al aire.
        ///  En los receptores este atributo no debe usarse (siempre tendr� el valor por defecto false)
        ///  Este par�metro de estado se hereda en M+N
        /// </summary>
        public bool _TxMute = false;
        public bool TxMute
        {
            get { return _TxMute; }
            set
            {
                if (isTx)
                {
                    _TxMute = value;
                    if (!value)
                        LogDebug<RdResource>(_Id + " Tx Selected ");
                }
            }
        }


		/// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uri"></param>
        /// <param name="type"></param>
        /// <param name="frecuency"></param>
        // JOI FREC_DES
		//public RdResource(string id, string uri, RdRsType type, string frecuency)
        //public RdResource(string id, string uri, RdRsType type, string frecuency, string site, RdFrecuency.NewRdFrequencyParams confParams = null)
        public RdResource(string id, string uri, RdRsType type, string frecuency, string site, RdFrecuency.NewRdFrequencyParams confParams = null, RdResource.NewRdResourceParams newRDRP = null) //JOI 201709 NEWRDRP        
		// JOI FREC_DES FIN
		{
			_Id = id;
			_Uri1 = seturi(uri);
			_Type = type;
			_Frecuency = frecuency;
            // JOI FREC_DES
            _Site = site;
            // JOI FREC_DES FIN
            // //JOI 201709 NEWRDRP INI
            if (new_params != null && newRDRP != null)
                SetNewRdResourceParams(newRDRP);
            //JOI 201709 NEWRDRP FIN
			
            if (new_params != null)
                _New_params_ClBss = confParams;

			for (_McastPort = Settings.Default.McastPortBegin; _Ports.ContainsKey(_McastPort); _McastPort++) ;
			_Ports[_McastPort] = this;

			_LastUri = _Uri1;

			Connect();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="uri2"></param>
		/// <param name="uri2"></param>
		/// <param name="type"></param>
		/// <param name="frecuency"></param>
        public RdResource(string id, string uri1, string uri2, RdRsType type, string frecuency, string site, bool selected)
        {
            _Id = id;
            _Uri1 = seturi(uri1);
            _Uri2 = seturi(uri2);
            _Type = type;
            _Frecuency = frecuency;
            _Site = site;
            _Selected = selected;

            for (_McastPort = Settings.Default.McastPortBegin; _Ports.ContainsKey(_McastPort); _McastPort++) ;
            _Ports[_McastPort] = this;

            _LastUri = _Uri1;
            _ToCheck = false;

            Connect();
        }

        //EDU 20170223
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uri2"></param>
        /// <param name="uri2"></param>
        /// <param name="type"></param>
        /// <param name="frecuency"></param>
        public RdResource(string id, string uri1, string uri2, RdRsType type, string frecuency, string site, bool selected, RdFrecuency.NewRdFrequencyParams new_params, CfgRecursoEnlaceExterno rs)
        {
            _Id = id;
            _Uri1 = seturi(uri1);
            _Uri2 = seturi(uri2);
            _Type = type;
            _Frecuency = frecuency;
            _Site = site;
            _Selected = selected;

            _New_params_ClBss = new_params;

            for (_McastPort = Settings.Default.McastPortBegin; _Ports.ContainsKey(_McastPort); _McastPort++) ;
            _Ports[_McastPort] = this;

            _LastUri = _Uri1;
            _ToCheck = false;

            SetNewRdResourceParams(rs);
                            
            Connect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uri1"></param>
        /// <param name="uri2"></param>
        /// <param name="type"></param>
        /// <param name="frecuency"></param>
        public RdResource(string id, string uri1, RdRsType type, string frecuency, bool toCheck)
        {
            _Id = id;
            _Uri1 = seturi(uri1);
            _Type = type;
            _Frecuency = frecuency;

            for (_McastPort = Settings.Default.McastPortBegin; _Ports.ContainsKey(_McastPort); _McastPort++) ;
            _Ports[_McastPort] = this;

            _LastUri = _Uri1;
            _ToCheck = toCheck;

            //if (!toCheck)
            Connect();
        }

        /// <summary>
        /// 
        /// </summary>
        //~RdResource()
        //{
        //    //we're being finalized (i.e. destroyed), call Dispose in case the user forgot to
        //    Dispose(); //<--Warning: subtle bug! Keep reading!
        //}

        #region IDisposable Members
        /// <summary>
        /// 
        /// </summary>
		public void Dispose()
		{
			_Ports.Remove(_McastPort);

			if (_SipCallId >= 0)
			{
				//WG67Subscribe(null, false);

				int call = _SipCallId;

				Reset();
				SipAgent.HangupCall(call);
			}
		}

		#endregion

        /// <summary>
        /// 
        /// </summary>
		public void Connect()
		{
            //20180509 JOI
            //if (MNManager.NodeInPoolForbidden(_Id) == true) //20180323 CONTROL M+N FORBIDDEN
              //  return;
            if ((_Uri1.Contains(_Id) == true) && (MNManager.NodeInPoolForbidden(_Id) == true))
                return;

            if (IsForbidden)
                return;

			if (_SipCallId < 0 && SipAgent.IP != null)
			{
				CORESIP_CallFlags flags = (_Type == RdRsType.Rx ? CORESIP_CallFlags.CORESIP_CALL_RD_RXONLY :
					                      (_Type == RdRsType.Tx ? CORESIP_CallFlags.CORESIP_CALL_RD_TXONLY : 
                                                                  CORESIP_CallFlags.CORESIP_CALL_NO_FLAGS));
				
                //EDU 20170223
                _SipCallId = SipAgent.MakeRdCall(null, _LastUri, _Frecuency, flags, Settings.Default.McastIp, _McastPort,
                    _New_params_ClBss.Priority, new_params.zona, _New_params_ClBss.FrequencyType,
                    _New_params_ClBss.CLDCalculateMethod, _New_params_ClBss.BssWindows, _New_params_ClBss.AudioSync,
                    _New_params_ClBss.AudioInBssWindow, _New_params_ClBss.NotUnassignable, new_params.cld_supervision_time, new_params.bss_method);

                if (_SipCallId >= 0)
                    _SipCallSt = CORESIP_CallState.CORESIP_CALL_STATE_CONNECTING;

                //WG67Subscribe(_Uri, true);
                // LogManager.GetCurrentClassLogger().Debug("MakeRdCall para {0}", _Frecuency);
			}
		}

        /// <summary>
        /// 
        /// </summary>
        public void Check()
        {
            Connect();
        }


        public void HandleChecked()
        {
            Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool HandleKaTimeout(int callId)
		{
			bool connected = Connected;

			Reset();
            /** 20180717. Para los codigos BYE de las sesiones RADIO */
            SipAgent.HangupCall(callId, SipAgent.WG67Reason_KATimeout);

			return connected;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
		public bool HandleRdInfo(CORESIP_RdInfo info)
		{
			bool oldSquelch = _Squelch;
            bool oldSelected = this.new_params.rx_selected;
            int oldQidxValue = this.new_params.rx_qidx;
			RdRsPttType oldPtt = _Ptt;

            //EDU 20170224
            new_params.rx_rtp_port = info.rx_rtp_port;
            new_params.rx_qidx = info.rx_qidx;
            new_params.tx_rtp_port = info.tx_rtp_port;
            new_params.tx_cld = info.tx_cld;
            new_params.tx_owd = info.tx_owd;
            // JCAM. 13/03/2017
            new_params.rx_selected = info.rx_selected;
            
            if (isRx)
			{
				_Squelch = (info.Squelch != 0);
            }
			//if (isTx))
			//{
			// Aunque el recurso sea de Rx se comprueba el PttId para confirmar la portadora
				if (info.PttType == CORESIP_PttType.CORESIP_PTT_OFF)
				{
					_Ptt = RdRsPttType.NoPtt;
				}
				else if (info.PttId != _PttId)
				{
					_Ptt = RdRsPttType.ExternalPtt;
				}
				else
				{
					_Ptt = RdRsPttType.OwnedPtt;
				}
			//}

			return ((oldSquelch != _Squelch) || (oldPtt != _Ptt) || oldSelected != info.rx_selected || oldQidxValue != info.rx_qidx);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateInfo"></param>
        /// <returns></returns>
		public bool HandleChangeInCallState(CORESIP_CallStateInfo stateInfo)
        {
			bool oldConnected = Connected;

			if (stateInfo.State != _SipCallSt)
			{
				if (stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED)
				{
					_SipCallSt = stateInfo.State;
					_PttId = stateInfo.PttId;

					if (isRx)
					{
						RdSrvRxRs rs = new RdSrvRxRs();
						rs.ClkRate = stateInfo.ClkRate;
						rs.ChannelCount = stateInfo.ChannelCount;
						rs.BitsPerSample = stateInfo.BitsPerSample;
						rs.FrameTime = stateInfo.FrameTime;
						rs.McastIp = Settings.Default.McastIp;
						rs.RdRxPort = _McastPort;

						RdRegistry.PublishRxRs(_Id, rs);
					}
					if (isTx)
					{
						RdRegistry.PublishTxRs(_Id, new RdSrvTxRs());
					}
				}
				else if (stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED)
				{
					/* La dualidad de pasarelas se gestiona con una �nica URI virtual que viene configurada en _Uri1
					 * No hay que cambiar a _Uri2 en ning�n caso */
					/*
					if (stateInfo.LastCode == SipAgent.SIP_DECLINE && stateInfo.LastReason.Length > 0)
					{
						string[] strReason = stateInfo.LastReason.Split(new string[] { "; cause=", "; text=" }, StringSplitOptions.RemoveEmptyEntries);
						// Intentando establecer conexi�n con un recurso de una pasarela no activa o no presente
						// En estos casos se ha de intentar por la complementaria
						if (strReason[0] == "WG-67" && strReason[1] == "2008")
							_LastUri = _LastUri == _Uri1 ? _Uri2 : _Uri1;
					}
					else if (stateInfo.LastCode == SipAgent.SIP_REQUEST_TIMEOUT)
						_LastUri = _LastUri == _Uri1 ? _Uri2 : _Uri1;
					*/
                    /* Descomentar cuando se quiera comprobar si se ha establecido
                     * sesi�n SIP al asignar una frecuencia en Tx */
                    //if (_Type==RdRsType.Tx || _Type == RdRsType.RxTx)
                    //{
                    //    RdRegistry.PublishTxRs(_Id, new RdSrvTxRs);
                    //}

					Reset();
				}
			}

			return (oldConnected != Connected);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wg67Info"></param>
		/// <returns></returns>
		public void HandleWG67Info(CORESIP_WG67Info wg67Info)
		{
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="subscribe"></param>
		public void WG67Subscribe(string dst, bool subscribe)
		{
			if (subscribe && (_WG67Subscription == IntPtr.Zero))
			{
				_WG67Subscription = SipAgent.CreateWG67Subscription(dst);
				//if (_WG67Subscription != IntPtr.Zero)
				//{
				//    SipAgent.WG67NotifyEvent += _WG67NotifyAsyncCb;
				//}
			}
			else if (!subscribe && (_WG67Subscription != IntPtr.Zero))
			{
				//SipCore.WG67NotifyEvent -= _WG67NotifyAsyncCb;
				SipAgent.DestroyWG67Subscription(_WG67Subscription);
				_WG67Subscription = IntPtr.Zero;
				//_WG67Info = null;
			}
		}


        public void SetNewRdResourceParams(CfgRecursoEnlaceExterno enlace)
        {
            // Pendiente de completar con los valores recibidos de configuraci�n
            new_params.zona = enlace.NombreZona;
            /** 20170807 */
            new_params.site = enlace.IdEmplazamiento;

            new_params.cld_supervision_time = this._New_params_ClBss.cld_supervision_time;  // Se obtinene a partir del CLD de la frecuencia. enlace.CldSupervision;
            new_params.enable_event_pttsq = enlace.EnableEventPttSq;
            new_params.offset_frequency = enlace.OffSetFrequency;
            new_params.bss_method = ((BssMethods)this._New_params_ClBss.MetodosBssOfrecidos).ToString();
        }
		
        // JOI 201709 NEWRDRP INI
        public void SetNewRdResourceParams(RdResource.NewRdResourceParams newrRDRP)
        {
            new_params.zona = newrRDRP.zona;
            new_params.site = newrRDRP.site;
            new_params.cld_supervision_time = newrRDRP.cld_supervision_time;
            new_params.enable_event_pttsq = newrRDRP.enable_event_pttsq;
            new_params.offset_frequency = newrRDRP.offset_frequency;
            new_params.bss_method = newrRDRP.bss_method;
        }
        // JOI 201709 NEWRDRP FIN
		
		#region Private Members
        /// <summary>
        /// 
        /// </summary>
		private string _Id;
        public string ID
        {
            get { return _Id; }
        }
		/// <summary>
		/// 
		/// </summary>
		private string _Uri1;
        public string Uri1
        {
            get { return _Uri1; }
        }
		/// <summary>
		/// 
		/// </summary>
		/// <summary>
		/// 
		/// </summary>
		private string _Uri2;
        public string Uri2
        {
            get { return _Uri2; }
        }
        /// <summary>
        /// 
        /// </summary>
        bool _ToCheck;
        public bool ToCheck
        {
            get { return _ToCheck; }
        }

        /// <summary>
		/// 
		/// </summary>
		private RdRsType _Type;
        /// <summary>
        /// 
        /// </summary>
		private string _Frecuency;
        public string Frecuency
        {
            get { return _Frecuency; }            
        }
        /// <summary>
        /// 
        /// </summary>
		private int _SipCallId = -1;
        /// <summary>
        /// 
        /// </summary>
		private CORESIP_CallState _SipCallSt = CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED;
        /// <summary>
        /// 
        /// </summary>
		private ushort _PttId = 0;
        /// <summary>
        /// 
        /// </summary>
		private uint _McastPort = 0;
        /// <summary>
        /// 
        /// </summary>
		private RdRsPttType _Ptt = RdRsPttType.NoPtt;
        /// <summary>
        /// 
        /// </summary>
		private bool _Squelch = false;
        /// <summary>
        /// 
        /// </summary>
		private static Dictionary<uint, RdResource> _Ports = new Dictionary<uint, RdResource>();

		/// <summary>
		/// 
		/// </summary>
		private IntPtr _WG67Subscription = IntPtr.Zero;

		/// <summary>
		/// 
		/// </summary>
		string _LastUri;

        /// <summary>
        /// 
        /// </summary>
        bool _OldSelected;

        /// <summary>
        /// Indica si el recurso se ha asignado por una conmutaci�n M+N
        /// </summary>
        private bool _ReplacedMN = false;
        public bool ReplacedMN
        {
            get { return _ReplacedMN; }
            set { _ReplacedMN = value; }
        }
        /// <summary>
        /// Indica si el recurso es Maestro en el grupo M+N // #3603
        /// </summary>
        private bool _isMasterMN = false;
        public bool MasterMN
        {
            get { return _isMasterMN; }
            set { _isMasterMN = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        private string _Site;
        public string Site
        {
            get { return _Site; }
            set { _Site = value; }
        }

        /// 
        /// 
        /// 
        private bool _Selected;
        public bool Selected
        {
            get { return _Selected; }
            set { _Selected = value; }
        }

        private RdFrecuency.NewRdFrequencyParams _New_params_ClBss = new RdFrecuency.NewRdFrequencyParams();    //EDU 20170223

        /// <summary>
        /// 
        /// </summary>
		private void Reset()
		{
			if (_SipCallSt == CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED)
			{
				if (isRx)
				{
					RdRegistry.PublishRxRs(_Id, null);
				}
				if ((_Type == RdRsType.Tx))
				{
					RdRegistry.PublishTxRs(_Id, null);
				}
			}

			_SipCallId = -1;
			_SipCallSt = CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED;
            _PttId = 0;
            _Ptt = RdRsPttType.NoPtt;
            _Squelch = false;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uriin"></param>
        /// <returns></returns>
        private string seturi(string uriin)
        {
#if DEBUG            
            if (U5ki.Infrastructure.Code.Globals.Test.IsRCNDFSimuladoRunning)
            {
                string[] parts = uriin.Split('@');
                return parts[0] + "@" + "192.168.0.71" + ">";
            }
#else
#endif
            return uriin;
        }

		#endregion
	}
}

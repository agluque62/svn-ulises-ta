using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;
using HMI.Model.Module.BusinessEntities;
using HMI.CD40.Module.Properties;
using U5ki.Infrastructure;
using Utilities;
using NLog;

namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// 
    /// </summary>
	class LcPosition
	{
        /// <summary>
        /// 
        /// </summary>
		public event GenericEventHandler StateChanged;

        /// <summary>
        /// 
        /// </summary>
		static LcPosition ActiveTx
		{
			get { return _ActiveTx; }
		}

        /// <summary>
        /// 
        /// </summary>
		static LcPosition ActiveRx
		{
			get { return _ActiveRx; }
		}

        /// <summary>
        /// 
        /// </summary>
		public int Pos
		{
			get { return _Pos; }
		}

        /// <summary>
        /// 
        /// </summary>
		public string Literal
		{
			get { return _Literal;; }
		}

        /// <summary>
        /// 
        /// </summary>
		public LcRxState RxState
		{
			get { return _RxState; }
			private set { SetState(value, _TxState); }
		}

        /// <summary>
        /// 
        /// </summary>
		public LcTxState TxState
		{
			get { return _TxState; }
			private set { SetState(_RxState, value); }
		}

        /// <summary>
        /// 
        /// </summary>
        private LcTxState _OldTxState;
        public LcTxState OldTxState
        {
            get { return _OldTxState; }
        }

        /// <summary>
        /// 
        /// </summary>
        private LcRxState _OldRxState;
        public LcRxState OldRxState
        {
            get { return _OldRxState; }
        }
        /// <summary>
        /// 
        /// </summary>
		public int Group
		{
			get
			{
				//return 0;
                return (_Groups.Count > 0 ? _Groups.IndexOf(_Dependencia)+1 : 0); //17_01_13 (-1)
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
		public LcPosition(int pos)
		{
			_Pos = pos;

			_CallTout.AutoReset = false;
			_CallTout.Elapsed += OnCallTimeout;
			// LALM 210420
			// Incidencia #3684 Encaminamiento -> ENC.03.03. Cuando En un puesto de TWRN que integra dos sectores,
			// Configuro Timer de 7 segundos
			_RxNotifTimer.AutoReset = true;
			_RxNotifTimer.Elapsed += onRxNotifTimer;
			_RxNotifTimer.Interval = 1000;
			_tiempo_memorizada = 7;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Reset()
		{
			MakeHangUpRx();
			MakeHangUpTx();
			SetState(LcRxState.Unavailable, LcTxState.Unavailable);

			_Literal = "";
			_Channels.Clear();
            ClearGroups();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cfg"></param>
		public void Reset(CfgEnlaceInterno cfg)
		{
			_Literal = cfg.Literal;
            _LastChannel = 0;
			_Channels.Clear();

			foreach (CfgRecursoEnlaceInterno dst in cfg.ListaRecursos)
			{
				switch (dst.Prefijo)
				{
					case Cd40Cfg.INT_DST:
						string hostId = Top.Cfg.GetUserHost(dst.NombreRecurso);

						if (_Channels.Find(delegate(SipChannel channel)
							{ return ((channel.Prefix == dst.Prefijo) && (channel.Id == hostId)); }) == null)
						{
							SipChannel ch = new IntChannel(cfg.OrigenR2, hostId, dst.NombreRecurso, dst.Prefijo);
							ch.RsChanged += OnRsChanged;

							_Channels.Insert(0, ch);
						}
						break;
                        
					case Cd40Cfg.PP_DST:
						if (_Channels.Find(delegate(SipChannel channel)
							{ return ((channel.Prefix == dst.Prefijo) && (channel.Id == dst.NombreRecurso)); }) == null)
						{
							SipChannel ch = dst.Interface == TipoInterface.TI_LCEN ?
								(SipChannel)new LcChannel(cfg.OrigenR2, dst.NombreRecurso, dst.Prefijo) :
								(SipChannel)new TlfPPChannel(cfg.OrigenR2, dst.NumeroAbonado, dst.NombreRecurso, dst.Prefijo, dst.Interface);
							ch.RsChanged += OnRsChanged;

							_Channels.Add(ch);
						}

						if (cfg.Dependencia != string.Empty)
						{
							_Dependencia = cfg.Dependencia;
							_Groups.Add(_Dependencia);
						}
						break;

					case Cd40Cfg.ATS_DST:
						string[] userId = Top.Cfg.GetUserFromAddress(dst.Prefijo, dst.NumeroAbonado);
						if (userId != null)
						{
							dst.NombreRecurso = userId[1];
							dst.Prefijo = Cd40Cfg.INT_DST;
							goto case Cd40Cfg.INT_DST;
						}

						TlfNet net = Top.Cfg.GetIPNet(dst.Prefijo, dst.NumeroAbonado);

						if (net != null)
						{
							SipChannel ch = _Channels.Find(delegate(SipChannel channel) { return ((channel.Prefix == dst.Prefijo) && (channel.Id == net.Id)); });

                            if (ch == null)
                            {
                                // Para Linea caliente en destinos ATS_DST, 
                                // solo se utilizan las lineas que vienen de un encaminamiento IP
                                List<SipLine> listLines =  net.Lines.FindAll(line => line.centralIP == false);
                                foreach (SipLine line in listLines)
                                {
                                    int index = net.Lines.IndexOf(line);
                                    net.Lines.RemoveAt(index);
                                    net.Routes.RemoveAt(index);
                                    net.RsTypes.RemoveAt(index);
                                }
                                //Caso de destino ATS de central no IP: no tiene canal ATS (no lo añado), solo canal LC
                                if (net.Lines.Count > 0)
                                {
                                ch = new TlfNetChannel(net, cfg.OrigenR2, dst.NumeroAbonado, null, dst.Prefijo);
                                ch.RsChanged += OnRsChanged;

                                _Channels.Add(ch);
                            }
                            }
                            else
                            {
                                ch.AddRemoteDestination(dst.NumeroAbonado, null);
                            }
						}

						break;
				}
			}

			if ((_SipCallRx != null) && !_SipCallRx.IsValid(_Channels))
			{
				MakeHangUpRx();
			}
			if ((_SipCallTx != null) && !_SipCallTx.IsValid(_Channels))
			{
				MakeHangUpTx();
			}

			LcRxState rxSt = _RxState;
			LcTxState txSt = _TxState;

			if ((_SipCallRx == null) || (_SipCallTx == null))
			{
				GetState(out rxSt, out txSt);
			}

			if (_SipCallRx != null)
			{
				rxSt = _RxState;
			}
			if (_SipCallTx != null)
			{
				txSt = _TxState;
			}

			SetState(rxSt, txSt);
		}

        /// <summary>
        /// 
        /// </summary>
		public void Call()
		{
			if ((_ActiveTx == null) && (_TxState == LcTxState.Idle))
			{
				Debug.Assert(_SipCallTx == null);
				_SipCallTx = SipCallInfo.NewLcCall(_Channels);

				TxState = TryCall();
				_ActiveTx = this;

				if (_TxState == LcTxState.Out)
				{
					_CallTout.Enabled = true;

					// Top.Recorder.Rec(CORESIP_CallType.CORESIP_CALL_IA, true);
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public void HangUpRx()
		{
			if (_SipCallRx != null)
			{
				MakeHangUpRx();
				RxState = LcRxState.Idle;
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public void HangUpTx()
		{
			if (_SipCallTx != null)
			{
				MakeHangUpTx();
				TxState = LcTxState.Idle;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sipCallId"></param>
        /// <param name="info"></param>
        /// <param name="inInfo"></param>
        /// <returns></returns>
		public int HandleIncomingCall(int sipCallId, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo)
		{
			SipCallInfo inCall = SipCallInfo.NewIncommingCall(_Channels, sipCallId, info, inInfo, false);
			if (inCall != null)
			{
				if ((Top.ScreenSaverEnabled) || Top.Hw.LCSpeaker == false)
                {
                    return SipAgent.SIP_DECLINE;
                }
                // Si la telefonía va por altavoz y hay llamada de LC se quita 
                // el estado "en espera de cuelgue"
                if (Top.Mixer.RxTlfAudioVia == TlfRxAudioVia.Speaker)
                  Top.Tlf.SetHangToneOff();
				MakeHangUpRx();
				RxState = LcRxState.Idle;

				if ((_ActiveRx != null) || ((_ActiveTx != null) && (_ActiveTx != this)))
				{
					// Notificamos el Mem y lo borramos
                    _OldRxState = _RxState;
                    RxState = LcRxState.Mem;
					_RxState = LcRxState.Idle;

					_RxNotifTimer.Enabled = true;
					_RxNotifTimer.Interval = 1000;
					_tiempo_memorizada = 7;
					return SipAgent.SIP_BUSY;
				}
				else
				{
					_SipCallRx = inCall;
					_ActiveRx = this;

					// LALM 210420
					// Incidencia #3684 Encaminamiento -> ENC.03.03. Cuando En un puesto de TWRN que integra dos sectores,
					// Cuando el destino de la llamada no corresponde al sector principal en una agrupacion
					if (inInfo.DstId != _Channels[0].AccId)
					{
						_lastaccid = inInfo.DstId;
						_RxNotifTimer.Enabled = true;
						_RxNotifTimer.Interval = 1000;
						_tiempo_memorizada = 7;
					}
					else
                    {//LALM 210621 Quito la memorizada si el destino correspode al sector principal.
						_lastaccid = "";
						_RxNotifTimer.Enabled = false;
						_RxNotifTimer.Interval = 1000;
						_tiempo_memorizada = 0;
					}
					//LALM 210610 
					// Incidencia #3684 Encaminamiento -> ENC.03.03. Cuando En un puesto de TWRN que integra dos sectores,
					// Cuando la llamada viene por linea lineas analógicas.
					// Tomo esta opción para líneas analógicas
					if (inInfo.SrcId[0] != 0)
                    {
						_lastsrcid = inInfo.SrcId;
						_RxNotifTimer.Enabled = true;
						_RxNotifTimer.Interval = 1000;
						_tiempo_memorizada = 7;
					}
					_Channels.Sort(delegate(SipChannel a, SipChannel b)
						{
							return b.First.CompareTo(a.First);
						}
					);

					return SipAgent.SIP_OK;
				}
			}

			return SipAgent.SIP_DECLINE;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sipCallId"></param>
        /// <param name="stateInfo"></param>
        /// <returns></returns>
        public bool HandleChangeInCallState(int sipCallId, CORESIP_CallStateInfo stateInfo)
		{
			if ((_SipCallRx != null) && (_SipCallRx.Id == sipCallId))
			{
				Debug.Assert(_ActiveRx == this);

				if (stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED)
				{
					//Debug.Assert(stateInfo.MediaStatus == CORESIP_MediaStatus.CORESIP_MEDIA_REMOTE_HOLD);
					//Debug.Assert(stateInfo.MediaDir == CORESIP_MediaDir.CORESIP_DIR_RECVONLY);

					Top.Mixer.Unlink(sipCallId);
					Top.Mixer.Link(sipCallId, MixerDev.SpkLc, MixerDir.Send, Mixer.LC_PRIORITY, FuentesGlp.RxLc);
                    Top.Recorder.SessionGlp(sipCallId, FuentesGlp.RxLc, true);
					RxState = LcRxState.Rx;
                    Top.Lc.HoldedTlf = true;
				}
				else if (stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED)
				{
					if (_RxState == LcRxState.Rx)
					{
						Top.Mixer.Unlink(sipCallId);
					}

					_SipCallRx = null;
					_ActiveRx = null;

                    _OldRxState = _RxState;
                    RxState = LcRxState.Idle;
                    Top.Lc.HoldedTlf = false;
				}

				return true;
			}

			if ((_SipCallTx != null) && (_SipCallTx.Id == sipCallId))
			{
				Debug.Assert(_ActiveTx == this);

				if (stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED)
				{
					Debug.Assert(stateInfo.MediaStatus == CORESIP_MediaStatus.CORESIP_MEDIA_ACTIVE);
//					Debug.Assert(stateInfo.MediaDir == CORESIP_MediaDir.CORESIP_DIR_SENDONLY);

					_CallTout.Enabled = false;
					Top.Mixer.Unlink(sipCallId);
					Top.Mixer.Link(sipCallId, MixerDev.MhpLc, MixerDir.Recv, Mixer.LC_PRIORITY, FuentesGlp.RxLc);

					TxState = LcTxState.Tx;
                    Top.Lc.HoldedTlf = true;
				}
				else if (stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED)
				{
					_SipCallTx.Id = -1;

					if (_TxState == LcTxState.Out)
					{
						_SipCallTx.LastCallResult = stateInfo.LastCode;

						if (stateInfo.LastCode == SipAgent.SIP_BUSY)
						{
							_CallTout.Enabled = false;
							TxState = LcTxState.Busy;
						}
                        else if (_CallTout.Enabled)
                        {
                            TxState = TryCall();
                        }
                        else
                        {
                            TxState = LcTxState.Congestion;
                        }
					}
					else
					{
						if (_TxState == LcTxState.Tx)
						{
							Top.Mixer.Unlink(sipCallId);
						}

						_SipCallTx = null;
						_ActiveTx = null;

						TxState = LcTxState.Idle;
                        Top.Lc.HoldedTlf = false;
					}
				}

				return true;
			}

			return false;
		}

        /// <summary>
        /// 
        /// </summary>
		public static void ClearGroups()
		{
			_Groups.Clear();
		}

		#region Private Members
        /// <summary>
        /// 
        /// </summary>
		private static Logger _Logger = LogManager.GetCurrentClassLogger();
		private static LcPosition _ActiveTx = null;
		private static LcPosition _ActiveRx = null;
		private static List<string> _Groups = new List<string>();
        /// <summary>
        /// Guarda el ultimo canal por el que se intentó la ultima llamada saliente
        /// </summary>
        private static int _LastChannel = 0;
		// LALM 210420
		// Incidencia #3684 Encaminamiento -> ENC.03.03. Cuando En un puesto de TWRN que integra dos sectores,
		private string _lastaccid = "";
		private string _lastsrcid = "";
        /// <summary>
        /// 
        /// </summary>
		private int _Pos;
		private string _Literal = "";
		private LcRxState _RxState = LcRxState.Unavailable;
		private LcTxState _TxState = LcTxState.Unavailable;
		private List<SipChannel> _Channels = new List<SipChannel>();
		private SipCallInfo _SipCallTx = null;
		private SipCallInfo _SipCallRx = null;
		private Timer _CallTout = new Timer(Settings.Default.LcCallsTout);
		// LALM 210419 nuevo timer control de lastaccid Peticiones #3684
		// Incidencia #3684 Encaminamiento -> ENC.03.03. Cuando En un puesto de TWRN que integra dos sectores,
		private Timer _RxNotifTimer = new UiTimer(1000);
		private int _tiempo_memorizada = 7;

		private int _Tone = -1;
		private string _Dependencia;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="tx"></param>
		private void GetState(out LcRxState rx, out LcTxState tx)
		{
			rx = LcRxState.Unavailable;
			tx = LcTxState.Unavailable;


			foreach (SipChannel ch in _Channels)
			{
                if (ch.DestinationReachableState() == SipChannel.DestinationState.Idle)
				{
						rx = LcRxState.Idle;
						tx = LcTxState.Idle;

						return;
					}
            }                 
				}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="path"></param>
        /// <returns></returns>
		protected bool TryCall(SipChannel ch, SipPath path)
		{
			string dstParams = "";
			string remoteId = path.Remote.Ids[0];

            if (!path.Line.centralIP)
            {
                //Estos parametros son internos, sirven para dar información a la pasarela
                //En encaminamiento IP no se deben usar
			if ((ch.Prefix != Cd40Cfg.INT_DST) &&
				((ch.Prefix != Cd40Cfg.PP_DST) || (string.Compare(remoteId, path.Line.Id) != 0)))
			{
				dstParams += string.Format(";cd40rs={0}", path.Line.Id);
			}
            }

			string dstUri = string.Format("<sip:{0}@{1}{2}>", remoteId, path.Line.Ip, dstParams);
			// LALM 210610 #3684
			// Incidencia #3684 Encaminamiento -> ENC.03.03. Cuando En un puesto de TWRN que integra dos sectores,
			// se señaliza ocupado cuando se intenta responder por AI a una llamada iniciada desde otro SCV,
			// al rol de mayor número ATS de los sectores agrupados
			// Tomo esta opción para líneas analógicas
			if (_lastsrcid!="")
            {
				dstUri = string.Format("<sip:{0}@{1}{2}>", _lastsrcid, path.Line.Ip, dstParams);
			}
			try
			{
                CORESIP_CallFlags flags = CORESIP_CallFlags.CORESIP_CALL_NO_FLAGS;
                if (path.ModoSinProxy == false)
                    flags |= CORESIP_CallFlags.CORESIP_CALL_EXTERNAL_IP;

				// LALM 210420
				// Incidencia #3684 Encaminamiento -> ENC.03.03. Cuando En un puesto de TWRN que integra dos sectores,
				// No puedo considerar el estado Idle ya que memorizada es un subestado de Idle.
				// Convendría saber cuando esta memorizada, por eso se pone el timer de 7 segundos.
				if (_RxState == LcRxState.Unavailable)
					/*|| _RxState == LcRxState.Idle*/
					_lastaccid = "";
				if (_lastaccid != "")
				{
					int sipCallId = SipAgent.MakeLcCall(_lastaccid, dstUri, flags);
					_SipCallTx.Update(sipCallId, _lastaccid, remoteId, ch, path.Remote, path.Line);
					//LALM 210609
					// Incidencia #3684 Encaminamiento -> ENC.03.03. Cuando En un puesto de TWRN que integra dos sectores,
					// No quito inmediatamente lastaccid, espero a que venza el teporizador
					//if (_RxState!=LcRxState.Rx)
					//	_lastaccid = "";
				}
				else
                {
					int sipCallId = SipAgent.MakeLcCall(ch.AccId, dstUri, flags);
					_SipCallTx.Update(sipCallId, ch.AccId, remoteId, ch, path.Remote, path.Line);

				}
				return true;
			}
			catch (Exception ex)
			{
				ch.SetCallResult(path.Remote, path.Line, _SipCallTx.Priority, -1);
				_Logger.Warn("ERROR llamando a " + dstUri, ex);
			}

			return false;
		}

        /// <summary>
        /// Intento de llamada saliente por alguno de los canales  disponibles.
        /// Cuando hay configurados varios canales se intenta primero por el ATS IP y
        /// luego en turno rotatorio en cada llamada por los LCEN.
        /// Si una llamada no progresa se intenta por el siguiente canal en la misma llamada
        /// </summary>
        /// <returns></returns>
		private LcTxState TryCall()
		{
            SipChannel channel = null;
            SipPath path = null;
			Debug.Assert((_SipCallTx != null) && !_SipCallTx.IsActive);

            // Busco el path sobre el canal IP si lo hay, para hacer el primer intento
            foreach (SipChannel ch in _Channels)
            {
                if (ch is TlfNetChannel)
                {
                    path = ch.GetPreferentPath(_SipCallTx.Priority);
                    if ((path != null) && TryCall(ch, path))
                    {
                        return LcTxState.Out;
                    }
                    else break;
                }
            }
            for (int i = 0; i < _Channels.Count; i++ )
            {
                //Rotación de canales
                _LastChannel = ++_LastChannel % _Channels.Count;
                channel = _Channels[_LastChannel];
                path = channel.GetPreferentPath(_SipCallTx.Priority);

                while (path != null)
                {
                    if (TryCall(channel, path))
                    {
                        return LcTxState.Out;
                    }
                    path = channel.GetPreferentPath(_SipCallTx.Priority);
                }
            }

			foreach (SipChannel ch in _Channels)
			{
				path = ch.GetDetourPath(_SipCallTx.Priority);

				while (path != null)
				{
					if (TryCall(ch, path))
					{
						return LcTxState.Out;
					}

					path = ch.GetDetourPath(_SipCallTx.Priority);
				}
			}

			_CallTout.Enabled = false;
			_RxNotifTimer.Enabled = false;
			return LcTxState.Congestion;
		}

        /// <summary>
        /// 
        /// </summary>
		private void MakeHangUpRx()
		{
			if (_SipCallRx != null)
			{
				Debug.Assert(_SipCallRx.IsActive);
				Debug.Assert(_ActiveRx == this);

				if (_RxState == LcRxState.Rx)
				{
					Top.Mixer.Unlink(_SipCallRx.Id);
				}

				SipAgent.HangupCall(_SipCallRx.Id);

				_ActiveRx = null;
				_SipCallRx = null;
                Top.Lc.HoldedTlf = false;
            }
		}

        /// <summary>
        /// 
        /// </summary>
		private void MakeHangUpTx()
		{
			if (_SipCallTx != null)
			{
				if (_SipCallTx.IsActive)
				{
					Debug.Assert(_ActiveTx == this);

					switch (_TxState)
					{
						case LcTxState.Out:
							_CallTout.Enabled = false;
							break;
						case LcTxState.Tx:
							Top.Mixer.Unlink(_SipCallTx.Id);
							break;
					}

					SipAgent.HangupCall(_SipCallTx.Id);
				}
				else
				{
					Debug.Assert((_TxState == LcTxState.Congestion) || (_TxState == LcTxState.Busy));
				}

				// Top.Recorder.Rec(CORESIP_CallType.CORESIP_CALL_IA, false);

				_SipCallTx = null;
				_ActiveTx = null;
                Top.Lc.HoldedTlf = false;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rxSt"></param>
        /// <param name="txSt"></param>
		private void SetState(LcRxState rxSt, LcTxState txSt)
		{
			bool changed = false;

            _OldRxState = _RxState;
            _OldTxState = _TxState;
            if (_RxState != rxSt)
			{
                _RxState = rxSt;
				changed = true;

				if (_TxState == txSt)
				{
					if ((rxSt != LcRxState.Unavailable) && (_TxState == LcTxState.Unavailable))
					{
						txSt = LcTxState.Idle;
					}
					else if ((rxSt == LcRxState.Unavailable) && (_TxState == LcTxState.Idle))
					{
						txSt = LcTxState.Unavailable;
					}
				}
			}

			if (_TxState != txSt)
			{
				if (_Tone >= 0)
				{
					Top.Mixer.Unlink(_Tone);
					SipAgent.DestroyWavPlayer(_Tone);
					_Tone = -1;
				}

				switch (txSt)
				{
					case LcTxState.Congestion:
						_Tone = SipAgent.CreateWavPlayer("Resources/Tones/Congestion.wav", true);
						Top.Mixer.Link(_Tone, MixerDev.SpkLc, MixerDir.Send, Mixer.LC_PRIORITY, FuentesGlp.RxLc);
						break;
					case LcTxState.Busy:
						_Tone = SipAgent.CreateWavPlayer("Resources/Tones/Busy.wav", true);
						Top.Mixer.Link(_Tone, MixerDev.SpkLc, MixerDir.Send, Mixer.LC_PRIORITY, FuentesGlp.RxLc);
						break;
				}

				_TxState = txSt;
				changed = true;
			}

			if (changed)
			{
				General.SafeLaunchEvent(StateChanged, this);
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void OnCallTimeout(object sender, ElapsedEventArgs e)
		{
			Top.WorkingThread.Enqueue("OnLcCallTimeout", delegate()
			{
				if (_TxState == LcTxState.Out)
				{
					Debug.Assert((_SipCallTx != null) && _SipCallTx.IsActive);
					Debug.Assert(_ActiveTx == this);

					SipAgent.HangupCall(_SipCallTx.Id);

					_SipCallTx.Id = -1;
					TxState = LcTxState.Congestion;
				}
			});
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
		private void OnRsChanged(object sender)
		{
			Resource rs = (Resource)sender;
            if (rs.Content is GwTlfRs)
            {
                //Este tipo de recurso son los creados por la 19+1 de telefonía, 
                //no vienen del Tifx y no deben afectar a las LC
                if (((GwTlfRs)rs.Content).Type == (uint)RsChangeInfo.RsTypeInfo.NoType)
                    return;
            }

			if (!rs.IsValid)
			{
				LcRxState rxSt;
				LcTxState txSt;

				GetState(out rxSt, out txSt);

				if (rxSt == LcRxState.Unavailable)
				{
					Debug.Assert(txSt == LcTxState.Unavailable);

					MakeHangUpRx();
					MakeHangUpTx();
				}
				else
				{
					if (_SipCallRx != null)
					{
						if (_SipCallRx.IsActive && (_SipCallRx.Line.RsLine == rs))
						{
							MakeHangUpRx();
						}
						else
						{
							rxSt = _RxState;
						}
					}

					if (_SipCallTx != null)
					{
						if (_TxState == LcTxState.Out)
						{
							if (_SipCallTx.Line.RsLine == rs)
							{
								_SipCallTx.Ch.ResetCallResults(rs);
							}
							else
							{
								foreach (SipChannel ch in _Channels)
								{
									if (ch.ResetCallResults(rs))
									{
										break;
									}
								}
							}
						}

						if (_SipCallTx.IsActive && (_SipCallTx.Line.RsLine == rs))
						{
							if (_TxState == LcTxState.Out)
							{
								SipAgent.HangupCall(_SipCallTx.Id);
								_SipCallTx.Id = -1;

								txSt = (_CallTout.Enabled ? TryCall() : LcTxState.Congestion);
							}
							else
							{
								MakeHangUpTx();
							}
						}
						else
						{
							txSt = _TxState;
						}
					}
				}

				SetState(rxSt, txSt);
			}
			else
			{
                //Esto es para cambiar la IP del proxy vivo (es de tipo GwTflRs y no de GwLcRs)
                if (rs.Content is GwTlfRs)
                    ResetIpLinesOfChannels(rs.Id, ((GwTlfRs)rs.Content).GwIp);
                else if (rs.Content is GwLcRs)
                    ResetIpLinesOfChannels(rs.Id, ((GwLcRs)rs.Content).GwIp);

				if (_SipCallTx != null)
				{
					if ((_TxState == LcTxState.Out) && (_SipCallTx.Line.RsLine != rs))
					{
						foreach (SipChannel ch in _Channels)
						{
							if (ch.ResetCallResults(rs))
							{
								break;
							}
						}
					}
				}
				else
				{
					if (_RxState == LcRxState.Unavailable)
					{
						Debug.Assert(_TxState == LcTxState.Unavailable);
                        //Pone a idle la tecla sin tener en cuenta todos sus canales y recursos
                        //No funciona LC con recurso IP sólo
                        //SetState(LcRxState.Idle, LcTxState.Idle);
			            LcRxState rxSt = _RxState;
			            LcTxState txSt = _TxState;

				        GetState(out rxSt, out txSt);
                        SetState(rxSt, txSt);                        
					}
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="gwIp"></param>
		protected void ResetIpLinesOfChannels(string id, string gwIp)
		{
			foreach (SipChannel ch in _Channels)
			{
				ch.ResetLine(id, gwIp);
			}
		}


		// LALM 210420 comprobacion cada 7 segundos si la llamada esta pendiente.
		// Incidencia #3684 Encaminamiento -> ENC.03.03. Cuando En un puesto de TWRN que integra dos sectores,
		// se señaliza ocupado cuando se intenta responder por AI a una llamada iniciada desde otro SCV,
		// al rol de mayor número ATS de los sectores agrupados
		private void onRxNotifTimer(object sender, ElapsedEventArgs e)
		{
			if (_tiempo_memorizada>0)
				_tiempo_memorizada--;
			else
            {
				if (RxState == LcRxState.Idle)
				{
					if (_lastaccid != "")
						_lastaccid = "";
					if (_lastsrcid != "")
						_lastsrcid = "";
				}
				_tiempo_memorizada = 7;
			}
		}
		#endregion
	}
}
